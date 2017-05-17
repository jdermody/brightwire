using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using System.Runtime.Serialization;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Input;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Engine
{
    class ExecutionEngine : IGraphEngine
    {
        class Context : IContext
        {
            readonly ExecutionEngine _engine;
            readonly IMiniBatchSequence _miniBatch;
            readonly List<IExecutionHistory> _forward = new List<IExecutionHistory>();
            INode _sourceNode = null;
            IMatrix _output = null;

            public Context(ExecutionEngine engine, IMiniBatchSequence miniBatch)
            {
                _engine = engine;
                _miniBatch = miniBatch;
                _engine._executionContext.Data = new MatrixGraphData(miniBatch.Input);
            }

            public bool IsTraining => false;
            public INode Source => _sourceNode;
            public IExecutionContext ExecutionContext => _engine._executionContext;
            public ILearningContext LearningContext => null;
            public ILinearAlgebraProvider LinearAlgebraProvider => _engine._lap;
            public IMiniBatchSequence BatchSequence => _miniBatch;
            public void AddBackward(IGraphData errorSignal, INode target, INode source) => throw new NotImplementedException();
            public void Backpropagate(IGraphData delta) => throw new NotImplementedException();
            public void AppendErrorSignal(IGraphData errorSignal, INode forNode) => throw new NotImplementedException();
            public void AddForward(IExecutionHistory action, Func<IBackpropagation> callback) => _forward.Add(action);
            public IGraphData ErrorSignal => throw new NotImplementedException();
            public bool HasNext => _forward.Any();
            public IGraphData Data => _engine._executionContext.Data;
            public IMatrix Output { get => _output; set => _output = value; }

            public bool ExecuteNext()
            {
                if (HasNext) {
                    var next = _forward.ElementAt(0);
                    _forward.RemoveAt(0);

                    _engine._executionContext.Data = next.Data;
                    _sourceNode = next.Source;
                    if (next.Source.Output != null) {
                        foreach (var output in next.Source.Output)
                            output.SendTo?.ExecuteForward(this, output.Channel);
                    }

                    return true;
                }
                return false;
            }
        }
        readonly Models.ExecutionGraph _graph;
        readonly IExecutionContext _executionContext;
        readonly List<Context> _executionResults = new List<Context>();
        readonly GraphFactory _factory;
        readonly ILinearAlgebraProvider _lap;
        IDataSource _dataSource = null;
        readonly INode _input;

        public ExecutionEngine(ILinearAlgebraProvider lap, Models.ExecutionGraph graph, IExecutionContext executionContext)
        {
            _lap = lap;
            _factory = new GraphFactory(lap);
            _graph = graph;
            _executionContext = executionContext;
            _input = _factory.CreateFrom(graph);
        }

        public Models.ExecutionGraph Graph => _graph;
        public IDataSource DataSource => _dataSource;
        public INode Input => _input;

        public IReadOnlyList<ExecutionResult> Execute(IDataSource dataSource, int batchSize = 128)
        {
            _dataSource = dataSource;
            var provider = new MiniBatchProvider(dataSource, false);
            _executionContext.Add(provider.GetMiniBatches(batchSize, _Execute));

            IGraphOperation operation;
            while ((operation = _executionContext.GetNextOperation()) != null) {
                operation.Execute();
            }

            var ret = new List<ExecutionResult>();
            foreach (var item in _executionResults)
                ret.Add(new ExecutionResult(item.BatchSequence, item.Output.AsIndexable().Rows.ToList()));

            _executionResults.Clear();
            _dataSource = null;
            return ret;
        }

        void _Execute(IMiniBatch batch)
        {
            if (batch.IsSequential) {
                IMiniBatchSequence curr = null;
                while ((curr = batch.GetNextSequence()) != null) {
                    var context = new Context(this, curr);
                    _input.ExecuteForward(context, 0);
                    while (context.HasNext)
                        context.ExecuteNext();
                    _executionResults.Add(context);
                }
            } else {
                var context = new Context(this, batch.CurrentSequence);
                _input.ExecuteForward(context, 0);

                while (context.HasNext)
                    context.ExecuteNext();

                _executionResults.Add(context);
            }
        }
    }
}
