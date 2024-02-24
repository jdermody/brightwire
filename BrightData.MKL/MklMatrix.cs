using BrightData.LinearAlgebra;
using MKLNET;

namespace BrightData.MKL
{
    /// <inheritdoc />
    /// <inheritdoc />
    public class MklMatrix(INumericSegment<float> data, uint rows, uint columns, MklLinearAlgebraProvider lap) 
        : MutableMatrix<MklLinearAlgebraProvider>(data, rows, columns, lap)
    {

        /// <inheritdoc />
        public override IMatrix Create(INumericSegment<float> segment) => new MklMatrix(segment, RowCount, ColumnCount, Lap);

        /// <inheritdoc />
        public override unsafe IMatrix Transpose()
        {
            var rows = (UIntPtr)RowCount;
            var cols = (UIntPtr)ColumnCount;

            var ret = lap.Copy(Segment, false, (size, a, r) => {
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
            return lap.CreateMatrix(ColumnCount, RowCount, ret);
        }
    }
}
