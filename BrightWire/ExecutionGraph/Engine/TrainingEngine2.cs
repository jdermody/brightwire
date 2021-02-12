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
            LearningContext = new LearningContext2(factory, errorMetric);
            Start = new InputFeeder(0, "engine-input-feeder");
        }

        class LearningContext2 : ILearningContext
        {
            readonly Dictionary<uint, float> _learningRateSchedule = new Dictionary<uint, float>();
            readonly Stack<(IGraphData? Data, Func<IGraphData?, IGraphData?> Callback)> _deferredBackpropagation = new Stack<(IGraphData?, Func<IGraphData?, IGraphData?>)>();
            readonly List<(INode Node, IFloatMatrix Error, Action<IFloatMatrix> Updater)> _layerMatrixUpdate = new List<(INode, IFloatMatrix, Action<IFloatMatrix>)>();
            readonly List<(INode Node, IFloatVector Error, Action<IFloatVector> Updater)> _layerVectorUpdate = new List<(INode, IFloatVector, Action<IFloatVector>)>();
            readonly HashSet<INode> _updatesDisabled = new HashSet<INode>();
            readonly Stopwatch _timer = new Stopwatch();

            public LearningContext2(GraphFactory graphFactory, IErrorMetric errorMetric)
            {
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
                    _layerMatrixUpdate.Add((fromNode, update, updater));
                }

                //var groups = _layerMatrixUpdate.Where(d => d.Node == fromNode).GroupBy(d => (d.Node, d.Error.RowCount));
                //foreach (var group in groups) {
                //    var count = group.Count();
                //    if (count > 1) {
                //        int i = 0;
                //    }
                //}
            }

            public void StoreUpdate(INode fromNode, IFloatVector update, Action<IFloatVector> updater)
            {
                if (!_updatesDisabled.Contains(fromNode)) {
                    _layerVectorUpdate.Add((fromNode, update, updater));
                }
            }

            void Update<T>(
                List<(INode Node, T Error, Action<T> Updater)> updates, 
                Func<T, uint> getSize,
                Func<T, T> cloner, 
                Action<T, T> addInPlace, 
                Action<int, T> divider
            ) where T: class
            {
                //foreach (var nodeGroup in updates.GroupBy(d => (d.Node, getSize(d.Error)))) {
                //    T? update = null;
                //    Action<T>? updater = null;
                //    int count = 0;
                //    foreach (var item in nodeGroup) {
                //        if (update == null) {
                //            updater = item.Updater;
                //            update = item.Error;
                //            count = 1;
                //        }
                //        else {
                //            if (count == 1)
                //                update = cloner(update!);
                //            addInPlace(update, item.Error);
                //            ++count;
                //        }
                //    }

                //    if (count > 1)
                //        divider(count, update!);
                //    if (update != null)
                //        updater!(update);
                //}

                foreach(var (fromNode, error, updater) in _layerMatrixUpdate)
                    updater(error);
                updates.Clear();
            }

            public void ApplyUpdates()
            {
                Update(_layerMatrixUpdate, m => m.RowCount, m => m.Clone(), (m, m2) => m.AddInPlace(m2), (m, c) => c.Multiply(1f / m));
                Update(_layerVectorUpdate, m => m.Count, m => m.Clone(), (m, m2) => m.AddInPlace(m2), (m, c) => c.Multiply(1f / m));
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
                _timer.Stop();
                RowCount = 0;
            }

            public void SetRowCount(uint rowCount)
            {
                RowCount = rowCount;
            }

            public void DeferBackpropagation(IGraphData? errorSignal, Func<IGraphData?, IGraphData?> update)
            {
                _deferredBackpropagation.Push((errorSignal, update));
            }

            public IGraphData? BackpropagateThroughTime(IGraphData? signalOverride, int maxDepth = Int32.MaxValue)
            {
                int depth = 0;
                IGraphData? lastError = null;
                while (_deferredBackpropagation.Count > 0 && depth < maxDepth) {
                    var (data, callback) = _deferredBackpropagation.Pop();
                    var error = callback(signalOverride ?? data ?? lastError);
                    if (error?.HasValue == true)
                        lastError = error;
                    ++depth;
                }
                _deferredBackpropagation.Clear();
                return lastError;
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
            readonly IGraphData? _nodeOutput;

            public BackpropagationInput(ExecutionHistory? history, ExecutionNode[] input)
            {
                _nodeOutput = history?.Data;
                _input = input;
            }

            public void Add(ExecutionNode fromNode, IGraphData? error)
            {
                if (error?.HasValue == true && _nodeOutput?.HasValue == true) {
                    if (error.Columns != _nodeOutput.Columns || error.Count != _nodeOutput.Count || error.Depth != _nodeOutput.Depth || error.Rows != _nodeOutput.Rows)
                        throw new ArgumentException("Unexpected delta size");
                }

                _error.Add(fromNode, error);
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
            readonly List<ExecutionNode> _ancestors = new List<ExecutionNode>();
            readonly List<ExecutionNode> _descendants = new List<ExecutionNode>();
            readonly Lazy<BackpropagationInput> _inputError;
            ExecutionHistory? _history = null;

            public ExecutionNode()
            {
                _inputError = new Lazy<BackpropagationInput>(() => new BackpropagationInput(_history, _descendants.Any() ? _descendants.ToArray() : new []{this}));
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

            public IEnumerable<IGraphData> Backpropagate(IGraphSequenceContext context, IGraphData? delta, ExecutionNode fromNode)
            {
                var input = _inputError.Value;
                input.Add(fromNode, delta);

                // if all inputs have been received
                if (input.IsComplete) {
                    var error = input.GetError();
                    if (error != null) {
                        var parents = _ancestors.Select(d => d._history!.Source).ToArray();
                        var sendTo = _history!.Backpropagation?.Backward(error, context, parents);
                        if(sendTo != null) {
                            foreach (var (signal, toNode) in sendTo) {
                                var context2 = (GraphSequenceContext) context;
                                var executionNode = context2.GetExecutionNode(toNode);
                                foreach (var ret in executionNode.Backpropagate(context, signal, this))
                                    yield return ret;
                            }
                        }
                        else {
                            foreach (var item in _ancestors) {
                                foreach(var ret in item.Backpropagate(context, error, this))
                                    yield return ret;
                            }
                        }

                        if (!_ancestors.Any())
                            yield return error;
                    }
                }
            }

            public override string ToString() => $"{Node} ({_ancestors.Count:N0} ancestors, {_descendants.Count:N0} descendants)";
        }

        class GraphSequenceContext : IGraphSequenceContext, ICanTrace
        {
            readonly List<ExecutionHistory> _pendingForward = new List<ExecutionHistory>();
            readonly Dictionary<INode, ExecutionNode> _nodeExecution = new Dictionary<INode, ExecutionNode>();

            public GraphSequenceContext(
                ILearningContext? learningContext, 
                IGraphExecutionContext executionContext,
                IMiniBatchSequence batchSequence)
            {
                LearningContext = learningContext;
                ExecutionContext = executionContext;
                BatchSequence = batchSequence;
                Data = NullGraphData.Instance;
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
                    _nodeExecution.Add(action.Source, executionNode = new ExecutionNode());
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

            public IGraphData? Backpropagate(IGraphData? delta)
            {
                var curr = _nodeExecution[Source ?? throw new Exception("No target node")];
                var errors = curr.Backpropagate(this, delta, curr).ToList();
                ErrorSignal = errors.Single();
                return ErrorSignal;
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

                    var ret = new ExecutionResult(BatchSequence, matrixOutput.SelectMany(m => m.Rows).ToArray());
                    if (ErrorSignal != null)
                        ret.Error = ErrorSignal.GetMatrix().Data.Rows.ToArray();
                    return ret;
                }
            }

            public string Trace()
            {
                var target = BatchSequence.Target;
                var output = Data;
                var error = ErrorSignal;
                var ret = new StringBuilder();

                if (target?.HasValue == true && output.HasValue && error?.HasValue == true) {
                    var t = target.GetMatrix().ReshapeAsVector().Data;
                    var o = output.GetMatrix().ReshapeAsVector().Data;
                    var e = error.GetMatrix().ReshapeAsVector().Data;
                    if (t.Size == o.Size && o.Size == e.Size) {
                        for (uint i = 0, len = t.Size; i < len; i++) {
                            ret.AppendLine($"{i}) output: {o[i]}, target: {t[i]}, error: {e[i]}");
                        }
                    }
                }

                return ret.ToString();
            }
        }

        float? _lastTestError = null;

        public IGraphSequenceContext Create(IGraphExecutionContext executionContext, IMiniBatchSequence sequence, ILearningContext? learningContext)
        {
            return new GraphSequenceContext(learningContext, executionContext, sequence);
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
                LearningContext.BackpropagateThroughTime(null);
                LearningContext.ApplyUpdates();
                foreach (var context in contextList) {
                    var result = context.Result;
                    context.Dispose();
                }
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
                foreach (var item in executionContext.ExecuteAdditional(learningContext))
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

        IGraphSequenceContext Train(IGraphExecutionContext executionContext, ILearningContext? learningContext, IMiniBatchSequence sequence)
        {
            var context = Create(executionContext, sequence, learningContext);
            Start.ExecuteForward(context, 0);

            while (context.HasNext)
                context.ExecuteNext();
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
