using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Icbld.BrightWire.Connectionist.Training.Helper
{
    public class TrainingContext : ITrainingContext
    {
        int _currentEpoch = 0;
        readonly Stack<double> _trainingErrorDelta = new Stack<double>();
        readonly List<IMatrix> _epochGarbage = new List<IMatrix>();
        readonly int _miniBatchSize;
        readonly Stopwatch _timer = new Stopwatch();

        public TrainingContext(float trainingRate, int miniBatchSize)
        {
            _miniBatchSize = miniBatchSize;
            TrainingRate = trainingRate;
        }

        public event Action<ITrainingContext> EpochComplete;
        public event Action<ITrainingContext, IRecurrentTrainingContext> RecurrentEpochComplete;

        public double LastTrainingError { get; private set; } = 0.0;
        public float TrainingRate { get; private set; }
        public int TrainingSamples { get; private set; }
        public int CurrentEpoch { get { return _currentEpoch; } }
        public long EpochMilliseconds { get { return _timer.ElapsedMilliseconds; } }
        public double EpochSeconds { get { return EpochMilliseconds / 1000.0; } }
        public int MiniBatchSize { get { return _miniBatchSize; } }

        public XmlWriter Logger
        {
            get;set;
        }

        public void Reset()
        {
            _currentEpoch = 0;
            _trainingErrorDelta.Clear();
            LastTrainingError = 0.0;
            _timer.Reset();
        }

        public void StartEpoch(int trainingSamples)
        {
            TrainingSamples = trainingSamples;
            ++_currentEpoch;
            _timer.Restart();
        }

        public void EndBatch()
        {
            Cleanup();
        }

        public void EndEpoch(double trainingError)
        {
            _timer.Stop();
            var delta = trainingError - LastTrainingError;
            LastTrainingError = trainingError;
            _trainingErrorDelta.Push(delta);

            Cleanup();

            EpochComplete?.Invoke(this);
        }

        public void EndRecurrentEpoch(double trainingError, IRecurrentTrainingContext context)
        {
            EndEpoch(trainingError);
            RecurrentEpochComplete?.Invoke(this, context);
        }

        public void WriteScore(double correctPercentage, bool asPercentage, bool flag = false)
        {
            var format = asPercentage
                ? "Epoch: {0}; t-error: {1:N4} [{2:N4}]; time: {3:N2}s; score: {4:P}"
                : "Epoch: {0}; t-error: {1:N4} [{2:N4}]; time: {3:N2}s; score: {4:N4}"
            ;
            var msg = String.Format(format,
                _currentEpoch,
                LastTrainingError,
                _trainingErrorDelta.First(),
                EpochSeconds,
                correctPercentage
            );
            if (flag)
                msg += "!!";
            Console.WriteLine(msg);
        }

        public IMatrix AddToGarbage(IMatrix matrix)
        {
            _epochGarbage.Add(matrix);
            return matrix;
        }

        public void Cleanup()
        {
            foreach (var item in _epochGarbage)
                item.Dispose();
            _epochGarbage.Clear();
        }
    }
}
