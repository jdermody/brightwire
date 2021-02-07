using BrightData;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Linq;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.DataSource
{
    internal class TensorDataSource : IDataSource
    {
        readonly uint _rows, _columns, _depth, _matrixSize;
        readonly Tensor3D<float>[] _data;
        readonly ILinearAlgebraProvider _lap;

        public TensorDataSource(ILinearAlgebraProvider lap, Tensor3D<float>[] data)
        {
            _lap = lap;
            _data = data;

            var first = data.First();
            InputSize = first.Size;
            OutputSize = null;
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = first.Depth;
            _matrixSize = _rows * _columns;
        }

        public bool IsSequential => false;
        public uint InputSize { get; }
	    public uint? OutputSize { get; }
	    public uint InputCount => 1;
        public uint RowCount => (uint)_data.Length;
        public IDataTableVectoriser? InputVectoriser { get; } = null;
        public IDataTableVectoriser? OutputVectoriser { get; } = null;

        public IDataSource CloneWith(IRowOrientedDataTable dataTable)
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
                return tensor.Matrix(z).Row(x).Segment[y];
            });
            return new MiniBatch(rows, this, new Tensor4DGraphData(input, _rows, _columns, _depth), null);
        }

        public IMiniBatch Get(IGraphExecutionContext executionContext, uint[] rows) => Get(rows);

        public uint[][] GetBuckets()
        {
            return new[] {
                _data.Length.AsRange().ToArray()
            };
        }

        public void OnBatchProcessed(IGraphSequenceContext context)
        {
            // nop
        }
    }
}
