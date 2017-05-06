using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Input;
using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Engine
{
    class TrainingEngine : IGraphTrainingEngine
    {
        class Context : IContext
        {
            readonly TrainingEngine _engine;
            readonly IMiniBatchSequence _miniBatch;
            readonly ILearningContext _learningContext;
            readonly List<IExecutionHistory> _pending = new List<IExecutionHistory>();
            readonly Dictionary<INode, List<(INode Parent, IExecutionHistory)>> _history = new Dictionary<INode, List<(INode Parent, IExecutionHistory)>>();
            IGraphData _data;
            INode _parentNode;
            IMatrix _output, _target;
            double _trainingError = 0;

            public Context(TrainingEngine engine, IMiniBatchSequence miniBatch, ILearningContext learningContext)
            {
                _engine = engine;
                _miniBatch = miniBatch;
                _learningContext = learningContext;
                _data = new MatrixGraphData(miniBatch.Input);
            }

            public bool IsTraining => _learningContext != null;
            public IGraphData Data => _data;
            public ILinearAlgebraProvider LinearAlgebraProvider => _engine._lap;
            public IExecutionContext ExecutionContext => _engine._executionContext;
            public ILearningContext LearningContext => _learningContext;
            public IMiniBatchSequence BatchSequence => _miniBatch;
            public bool HasNext => _pending.Any();
            public IMatrix Output => _output;
            public IMatrix Target => _target;
            public double TrainingError => _trainingError;

            public void Add(IExecutionHistory action, Func<IBackpropagation> callback)
            {
                if(callback != null && IsTraining)
                    action.Backpropagation = callback();
                _pending.Add(action);

                List<(INode, IExecutionHistory)> temp;
                if (!_history.TryGetValue(action.Source, out temp))
                    _history.Add(action.Source, temp = new List<(INode, IExecutionHistory)>());
                temp.Add((_parentNode, action));
            }

            public bool ExecuteNext()
            {
                if(HasNext) {
                    var next = _pending.ElementAt(0);
                    _pending.RemoveAt(0);

                    _data = next.Data;
                    _parentNode = next.Source;
                    if (next.Source.Output != null) {
                        foreach (var output in next.Source.Output) {
                            if (output.IsPrimary)
                                output.SendTo?.SetPrimaryInput(this);
                            else
                                output.SendTo?.SetSecondaryInput(this);
                        }
                    }

                    return true;
                }
                return false;
            }

            public void Backpropagate(IMatrix output, IMatrix target, IMatrix delta)
            {
                // store the output and the target
                _output = output;
                _target = target;

                // calculate training error
                if(_learningContext?.CalculateTrainingError == true)
                    _trainingError = Math.Sqrt(delta.AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average());

                // backpropagate the error through the graph
                var stack = new Stack<(INode Parent, IMatrix Delta)>();
                stack.Push((_parentNode, delta));
                while(stack.Any()) {
                    var next = stack.Pop();
                    foreach(var item in _history[next.Parent]) {
                        var errorSignal = next.Delta;
                        if(item.Item2.Backpropagation != null)
                            errorSignal = item.Item2.Backpropagation.Backward(errorSignal, this, item.Parent != null);

                        if(item.Parent != null)
                            stack.Push((item.Parent, errorSignal));
                    }
                }
            }
        }

        readonly ExecutionContext _executionContext;
        readonly ILinearAlgebraProvider _lap;
        readonly IDataSource _dataSource;
        readonly List<Context> _executionResults = new List<Context>();
        readonly ILearningContext _learningContext;
        readonly INode _input;
        readonly bool _isStochastic;
        float? _lastTestError = null;
        double? _lastTrainingError = null, _trainingErrorDelta = null;
        int _noImprovementCount = 0;

        public TrainingEngine(ILinearAlgebraProvider lap, IDataSource dataSource, bool isStochastic, Func<ILearningContext> createLearningContext)
        {
            _lap = lap;
            _dataSource = dataSource;
            _isStochastic = isStochastic;
            _executionContext = new ExecutionContext(lap);
            _learningContext = createLearningContext();
            _input = new FlowThrough();
        }

        public IReadOnlyList<(IIndexableVector Output, IIndexableVector TargetOutput)> Execute(IDataSource dataSource, int batchSize)
        {
            var provider = new MiniBatchProvider(dataSource, _lap, _isStochastic);
            _executionContext.Add(provider.GetMiniBatches(batchSize, _Execute));

            IGraphOperation operation;
            while ((operation = _executionContext.GetNextOperation()) != null) {
                operation.Execute();
            }

            var ret = new List<(IIndexableVector Output, IIndexableVector TargetOutput)>();
            foreach (var item in _executionResults)
                ret.AddRange(item.Output.AsIndexable().Rows.Zip(item.Target.AsIndexable().Rows, (o, t) => (o, t)));

            _executionResults.Clear();
            return ret;
        }

        public double Train()
        {
            _learningContext.StartEpoch();
            var provider = new MiniBatchProvider(_dataSource, _lap, _isStochastic);
            _executionContext.Add(provider.GetMiniBatches(_learningContext.BatchSize, batch => _Train(_learningContext, batch)));

            IGraphOperation operation;
            while ((operation = _executionContext.GetNextOperation()) != null) {
                operation.Execute();
                _learningContext.ApplyUpdates();
            }

            double ret = 0, count = 0;
            foreach (var item in _executionResults) {
                ret += item.TrainingError;
                ++count;
            }
            if (count > 0)
                ret /= count;
            _executionResults.Clear();
            _lastTrainingError = ret;
            if (_lastTrainingError.HasValue) {
                _trainingErrorDelta = ret - _lastTrainingError.Value;
            }
            _learningContext.EndEpoch();
            return ret;
        }

        public IExecutionContext ExecutionContext => _executionContext;
        public IDataSource DataSource => _dataSource;
        public ILearningContext LearningContext => _learningContext;
        public INode Input => _input;

        void _Execute(IMiniBatch batch)
        {
            _Train(null, batch);
        }

        void _Train(ILearningContext learningContext, IMiniBatch batch)
        {
            if (batch.IsSequential) {

            } else {
                var context = new Context(this, batch.CurrentSequence, learningContext);
                _input.SetPrimaryInput(context);

                while (context.HasNext)
                    context.ExecuteNext();

                _executionResults.Add(context);
            }
        }

        public void WriteTestResults(IDataSource testDataSource, IErrorMetric errorMetric, int batchSize = 128)
        {
            var testError = errorMetric.Compute(Execute(testDataSource, batchSize)).Average();
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

            var format = isPercentage
                ? "Epoch: {0}; t-error: {1:N4} [{2:N4}]; time: {3:N2}s; score: {4:P}"
                : "Epoch: {0}; t-error: {1:N4} [{2:N4}]; time: {3:N2}s; score: {4:N4}"
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
        }
    }
}
