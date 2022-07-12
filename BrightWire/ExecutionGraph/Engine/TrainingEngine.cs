using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Engine
{
    class TrainingEngine : IGraphTrainingEngine
    {
        readonly GraphFactory _factory;
        float? _lastTestError = null;
        readonly InputFeeder _start;

        public TrainingEngine(GraphFactory factory, IDataSource dataSource, IErrorMetric errorMetric)
        {
            _factory = factory;
            DataSource = dataSource;
            LinearAlgebraProvider = factory.LinearAlgebraProvider;
            LearningContext = new LearningContext(factory, errorMetric);
            Start = _start = new InputFeeder(0, "engine-input-feeder");
        }

        public IGraphContext Create(GraphExecutionContext executionContext, IMiniBatchSequence sequence, ILearningContext? learningContext)
        {
            return new TrainingGraphSequenceContext(learningContext, executionContext, sequence);
        }

        public void SetStartNode(NodeBase? startNode)
        {
            Start = startNode ?? _start;
        }

        public BrightDataContext Context => _factory.Context;
        public LinearAlgebraProvider LinearAlgebraProvider { get; }
        public ExecutionGraphModel Graph => Start.GetGraph();
        public IDataSource DataSource { get; }
        public IEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null, CancellationToken ct = default)
        {
            LinearAlgebraProvider.PushScope();
            var provider = new MiniBatchProvider(dataSource, null);
            using var executionContext = new GraphExecutionContext(this);
            executionContext.Add(provider.GetMiniBatches(batchSize));
            float operationCount = executionContext.RemainingOperationCount;
            var index = 0f;

            while (executionContext.GetNextOperation() is { } operation && !ct.IsCancellationRequested) {
                LinearAlgebraProvider.PushScope();
                foreach (var context in Execute(executionContext, operation, ct)) {
                    foreach (var result in context.Results)
                        yield return result;
                    context.Dispose();
                }
                LinearAlgebraProvider.PopScope();

                if (batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            LinearAlgebraProvider.PopScope();
        }

        IEnumerable<IGraphContext> Execute(GraphExecutionContext executionContext, IGraphOperation operation, CancellationToken ct)
        {
            return Train(executionContext, null, operation, ct);
        }

        public NodeBase Start { get; private set; }

        public void Train(GraphExecutionContext executionContext, Action<float>? batchCompleteCallback = null, CancellationToken ct = default)
        {
            LinearAlgebraProvider.PushScope();
            LearningContext.StartEpoch();

            var provider = new MiniBatchProvider(DataSource, Context.Random);
            executionContext.Add(provider.GetMiniBatches(LearningContext.BatchSize));

            IGraphOperation? operation;
            float operationCount = executionContext.RemainingOperationCount;
            var index = 0f;
            while ((operation = executionContext.GetNextOperation()) != null && !ct.IsCancellationRequested) {
                LinearAlgebraProvider.PushScope();
                var contextList = Train(executionContext, LearningContext, operation, ct).ToList();
                LearningContext.ApplyUpdates();
                foreach (var context in contextList)
                    context.Dispose();
                LinearAlgebraProvider.PopScope();

                if (batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            LearningContext.EndEpoch();
            LinearAlgebraProvider.PopScope();
        }

        IEnumerable<IGraphContext> Train(GraphExecutionContext executionContext, ILearningContext? learningContext, IGraphOperation operation, CancellationToken ct)
        {
            var batch = operation.GetMiniBatch();
            if (batch.IsSequential) {
                IMiniBatchSequence? curr;
                var contextTable = new Dictionary<IMiniBatchSequence, IGraphContext>();
                while ((curr = batch.GetNextSequence()) != null && !ct.IsCancellationRequested) {
                    var context = Train(executionContext, learningContext, curr);
                    contextTable.Add(context.BatchSequence, context);
                }

                var additionalResults = new List<IGraphContext>();
                if (executionContext.HasContinuations)
                    additionalResults.AddRange(Continue(batch, executionContext, sequence => contextTable[sequence], learningContext, ct));

                foreach (var item in contextTable)
                    yield return item.Value;

                foreach(var result in additionalResults)
                    yield return result;
            }
            else
                yield return Train(executionContext, learningContext, batch.CurrentSequence);
        }

        IEnumerable<IGraphContext> Continue(IMiniBatch batch, GraphExecutionContext executionContext, Func<IMiniBatchSequence, IGraphContext> lookupContext, ILearningContext? learningContext, CancellationToken ct)
        {
            while (executionContext.HasContinuations && !ct.IsCancellationRequested) {
                var additionalContext = new List<(IGraphContext Context, Action<IGraphContext[]> OnEnd)>();
                foreach (var item in executionContext.ExecuteAdditionalMiniBatch(learningContext))
                    additionalContext.Add(item);

                // after all have executed...
                if (additionalContext.Any()) {
                    var groups = additionalContext.GroupBy(d => d.OnEnd);
                    foreach (var group in groups)
                        group.Key(group.Select(d => d.Context).ToArray());

                    foreach (var (context, _) in additionalContext)
                        yield return context;
                }

                batch.Reset();
                IMiniBatchSequence? currentSequence;
                while ((currentSequence = batch.GetNextSequence()) != null) {
                    var context = lookupContext(currentSequence);
                    executionContext.Continue(context);
                    yield return context;
                }
            }
        }

        IGraphContext Train(GraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatchSequence sequence)
        {
            var context = Create(executionContext, sequence, learningContext);
            Start.Forward(GraphData.Null, context);
            return context;
        }

        public bool Test(IDataSource testDataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null, Action<float, bool, bool>? values = null)
        {
            static string Write(string name, float score, bool isPercentage)
		    {
			    if (isPercentage)
				    return $"{name}score: {score:P}";
			    return $"{name}error: {score:N4}";
		    }
            var testResults = Execute(testDataSource, batchSize, batchCompleteCallback)
                .Where(b => b.Target != null)
                .ToList();
            var errorMetric = LearningContext.ErrorMetric;
            var testError = testResults.Any() ? testResults.Average(o => o.CalculateError(errorMetric)) : 0;

            bool flag = true, isPercentage = errorMetric.DisplayAsPercentage;
            float? testErrorDelta = null;
            if (_lastTestError.HasValue) {
                testErrorDelta = testError - _lastTestError.Value;
                if (isPercentage && _lastTestError.Value > testError)
                    flag = false;
                else if (!isPercentage && _lastTestError.Value < testError)
                    flag = false;
                else
                    _lastTestError = testError;
            } else
                _lastTestError = testError;

            var msg = new StringBuilder();
            values?.Invoke(testError, isPercentage, flag);
            if (LearningContext.CurrentEpoch == 0)
                msg.Append(Write("\rInitial ", testError, isPercentage));
            else {
                var testScore = Write("", testError, isPercentage);
                if (testErrorDelta.HasValue)
                    testScore += $" [{testErrorDelta.Value:N4}]";

                msg.Append($"\rEpoch {LearningContext.CurrentEpoch} - time: {LearningContext.EpochSeconds:N2}s; {testScore}");
                if (flag)
                    msg.Append("!!");
            }

            for (var i = msg.Length; i < 117; i++)
                msg.Append(' ');

            LearningContext.GraphFactory.Context.UserNotifications?.OnMessage(msg.ToString());
            return flag;
        }

        public ILearningContext LearningContext { get; }
        public void LoadParametersFrom(GraphFactory factory, ExecutionGraphModel graph)
        {
            LoadParameters(factory, graph.InputNode);
            foreach (var node in graph.OtherNodes)
                LoadParameters(factory, node);
        }

        public IGraphExecutionEngine CreateExecutionEngine(ExecutionGraphModel? model)
        {
            return _factory.CreateExecutionEngine(model ?? Graph);
        }

        public void Reset()
        {
            _lastTestError = null;
            LearningContext.ResetEpoch();
        }

        void LoadParameters(GraphFactory factory, ExecutionGraphModel.Node nodeModel)
		{
			var node = Start.FindById(nodeModel.Id);
            if(node is null && !String.IsNullOrEmpty(nodeModel.Name))
                node = Start.FindByName(nodeModel.Name);
			node?.LoadParameters(factory, nodeModel);
		}
    }
}
