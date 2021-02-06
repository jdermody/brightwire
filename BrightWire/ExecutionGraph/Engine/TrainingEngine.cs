using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.Engine
{
	/// <summary>
	/// Trains graphs as it executes them
	/// </summary>
    internal class TrainingEngine : EngineBase, IGraphTrainingEngine
	{
		readonly List<(IMiniBatchSequence Sequence, double? TrainingError, Matrix<float>[] Output)> _executionResults = new List<(IMiniBatchSequence, double?, Matrix<float>[])>();
		readonly List<IGraphContext> _contextList = new List<IGraphContext>();
		readonly INode[] _input;
		readonly Random _random;
		float? _lastTestError = null;
		double? _lastTrainingError = null, _trainingErrorDelta = null;

		public TrainingEngine(ILinearAlgebraProvider lap, IDataSource dataSource, ILearningContext learningContext, INode? start) : base(lap)
		{
			_dataSource = dataSource;
            _random = lap.Context.Random;
			LearningContext = learningContext;
			learningContext.SetRowCount(dataSource.RowCount);

			if (start == null) {
				_input = dataSource.InputCount.AsRange().Select(i => (INode)new InputFeeder(i)).ToArray();
				Start = new FlowThrough();
				Start.Output.AddRange(_input.Select(i => new WireToNode(i)));
			} else {
				Start = start;
				_input = start.Output.Select(w => w.SendTo).ToArray();
			}
		}

		public IEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null)
		{
			_lap.PushLayer();
            var provider = new MiniBatchProvider(dataSource, _random);
            using var executionContext = new ExecutionContext(_lap);

            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(batchSize, mb => Execute(executionContext, mb)));
            float operationCount = executionContext.RemainingOperationCount;
            float index = 0f;
            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                operation.Execute(executionContext);
                ClearContextList();
                foreach (var (sequence, _, matrices) in _executionResults) {
                    uint outputIndex = 0;
                    foreach (var output in matrices) {
                        yield return new ExecutionResult(sequence, output.Rows.ToArray(), outputIndex);
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

            _lap.PopLayer();
        }

		protected override IEnumerable<ExecutionResult> GetResults()
		{
            foreach (var (sequence, _, matrices) in _executionResults) {
				uint outputIndex = 0;
				foreach (var output in matrices) {
					yield return new ExecutionResult(sequence, output.Rows.ToArray(), outputIndex);
					++outputIndex;
				}
			}

			_executionResults.Clear();
        }

		protected override void ClearContextList()
		{
			foreach (var item in _contextList)
				item.Dispose();
			_contextList.Clear();
		}

		public double Train(IGraphExecutionContext executionContext, Action<float>? batchCompleteCallback = null)
		{
			_lap.PushLayer();
			LearningContext.StartEpoch();
			var provider = new MiniBatchProvider(_dataSource ?? throw new Exception("No data source was set"), _random);
			executionContext.Add(provider.GetMiniBatches(LearningContext.BatchSize, batch => _contextList.AddRange(Train(executionContext, LearningContext, batch))));

			IGraphOperation? operation;
			float operationCount = executionContext.RemainingOperationCount;
			float index = 0f;
			while ((operation = executionContext.GetNextOperation()) != null) {
				_lap.PushLayer();
				operation.Execute(executionContext);
				LearningContext.ApplyUpdates();
				ClearContextList();
				_lap.PopLayer();

				if (batchCompleteCallback != null) {
					var percentage = (++index) / operationCount;
					batchCompleteCallback(percentage);
				}
			}

			double trainingError = 0;
			if (LearningContext.TrainingErrorCalculation == TrainingErrorCalculation.Fast) {
				var count = 0;
				foreach (var (_, d, _) in _executionResults) {
					if (d.HasValue) {
						trainingError += d.Value;
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

		public IDataSource? DataSource => _dataSource;
		public ILearningContext LearningContext { get; }
		public INode GetInput(uint index) => _input[(int)index];
		public ExecutionGraphModel Graph => Start.GetGraph();
		public ILinearAlgebraProvider LinearAlgebraProvider => _lap;
		public INode Start { get; }

		protected override void Execute(IGraphExecutionContext executionContext, IMiniBatch batch)
		{
			_contextList.AddRange(Train(executionContext, null, batch));
		}

		List<TrainingEngineContext> Train(IGraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatch batch)
		{
			var ret = new List<TrainingEngineContext>();
			if (batch.IsSequential) {
				IMiniBatchSequence? curr;
				while ((curr = batch.GetNextSequence()) != null)
					ret.Add(Train(executionContext, learningContext, curr));

				var contextTable = new Lazy<Dictionary<IMiniBatchSequence, TrainingEngineContext>>(() => ret.ToDictionary(c => c.BatchSequence, c => c));
				var didContinue = Continue(batch, executionContext, sequence => contextTable.Value[sequence]);
				if (didContinue) {
					foreach (var context in ret)
						CompleteSequence(context);
				}
			} else
				ret.Add(Train(executionContext, learningContext, batch.CurrentSequence));
			return ret;
		}

		void CompleteSequence(TrainingEngineContext context)
		{
			_dataSource?.OnBatchProcessed(context);
			var output = context.Output.ToList();
			_executionResults.Add((context.BatchSequence, context.TrainingError, output.Any()
				? output.Select(o => o.GetMatrix().Data).ToArray()
				: new[] { context.Data.GetMatrix().Data }
			));
		}

		TrainingEngineContext Train(IGraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatchSequence sequence)
		{
			var context = new TrainingEngineContext(executionContext, sequence, learningContext);
			Start.ExecuteForward(context, 0);

			while (context.HasNext)
				context.ExecuteNext();

			if (!executionContext.HasContinuations)
				CompleteSequence(context);
			return context;
		}

		static string Write(string name, float score, bool isPercentage)
		{
			if (isPercentage)
				return $"{name}-score: {score:P}";
			return $"{name}-error: {score:N4}";
		}

		public bool Test(
			IDataSource testDataSource,
			IErrorMetric errorMetric,
			uint batchSize = 128,
			Action<float>? batchCompleteCallback = null,
			Action<float, double, bool, bool>? values = null
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

            var msg = new StringBuilder();
			values?.Invoke(testError, _lastTrainingError ?? 0, isPercentage, flag);
			if (LearningContext.CurrentEpoch == 0)
                msg.Append(Write("\rInitial test", testError, isPercentage));
            else {
                var trainingScore = "";
                if (LearningContext.TrainingErrorCalculation != TrainingErrorCalculation.None) {
                    trainingScore = Write(" training", Convert.ToSingle(_lastTrainingError ?? 0), LearningContext.TrainingErrorCalculation == TrainingErrorCalculation.TrainingData && isPercentage);
                    trainingScore += $" [{_trainingErrorDelta:N4}];";
                }

                var testScore = Write("test", testError, isPercentage);
                if (testErrorDelta.HasValue)
                    testScore += $" [{testErrorDelta.Value:N4}]";


                msg.Append($"\rEpoch {LearningContext.CurrentEpoch} -{trainingScore} time: {LearningContext.EpochSeconds:N2}s; {testScore}");
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
	}
}
