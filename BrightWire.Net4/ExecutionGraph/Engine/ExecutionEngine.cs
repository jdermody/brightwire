using System.Collections.Generic;
using System.Linq;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Helper;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.Models;
using System;

namespace BrightWire.ExecutionGraph.Engine
{
    /// <summary>
    /// Executes (without training) graphs
    /// </summary>
    class ExecutionEngine : IGraphEngine
    {
        readonly Models.ExecutionGraph _graph;
        readonly List<(ExecutionEngineContext Context, IMatrix Data)> _executionResults = new List<(ExecutionEngineContext, IMatrix)>();
        readonly ILinearAlgebraProvider _lap;
        IDataSource _dataSource = null;
        readonly INode _start;

        public ExecutionEngine(ILinearAlgebraProvider lap, Models.ExecutionGraph graph, INode start)
        {
            _lap = lap;
            _graph = graph;
            _start = start;
        }

        public INode Start => _start;
        public Models.ExecutionGraph Graph => _graph;
        public IDataSource DataSource => _dataSource;
        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;

        public IReadOnlyList<ExecutionResult> Execute(IDataSource dataSource, int batchSize = 128, Action<float> batchCompleteCallback = null)
        {
            _lap.PushLayer();
            _dataSource = dataSource;
            var ret = new List<ExecutionResult>();
            var provider = new MiniBatchProvider(dataSource, false);
            using (var executionContext = new ExecutionContext(_lap)) {
                executionContext.Add(provider.GetMiniBatches(batchSize, mb => _Execute(executionContext, mb)));
                float operationCount = executionContext.RemainingOperationCount;
                float index = 0f;

                IGraphOperation operation;
                while ((operation = executionContext.GetNextOperation()) != null) {
                    _lap.PushLayer();
                    operation.Execute(executionContext);
                    foreach (var item in _executionResults) {
                        ret.Add(new ExecutionResult(item.Context.BatchSequence, item.Data.AsIndexable().Rows.Select(r => r.Data).ToList()));
                        item.Context.Dispose();
                        item.Data?.Dispose();
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
            _dataSource = null;
            return ret;
        }

        public ExecutionResult Execute(float[] input)
        {
            _lap.PushLayer();
            ExecutionResult ret = null;
            var provider = new MiniBatchProvider(new SingleRowDataSource(input, false, MiniBatchSequenceType.Standard, 0), false);
            using (var executionContext = new ExecutionContext(_lap)) {
                executionContext.Add(provider.GetMiniBatches(1, mb => _Execute(executionContext, mb)));

                IGraphOperation operation;
                while ((operation = executionContext.GetNextOperation()) != null) {
                    _lap.PushLayer();
                    operation.Execute(executionContext);
                    foreach (var item in _executionResults) {
                        ret = new ExecutionResult(item.Context.BatchSequence, item.Data.AsIndexable().Rows.Select(r => r.Data).ToList());
                        item.Context.Dispose();
                        item.Data?.Dispose();
                    }
                    _executionResults.Clear();
                    _lap.PopLayer();
                }
            }
            _lap.PopLayer();
            _dataSource = null;
            return ret;
        }

        public ExecutionResult ExecuteSequential(int sequenceIndex, float[] input, IExecutionContext executionContext, MiniBatchSequenceType sequenceType)
        {
            _lap.PushLayer();
            ExecutionResult ret = null;
            var provider = new MiniBatchProvider(new SingleRowDataSource(input, true, sequenceType, sequenceIndex), false);
            executionContext.Add(provider.GetMiniBatches(1, mb => _Execute(executionContext, mb)));

            IGraphOperation operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                operation.Execute(executionContext);
                foreach (var item in _executionResults) {
                    ret = new ExecutionResult(item.Context.BatchSequence, item.Data.AsIndexable().Rows.Select(r => r.Data).ToList());
                    item.Context.Dispose();
                    item.Data?.Dispose();
                }
                _executionResults.Clear();
                _lap.PopLayer();
            }
            _lap.PopLayer();
            _dataSource = null;
            return ret;
        }

        IReadOnlyList<IContext> _Execute(IExecutionContext executionContext, IMiniBatch batch)
        {
            var ret = new List<IContext>();
            if (batch.IsSequential) {
                IMiniBatchSequence curr = null;
                while ((curr = batch.GetNextSequence()) != null) {
                    var context = new ExecutionEngineContext(executionContext, curr);
                    _start.ExecuteForward(context, 0);
                    while (context.HasNext)
                        context.ExecuteNext();
                    _executionResults.Add((context, context.Data.GetMatrix()));
                    ret.Add(context);
                }
            } else {
                var context = new ExecutionEngineContext(executionContext, batch.CurrentSequence);
                _start.ExecuteForward(context, 0);

                while (context.HasNext)
                    context.ExecuteNext();

                _executionResults.Add((context, context.Data.GetMatrix()));
                ret.Add(context);
            }
            return ret;
        }
    }
}
