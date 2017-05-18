using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Engine
{
    class TrainingEngine : IGraphTrainingEngine
    {
        readonly IExecutionContext _executionContext;
        readonly ILinearAlgebraProvider _lap;
        readonly IDataSource _dataSource;
        readonly List<(TrainingEngineContext Context, IMatrix Output)> _executionResults = new List<(TrainingEngineContext, IMatrix)>();
        readonly ILearningContext _learningContext;
        readonly INode _input;
        readonly bool _isStochastic;
        float? _lastTestError = null;
        double? _lastTrainingError = null, _trainingErrorDelta = null;
        int _noImprovementCount = 0;

        public TrainingEngine(ILinearAlgebraProvider lap, IDataSource dataSource, ILearningContext learningContext, IExecutionContext executionContext, INode input)
        {
            _lap = lap;
            _dataSource = dataSource;
            _isStochastic = lap.IsStochastic;
            _executionContext = executionContext;
            _learningContext = learningContext;
            _input = input ?? new FlowThrough();
        }

        public IReadOnlyList<ExecutionResult> Execute(IDataSource dataSource, int batchSize = 128)
        {
            var provider = new MiniBatchProvider(dataSource, _isStochastic);
            _executionContext.Add(provider.GetMiniBatches(batchSize, _Execute));

            IGraphOperation operation;
            while ((operation = _executionContext.GetNextOperation()) != null) {
                operation.Execute();
            }

            var ret = new List<ExecutionResult>();
            foreach (var item in _executionResults) {
                ret.Add(new ExecutionResult(item.Context.BatchSequence, item.Output.AsIndexable().Rows.ToList()));
            }

            _executionResults.Clear();
            return ret;
        }

        public double Train(Action<float> batchCompleteCallback = null)
        {
            _learningContext.StartEpoch();
            var provider = new MiniBatchProvider(_dataSource, _isStochastic);
            _executionContext.Add(provider.GetMiniBatches(_learningContext.BatchSize, batch => _Train(_learningContext, batch)));

            IGraphOperation operation;
            float operationCount = _executionContext.RemainingOperationCount;
            float index = 0f;
            while ((operation = _executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                operation.Execute();
                _learningContext.ApplyUpdates();
                _lap.PopLayer();

                if(batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            double ret = 0, count = 0;
            foreach (var item in _executionResults) {
                ret += item.Context.TrainingError;
                ++count;
            }
            if (count > 0)
                ret /= count;
            _executionResults.Clear();
            if (_lastTrainingError.HasValue)
                _trainingErrorDelta = ret - _lastTrainingError.Value;
            _lastTrainingError = ret;
            _learningContext.EndEpoch();
            return ret;
        }

        public IExecutionContext ExecutionContext => _executionContext;
        public IDataSource DataSource => _dataSource;
        public ILearningContext LearningContext => _learningContext;
        public INode Input => _input;
        public Models.ExecutionGraph Graph => _input.GetGraph();

        void _Execute(IMiniBatch batch)
        {
            _Train(null, batch);
        }

        void _Train(ILearningContext learningContext, IMiniBatch batch)
        {
            if (batch.IsSequential) {
                IMiniBatchSequence curr = null;
                while ((curr = batch.GetNextSequence()) != null)
                    _Train(learningContext, curr);
            } else
                _Train(learningContext, batch.CurrentSequence);
        }

        void _Train(ILearningContext learningContext, IMiniBatchSequence sequence)
        {
            var context = new TrainingEngineContext(_executionContext, sequence, learningContext);
            _input.ExecuteForward(context, 0);

            while (context.HasNext)
                context.ExecuteNext();

            _dataSource.OnBatchProcessed(context);
            _executionResults.Add((context, context.Data.GetMatrix()));
        }

        public bool Test(IDataSource testDataSource, IErrorMetric errorMetric, int batchSize = 128)
        {
            _lap.PushLayer();
            var testError = Execute(testDataSource, batchSize)
                .Where(b => b.Target != null)
                .Average(o => o.CalculateError(errorMetric))
            ;
            _lap.PopLayer();
            
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
                    ? "\rEpoch: {0} - training-error: {1:N4} [{2:N4}]; time: {3:N2}s; test-score: {4:P}"
                    : "\rEpoch: {0} - training-error: {1:N4} [{2:N4}]; time: {3:N2}s; test-score: {4:N4}"
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
