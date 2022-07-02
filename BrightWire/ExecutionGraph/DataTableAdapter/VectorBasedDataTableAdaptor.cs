using System.Linq;
using System.Numerics;
using BrightData;
using BrightData.DataTable2;
using BrightData.DataTable2.TensorData;
using BrightData.LinearAlgebra;

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
            using var firstRow = dataTable.GetRow(0);
            var input = (IVectorInfo)firstRow[_featureColumnIndices.First()];
            var output = (IVectorInfo)firstRow[_targetColumnIndex];

            InputSize = input.Size;
            OutputSize = output.Size;
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }

        public override IMiniBatch Get(uint[] rows)
        {
            var data = GetRows(rows)
                .Select(r => (_featureColumnIndices.Select(i => ((IVectorInfo)r[i]).ToArray()).ToArray(), ((IVectorInfo)r[_targetColumnIndex]).ToArray()))
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
