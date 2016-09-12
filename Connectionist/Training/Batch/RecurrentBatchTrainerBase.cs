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

        public ITrainingContext CreateContext(float trainingRate, int batchSize, IErrorMetric errorMetric)
        {
            return new TrainingContext(trainingRate, batchSize, errorMetric);
        }

        T[] _GetArray<T>(IEnumerable<T> sequence, bool shuffle)
        {
            return (shuffle ? sequence.Shuffle() : sequence).ToArray();
        }

        protected IEnumerable<ISequentialMiniBatch> _GetMiniBatches(ISequentialTrainingDataProvider data, bool shuffle, int batchSize)
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
        }
    }
}
