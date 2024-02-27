using System;
using System.Diagnostics;
using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Row major 3D tensor
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    public class MutableTensor3D<LAP> : MutableTensorBase<ITensor3D, LAP>, ITensor3D
        where LAP: LinearAlgebraProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Rows in each matrix</param>
        /// <param name="columns">Columns in each matrix</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <exception cref="ArgumentException"></exception>
        public MutableTensor3D(INumericSegment<float> data, uint depth, uint rows, uint columns, LAP lap) : base(data, lap)
        {
            Depth = depth;
            RowCount = rows;
            ColumnCount = columns;
            MatrixSize = rows * columns;
            TotalSize = MatrixSize * depth;
#if  DEBUG
            if (data.Size != TotalSize)
                throw new ArgumentException("Sizes do not align");
#endif
        }

        /// <inheritdoc />
        public override ITensor3D Create(INumericSegment<float> segment) => new MutableTensor3D<LAP>(segment, Depth, RowCount, ColumnCount, Lap);

        /// <summary>
        /// Returns a read only matrix
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IReadOnlyMatrix GetMatrixAsReadOnly(uint index) => new ReadOnlyMatrix(Matrix(index), RowCount, ColumnCount);

        MutableTensorSegmentWrapper<float> Matrix(uint index) => new(Segment, index * MatrixSize, 1, MatrixSize);

        /// <inheritdoc />
        public uint Depth { get; private set; }

        /// <inheritdoc />
        public uint RowCount { get; private set; }

        /// <inheritdoc />
        public uint ColumnCount { get; private set; }

        /// <inheritdoc />
        public uint MatrixSize { get; private set; }

        /// <inheritdoc />
        public sealed override uint TotalSize { get; protected set; }

        /// <inheritdoc />
        public sealed override uint[] Shape
        {
            get => [ColumnCount, RowCount, Depth];
            protected set
            {
                ColumnCount = value[0];
                RowCount = value[1];
                Depth = value[2];
                MatrixSize = RowCount * ColumnCount;
                TotalSize = MatrixSize * Depth;
            }
        }

        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[int depth, int rowY, int columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[uint depth, uint rowY, uint columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        IReadOnlyMatrix IReadOnlyTensor3D.GetMatrix(uint index) => GetMatrixAsReadOnly(index);

        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[long depth, long rowY, long columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        /// <summary>
        /// Returns a value from the tensor
        /// </summary>
        /// <param name="depth">Matrix index</param>
        /// <param name="rowY">Row index</param>
        /// <param name="columnX">Column index</param>
        public float this[ulong depth, ulong rowY, ulong columnX]
        {
            get => Segment[depth * MatrixSize + columnX * RowCount + rowY];
            set => Segment[depth * MatrixSize + columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public virtual IMatrix GetMatrix(uint index)
        {
            var segment = new MutableTensorSegmentWrapper<float>(Segment, index * MatrixSize, 1, MatrixSize);
            return Lap.CreateMatrix(RowCount, ColumnCount, segment);
        }

        /// <inheritdoc />
        public virtual ITensor3D AddPadding(uint padding)
        {
            var newRows = RowCount + padding * 2;
            var newColumns = ColumnCount + padding * 2;
            var ret = Lap.CreateTensor3D(Depth, newRows, newColumns, true);

            for (uint k = 0; k < Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        if (i < padding || j < padding)
                            continue;
                        if (i >= newRows - padding || j >= newColumns - padding)
                            continue;
                        ret[k, j, i] = this[k, j - padding, i - padding];
                    }
                }
            }
            return ret;
        }

        /// <inheritdoc />
        public virtual ITensor3D RemovePadding(uint padding)
        {
            var newRows = RowCount - padding * 2;
            var newColumns = ColumnCount - padding * 2;
            var ret = Lap.CreateTensor3D(Depth, newRows, newColumns, true);
            for (uint k = 0; k < Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        ret[k, j, i] = this[k, j + padding, i + padding];
                    }
                }
            }

            return ret;
        }

        /// <inheritdoc />
        public virtual IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, xStride, yStride);
            var filterSize = filterWidth * filterHeight;
            var ret = Lap.CreateMatrix((uint)convolutions.Count, filterSize * Depth, (_, _) => 0f);

            for (var i = 0; i < convolutions.Count; i++) {
                var (offsetX, offsetY) = convolutions[i];
                for (uint k = 0; k < Depth; k++) {
                    var filterOffset = k * filterSize;
                    for (uint y = 0; y < filterHeight; y++) {
                        for (uint x = 0; x < filterWidth; x++) {
                            // write in column major format
                            var filterIndex = filterOffset + (x * filterHeight + y);
                            ret[(uint)i, filterIndex] = this[k, offsetY + y, offsetX + x];
                        }
                    }
                }
            }

            return ret;
        }

        /// <inheritdoc />
        public virtual ITensor3D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, xStride, yStride);
            using var temp = SpanOwner<IMatrix>.Allocate((int)outputDepth, AllocationMode.Default);
            var ptr = temp.Span;

            for (var i = 0; i < outputDepth; i++)
                ptr[i] = Lap.CreateMatrix(outputRows, outputColumns, true);
            for (uint k = 0; k < Depth; k++) {
                using var slice = GetMatrix(k);
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

            return Lap.CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual (ITensor3D Result, ITensor3D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var newColumns = (ColumnCount - filterWidth) / xStride + 1;
            var newRows = (RowCount - filterHeight) / yStride + 1;
            using var temp = SpanOwner<IMatrix>.Allocate((int)Depth, AllocationMode.Default);
            var ptr = temp.Span;
            var indexList = saveIndices ? new IMatrix[Depth] : null;
            var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, xStride, yStride);

            for (uint k = 0; k < Depth; k++) {
                var indices = saveIndices ? Lap.CreateMatrix(newRows, newColumns, true) : null;
                var layer = Lap.CreateMatrix(newRows, newColumns, true);

                foreach (var (cx, cy) in convolutions) {
                    var targetX = cx / xStride;
                    var targetY = cy / yStride;
                    var maxVal = float.MinValue;
                    var bestOffset = -1;
                    var offset = 0;

                    for (uint x = 0; x < filterWidth; x++) {
                        for (uint y = 0; y < filterHeight; y++) {
                            var val = this[k, cy + y, cx + x];
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
                Lap.CreateTensor3DAndThenDisposeInput(ptr),
                indexList != null ? Lap.CreateTensor3DAndThenDisposeInput(indexList) : null
            );
        }

        /// <inheritdoc />
        public virtual ITensor3D ReverseMaxPool(ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            using var temp = SpanOwner<IMatrix>.Allocate((int)Depth, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint k = 0; k < Depth; k++) {
                using var source = GetMatrix(k);
                var sourceRows = source.RowCount;
                var sourceColumns = source.ColumnCount;
                using var index = indices.GetMatrix(k);
                var target = ptr[(int)k] = Lap.CreateMatrix(outputRows, outputColumns, true);

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

            return Lap.CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual IMatrix AddAllMatrices()
        {
            var ret = Lap.CreateMatrix(RowCount, ColumnCount, true);

            for (uint i = 0; i < Depth; i++) {
                using var matrix = GetMatrix(i);
                ret.AddInPlace(matrix);
            }

            return ret;
        }

        /// <inheritdoc />
        public virtual ITensor3D MultiplyEachMatrixBy(IMatrix other)
        {
            using var temp = SpanOwner<IMatrix>.Allocate((int)Depth, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < Depth; i++) {
                using var matrix = GetMatrix(i);
                ptr[(int)i] = matrix.Multiply(other);
            }
            return Lap.CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual ITensor3D TransposeAndMultiplyEachMatrixBy(IMatrix other)
        {
            using var temp = SpanOwner<IMatrix>.Allocate((int)Depth, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < Depth; i++) {
                using var matrix = GetMatrix(i);
                ptr[(int)i] = matrix.TransposeThisAndMultiply(other);
            }
            return Lap.CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual void AddToEachRow(IVector vector)
        {
            for (uint k = 0; k < Depth; k++) {
                for (uint j = 0; j < ColumnCount; j++) {
                    for (uint i = 0; i < RowCount; i++)
                        this[k, i, j] += vector[j];
                }
            }
        }

        /// <inheritdoc />
        public virtual void AddToEachColumn(IVector vector)
        {
            for (uint k = 0; k < Depth; k++) {
                for (uint j = 0; j < ColumnCount; j++) {
                    for (uint i = 0; i < RowCount; i++)
                        this[k, i, j] += vector[i];
                }
            }
        }

        /// <inheritdoc />
        public virtual ITensor3D Multiply(ITensor4D other)
        {
            Debug.Assert(other.Count == Depth);
            using var temp = SpanOwner<IMatrix>.Allocate((int)other.Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < other.Count; i++) {
                using var item = other.GetTensor(i);
                using var multiplyWith = item.Reshape(null, other.Depth);
                using var slice = GetMatrix(i);
                var result = slice.Multiply(multiplyWith);
                ptr[(int)i] = result;
            }

            return Lap.CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual ITensor3D TransposeAndMultiply(ITensor4D other)
        {
            Debug.Assert(other.Count == Depth);
            using var temp = SpanOwner<IMatrix>.Allocate((int)other.Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < other.Count; i++) {
                using var item = other.GetTensor(i);
                using var multiplyWith = item.Reshape(null, other.Depth);
                using var slice = GetMatrix(i);
                var result = slice.TransposeAndMultiply(multiplyWith);
                ptr[(int)i] = result;
            }

            return Lap.CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public virtual ITensor3D TransposeThisAndMultiply(ITensor4D other)
        {
            Debug.Assert(other.Count == Depth);
            using var temp = SpanOwner<IMatrix>.Allocate((int)other.Count, AllocationMode.Default);
            var ptr = temp.Span;

            for (uint i = 0; i < other.Count; i++) {
                using var item = other.GetTensor(i);
                using var multiplyWith = item.Reshape(null, other.Depth);
                using var slice = GetMatrix(i);
                var result = slice.TransposeThisAndMultiply(multiplyWith);
                ptr[(int)i] = result;
            }

            return Lap.CreateTensor3DAndThenDisposeInput(ptr);
        }

        /// <inheritdoc />
        public override string ToString() => $"Tensor3D (Depth: {Depth}, Rows: {RowCount}, Columns: {ColumnCount})";

        /// <inheritdoc />
        public ITensor3D Create(LinearAlgebraProvider lap) => lap.CreateTensor3D(Depth, RowCount, ColumnCount, (IReadOnlyNumericSegment<float>)Segment);
    }

    /// <summary>
    /// 3D tensor 
    /// </summary>
    public class BrightTensor3D : MutableTensor3D<LinearAlgebraProvider>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Tensor segment</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Rows in each matrix</param>
        /// <param name="columns">Columns in each matrix</param>
        /// <param name="lap">Linear algebra provider</param>
        public BrightTensor3D(INumericSegment<float> data, uint depth, uint rows, uint columns, LinearAlgebraProvider lap) : base(data, depth, rows, columns, lap)
        {
        }
    }
}
