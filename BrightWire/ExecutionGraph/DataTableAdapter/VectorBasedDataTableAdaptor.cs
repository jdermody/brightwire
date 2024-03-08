using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.LinearAlgebra.ReadOnly;
using BrightWire.ExecutionGraph.Helper;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Segment table adapter for tables with vector data
    /// </summary>
    internal class VectorBasedDataTableAdapter : TypedRowBasedDataTableAdapterBase<ReadOnlyVector<float>, ReadOnlyVector<float>>
    {
        /// <summary>
        /// Segment table adapter for tables with vector data
        /// </summary>
        VectorBasedDataTableAdapter(IDataTable dataTable, uint[] featureColumns, uint inputSize, uint? outputSize) : base(dataTable, featureColumns)
        {
            InputSize = inputSize;
            OutputSize = outputSize;
        }

        public static async Task<VectorBasedDataTableAdapter> Create(IDataTable dataTable, uint[] featureColumns)
        {
            var buffer = dataTable.GetRowsBuffer<ReadOnlyVector<float>, ReadOnlyVector<float>>(featureColumns.Single(), dataTable.GetTargetColumnOrThrow());
            var firstRow = await buffer.GetItem(0);
            return new(dataTable, featureColumns, firstRow.C1.Size, firstRow.C2.Size);
        }

        public override uint InputSize { get; }
        public override uint? OutputSize { get; }

        public override async Task<MiniBatch> Get(uint[] rows)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            using var inputRows = MemoryOwner<IReadOnlyNumericSegment<float>>.Allocate(rows.Length);
            using var targetRows = MemoryOwner<IReadOnlyNumericSegment<float>>.Allocate(rows.Length);
            var index = 0;

            await foreach (var row in GetRows(rows)) {
                inputRows.Span[index] = row.C1.ReadOnlySegment;
                targetRows.Span[index] = row.C2.ReadOnlySegment;
                ++index;
            }

            var input = lap.CreateMatrixFromRows(inputRows.Span);
            var output = OutputSize > 0
                ? lap.CreateMatrixFromRows(targetRows.Span)
                : null;

            return new MiniBatch(rows, this, input.AsGraphData(), output?.AsGraphData());
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new VectorBasedDataTableAdapter(dataTable, _featureColumns, InputSize, OutputSize);
        }
    }
}
