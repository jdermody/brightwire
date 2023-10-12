using BrightData;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Linq;
using BrightData.LinearAlgebra;
using BrightData.Analysis;

namespace BrightWire.ExecutionGraph.DataSource
{
    internal class TensorDataSource : IDataSource
    {
        readonly uint _rows, _columns, _depth, _matrixSize;
        readonly ITensor3D[] _data;
        readonly LinearAlgebraProvider _lap;

        public TensorDataSource(LinearAlgebraProvider lap, ITensor3D[] data)
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
        public VectorisationModel? InputVectoriser { get; } = null;
        public VectorisationModel? OutputVectoriser { get; } = null;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(uint[] rows)
        {
            var data = rows.Select(i => _data[(int)i]).ToList();
            var input = _lap.CreateMatrix(InputSize, (uint)data.Count, (i, j) => {
                var tensor = _data[(int)j];
                var rem = i % _matrixSize;
                var z = i / _matrixSize;
                var x = rem % _rows;
                var y = rem / _rows;
                return tensor.GetMatrix(z).GetRowAsReadOnly(x)[y];
            });
            return new MiniBatch(rows, this, new Tensor4DGraphData(input, _rows, _columns, _depth), null);
        }

        public uint[][] GetSequentialBatches()
        {
            return new[] {
                _data.Length.AsRange().ToArray()
            };
        }
    }
}
