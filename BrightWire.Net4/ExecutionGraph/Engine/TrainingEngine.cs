using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Engine
{
    class TrainingEngine : IGraphTrainingEngine
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IDataSource _dataSource;
        readonly List<(IMiniBatchSequence Sequence, float TrainingError, FloatMatrix Output)> _executionResults = new List<(IMiniBatchSequence Sequence, float TrainingError, FloatMatrix Output)>();
        readonly ILearningContext _learningContext;
        readonly INode _input;
        readonly bool _isStochastic;
        float? _lastTestError = null;
        double? _lastTrainingError = null, _trainingErrorDelta = null;
        int _noImprovementCount = 0;

        public TrainingEngine(ILinearAlgebraProvider lap, IDataSource dataSource, ILearningContext learningContext, INode input)
        {
            _lap = lap;
            _dataSource = dataSource;
            _isStochastic = lap.IsStochastic;
            _learningContext = learningContext;
            learningContext.SetRowCount(dataSource.RowCount);
            _input = input ?? new FlowThrough();
        }

        public IReadOnlyList<ExecutionResult> Execute(IDataSource dataSource, int batchSize = 128)
        {
            var ret = new List<ExecutionResult>();
            var provider = new MiniBatchProvider(dataSource, _isStochastic);
            using (var executionContext = new ExecutionContext(_lap)) {
                executionContext.Add(provider.GetMiniBatches(batchSize, mb => _Execute(executionContext, mb)));

                IGraphOperation operation;
                while ((operation = executionContext.GetNextOperation()) != null) {
                    operation.Execute(executionContext);
                }

                foreach (var item in _executionResults)
                    ret.Add(new ExecutionResult(item.Sequence, item.Output.Row));
            }
            _executionResults.Clear();
            return ret;
        }

        public double Train(IExecutionContext executionContext, Action<float> batchCompleteCallback = null)
        {
            _lap.PushLayer();
            _learningContext.StartEpoch();
            var provider = new MiniBatchProvider(_dataSource, _isStochastic);
            executionContext.Add(provider.GetMiniBatches(_learningContext.BatchSize, batch => _Train(executionContext, _learningContext, batch)));

            IGraphOperation operation;
            float operationCount = executionContext.RemainingOperationCount;
            float index = 0f;
            while ((operation = executionContext.GetNextOperation()) != null) {
                operation.Execute(executionContext);
                _learningContext.ApplyUpdates();

                if(batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            double ret = 0, count = 0;
            foreach (var item in _executionResults) {
                ret += item.TrainingError;
                ++count;
            }
            if (count > 0)
                ret /= count;
            _lap.PopLayer();
            _executionResults.Clear();
            if (_lastTrainingError.HasValue)
                _trainingErrorDelta = ret - _lastTrainingError.Value;
            _lastTrainingError = ret;
            _learningContext.EndEpoch();
            return ret;
        }

        public IDataSource DataSource => _dataSource;
        public ILearningContext LearningContext => _learningContext;
        public INode Input => _input;
        public Models.ExecutionGraph Graph => _input.GetGraph();
        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;

        void _Execute(IExecutionContext executionContext, IMiniBatch batch)
        {
            _Train(executionContext, null, batch);
        }

        void _Train(IExecutionContext executionContext, ILearningContext learningContext, IMiniBatch batch)
        {
            if (batch.IsSequential) {
                IMiniBatchSequence curr = null;
                while ((curr = batch.GetNextSequence()) != null)
                    _Train(executionContext, learningContext, curr);
            } else
                _Train(executionContext, learningContext, batch.CurrentSequence);
        }

        void _Train(IExecutionContext executionContext, ILearningContext learningContext, IMiniBatchSequence sequence)
        {
            _lap.PushLayer();
            using (var context = new TrainingEngineContext(executionContext, sequence, learningContext)) {
                _input.ExecuteForward(context, 0);

                while (context.HasNext)
                    context.ExecuteNext();

                _dataSource.OnBatchProcessed(context);
                _executionResults.Add((context.BatchSequence, 0f, context.Data.GetMatrix().Data));
            }
            _lap.PopLayer();
        }

        public bool Test(IDataSource testDataSource, IErrorMetric errorMetric, int batchSize = 128)
        {
            var testError = Execute(testDataSource, batchSize)
                .Where(b => b.Target != null)
                .Average(o => o.CalculateError(errorMetric))
            ;
            
            bool flag = true, isPercentage = errorMetric.DisplayAsPercentage;
            if (_lastTestError.HasValue) {
                if (isPercentage && _lastTestError.Value > testError)
                    flag = false;
                else if (!isPercentage && _lastTestError.Value < testError)
                    flag = false;
                else
                    _lastTestError = testError;
            } else
                _lastTestError = testError;

            if (_learningContext.CurrentEpoch == 0) {
                var score = String.Format(isPercentage ? "{0:P}" : "{0:N4}", testError);
                Console.WriteLine($"Initial test score: {score}");
                return false;
            } else {
                var format = isPercentage
                    ? "\rEpoch {0} - training-error: {1:N4} [{2:N4}]; time: {3:N2}s; test-score: {4:P}"
                    : "\rEpoch {0} - training-error: {1:N4} [{2:N4}]; time: {3:N2}s; test-score: {4:N4}"
                ;
                var msg = String.Format(format,
                    _learningContext.CurrentEpoch,
                    _lastTrainingError ?? 0,
                    _trainingErrorDelta,
                    _learningContext.EpochSeconds,
                    testError
                );
                if (flag)
                    msg += "!!";
                else
                    ++_noImprovementCount;
                Console.WriteLine(msg);
                return flag;
            }
        }
    }
}
