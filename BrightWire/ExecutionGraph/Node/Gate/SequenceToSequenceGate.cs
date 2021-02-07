using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class SequenceToSequenceGate : NodeBase
    {
        class Backpropagation : BackpropagationBase<SequenceToSequenceGate>
        {
            public Backpropagation(SequenceToSequenceGate source) : base(source)
            {
            }

            public override void BackwardInternal(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart)
                    _source.AddGradient(context, errorSignal.GetMatrix());
                // backpropagation should stop here
            }
        }

        readonly ConcurrentDictionary<IGraphSequenceContext, IFloatMatrix> _contextError = new ConcurrentDictionary<IGraphSequenceContext, IFloatMatrix>();

        public SequenceToSequenceGate(string? name, string? id = null) : base(name, id)
        {
        }

        public void AddGradient(IGraphSequenceContext context, IFloatMatrix gradient)
        {
            _contextError.TryAdd(context, gradient.Clone());
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceEnd) {
                var nextBatch = context.BatchSequence.MiniBatch.NextMiniBatch;
                if (nextBatch == null)
                    throw new Exception("No following mini batch was found");

                context.ExecutionContext.RegisterAdditional(nextBatch, context.Data, OnStartEncoder, OnEndEncoder);
            }
        }

        void OnStartEncoder(IGraphSequenceContext context, IGraphData data)
        {
            AddNextGraphAction(context, data, () => new Backpropagation(this));
        }

        void OnEndEncoder(IGraphSequenceContext[] context)
        {
            for(int i = 0, len = context.Length; i < len; i++) {
                var item = context[i];
                item.StoreExecutionResult();
                if (item.LearningContext != null) {
                    var learningContext = item.LearningContext;
                    learningContext.DeferBackpropagation(null, item.Backpropagate);
                    if(i == len-1 && _contextError.TryGetValue(item, out var gradient))
                        learningContext.BackpropagateThroughTime(new MatrixGraphData(gradient));
                }
            }
            foreach(var item in _contextError)
                item.Value.Dispose();
            _contextError.Clear();
        }
    }
}
