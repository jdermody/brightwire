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
            var firstRow = dataTable.GetRow(0);
            var input = (FloatVector)firstRow.Data[_dataColumnIndex.First()];
            var output = (FloatVector)firstRow.Data[_dataTargetIndex];

            InputSize = input.Size;
            OutputSize = output.Size;
        }

        public override int InputSize { get; }
	    public override int OutputSize { get; }
	    public override bool IsSequential => false;

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows)
                .Select(r => ((_dataColumnIndex.Select(i => ((FloatVector)r.Data[i]).Data).ToArray(), ((FloatVector)r.Data[_dataTargetIndex]).Data)))
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
