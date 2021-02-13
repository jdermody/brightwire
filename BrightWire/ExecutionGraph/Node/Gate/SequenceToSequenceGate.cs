using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BrightData;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class SequenceToSequenceGate : NodeBase
    {
        //class Backpropagation : BackpropagationBase<SequenceToSequenceGate>
        //{
        //    public Backpropagation(SequenceToSequenceGate source) : base(source)
        //    {
        //    }

        //    public override void BackwardInternal(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
        //    {
        //        if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart) {
        //            SendErrorTo(context.Data, context, parents);
        //        }
        //    }

        //    public override IEnumerable<(IGraphData signal, INode toNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
        //    {
        //        if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart) {
        //            foreach(var parent in parents)
        //                yield return (context.Data, parent);
        //        }
        //    }
        //}

        ConcurrentStack<IGraphSequenceContext> _encoderContext;

        public SequenceToSequenceGate(string? name, string? id = null) : base(name, id)
        {
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            _encoderContext ??= new ConcurrentStack<IGraphSequenceContext>();
            _encoderContext.Push(context);
            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceEnd) {
                var nextBatch = context.BatchSequence.MiniBatch.NextMiniBatch;
                if (nextBatch == null)
                    throw new Exception("No following mini batch was found");

                context.ExecutionContext.RegisterAdditional(nextBatch, context.Data, OnStartEncoder, OnEndEncoder);
            }
        }

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            _encoderContext ??= new ConcurrentStack<IGraphSequenceContext>();
            _encoderContext.Push(context);
            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceEnd) {
                var nextBatch = context.BatchSequence.MiniBatch.NextMiniBatch;
                if (nextBatch == null)
                    throw new Exception("No following mini batch was found");

                context.ExecutionContext.RegisterAdditional(nextBatch, signal, OnStartEncoder, OnEndEncoder);
            }

            return (this, NullGraphData.Instance, null);
        }

        void OnStartEncoder(IGraphSequenceContext context, IGraphData data)
        {
            AddNextGraphAction(context, data, null/*, () => new Backpropagation(this)*/);
        }

        void OnEndEncoder(IGraphSequenceContext[] context)
        {
            var learningContext = context.FirstOrDefault()?.LearningContext;
            if (learningContext != null) {
                var gradient = learningContext.BackpropagateThroughTime(null);

                //var firstContext = (ICanTrace)context.Single(c => c.BatchSequence.Type == MiniBatchSequenceType.SequenceStart);
                //var lastContext = (ICanTrace)context.Single(c => c.BatchSequence.Type == MiniBatchSequenceType.SequenceEnd);

                //firstContext.Trace();
                //lastContext.Trace();
                
                if (gradient != null) {
                    foreach (var item in _encoderContext.Reverse())
                        learningContext.DeferBackpropagation(null, delta => item.Backpropagate(delta));
                    learningContext.BackpropagateThroughTime(gradient);
                }
            }

            _encoderContext.Clear();
        }
    }
}
