using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Divides epochs into a list of mini batches
    /// </summary>
    class MiniBatchProvider
    {
        class MiniBatchOperation : IGraphOperation
        {
            readonly uint[] _rows;
            readonly MiniBatchProvider _provider;
            readonly Action<IMiniBatch> _handler;

            public MiniBatchOperation(uint[] rows, MiniBatchProvider provider, Action<IMiniBatch> handler)
            {
                _rows = rows;
                _handler = handler;
                _provider = provider;
            }

            public void Execute(IExecutionContext executionContext)
            {
                var dataSource = _provider._dataSource;
                var miniBatch = dataSource.Get(executionContext, _rows);
                _handler(miniBatch);
            }
        }
        readonly IDataSource _dataSource;
        readonly bool _isStochastic;

        public MiniBatchProvider(IDataSource dataSource, bool isStochastic)
        {
            _dataSource = dataSource;
            _isStochastic = isStochastic;
        }

        public IEnumerable<IGraphOperation> GetMiniBatches(uint batchSize, Action<IMiniBatch> handler)
        {
            var buckets = _dataSource.GetBuckets();
            if (_isStochastic)
                buckets.Shuffle();

            foreach (var bucket in buckets) {
                var range = Enumerable.Range(0, bucket.Length);
                var iterationOrder = (_isStochastic ? range.Shuffle() : range).ToList();

                for (uint j = 0; j < bucket.Length; j += batchSize) {
                    var maxRows = Math.Min(iterationOrder.Count, batchSize + j) - j;
                    var rows = iterationOrder.Skip((int)j).Take((int)maxRows).Select(i => bucket[i]).ToArray();
                    yield return new MiniBatchOperation(rows, this, handler);
                }
            }
        }
    }
}
