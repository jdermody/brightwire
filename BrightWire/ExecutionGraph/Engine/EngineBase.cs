using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Engine
{
    internal abstract class EngineBase<GCT> : ICreateGraphContext
        where GCT: IGraphSequenceContext
    {
        protected readonly ILinearAlgebraProvider _lap;
        protected IDataSource? _dataSource = null;

        protected EngineBase(ILinearAlgebraProvider lap) { _lap = lap; }

        protected abstract IEnumerable<IGraphSequenceContext> Execute(IGraphExecutionContext context, IMiniBatch miniBatch);
        public abstract GCT CreateContext(IGraphExecutionContext executionContext, IMiniBatchSequence sequence);

        protected IEnumerable<IGraphSequenceContext> Continue(IMiniBatch batch, IGraphExecutionContext executionContext, Func<IMiniBatchSequence, IGraphSequenceContext> lookupContext)
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

        public IEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null)
        {
            _lap.PushLayer();
            _dataSource = dataSource;
            var provider = new MiniBatchProvider(dataSource, null);
            using var executionContext = new ExecutionContext(_lap, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(batchSize, mb => Execute(executionContext, mb)));
            float operationCount = executionContext.RemainingOperationCount;
            float index = 0f;

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
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
                _lap.PopLayer();

                if (batchCompleteCallback != null) {
                    var percentage = (++index) / operationCount;
                    batchCompleteCallback(percentage);
                }
            }

            _lap.PopLayer();
        }

        public ExecutionResult? Execute(float[] input)
        {
            _lap.PushLayer();
            _dataSource = new SingleRowDataSource(input, _lap, false, MiniBatchSequenceType.Standard, 0);
            var provider = new MiniBatchProvider(_dataSource, null);
            using var executionContext = new ExecutionContext(_lap, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1, mb => Execute(executionContext, mb)));

            IGraphOperation? operation;
            IGraphSequenceContext? context = null;
            // TODO: check that there is a single operation?
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                context = operation.Execute(executionContext).Single();
                _lap.PopLayer();
            }
            var ret = context?.Result;
            context?.Dispose();
            _lap.PopLayer();
            _dataSource = null;
            return ret;
        }

        //protected ExecutionResult _Execute(float[] input)
        //{
        //    _lap.PushLayer();
        //    ExecutionResult ret = null;
        //    _dataSource = new SingleRowDataSource(input, false, MiniBatchSequenceType.Standard, 0);
        //    var provider = new MiniBatchProvider(_dataSource, _lap.Context.Random);
        //    using (var executionContext = new ExecutionContext(_lap)) {
        //        executionContext.Add(provider.GetMiniBatches(1, mb => _Execute(executionContext, mb)));

        //        IGraphOperation operation;
        //        while ((operation = executionContext.GetNextOperation()) != null) {
        //            operation.Execute(executionContext);
        //            _ClearContextList();
        //        }

        //        ret = _GetResults().Single();
        //    }
        //    _lap.PopLayer();
        //    _dataSource = null;
        //    return ret;
        //}

        public IEnumerable<ExecutionResult> ExecuteSequential(float[][] input)
        {
            _lap.PushLayer();
            _dataSource = new SequentialRowDataSource(input, _lap);
            var provider = new MiniBatchProvider(_dataSource, null);
            using var executionContext = new ExecutionContext(_lap, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1, mb => Execute(executionContext, mb)));

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                foreach (var context in operation.Execute(executionContext)) {
                    yield return context.Result;
                    context.Dispose();
                }

                _lap.PopLayer();
            }

            _lap.PopLayer();
            _dataSource = null;
        }

        public ExecutionResult? ExecuteSingleSequentialStep(uint sequenceIndex, float[] input, MiniBatchSequenceType sequenceType)
        {
            _lap.PushLayer();
            _dataSource = new SingleRowDataSource(input, _lap, true, sequenceType, sequenceIndex);
            var provider = new MiniBatchProvider(_dataSource, _lap.Context.Random);
            using var executionContext = new ExecutionContext(_lap, this);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1, mb => Execute(executionContext, mb)));

            IGraphOperation? operation;
            IGraphSequenceContext? context = null;
            // TODO: check that there is a single operation?
            while ((operation = executionContext.GetNextOperation()) != null) {
                context = operation.Execute(executionContext).Single();
            }

            var ret = context?.Result;
            context?.Dispose();
            _lap.PopLayer();
            _dataSource = null;
            return ret;
        }

        IGraphSequenceContext ICreateGraphContext.Create(IGraphExecutionContext executionContext, IMiniBatchSequence sequence) => CreateContext(executionContext, sequence);
    }
}
