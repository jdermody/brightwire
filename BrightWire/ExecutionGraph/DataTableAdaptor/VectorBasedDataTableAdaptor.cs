using BrightTable;
using BrightWire.Models;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Data table adaptor for tables with vector data
    /// </summary>
    class VectorBasedDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
	    public VectorBasedDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable) : base(lap, dataTable)
        {
            var firstRow = dataTable.Row(0);
            var input = (FloatVector)firstRow[(uint)_dataColumnIndex.First()];
            var output = (FloatVector)firstRow[_dataTargetIndex];

            InputSize = input.Size;
            OutputSize = output.Size;
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }
	    public override bool IsSequential => false;

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<uint> rows)
        {
            var data = _GetRows(rows)
                .Select(r => ((_dataColumnIndex.Select(i => ((FloatVector)r[i]).Data.ToArray()).ToArray(), ((FloatVector)r[_dataTargetIndex]).Data.ToArray())))
                .ToList()
            ;
            return _GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new VectorBasedDataTableAdaptor(_lap, dataTable);
        }
    }
}
