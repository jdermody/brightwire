using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.Engine
{
	/// <summary>
	/// Trains graphs as it executes them
	/// </summary>
	class TrainingEngine : EngineBase, IGraphTrainingEngine
	{
		readonly List<(IMiniBatchSequence Sequence, double? TrainingError, IReadOnlyList<FloatMatrix> Output)> _executionResults = new List<(IMiniBatchSequence Sequence, double? TrainingError, IReadOnlyList<FloatMatrix> Output)>();
		readonly List<IContext> _contextList = new List<IContext>();
		readonly IReadOnlyList<INode> _input;
		readonly bool _isStochastic;
		float? _lastTestError = null;
		double? _lastTrainingError = null, _trainingErrorDelta = null;

		public TrainingEngine(ILinearAlgebraProvider lap, IDataSource dataSource, ILearningContext learningContext, INode start) : base(lap)
		{
			_dataSource = dataSource;
			_isStochastic = lap.IsStochastic;
			LearningContext = learningContext;
			learningContext.SetRowCount(dataSource.RowCount);

			if (start == null) {
				_input = Enumerable.Range(0, dataSource.InputCount).Select(i => new InputFeeder(i)).ToList();
				Start = new FlowThrough();
				Start.Output.AddRange(_input.Select(i => new WireToNode(i)));
			} else {
				Start = start;
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
					foreach (var item in _executionResults) {
						int outputIndex = 0;
						foreach (var output in item.Output) {
							ret.Add(new ExecutionResult(item.Sequence, output.Row, outputIndex));
							++outputIndex;
						}
					}

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

		protected override IReadOnlyList<ExecutionResult> _GetResults()
		{
			var ret = new List<ExecutionResult>();
			foreach (var item in _executionResults) {
				int outputIndex = 0;
				foreach (var output in item.Output) {
					ret.Add(new ExecutionResult(item.Sequence, output.Row, outputIndex));
					++outputIndex;
				}
			}

			_executionResults.Clear();
			return ret;
		}

		protected override void _ClearContextList()
		{
			foreach (var item in _contextList)
				item.Dispose();
			_contextList.Clear();
		}

		public double Train(IExecutionContext executionContext, Action<float> batchCompleteCallback = null)
		{
			_lap.PushLayer();
			LearningContext.StartEpoch();
			var provider = new MiniBatchProvider(_dataSource, _isStochastic);
			executionContext.Add(provider.GetMiniBatches(LearningContext.BatchSize, batch => _contextList.AddRange(_Train(executionContext, LearningContext, batch))));

			IGraphOperation operation;
			float operationCount = executionContext.RemainingOperationCount;
			float index = 0f;
			while ((operation = executionContext.GetNextOperation()) != null) {
				_lap.PushLayer();
				operation.Execute(executionContext);
				LearningContext.ApplyUpdates();
				_ClearContextList();
				_lap.PopLayer();

				if (batchCompleteCallback != null) {
					var percentage = (++index) / operationCount;
					batchCompleteCallback(percentage);
				}
			}

			double trainingError = 0;
			if (LearningContext.TrainingErrorCalculation == TrainingErrorCalculation.Fast) {
				var count = 0;
				foreach (var item in _executionResults) {
					if (item.TrainingError.HasValue) {
						trainingError += item.TrainingError.Value;
						++count;
					}
				}

				if (count > 0)
					trainingError /= count;
			}
			LearningContext.EndEpoch();
			_executionResults.Clear();
			_lap.PopLayer();

			if (LearningContext.TrainingErrorCalculation == TrainingErrorCalculation.TrainingData) {
				if (LearningContext.ErrorMetric != null) {
					trainingError = Execute(_dataSource, LearningContext.BatchSize, batchCompleteCallback)
						.Where(b => b.Target != null)
						.Average(o => o.CalculateError(LearningContext.ErrorMetric))
					;
				}
			}

			if (_lastTrainingError.HasValue)
				_trainingErrorDelta = trainingError - _lastTrainingError.Value;
			_lastTrainingError = trainingError;
			return trainingError;
		}

		public IDataSource DataSource => _dataSource;
		public ILearningContext LearningContext { get; }
		public INode GetInput(int index) => _input[index];
		public Models.ExecutionGraph Graph => Start.GetGraph();
		public ILinearAlgebraProvider LinearAlgebraProvider => _lap;
		public INode Start { get; }

		protected override void _Execute(IExecutionContext executionContext, IMiniBatch batch)
		{
			_contextList.AddRange(_Train(executionContext, null, batch));
		}

		IReadOnlyList<IContext> _Train(IExecutionContext executionContext, ILearningContext learningContext, IMiniBatch batch)
		{
			var ret = new List<TrainingEngineContext>();
			if (batch.IsSequential) {
				IMiniBatchSequence curr;
				while ((curr = batch.GetNextSequence()) != null)
					ret.Add(_Train(executionContext, learningContext, curr));

				var contextTable = new Lazy<Dictionary<IMiniBatchSequence, TrainingEngineContext>>(() => ret.ToDictionary(c => c.BatchSequence, c => c));
				var didContinue = _Continue(batch, executionContext, sequence => contextTable.Value[sequence]);
				if (didContinue) {
					foreach (var context in ret)
						_CompleteSequence(context);
				}
			} else
				ret.Add(_Train(executionContext, learningContext, batch.CurrentSequence));
			return ret;
		}

		void _CompleteSequence(TrainingEngineContext context)
		{
			_dataSource.OnBatchProcessed(context);
			var output = context.Output;
			_executionResults.Add((context.BatchSequence, context.TrainingError, output.Any()
				? output.Select(o => o.GetMatrix().Data).ToArray()
				: new[] { context.Data.GetMatrix().Data }
			));
		}

		TrainingEngineContext _Train(IExecutionContext executionContext, ILearningContext learningContext, IMiniBatchSequence sequence)
		{
			var context = new TrainingEngineContext(executionContext, sequence, learningContext);
			Start.ExecuteForward(context, 0);

			while (context.HasNext)
				context.ExecuteNext();

			if (!executionContext.HasContinuations)
				_CompleteSequence(context);
			return context;
		}

		static string _Write(string name, float score, bool isPercentage)
		{
			if (isPercentage)
				return $"{name}-score: {score:P}";
			return $"{name}-error: {score:N4}";
		}

		public bool Test(
			IDataSource testDataSource,
			IErrorMetric errorMetric,
			int batchSize = 128,
			Action<float> batchCompleteCallback = null,
			Action<float, double, bool, bool> values = null
		)
		{
			var testError = Execute(testDataSource, batchSize, batchCompleteCallback)
				.Where(b => b.Target != null)
				.Average(o => o.CalculateError(errorMetric))
			;

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

			values?.Invoke(testError, _lastTrainingError ?? 0, isPercentage, flag);
			if (LearningContext.CurrentEpoch == 0) {
				LearningContext.MessageLog(_Write("\rInitial test", testError, isPercentage));
				return false;
			}

			var trainingScore = "";
			if (LearningContext.TrainingErrorCalculation != TrainingErrorCalculation.None) {
				trainingScore = _Write(" training", Convert.ToSingle(_lastTrainingError ?? 0), LearningContext.TrainingErrorCalculation == TrainingErrorCalculation.TrainingData && isPercentage);
				trainingScore += $" [{_trainingErrorDelta:N4}];";
			}

			var testScore = _Write("test", testError, isPercentage);
			if (testErrorDelta.HasValue)
				testScore += $" [{testErrorDelta.Value:N4}]";

			var msg = $"\rEpoch {LearningContext.CurrentEpoch} -{trainingScore} time: {LearningContext.EpochSeconds:N2}s; {testScore}";
			if (flag)
				msg += "!!";

			LearningContext.MessageLog(msg);
			return flag;
		}

		void _LoadParamaters(Models.ExecutionGraph.Node nodeModel)
		{
			var node = Start.FindById(nodeModel.Id);
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
