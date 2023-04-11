﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BrightData.Helper;
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
                foreach(var item in set.Keys)
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
        public virtual string ProviderName => "default";

        /// <summary>
        /// Type of vectors that will be created
        /// </summary>
        public virtual Type VectorType { get; } = typeof(BrightVector);

        /// <summary>
        /// Type of matrices that will be created
        /// </summary>
        public virtual Type MatrixType { get; } = typeof(BrightMatrix);

        /// <summary>
        /// Type of 3D tensors that will be created
        /// </summary>
        public virtual Type Tensor3DType { get; } = typeof(BrightTensor3D);

        /// <summary>
        /// Type of 4D tensors that will be created
        /// </summary>
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
                foreach(var item in set.Keys)
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
        public virtual ITensorSegment CreateSegment(params float[] data) => new ArrayBasedTensorSegment(data);

        /// <summary>
        /// Creates a tensor segment
        /// </summary>
        /// <param name="size">Segment size</param>
        /// <param name="initialiseToZero">True to initialize the all values in the segment to zero</param>
        /// <returns></returns>
        public virtual ITensorSegment CreateSegment(uint size, bool initialiseToZero) => new ArrayPoolTensorSegment(MemoryOwner<float>.Allocate((int)size, initialiseToZero ? AllocationMode.Clear : AllocationMode.Default));

        /// <summary>
        /// Creates a tensor segment
        /// </summary>
        /// <param name="size">Segment size</param>
        /// <param name="initializer">Function to initialize each value in the segment</param>
        /// <returns></returns>
        public virtual ITensorSegment CreateSegment(uint size, Func<uint /* index */, float> initializer)
        {
            var ret = MemoryOwner<float>.Allocate((int)size, AllocationMode.Clear);
            var ptr = ret.Span;
            for (var i = 0; i < ptr.Length; i++)
                ptr[i] = initializer((uint)i);
            return new ArrayPoolTensorSegment(ret);
        }

        /// <summary>
        /// Creates a clone of the tensor segment
        /// </summary>
        /// <param name="segment">Segment to clone</param>
        /// <returns></returns>
        public virtual ITensorSegment Clone(ITensorSegment segment)
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
        public virtual IVector CreateVector(ITensorSegment data) => new BrightVector(data, this);

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
        public IVector CreateVector(IReadOnlyVector vector) => vector.Create(this);

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
        public virtual IMatrix CreateMatrix(uint rowCount, uint columnCount, ITensorSegment data) => new BrightMatrix(data, rowCount, columnCount, this);

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
            var array = segment.GetArrayIfEasilyAvailable()!;
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
        public IMatrix CreateMatrix(IReadOnlyMatrix matrix) => matrix.Create(this);

        /// <summary>
        /// Creates a matrix from the rows supplied as vectors
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(IVector[] rows) => CreateMatrixFromRows(rows.Select(v => v.Segment).ToArray());

        /// <summary>
        /// Creates a matrix from the rows supplied as read only vectors
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(IReadOnlyVector[] rows) => CreateMatrixFromRows(rows.Select(v => v.Segment).ToArray());

        /// <summary>
        /// Creates a matrix from the rows supplied
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(IEnumerable<float[]> rows) => CreateMatrixFromRows(rows.ToArray().AsSpan());

        /// <summary>
        /// Creates a matrix from the rows supplied
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(float[][] rows) => CreateMatrixFromRows(rows.AsSpan());

        /// <summary>
        /// Creates a matrix from the rows supplied as vectors and then disposes each input vector
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRowsAndThenDisposeInput(IVector[] rows)
        {
            try {
                return CreateMatrixFromRows(rows.Select(v => v.Segment).ToArray());
            }
            finally {
                foreach(var row in rows)
                    row.Dispose();
            }
        }

        /// <summary>
        /// Creates a matrix from the rows supplied as tensor segments
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromRows(ITensorSegment[] rows) => CreateMatrixFromRows(rows.AsSpan());

        /// <summary>
        /// Creates a matrix from the rows supplied as tensor segments
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromRows(ReadOnlySpan<ITensorSegment> rows)
        {
            var columns = rows[0].Size;
            var ret = CreateMatrix((uint)rows.Length, columns, false);
            for(var i = 0; i < rows.Length; i++)
                rows[i].CopyTo(ret.Row((uint)i));
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
                var targetRow = ret.Row((uint)i);
                targetRow.CopyFrom(source, 0);
            }
            return ret;
        }

        /// <summary>
        /// Creates a matrix from the columns supplied as vectors
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(IVector[] columns) => CreateMatrixFromColumns(columns.Select(v => v.Segment).ToArray());

        /// <summary>
        /// Creates a matrix from the columns supplied as read only vectors
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(IReadOnlyVector[] columns) => CreateMatrixFromColumns(columns.Select(v => v.Segment).ToArray());

        /// <summary>
        /// Creates a matrix from the columns supplied
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(IEnumerable<float[]> columns) => CreateMatrixFromColumns(columns.ToArray().AsSpan());

        /// <summary>
        /// Creates a matrix from the columns supplied
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(float[][] columns) => CreateMatrixFromColumns(columns.AsSpan());

        /// <summary>
        /// Creates a matrix from the columns supplied as vectors and then disposes each input vector
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumnsAndThenDisposeInput(IVector[] columns)
        {
            try {
                return CreateMatrixFromColumns(columns.Select(v => v.Segment).ToArray());
            }
            finally {
                foreach (var column in columns)
                    column.Dispose();
            }
        }

        /// <summary>
        /// Creates a matrix from the columns supplied as tensor segments
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public IMatrix CreateMatrixFromColumns(ITensorSegment[] columns) => CreateMatrixFromColumns(columns.AsSpan());

        /// <summary>
        /// Creates a matrix from the columns supplied as tensor segments
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual IMatrix CreateMatrixFromColumns(ReadOnlySpan<ITensorSegment> columns)
        {
            var rows = columns[0].Size;
            var ret = CreateMatrix(rows, (uint)columns.Length, false);
            for(var i = 0; i < columns.Length; i++)
                columns[i].CopyTo(ret.Column((uint)i));
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
                var target = ret.Column((uint)i);
                target.CopyFrom(source, 0);
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
        public virtual ITensor3D CreateTensor3D(uint depth, uint rowCount, uint columnCount, ITensorSegment data) => new BrightTensor3D(data, depth, rowCount, columnCount, this);

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
        public ITensor3D CreateTensor3D(IReadOnlyTensor3D tensor) => CreateTensor3D(tensor.AllMatrices());

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
                foreach(var item in matrices)
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
                foreach(var item in matrices)
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
                    s.Segment.CopyTo(t.Segment);
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
        public virtual ITensor4D CreateTensor4D(uint count, uint depth, uint rowCount, uint columnCount, ITensorSegment data) => new BrightTensor4D(data, count, depth, rowCount, columnCount, this);

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
        public ITensor4D CreateTensor4D(IReadOnlyTensor4D tensor) => tensor.Create(this);

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
                foreach(var item in tensors)
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
                foreach(var item in tensors)
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
        public ITensor4D CreateTensor4D<T>(Span<T> tensors) where T: IReadOnlyTensor3D
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
                    s.Segment.CopyTo(t.Segment);
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
        protected static uint GetSize(ITensorSegment tensor, ITensorSegment tensor2)
        {
            if (tensor.Size != tensor2.Size)
                throw new Exception("Expected tensors to have same size");
            return tensor.Size;
        }

        /// <summary>
        /// Applies a mapping function to each value in the segment to create a new segment (potentially in parallel)
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper">Mapping function that receives each value from the segment</param>
        /// <returns></returns>
        public ITensorSegment MapParallel(ITensorSegment segment, Func<float /* value */, float /* new value */> mapper)
        {
            var size = segment.Size;
            var ret = CreateSegment(size, false);

            if (size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, (int)segment.Size, i => ret[i] = mapper(segment[i]));
            else {
                for (uint i = 0; i < size; i++)
                    ret[i] = mapper(segment[i]);
            }
            return ret;
        }

        /// <summary>
        /// Applies a mapping function to each value in the segment in place (potentially in parallel)
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper">Mapping function that receives each value from the segment</param>
        public static void MapParallelInPlace(ITensorSegment segment, Func<float /* value */, float /* new value */> mapper)
        {
            var size = segment.Size;

            if(size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, (int)segment.Size, i => segment[i] = mapper(segment[i]));
            else {
                for (uint i = 0; i < size; i++)
                    segment[i] = mapper(segment[i]);
            }
        }

        /// <summary>
        /// Applies a mapping function to each value in the segment to create a new segment (potentially in parallel)
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper">Mapping function that receives the index and each value from the segment</param>
        /// <returns></returns>
        public ITensorSegment MapParallel(ITensorSegment segment, Func<uint /* index */, float /* value */, float /* new value */> mapper)
        {
            var size = segment.Size;
            var ret = CreateSegment(size, false);
            
            if(size >= Consts.MinimumSizeForParallel)
                Parallel.For(0, (int)size, i => ret[i] = mapper((uint)i, segment[i]));
            else {
                for (uint i = 0; i < size; i++)
                    ret[i] = mapper(i, segment[i]);
            }
            return ret;
        }

        /// <summary>
        /// Applies a mapping function to each value in the segment in place (potentially in parallel)
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper">Mapping function that receives the index and each value from the segment</param>
        public void MapParallelInPlace(ITensorSegment segment, Func<uint /* index */, float /* value */, float /* new value */> mapper)
        {
            var ret = CreateSegment(segment.Size, false);
            try {
                Parallel.For(0, (int)segment.Size, i => ret[i] = mapper((uint)i, segment[i]));
                ret.CopyTo(segment);
            }
            finally {
                ret.Release();
            }
        }

        /// <summary>
        /// Creates a tensor from a tensor shape
        /// </summary>
        /// <param name="shape">Array containing the size of each dimension in the tensor</param>
        /// <param name="segment">Tensor segment</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public ITensor CreateTensor(uint[] shape, ITensorSegment segment)
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
        public virtual ITensorSegment Add(ITensorSegment tensor, ITensorSegment tensor2) => tensor.Add(tensor2);

        /// <summary>
        /// Adds two tensors into a new tensor and applies coefficients to each element in the two tensors
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="tensor2"></param>
        /// <param name="coefficient1"></param>
        /// <param name="coefficient2"></param>
        /// <returns></returns>
        public virtual ITensorSegment Add(ITensorSegment tensor, ITensorSegment tensor2, float coefficient1, float coefficient2) => tensor.Add(tensor2, coefficient1, coefficient2);

        /// <summary>
        /// Creates a new tensor from adding a scalar to each element in the tensor 
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public virtual ITensorSegment Add(ITensorSegment tensor, float scalar) => tensor.Add(scalar);

        /// <summary>
        /// Adds another tensor to the first tensor which will be modified in place
        /// </summary>
        /// <param name="target">First tensor</param>
        /// <param name="other">Other tensor</param>
        public virtual void AddInPlace(ITensorSegment target, ITensorSegment other) => target.AddInPlace(other);

        /// <summary>
        /// Adds another tensor to the first tensor and applies coefficients to each element in each tensor
        /// </summary>
        /// <param name="target">First tensor</param>
        /// <param name="other">Other tensor</param>
        /// <param name="coefficient1">Coefficient applied to each element of the first tensor</param>
        /// <param name="coefficient2">Coefficient applied to each element of the other tensor</param>
        public virtual void AddInPlace(ITensorSegment target, ITensorSegment other, float coefficient1, float coefficient2) => target.AddInPlace(other, coefficient1, coefficient2);

        /// <summary>
        /// Adds a scalar to each element of this tensor - modified in place
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scalar"></param>
        public virtual void AddInPlace(ITensorSegment target, float scalar) => target.AddInPlace(scalar);

        /// <summary>
        /// Multiplies each element of the tensor by a scalar - modified in place
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scalar"></param>
        public virtual void MultiplyInPlace(ITensorSegment target, float scalar) => target.MultiplyInPlace(scalar);

        /// <summary>
        /// Creates a new tensor by multiplying each element of the tensor with a scalar
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public virtual ITensorSegment Multiply(ITensorSegment target, float scalar) => target.Multiply(scalar);

        /// <summary>
        /// Subtracts the second tensor from the first tensor into a new tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual ITensorSegment Subtract(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.Subtract(tensor2);

        /// <summary>
        /// Subtracts the second tensor from the first tensor into a new tensor and applies coefficients to each value in each tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <param name="coefficient1">Coefficient to apply to each element in the first tensor</param>
        /// <param name="coefficient2">Coefficient to apply to each element in the second tensor</param>
        /// <returns></returns>
        public virtual ITensorSegment Subtract(ITensorSegment tensor1, ITensorSegment tensor2, float coefficient1, float coefficient2) => tensor1.Subtract(tensor2, coefficient1, coefficient2);

        /// <summary>
        /// Subtracts the second tensor from the first tensor - first tensor modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        public virtual void SubtractInPlace(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.SubtractInPlace(tensor2);

        /// <summary>
        /// Subtracts the second tensor from the first tensor and applies coefficient to each value in the tensors - first tensor modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <param name="coefficient1">Coefficient to apply to each element in the first tensor</param>
        /// <param name="coefficient2">Coefficient to apply to each element in the second tensor</param>
        public virtual void SubtractInPlace(ITensorSegment tensor1, ITensorSegment tensor2, float coefficient1, float coefficient2) => tensor1.SubtractInPlace(tensor2, coefficient1, coefficient2);

        /// <summary>
        /// Creates a new tensor by multiplying each element in the first tensor with the corresponding value in the second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual ITensorSegment PointwiseMultiply(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.PointwiseMultiply(tensor2);

        /// <summary>
        /// Multiplies each element in the first tensor with the corresponding value in the second tensor - first tensor modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        public virtual void PointwiseMultiplyInPlace(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.PointwiseMultiplyInPlace(tensor2);
        
        /// <summary>
        /// Creates a new tensor by dividing each element in the first tensor with the corresponding value in the second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual ITensorSegment PointwiseDivide(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.PointwiseDivide(tensor2);

        /// <summary>
        /// Dividing each element in the first tensor with the corresponding value in the second tensor - first tensor is modified in place
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        public virtual void PointwiseDivideInPlace(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.PointwiseDivideInPlace(tensor2);

        /// <summary>
        /// Calculates the dot product of the first with the second tensor
        /// </summary>
        /// <param name="tensor1"></param>
        /// <param name="tensor2"></param>
        /// <returns></returns>
        public virtual float DotProduct(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.DotProduct(tensor2);

        /// <summary>
        /// Creates a new tensor that contains the square root of each value in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="adjustment">A small value to add to each value in case of zeros</param>
        /// <returns></returns>
        public virtual ITensorSegment Sqrt(ITensorSegment tensor, float adjustment = FloatMath.AlmostZero) => tensor.Sqrt(adjustment);

        /// <summary>
        /// Finds the index (if any) of the first element with a matching value
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="value"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public virtual uint? Search(ITensorSegment tensor, float value, float tolerance = FloatMath.AlmostZero) => tensor.Search(value, tolerance);

        /// <summary>
        /// Constrains each value in this tensor to lie between the min and max values (if supplied)
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public virtual void ConstrainInPlace(ITensorSegment tensor, float? minValue, float? maxValue) => tensor.ConstrainInPlace(minValue, maxValue);

        /// <summary>
        /// Finds the average of the values in the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float Average(ITensorSegment tensor) => tensor.Average();

        /// <summary>
        /// Finds the L1 norm (manhattan distance) of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float L1Norm(ITensorSegment tensor) => tensor.L1Norm();

        /// <summary>
        /// Finds the L2 norm (euclidean distance) of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float L2Norm(ITensorSegment tensor) => tensor.L2Norm();

        /// <summary>
        /// Finds the minimum and maximum values of the tensor (and their corresponding indices)
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment tensor) => tensor.GetMinAndMaxValues();

        /// <summary>
        /// Finds the index of the minimum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual uint GetMinIndex(ITensorSegment tensor) => GetMinAndMaxValues(tensor).MinIndex;

        /// <summary>
        /// Finds the index of the maximum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual uint GetMaxIndex(ITensorSegment tensor) => GetMinAndMaxValues(tensor).MaxIndex;

        /// <summary>
        /// Finds the minimum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float GetMin(ITensorSegment tensor) => GetMinAndMaxValues(tensor).Min;

        /// <summary>
        /// Finds the maximum value of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual float GetMax(ITensorSegment tensor) => GetMinAndMaxValues(tensor).Max;

        /// <summary>
        /// Checks if the tensor is entirely finite (not NaN or Infinity)
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual bool IsEntirelyFinite(ITensorSegment tensor) => tensor.IsEntirelyFinite();

        /// <summary>
        /// Reverse the order of the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment Reverse(ITensorSegment tensor) => tensor.Reverse();

        /// <summary>
        /// Splits the tensor into separate (contiguous) sub tensors
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="blockCount">Number of blocks to split into</param>
        /// <returns></returns>
        public virtual IEnumerable<ITensorSegment> Split(ITensorSegment tensor, uint blockCount) => tensor.Split(blockCount);

        /// <summary>
        /// Finds the cosine distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float CosineDistance(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.CosineDistance(tensor2);

        /// <summary>
        /// Finds the euclidean distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float EuclideanDistance(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.EuclideanDistance(tensor2);

        /// <summary>
        /// Finds the mean squared distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float MeanSquaredDistance(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.MeanSquaredDistance(tensor2);

        /// <summary>
        /// Finds the squared euclidean distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float SquaredEuclideanDistance(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.SquaredEuclideanDistance(tensor2);

        /// <summary>
        /// Finds the manhattan distance between the first and second tensor
        /// </summary>
        /// <param name="tensor1">First tensor</param>
        /// <param name="tensor2">Second tensor</param>
        /// <returns></returns>
        public virtual float ManhattanDistance(ITensorSegment tensor1, ITensorSegment tensor2) => tensor1.ManhattanDistance(tensor2);

        /// <summary>
        /// Creates a new tensor that contains the absolute value of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment Abs(ITensorSegment tensor) => tensor.Abs();

        /// <summary>
        /// Creates a new tensor that contains the natural logarithm of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment Log(ITensorSegment tensor) => tensor.Log();

        /// <summary>
        /// Creates a new tensor that contains the exponent of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment Exp(ITensorSegment tensor) => tensor.Exp();

        /// <summary>
        /// Creates a new tensor that contains the square of each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment Squared(ITensorSegment tensor) => tensor.Squared();

        /// <summary>
        /// Finds the standard deviation of each element in the tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="mean">Pre calculated mean of the tensor or null to calculate</param>
        /// <returns></returns>
        public virtual float StdDev(ITensorSegment tensor, float? mean) => tensor.StdDev(mean);

        /// <summary>
        /// Creates a new tensor that contains the sigmoid function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment Sigmoid(ITensorSegment tensor) => tensor.Sigmoid();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the sigmoid function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment SigmoidDerivative(ITensorSegment tensor) => tensor.SigmoidDerivative();

        /// <summary>
        /// Creates a new tensor that contains the tanh function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment Tanh(ITensorSegment tensor) => tensor.Tanh();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the tanh function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment TanhDerivative(ITensorSegment tensor) => tensor.TanhDerivative();

        /// <summary>
        /// Creates a new tensor that contains the relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment Relu(ITensorSegment tensor) => tensor.Relu();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment ReluDerivative(ITensorSegment tensor) => tensor.ReluDerivative();

        /// <summary>
        /// Creates a new tensor that contains the leaky relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment LeakyRelu(ITensorSegment tensor) => tensor.LeakyRelu();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the leaky relu function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment LeakyReluDerivative(ITensorSegment tensor) => tensor.LeakyReluDerivative();

        /// <summary>
        /// Creates a new tensor that contains the softmax function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual ITensorSegment Softmax(ITensorSegment tensor) => tensor.Softmax();

        /// <summary>
        /// Creates a new tensor that contains the derivative of the softmax function applied to each element in this tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual IMatrix SoftmaxDerivative(ITensorSegment tensor) => tensor.SoftmaxDerivative(this);

        /// <summary>
        /// Creates a new tensor that each element of the tensor raised to the specified power
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public virtual ITensorSegment Pow(ITensorSegment tensor, float power) => tensor.Pow(power);

        /// <summary>
        /// Rounds each element in the tensor to be either the lower or the upper parameter depending on its distance
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public virtual void RoundInPlace(ITensorSegment tensor, float lower, float upper) => tensor.RoundInPlace(lower, upper);

        /// <summary>
        /// Returns a new tensor with only the supplied indices
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public virtual ITensorSegment CherryPickIndices(ITensorSegment tensor, params uint[] indices) => tensor.CherryPickIndices(indices);

        /// <summary>
        /// Calculate the matrix transpose
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public virtual unsafe IMatrix Transpose(IMatrix matrix)
        {
            var columnCount = matrix.ColumnCount;
            var rowCount = matrix.RowCount;
            var ret = CreateMatrix(columnCount, rowCount, false);
            var temp = SpanOwner<float>.Empty;
            fixed (float* matrixPtr = &MemoryMarshal.GetReference(matrix.Segment.GetSpan(ref temp, out var wasTempUsed)))
            fixed (float* retPtr = &MemoryMarshal.GetReference(ret.Segment.GetSpan())) {
                CacheTranspose(matrixPtr, matrix.RowCount, matrix.ColumnCount, 0, matrix.RowCount, 0, matrix.ColumnCount, retPtr);
                if (wasTempUsed)
                    temp.Dispose();
            }
            return ret;
        }

        static unsafe void CacheTranspose(float* from, uint rows, uint columns, uint rb, uint re, uint cb, uint ce, float* to) {
            uint r = re - rb, c = ce - cb;
            if (r <= 16 && c <= 16) {
                for (var i = rb; i < re; i++) {
                    for (var j = cb; j < ce; j++) {
                        to[i * columns + j] = from[j * rows + i];
                    }
                }
            } else if (r >= c) {
                CacheTranspose(from, rows, columns, rb, rb + (r / 2), cb, ce, to);
                CacheTranspose(from, rows, columns, rb + (r / 2), re, cb, ce, to);
            } else {
                CacheTranspose(from, rows, columns, rb, re, cb, cb + (c / 2), to);
                CacheTranspose(from, rows, columns, rb, re, cb + (c / 2), ce, to);
            }
        }

        /// <summary>
        /// Multiplies the matrix with a vector
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public virtual IVector Multiply(IMatrix matrix, IVector vector)
        {
            using var temp = vector.Reshape(null, 1);
            using var temp2 = Multiply(matrix, temp);
            return temp2.Reshape();
        }

        /// <summary>
        /// Matrix multiplication
        /// </summary>
        /// <param name="matrix1"></param>
        /// <param name="matrix2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual IMatrix Multiply(IMatrix matrix1, IMatrix matrix2)
        {
            if (matrix1.ColumnCount != matrix2.RowCount)
                throw new Exception("Matrix sizes do not agree");

            // transpose so that we can can get contiguous vectors
            using var transposedThis = Transpose(matrix1);
            return MultiplyWithThisTransposed(transposedThis, matrix2);
            //return OldMultiply(matrix, other);
        }

        unsafe IMatrix MultiplyWithThisTransposed(IMatrix transposedThis, IMatrix other)
        {
            var size = (int)other.RowCount;
            var vectorSize = ExtensionMethods.NumericsVectorSize;
            var numVectors = size / vectorSize;
            var ceiling = numVectors * vectorSize;

            var rowCount = transposedThis.ColumnCount;
            var columnCount = other.ColumnCount;
            var ret = CreateMatrix(rowCount, columnCount, false);

            SpanOwner<float> matrixTemp = SpanOwner<float>.Empty, otherTemp = SpanOwner<float>.Empty;
            var matrixSpan = transposedThis.Segment.GetSpan(ref matrixTemp, out var wasMatrixTempUsed);
            var otherSpan = other.Segment.GetSpan(ref otherTemp, out var wasOtherTempUsed);
            try {
                var retSpan = ret.Segment.GetSpan();
                var lda = (int)transposedThis.RowCount;
                var ldb = (int)other.RowCount;
                fixed (float* matrixPtr = &MemoryMarshal.GetReference(matrixSpan))
                fixed (float* otherPtr = &MemoryMarshal.GetReference(otherSpan))
                fixed (float* retPtr = &MemoryMarshal.GetReference(retSpan)) {
                    var matrixPtr2 = matrixPtr;
                    var otherPtr2 = otherPtr;
                    var retPtr2 = retPtr;
                    var totalSize = rowCount * columnCount;
                    if (totalSize >= Consts.MinimumSizeForParallel) {
                        Parallel.For(0, totalSize, ind => {
                            var i = (uint)(ind % rowCount);
                            var j = (uint)(ind / rowCount);

                            var xPtr = &matrixPtr2[i * lda];
                            var xSpan = new ReadOnlySpan<float>(xPtr, lda);
                            var xVectors = MemoryMarshal.Cast<float, Vector<float>>(xSpan);

                            var yPtr = &otherPtr2[j * ldb];
                            var ySpan = new ReadOnlySpan<float>(yPtr, ldb);
                            var yVectors = MemoryMarshal.Cast<float, Vector<float>>(ySpan);

                            var sum = 0f;
                            for (var z = 0; z < numVectors; z++) {
                                var temp = Vector.Multiply(xVectors[z], yVectors[z]);
                                sum += Vector.Sum(temp);
                            }

                            for (var z = ceiling; z < size; z++)
                                sum += xSpan[z] * ySpan[z];
                            retPtr2[j * rowCount + i] = sum;
                        });
                    }
                    else {
                        for (uint ind = 0; ind < totalSize; ind++) {
                            var i = ind % rowCount;
                            var j = ind / rowCount;

                            var xPtr = &matrixPtr2[i * lda];
                            var xSpan = new ReadOnlySpan<float>(xPtr, lda);
                            var xVectors = MemoryMarshal.Cast<float, Vector<float>>(xSpan);

                            var yPtr = &otherPtr2[j * ldb];
                            var ySpan = new ReadOnlySpan<float>(yPtr, ldb);
                            var yVectors = MemoryMarshal.Cast<float, Vector<float>>(ySpan);

                            var sum = 0f;
                            for (var z = 0; z < numVectors; z++) {
                                var temp = Vector.Multiply(xVectors[z], yVectors[z]);
                                sum += Vector.Sum(temp);
                            }

                            for (var z = ceiling; z < size; z++)
                                sum += xSpan[z] * ySpan[z];
                            retPtr2[j * rowCount + i] = sum;
                        }
                    }
                }
            }
            finally {
                if(wasMatrixTempUsed)
                    matrixTemp.Dispose();
                if(wasOtherTempUsed)
                    otherTemp.Dispose();
            }

            return ret;
        }

        /// <summary>
        /// Transposes the second matrix and then multiplies the first with the second matrix
        /// </summary>
        /// <param name="matrix1"></param>
        /// <param name="matrix2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual IMatrix TransposeSecondAndMultiply(IMatrix matrix1, IMatrix matrix2)
        {
            if (matrix1.ColumnCount != matrix2.ColumnCount)
                throw new Exception("Matrix sizes do not agree");
            using var transposed = matrix2.Transpose();
            return Multiply(matrix1, transposed);
        }

        /// <summary>
        /// Transposes the first matrix and then multiplies with the second matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual IMatrix TransposeFirstAndMultiply(IMatrix matrix, IMatrix other)
        {
            if (matrix.RowCount != other.RowCount)
                throw new Exception("Matrix sizes do not agree");
            return MultiplyWithThisTransposed(matrix, other);
        }

        /// <summary>
        /// Returns the matrix diagonal
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual IVector GetDiagonal(IMatrix matrix)
        {
            if(matrix.RowCount != matrix.ColumnCount)
                throw new Exception("Diagonal can only be found from square matrices");
            return CreateVector(matrix.RowCount, i => matrix[i, i]);
        }

        /// <summary>
        /// Returns the sum of each row
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public virtual IVector RowSums(IMatrix matrix)
        {
            var rows = matrix.AllRows(false);
            return CreateVector(matrix.RowCount, i => rows[i].Segment.Sum());
        }

        /// <summary>
        /// Returns the sum of each column
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public virtual IVector ColumnSums(IMatrix matrix)
        {
            var columns = matrix.AllColumns(false);
            return CreateVector(matrix.ColumnCount, i => columns[i].Segment.Sum());
        }

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
            return ret ?? CreateVector(tensor.ColumnCount, true);
        }

        /// <summary>
        /// Returns the sum of each matrix row
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        public virtual IVector RowSums(ITensor4D tensor)
        {
            IVector? ret = null;
            for (uint i = 0, count = tensor.Count; i < count; i++) {
                using var subTensor = tensor.GetTensor(i);
                using var tensorAsMatrix = subTensor.Reshape(subTensor.RowCount * subTensor.ColumnCount, subTensor.Depth);
                var rowSums = tensorAsMatrix.RowSums();
                if (ret == null)
                    ret = rowSums;
                else {
                    ret.AddInPlace(rowSums);
                    rowSums.Dispose();
                }
            }
            return ret ?? CreateVector(tensor.ColumnCount, true);
        }

        /// <summary>
        /// Calculates the singular value decomposition of the matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual (IMatrix U, IVector S, IMatrix VT) Svd(IMatrix matrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// In place L1 regularization of the tensor
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="coefficient"></param>
        public virtual void L1Regularisation(ITensorSegment segment, float coefficient) => segment.L1Regularization(coefficient);

        /// <summary>
        /// Splits the matrix at the specified column index into two sub matrices
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public virtual (IMatrix Left, IMatrix Right) SplitAtColumn(IMatrix matrix, uint columnIndex)
        {
            var ret1 = CreateMatrix(matrix.RowCount, columnIndex, (x, y) => matrix[x, y]);
            var ret2 = CreateMatrix(matrix.RowCount, matrix.ColumnCount - columnIndex, (x, y) => matrix[x, columnIndex + y]);
            return (ret1, ret2);
        }

        /// <summary>
        /// Splits the matrix at the specified row index into two sub matrices
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public virtual (IMatrix Top, IMatrix Bottom) SplitAtRow(IMatrix matrix, uint rowIndex)
        {
            var ret1 = CreateMatrix(rowIndex, matrix.ColumnCount, (x, y) => matrix[x, y]);
            var ret2 = CreateMatrix(matrix.RowCount - rowIndex, matrix.ColumnCount, (x, y) => matrix[rowIndex + x, y]);
            return (ret1, ret2);
        }

        /// <summary>
        /// Concatenates the two matrices (column count must agree)
        /// </summary>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public virtual IMatrix ConcatColumns(IMatrix top, IMatrix bottom)
        {
            Debug.Assert(top.ColumnCount == bottom.ColumnCount);
            return CreateMatrix(top.RowCount + bottom.RowCount, top.ColumnCount, (x, y) => {
                var m = x >= top.RowCount ? bottom : top;
                return m[x >= top.RowCount ? x - top.RowCount : x, y];
            });
        }

        /// <summary>
        /// Concatenates the two matrices (row count must agree)
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public virtual IMatrix ConcatRows(IMatrix left, IMatrix right)
        {
            Debug.Assert(left.RowCount == right.RowCount);
            return CreateMatrix(left.RowCount, left.ColumnCount + right.ColumnCount, (x, y) => {
                var m = y >= left.ColumnCount ? right : left;
                return m[x, y >= left.ColumnCount ? y - left.ColumnCount : y];
            });
        }

        /// <summary>
        /// Applies zero padding to the each matrix in the tensor
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

            for(int i = 0; i < convolutions.Count; i++) {
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
                var filters = filter.GetColumn(k).Segment.Split(outputDepth).ToArray();

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
        /// Adds padding to each sub matrix
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public virtual ITensor4D AddPadding(ITensor4D tensor, uint padding)
        {
            using var temp = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                ptr[(int)i] = subTensor.AddPadding(padding);
            }

            return CreateTensor4DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Removes padding from each sub matrix
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public virtual ITensor4D RemovePadding(ITensor4D tensor, uint padding)
        {
            using var temp = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                ptr[(int)i] = subTensor.RemovePadding(padding);
            }

            return CreateTensor4DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Applies a max pooling operation to the 4D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <param name="saveIndices"></param>
        /// <returns></returns>
        public virtual (ITensor4D Result, ITensor4D? Indices) MaxPool(ITensor4D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var indexList = saveIndices 
                ? new ITensor3D[tensor.Count]
                : null;
            using var temp = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Default);
            var ptr = temp.Span;
            
            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                var (result, indices) = subTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, saveIndices);
                ptr[(int)i] = result;
                if (indexList != null && indices != null)
                    indexList[i] = indices;
            }

            return (CreateTensor4DAndThenDisposeInput(ptr), indexList != null ? CreateTensor4DAndThenDisposeInput(indexList) : null);
        }

        /// <summary>
        /// Reverses a max pooling operation
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
        public virtual ITensor4D ReverseMaxPool(ITensor4D tensor, ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var temp = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                using var indexTensor = indices.GetTensor(i);
                var result = subTensor.ReverseMaxPool(indexTensor, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
                ptr[(int)i] = result;
            }

            return CreateTensor4DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Convolutional operation applied to each sub tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="xStride"></param>
        /// <param name="yStride"></param>
        /// <returns></returns>
        public virtual ITensor3D Im2Col(ITensor4D tensor, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var temp = SpanOwner<IMatrix>.Allocate((int)tensor.Count, AllocationMode.Default);
            var ptr = temp.Span;
            
            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                ptr[(int)i] = subTensor.Im2Col(filterWidth, filterHeight, xStride, yStride);
            }

            return CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Reverses the convolutional operator on each sub tensor
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
        public virtual ITensor4D ReverseIm2Col(ITensor4D tensor, IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var temp = SpanOwner<ITensor3D>.Allocate((int)tensor.Count, AllocationMode.Default);
            var ptr = temp.Span;
            
            for (uint i = 0; i < tensor.Count; i++) {
                using var subTensor = tensor.GetTensor(i);
                ptr[(int)i] = subTensor.ReverseIm2Col(filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
            }

            return CreateTensor4DAndThenDisposeInput(ptr);
        }

        /// <summary>
        /// Returns a sub matrix from the 3D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual IMatrix GetMatrix(ITensor3D tensor, uint index)
        {
            var segment = new TensorSegmentWrapper(tensor.Segment, index * tensor.MatrixSize, 1, tensor.MatrixSize);
            return CreateMatrix(tensor.RowCount, tensor.ColumnCount, segment);
        }

        /// <summary>
        /// Returns a sub tensor from the 4D tensor
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual ITensor3D GetTensor(ITensor4D tensor, uint index)
        {
            var segment = new TensorSegmentWrapper(tensor.Segment, index * tensor.TensorSize, 1, tensor.TensorSize);
            return CreateTensor3D(tensor.Depth, tensor.RowCount, tensor.ColumnCount, segment);
        }

        /// <summary>
        /// Returns a new matrix from the specified row indices
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="rowIndices"></param>
        /// <returns></returns>
        public virtual IMatrix GetNewMatrixFromRows(IMatrix matrix, IEnumerable<uint> rowIndices)
        {
            var ret = CreateMatrixFromRows(rowIndices.Select(matrix.GetRow).ToArray());
            return ret;
        }

        /// <summary>
        /// Returns a new matrix from the specified column indices
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public virtual IMatrix GetNewMatrixFromColumns(IMatrix matrix, IEnumerable<uint> columnIndices)
        {
            var ret = CreateMatrixFromColumns(columnIndices.Select(matrix.GetColumn).ToArray());
            return ret;
        }

        /// <summary>
        /// Adds a tensor to each row of the matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="segment"></param>
        public virtual void AddToEachRow(IMatrix matrix, ITensorSegment segment)
        {
            matrix.MapIndexedInPlace((_, k, v) => v + segment[k]);
        }

        /// <summary>
        /// Adds a tensor to each column of the matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="segment"></param>
        public virtual void AddToEachColumn(IMatrix matrix, ITensorSegment segment)
        {
            matrix.MapIndexedInPlace((j, _, v) => v + segment[j]);
        }

        /// <summary>
        /// Multiplies each row of the matrix with the tensor
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="segment"></param>
        public virtual void MultiplyEachRowWith(IMatrix matrix, ITensorSegment segment)
        {
            matrix.MapIndexedInPlace((_, k, v) => v * segment[k]);
        }

        /// <summary>
        /// Multiplies each column of the matrix with the tensor
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="segment"></param>
        public virtual void MultiplyEachColumnWith(IMatrix matrix, ITensorSegment segment)
        {
            matrix.MapIndexedInPlace((j, _, v) => v * segment[j]);
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
        /// <param name="segment"></param>
        /// <returns></returns>
        public virtual float Sum(ITensorSegment segment) => segment.Sum();

        /// <summary>
        /// Calculates the softmax of a set of tensors
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public virtual ITensorSegment[] MultiSoftmax(ArraySegment<ITensorSegment> segments)
        {
            var len = segments.Count;
            var ret = new ITensorSegment[len];
            if (len >= Consts.MinimumSizeForParallel)
                Parallel.For(0, len, i => ret[i] = Softmax(segments[i]));
            else {
                for(var i = 0; i < len; i++)
                    ret[i] = Softmax(segments[i]);
            }
            return ret;
        }
        
        /// <summary>
        /// Calculates the softmax derivative of a set of tensors
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public virtual IMatrix[] MultiSoftmaxDerivative(ITensorSegment[] segments)
        {
            var len = segments.Length;
            var ret = new IMatrix[len];
            if (len >= Consts.MinimumSizeForParallel)
                Parallel.For(0, len, i => ret[i] = SoftmaxDerivative(segments[i]));
            else {
                for(var i = 0; i < len; i++)
                    ret[i] = SoftmaxDerivative(segments[i]);
            }
            return ret;
        }

        /// <summary>
        /// Computes the per row softmax derivative
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public virtual ITensorSegment[] SoftmaxDerivativePerRow(IMatrix matrix, ITensorSegment[] rows)
        {
            var derivatives = MultiSoftmaxDerivative(rows);
            using var transposed = matrix.Transpose();

            var ret = new ITensorSegment[matrix.RowCount];
            for (uint i = 0; i < matrix.RowCount; i++) {
                using var derivative = derivatives[i];
                using var row = transposed.GetColumnVector(i);
                using var sm = derivative.Multiply(row);
                ret[i] = sm.Segment;
                sm.Segment.AddRef();
            }

            return ret;
        }
    }
}
