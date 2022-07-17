using System.Linq;
using System.Numerics;
using BrightData;
using BrightData.LinearAlgebra;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Segment table adapter for tables with vector data
    /// </summary>
    internal class VectorBasedDataTableAdapter : RowBasedDataTableAdapterBase
    {
        readonly uint[] _featureColumns;

        public VectorBasedDataTableAdapter(BrightDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            var firstRow = dataTable.GetRow(0);
            var input = (IReadOnlyVector)firstRow[_featureColumnIndices.Single()];
            var output = (IReadOnlyVector)firstRow[_targetColumnIndex];

            InputSize = input.Size;
            OutputSize = output.Size;
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }

        public override IMiniBatch Get(uint[] rows)
        {
            var featureColumnIndex = _featureColumnIndices.Single();
            var data = GetRows(rows)
                .Select(r => (((IReadOnlyVector)r[featureColumnIndex]).ToArray(), ((IReadOnlyVector)r[_targetColumnIndex]).ToArray()))
                .ToArray()
            ;
            return GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(BrightDataTable dataTable)
        {
            return new VectorBasedDataTableAdapter(dataTable, _featureColumns);
        }
    }
}
