using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BrightData;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Engine
{
    class TrainingEngine2 : IGraphTrainingEngine
    {
        readonly GraphFactory _factory;

        public TrainingEngine2(GraphFactory factory, IDataSource dataSource, IErrorMetric errorMetric)
        {
            _factory = factory;
            DataSource = dataSource;
            LinearAlgebraProvider = factory.LinearAlgebraProvider;
            LearningContext = new LearningContext2(this, factory, errorMetric);
            Start = new InputFeeder(0, "engine-input-feeder");
        }

        class LearningContext2 : ILearningContext
        {
            readonly TrainingEngine2 _engine;
            readonly Dictionary<uint, float> _learningRateSchedule = new Dictionary<uint, float>();
            readonly Stack<(IGraphData? Data, Action<IGraphData?> Callback)> _deferredBackpropagation = new Stack<(IGraphData?, Action<IGraphData?>)>();
            readonly List<(IFloatMatrix Error, Action<IFloatMatrix> Updater)> _layerMatrixUpdate = new List<(IFloatMatrix, Action<IFloatMatrix>)>();
            readonly List<(IFloatVector Error, Action<IFloatVector> Updater)> _layerVectorUpdate = new List<(IFloatVector, Action<IFloatVector>)>();
            readonly HashSet<INode> _updatesDisabled = new HashSet<INode>();
            readonly Stopwatch _timer = new Stopwatch();

            public LearningContext2(TrainingEngine2 engine, GraphFactory graphFactory, IErrorMetric errorMetric)
            {
                _engine = engine;
                GraphFactory = graphFactory;
                ErrorMetric = errorMetric;
                CurrentEpoch = 0;
                RowCount = 0;
            }

            public double EpochSeconds => EpochMilliseconds / 1000.0;
            public long EpochMilliseconds => _timer.ElapsedMilliseconds;
            public uint CurrentEpoch { get; private set; }
            public float LearningRate { get; set; }
            public float BatchLearningRate => LearningRate / BatchSize;
            public uint BatchSize { get; set; }
            public uint RowCount { get; private set; }

            public void StoreUpdate(INode fromNode, IFloatMatrix update, Action<IFloatMatrix> updater)
            {
                if (!_updatesDisabled.Contains(fromNode)) {
                    _layerMatrixUpdate.Add((update, updater));
                }
            }

            public void StoreUpdate(INode fromNode, IFloatVector update, Action<IFloatVector> updater)
            {
                if (!_updatesDisabled.Contains(fromNode)) {
                    _layerVectorUpdate.Add((update, updater));
                }
            }

            public IGraphData? ApplyUpdates(IGraphData? gradient)
            {
                var ret = BackpropagateThroughTime(gradient);
                foreach(var (error, updater) in _layerMatrixUpdate)
                    updater(error);
                foreach(var (error, updater) in _layerVectorUpdate)
                    updater(error);
                _layerMatrixUpdate.Clear();
                _layerVectorUpdate.Clear();
                return ret;
            }

            public void StartEpoch()
            {
                BeforeEpochStarts?.Invoke(this);
                if (_learningRateSchedule.TryGetValue(++CurrentEpoch, out float newLearningRate)) {
                    LearningRate = newLearningRate;
                    MessageLog($"Learning rate changed to {newLearningRate}");
                }

                RowCount = 0;
                _timer.Restart();
                _layerVectorUpdate.Clear();
                _layerMatrixUpdate.Clear();
            }

            public void EndEpoch()
            {
                AfterEpochEnds?.Invoke(this);
                ApplyUpdates(null);
                _timer.Stop();
                RowCount = 0;
            }

            public void SetRowCount(uint rowCount)
            {
                RowCount = rowCount;
            }

            public void DeferBackpropagation(IGraphData? errorSignal, Action<IGraphData?> update)
            {
                _deferredBackpropagation.Push((errorSignal, update));
            }

            public IGraphData? BackpropagateThroughTime(IGraphData? signal, int maxDepth = Int32.MaxValue)
            {
                int depth = 0;
                IGraphData? currentSignal = null;
                while (_deferredBackpropagation.Count > 0 && depth < maxDepth) {
                    var (data, callback) = _deferredBackpropagation.Pop();
                    if (depth == 0)
                        callback(signal ?? data);
                    else
                        callback(data ?? currentSignal);
                    if (data != null)
                        currentSignal = data;
                    ++depth;
                }
                _deferredBackpropagation.Clear();
                return currentSignal;
            }

            public void ScheduleLearningRate(uint atEpoch, float newLearningRate)
            {
                _learningRateSchedule[atEpoch] = newLearningRate;
            }

            public void EnableNodeUpdates(INode node, bool enableUpdates)
            {
                if (enableUpdates)
                    _updatesDisabled.Remove(node);
                else
                    _updatesDisabled.Add(node);
            }

            public Action<string> MessageLog { get; set; } = Console.WriteLine;
            public event Action<ILearningContext>? BeforeEpochStarts;
            public event Action<ILearningContext>? AfterEpochEnds;

            // TODO: remove set
            public IErrorMetric ErrorMetric { get; set; }
            public GraphFactory GraphFactory { get; }
        }

        class BackpropagationInput
        {
            readonly ExecutionNode[] _input;
            readonly Dictionary<ExecutionNode, IGraphData?> _error = new Dictionary<ExecutionNode, IGraphData?>();

            public BackpropagationInput(ExecutionNode[] input)
            {
                _input = input;
            }

            public void Add(ExecutionNode node, IGraphData? error)
            {
                _error.Add(node, error);
                if (_error.Count == _input.Length)
                    IsComplete = _input.All(n => _error.ContainsKey(n));
                else if (_error.Count > _input.Length)
                    throw new Exception("Errors do not match input");
            }

            public bool IsComplete { get; private set; } = false;

            public IGraphData? GetError()
            {
                if (_error.Count == 1)
                    return _error.Single().Value;

                IGraphData? ret = null;
                IFloatMatrix? matrix = null;
                var count = 0;
                foreach (var item in _error.Values) {
                    if (item?.HasValue != true) 
                        continue;

                    ret ??= item;
                    if (matrix == null) {
                        matrix = item.GetMatrix();
                        count = 1;
                    }
                    else {
                        matrix.AddInPlace(item.GetMatrix());
                        ++count;
                    }
                }

                if (matrix != null) {
                    if (count > 1)
                        matrix.Multiply(1f / count);
                    return ret!.ReplaceWith(matrix);
                }

                return null;
            }
        }

        class ExecutionNode
        {
            readonly TrainingEngine2 _engine;
            readonly List<ExecutionNode> _ancestors = new List<ExecutionNode>();
            readonly List<ExecutionNode> _descendants = new List<ExecutionNode>();
            readonly Lazy<BackpropagationInput> _inputError;
            ExecutionHistory? _history = null;

            public ExecutionNode(TrainingEngine2 engine)
            {
                _engine = engine;
                _inputError = new Lazy<BackpropagationInput>(() => new BackpropagationInput(_descendants.Any() ? _descendants.ToArray() : new []{this}));
            }

            public void Add(ExecutionHistory history)
            {
                if (_history != null)
                    throw new Exception("History was repeated");
                _history = history;
            }

            public INode? Node => _history?.Source;

            public void AddDescendant(ExecutionNode executionNode)
            {
                _descendants.Add(executionNode);
                executionNode._ancestors.Add(this);
            }

            public void Backpropagate(IGraphSequenceContext context, IGraphData? delta, ExecutionNode fromNode)
            {
                var input = _inputError.Value;
                input.Add(fromNode, delta);

                // if all inputs have been received
                if (input.IsComplete) {
                    var error = input.GetError();
                    if (error != null) {
                        var parents = _ancestors.Select(d => d._history!.Source).ToArray();
                        var ret = _history!.Backpropagation?.Backward(error, context, parents);
                        if(ret != null) {
                            foreach (var (signal, toNode) in ret) {
                                var context2 = (GraphSequenceContext) context;
                                var executionNode = context2.GetExecutionNode(toNode);
                                executionNode.Backpropagate(context, signal, this);
                            }
                        }else foreach(var item in _ancestors)
                            item.Backpropagate(context, error, this);
                    }
                }
            }
        }

        class GraphSequenceContext : IGraphSequenceContext
        {
            readonly TrainingEngine2 _engine;
            readonly List<ExecutionHistory> _pendingForward = new List<ExecutionHistory>();
            readonly Dictionary<INode, ExecutionNode> _nodeExecution = new Dictionary<INode, ExecutionNode>();

            public GraphSequenceContext(
                TrainingEngine2 engine,
                ILearningContext? learningContext, 
                IGraphExecutionContext executionContext,
                IMiniBatchSequence batchSequence)
            {
                _engine = engine;
                LearningContext = learningContext;
                ExecutionContext = executionContext;
                BatchSequence = batchSequence;
                Data = new NullGraphData();
            }

            public void Dispose()
            {
            }

            public INode? Source { get; private set; } = null;
            public IGraphData Data { get; private set; }
            public IGraphExecutionContext ExecutionContext { get; }
            public ILearningContext? LearningContext { get; }
            public ILinearAlgebraProvider LinearAlgebraProvider => ExecutionContext.LinearAlgebraProvider;
            public IMiniBatchSequence BatchSequence { get; }

            public void AddForward(ExecutionHistory action, Func<IBackpropagate>? callback)
            {
                if (callback != null && LearningContext != null)
                    action.Backpropagation = callback();
                _pendingForward.Add(action);

                // add the history to the execution node
                if (!_nodeExecution.TryGetValue(action.Source, out var executionNode))
                    _nodeExecution.Add(action.Source, executionNode = new ExecutionNode(_engine));
                executionNode.Add(action);

                // connect the node to its parents in the graph
                foreach (var parent in action.Parents)
                    _nodeExecution[parent].AddDescendant(executionNode);
            }

            public ExecutionNode GetExecutionNode(INode node) => _nodeExecution[node];

            public void AddBackward(IGraphData? errorSignal, INode target, INode? source)
            {
                throw new NotImplementedException();
                //_backward.Push((error, target, source));
            }

            public void Backpropagate(IGraphData? delta)
            {
                var curr = _nodeExecution[Source ?? throw new Exception("No target node")];
                curr.Backpropagate(this, delta ?? ErrorSignal, curr);

                // initialise backpropagation stack
                //AddBackward(delta, Source ?? throw new Exception("No target node"), null);

                //// backpropagate the error through the graph
                //ErrorSignal = null;
                //while (_backward.Any())
                //{
                //    var next = _backward.Pop();
                //    _errorSignal = GetErrorSignal(next.ErrorSignal, next.Target);

                //    if (_history.TryGetValue(next.Target, out var history))
                //    {
                //        foreach (var item in history)
                //        {
                //            if (item.Backpropagation != null)
                //            {
                //                item.Backpropagation.Backward(next.Source, _errorSignal, this, item.Parents);
                //                item.Backpropagation.Dispose();
                //            }
                //            else
                //            {
                //                foreach (var parent in item.Parents)
                //                    AddBackward(_errorSignal, parent, next.Target);
                //            }
                //        }
                //    }
                //}

                //ErrorSignal = _engine.Backpropagate(delta);
            }

            public IGraphData? ErrorSignal { get; private set; } = null;
            public bool HasNext => _pendingForward.Any();
            public bool ExecuteNext()
            {
                if (HasNext) {
                    var next = _pendingForward.ElementAt(0);
                    _pendingForward.RemoveAt(0);

                    Data = next.Data;
                    Source = next.Source;
                    foreach (var output in next.Source.Output)
                        output.SendTo.ExecuteForward(this, output.Channel);

                    return true;
                }
                return false;
            }

            public void SetOutput(IGraphData data, int channel = 0)
            {
                throw new NotImplementedException();
            }

            public IGraphData? GetOutput(int channel = 0)
            {
                throw new NotImplementedException();
            }

            public IGraphData[] Output { get; } = new IGraphData[0];
            public ExecutionResult Result 
            {
                get
                {
                    var matrixOutput = Output.Any()
                        ? Output.Select(o => o.GetMatrix().Data)
                        : new[] {Data.GetMatrix().Data};

                    return new ExecutionResult(BatchSequence, matrixOutput.SelectMany(m => m.Rows).ToArray());
                }
            }
        }

        float? _lastTestError = null;

        public IGraphSequenceContext Create(IGraphExecutionContext executionContext, IMiniBatchSequence sequence)
        {
            return new GraphSequenceContext(this, LearningContext, executionContext, sequence);
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
                foreach (var context in operation.Execute(executionContext)) {
                    yield return context.Result;
                    context.Dispose();
                }

                //foreach (var (context, data) in _executionResults) {
                //    uint outputIndex = 0;
                //    foreach (var output in data) {
                //        ret.Add(new ExecutionResult(context.BatchSequence, output.AsIndexable().Rows.Select(r => r.Data).ToArray(), outputIndex));
                //        ++outputIndex;
                //    }
                //    context.Dispose();
                //    foreach (var matrix in data)
                //        matrix.Dispose();
                //}
                //_executionResults.Clear();
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

        public INode Start { get; }

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
                var contextList = operation.Execute(executionContext).ToList();
                LearningContext.ApplyUpdates(null);
                foreach(var context in contextList)
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
                    additionalResults.AddRange(Continue(batch, executionContext, sequence => contextTable[sequence]));

                foreach(var result in additionalResults)
                    yield return result;
            }
            else
                yield return Train(executionContext, learningContext, batch.CurrentSequence);
        }

        IEnumerable<IGraphSequenceContext> Continue(IMiniBatch batch, IGraphExecutionContext executionContext, Func<IMiniBatchSequence, IGraphSequenceContext> lookupContext)
        {
            while (executionContext.HasContinuations) {
                var additionalContext = new List<(IGraphSequenceContext Context, Action<IGraphSequenceContext[]> OnEnd)>();
                foreach (var item in executionContext.ExecuteAdditional())
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
                    while (context.HasNext)
                        context.ExecuteNext();
                    yield return context;
                }
            }
        }

        GraphSequenceContext Train(IGraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatchSequence sequence)
        {
            var context = new GraphSequenceContext(this, learningContext, executionContext, sequence);
            Start.ExecuteForward(context, 0);

            while (context.HasNext)
                context.ExecuteNext();
            return context;
        }

        public bool Test(IDataSource testDataSource, IErrorMetric errorMetric, uint batchSize = 128, Action<float>? batchCompleteCallback = null, Action<float, bool, bool>? values = null)
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

        void LoadParamaters(GraphFactory factory, ExecutionGraphModel.Node nodeModel)
		{
			var node = Start.FindById(nodeModel.Id);
			node?.LoadParameters(factory, nodeModel);
		}

        IGraphData? BackpropagateThroughTime(IGraphData? signal, int maxDepth)
        {
            throw new NotImplementedException();
        }

        void DeferBackpropagation(IGraphData? errorSignal, Action<IGraphData?> update)
        {
            throw new NotImplementedException();
        }

        IGraphData? ApplyUpdates(IGraphData? gradient)
        {
            throw new NotImplementedException();
        }

        void StoreUpdate<T>(INode fromNode, T update, Action<T> updater) where T : notnull
        {
            throw new NotImplementedException();
        }
    }
}
