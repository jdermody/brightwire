using System.Threading.Tasks;
using BrightData;
using BrightWire.ExecutionGraph.Helper;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that classify tensors (volumes)
    /// </summary>
    internal class Tensor3DBasedDataTableAdapter : RowBasedDataTableAdapterBase, IVolumeDataSource
    {
        readonly uint[] _featureColumns;
        readonly uint _inputSize, _outputSize, _inputColumnIndex;

        public Tensor3DBasedDataTableAdapter(IDataTable dataTable, uint[] featureColumns)
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            var firstRow = dataTable[0];
            var input = (IReadOnlyTensor3D)firstRow[_inputColumnIndex = _featureColumnIndices[0]];
            var output = (IReadOnlyVector)firstRow[_targetColumnIndex];
            _outputSize = output.Size;
            _inputSize = input.ReadOnlySegment.Size;
            Height = input.RowCount;
            Width = input.ColumnCount;
            Depth = input.Depth;
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
