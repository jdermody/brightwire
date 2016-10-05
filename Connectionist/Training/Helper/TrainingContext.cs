using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire.Connectionist.Training.Helper
{
    internal class TrainingContext : ITrainingContext
    {
        int _currentEpoch = 0, _noImprovementCount = 0;
        readonly Stack<double> _trainingErrorDelta = new Stack<double>();
        readonly int _miniBatchSize;
        readonly Stopwatch _timer = new Stopwatch();
        readonly Dictionary<int, float> _scheduledTrainingRate = new Dictionary<int, float>();
        readonly IErrorMetric _errorMetric;

        public TrainingContext(float trainingRate, int miniBatchSize, IErrorMetric errorMetric)
        {
            _miniBatchSize = miniBatchSize;
            TrainingRate = trainingRate;
            _errorMetric = errorMetric;
        }

        public event Action<ITrainingContext> EpochComplete;
        public event Action<ITrainingContext, IRecurrentTrainingContext> RecurrentEpochComplete;

        public IErrorMetric ErrorMetric { get { return _errorMetric; } }
        public double LastTrainingError { get; private set; } = 0.0;
        public float TrainingRate { get; private set; }
        public int TrainingSamples { get; private set; }
        public int CurrentEpoch { get { return _currentEpoch; } }
        public long EpochMilliseconds { get { return _timer.ElapsedMilliseconds; } }
        public double EpochSeconds { get { return EpochMilliseconds / 1000.0; } }
        public int MiniBatchSize { get { return _miniBatchSize; } }
        public bool ShouldContinue { get { return true; } }

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
        }

        public void EndEpoch(double trainingError)
        {
            _timer.Stop();
            var delta = trainingError - LastTrainingError;
            LastTrainingError = trainingError;
            _trainingErrorDelta.Push(delta);

            EpochComplete?.Invoke(this);

            float trainingRate;
            if (_scheduledTrainingRate.TryGetValue(CurrentEpoch, out trainingRate)) {
                TrainingRate = trainingRate;
                Console.WriteLine("Training rate changed to " + trainingRate);
            }
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
            else
                ++_noImprovementCount;
            Console.WriteLine(msg);
        }

        public void ReduceTrainingRate()
        {
            TrainingRate /= 3f;
        }

        public void ScheduleTrainingRateChange(int atEpoch, float newTrainingRate)
        {
            _scheduledTrainingRate[atEpoch] = newTrainingRate;
        }
    }
}
