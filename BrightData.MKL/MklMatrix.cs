using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    /// <inheritdoc />
    public class MklMatrix : BrightMatrix<MklLinearAlgebraProvider>
    {
        /// <inheritdoc />
        public MklMatrix(INumericSegment<float> data, uint rows, uint columns, MklLinearAlgebraProvider computationUnit) : base(data, rows, columns, computationUnit)
        {
        }

        /// <inheritdoc />
        public override IMatrix Create(INumericSegment<float> segment) => new MklMatrix(segment, RowCount, ColumnCount, Lap);
    }
}
