using BrightData.LinearAlgebra;
using MKLNET;

namespace BrightData.MKL
{
    /// <inheritdoc />
    /// <inheritdoc />
    public unsafe class MklMatrix(INumericSegment<float> data, uint rows, uint columns, MklLinearAlgebraProvider lap) 
        : MutableMatrix<float, MklLinearAlgebraProvider>(data, rows, columns, lap)
    {

        /// <inheritdoc />
        public override IMatrix<float> Create(INumericSegment<float> segment) => new MklMatrix(segment, RowCount, ColumnCount, Lap);

        /// <inheritdoc />
        public override IMatrix<float> Transpose()
        {
            var rows = (UIntPtr)RowCount;
            var cols = (UIntPtr)ColumnCount;

            var ret = Lap.Copy(Segment, false, (size, a, r) => {
                var ap = new Span<float>(a, size);
                var rp = new Span<float>(r, size);
                ap.CopyTo(rp);
                Blas.imatcopy(
                    LayoutChar.ColMajor,
                    TransChar.Yes,
                    rows,
                    cols,
                    1f,
                    rp,
                    rows,
                    cols
                );
            });
            return Lap.CreateMatrix(ColumnCount, RowCount, ret);
        }

        /// <inheritdoc />
        public override IMatrix<float> Multiply(IMatrix<float> other)
        {
            int rowsA = (int)RowCount, 
                columnsARowsB = (int)ColumnCount, 
                columnsB = (int)other.ColumnCount
                ;
            var ret = Lap.ApplyWithNewSize(Segment, other.Segment, RowCount * other.ColumnCount, false, (a, b, r) => {
                Blas.Unsafe.gemm(
                    Layout.ColMajor,
                    Trans.No,
                    Trans.No,
                    rowsA,
                    columnsB,
                    columnsARowsB,
                    1f,
                    a,
                    rowsA,
                    b,
                    columnsARowsB,
                    0f,
                    r,
                    rowsA
                );
            });
            return Lap.CreateMatrix(RowCount, other.ColumnCount, ret);
        }

        /// <inheritdoc />
        public override IMatrix<float> TransposeThisAndMultiply(IMatrix<float> other)
        {
            int rowsA = (int)RowCount, 
                columnsA = (int)ColumnCount, 
                columnsB = (int)other.ColumnCount, 
                rowsB = (int)other.RowCount
                ;
            var ret = Lap.ApplyWithNewSize(Segment, other.Segment, ColumnCount * other.ColumnCount, false, (a, b, r) => {
                Blas.Unsafe.gemm(
                    Layout.ColMajor,
                    Trans.Yes,
                    Trans.No,
                    columnsA,
                    columnsB,
                    rowsB,
                    1f,
                    a,
                    rowsA,
                    b,
                    rowsB,
                    0f,
                    r,
                    columnsA
                );
            });
            return Lap.CreateMatrix(ColumnCount, other.ColumnCount, ret);
        }

        /// <inheritdoc />
        public override IMatrix<float> TransposeAndMultiply(IMatrix<float> other)
        {
            int rowsA = (int)RowCount, 
                columnsARowsB = (int)ColumnCount, 
                rowsB = (int)other.RowCount
                ;
            var ret = Lap.ApplyWithNewSize(Segment, other.Segment, RowCount * other.RowCount, false, (a, b, r) => {
                Blas.Unsafe.gemm(
                    Layout.ColMajor,
                    Trans.No,
                    Trans.Yes,
                    rowsA,
                    rowsB,
                    columnsARowsB,
                    1f,
                    a,
                    rowsA,
                    b,
                    rowsB,
                    0f,
                    r,
                    rowsA
                );
            });
            return Lap.CreateMatrix(RowCount, other.RowCount, ret);
        }

        /// <inheritdoc />
        public override (IMatrix<float> U, IVector<float> S, IMatrix<float> VT) Svd()
        {
            var rows = (int)RowCount;
            var cols = (int)ColumnCount;
            var mn = Math.Min(rows, cols);
            var buffer = Segment.GetLocalOrNewArray();

            var s = Lap.CreateSegment((uint)mn, false);
            var u = Lap.CreateSegment((uint)(rows * rows), false);
            var vt = Lap.CreateSegment((uint)(cols * cols), false);
            using var rWork = Lap.CreateSegment((uint)mn, false);

            var ret = Lapack.gesvd(
                Layout.ColMajor,
                'A',
                'A',
                rows,
                cols,
                buffer,
                rows,
                s.GetUnderlyingArray().Array!,
                u.GetUnderlyingArray().Array!,
                rows,
                vt.GetUnderlyingArray().Array!,
                cols,
                rWork.GetUnderlyingArray().Array!
            );
            if (ret != 0)
                throw new Exception("Failed to compute SVD");

            return (
                Lap.CreateMatrix((uint)rows, (uint)rows, u),
                Lap.CreateVector(s),
                Lap.CreateMatrix((uint)cols, (uint)cols, vt)
            );
        }
    }
}
