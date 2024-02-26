using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Linear algebra provider
    /// </summary>
    public class LinearAlgebraProvider : IDisposable
    {
        /// <summary>
        /// A scope of disposable objects
        /// </summary>
        protected readonly ConcurrentStack<ConcurrentDictionary<IDisposable, bool>> Scope = new();
        bool _isPoppingScope = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Bright data context</param>
        public LinearAlgebraProvider(BrightDataContext context)
        {
            Context = context;
            PushScope();
        }

        /// <inheritdoc />
        ~LinearAlgebraProvider()
        {
            InternalDispose();
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            InternalDispose();
        }

        void InternalDispose()
        {
            _isPoppingScope = true;
            foreach (var set in Scope) {
                foreach (var item in set.Keys)
                    item.Dispose();
            }
            _isPoppingScope = false;
            Scope.Clear();
        }

        /// <summary>
        /// Bright data context
        /// </summary>
        public BrightDataContext Context { get; }

        /// <summary>
        /// Provider name
        /// </summary>
        public virtual string ProviderName => Consts.DefaultLinearAlgebraProviderName;

        /// <summary>
        /// Type of vectors that will be created
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public virtual Type VectorType { get; } = typeof(BrightVector);

        /// <summary>
        /// Type of matrices that will be created
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public virtual Type MatrixType { get; } = typeof(BrightMatrix);

        /// <summary>
        /// Type of 3D tensors that will be created
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public virtual Type Tensor3DType { get; } = typeof(BrightTensor3D);

        /// <summary>
        /// Type of 4D tensors that will be created
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public virtual Type Tensor4DType { get; } = typeof(BrightTensor4D);

        /// <summary>
        /// Adds a new scope
        /// </summary>
        public void PushScope() => Scope.Push(new());

        /// <summary>
        /// Pops that last scope and disposes all objects within that scope
        /// </summary>
        public virtual void PopScope()
        {
            if (Scope.TryPop(out var set)) {
                foreach (var item in set.Keys)
                    item.Dispose();
            }
        }
        internal bool AddToScope(IDisposable obj) => Scope.First().TryAdd(obj, true);
        internal bool RemoveFromScope(IDisposable obj) => _isPoppingScope || (Scope.FirstOrDefault()?.TryRemove(new KeyValuePair<IDisposable, bool>(obj, true)) ?? false);

        /// <summary>
        /// Creates a tensor segment from an array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> CreateSegment(params float[] data) => new MutableTensorSegment<float>(data);

        /// <summary>
        /// Creates a tensor segment
        /// </summary>
        /// <param name="size">Segment size</param>
        /// <param name="initialiseToZero">True to initialize the all values in the segment to zero</param>
        /// <returns></returns>
        public virtual INumericSegment<float> CreateSegment(uint size, bool initialiseToZero) => new ArrayPoolTensorSegment<float>(MemoryOwner<float>.Allocate((int)size, initialiseToZero ? AllocationMode.Clear : AllocationMode.Default));

        /// <summary>
        /// Creates a tensor segment
        /// </summary>
        /// <param name="size">Segment size</param>
        /// <param name="initializer">Function to initialize each value in the segment</param>
        /// <returns></returns>
        public virtual INumericSegment<float> CreateSegment(uint size, Func<uint /* index */, float> initializer)
        {
            var ret = MemoryOwner<float>.Allocate((int)size, AllocationMode.Clear);
            var ptr = ret.Span;
            for (var i = 0; i < ptr.Length; i++)
                ptr[i] = initializer((uint)i);
            return new ArrayPoolTensorSegment<float>(ret);
        }

        /// <summary>
        /// Creates a clone of the tensor segment
        /// </summary>
        /// <param name="segment">Segment to clone</param>
        /// <returns></returns>
        public virtual INumericSegment<float> Clone(IReadOnlyNumericSegment<float> segment)
        {
            var ret = CreateSegment(segment.Size, false);
            segment.CopyTo(ret);
            return ret;
        }

        /// <summary>
        /// Creates a vector from a tensor segment
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual IVector CreateVector(INumericSegment<float> data) => new BrightVector(data, this);

        /// <summary>
        /// Creates a vector from a read only tensor segment
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual IVector CreateVector(IReadOnlyNumericSegment<float> data) => new BrightVector(Clone(data), this);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="size">Size of the vector</param>
        /// <param name="initialiseToZero">True to initialize each value to zero</param>
        /// <returns></returns>
        public IVector CreateVector(uint size, bool initialiseToZero) => CreateVector(CreateSegment(size, initialiseToZero));

        /// <summary>
        /// Creates a vector from an array of floats
        /// </summary>
        /// <param name="data">Float array</param>
        /// <returns></returns>
        public IVector CreateVector(params float[] data) => CreateVector(data.AsSpan());

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="size">Size of the vector</param>
        /// <param name="value">Initial value of each item in the vector</param>
        /// <returns></returns>
        public IVector CreateVector(uint size, float value) => CreateVector(size, _ => value);

        /// <summary>
        /// Creates a vector from a span of floats
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public IVector CreateVector(ReadOnlySpan<float> span)
        {
            var segment = CreateSegment((uint)span.Length, false);
            segment.CopyFrom(span);
            return CreateVector(segment);
        }

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="size">Size of the vector</param>
        /// <param name="initializer">Function to initialize each value in the vector</param>
        /// <returns></returns>
        public IVector CreateVector(uint size, Func<uint /* index */, float> initializer) => CreateVector(CreateSegment(size, initializer));

        /// <summary>
        /// Creates a new vector from an existing vector
        /// </summary>
        /// <param name="vector">Vector to clone</param>
        /// <returns></returns>
        public IVector CreateVector(IVector vector) => CreateVector(Clone(vector.Segment));

        /// <summary>
        /// Creates a vector from a read only vector
        /// </summary>
        /// <param name="vector">Read only vector</param>
        /// <returns></returns>
        public IVector CreateVector(IReadOnlyVector vector) => CreateVector(Clone(vector.ReadOnlySegment));

        /// <summary>
        /// Creates a vector from an enumerable of floats
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public IVector CreateVector(IEnumerable<float> values) => CreateVector(values.ToArray().AsSpan());

        /// <summary>
        /// Creates a matrix from a segment
        /// </summary>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="columnCount">Number of columns</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrix(uint rowCount, uint columnCount, INumericSegment<float> data) => new BrightMatrix(data, rowCount, columnCount, this);

        /// <summary>
        /// Creates a matrix from a segment
        /// </summary>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="columnCount">Number of columns</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrix(uint rowCount, uint columnCount, IReadOnlyNumericSegment<float> data) => new BrightMatrix(Clone(data), rowCount, columnCount, this);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="columnCount">Number of columns</param>
        /// <param name="initialiseToZero">True to initialize each value to zero</param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrix(uint rowCount, uint columnCount, bool initialiseToZero) => CreateMatrix(rowCount, columnCount, CreateSegment(rowCount * columnCount, initialiseToZero));

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="columnCount">Number of columns</param>
        /// <param name="initializer">Function to initialize each value in the matrix that will receive (row index, column index)</param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrix(uint rowCount, uint columnCount, Func<uint /* row index */, uint /* column index */, float> initializer)
        {
            var segment = CreateSegment(rowCount * columnCount, false);
            var array = segment.GetUnderlyingArray().Array!;
            for (uint i = 0, len = segment.Size; i < len; i++)
                array[i] = initializer(i % rowCount, i / rowCount);
            return CreateMatrix(rowCount, columnCount, segment);
        }

        /// <summary>
        /// Creates a new matrix from an existing matrix
        /// </summary>
        /// <param name="matrix">Matrix to clone</param>
        /// <returns></returns>
        public IMatrix CreateMatrix(IMatrix matrix) => CreateMatrix(matrix.RowCount, matrix.ColumnCount, Clone(matrix.Segment));

        /// <summary>
        /// Creates a matrix from a read only matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public IMatrix CreateMatrix(IReadOnlyMatrix matrix) => CreateMatrix(matrix.RowCount, matrix.ColumnCount, matrix.ReadOnlySegment);

        /// <summary>
        /// Creates a matrix from the rows supplied as vectors
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(IVector[] rows) => CreateMatrixFromRows(rows.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the columns supplied as vectors
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(IVector[] columns) => CreateMatrixFromColumns(columns.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the rows supplied as read only vectors
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(IReadOnlyVector[] rows) => CreateMatrixFromRows(rows.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the columns supplied as read only vectors
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(IReadOnlyVector[] columns) => CreateMatrixFromColumns(columns.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the rows supplied as read only vectors
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(ReadOnlyVector[] rows) => CreateMatrixFromRows(rows.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the columns supplied as read only vectors
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(ReadOnlyVector[] columns) => CreateMatrixFromColumns(columns.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the rows supplied
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(IEnumerable<float[]> rows) => CreateMatrixFromRows(rows.ToArray().AsSpan());

        /// <summary>
        /// Creates a matrix from the columns supplied
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(IEnumerable<float[]> columns) => CreateMatrixFromColumns(columns.ToArray().AsSpan());

        /// <summary>
        /// Creates a matrix from the rows supplied
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(float[][] rows) => CreateMatrixFromRows(rows.AsSpan());

        /// <summary>
        /// Creates a matrix from the columns supplied
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(float[][] columns) => CreateMatrixFromColumns(columns.AsSpan());

        /// <summary>
        /// Creates a matrix from the rows supplied as tensor segments
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromRows(IReadOnlyNumericSegment<float>[] rows)
        {
            var columns = rows[0].Size;
            var ret = CreateMatrix((uint)rows.Length, columns, false);
            for (var i = 0; i < rows.Length; i++)
                rows[i].CopyTo(ret.GetRow((uint)i));
            return ret;
        }

        /// <summary>
        /// Creates a matrix from the columns supplied as tensor segments
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromColumns(IReadOnlyNumericSegment<float>[] columns)
        {
            var rows = columns[0].Size;
            var ret = CreateMatrix(rows, (uint)columns.Length, false);
            for (var i = 0; i < columns.Length; i++)
                columns[i].CopyTo(ret.GetColumn((uint)i));
            return ret;
        }

        /// <summary>
        /// Creates a matrix from the rows supplied as tensor segments
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromRows(INumericSegment<float>[] rows)
        {
            var columns = rows[0].Size;
            var ret = CreateMatrix((uint)rows.Length, columns, false);
            for (var i = 0; i < rows.Length; i++)
                rows[i].CopyTo(ret.GetRow((uint)i));
            return ret;
        }

        /// <summary>
        /// Creates a matrix from the columns supplied as tensor segments
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromColumns(INumericSegment<float>[] columns)
        {
            var rows = columns[0].Size;
            var ret = CreateMatrix(rows, (uint)columns.Length, false);
            for (var i = 0; i < columns.Length; i++)
                columns[i].CopyTo(ret.GetColumn((uint)i));
            return ret;
        }

        /// <summary>
        /// Creates a matrix from the rows supplied as tensor segments
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromRows(ReadOnlySpan<IReadOnlyNumericSegment<float>> rows)
        {
            var columns = rows[0].Size;
            var ret = CreateMatrix((uint)rows.Length, columns, false);
            for (var i = 0; i < rows.Length; i++)
                rows[i].CopyTo(ret.GetRow((uint)i));
            return ret;
        }

        /// <summary>
        /// Creates a matrix from the columns supplied as tensor segments
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromColumns(ReadOnlySpan<IReadOnlyNumericSegment<float>> columns)
        {
            var rows = columns[0].Size;
            var ret = CreateMatrix(rows, (uint)columns.Length, false);
            for (var i = 0; i < columns.Length; i++)
                columns[i].CopyTo(ret.GetColumn((uint)i));
            return ret;
        }

        /// <summary>
        /// Creates a matrix from rows supplied
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromRows(ReadOnlySpan<float[]> rows)
        {
            var columns = (uint)rows[0].Length;
            var ret = CreateMatrix((uint)rows.Length, columns, false);
            for (var i = 0; i < rows.Length; i++) {
                var source = rows[i].AsSpan();
                var targetRow = ret.GetRow((uint)i);
                targetRow.CopyFrom(source);
            }
            return ret;
        }

        /// <summary>
        /// Creates a matrix from the columns supplied
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromColumns(ReadOnlySpan<float[]> columns)
        {
            var rows = (uint)columns[0].Length;
            var ret = CreateMatrix(rows, (uint)columns.Length, false);
            for (var i = 0; i < columns.Length; i++) {
                var source = columns[i].AsSpan();
                var target = ret.GetColumn((uint)i);
                target.CopyFrom(source);
            }
            return ret;
        }

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rowCount">Rows in each matrix</param>
        /// <param name="columnCount">Columns in each matrix</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount, INumericSegment<float> data) => new BrightTensor3D(data, depth, rowCount, columnCount, this);

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rowCount">Rows in each matrix</param>
        /// <param name="columnCount">Columns in each matrix</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount, IReadOnlyNumericSegment<float> data) => new BrightTensor3D(Clone(data), depth, rowCount, columnCount, this);

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rowCount">Rows in each matrix</param>
        /// <param name="columnCount">Columns in each matrix</param>
        /// <param name="initialiseToZero">True to initialize each value to zero</param>
        /// <returns></returns>
        public ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount, bool initialiseToZero) => CreateTensor3D(depth, rowCount, columnCount, CreateSegment(depth * rowCount * columnCount, initialiseToZero));

        /// <summary>
        /// Creates a 3D tensor from existing matrices
        /// </summary>
        /// <param name="matrices">Matrices that will form the 3D tensor</param>
        /// <returns></returns>
        public ITensor3D CreateTensor3D(params IMatrix[] matrices) => CreateTensor3D(matrices.AsSpan());

        /// <summary>
        /// Creates a 3D tensor from existing matrices
        /// </summary>
        /// <param name="matrices">Matrices that will form the 3D tensor</param>
        /// <returns></returns>
        public ITensor3D CreateTensor3D(params IReadOnlyMatrix[] matrices) => CreateTensor3D(matrices.AsSpan());

        /// <summary>
        /// Creates a 3D tensor from another 3D tensor (clone)
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public ITensor3D CreateTensor3D(ITensor3D tensor) => CreateTensor3D(tensor.Depth, tensor.RowCount, tensor.ColumnCount, Clone(tensor.Segment));

        /// <summary>
        /// Creates a 3D tensor from a read only 3D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public ITensor3D CreateTensor3D(IReadOnlyTensor3D tensor)
        {
            var matrices = new IReadOnlyMatrix[tensor.Depth];
            for (uint i = 0; i < tensor.Depth; i++)
                matrices[i] = tensor.GetMatrix(i);
            return CreateTensor3D(matrices);
        }

        /// <summary>
        /// Creates a 3D tensor from existing matrices and then disposes each matrix
        /// </summary>
        /// <param name="matrices"></param>
        /// <returns></returns>
        public ITensor3D CreateTensor3DAndThenDisposeInput(params IMatrix[] matrices)
        {
            try {
                return CreateTensor3D(matrices.AsSpan());
            }
            finally {
                foreach (var item in matrices)
                    item.Dispose();
            }
        }

        /// <summary>
        /// Creates a 3D tensor from existing matrices and then disposes each matrix
        /// </summary>
        /// <param name="matrices"></param>
        /// <returns></returns>
        public ITensor3D CreateTensor3DAndThenDisposeInput(Span<IMatrix> matrices)
        {
            try {
                return CreateTensor3D(matrices);
            }
            finally {
                foreach (var item in matrices)
                    item.Dispose();
            }
        }

        /// <summary>
        /// Creates a 3D tensor from existing matrices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrices"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ITensor3D CreateTensor3D<T>(Span<T> matrices) where T : IReadOnlyMatrix
        {
            var first = matrices[0];
            var depth = (uint)matrices.Length;
            var rows = first.RowCount;
            var columns = first.ColumnCount;

            var data = CreateSegment(depth * rows * columns, false);
            var ret = CreateTensor3D(depth, rows, columns, data);
            var allSame = true;
            for (uint i = 0; i < ret.Depth; i++) {
                using var t = ret.GetMatrix(i);
                var s = matrices[(int)i];
                if (s.RowCount == t.RowCount && s.ColumnCount == t.ColumnCount)
                    s.ReadOnlySegment.CopyTo(t.Segment);
                else {
                    allSame = false;
                    break;
                }
            }

            if (!allSame) {
                throw new ArgumentException("Input matrices had different sizes");
            }

            return ret;
        }

        /// <summary>
        /// Creates a 4D tensor
        /// </summary>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices in each 3D tensor</param>
        /// <param name="rowCount">Number of rows in each matrix</param>
        /// <param name="columnCount">Number of columns in each matrix</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, IReadOnlyNumericSegment<float> data) => new BrightTensor4D(Clone(data), count, depth, rowCount, columnCount, this);

        /// <summary>
        /// Creates a 4D tensor
        /// </summary>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices in each 3D tensor</param>
        /// <param name="rowCount">Number of rows in each matrix</param>
        /// <param name="columnCount">Number of columns in each matrix</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, INumericSegment<float> data) => new BrightTensor4D(data, count, depth, rowCount, columnCount, this);

        /// <summary>
        /// Creates a 4D tensor
        /// </summary>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices in each 3D tensor</param>
        /// <param name="rowCount">Number of rows in each matrix</param>
        /// <param name="columnCount">Number of columns in each matrix</param>
        /// <param name="initialiseToZero">True to initialize each value to zero</param>
        /// <returns></returns>
        public ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, bool initialiseToZero) => CreateTensor4D(count, depth, rowCount, columnCount, CreateSegment(count * depth * rowCount * columnCount, initialiseToZero));

        /// <summary>
        /// Creates a 4D tensor from existing 3D tensors
        /// </summary>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public ITensor4D CreateTensor4D(params ITensor3D[] tensors) => CreateTensor4D(tensors.AsSpan());

        /// <summary>
        /// Creates a 4D tensor from existing 3D tensors
        /// </summary>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public ITensor4D CreateTensor4D(params IReadOnlyTensor3D[] tensors) => CreateTensor4D(tensors.AsSpan());

        /// <summary>
        /// Clones this 4D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public ITensor4D CreateTensor4D(ITensor4D tensor) => CreateTensor4D(tensor.Count, tensor.Depth, tensor.RowCount, tensor.ColumnCount, Clone(tensor.Segment));

        /// <summary>
        /// Creates a 4D tensor from an existing 4D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public ITensor4D CreateTensor4D(IReadOnlyTensor4D tensor) => CreateTensor4D(tensor.Count, tensor.Depth, tensor.RowCount, tensor.ColumnCount, tensor.ReadOnlySegment);

        /// <summary>
        /// Creates a 4D tensor from existing 3D tensors and then disposes each 3D tensor
        /// </summary>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public ITensor4D CreateTensor4DAndThenDisposeInput(params ITensor3D[] tensors)
        {
            try {
                return CreateTensor4D(tensors.AsSpan());
            }
            finally {
                foreach (var item in tensors)
                    item.Dispose();
            }
        }

        /// <summary>
        /// Creates a 4D tensor from existing 3D tensors and then disposes each 3D tensor
        /// </summary>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public ITensor4D CreateTensor4DAndThenDisposeInput(Span<ITensor3D> tensors)
        {
            try {
                return CreateTensor4D(tensors);
            }
            finally {
                foreach (var item in tensors)
                    item.Dispose();
            }
        }

        /// <summary>
        /// Creates a 4D tensor from existing 3D tensors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tensors"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ITensor4D CreateTensor4D<T>(Span<T> tensors) where T : IHaveTensor3DDimensions, IHaveReadOnlyTensorSegment<float>
        {
            var first = tensors[0];
            var count = (uint)tensors.Length;
            var rows = first.RowCount;
            var columns = first.ColumnCount;
            var depth = first.Depth;

            var data = CreateSegment(depth * rows * columns * count, false);
            var ret = CreateTensor4D(count, depth, rows, columns, data);
            var allSame = true;
            for (uint i = 0; i < ret.Count; i++) {
                using var t = ret.GetTensor(i);
                var s = tensors[(int)i];
                if (s.RowCount == t.RowCount && s.ColumnCount == t.ColumnCount && s.Depth == t.Depth)
                    s.ReadOnlySegment.CopyTo(t.Segment);
                else {
                    allSame = false;
                    break;
                }
            }

            if (!allSame)
                throw new ArgumentException("Input tensors had different sizes");
            return ret;
        }

        /// <summary>
        /// Returns the size from both tensors (the size is expected to be the same)
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="tensor2"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Throws an exception if the tensors are a different size</exception>
        protected static uint GetSize(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> tensor2)
        {
            if (tensor.Size != tensor2.Size)
                throw new Exception("Expected tensors to have same size");
            return tensor.Size;
        }

        /// <summary>
        /// Creates a tensor from a tensor shape
        /// </summary>
        /// <param name="shape">Array containing the size of each dimension in the tensor</param>
        /// <param name="segment">Tensor segment</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public ITensor CreateTensor(uint[] shape, INumericSegment<float> segment)
        {
            return shape.Length switch {
                1 when shape[0] != segment.Size => throw new ArgumentException("Shape does not match segment size"),
                1 => CreateVector(segment),

                2 when shape[0] * shape[1] != segment.Size => throw new ArgumentException("Shape does not match segment size"),
                2 => CreateMatrix(shape[1], shape[0], segment),

                3 when shape[0] * shape[1] * shape[2] != segment.Size => throw new ArgumentException("Shape does not match segment size"),
                3 => CreateTensor3D(shape[2], shape[1], shape[0], segment),

                4 when shape[0] * shape[1] * shape[2] * shape[3] != segment.Size => throw new ArgumentException("Shape does not match segment size"),
                4 => CreateTensor4D(shape[3], shape[2], shape[1], shape[0], segment),

                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Adds two tensors into a new tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="tensor2"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Add(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> tensor2) => 
            tensor.ApplyReadOnlySpans(tensor2, (x, y) => x.Add(y)).ToSegment();

        /// <summary>
        /// Adds two tensors into a new tensor and applies coefficients to each element in the two tensors
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="tensor2"></param>
        /// <param name="coefficient1"></param>
        /// <param name="coefficient2"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Add(IReadOnlyNumericSegment<float> tensor, IReadOnlyNumericSegment<float> tensor2, float coefficient1, float coefficient2) =>
            tensor.ApplyReadOnlySpans(tensor2, (x, y) => x.Add(y, coefficient1, coefficient2)).ToSegment();

        /// <summary>
        /// Creates a new tensor from adding a scalar to each element in the tensor 
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Add(IReadOnlyNumericSegment<float> tensor, float scalar) => tensor.ApplyReadOnlySpan(x => x.Add(scalar)).ToSegment();

        /// <summary>
        /// Adds another tensor to the first tensor which will be modified in place
        /// </summary>
        /// <param name="target">First tensor</param>
        /// <param name="other">Other tensor</param>
        public virtual void AddInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other) => target.ApplySpans(true, other, (x, y) => x.AddInPlace(y));

        /// <summary>
        /// Adds another tensor to the first tensor and applies coefficients to each element in each tensor
        /// </summary>
        /// <param name="target">First tensor</param>
        /// <param name="other">Other tensor</param>
        /// <param name="coefficient1">Coefficient applied to each element of the first tensor</param>
        /// <param name="coefficient2">Coefficient applied to each element of the other tensor</param>
        public virtual void AddInPlace(INumericSegment<float> target, IReadOnlyNumericSegment<float> other, float coefficient1, float coefficient2) => target.ApplySpans(true, other, (x, y) => x.AddInPlace(y, coefficient1, coefficient2));

        /// <summary>
        /// Adds a scalar to each element of this tensor - modified in place
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scalar"></param>
        public virtual void AddInPlace(INumericSegment<float> target, float scalar) => target.ApplySpan(true, x => x.AddInPlace(scalar));

        /// <summary>
        /// Multiplies each element of the tensor by a scalar - modified in place
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scalar"></param>
        public virtual void MultiplyInPlace(INumericSegment<float> target, float scalar) => target.ApplySpan(true, x => x.MultiplyInPlace(scalar));

        /// <summary>
        /// Creates a new tensor by multiplying each element of the tensor with a scalar
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Multiply(IReadOnlyNumericSegment<float> target, float scalar) => target.ApplyReadOnlySpan(x => x.Multiply(scalar)).ToSegment();

        /// <summary>
        /// Subtracts the second tensor from the first tensor into a new tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual INumericSegment<float> Subtract(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.Subtract(y)).ToSegment();

        /// <summary>
        /// Subtracts the second tensor from the first tensor into a new tensor and applies coefficients to each value in each tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <param name="coefficient1">Coefficient to apply to each element in the first tensor</param>
        /// <param name="coefficient2">Coefficient to apply to each element in the second tensor</param>
        /// <returns></returns>
        public virtual INumericSegment<float> Subtract(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2, float coefficient1, float coefficient2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.Subtract(y, coefficient1, coefficient2)).ToSegment();

        /// <summary>
        /// Subtracts the second tensor from the first tensor - first tensor modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        public virtual void SubtractInPlace(INumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplySpans(true, tensor2, (x, y) => x.SubtractInPlace(y));

        /// <summary>
        /// Subtracts the second tensor from the first tensor and applies coefficient to each value in the tensors - first tensor modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <param name="coefficient1">Coefficient to apply to each element in the first tensor</param>
        /// <param name="coefficient2">Coefficient to apply to each element in the second tensor</param>
        public virtual void SubtractInPlace(INumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2, float coefficient1, float coefficient2) => tensor1.ApplySpans(true, tensor2, (x, y) => x.SubtractInPlace(y, coefficient1, coefficient2));

        /// <summary>
        /// Creates a new tensor by multiplying each element in the first tensor with the corresponding value in the second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual INumericSegment<float> PointwiseMultiply(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.PointwiseMultiply(y)).ToSegment();

        /// <summary>
        /// Multiplies each element in the first tensor with the corresponding value in the second tensor - first tensor modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        public virtual void PointwiseMultiplyInPlace(INumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplySpans(true, tensor2, (x, y) => x.PointwiseMultiplyInPlace(y));

        /// <summary>
        /// Creates a new tensor by dividing each element in the first tensor with the corresponding value in the second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual INumericSegment<float> PointwiseDivide(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.PointwiseDivide(y)).ToSegment();

        /// <summary>
        /// Dividing each element in the first tensor with the corresponding value in the second tensor - first tensor is modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        public virtual void PointwiseDivideInPlace(INumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplySpans(true, tensor2, (x, y) => x.PointwiseDivideInPlace(y));

        /// <summary>
        /// Calculates the dot product of the first with the second tensor
        /// </summary>
        /// <param name="tensor1"></param>
        /// <param name="tensor2"></param>
        /// <returns></returns>
        public virtual float DotProduct(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.DotProduct(y));

        /// <summary>
        /// Creates a new tensor that contains the square root of each value in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="adjustment">A small value to add to each value in case of zeros</param>
        /// <returns></returns>
        public virtual INumericSegment<float> Sqrt(IReadOnlyNumericSegment<float> tensor, float adjustment = FloatMath.AlmostZero) => tensor.ApplyReadOnlySpan(x => x.Sqrt(adjustment)).ToSegment();

        /// <summary>
        /// Constrains each value in this tensor to lie between the min and max values (if supplied)
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public virtual void ConstrainInPlace(INumericSegment<float> tensor, float? minValue, float? maxValue) => tensor.ApplySpan(true, x => x.ConstrainInPlace(minValue, maxValue));

        /// <summary>
        /// Finds the average of the values in the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float Average(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Average());

        /// <summary>
        /// Finds the L1 norm (manhattan distance) of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float L1Norm(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.L1Norm());

        /// <summary>
        /// Finds the L2 norm (euclidean distance) of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float L2Norm(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.L2Norm());

        /// <summary>
        /// Finds the minimum and maximum values of the tensor (and their corresponding indices)
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.GetMinAndMaxValues());

        /// <summary>
        /// Finds the index of the minimum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual uint GetMinIndex(IReadOnlyNumericSegment<float> tensor) => GetMinAndMaxValues(tensor).MinIndex;

        /// <summary>
        /// Finds the index of the maximum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual uint GetMaxIndex(IReadOnlyNumericSegment<float> tensor) => GetMinAndMaxValues(tensor).MaxIndex;

        /// <summary>
        /// Finds the minimum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float GetMin(IReadOnlyNumericSegment<float> tensor) => GetMinAndMaxValues(tensor).Min;

        /// <summary>
        /// Finds the maximum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float GetMax(IReadOnlyNumericSegment<float> tensor) => GetMinAndMaxValues(tensor).Max;

        /// <summary>
        /// Checks if the tensor is entirely finite (not NaN or Infinity)
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual bool IsEntirelyFinite(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.IsEntirelyFinite());

        /// <summary>
        /// Reverse the order of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Reverse(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Reverse()).ToSegment();

        /// <summary>
        /// Splits the tensor into separate (contiguous) sub tensors
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="blockCount">Number of blocks to split into</param>
        /// <returns></returns>
        public virtual IEnumerable<IReadOnlyNumericSegment<float>> Split(IReadOnlyNumericSegment<float> tensor, uint blockCount) => tensor.Split(blockCount);

        /// <summary>
        /// Finds the cosine distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float CosineDistance(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.CosineDistance(y));

        /// <summary>
        /// Finds the euclidean distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float EuclideanDistance(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.EuclideanDistance(y));

        /// <summary>
        /// Finds the mean squared distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float MeanSquaredDistance(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.MeanSquaredDistance(y));

        /// <summary>
        /// Finds the squared euclidean distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float SquaredEuclideanDistance(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.SquaredEuclideanDistance(y));

        /// <summary>
        /// Finds the manhattan distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float ManhattanDistance(IReadOnlyNumericSegment<float> tensor1, IReadOnlyNumericSegment<float> tensor2) => tensor1.ApplyReadOnlySpans(tensor2, (x, y) => x.ManhattanDistance(y));

        /// <summary>
        /// Creates a new tensor that contains the absolute value of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Abs(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Abs()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the natural logarithm of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Log(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Log()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the exponent of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Exp(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Exp()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the square of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Squared(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Squared()).ToSegment();

        /// <summary>
        /// Finds the standard deviation of each element in the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="mean">Pre-calculated mean of the tensor or null to calculate</param>
        /// <returns></returns>
        public virtual float StdDev(IReadOnlyNumericSegment<float> tensor, float? mean) => tensor.ApplyReadOnlySpan(x => x.StdDev(mean));

        /// <summary>
        /// Creates a new tensor that contains the sigmoid function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Sigmoid(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Sigmoid()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the sigmoid function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> SigmoidDerivative(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.SigmoidDerivative()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the tanh function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Tanh(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Tanh()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the tanh function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> TanhDerivative(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.TanhDerivative()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Relu(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Relu()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> ReluDerivative(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.ReluDerivative()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the leaky relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> LeakyRelu(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.LeakyRelu()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the leaky relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> LeakyReluDerivative(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.LeakyReluDerivative()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the softmax function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Softmax(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Softmax()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the softmax function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual IMatrix SoftmaxDerivative(IReadOnlyNumericSegment<float> tensor)
        {
            return CreateMatrix(tensor.Size, tensor.Size, (x, y) => {
                var xVal = tensor[x];
                return x == y
                    ? xVal * (1 - xVal)
                    : -xVal * tensor[y];
            });
        }

        /// <summary>
        /// Creates a new tensor that each element of the tensor raised to the specified power
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> Pow(IReadOnlyNumericSegment<float> tensor, float power) => tensor.ApplyReadOnlySpan(x => x.Pow(power)).ToSegment();

        /// <summary>
        /// Rounds each element in the tensor to be either the lower or the upper parameter depending on its distance
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public virtual void RoundInPlace(INumericSegment<float> tensor, float lower, float upper) => tensor.ApplySpan(true, x => x.RoundInPlace(lower, upper));

        /// <summary>
        /// Returns a new tensor with only the supplied indices
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public virtual INumericSegment<float> CherryPickIndices(IReadOnlyNumericSegment<float> tensor, params uint[] indices) => tensor.ApplyReadOnlySpan(x => x.CherryPickIndices(indices)).ToSegment();

        /// <summary>
        /// Returns the sum of each matrix column
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual IVector ColumnSums(ITensor4D tensor)
        {
            IVector? ret = null;
            for (uint i = 0, count = tensor.Count; i < count; i++) {
                using var subTensor = tensor.GetTensor(i);
                using var tensorAsMatrix = subTensor.Reshape(subTensor.RowCount * subTensor.ColumnCount, subTensor.Depth);
                var columnSums = tensorAsMatrix.ColumnSums();
                if (ret == null)
                    ret = columnSums;
                else {
                    ret.AddInPlace(columnSums);
                    columnSums.Dispose();
                }
            }
            return ret ?? CreateVector(tensor.Depth, true);
        }

        /// <summary>
        /// In place L1 regularization of the tensor
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="coefficient"></param>
        public virtual void L1Regularisation(INumericSegment<float> segment, float coefficient) => segment.ApplySpan(true, x => x.L1Regularization(coefficient));

        /// <summary>
        /// Applies zero padding to each matrix in the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public virtual ITensor3D AddPadding(ITensor3D tensor, uint padding)
        {
            var newRows = tensor.RowCount + padding * 2;
            var newColumns = tensor.ColumnCount + padding * 2;
            var ret = CreateTensor3D(tensor.Depth, newRows, newColumns, true);

            for (uint k = 0; k < tensor.Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        if (i < padding || j < padding)
                            continue;
                        if (i >= newRows - padding || j >= newColumns - padding)
                            continue;
                        ret[k, j, i] = tensor[k, j - padding, i - padding];
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Removes padding from each matrix in a tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public virtual ITensor3D RemovePadding(ITensor3D tensor, uint padding)
        {
            var newRows = tensor.RowCount - padding * 2;
            var newColumns = tensor.ColumnCount - padding * 2;
            var ret = CreateTensor3D(tensor.Depth, newRows, newColumns, true);
            for (uint k = 0; k < tensor.Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        ret[k, j, i] = tensor[k, j + padding, i + padding];
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Applies the convolutional operator the 3D tensor to obtain a matrix
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        public virtual IMatrix Im2Col(ITensor3D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(tensor.ColumnCount, tensor.RowCount, filterWidth, filterHeight, xStride, yStride);
            var filterSize = filterWidth * filterHeight;
            var ret = CreateMatrix((uint)convolutions.Count, filterSize * tensor.Depth, (_, _) => 0f);

            for (int i = 0; i < convolutions.Count; i++) {
                var (offsetX, offsetY) = convolutions[i];
                for (uint k = 0; k < tensor.Depth; k++) {
                    var filterOffset = k * filterSize;
                    for (uint y = 0; y < filterHeight; y++) {
                        for (uint x = 0; x < filterWidth; x++) {
                            // write in column major format
                            var filterIndex = filterOffset + (x * filterHeight + y);
                            ret[(uint)i, filterIndex] = tensor[k, offsetY + y, offsetX + x];
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Reverses a previous Im2Col operation
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="filter"></param>
        /// <param name="outputRows"></param>
        /// <param name="outputColumns"></param>
        /// <param name="outputDepth"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        public virtual ITensor3D ReverseIm2Col(ITensor3D tensor, IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, xStride, yStride);
            using var temp = SpanOwner<IMatrix>.Allocate((int)outputDepth, AllocationMode.Default);
            var ptr = temp.Span;

            for (var i = 0; i < outputDepth; i++)
                ptr[i] = CreateMatrix(outputRows, outputColumns, true);
            for (uint k = 0; k < tensor.Depth; k++) {
                using var slice = tensor.GetMatrix(k);
                var filters = filter.GetReadOnlyColumn(k).Split(outputDepth).ToArray();

                foreach (var (cx, cy) in convolutions) {
                    var errorY = cy / xStride;
                    var errorX = cx / yStride;
                    if (errorX < slice.ColumnCount && errorY < slice.RowCount) {
                        var error = slice[errorY, errorX];
                        for (uint y = 0; y < filterHeight; y++) {
                            for (uint x = 0; x < filterWidth; x++) {
                                var filterIndex = (filterWidth - x - 1) * filterHeight + (filterHeight - y - 1);
                                for (uint z = 0; z < outputDepth; z++)
                                    ptr[(int)z][cy + y, cx + x] += filters[z][filterIndex] * error;
                            }
                        }
                    }
                }
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Calculates the max pooling operator of the 3D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <param name="saveIndices"></param>
        /// <returns></returns>
        public virtual (ITensor3D Result, ITensor3D? Indices) MaxPool(ITensor3D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var newColumns = (tensor.ColumnCount - filterWidth) / xStride + 1;
            var newRows = (tensor.RowCount - filterHeight) / yStride + 1;
            using var temp = SpanOwner<IMatrix>.Allocate((int)tensor.Depth, AllocationMode.Default);
            var ptr = temp.Span;
            var indexList = saveIndices ? new IMatrix[tensor.Depth] : null;
            var convolutions = ConvolutionHelper.Default(tensor.ColumnCount, tensor.RowCount, filterWidth, filterHeight, xStride, yStride);

            for (uint k = 0; k < tensor.Depth; k++) {
                var indices = saveIndices ? CreateMatrix(newRows, newColumns, true) : null;
                var layer = CreateMatrix(newRows, newColumns, true);

                foreach (var (cx, cy) in convolutions) {
                    var targetX = cx / xStride;
                    var targetY = cy / yStride;
                    var maxVal = float.MinValue;
                    var bestOffset = -1;
                    var offset = 0;

                    for (uint x = 0; x < filterWidth; x++) {
                        for (uint y = 0; y < filterHeight; y++) {
                            var val = tensor[k, cy + y, cx + x];
                            if (val > maxVal || bestOffset == -1) {
                                bestOffset = offset;
                                maxVal = val;
                            }

                            ++offset;
                        }
                    }

                    if (indices != null)
                        indices[targetY, targetX] = bestOffset;
                    layer[targetY, targetX] = maxVal;
                }

                ptr[(int)k] = layer;
                if (indexList != null && indices != null)
                    indexList[k] = indices;
            }

            return (
                CreateTensor3DAndThenDisposeInput(ptr),
                indexList != null ? CreateTensor3DAndThenDisposeInput(indexList) : null
            );
        }

        /// <summary>
        /// Calculates a reverse max pool operation
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="indices"></param>
        /// <param name="outputRows"></param>
        /// <param name="outputColumns"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        public virtual ITensor3D ReverseMaxPool(ITensor3D tensor, ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var temp = SpanOwner<IMatrix>.Allocate((int)tensor.Depth, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint k = 0; k < tensor.Depth; k++) {
                using var source = tensor.GetMatrix(k);
                var sourceRows = source.RowCount;
                var sourceColumns = source.ColumnCount;
                using var index = indices.GetMatrix(k);
                var target = ptr[(int)k] = CreateMatrix(outputRows, outputColumns, true);

                for (uint j = 0; j < sourceColumns; j++) {
                    for (uint i = 0; i < sourceRows; i++) {
                        var value = source[i, j];
                        var offset = index[i, j];
                        var offsetRow = (uint)offset % filterHeight;
                        var offsetColumn = (uint)offset / filterHeight;
                        target[(int)(i * yStride + offsetRow), (int)(j * xStride + offsetColumn)] = value;
                    }
                }
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Creates the pointwise sum of each matrix within the 3D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual IMatrix AddMatrices(ITensor3D tensor)
        {
            var ret = CreateMatrix(tensor.RowCount, tensor.ColumnCount, true);

            for (uint i = 0; i < tensor.Depth; i++) {
                using var matrix = tensor.GetMatrix(i);
                ret.AddInPlace(matrix);
            }

            return ret;
        }

        /// <summary>
        /// Multiplies each sub matrix within the 3D tensor with another matrix
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual ITensor3D Multiply(ITensor3D tensor, IMatrix other)
        {
            using var temp = SpanOwner<IMatrix>.Allocate((int)tensor.Depth, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < tensor.Depth; i++) {
                using var matrix = tensor.GetMatrix(i);
                ptr[(int)i] = matrix.Multiply(other);
            }
            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Transposes each sub matrix within the 3D tensor and then multiplies them with another matrix
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual ITensor3D TransposeFirstAndMultiply(ITensor3D tensor, IMatrix other)
        {
            using var temp = SpanOwner<IMatrix>.Allocate((int)tensor.Depth, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < tensor.Depth; i++) {
                using var matrix = tensor.GetMatrix(i);
                ptr[(int)i] = matrix.TransposeThisAndMultiply(other);
            }
            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Adds a vector to each row of each matrix within the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="vector"></param>
        public virtual void AddToEachRow(ITensor3D tensor, IVector vector)
        {
            for (uint k = 0; k < tensor.Depth; k++) {
                for (uint j = 0; j < tensor.ColumnCount; j++) {
                    for (uint i = 0; i < tensor.RowCount; i++)
                        tensor[k, i, j] += vector[j];
                }
            }
        }

        /// <summary>
        /// Adds a vector to each column of each matrix within the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="vector"></param>
        public virtual void AddToEachColumn(ITensor3D tensor, IVector vector)
        {
            for (uint k = 0; k < tensor.Depth; k++) {
                for (uint j = 0; j < tensor.ColumnCount; j++) {
                    for (uint i = 0; i < tensor.RowCount; i++)
                        tensor[k, i, j] += vector[i];
                }
            }
        }

        /// <summary>
        /// Multiplies each sub matrix within the 3D tensor with a reshaped matrix from each sub 3D tensor within the 4D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual ITensor3D Multiply(ITensor3D tensor, ITensor4D other)
        {
            Debug.Assert(other.Count == tensor.Depth);
            using var temp = SpanOwner<IMatrix>.Allocate((int)other.Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < other.Count; i++) {
                using var item = other.GetTensor(i);
                using var multiplyWith = item.Reshape(null, other.Depth);
                using var slice = tensor.GetMatrix(i);
                var result = slice.Multiply(multiplyWith);
                ptr[(int)i] = result;
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Multiplies each transposed sub matrix within the 3D tensor with a reshaped matrix from each sub 3D tensor within the 4D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual ITensor3D TransposeFirstAndMultiply(ITensor3D tensor, ITensor4D other)
        {
            Debug.Assert(other.Count == tensor.Depth);
            using var temp = SpanOwner<IMatrix>.Allocate((int)other.Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < other.Count; i++) {
                using var item = other.GetTensor(i);
                using var multiplyWith = item.Reshape(null, other.Depth);
                using var slice = tensor.GetMatrix(i);
                var result = slice.TransposeThisAndMultiply(multiplyWith);
                ptr[(int)i] = result;
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Multiplies each sub matrix within the 3D tensor with the transposed reshaped matrix from each sub 3D tensor within the 4D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual ITensor3D TransposeSecondAndMultiply(ITensor3D tensor, ITensor4D other)
        {
            Debug.Assert(other.Count == tensor.Depth);
            using var temp = SpanOwner<IMatrix>.Allocate((int)other.Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < other.Count; i++) {
                using var item = other.GetTensor(i);
                using var multiplyWith = item.Reshape(null, other.Depth);
                using var slice = tensor.GetMatrix(i);
                var result = slice.TransposeAndMultiply(multiplyWith);
                ptr[(int)i] = result;
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Finds the distance between each pair of vectors
        /// </summary>
        /// <param name="vectors">First set of vectors</param>
        /// <param name="compareTo">Second set of vectors</param>
        /// <param name="distanceMetric">Distance metric</param>
        /// <returns>Matrix with the rows corresponding to the first set and columns corresponding to the second set and each element containing the distance</returns>
        public virtual IMatrix FindDistances(IVector[] vectors, IReadOnlyList<IVector> compareTo, DistanceMetric distanceMetric)
        {
            var rows = (uint)compareTo.Count;
            var columns = (uint)vectors.Length;
            var ret = CreateMatrix(rows, columns, false);
            var totalSize = rows * columns;

            if (totalSize >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, rows * columns, ind => {
                    var i = (uint)(ind % rows);
                    var j = (uint)(ind / rows);
                    ret[i, j] = compareTo[(int)i].FindDistance(vectors[j], distanceMetric);
                });
            }
            else {
                for (uint i = 0; i < rows; i++) {
                    for (uint j = 0; j < columns; j++) {
                        ret[i, j] = compareTo[(int)i].FindDistance(vectors[j], distanceMetric);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Binds a new thread to this provider
        /// </summary>
        public virtual void BindThread()
        {
            // nop
        }

        /// <summary>
        /// Returns the sum of values in the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float Sum(IReadOnlyNumericSegment<float> tensor) => tensor.ApplyReadOnlySpan(x => x.Sum());

        /// <summary>
        /// Calculates the softmax of a set of tensors
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public virtual INumericSegment<float>[] MultiSoftmax(ArraySegment<IReadOnlyNumericSegment<float>> segments)
        {
            var len = segments.Count;
            var ret = new INumericSegment<float>[len];
            if (len >= Consts.MinimumSizeForParallel)
                Parallel.For(0, len, i => ret[i] = Softmax(segments[i]));
            else {
                for (var i = 0; i < len; i++)
                    ret[i] = Softmax(segments[i]);
            }
            return ret;
        }

        /// <summary>
        /// Calculates the softmax derivative of a set of tensors
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public virtual IMatrix[] MultiSoftmaxDerivative(IReadOnlyNumericSegment<float>[] segments)
        {
            var len = segments.Length;
            var ret = new IMatrix[len];
            if (len >= Consts.MinimumSizeForParallel)
                Parallel.For(0, len, i => ret[i] = SoftmaxDerivative(segments[i]));
            else {
                for (var i = 0; i < len; i++)
                    ret[i] = SoftmaxDerivative(segments[i]);
            }
            return ret;
        }
    }
}
