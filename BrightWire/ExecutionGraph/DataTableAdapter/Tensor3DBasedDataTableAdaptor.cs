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
            _inputColumnIndex = featureColumns.Single();
            _outputSize = outputSize;
            _inputSize = inputSize;
            Height = height;
            Width = width;
            Depth = depth;
        }

        public static async Task<Tensor3DBasedDataTableAdapter> Create(IDataTable dataTable, uint[] featureColumns)
        {
            var targetColumn = dataTable.GetTargetColumn();
            ReadOnlyTensor3D<float> firstTensor;
            var outputSize = 0U;

            if (targetColumn.HasValue) {
                var firstRow = await dataTable.GetRow<ReadOnlyTensor3D<float>, ReadOnlyVector<float>>(featureColumns.Single(), targetColumn.Value);
                firstTensor = firstRow.C1;
                outputSize = firstRow.C2.Size;
            }
            else {
                firstTensor = await dataTable.Get<ReadOnlyTensor3D<float>>(featureColumns.Single(), 0);
            }
            return new(dataTable, featureColumns, firstTensor.Size, outputSize, firstTensor.RowCount, firstTensor.ColumnCount, firstTensor.Depth);
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
            MemoryOwner<IReadOnlyNumericSegment<float>>? targetRows = null;
            var index = 0;

            try {
                await foreach (var row in GetRows(rows)) {
                    inputRows.Span[index] = GetSegment(_inputColumnIndex, row);
                    if (_targetColumnIndex.HasValue) {
                        (targetRows ??= MemoryOwner<IReadOnlyNumericSegment<float>>.Allocate(rows.Length)).Span[index] = GetSegment(_targetColumnIndex.Value, row);
                    }

                    ++index;
                }

                var input = lap.CreateMatrixFromColumns(inputRows.Span);
                var output = targetRows is not null
                    ? lap.CreateMatrixFromRows(targetRows.Span).AsGraphData()
                    : null;
                return new MiniBatch(rows, this, new Tensor4DGraphData(input, Height, Width, Depth), output);
            }
            finally {
                targetRows?.Dispose();
            }
        }
    }
}
