using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Vectorises each row of the data table on demand
    /// </summary>
    internal class DefaultDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
        readonly uint[] _featureColumns;

        public DefaultDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, IDataTableVectoriser? inputVectoriser, IDataTableVectoriser? outputVectoriser, uint[] featureColumns)
            : base(lap, dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            InputVectoriser = inputVectoriser ?? dataTable.GetVectoriser(_dataColumnIndex);
            OutputVectoriser = outputVectoriser ?? dataTable.GetVectoriser(dataTable.GetTargetColumnOrThrow());
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
            var rows = GetRows(rowIndices);
            var data = rows
                .Select(r => (new[] { InputVectoriser.Vectorise(r) }, OutputVectoriser.Vectorise(r)))
                .ToArray()
            ;
            return GetMiniBatch(rowIndices, data);
        }
    }
}
