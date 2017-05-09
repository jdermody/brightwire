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
            readonly LearningContext _learningContext;
            readonly List<IExecutionHistory> _forward = new List<IExecutionHistory>();
            readonly Stack<(IMatrix, INode)> _backward = new Stack<(IMatrix, INode)>();
            readonly Dictionary<INode, List<IExecutionHistory>> _history = new Dictionary<INode, List<IExecutionHistory>>();
            IGraphData _data;
            INode _sourceNode;
            IMatrix _output, _target;
            double _trainingError = 0;

            public Context(TrainingEngine engine, IMiniBatchSequence miniBatch, LearningContext learningContext)
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
            public bool HasNext => _forward.Any();
            public IMatrix Output => _output;
            public IMatrix Target => _target;
            public double TrainingError => _trainingError;
            public INode Source => _sourceNode;

            public void Forward(IExecutionHistory action, Func<IBackpropagation> callback)
            {
                if(callback != null && IsTraining)
                    action.Backpropagation = callback();
                _forward.Add(action);

                List<IExecutionHistory> temp;
                if (!_history.TryGetValue(action.Source, out temp))
                    _history.Add(action.Source, temp = new List<IExecutionHistory>());
                temp.Add(action);
            }

            public void Backward(IMatrix error, INode source)
            {
                _backward.Push((error, source));
            }
            
            public bool ExecuteNext()
            {
                if(HasNext) {
                    var next = _forward.ElementAt(0);
                    _forward.RemoveAt(0);

                    _data = next.Data;
                    _sourceNode = next.Source;
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

            public void StartBackpropagation(IMatrix output, IMatrix target, IMatrix delta)
            {
                // store the output and the target
                _output = output;
                _target = target;

                // calculate training error
                if(_learningContext?.CalculateTrainingError == true)
                    _trainingError = Math.Sqrt(delta.AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average());

                // initialise backpropagation stack
                _backward.Clear();
                _backward.Push((delta, _sourceNode));

                // backpropagate the error through the graph
                List<IExecutionHistory> history;
                while (_backward.Any()) {
                    var next = _backward.Pop();
                    var errorSignal = next.Item1;
                    if (next.Item2 != null && _history.TryGetValue(next.Item2, out history)) {
                        foreach (var item in history) {
                            if (item.Backpropagation != null)
                                item.Backpropagation.Backward(errorSignal, this, item.Parents);
                            else {
                                foreach (var parent in item.Parents)
                                    _backward.Push((errorSignal, parent));
                            }
                        }
                    }
                }
            }
        }

        readonly ExecutionContext _executionContext;
        readonly ILinearAlgebraProvider _lap;
        readonly IDataSource _dataSource;
        readonly List<Context> _executionResults = new List<Context>();
        readonly LearningContext _learningContext;
        readonly INode _input;
        readonly bool _isStochastic;
        float? _lastTestError = null;
        double? _lastTrainingError = null, _trainingErrorDelta = null;
        int _noImprovementCount = 0;

        public TrainingEngine(ILinearAlgebraProvider lap, IDataSource dataSource, bool isStochastic, LearningContext learningContext)
        {
            _lap = lap;
            _dataSource = dataSource;
            _isStochastic = isStochastic;
            _executionContext = new ExecutionContext(lap);
            _learningContext = learningContext;
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

        void _Train(LearningContext learningContext, IMiniBatch batch)
        {
            if (batch.IsSequential) {
                IMiniBatchSequence curr = null;
                while ((curr = batch.GetNextSequence()) != null) {
                    var context = new Context(this, curr, learningContext);
                    _input.SetPrimaryInput(context);
                    while (context.HasNext)
                        context.ExecuteNext();
                    _executionResults.Add(context);
                }
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
