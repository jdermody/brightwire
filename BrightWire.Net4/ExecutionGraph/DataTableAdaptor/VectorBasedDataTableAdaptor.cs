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
        readonly int _inputSize;
        readonly int _outputSize;

        public VectorBasedDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable) : base(lap, dataTable)
        {
            var firstRow = dataTable.GetRow(0);
            var input = (FloatVector)firstRow.Data[0];
            var output = (FloatVector)firstRow.Data[1];

            _inputSize = input.Size;
            _outputSize = output.Size;
        }

        public override int InputSize => _inputSize;
        public override int OutputSize => _outputSize;
        public override bool IsSequential => false;

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows)
                .Select(r => (((FloatVector)r.Data[0]).Data, ((FloatVector)r.Data[1]).Data))
                .ToList()
            ;
            return _GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new VectorBasedDataTableAdaptor(_lap, dataTable);
        }
    }
}
