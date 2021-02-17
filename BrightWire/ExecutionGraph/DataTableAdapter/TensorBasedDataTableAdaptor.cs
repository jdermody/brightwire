using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that classify tensors (volumes)
    /// </summary>
    internal class TensorBasedDataTableAdapter : RowBasedDataTableAdapterBase, IVolumeDataSource
    {
        readonly uint[] _featureColumns;
        readonly uint _inputSize, _outputSize;

        public TensorBasedDataTableAdapter(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, uint[] featureColumns)
            : base(lap, dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            var firstRow = dataTable.Row(0);
            var input = (Tensor3D<float>)firstRow[_featureColumnIndices[0]];
            var output = (Vector<float>)firstRow[_targetColumnIndex];
            _outputSize = output.Size;
            _inputSize = input.Size;
            Height = input.RowCount;
            Width = input.ColumnCount;
            Depth = input.Depth;
        }

        TensorBasedDataTableAdapter(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable, uint inputSize, uint outputSize, uint rows, uint columns, uint depth, uint[] featureColumns) 
            : base(lap, dataTable, featureColumns)
        {
            _inputSize = inputSize;
            _outputSize = outputSize;
            Height = rows;
            Width = columns;
            Depth = depth;
            _featureColumns = featureColumns;
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new TensorBasedDataTableAdapter(_lap, dataTable, _inputSize, _outputSize, Height, Width, Depth, _featureColumns);
        }

        public override uint InputSize => _inputSize;
        public override uint? OutputSize => _outputSize;
        public uint Width { get; }
	    public uint Height { get; }
	    public uint Depth { get; }

	    public override IMiniBatch Get(uint[] rows)
        {
            var data = GetRows(rows)
                .Select(r => (_featureColumnIndices.Select(i => ((Tensor3D<float>)r[i]).GetAsRaw()).ToList(), Data: ((Vector<float>)r[_targetColumnIndex]).Segment))
                .ToArray()
            ;
            var inputList = new IGraphData[_featureColumnIndices.Length];
            for (var i = 0; i < _featureColumnIndices.Length; i++) {
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
