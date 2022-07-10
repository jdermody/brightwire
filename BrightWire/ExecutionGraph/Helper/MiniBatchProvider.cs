using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Divides epochs into a list of mini batches
    /// </summary>
    internal class MiniBatchProvider
    {
        public class Operation : IGraphOperation
        {
            readonly uint[] _rows;
            readonly MiniBatchProvider _provider;

            public Operation(uint[] rows, MiniBatchProvider provider)
            {
                _rows = rows;
                _provider = provider;
            }

            public IMiniBatch GetMiniBatch()
            {
                var dataSource = _provider._dataSource;
                return dataSource.Get(_rows);
            }
        }
        readonly IDataSource _dataSource;
        readonly Random? _random;

        public MiniBatchProvider(IDataSource dataSource, Random? random)
        {
            _dataSource = dataSource;
            _random = random;
        }

        public IEnumerable<Operation> GetMiniBatches(uint batchSize)
        {
            var buckets = _dataSource.GetSequentialBatches();
            if (_random != null)
                buckets = buckets.Select(v => v.Shuffle(_random).ToArray()).Shuffle(_random).ToArray();

            foreach (var bucket in buckets) {
                var range = bucket.Length.AsRange();
                var iterationOrder = (_random != null ? range.Shuffle(_random) : range).ToList();

                for (uint j = 0; j < bucket.Length; j += batchSize) {
                    var maxRows = Math.Min(iterationOrder.Count, batchSize + j) - j;
                    var rows = iterationOrder.Skip((int)j).Take((int)maxRows).Select(i => bucket[i]).ToArray();
                    yield return new Operation(rows, this);
                }
            }
        }
    }
}
