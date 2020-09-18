using BrightTable;
using BrightTable.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Vectorises each row of the data table on demand
    /// </summary>
    class DefaultDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
        readonly DataTableVectoriser _inputVectoriser;
        readonly DataTableVectoriser _outputVectoriser;

        public DefaultDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, DataTableVectoriser inputVectoriser, DataTableVectoriser outputVectoriser)
            : base(lap, dataTable)
        {
            _inputVectoriser = inputVectoriser ?? new DataTableVectoriser(dataTable, _dataColumnIndex);
            _outputVectoriser = outputVectoriser ?? new DataTableVectoriser(dataTable, dataTable.GetTargetColumnOrThrow());
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new DefaultDataTableAdaptor(_lap, dataTable, _inputVectoriser, _outputVectoriser);
        }

        public override uint InputSize => _inputVectoriser.Size;
        public override uint? OutputSize => _outputVectoriser.Size;
        public override bool IsSequential => false;

        public override IMiniBatch Get(IExecutionContext executionContext, uint[] rowIndices)
        {
            var rows = _GetRows(rowIndices);
            var data = rows
                .Select(r => (new[] { _inputVectoriser.Convert(r) }, _outputVectoriser.Convert(r)))
                .ToArray()
            ;
            return _GetMiniBatch(rowIndices, data);
        }
    }
}
