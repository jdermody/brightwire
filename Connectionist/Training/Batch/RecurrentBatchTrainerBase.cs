using BrightWire.Connectionist.Training.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Training.Batch
{
    class RecurrentBatchTrainerBase
    {
        readonly protected bool _stochastic;
        readonly protected ILinearAlgebraProvider _lap;
        readonly protected List<INeuralNetworkRecurrentTrainerFilter> _filter = new List<INeuralNetworkRecurrentTrainerFilter>();

        public RecurrentBatchTrainerBase(ILinearAlgebraProvider lap, bool stochastic)
        {
            _lap = lap;
            _stochastic = stochastic;
        }

        public ILinearAlgebraProvider LinearAlgebraProvider { get { return _lap; } }

        public ITrainingContext CreateContext(float trainingRate, int batchSize)
        {
            return new TrainingContext(trainingRate, batchSize);
        }

        T[] _GetArray<T>(IEnumerable<T> sequence, bool shuffle)
        {
            return (shuffle ? sequence.Shuffle() : sequence).ToArray();
        }

        protected IEnumerable<Tuple<IMatrix[], IMatrix[], int[]>> _GetMiniBatches(ISequentialTrainingDataProvider data, bool shuffle, int batchSize)
        {
            var sequences = shuffle ? data.Length.Shuffle() : data.Length;
            foreach (var item in sequences) {
                var range = Enumerable.Range(0, item.Item2);
                var items = shuffle ? range.Shuffle().ToList() : range.ToList();
                for (var i = 0; i < items.Count; i += batchSize) {
                    var batch = items.Skip(i).Take(batchSize).ToList();
                    yield return data.GetTrainingData(item.Item1, batch);
                }
            }

            // group the data by sequence size
            //var sequencesByLength = _GetArray(data
            //    .Select((a, ind) => Tuple.Create(a, ind))
            //    .GroupBy(s => s.Item1.Length)
            //    .OrderBy(g => g.Key)
            //    .Select(g => g.ToList()
            //), shuffle);
            //var sequenceIndexTable = data.Select((t, i) => Tuple.Create(i, t)).ToDictionary(t => t.Item2, t => t.Item1);
            //for (var i = 0; i < sequencesByLength.Length; i++) {
            //    var dataGroup = sequencesByLength[i];
            //    var firstItem = dataGroup.First().Item1;
            //    var sequenceLength = firstItem.Length;
            //    var inputSize = firstItem.First().Item1.Length;
            //    var outputSize = firstItem.First().Item2.Length;
            //    var iterationOrder = _GetArray(Enumerable.Range(0, dataGroup.Count), shuffle);

            //    // break the data into matrices corresponding to: rows for each feature, columns for each sample.  Array of such for each sequence.
            //    for (var j = 0; j < dataGroup.Count; j += batchSize) {
            //        var maxRows = Math.Min(iterationOrder.Length, batchSize + j) - j;
            //        var input = new IMatrix[sequenceLength];
            //        var output = new IMatrix[sequenceLength];
            //        for (var k = 0; k < sequenceLength; k++) {
            //            input[k] = _lap.Create(maxRows, inputSize, (x, y) => dataGroup[iterationOrder[j + x]].Item1[k].Item1[y]);
            //            output[k] = _lap.Create(maxRows, outputSize, (x, y) => dataGroup[iterationOrder[j + x]].Item1[k].Item2[y]);
            //        }
            //        var inputIndex = Enumerable.Range(0, maxRows).Select(x => dataGroup[iterationOrder[j + x]].Item2).ToArray();
            //        yield return Tuple.Create(input, output, inputIndex);
            //    }
            //}
        }
    }
}
