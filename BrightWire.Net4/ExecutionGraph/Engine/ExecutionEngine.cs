using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using System.Runtime.Serialization;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Engine
{
    class ExecutionEngine : IGraphEngine
    {
        class Context : IContext
        {
            readonly IExecutionContext _executionContext;
            readonly IMiniBatchSequence _miniBatch;
            readonly List<IExecutionHistory> _forward = new List<IExecutionHistory>();
            INode _sourceNode = null;
            IGraphData _data;

            public Context(IExecutionContext executionContext, IMiniBatchSequence miniBatch)
            {
                _executionContext = executionContext;
                _miniBatch = miniBatch;
                _data = new MatrixGraphData(miniBatch.Input);
            }

            public void Dispose()
            {
                foreach (var item in _forward)
                    item.Data.Release();
                _data?.Release();
            }

            public bool IsTraining => false;
            public INode Source => _sourceNode;
            public IExecutionContext ExecutionContext => _executionContext;
            public ILearningContext LearningContext => null;
            public ILinearAlgebraProvider LinearAlgebraProvider => _executionContext.LinearAlgebraProvider;
            public IMiniBatchSequence BatchSequence => _miniBatch;
            public void AddBackward(IGraphData errorSignal, INode target, INode source) => throw new NotImplementedException();
            public void Backpropagate(IGraphData delta) => throw new NotImplementedException();
            public void AppendErrorSignal(IGraphData errorSignal, INode forNode) => throw new NotImplementedException();
            public void AddForward(IExecutionHistory action, Func<IBackpropagation> callback) => _forward.Add(action);
            public IGraphData ErrorSignal => throw new NotImplementedException();
            public bool HasNext => _forward.Any();
            public IGraphData Data => _data;

            public bool ExecuteNext()
            {
                if (HasNext) {
                    var next = _forward.ElementAt(0);
                    _forward.RemoveAt(0);

                    _data?.Release();
                    _data = next.Data;

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
        readonly List<(Context Context, IMatrix Data)> _executionResults = new List<(Context, IMatrix)>();
        readonly ILinearAlgebraProvider _lap;
        IDataSource _dataSource = null;
        readonly INode _input;

        public ExecutionEngine(ILinearAlgebraProvider lap, Models.ExecutionGraph graph, INode input)
        {
            _lap = lap;
            _graph = graph;
            _input = input;
        }

        public Models.ExecutionGraph Graph => _graph;
        public IDataSource DataSource => _dataSource;
        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;
        public INode Input => _input;

        public IReadOnlyList<ExecutionResult> Execute(IDataSource dataSource, int batchSize = 128)
        {
            _lap.PushLayer();
            _dataSource = dataSource;
            var ret = new List<ExecutionResult>();
            var provider = new MiniBatchProvider(dataSource, false);
            using (var executionContext = new ExecutionContext(_lap)) {
                executionContext.Add(provider.GetMiniBatches(batchSize, mb => _Execute(executionContext, mb)));

                IGraphOperation operation;
                while ((operation = executionContext.GetNextOperation()) != null) {
                    operation.Execute(executionContext);
                }

                foreach (var item in _executionResults) {
                    ret.Add(new ExecutionResult(item.Context.BatchSequence, item.Data.AsIndexable().Rows.Select(r => r.Data).ToList()));
                    item.Context.Dispose();
                    item.Data?.Dispose();
                }
            }
            _lap.PopLayer();
            _executionResults.Clear();
            _dataSource = null;
            return ret;
        }

        void _Execute(IExecutionContext executionContext, IMiniBatch batch)
        {
            if (batch.IsSequential) {
                IMiniBatchSequence curr = null;
                while ((curr = batch.GetNextSequence()) != null) {
                    var context = new Context(executionContext, curr);
                    _input.ExecuteForward(context, 0);
                    while (context.HasNext)
                        context.ExecuteNext();
                    _executionResults.Add((context, context.Data.GetMatrix()));
                }
            } else {
                var context = new Context(executionContext, batch.CurrentSequence);
                _input.ExecuteForward(context, 0);

                while (context.HasNext)
                    context.ExecuteNext();

                _executionResults.Add((context, context.Data.GetMatrix()));
            }
        }
    }
}
