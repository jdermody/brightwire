using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Row major matrix type
    /// </summary>
    /// <typeparam name="LAP"></typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="data">Tensor segment</param>
    /// <param name="rows">Number of rows</param>
    /// <param name="columns">Number of columns</param>
    /// <param name="lap">Linear algebra provider</param>
    public class MutableMatrix<LAP>(INumericSegment<float> data, uint rows, uint columns, LAP lap) : MutableTensorBase<IMatrix, LAP>(data, lap), IMatrix
        where LAP: LinearAlgebraProvider
    {
        /// <inheritdoc />
        public uint RowCount { get; private set; } = rows;

        /// <inheritdoc />
        public uint ColumnCount { get; private set; } = columns;

        /// <inheritdoc />
        public sealed override uint TotalSize { get; protected set; } = rows * columns;

        /// <inheritdoc />
        public sealed override uint[] Shape
        {
            get => [ColumnCount, RowCount];
            protected set
            {
                ColumnCount = value[0];
                RowCount = value[1];
                TotalSize = RowCount * ColumnCount;
            }
        }

        /// <inheritdoc cref="IMatrix" />
        public float this[int rowY, int columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc cref="IMatrix" />
        public float this[uint rowY, uint columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public float this[long rowY, long columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public float this[ulong rowY, ulong columnX]
        {
            get => Segment[columnX * RowCount + rowY];
            set => Segment[columnX * RowCount + rowY] = value;
        }

        /// <inheritdoc />
        public INumericSegment<float> GetRow(uint index)
        {
            if(index > RowCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of rows is {RowCount} but index {index} was requested");
            return new MutableTensorSegmentWrapper<float>(Segment, index, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> GetReadOnlyRow(uint index)
        {
            if(index > RowCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of rows is {RowCount} but index {index} was requested");
            return new ReadOnlyTensorSegmentWrapper<float>(Segment, index, RowCount, ColumnCount);
        }

        /// <inheritdoc />
        public INumericSegment<float> GetColumn(uint index)
        {
            if(index > ColumnCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of columns is {ColumnCount} but index {index} was requested");
            return new MutableTensorSegmentWrapper<float>(Segment, index * RowCount, 1, RowCount);
        }

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> GetReadOnlyColumn(uint index)
        {
            if(index > ColumnCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Number of columns is {ColumnCount} but index {index} was requested");
            return new ReadOnlyTensorSegmentWrapper<float>(Segment, index * RowCount, 1, RowCount);
        }

        /// <inheritdoc />
        public unsafe ReadOnlySpan<float> GetRowSpan(uint rowIndex, ref SpanOwner<float> temp)
        {
            temp = SpanOwner<float>.Allocate((int)TotalSize);
            var span = temp.Span;
            fixed (float* ptr = span) {
                Segment.CopyTo(ptr, (int)rowIndex * (int)RowCount, (int)RowCount, (int)ColumnCount);
            }
            return span;
        }

        /// <inheritdoc />
        public ReadOnlySpan<float> GetColumnSpan(uint columnIndex)
        {
            var ret = Segment.Contiguous!.ReadOnlySpan;
            return ret.Slice((int)(columnIndex * RowCount), (int)RowCount);
        }

        /// <inheritdoc />
        public override IMatrix Create(INumericSegment<float> segment) => Lap.CreateMatrix(RowCount, ColumnCount, segment);

        /// <inheritdoc />
        public virtual IVector GetRowVector(uint index) => Lap.CreateVector(GetRow(index));

        /// <inheritdoc />
        public virtual IVector GetColumnVector(uint index) => Lap.CreateVector(GetColumn(index));

        /// <inheritdoc />
        public virtual IMatrix Transpose()
        {
            var (buffer, rowCount, columnCount) = ReadOnlySegment.ApplyReadOnlySpan(x => x.Transpose(RowCount, ColumnCount));
            return new BrightMatrix(new ArrayPoolTensorSegment<float>(buffer), rowCount, columnCount, LinearAlgebraProvider);
        }

        IReadOnlyMatrix IReadOnlyMatrix.Transpose()
        {
            var (segment, rowCount, columnCount) = ReadOnlySegment.Transpose(RowCount, ColumnCount);
            return new ReadOnlyMatrix(segment, rowCount, columnCount);
        }

        /// <inheritdoc />
        public virtual IVector GetDiagonal()
        {
            if (RowCount != ColumnCount)
                throw new Exception("Diagonal can only be found from square matrices");
            return Lap.CreateVector(RowCount, i => this[i, i]);
        }

        /// <inheritdoc />
        public virtual IVector RowSums()
        {
            var rows = this.AllRowsAsReadOnly(false);
            return Lap.CreateVector(RowCount, i => rows[i].Sum());
        }

        /// <inheritdoc />
        public virtual IVector ColumnSums()
        {
            var columns = this.AllColumnsAsReadOnly(false);
            return Lap.CreateVector(ColumnCount, i => columns[i].Sum());
        }

        /// <inheritdoc />
        public virtual IVector Multiply(IVector vector)
        {
            using var temp = vector.Reshape(null, 1);
            using var temp2 = Multiply(temp);
            return temp2.Reshape();
        }

        /// <inheritdoc />
        public virtual IMatrix Multiply(IMatrix other)
        {
            if (ColumnCount != other.RowCount)
                throw new Exception("Matrix sizes do not agree");

            // transpose so that we can get contiguous vectors
            using var transposedThis = Transpose();
            return MultiplyWithThisTransposed(lap, transposedThis, other);
        }

        /// <inheritdoc />
        public virtual IMatrix TransposeAndMultiply(IMatrix other)
        {
            if (ColumnCount != other.ColumnCount)
                throw new Exception("Matrix sizes do not agree");
            using var transposed = other.Transpose();
            return Multiply(transposed);
        }

        /// <inheritdoc />
        public virtual IMatrix TransposeThisAndMultiply(IMatrix other)
        {
            if (RowCount != other.RowCount)
                throw new Exception("Matrix sizes do not agree");
            return MultiplyWithThisTransposed(lap, this, other);
        }

        /// <inheritdoc />
        public IMatrix MapIndexed(Func<uint, uint, float, float> mutator)
        {
            var ret = Segment.MapParallel((ind, val) => {
                var i = ind % RowCount;
                var j = ind / RowCount;
                return mutator(i, j, val);
            });
            return Lap.CreateMatrix(RowCount, ColumnCount, ret);
        }

        /// <inheritdoc />
        public void MapIndexedInPlace(Func<uint, uint, float, float> mutator)
        {
            var ret = Segment.MapParallel((ind, val) => {
                var i = ind % RowCount;
                var j = ind / RowCount;
                return mutator(i, j, val);
            });
            try {
                ret.CopyTo(Segment);
            }
            finally {
                ret.Release();
            }
        }

        /// <inheritdoc />
        public virtual (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex)
        {
            var ret1 = Lap.CreateMatrix(RowCount, columnIndex, (x, y) => this[x, y]);
            var ret2 = Lap.CreateMatrix(RowCount, ColumnCount - columnIndex, (x, y) => this[x, columnIndex + y]);
            return (ret1, ret2);
        }

        /// <inheritdoc />
        public virtual (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex)
        {
            var ret1 = Lap.CreateMatrix(rowIndex, ColumnCount, (x, y) => this[x, y]);
            var ret2 = Lap.CreateMatrix(RowCount - rowIndex, ColumnCount, (x, y) => this[rowIndex + x, y]);
            return (ret1, ret2);
        }

        /// <inheritdoc />
        public virtual IMatrix ConcatBelow(IMatrix bottom)
        {
            Debug.Assert(ColumnCount == bottom.ColumnCount);
            return Lap.CreateMatrix(RowCount + bottom.RowCount, ColumnCount, (x, y) => {
                var m = x >= RowCount ? bottom : this;
                return m[x >= RowCount ? x - RowCount : x, y];
            });
        }

        /// <inheritdoc />
        public virtual IMatrix ConcatRight(IMatrix right)
        {
            Debug.Assert(RowCount == right.RowCount);
            return Lap.CreateMatrix(RowCount, ColumnCount + right.ColumnCount, (x, y) => {
                var m = y >= ColumnCount ? right : this;
                return m[x, y >= ColumnCount ? y - ColumnCount : y];
            });
        }

        /// <inheritdoc />
        public virtual (IMatrix U, IVector S, IMatrix VT) Svd() => throw new NotImplementedException();

        /// <inheritdoc />
        public virtual IMatrix GetNewMatrixFromRows(IEnumerable<uint> rowIndices) => Lap.CreateMatrixFromRows(rowIndices.Select(GetReadOnlyRow).ToArray());

        /// <inheritdoc />
        public virtual IMatrix GetNewMatrixFromColumns(IEnumerable<uint> columnIndices) => Lap.CreateMatrixFromColumns(columnIndices.Select(GetReadOnlyColumn).ToArray());

        /// <inheritdoc />
        public virtual void AddToEachRow(IReadOnlyNumericSegment<float> segment) => MapIndexedInPlace((_, k, v) => v + segment[k]);

        /// <inheritdoc />
        public virtual void AddToEachColumn(IReadOnlyNumericSegment<float> segment) => MapIndexedInPlace((j, _, v) => v + segment[j]);

        /// <inheritdoc />
        public virtual void MultiplyEachRowWith(IReadOnlyNumericSegment<float> segment) => MapIndexedInPlace((_, k, v) => v * segment[k]);

        /// <inheritdoc />
        public virtual void MultiplyEachColumnWith(IReadOnlyNumericSegment<float> segment) => MapIndexedInPlace((j, _, v) => v * segment[j]);

        /// <inheritdoc />
        public virtual INumericSegment<float>[] SoftmaxPerRow()
        {
            using var segments = SpanOwner<IReadOnlyNumericSegment<float>>.Allocate((int)RowCount);
            var ptr = segments.Span;
            for (var i = 0; i < RowCount; i++)
                ptr[i] = GetRow((uint)i);
            return Lap.MultiSoftmax(segments.DangerousGetArray());
        }

        /// <inheritdoc />
        public virtual INumericSegment<float>[] SoftmaxDerivativePerRow(IReadOnlyNumericSegment<float>[] rows)
        {
            var derivatives = Lap.MultiSoftmaxDerivative(rows);
            using var transposed = Transpose();

            var ret = new INumericSegment<float>[RowCount];
            for (uint i = 0; i < RowCount; i++) {
                using var derivative = derivatives[i];
                using var row = transposed.GetColumnVector(i);
                using var sm = derivative.Multiply(row);
                ret[i] = sm.Segment;
                sm.Segment.AddRef();
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe IMatrix MultiplyWithThisTransposed(LinearAlgebraProvider lap, IMatrix transposedThis, IMatrix other)
        {
            var lda = (int)transposedThis.RowCount;
            var ldb = (int)other.RowCount;
            if (lda != ldb)
                throw new ArgumentException("Expected matrix rows to be the same");

            var rowCount = transposedThis.ColumnCount;
            var columnCount = other.ColumnCount;
            var ret = lap.CreateMatrix(rowCount, columnCount, false);

            SpanOwner<float> matrixTemp = SpanOwner<float>.Empty, otherTemp = SpanOwner<float>.Empty;
            var matrixSpan = transposedThis.Segment.GetSpan(ref matrixTemp, out var wasMatrixTempUsed);
            var otherSpan = other.Segment.GetSpan(ref otherTemp, out var wasOtherTempUsed);
            try {
                var retSpan = ret.Segment.Contiguous!.ReadOnlySpan;
                fixed (float* matrixPtr = matrixSpan)
                fixed (float* otherPtr = otherSpan)
                fixed (float* retPtr = retSpan) {
                    MatrixMultiplyChunked(matrixPtr, otherPtr, lda, rowCount, columnCount, retPtr);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void MatrixMultiplyChunked(float* a, float* b, int size, uint rows, uint cols, float* ret)
        {
            const int ChunkSize = 128;
            var vectorSize = Vector<float>.Count;
            var numVectors = size / vectorSize;
            var ceiling = numVectors * vectorSize;
            var totalSize = rows * cols;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]void Multiply(long startIndex)
            {
                for(long index = startIndex, len = Math.Min(startIndex + ChunkSize, totalSize); index < len; index++) {
                    var i = (uint)(index % rows);
                    var j = (uint)(index / rows);

                    var xPtr = &a[i * size];
                    var yPtr = &b[j * size];
                    var xVectors = (Vector<float>*)xPtr;
                    var yVectors = (Vector<float>*)yPtr;

                    var vSum = Vector<float>.Zero;
                    for (var z = 0; z < numVectors; z++)
                        vSum += xVectors[z] * yVectors[z];

                    var sum = Vector.Dot(vSum, Vector<float>.One);
                    for (var z = ceiling; z < size; z++)
                        sum += xPtr[z] * yPtr[z];
                    ret[j * rows + i] = sum;
                }
            }

            if (totalSize >= Consts.MinimumSizeForParallel)
                Parallel.For(0, totalSize / ChunkSize + 1, x => Multiply(x * ChunkSize));
            else {
                for (uint ind = 0; ind < totalSize; ind += ChunkSize)
                    Multiply(ind);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var preview = String.Join("|", Segment.Values.Take(Consts.DefaultPreviewSize));
            if (TotalSize > Consts.DefaultPreviewSize)
                preview += "|...";
            return $"Matrix (Rows: {RowCount}, Columns: {ColumnCount}) {preview}";
        }

        /// <inheritdoc />
        public IMatrix Create(LinearAlgebraProvider lap2) => lap2.CreateMatrix((IReadOnlyMatrix)this);
    }

    /// <summary>
    /// Matrix type
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="data">Tensor segment</param>
    /// <param name="rows">Number of rows</param>
    /// <param name="columns">Number of columns</param>
    /// <param name="lap">Linear algebra provider</param>
    public class BrightMatrix(INumericSegment<float> data, uint rows, uint columns, LinearAlgebraProvider lap) : MutableMatrix<LinearAlgebraProvider>(data, rows, columns, lap);
}
