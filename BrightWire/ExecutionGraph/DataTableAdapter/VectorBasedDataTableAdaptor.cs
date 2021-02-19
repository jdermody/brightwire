using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Segment table adaptor for tables with vector data
    /// </summary>
    internal class VectorBasedDataTableAdapter : RowBasedDataTableAdapterBase
    {
        readonly uint[] _featureColumns;

        public VectorBasedDataTableAdapter(IRowOrientedDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            var firstRow = dataTable.Row(0);
            var input = (Vector<float>)firstRow[_featureColumnIndices.First()];
            var output = (Vector<float>)firstRow[_targetColumnIndex];

            InputSize = input.Size;
            OutputSize = output.Size;
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }

        public override IMiniBatch Get(uint[] rows)
        {
            var data = GetRows(rows)
                .Select(r => (_featureColumnIndices.Select(i => ((Vector<float>)r[i]).ToArray()).ToArray(), ((Vector<float>)r[_targetColumnIndex]).ToArray()))
                .ToArray()
            ;
            return GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new VectorBasedDataTableAdapter(dataTable, _featureColumns);
        }
    }
}
