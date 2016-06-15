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
        readonly IErrorMetric _errorMetric;
        readonly string _dataFile;
        readonly int _reportCadence;
        double _bestScore;
        FeedForwardNetwork _bestOutput = null;

        public FeedForwardManager(INeuralNetworkTrainer trainer, string dataFile, ITrainingDataProvider testData, IErrorMetric errorMetric, int reportCadence = 1)
        {
            _reportCadence = reportCadence;
            _trainer = trainer;
            _dataFile = dataFile;
            _testData = testData;
            _errorMetric = errorMetric;

            _LoadFromFile();
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

        private void _LoadFromFile()
        {
            if (File.Exists(_dataFile)) {
                using (var stream = new FileStream(_dataFile, FileMode.Open, FileAccess.Read))
                    _trainer.NetworkInfo = Serializer.Deserialize<FeedForwardNetwork>(stream);
            }
        }

        public INeuralNetworkTrainer Trainer { get { return _trainer; } }

        double _GetScore()
        {
            return _trainer.Execute(_testData).Average(d => _errorMetric.Compute(d.Item1, d.Item2));
        }

        public void Train(ITrainingDataProvider trainingData, int numEpochs, ITrainingContext context)
        {
            _bestScore = _GetScore();
            Console.WriteLine(_errorMetric.DisplayAsPercentage ? "Initial score: {0:P}" : "Initial score: {0}", _bestScore);

            _bestOutput = null;
            context.EpochComplete += OnEpochComplete;
            _trainer.Train(trainingData, numEpochs, context);
            context.EpochComplete -= OnEpochComplete;

            if (_bestOutput != null) {
                using (var stream = new FileStream(_dataFile, FileMode.Create, FileAccess.Write))
                    Serializer.Serialize(stream, _bestOutput);

                // ensure best values are current
                _trainer.NetworkInfo = _bestOutput;
            }
            _LoadFromFile();
        }

        private void OnEpochComplete(ITrainingContext context)
        {
            var score = _GetScore();
            var flag = false;
            if ((_errorMetric.HigherIsBetter && score > _bestScore) || (!_errorMetric.HigherIsBetter && score < _bestScore)) {
                _bestScore = score;
                _bestOutput = _trainer.NetworkInfo;
                flag = true;
            }
            if((context.CurrentEpoch % _reportCadence) == 0)
                context.WriteScore(score, _errorMetric.DisplayAsPercentage, flag);
        }
    }
}
