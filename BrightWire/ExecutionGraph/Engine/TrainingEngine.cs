using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BrightData;
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

        public TrainingEngine(GraphFactory factory, IDataSource dataSource, IErrorMetric errorMetric)
        {
            _factory = factory;
            DataSource = dataSource;
            LinearAlgebraProvider = factory.LinearAlgebraProvider;
            LearningContext = new LearningContext(factory, errorMetric);
            Start = new InputFeeder(0, "engine-input-feeder");
        }

        public IGraphSequenceContext Create(IGraphExecutionContext executionContext, IMiniBatchSequence sequence, ILearningContext? learningContext)
        {
            return new TrainingGraphSequenceContext(learningContext, executionContext, sequence);
        }

        public ILinearAlgebraProvider LinearAlgebraProvider { get; }
        public ExecutionGraphModel Graph => Start.GetGraph();
        public IDataSource DataSource { get; }
        public IEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null)
        {
            LinearAlgebraProvider.PushLayer();
            var provider = new MiniBatchProvider(dataSource, null);
            using var executionContext = new Helper.ExecutionContext(LinearAlgebraProvider, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(batchSize, mb => Execute(executionContext, mb)));
            float operationCount = executionContext.RemainingOperationCount;
            float index = 0f;

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                LinearAlgebraProvider.PushLayer();
                foreach (var context in operation.Execute()) {
                    foreach (var result in context.Results)
                        yield return result;
                    context.Dispose();
                }
                LinearAlgebraProvider.PopLayer();

                if (batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            LinearAlgebraProvider.PopLayer();
        }

        IEnumerable<IGraphSequenceContext> Execute(IGraphExecutionContext executionContext, IMiniBatch batch)
        {
            return Train(executionContext, null, batch);
        }

        public NodeBase Start { get; }

        public void Train(IGraphExecutionContext executionContext, Action<float>? batchCompleteCallback = null)
        {
            LinearAlgebraProvider.PushLayer();
            LearningContext.StartEpoch();
            var provider = new MiniBatchProvider(DataSource, LinearAlgebraProvider.Context.Random);
            executionContext.Add(provider.GetMiniBatches(LearningContext.BatchSize, batch => Train(executionContext, LearningContext, batch)));

            IGraphOperation? operation;
            float operationCount = executionContext.RemainingOperationCount;
            float index = 0f;
            while ((operation = executionContext.GetNextOperation()) != null) {
                LinearAlgebraProvider.PushLayer();
                var contextList = operation.Execute().ToList();
                LearningContext.ApplyUpdates();
                foreach (var context in contextList)
                    context.Dispose();
                LinearAlgebraProvider.PopLayer();

                if (batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            LearningContext.EndEpoch();
            LinearAlgebraProvider.PopLayer();
        }

        IEnumerable<IGraphSequenceContext> Train(IGraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatch batch)
        {
            if (batch.IsSequential) {
                IMiniBatchSequence? curr;
                var contextTable = new Dictionary<IMiniBatchSequence, IGraphSequenceContext>();
                while ((curr = batch.GetNextSequence()) != null) {
                    var context = Train(executionContext, learningContext, curr);
                    contextTable.Add(context.BatchSequence, context);
                }

                var additionalResults = new List<IGraphSequenceContext>();
                if (executionContext.HasContinuations)
                    additionalResults.AddRange(Continue(batch, executionContext, sequence => contextTable[sequence], learningContext));

                foreach (var item in contextTable)
                    yield return item.Value;

                foreach(var result in additionalResults)
                    yield return result;
            }
            else
                yield return Train(executionContext, learningContext, batch.CurrentSequence);
        }

        IEnumerable<IGraphSequenceContext> Continue(IMiniBatch batch, IGraphExecutionContext executionContext, Func<IMiniBatchSequence, IGraphSequenceContext> lookupContext, ILearningContext? learningContext)
        {
            while (executionContext.HasContinuations) {
                var additionalContext = new List<(IGraphSequenceContext Context, Action<IGraphSequenceContext[]> OnEnd)>();
                foreach (var item in executionContext.ExecuteAdditionalMiniBatch(learningContext))
                    additionalContext.Add(item);

                // after all have executed...
                if (additionalContext.Any()) {
                    var groups = additionalContext.GroupBy(d => d.OnEnd);
                    foreach (var group in groups)
                        group.Key(group.Select(d => d.Context).ToArray());

                    foreach (var item in additionalContext)
                        yield return item.Context;
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

        IGraphSequenceContext Train(IGraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatchSequence sequence)
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

            LearningContext.MessageLog(msg.ToString());
            return flag;
        }

        public ILearningContext LearningContext { get; }
        public void LoadParametersFrom(GraphFactory factory, ExecutionGraphModel graph)
        {
            LoadParamaters(factory, graph.InputNode);
            foreach (var node in graph.OtherNodes)
                LoadParamaters(factory, node);
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

        void LoadParamaters(GraphFactory factory, ExecutionGraphModel.Node nodeModel)
		{
			var node = Start.FindById(nodeModel.Id);
			node?.LoadParameters(factory, nodeModel);
		}
    }
}
