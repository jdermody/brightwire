using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Input
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

            public void Execute()
            {
                var lap = _provider._lap;
                var dataSource = _provider._dataSource;
                if (dataSource.IsSequential) {
                    var miniBatchData = dataSource.GetSequential(_rows);
                    List<FloatVector> temp;
                    var inputData = new Dictionary<int, List<FloatVector>>();
                    var outputData = new Dictionary<int, List<FloatVector>>();
                    foreach (var item in miniBatchData) {
                        var input = item.Input;
                        var output = item.Output;
                        for (int i = 0, len = input.RowCount; i < len; i++) {
                            if (!inputData.TryGetValue(i, out temp))
                                inputData.Add(i, temp = new List<FloatVector>());
                            temp.Add(input.Row[i]);

                            if (output != null) {
                                if (!outputData.TryGetValue(i, out temp))
                                    outputData.Add(i, temp = new List<FloatVector>());
                                temp.Add(output.Row[i]);
                            }
                        }
                    }

                    var miniBatch = new MiniBatch(_rows, dataSource);
                    foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                        var input = lap.Create(item.Value);
                        IMatrix output = null;
                        if (outputData.TryGetValue(item.Key, out temp))
                            output = lap.Create(temp);
                        var type = (item.Key == 0)
                            ? MiniBatchType.SequenceStart
                            : item.Key == (inputData.Count - 1)
                                ? MiniBatchType.SequenceEnd
                                : MiniBatchType.Standard
                        ;
                        miniBatch.Add(type, input, output);
                    }
                    _handler(miniBatch);
                } else {
                    var miniBatch = dataSource.Get(_rows);
                    var input = lap.Create(miniBatch.Count, dataSource.InputSize, (x, y) => miniBatch[x].Item1[y]);
                    var output = dataSource.OutputSize > 0 ? lap.Create(miniBatch.Count, dataSource.OutputSize, (x, y) => miniBatch[x].Item2[y]) : null;
                    _handler(new MiniBatch(_rows, dataSource, input, output));
                }
            }
        }
        readonly IDataSource _dataSource;
        readonly ILinearAlgebraProvider _lap;
        readonly bool _isStochastic;

        public MiniBatchProvider(IDataSource dataSource, ILinearAlgebraProvider lap, bool isStochastic)
        {
            _dataSource = dataSource;
            _lap = lap;
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
