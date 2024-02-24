using System.Threading.Tasks;
using BrightData;
using BrightData.DataTable.Helper;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Vectorise each row of the data table on demand
    /// </summary>
    internal class DefaultDataTableAdapter : GenericRowBasedDataTableAdapterBase
    {
        readonly uint[] _featureColumns;

        DefaultDataTableAdapter(IDataTable dataTable, VectorisationModel? inputVectoriser, VectorisationModel? outputVectoriser, uint[] featureColumns)
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            InputVectoriser = inputVectoriser;
            OutputVectoriser = outputVectoriser;
        }

        public static async Task<DefaultDataTableAdapter> Create(IDataTable dataTable, VectorisationModel? inputVectoriser, VectorisationModel? outputVectoriser, uint[] featureColumns)
        {
            return new DefaultDataTableAdapter(
                dataTable,
                inputVectoriser ?? await dataTable.GetVectoriser(true, featureColumns),
                outputVectoriser ?? await dataTable.GetVectoriser(true, dataTable.GetTargetColumnOrThrow()),
                featureColumns
            );
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new DefaultDataTableAdapter(dataTable, InputVectoriser, OutputVectoriser, _featureColumns);
        }

        public override uint InputSize => InputVectoriser!.OutputSize;
        public override uint? OutputSize => OutputVectoriser?.OutputSize;

        public override async Task<MiniBatch> Get(uint[] rowIndices)
        {
            var index = 0;
            var data = new (float[], float[])[rowIndices.Length];
            await foreach (var row in GetRows(rowIndices))
                data[index++] = (InputVectoriser!.Vectorise(row), OutputVectoriser!.Vectorise(row));
            return GetMiniBatch(rowIndices, data);
        }
    }
}
