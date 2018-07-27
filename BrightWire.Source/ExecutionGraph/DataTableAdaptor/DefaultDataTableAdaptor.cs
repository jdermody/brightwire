using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Vectorises each row of the data table on demand
    /// </summary>
    class DefaultDataTableAdaptor : RowBasedDataTableAdaptorBase, IRowEncoder
    {
        readonly IDataTableVectoriser _vectoriser;

        public DefaultDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable, IDataTableVectoriser vectoriser = null)
            : base(lap, dataTable)
        {
            _vectoriser = vectoriser ?? dataTable.GetVectoriser();
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new DefaultDataTableAdaptor(_lap, dataTable, _vectoriser);
        }

        public override int InputSize => _vectoriser.InputSize;
        public override int OutputSize => _vectoriser.OutputSize;
        public override bool IsSequential => false;

        public float[] Encode(IRow row)
        {
            return _vectoriser.GetInput(row).Data;
        }

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows)
                .Select(r => (new[] { Encode(r) }, _vectoriser.GetOutput(r).Data))
                .ToList()
            ;
            return _GetMiniBatch(rows, data);
        }
    }
}
