﻿using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Engine
{
    internal abstract class EngineBase
    {
        protected readonly ILinearAlgebraProvider _lap;
        protected IDataSource? _dataSource = null;

        protected EngineBase(ILinearAlgebraProvider lap) { _lap = lap; }

        protected abstract void ClearContextList();
        protected abstract void Execute(IGraphExecutionContext context, IMiniBatch miniBatch);
        protected abstract IEnumerable<ExecutionResult> GetResults();

        protected bool Continue(IMiniBatch batch, IGraphExecutionContext executionContext, Func<IMiniBatchSequence, IGraphContext> lookupContext)
        {
            var ret = false;

	        while (executionContext.HasContinuations) {
                batch.Reset();
	            IMiniBatchSequence? currentSequence;
	            while ((currentSequence = batch.GetNextSequence()) != null) {
                    var context = lookupContext(currentSequence);
                    executionContext.Continue(context);
                    while (context.HasNext)
                        context.ExecuteNext();
                }
                ret = true;
            }
            return ret;
        }

        public ExecutionResult? Execute(float[] input)
        {
            _lap.PushLayer();
            ExecutionResult? ret = null;
            _dataSource = new SingleRowDataSource(input, _lap, false, MiniBatchSequenceType.Standard, 0);
            var provider = new MiniBatchProvider(_dataSource, null);
            using var executionContext = new ExecutionContext(_lap);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1, mb => Execute(executionContext, mb)));

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                operation.Execute(executionContext);
                ret = GetResults().Single();
                ClearContextList();
                _lap.PopLayer();
            }

            _lap.PopLayer();
            _dataSource = null;
            return ret;
        }

        public IEnumerable<ExecutionResult> ExecuteSequential(float[][] input)
        {
            _lap.PushLayer();
            _dataSource = new SequentialRowDataSource(input, _lap);
            var provider = new MiniBatchProvider(_dataSource, null);
            using var executionContext = new ExecutionContext(_lap);
            // ReSharper disable once AccessToDisposedClosure
            executionContext.Add(provider.GetMiniBatches(1, mb => Execute(executionContext, mb)));

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                _lap.PushLayer();
                operation.Execute(executionContext);
                foreach (var result in GetResults())
                    yield return result;
                ClearContextList();
                _lap.PopLayer();
            }

            _lap.PopLayer();
            _dataSource = null;
        }

        public ExecutionResult? ExecuteSequential(uint sequenceIndex, float[] input, IGraphExecutionContext executionContext, MiniBatchSequenceType sequenceType)
        {
            _lap.PushLayer();
            _dataSource = new SingleRowDataSource(input, _lap, true, sequenceType, sequenceIndex);
            var provider = new MiniBatchProvider(_dataSource, _lap.Context.Random);
            executionContext.Add(provider.GetMiniBatches(1, mb => Execute(executionContext, mb)));

            IGraphOperation? operation;
            while ((operation = executionContext.GetNextOperation()) != null) {
                operation.Execute(executionContext);
                ClearContextList();
            }

            var ret = GetResults().SingleOrDefault();
            _lap.PopLayer();
            _dataSource = null;
            return ret;
        }
    }
}
