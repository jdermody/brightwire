using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables that classify tensors (volumes)
    /// </summary>
    internal class TensorBasedDataTableAdaptor : RowBasedDataTableAdaptorBase, IVolumeDataSource
    {
        readonly uint _inputSize, _outputSize;

        public TensorBasedDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, uint[] featureColumns)
            : base(lap, dataTable, featureColumns)
        {
            var firstRow = dataTable.Row(0);
            var input = (Tensor3D<float>)firstRow[_dataColumnIndex[0]];
            var output = (Vector<float>)firstRow[_dataTargetIndex];
            _outputSize = output.Size;
            _inputSize = input.Size;
            Height = input.RowCount;
            Width = input.ColumnCount;
            Depth = input.Depth;
        }

        TensorBasedDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, uint inputSize, uint outputSize, uint rows, uint columns, uint depth) 
            :base(lap, dataTable, null)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;
            Height = rows;
            Width = columns;
            Depth = depth;
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new TensorBasedDataTableAdaptor(_lap, dataTable, _inputSize, _outputSize, Height, Width, Depth);
        }

        public override bool IsSequential => false;
        public override uint InputSize => _inputSize;
        public override uint? OutputSize => _outputSize;
        public uint Width { get; }
	    public uint Height { get; }
	    public uint Depth { get; }

	    public override IMiniBatch Get(uint[] rows)
        {
            var data = GetRows(rows)
                .Select(r => (_dataColumnIndex.Select(i => ((Tensor3D<float>)r[i]).GetAsRaw()).ToList(), Data: ((Vector<float>)r[_dataTargetIndex]).Segment))
                .ToArray()
            ;
            var inputList = new IGraphData[_dataColumnIndex.Length];
            for (var i = 0; i < _dataColumnIndex.Length; i++) {
	            var i1 = i;
	            var input = _lap.CreateMatrix(InputSize, (uint)data.Length, (x, y) => data[y].Item1[i1][x]);
                var tensor = new Tensor4DGraphData(input, Height, Width, Depth);
                inputList[i] = tensor;
            }
            var output = OutputSize > 0 
                ? new MatrixGraphData(_lap.CreateMatrix((uint)data.Length, OutputSize.Value, (x, y) => data[x].Data[y]))
                : null;
            
            // TODO: change from single
            return new MiniBatch(rows, this, inputList.Single(), output);
        }
    }
}
