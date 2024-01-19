using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Divides epochs into a list of mini batches
    /// </summary>
    internal class MiniBatchProvider(IDataSource dataSource, Random? random)
    {
        public class Operation(uint[] rows, MiniBatchProvider provider) : IGraphOperation
        {
            public IMiniBatch GetMiniBatch()
            {
                var dataSource = provider._dataSource;
                return dataSource.Get(rows);
            }
        }
        readonly IDataSource _dataSource = dataSource;

        public IEnumerable<Operation> GetMiniBatches(uint batchSize)
        {
            var buckets = _dataSource.GetSequentialBatches();
            if (random != null)
                buckets = buckets.Select(v => v.Shuffle(random).ToArray()).Shuffle(random).ToArray();

            foreach (var bucket in buckets) {
                var range = bucket.Length.AsRange();
                var iterationOrder = (random != null ? range.Shuffle(random) : range).ToList();

                for (uint j = 0; j < bucket.Length; j += batchSize) {
                    var maxRows = Math.Min(iterationOrder.Count, batchSize + j) - j;
                    var rows = iterationOrder.Skip((int)j).Take((int)maxRows).Select(i => bucket[i]).ToArray();
                    yield return new Operation(rows, this);
                }
            }
        }
    }
}
