using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Execution
{
    class TrainingEngine : ITrainingEngine
    {
        readonly LearningContext _context;
        readonly IGraphInput _input;
        double? _lastTrainingError = null, _lastTestError = null;
        double _trainingErrorDelta = 0;
        int _noImprovementCount = 0;

        public TrainingEngine(LearningContext context, IGraphInput input)
        {
            _context = context;
            _input = input;
        }

        public IGraphInput Input => _input;

        public double? Train(IMiniBatchProvider provider)
        {
            var trainingErrorList = new List<double>();

            _context.StartEpoch();
            _context.SetRowCount(_input.RowCount);
            var trainingError = _input.Train(provider, _context);
            if (trainingError.HasValue)
                trainingErrorList.Add(trainingError.Value);
            _context.EndEpoch();

            double? ret = null;
            if (trainingErrorList.Any()) {
                ret = trainingErrorList.Average();
                if(_lastTrainingError.HasValue) {
                    _trainingErrorDelta = ret.Value - _lastTrainingError.Value;
                }
            }

            _lastTrainingError = ret;
            return ret;
        }

        public IReadOnlyList<(IIndexableVector Output, IIndexableVector TargetOutput)> Test(IMiniBatchProvider provider, int batchSize = 128)
        {
            return _input.Test(provider, batchSize);
        }

        public void WriteTestResults(IMiniBatchProvider provider, IErrorMetric errorMetric, int batchSize = 128)
        {
            var testError = errorMetric.Compute(Test(provider, batchSize)).Average();
            bool flag = true, isPercentage = errorMetric.DisplayAsPercentage;
            if(_lastTestError.HasValue) {
                if (isPercentage && _lastTestError.Value > testError)
                    flag = false;
                else if (!isPercentage && _lastTestError.Value < testError)
                    flag = false;
                else
                    _lastTestError = testError;
            } else
                _lastTestError = testError;

            var format = isPercentage
                ? "Epoch: {0}; t-error: {1:N4} [{2:N4}]; time: {3:N2}s; score: {4:P}"
                : "Epoch: {0}; t-error: {1:N4} [{2:N4}]; time: {3:N2}s; score: {4:N4}"
            ;
            var msg = String.Format(format,
                _context.CurrentEpoch,
                _lastTrainingError ?? 0,
                _trainingErrorDelta,
                _context.EpochSeconds,
                testError
            );
            if (flag)
                msg += "!!";
            else
                ++_noImprovementCount;
            Console.WriteLine(msg);
        }
    }
}
