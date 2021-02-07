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
    internal class TrainingEngine : EngineBase<TrainingEngineContext>, IGraphTrainingEngine
	{
        readonly INode[] _input;
		readonly Random _random;
		float? _lastTestError = null;
        bool _isTraining;

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
            _isTraining = false;
			_lap.PushLayer();
            var provider = new MiniBatchProvider(dataSource, _random);
            using var executionContext = new ExecutionContext(_lap, this);

            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(batchSize, mb => Execute(executionContext, mb)));
            float operationCount = executionContext.RemainingOperationCount;
            float index = 0f;
            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                foreach(var result in operation.Execute(executionContext))
                    yield return result;
                //foreach (var (sequence, matrices) in _executionResults) {
                //    uint outputIndex = 0;
                //    foreach (var output in matrices) {
                //        yield return new ExecutionResult(sequence, output.Rows.ToArray(), outputIndex);
                //        ++outputIndex;
                //    }
                //}

                _lap.PopLayer();

                if (batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            _lap.PopLayer();
        }

		//protected override IEnumerable<ExecutionResult> GetResults()
		//{
  //          foreach (var (sequence, matrices) in _executionResults) {
		//		uint outputIndex = 0;
		//		foreach (var output in matrices) {
		//			yield return new ExecutionResult(sequence, output.Rows.ToArray(), outputIndex);
		//			++outputIndex;
		//		}
		//	}

		//	_executionResults.Clear();
  //      }

        public override TrainingEngineContext CreateContext(IGraphExecutionContext executionContext, IMiniBatchSequence sequence) =>
            new TrainingEngineContext(executionContext, sequence, _isTraining ? LearningContext : null);

        public override IGraphEngine GraphEngine => this;

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
				operation.Execute(executionContext);
				LearningContext.ApplyUpdates();
				ClearContextList();
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

		public IDataSource? DataSource => _dataSource;
		public ILearningContext LearningContext { get; }
		public INode GetInput(uint index) => _input[(int)index];
		public ExecutionGraphModel Graph => Start.GetGraph();
		public ILinearAlgebraProvider LinearAlgebraProvider => _lap;
		public INode Start { get; }

		protected override IEnumerable<ExecutionResult> Execute(IGraphExecutionContext executionContext, IMiniBatch batch)
		{
            _isTraining = false;
			_contextList.AddRange(Train(executionContext, null, batch));
		}

		IEnumerable<ExecutionResult> Train(IGraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatch batch)
        {
            _isTraining = learningContext != null;
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

			if (!executionContext.HasContinuations)
				CompleteSequence(context);
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
	}
}
