using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;

namespace BrightWire.ExecutionGraph.Engine
{
	/// <summary>
	/// Trains graphs as it executes them
	/// </summary>
    internal class TrainingEngine : EngineBase<TrainingEngineContext>, IGraphTrainingEngine
	{
        readonly GraphFactory _factory;
        readonly INode[] _input;
		readonly Random _random;
		float? _lastTestError = null;
        bool _isTraining;

		public TrainingEngine(GraphFactory factory, IDataSource dataSource, ILearningContext learningContext, INode? start) : base(factory.LinearAlgebraProvider)
		{
            _factory = factory;
            _dataSource = dataSource;
            _random = factory.LinearAlgebraProvider.Context.Random;
			LearningContext = learningContext;
			learningContext.SetRowCount(dataSource.RowCount);

			if (start == null) {
                _input = new INode[] { new InputFeeder(0) };
				Start = new FlowThrough();
				Start.Output.AddRange(_input.Select(i => new WireToNode(i)));
			} else {
				Start = start;
				_input = start.Output.Select(w => w.SendTo).ToArray();
			}
		}

        public override TrainingEngineContext CreateContext(IGraphExecutionContext executionContext, IMiniBatchSequence sequence) =>
            new TrainingEngineContext(executionContext, sequence, _isTraining ? LearningContext : null);

        public void Train(IGraphExecutionContext executionContext, Action<float>? batchCompleteCallback = null)
		{
            _isTraining = true;
			_lap.PushLayer();
			LearningContext.StartEpoch();
			var provider = new MiniBatchProvider(_dataSource ?? throw new Exception("No data source was set"), _random);
			executionContext.Add(provider.GetMiniBatches(LearningContext.BatchSize, batch => Train(executionContext, LearningContext, batch)));

			IGraphOperation? operation;
			float operationCount = executionContext.RemainingOperationCount;
			float index = 0f;
			while ((operation = executionContext.GetNextOperation()) != null) {
				_lap.PushLayer();
                var contextList = operation.Execute(executionContext).ToList();
                LearningContext.ApplyUpdates(null);
				foreach(var context in contextList)
					context.Dispose();
                _lap.PopLayer();

				if (batchCompleteCallback != null) {
					var percentage = (++index) / operationCount;
					batchCompleteCallback(percentage);
				}
			}

			LearningContext.EndEpoch();
            _lap.PopLayer();

            _isTraining = false;
        }

		public IDataSource DataSource => _dataSource;
		public ILearningContext LearningContext { get; }
		public INode GetInput(uint index) => _input[(int)index];
		public ExecutionGraphModel Graph => Start.GetGraph();
		public ILinearAlgebraProvider LinearAlgebraProvider => _lap;
		public INode Start { get; }

		protected override IEnumerable<IGraphSequenceContext> Execute(IGraphExecutionContext executionContext, IMiniBatch batch)
		{
            _isTraining = false;
			return Train(executionContext, null, batch);
		}

		IEnumerable<IGraphSequenceContext> Train(IGraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatch batch)
        {
            _isTraining = learningContext != null;
            if (batch.IsSequential) {
                IMiniBatchSequence? curr;
                var contextTable = new Dictionary<IMiniBatchSequence, IGraphSequenceContext>();
                while ((curr = batch.GetNextSequence()) != null) {
                    var context = Train(executionContext, learningContext, curr);
                    contextTable.Add(context.BatchSequence, context);
                }

                var additionalResults = new List<IGraphSequenceContext>();
                if (executionContext.HasContinuations)
                    additionalResults.AddRange(Continue(batch, executionContext, sequence => contextTable[sequence]));

                foreach(var result in additionalResults)
                    yield return result;
            }
            else
                yield return Train(executionContext, learningContext, batch.CurrentSequence);
        }

        //public void AddExecutionResult(IGraphSequenceContext context)
        //{
        //    var output = context.Output;
        //    _executionResults.Add((context.BatchSequence, output.Any()
        //        ? output.Select(o => o.GetMatrix().Data).ToArray()
        //        : new[] { context.Data.GetMatrix().Data }
        //    ));
        //}

		TrainingEngineContext Train(IGraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatchSequence sequence)
		{
            _isTraining = learningContext != null;
			var context = new TrainingEngineContext(executionContext, sequence, learningContext);
			Start.ExecuteForward(context, 0);

			while (context.HasNext)
				context.ExecuteNext();
            return context;
		}

		static string Write(string name, float score, bool isPercentage)
		{
			if (isPercentage)
				return $"{name}score: {score:P}";
			return $"{name}error: {score:N4}";
		}

		public bool Test(
			IDataSource testDataSource,
			IErrorMetric errorMetric,
			uint batchSize = 128,
			Action<float>? batchCompleteCallback = null,
			Action<float, bool, bool>? values = null
		)
        {
            var testResults = Execute(testDataSource, batchSize, batchCompleteCallback)
                .Where(b => b.Target != null)
                .ToList();
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

		void LoadParamaters(GraphFactory factory, ExecutionGraphModel.Node nodeModel)
		{
			var node = Start.FindById(nodeModel.Id);
			node?.LoadParameters(factory, nodeModel);
		}

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
    }
}
