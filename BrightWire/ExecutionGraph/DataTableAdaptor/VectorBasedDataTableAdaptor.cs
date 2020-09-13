using BrightTable;
using BrightWire.Models;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Segment table adaptor for tables with vector data
    /// </summary>
    class VectorBasedDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
	    public VectorBasedDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable) : base(lap, dataTable)
        {
            var firstRow = dataTable.Row(0);
            var input = (Vector<float>)firstRow[(uint)_dataColumnIndex.First()];
            var output = (Vector<float>)firstRow[_dataTargetIndex];

            InputSize = input.Size;
            OutputSize = output.Size;
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }
	    public override bool IsSequential => false;

        public override IMiniBatch Get(IExecutionContext executionContext, uint[] rows)
        {
            var data = _GetRows(rows)
                .Select(r => ((_dataColumnIndex.Select(i => ((Vector<float>)r[i]).Segment.ToArray()).ToArray(), ((Vector<float>)r[_dataTargetIndex]).Segment.ToArray())))
                .ToArray()
            ;
            return _GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new VectorBasedDataTableAdaptor(_lap, dataTable);
        }
    }
}
