using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.Engine
{
    /// <summary>
    /// Trains graphs as it executes them
    /// </summary>
    class TrainingEngine : IGraphTrainingEngine
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IDataSource _dataSource;
        readonly List<(IMiniBatchSequence Sequence, double TrainingError, FloatMatrix Output)> _executionResults = new List<(IMiniBatchSequence Sequence, double TrainingError, FloatMatrix Output)>();
        readonly List<IContext> _contextList = new List<IContext>();
        readonly ILearningContext _learningContext;
        readonly IReadOnlyList<INode> _input;
        readonly INode _start;
        readonly bool _isStochastic;
        float? _lastTestError = null;
        double? _lastTrainingError = null, _trainingErrorDelta = null;
        int _noImprovementCount = 0;

        public TrainingEngine(ILinearAlgebraProvider lap, IDataSource dataSource, ILearningContext learningContext, INode start)
        {
            _lap = lap;
            _dataSource = dataSource;
            _isStochastic = lap.IsStochastic;
            _learningContext = learningContext;
            learningContext.SetRowCount(dataSource.RowCount);

            if(start == null) {
                _input = Enumerable.Range(0, dataSource.InputCount).Select(i => new InputFeeder(i)).ToList();
                _start = new FlowThrough();
                _start.Output.AddRange(_input.Select(i => new WireToNode(i)));
            }else {
                _start = start;
                _input = start.Output.Select(w => w.SendTo).ToList();
            }
        }

        public IReadOnlyList<ExecutionResult> Execute(IDataSource dataSource, int batchSize = 128, Action<float> batchCompleteCallback = null)
        {
            _lap.PushLayer();
            var ret = new List<ExecutionResult>();
            var provider = new MiniBatchProvider(dataSource, _isStochastic);
            using (var executionContext = new ExecutionContext(_lap)) {
                executionContext.Add(provider.GetMiniBatches(batchSize, mb => _Execute(executionContext, mb)));
                float operationCount = executionContext.RemainingOperationCount;
                float index = 0f;
                IGraphOperation operation;
                while ((operation = executionContext.GetNextOperation()) != null) {
                    _lap.PushLayer();
                    operation.Execute(executionContext);
                    _ClearContextList();
                    foreach (var item in _executionResults)
                        ret.Add(new ExecutionResult(item.Sequence, item.Output.Row));
                    _executionResults.Clear();
                    _lap.PopLayer();

                    if (batchCompleteCallback != null) {
                        var percentage = (++index) / operationCount;
                        batchCompleteCallback(percentage);
                    }
                }
            }
            _lap.PopLayer();
            return ret;
        }

        void _ClearContextList()
        {
            foreach (var item in _contextList)
                item.Dispose();
            _contextList.Clear();
        }

        public double Train(IExecutionContext executionContext, Action<float> batchCompleteCallback = null)
        {
            _lap.PushLayer();
            _learningContext.StartEpoch();
            var provider = new MiniBatchProvider(_dataSource, _isStochastic);
            executionContext.Add(provider.GetMiniBatches(_learningContext.BatchSize, batch => _contextList.AddRange(_Train(executionContext, _learningContext, batch))));

            IGraphOperation operation;
            float operationCount = executionContext.RemainingOperationCount;
            float index = 0f;
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                operation.Execute(executionContext);
                _learningContext.ApplyUpdates();
                _ClearContextList();
                _lap.PopLayer();

                if (batchCompleteCallback != null) {
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

            if (_lastTrainingError.HasValue)
                _trainingErrorDelta = ret - _lastTrainingError.Value;
            _lastTrainingError = ret;
            _learningContext.EndEpoch();
            _executionResults.Clear();
            _lap.PopLayer();
            return ret;
        }

        public IDataSource DataSource => _dataSource;
        public ILearningContext LearningContext => _learningContext;
        public INode GetInput(int index) => _input[index];
        public Models.ExecutionGraph Graph => _start.GetGraph();
        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;
        public INode Start => _start;

        void _Execute(IExecutionContext executionContext, IMiniBatch batch)
        {
            _contextList.AddRange(_Train(executionContext, null, batch));
        }

        IReadOnlyList<IContext> _Train(IExecutionContext executionContext, ILearningContext learningContext, IMiniBatch batch)
        {
            var ret = new List<IContext>();
            if (batch.IsSequential) {
                IMiniBatchSequence curr = null;
                while ((curr = batch.GetNextSequence()) != null)
                    ret.Add(_Train(executionContext, learningContext, curr));
            } else
                ret.Add(_Train(executionContext, learningContext, batch.CurrentSequence));
            return ret;
        }

        IContext _Train(IExecutionContext executionContext, ILearningContext learningContext, IMiniBatchSequence sequence)
        {
            var context = new TrainingEngineContext(executionContext, sequence, learningContext);
            _start.ExecuteForward(context, 0);

            while (context.HasNext)
                context.ExecuteNext();

            _dataSource.OnBatchProcessed(context);
            _executionResults.Add((context.BatchSequence, context.TrainingError, context.Data.GetMatrix().Data));
            return context;
        }

        public bool Test(IDataSource testDataSource, IErrorMetric errorMetric, int batchSize = 128, Action<float> batchCompleteCallback = null)
        {
            var testError = Execute(testDataSource, batchSize, batchCompleteCallback)
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
                Console.WriteLine($"\rInitial test score: {score}");
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

        public ExecutionResult Execute(float[] input)
        {
            _lap.PushLayer();
            ExecutionResult ret = null;
            var provider = new MiniBatchProvider(new Helper.SingleRowDataSource(input), _isStochastic);
            using (var executionContext = new ExecutionContext(_lap)) {
                executionContext.Add(provider.GetMiniBatches(1, mb => _Execute(executionContext, mb)));

                IGraphOperation operation;
                while ((operation = executionContext.GetNextOperation()) != null) {
                    operation.Execute(executionContext);
                    _ClearContextList();
                }

                foreach (var item in _executionResults)
                    ret = new ExecutionResult(item.Sequence, item.Output.Row);
            }
            _executionResults.Clear();
            _lap.PopLayer();
            return ret;
        }

        void _LoadParamaters(Models.ExecutionGraph.Node nodeModel)
        {
            var node = _start.FindById(nodeModel.Id);
            node.LoadParameters(nodeModel);
        }

        public void LoadParametersFrom(Models.ExecutionGraph graph)
        {
            if (graph.InputNode != null)
                _LoadParamaters(graph.InputNode);
            if (graph.OtherNodes != null) {
                foreach (var node in graph.OtherNodes)
                    _LoadParamaters(node);
            }
        }
    }
}
