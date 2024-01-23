using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    /// <inheritdoc />
    /// <inheritdoc />
    public class MklMatrix(INumericSegment<float> data, uint rows, uint columns, MklLinearAlgebraProvider computationUnit) 
        : MutableMatrix<MklLinearAlgebraProvider>(data, rows, columns, computationUnit)
    {

        /// <inheritdoc />
        public override IMatrix Create(INumericSegment<float> segment) => new MklMatrix(segment, RowCount, ColumnCount, Lap);
    }
}
