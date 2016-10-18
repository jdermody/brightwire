using BrightWire.Connectionist.Training.Helper;
using BrightWire.Models;
using BrightWire.Models.Simple;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.Connectionist.Training.Manager
{
    internal class BidirectionalManager : RecurrentManagerBase, IBidirectionalRecurrentTrainingManager
    {
        readonly ILinearAlgebraProvider _lap;
        readonly INeuralNetworkBidirectionalBatchTrainer _trainer;
        readonly string _dataFile;
        double _bestScore;
        BidirectionalNetwork _bestOutput = null;
        float[] _forwardMemory, _backwardMemory;

        public BidirectionalManager(ILinearAlgebraProvider lap,
            INeuralNetworkBidirectionalBatchTrainer trainer,
            string dataFile,
            ISequentialTrainingDataProvider testData,
            int memorySize,
            int? autoAdjustOnNoChangeCount = 5) : base(testData, autoAdjustOnNoChangeCount)
        {
            _lap = lap;
            _trainer = trainer;
            _dataFile = dataFile;

            var memory = _Load(_trainer, dataFile, memorySize);
            _forwardMemory = memory.Item1;
            _backwardMemory = memory.Item2;
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

        public INeuralNetworkBidirectionalBatchTrainer Trainer { get { return _trainer; } }

        public BidirectionalMemory Memory
        {
            get
            {
                return new BidirectionalMemory(_forwardMemory, _backwardMemory);
            }
        }

        public void Train(ISequentialTrainingDataProvider trainingData, int numEpochs, ITrainingContext context, IRecurrentTrainingContext recurrentContext = null)
        {
            if (recurrentContext == null)
                recurrentContext = new RecurrentContext(_lap, context);

            _bestScore = _GetScore(_testData, _trainer, _forwardMemory, _backwardMemory, recurrentContext);
            Console.WriteLine(context.ErrorMetric.DisplayAsPercentage ? "Initial score: {0:P}" : "Initial score: {0}", _bestScore);

            // train
            recurrentContext.TrainingContext.RecurrentEpochComplete += OnEpoch;
            var ret = _trainer.Train(trainingData, _forwardMemory, _backwardMemory, numEpochs, recurrentContext);
            recurrentContext.TrainingContext.RecurrentEpochComplete -= OnEpoch;

            // ensure best values are current
            ApplyBestParams();
        }

        public override void ApplyBestParams()
        {
            if (_bestOutput != null) {
                _trainer.NetworkInfo = _bestOutput;
                _forwardMemory = _bestOutput.ForwardMemory.Data;
                _backwardMemory = _bestOutput.BackwardMemory.Data;
            }
        }

        private void OnEpoch(ITrainingContext context, IRecurrentTrainingContext recurrentContext)
        {
            if(_CalculateTestScore(context, _forwardMemory, _backwardMemory, _testData, _trainer, recurrentContext, ref _bestScore, ref _bestOutput)) {
                using (var stream = new FileStream(_dataFile, FileMode.Create, FileAccess.Write))
                    Serializer.Serialize(stream, _bestOutput);
            }
        }
    }
}
