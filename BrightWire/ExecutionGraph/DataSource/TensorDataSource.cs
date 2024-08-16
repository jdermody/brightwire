using BrightData;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Linq;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;
using BrightData.Buffer.Vectorisation;

namespace BrightWire.ExecutionGraph.DataSource
{
    internal class TensorDataSource : IDataSource
    {
        readonly uint _rows, _columns, _depth, _matrixSize;
        readonly ITensor3D<float>[] _data;
        readonly LinearAlgebraProvider<float> _lap;

        public TensorDataSource(LinearAlgebraProvider<float> lap, ITensor3D<float>[] data)
        {
            _lap = lap;
            _data = data;

            var first = data.First();
            InputSize = first.Segment.Size;
            OutputSize = null;
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = first.Depth;
            _matrixSize = _rows * _columns;
        }

        public uint InputSize { get; }
	    public uint? OutputSize { get; }
        public uint RowCount => (uint)_data.Length;
        public VectorisationModel? InputVectoriser => null;
        public VectorisationModel? OutputVectoriser => null;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public Task<MiniBatch> Get(uint[] rows)
        {
            var data = rows.Select(i => _data[(int)i]).ToList();
            var input = _lap.CreateMatrix(InputSize, (uint)data.Count, (i, j) => {
                var tensor = _data[(int)j];
                var rem = i % _matrixSize;
                var z = i / _matrixSize;
                var x = rem % _rows;
                var y = rem / _rows;
                return tensor.GetMatrix(z)[x, y];
            });
            var ret = new MiniBatch(rows, this, new Tensor4DGraphData(input, _rows, _columns, _depth), null);
            return Task.FromResult(ret);
        }

        public uint[][] GetSequentialBatches()
        {
            return [
                _data.Length.AsRange().ToArray()
            ];
        }
    }
}
