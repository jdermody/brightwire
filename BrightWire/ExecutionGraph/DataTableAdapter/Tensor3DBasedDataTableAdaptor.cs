using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.LinearAlgebra.ReadOnly;
using BrightWire.ExecutionGraph.Helper;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that classify tensors (volumes)
    /// </summary>
    internal class Tensor3DBasedDataTableAdapter : GenericRowBasedDataTableAdapterBase, IVolumeDataSource
    {
        readonly uint[] _featureColumns;
        readonly uint _inputSize, _outputSize, _inputColumnIndex;

        Tensor3DBasedDataTableAdapter(IDataTable dataTable, uint[] featureColumns, uint inputSize, uint outputSize, uint height, uint width, uint depth)
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            _outputSize = outputSize;
            _inputSize = inputSize;
            Height = height;
            Width = width;
            Depth = depth;
        }

        public static async Task<Tensor3DBasedDataTableAdapter> Create(IDataTable dataTable, uint[] featureColumns)
        {
            var buffer = dataTable.GetRowsBuffer<ReadOnlyTensor3D<float>, ReadOnlyVector<float>>(featureColumns.Single(), dataTable.GetTargetColumnOrThrow());
            var firstRow = await buffer.GetItem(0);
            var firstTensor = firstRow.C1;
            return new(dataTable, featureColumns, firstTensor.Size, firstRow.C2.Size, firstTensor.RowCount, firstTensor.ColumnCount, firstTensor.Depth);
        }

        Tensor3DBasedDataTableAdapter(IDataTable dataTable, uint inputSize, uint outputSize, uint rows, uint columns, uint depth, uint[] featureColumns)
            : base(dataTable, featureColumns)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;
            Height = rows;
            Width = columns;
            Depth = depth;
            _featureColumns = featureColumns;
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new Tensor3DBasedDataTableAdapter(dataTable, _inputSize, _outputSize, Height, Width, Depth, _featureColumns);
        }

        public override uint InputSize => _inputSize;
        public override uint? OutputSize => _outputSize;
        public uint Width { get; }
        public uint Height { get; }
        public uint Depth { get; }

        public override async Task<MiniBatch> Get(uint[] rows)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            using var inputRows = MemoryOwner<IReadOnlyNumericSegment<float>>.Allocate(rows.Length);
            using var targetRows = MemoryOwner<IReadOnlyNumericSegment<float>>.Allocate(rows.Length);
            var index = 0;

            await foreach (var row in GetRows(rows)) {
                inputRows.Span[index] = GetSegment(_inputColumnIndex, row);
                targetRows.Span[index] = GetSegment(_targetColumnIndex, row);
                ++index;
            }

            var input = lap.CreateMatrixFromColumns(inputRows.Span);
            var output = OutputSize > 0
                ? lap.CreateMatrixFromRows(targetRows.Span).AsGraphData()
                : null;

            return new MiniBatch(rows, this, new Tensor4DGraphData(input, Height, Width, Depth), output);
        }
    }
}
