using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.DataSource
{
    class TensorDataSource : IDataSource
    {
        readonly int _inputSize, _outputSize, _rows, _columns, _depth, _matrixSize;
        readonly IReadOnlyList<FloatTensor> _data;
        readonly ILinearAlgebraProvider _lap;

        public TensorDataSource(ILinearAlgebraProvider lap, IReadOnlyList<FloatTensor> data)
        {
            _lap = lap;
            _data = data;

            var first = data.First();
            _inputSize = first.Size;
            _outputSize = -1;
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = first.Depth;
            _matrixSize = _rows * _columns;
        }

        public bool IsSequential => false;

        public int InputSize => _inputSize;

        public int OutputSize => _outputSize;

        public int InputCount => 1;

        public int RowCount => _data.Count;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = rows.Select(i => _data[i]).ToList();
            var input = _lap.CreateMatrix(InputSize, data.Count, (i, j) => {
                var tensor = _data[j];
                var rem = i % _matrixSize;
                var z = i / _matrixSize;
                var x = rem % _rows;
                var y = rem / _rows;
                return tensor.Matrix[z].Row[x].Data[y];
            });

            var inputList = new List<IGraphData> {
                new Tensor4DGraphData(input, _rows, _columns, _depth)
            };
            return new MiniBatch(rows, this, inputList, null);
        }

        public IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return new[] {
                Enumerable.Range(0, _data.Count).ToList()
            };
        }

        public void OnBatchProcessed(IContext context)
        {
            // nop
        }
    }
}
