using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that classify tensors (volumes)
    /// </summary>
    internal class TensorBasedDataTableAdapter : RowBasedDataTableAdapterBase, IVolumeDataSource
    {
        readonly uint[] _featureColumns;
        readonly uint _inputSize, _outputSize;

        public TensorBasedDataTableAdapter(BrightDataTable dataTable, uint[] featureColumns)
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            var firstRow = dataTable.GetRow(0);
            var input = (IReadOnlyTensor3D)firstRow[_featureColumnIndices[0]];
            var output = (IReadOnlyVector)firstRow[_targetColumnIndex];
            _outputSize = output.Size;
            _inputSize = input.Segment.Size;
            Height = input.RowCount;
            Width = input.ColumnCount;
            Depth = input.Depth;
        }

        TensorBasedDataTableAdapter(BrightDataTable dataTable, uint inputSize, uint outputSize, uint rows, uint columns, uint depth, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;
            Height = rows;
            Width = columns;
            Depth = depth;
            _featureColumns = featureColumns;
        }

        public override IDataSource CloneWith(BrightDataTable dataTable)
        {
            return new TensorBasedDataTableAdapter(dataTable, _inputSize, _outputSize, Height, Width, Depth, _featureColumns);
        }

        public override uint InputSize => _inputSize;
        public override uint? OutputSize => _outputSize;
        public uint Width { get; }
	    public uint Height { get; }
	    public uint Depth { get; }

	    public override IMiniBatch Get(uint[] rows)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            var data = GetRows(rows)
                .Select(r => (_featureColumnIndices.Select(i => ((IReadOnlyTensor3D)r[i]).Segment).ToList(), Data: ((IReadOnlyVector)r[_targetColumnIndex]).Segment))
                .ToArray()
            ;
            var inputList = new IGraphData[_featureColumnIndices.Length];
            for (var i = 0; i < _featureColumnIndices.Length; i++) {
	            var i1 = i;
	            var input = lap.CreateMatrix(InputSize, (uint)data.Length, (x, y) => data[y].Item1[i1][x]);
                var tensor = new Tensor4DGraphData(input, Height, Width, Depth);
                inputList[i] = tensor;
            }
            var output = OutputSize > 0 
                ? lap.CreateMatrix((uint)data.Length, OutputSize.Value, (x, y) => data[x].Data[y]).AsGraphData()
                : null;
            
            // TODO: change from single
            return new MiniBatch(rows, this, inputList.Single(), output);
        }
    }
}
