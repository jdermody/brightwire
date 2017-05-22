using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Helper
{
    class MiniBatchProvider
    {
        class MiniBatchOperation : IGraphOperation
        {
            readonly IReadOnlyList<int> _rows;
            readonly MiniBatchProvider _provider;
            readonly Action<IMiniBatch> _handler;

            public MiniBatchOperation(IReadOnlyList<int> rows, MiniBatchProvider provider, Action<IMiniBatch> handler)
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

        public IReadOnlyList<IGraphOperation> GetMiniBatches(int batchSize, Action<IMiniBatch> handler)
        {
            var ret = new List<IGraphOperation>();
            var buckets = _dataSource.GetBuckets();
            if (_isStochastic)
                buckets.Shuffle();

            foreach (var bucket in buckets) {
                var range = Enumerable.Range(0, bucket.Count);
                var iterationOrder = (_isStochastic ? range.Shuffle() : range).ToList();

                for (var j = 0; j < bucket.Count; j += batchSize) {
                    var maxRows = Math.Min(iterationOrder.Count, batchSize + j) - j;
                    var rows = iterationOrder.Skip(j).Take(maxRows).Select(i => bucket[i]).ToList();
                    ret.Add(new MiniBatchOperation(rows, this, handler));
                }
            }
            return ret;
        }
    }
}
