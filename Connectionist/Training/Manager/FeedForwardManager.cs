using BrightWire.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Training.Manager
{
    public class FeedForwardManager : IFeedForwardTrainingManager
    {
        readonly INeuralNetworkTrainer _trainer;
        readonly ITrainingDataProvider _testData;
        readonly string _dataFile;
        readonly int _reportCadence;
        double _bestScore;
        FeedForwardNetwork _bestOutput = null;

        public FeedForwardManager(INeuralNetworkTrainer trainer, string dataFile, ITrainingDataProvider testData, int reportCadence = 1)
        {
            _reportCadence = reportCadence;
            _trainer = trainer;
            _dataFile = dataFile;
            _testData = testData;

            if (File.Exists(_dataFile)) {
                using (var stream = new FileStream(_dataFile, FileMode.Open, FileAccess.Read))
                    _trainer.NetworkInfo = Serializer.Deserialize<FeedForwardNetwork>(stream);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                _trainer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public INeuralNetworkTrainer Trainer { get { return _trainer; } }

        double _GetScore(ITrainingContext context)
        {
            return _trainer.Execute(_testData).Average(d => context.ErrorMetric.Compute(d.Output, d.ExpectedOutput));
        }

        public void Train(ITrainingDataProvider trainingData, int numEpochs, ITrainingContext context)
        {
            _bestScore = _GetScore(context);
            Console.WriteLine(context.ErrorMetric.DisplayAsPercentage ? "Initial score: {0:P}" : "Initial score: {0}", _bestScore);

            _bestOutput = null;
            context.EpochComplete += OnEpochComplete;
            _trainer.Train(trainingData, numEpochs, context);
            context.EpochComplete -= OnEpochComplete;

            // ensure best values are current
            if (_bestOutput != null)
                _trainer.NetworkInfo = _bestOutput;
        }

        private void OnEpochComplete(ITrainingContext context)
        {
            var score = _GetScore(context);
            var flag = false;
            var errorMetric = context.ErrorMetric;
            if ((errorMetric.HigherIsBetter && score > _bestScore) || (!errorMetric.HigherIsBetter && score < _bestScore)) {
                _bestScore = score;
                _bestOutput = _trainer.NetworkInfo;
                flag = true;
            }
            if((context.CurrentEpoch % _reportCadence) == 0)
                context.WriteScore(score, errorMetric.DisplayAsPercentage, flag);

            if(flag) {
                using (var stream = new FileStream(_dataFile, FileMode.Create, FileAccess.Write))
                    Serializer.Serialize(stream, _bestOutput);
            }
        }
    }
}
