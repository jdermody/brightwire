using BrightWire.Models;
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

        class MiniBatch : IGraphOperation
        {
            readonly IReadOnlyList<int> _rows;
            readonly MiniBatchProvider _provider;
            readonly Action<(MiniBatchType Type, IMatrix Input, IMatrix Output)> _handler;

            public MiniBatch(IReadOnlyList<int> rows, MiniBatchProvider provider, Action<(MiniBatchType Type, IMatrix Input, IMatrix Output)> handler)
            {
                _rows = rows;
                _handler = handler;
                _provider = provider;
            }

            public bool CanContinue => true;

            public void Execute()
            {
                var lap = _provider._lap;
                var dataSource = _provider._dataSource;
                if (dataSource.IsSequential) {
                    var miniBatch = dataSource.GetSequential(_rows);
                    List<FloatArray> temp;
                    var inputData = new Dictionary<int, List<FloatArray>>();
                    var outputData = new Dictionary<int, List<FloatArray>>();
                    foreach (var item in miniBatch) {
                        var input = item.Input;
                        var output = item.Output;
                        for(int i = 0, len = input.RowCount; i < len; i++) {
                            if (!inputData.TryGetValue(i, out temp))
                                inputData.Add(i, temp = new List<FloatArray>());
                            temp.Add(input.Row[i]);

                            if (output != null) {
                                if (!outputData.TryGetValue(i, out temp))
                                    outputData.Add(i, temp = new List<FloatArray>());
                                temp.Add(output.Row[i]);
                            }
                        }
                    }

                    foreach(var item in inputData.OrderBy(kv => kv.Key)) {
                        var input = lap.Create(item.Value);
                        IMatrix output = null;
                        if(outputData.TryGetValue(item.Key, out temp))
                            output = lap.Create(temp);
                        var type = (item.Key == 0)
                            ? MiniBatchType.SequenceStart 
                            : item.Key == (inputData.Count - 1)
                                ? MiniBatchType.SequenceEnd 
                                : MiniBatchType.Standard
                        ;
                        _handler((type, input, output));
                    }
                } else {
                    var miniBatch = dataSource.Get(_rows);
                    _handler((
                        MiniBatchType.Standard,
                        lap.Create(miniBatch.Count, dataSource.InputSize, (x, y) => miniBatch[x].Item1[y]),
                        dataSource.OutputSize > 0 ? lap.Create(miniBatch.Count, dataSource.OutputSize, (x, y) => miniBatch[x].Item2[y]) : null
                    ));
                }
            }
        }

        public MiniBatchProvider(IDataSource dataSource, ILinearAlgebraProvider lap)
        {
            _dataSource = dataSource;
            _lap = lap;
        }

        public IDataSource DataSource { get { return _dataSource; } }

        public IReadOnlyList<IGraphOperation> GetMiniBatches(int batchSize, bool isStochastic, Action<(MiniBatchType Type, IMatrix Input, IMatrix Output)> handler)
        {
            var ret = new List<IGraphOperation>();
            var buckets = _dataSource.GetBuckets();
            if (isStochastic)
                buckets.Shuffle();

            foreach (var bucket in buckets) {
                var range = Enumerable.Range(0, bucket.Count);
                var iterationOrder = (isStochastic ? range.Shuffle() : range).ToList();

                for (var j = 0; j < bucket.Count; j += batchSize) {
                    var maxRows = Math.Min(iterationOrder.Count, batchSize + j) - j;
                    var rows = iterationOrder.Skip(j).Take(maxRows).Select(i => bucket[i]).ToList();
                    ret.Add(new MiniBatch(rows, this, handler));
                }
            }
            return ret;
        }
    }
}
