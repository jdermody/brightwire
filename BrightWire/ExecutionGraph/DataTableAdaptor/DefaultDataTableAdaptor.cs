using BrightTable;
using BrightTable.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Vectorises each row of the data table on demand
    /// </summary>
    class DefaultDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
        readonly DataTableVectoriser _vectoriser;

        public DefaultDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, DataTableVectoriser vectoriser)
            : base(lap, dataTable)
        {
            _vectoriser = vectoriser ?? new DataTableVectoriser(dataTable);
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new DefaultDataTableAdaptor(_lap, dataTable, _vectoriser);
        }

        public override int InputSize => _vectoriser.InputSize;
        public override int OutputSize => _vectoriser.OutputSize;
        public override bool IsSequential => false;

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows)
                .Select(r => (new[] { _vectoriser.GetInput(r) }, _vectoriser.GetOutput(r)))
                .ToList()
            ;
            return _GetMiniBatch(rows, data);
        }
    }
}
