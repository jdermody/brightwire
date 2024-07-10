using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Linear algebra provider
    /// </summary>
    public class LinearAlgebraProvider<T> : IDisposable 
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
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
        public virtual Type VectorType { get; } = typeof(MutableVector<T, LinearAlgebraProvider<T>>);

        /// <summary>
        /// Type of matrices that will be created
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public virtual Type MatrixType { get; } = typeof(MutableMatrix<T, LinearAlgebraProvider<T>>);

        /// <summary>
        /// Type of 3D tensors that will be created
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public virtual Type Tensor3DType { get; } = typeof(MutableTensor3D<T, LinearAlgebraProvider<T>>);

        /// <summary>
        /// Type of 4D tensors that will be created
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public virtual Type Tensor4DType { get; } = typeof(MutableTensor4D<T, LinearAlgebraProvider<T>>);

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
        public virtual INumericSegment<T> CreateSegment(params T[] data) => new MutableTensorSegment<T>(data);

        /// <summary>
        /// Creates a tensor segment
        /// </summary>
        /// <param name="size">Segment size</param>
        /// <param name="initialiseToZero">True to initialize the all values in the segment to zero</param>
        /// <returns></returns>
        public virtual INumericSegment<T> CreateSegment(uint size, bool initialiseToZero) => new ArrayPoolTensorSegment<T>(MemoryOwner<T>.Allocate((int)size, initialiseToZero ? AllocationMode.Clear : AllocationMode.Default));

        /// <summary>
        /// Creates a tensor segment
        /// </summary>
        /// <param name="size">Segment size</param>
        /// <param name="initializer">Function to initialize each value in the segment</param>
        /// <returns></returns>
        public virtual INumericSegment<T> CreateSegment(uint size, Func<uint /* index */, T> initializer)
        {
            var ret = MemoryOwner<T>.Allocate((int)size, AllocationMode.Clear);
            var ptr = ret.Span;
            for (var i = 0; i < ptr.Length; i++)
                ptr[i] = initializer((uint)i);
            return new ArrayPoolTensorSegment<T>(ret);
        }

        /// <summary>
        /// Creates a tensor segment with each value initialized to an initial value
        /// </summary>
        /// <param name="size"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        public INumericSegment<T> CreateSegment(uint size, T initialValue) => CreateSegment(size, _ => initialValue);

        /// <summary>
        /// Creates a clone of the tensor segment
        /// </summary>
        /// <param name="segment">Segment to clone</param>
        /// <returns></returns>
        public virtual INumericSegment<T> Clone(IReadOnlyNumericSegment<T> segment)
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
        public virtual IVector<T> CreateVector(INumericSegment<T> data) => new MutableVector<T, LinearAlgebraProvider<T>>(data, this);

        /// <summary>
        /// Creates a vector from a read only tensor segment
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual IVector<T> CreateVector(IReadOnlyNumericSegment<T> data) => new MutableVector<T, LinearAlgebraProvider<T>>(Clone(data), this);

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="size">Size of the vector</param>
        /// <param name="initialiseToZero">True to initialize each value to zero</param>
        /// <returns></returns>
        public IVector<T> CreateVector(uint size, bool initialiseToZero) => CreateVector(CreateSegment(size, initialiseToZero));

        /// <summary>
        /// Creates a vector from an array of values
        /// </summary>
        /// <param name="data">T array</param>
        /// <returns></returns>
        public IVector<T> CreateVector(params T[] data) => CreateVector(data.AsSpan());

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="size">Size of the vector</param>
        /// <param name="value">Initial value of each item in the vector</param>
        /// <returns></returns>
        public IVector<T> CreateVector(uint size, T value) => CreateVector(size, _ => value);

        /// <summary>
        /// Creates a vector from a span of values
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public IVector<T> CreateVector(ReadOnlySpan<T> span)
        {
            var segment = CreateSegment((uint)span.Length, false);
            segment.CopyFrom(span);
            return CreateVector(segment);
        }

        /// <summary>
        /// Creates a vector from a memory block of values
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public IVector<T> CreateVector(ReadOnlyMemory<T> memory)
        {
            var segment = CreateSegment((uint)memory.Length, false);
            segment.CopyFrom(memory.Span);
            return CreateVector(segment);
        }

        /// <summary>
        /// Creates a vector
        /// </summary>
        /// <param name="size">Size of the vector</param>
        /// <param name="initializer">Function to initialize each value in the vector</param>
        /// <returns></returns>
        public IVector<T> CreateVector(uint size, Func<uint /* index */, T> initializer) => CreateVector(CreateSegment(size, initializer));

        /// <summary>
        /// Creates a new vector from an existing vector
        /// </summary>
        /// <param name="vector">Vector to clone</param>
        /// <returns></returns>
        public IVector<T> CreateVector(IVector<T> vector) => CreateVector(Clone(vector.Segment));

        /// <summary>
        /// Creates a vector from a read only vector
        /// </summary>
        /// <param name="vector">Read only vector</param>
        /// <returns></returns>
        public IVector<T> CreateVector(IReadOnlyVector<T> vector) => CreateVector(Clone(vector.ReadOnlySegment));

        /// <summary>
        /// Creates a vector from an enumerable of floats
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public IVector<T> CreateVector(IEnumerable<T> values) => CreateVector(values.ToArray().AsSpan());

        /// <summary>
        /// Creates a matrix from a segment
        /// </summary>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="columnCount">Number of columns</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual IMatrix<T> CreateMatrix(uint rowCount, uint columnCount, INumericSegment<T> data) => new MutableMatrix<T, LinearAlgebraProvider<T>>(data, rowCount, columnCount, this);

        /// <summary>
        /// Creates a matrix from a segment
        /// </summary>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="columnCount">Number of columns</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual IMatrix<T> CreateMatrix(uint rowCount, uint columnCount, IReadOnlyNumericSegment<T> data) => new MutableMatrix<T, LinearAlgebraProvider<T>>(Clone(data), rowCount, columnCount, this);

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="columnCount">Number of columns</param>
        /// <param name="initialiseToZero">True to initialize each value to zero</param>
        /// <returns></returns>
        public virtual IMatrix<T> CreateMatrix(uint rowCount, uint columnCount, bool initialiseToZero) => CreateMatrix(rowCount, columnCount, CreateSegment(rowCount * columnCount, initialiseToZero));

        /// <summary>
        /// Creates a matrix
        /// </summary>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="columnCount">Number of columns</param>
        /// <param name="initializer">Function to initialize each value in the matrix that will receive (row index, column index)</param>
        /// <returns></returns>
        public virtual IMatrix<T> CreateMatrix(uint rowCount, uint columnCount, Func<uint /* row index */, uint /* column index */, T> initializer)
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
        public IMatrix<T> CreateMatrix(IMatrix<T> matrix) => CreateMatrix(matrix.RowCount, matrix.ColumnCount, Clone(matrix.Segment));

        /// <summary>
        /// Creates a matrix from a read only matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrix(IReadOnlyMatrix<T> matrix) => CreateMatrix(matrix.RowCount, matrix.ColumnCount, matrix.ReadOnlySegment);

        /// <summary>
        /// Creates a matrix from the rows supplied as vectors
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromRows(IVector<T>[] rows) => CreateMatrixFromRows(rows.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the columns supplied as vectors
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromColumns(IVector<T>[] columns) => CreateMatrixFromColumns(columns.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the rows supplied as read only vectors
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromRows(IReadOnlyVector<T>[] rows) => CreateMatrixFromRows(rows.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the columns supplied as read only vectors
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromColumns(IReadOnlyVector<T>[] columns) => CreateMatrixFromColumns(columns.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the rows supplied as read only vectors
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromRows(ReadOnlyVector<T>[] rows) => CreateMatrixFromRows(rows.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the columns supplied as read only vectors
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromColumns(ReadOnlyVector<T>[] columns) => CreateMatrixFromColumns(columns.Select(v => v.ReadOnlySegment).ToArray());

        /// <summary>
        /// Creates a matrix from the rows supplied
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromRows(IEnumerable<T[]> rows) => CreateMatrixFromRows(rows.ToArray().AsSpan());

        /// <summary>
        /// Creates a matrix from the columns supplied
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromColumns(IEnumerable<T[]> columns) => CreateMatrixFromColumns(columns.ToArray().AsSpan());

        /// <summary>
        /// Creates a matrix from the rows supplied
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromRows(T[][] rows) => CreateMatrixFromRows(rows.AsSpan());

        /// <summary>
        /// Creates a matrix from the columns supplied
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix<T> CreateMatrixFromColumns(T[][] columns) => CreateMatrixFromColumns(columns.AsSpan());

        /// <summary>
        /// Creates a matrix from the rows supplied as tensor segments
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public virtual IMatrix<T> CreateMatrixFromRows(IReadOnlyNumericSegment<T>[] rows)
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
        public virtual IMatrix<T> CreateMatrixFromColumns(IReadOnlyNumericSegment<T>[] columns)
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
        public virtual IMatrix<T> CreateMatrixFromRows(INumericSegment<T>[] rows)
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
        public virtual IMatrix<T> CreateMatrixFromColumns(INumericSegment<T>[] columns)
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
        public virtual IMatrix<T> CreateMatrixFromRows(ReadOnlySpan<IReadOnlyNumericSegment<T>> rows)
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
        public virtual IMatrix<T> CreateMatrixFromColumns(ReadOnlySpan<IReadOnlyNumericSegment<T>> columns)
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
        public virtual IMatrix<T> CreateMatrixFromRows(ReadOnlySpan<T[]> rows)
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
        public virtual IMatrix<T> CreateMatrixFromColumns(ReadOnlySpan<T[]> columns)
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
        public virtual ITensor3D<T> CreateTensor3D(uint depth, uint rowCount, uint columnCount, INumericSegment<T> data) => new MutableTensor3D<T, LinearAlgebraProvider<T>>(data, depth, rowCount, columnCount, this);

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rowCount">Rows in each matrix</param>
        /// <param name="columnCount">Columns in each matrix</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual ITensor3D<T> CreateTensor3D(uint depth, uint rowCount, uint columnCount, IReadOnlyNumericSegment<T> data) => new MutableTensor3D<T, LinearAlgebraProvider<T>>(Clone(data), depth, rowCount, columnCount, this);

        /// <summary>
        /// Creates a 3D tensor
        /// </summary>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rowCount">Rows in each matrix</param>
        /// <param name="columnCount">Columns in each matrix</param>
        /// <param name="initialiseToZero">True to initialize each value to zero</param>
        /// <returns></returns>
        public ITensor3D<T> CreateTensor3D(uint depth, uint rowCount, uint columnCount, bool initialiseToZero) => CreateTensor3D(depth, rowCount, columnCount, CreateSegment(depth * rowCount * columnCount, initialiseToZero));

        /// <summary>
        /// Creates a 3D tensor from existing matrices
        /// </summary>
        /// <param name="matrices">Matrices that will form the 3D tensor</param>
        /// <returns></returns>
        public ITensor3D<T> CreateTensor3D(params IMatrix<T>[] matrices) => CreateTensor3D(matrices.AsSpan());

        /// <summary>
        /// Creates a 3D tensor from existing matrices
        /// </summary>
        /// <param name="matrices">Matrices that will form the 3D tensor</param>
        /// <returns></returns>
        public ITensor3D<T> CreateTensor3D(params IReadOnlyMatrix<T>[] matrices) => CreateTensor3D(matrices.AsSpan());

        /// <summary>
        /// Creates a 3D tensor from another 3D tensor (clone)
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public ITensor3D<T> CreateTensor3D(ITensor3D<T> tensor) => CreateTensor3D(tensor.Depth, tensor.RowCount, tensor.ColumnCount, Clone(tensor.Segment));

        /// <summary>
        /// Creates a 3D tensor from a read only 3D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public ITensor3D<T> CreateTensor3D(IReadOnlyTensor3D<T> tensor)
        {
            var matrices = new IReadOnlyMatrix<T>[tensor.Depth];
            for (uint i = 0; i < tensor.Depth; i++)
                matrices[i] = tensor.GetMatrix(i);
            return CreateTensor3D(matrices);
        }

        /// <summary>
        /// Creates a 3D tensor from existing matrices and then disposes each matrix
        /// </summary>
        /// <param name="matrices"></param>
        /// <returns></returns>
        public ITensor3D<T> CreateTensor3DAndThenDisposeInput(params IMatrix<T>[] matrices)
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
        public ITensor3D<T> CreateTensor3DAndThenDisposeInput(Span<IMatrix<T>> matrices)
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
        /// <typeparam name="TT"></typeparam>
        /// <param name="matrices"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ITensor3D<T> CreateTensor3D<TT>(Span<TT> matrices) 
            where TT : IReadOnlyMatrix<T>
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
        public virtual ITensor4D<T> CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, IReadOnlyNumericSegment<T> data) => new MutableTensor4D<T, LinearAlgebraProvider<T>>(Clone(data), count, depth, rowCount, columnCount, this);

        /// <summary>
        /// Creates a 4D tensor
        /// </summary>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices in each 3D tensor</param>
        /// <param name="rowCount">Number of rows in each matrix</param>
        /// <param name="columnCount">Number of columns in each matrix</param>
        /// <param name="data">Tensor segment</param>
        /// <returns></returns>
        public virtual ITensor4D<T> CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, INumericSegment<T> data) => new MutableTensor4D<T, LinearAlgebraProvider<T>>(data, count, depth, rowCount, columnCount, this);

        /// <summary>
        /// Creates a 4D tensor
        /// </summary>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices in each 3D tensor</param>
        /// <param name="rowCount">Number of rows in each matrix</param>
        /// <param name="columnCount">Number of columns in each matrix</param>
        /// <param name="initialiseToZero">True to initialize each value to zero</param>
        /// <returns></returns>
        public ITensor4D<T> CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, bool initialiseToZero) => CreateTensor4D(count, depth, rowCount, columnCount, CreateSegment(count * depth * rowCount * columnCount, initialiseToZero));

        /// <summary>
        /// Creates a 4D tensor from existing 3D tensors
        /// </summary>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public ITensor4D<T> CreateTensor4D(params ITensor3D<T>[] tensors) => CreateTensor4D(tensors.AsSpan());

        /// <summary>
        /// Creates a 4D tensor from existing 3D tensors
        /// </summary>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public ITensor4D<T> CreateTensor4D(params IReadOnlyTensor3D<T>[] tensors) => CreateTensor4D(tensors.AsSpan());

        /// <summary>
        /// Clones this 4D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public ITensor4D<T> CreateTensor4D(ITensor4D<T> tensor) => CreateTensor4D(tensor.Count, tensor.Depth, tensor.RowCount, tensor.ColumnCount, Clone(tensor.Segment));

        /// <summary>
        /// Creates a 4D tensor from an existing 4D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public ITensor4D<T> CreateTensor4D(IReadOnlyTensor4D<T> tensor) => CreateTensor4D(tensor.Count, tensor.Depth, tensor.RowCount, tensor.ColumnCount, tensor.ReadOnlySegment);

        /// <summary>
        /// Creates a 4D tensor from existing 3D tensors and then disposes each 3D tensor
        /// </summary>
        /// <param name="tensors"></param>
        /// <returns></returns>
        public ITensor4D<T> CreateTensor4DAndThenDisposeInput(params ITensor3D<T>[] tensors)
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
        public ITensor4D<T> CreateTensor4DAndThenDisposeInput(Span<ITensor3D<T>> tensors)
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
        /// <typeparam name="TT"></typeparam>
        /// <param name="tensors"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ITensor4D<T> CreateTensor4D<TT>(Span<TT> tensors) where TT : IHaveTensor3DDimensions, IHaveReadOnlyTensorSegment<T>
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
        protected static uint GetSize(IReadOnlyNumericSegment<T> tensor, IReadOnlyNumericSegment<T> tensor2)
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
        public ITensor<T> CreateTensor(uint[] shape, INumericSegment<T> segment)
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
        public virtual INumericSegment<T> Add(IReadOnlyNumericSegment<T> tensor, IReadOnlyNumericSegment<T> tensor2) => 
            tensor.ReduceReadOnlySpans(tensor2, (x, y) => x.Add(y)).ToSegment();

        /// <summary>
        /// Adds two tensors into a new tensor and applies coefficients to each element in the two tensors
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="tensor2"></param>
        /// <param name="coefficient1"></param>
        /// <param name="coefficient2"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Add(IReadOnlyNumericSegment<T> tensor, IReadOnlyNumericSegment<T> tensor2, T coefficient1, T coefficient2) =>
            tensor.ReduceReadOnlySpans(tensor2, (x, y) => x.Add(y, coefficient1, coefficient2)).ToSegment();

        /// <summary>
        /// Creates a new tensor from adding a scalar to each element in the tensor 
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Add(IReadOnlyNumericSegment<T> tensor, T scalar) => tensor.ApplyReadOnlySpan(x => x.Add(scalar)).ToSegment();

        /// <summary>
        /// Adds another tensor to the first tensor which will be modified in place
        /// </summary>
        /// <param name="target">First tensor</param>
        /// <param name="other">Other tensor</param>
        public virtual void AddInPlace(INumericSegment<T> target, IReadOnlyNumericSegment<T> other) => target.ApplySpans(true, other, (x, y) => x.AddInPlace(y));

        /// <summary>
        /// Adds another tensor to the first tensor and applies coefficients to each element in each tensor
        /// </summary>
        /// <param name="target">First tensor</param>
        /// <param name="other">Other tensor</param>
        /// <param name="coefficient1">Coefficient applied to each element of the first tensor</param>
        /// <param name="coefficient2">Coefficient applied to each element of the other tensor</param>
        public virtual void AddInPlace(INumericSegment<T> target, IReadOnlyNumericSegment<T> other, T coefficient1, T coefficient2) => target.ApplySpans(true, other, (x, y) => x.AddInPlace(y, coefficient1, coefficient2));

        /// <summary>
        /// Adds a scalar to each element of this tensor - modified in place
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scalar"></param>
        public virtual void AddInPlace(INumericSegment<T> target, T scalar) => target.ApplySpan(true, x => x.AddInPlace(scalar));

        /// <summary>
        /// Multiplies each element of the tensor by a scalar - modified in place
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scalar"></param>
        public virtual void MultiplyInPlace(INumericSegment<T> target, T scalar) => target.ApplySpan(true, x => x.MultiplyInPlace(scalar));

        /// <summary>
        /// Creates a new tensor by multiplying each element of the tensor with a scalar
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Multiply(IReadOnlyNumericSegment<T> target, T scalar) => target.ApplyReadOnlySpan(x => x.Multiply(scalar)).ToSegment();

        /// <summary>
        /// Subtracts the second tensor from the first tensor into a new tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual INumericSegment<T> Subtract(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.Subtract(y)).ToSegment();

        /// <summary>
        /// Subtracts the second tensor from the first tensor into a new tensor and applies coefficients to each value in each tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <param name="coefficient1">Coefficient to apply to each element in the first tensor</param>
        /// <param name="coefficient2">Coefficient to apply to each element in the second tensor</param>
        /// <returns></returns>
        public virtual INumericSegment<T> Subtract(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2, T coefficient1, T coefficient2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.Subtract(y, coefficient1, coefficient2)).ToSegment();

        /// <summary>
        /// Subtracts the second tensor from the first tensor - first tensor modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        public virtual void SubtractInPlace(INumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ApplySpans(true, tensor2, (x, y) => x.SubtractInPlace(y));

        /// <summary>
        /// Subtracts the second tensor from the first tensor and applies coefficient to each value in the tensors - first tensor modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <param name="coefficient1">Coefficient to apply to each element in the first tensor</param>
        /// <param name="coefficient2">Coefficient to apply to each element in the second tensor</param>
        public virtual void SubtractInPlace(INumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2, T coefficient1, T coefficient2) => tensor1.ApplySpans(true, tensor2, (x, y) => x.SubtractInPlace(y, coefficient1, coefficient2));

        /// <summary>
        /// Creates a new tensor by multiplying each element in the first tensor with the corresponding value in the second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual INumericSegment<T> PointwiseMultiply(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.PointwiseMultiply(y)).ToSegment();

        /// <summary>
        /// Multiplies each element in the first tensor with the corresponding value in the second tensor - first tensor modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        public virtual void PointwiseMultiplyInPlace(INumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ApplySpans(true, tensor2, (x, y) => x.PointwiseMultiplyInPlace(y));

        /// <summary>
        /// Creates a new tensor by dividing each element in the first tensor with the corresponding value in the second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual INumericSegment<T> PointwiseDivide(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.PointwiseDivide(y)).ToSegment();

        /// <summary>
        /// Dividing each element in the first tensor with the corresponding value in the second tensor - first tensor is modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        public virtual void PointwiseDivideInPlace(INumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ApplySpans(true, tensor2, (x, y) => x.PointwiseDivideInPlace(y));

        /// <summary>
        /// Calculates the dot product of the first with the second tensor
        /// </summary>
        /// <param name="tensor1"></param>
        /// <param name="tensor2"></param>
        /// <returns></returns>
        public virtual T DotProduct(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.DotProduct(y));

        /// <summary>
        /// Creates a new tensor that contains the square root of each value in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="adjustment">A small value to add to each value in case of zeros</param>
        /// <returns></returns>
        public virtual INumericSegment<T> Sqrt(IReadOnlyNumericSegment<T> tensor, T? adjustment = null) => tensor.ApplyReadOnlySpan(x => x.Sqrt(adjustment ?? Math<T>.AlmostZero)).ToSegment();

        /// <summary>
        /// Constrains each value in this tensor to lie between the min and max values (if supplied)
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public virtual void ConstrainInPlace(INumericSegment<T> tensor, T? minValue, T? maxValue) => tensor.ApplySpan(true, x => x.ConstrainInPlace(minValue ?? T.MinValue, maxValue ?? T.MaxValue));

        /// <summary>
        /// Finds the average of the values in the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual T Average(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Average());

        /// <summary>
        /// Finds the L1 norm (manhattan distance) of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual T L1Norm(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.L1Norm());

        /// <summary>
        /// Finds the L2 norm (euclidean distance) of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual T L2Norm(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.L2Norm());

        /// <summary>
        /// Finds the minimum and maximum values of the tensor (and their corresponding indices)
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.GetMinAndMaxValues());

        /// <summary>
        /// Finds the index of the minimum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual uint GetMinIndex(IReadOnlyNumericSegment<T> tensor) => GetMinAndMaxValues(tensor).MinIndex;

        /// <summary>
        /// Finds the index of the maximum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual uint GetMaxIndex(IReadOnlyNumericSegment<T> tensor) => GetMinAndMaxValues(tensor).MaxIndex;

        /// <summary>
        /// Finds the minimum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual T GetMin(IReadOnlyNumericSegment<T> tensor) => GetMinAndMaxValues(tensor).Min;

        /// <summary>
        /// Finds the maximum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual T GetMax(IReadOnlyNumericSegment<T> tensor) => GetMinAndMaxValues(tensor).Max;

        /// <summary>
        /// Checks if the tensor is entirely finite (not NaN or Infinity)
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual bool IsEntirelyFinite(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.IsEntirelyFinite());

        /// <summary>
        /// Reverse the order of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Reverse(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Reverse()).ToSegment();

        /// <summary>
        /// Splits the tensor into separate (contiguous) sub tensors
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="blockCount">Number of blocks to split into</param>
        /// <returns></returns>
        public virtual IEnumerable<IReadOnlyNumericSegment<T>> Split(IReadOnlyNumericSegment<T> tensor, uint blockCount) => tensor.Split(blockCount);

        /// <summary>
        /// Finds the cosine distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual T CosineDistance(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.CosineDistance(y));

        /// <summary>
        /// Finds the euclidean distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual T EuclideanDistance(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.EuclideanDistance(y));

        /// <summary>
        /// Finds the mean squared distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual T MeanSquaredDistance(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.MeanSquaredDistance(y));

        /// <summary>
        /// Finds the squared euclidean distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual T SquaredEuclideanDistance(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.SquaredEuclideanDistance(y));

        /// <summary>
        /// Finds the manhattan distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual T ManhattanDistance(IReadOnlyNumericSegment<T> tensor1, IReadOnlyNumericSegment<T> tensor2) => tensor1.ReduceReadOnlySpans(tensor2, (x, y) => x.ManhattanDistance(y));

        /// <summary>
        /// Creates a new tensor that contains the absolute value of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Abs(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Abs()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the natural logarithm of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Log(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Log()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the exponent of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Exp(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Exp()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the square of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Squared(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Squared()).ToSegment();

        /// <summary>
        /// Finds the standard deviation of each element in the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="mean">Pre-calculated mean of the tensor or null to calculate</param>
        /// <returns></returns>
        public virtual T StdDev(IReadOnlyNumericSegment<T> tensor, T? mean) => tensor.ApplyReadOnlySpan(x => x.StdDev(mean));

        /// <summary>
        /// Creates a new tensor that contains the sigmoid function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Sigmoid(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Sigmoid()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the sigmoid function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> SigmoidDerivative(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.SigmoidDerivative()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the tanh function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Tanh(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Tanh()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the tanh function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> TanhDerivative(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.TanhDerivative()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Relu(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Relu()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> ReluDerivative(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.ReluDerivative()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the leaky relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> LeakyRelu(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.LeakyRelu()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the leaky relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> LeakyReluDerivative(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.LeakyReluDerivative()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the softmax function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Softmax(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Softmax()).ToSegment();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the softmax function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual IMatrix<T> SoftmaxDerivative(IReadOnlyNumericSegment<T> tensor)
        {
            return CreateMatrix(tensor.Size, tensor.Size, (x, y) => {
                var xVal = tensor[x];
                return x == y
                    ? xVal * (T.One - xVal)
                    : -xVal * tensor[y];
            });
        }

        /// <summary>
        /// Creates a new tensor that each element of the tensor raised to the specified power
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> Pow(IReadOnlyNumericSegment<T> tensor, T power) => tensor.ApplyReadOnlySpan(x => x.Pow(power)).ToSegment();

        /// <summary>
        /// Rounds each element in the tensor to be either the lower or the upper parameter depending on its distance
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public virtual void RoundInPlace(INumericSegment<T> tensor, T lower, T upper) => tensor.ApplySpan(true, x => x.RoundInPlace(lower, upper));

        /// <summary>
        /// Returns a new tensor with only the supplied indices
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public virtual INumericSegment<T> CherryPickIndices(IReadOnlyNumericSegment<T> tensor, params uint[] indices) => tensor.ApplyReadOnlySpan(x => x.CherryPickIndices(indices)).ToSegment();

        /// <summary>
        /// In place L1 regularization of the tensor
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="coefficient"></param>
        public virtual void L1Regularisation(INumericSegment<T> segment, T coefficient) => segment.ApplySpan(true, x => x.L1Regularization(coefficient));

        /// <summary>
        /// Finds the distance between each pair of vectors
        /// </summary>
        /// <param name="vectors">First set of vectors</param>
        /// <param name="compareTo">Second set of vectors</param>
        /// <param name="distanceMetric">Distance metric</param>
        /// <returns>Matrix with the rows corresponding to the first set and columns corresponding to the second set and each element containing the distance</returns>
        public virtual IMatrix<T> FindDistances(IVector<T>[] vectors, IReadOnlyList<IVector<T>> compareTo, DistanceMetric distanceMetric)
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
        public virtual T Sum(IReadOnlyNumericSegment<T> tensor) => tensor.ApplyReadOnlySpan(x => x.Sum());

        /// <summary>
        /// Calculates the softmax of a set of tensors
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public virtual INumericSegment<T>[] MultiSoftmax(ArraySegment<IReadOnlyNumericSegment<T>> segments)
        {
            var len = segments.Count;
            var ret = new INumericSegment<T>[len];
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
        public virtual IMatrix<T>[] MultiSoftmaxDerivative(IReadOnlyNumericSegment<T>[] segments)
        {
            var len = segments.Length;
            var ret = new IMatrix<T>[len];
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
