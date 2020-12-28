using BrightTable;
using BrightTable.Transformations;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Vectorises each row of the data table on demand
    /// </summary>
    class DefaultDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
        private readonly uint[] _featureColumns;

        public DefaultDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, IVectorise inputVectoriser, IVectorise outputVectoriser, uint[] featureColumns)
            : base(lap, dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            InputVectoriser = inputVectoriser ?? new DataTableVectoriser(dataTable, _dataColumnIndex);
            OutputVectoriser = outputVectoriser ?? new DataTableVectoriser(dataTable, dataTable.GetTargetColumnOrThrow());
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new DefaultDataTableAdaptor(_lap, dataTable, InputVectoriser, OutputVectoriser, _featureColumns);
        }

        public override uint InputSize => InputVectoriser.OutputSize;
        public override uint? OutputSize => OutputVectoriser.OutputSize;
        public override bool IsSequential => false;

        public override IMiniBatch Get(IGraphExecutionContext executionContext, uint[] rowIndices)
        {
            var rows = _GetRows(rowIndices);
            var data = rows
                .Select(r => (new[] { InputVectoriser.Vectorise(r) }, OutputVectoriser.Vectorise(r)))
                .ToArray()
            ;
            return _GetMiniBatch(rowIndices, data);
        }
    }
}
