using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Input
{
    public class MiniBatchProvider : IMiniBatchProvider
    {
        readonly IDataSource _dataSource;
        readonly ILinearAlgebraProvider _lap;

        public MiniBatchProvider(IDataSource dataSource, ILinearAlgebraProvider lap)
        {
            _dataSource = dataSource;
            _lap = lap;
        }

        public IDataSource DataSource { get { return _dataSource; } }

        public IEnumerable<(IMatrix, IMatrix)> GetMiniBatches(int batchSize, bool isStochastic)
        {
            var range = Enumerable.Range(0, _dataSource.RowCount);
            var iterationOrder = (isStochastic ? range.Shuffle() : range).ToList();

            for (var j = 0; j < _dataSource.RowCount; j += batchSize) {
                var maxRows = Math.Min(iterationOrder.Count, batchSize + j) - j;
                var rows = iterationOrder.Skip(j).Take(maxRows).ToList();
                var miniBatch = _dataSource.Get(rows);
                yield return (
                    _lap.Create(miniBatch.Count, _dataSource.InputSize, (x, y) => miniBatch[x].Item1[y]),
                    _dataSource.OutputSize > 0 ? _lap.Create(miniBatch.Count, _dataSource.OutputSize, (x, y) => miniBatch[x].Item2[y]) : null
                );
            }
        }
    }
}
