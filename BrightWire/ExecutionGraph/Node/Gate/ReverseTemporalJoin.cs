using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    internal class ReverseTemporalJoin : MultiGateBase
    {
        class Backpropagation(ReverseTemporalJoin source, uint reverseSize, NodeBase forward, NodeBase backward)
            : BackpropagationBase<ReverseTemporalJoin>(source)
        {
            public override IEnumerable<(IGraphData Signal, IGraphContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphContext context, NodeBase[] parents)
            {
                var matrix = errorSignal.GetMatrix();
                var (left, right) = matrix.SplitAtColumn(matrix.ColumnCount - reverseSize);
                yield return (errorSignal.ReplaceWith(left), context, forward);

                var batch = context.BatchSequence.MiniBatch;
                var sequenceIndex = context.BatchSequence.SequenceIndex;
                var reversedSequenceIndex = batch.SequenceCount - sequenceIndex - 1;
                _source._reverseBackpropagation.Add(reversedSequenceIndex, (backward, errorSignal.ReplaceWith(right)));
                _source._contextTable.Add(sequenceIndex, context);

                if (sequenceIndex == 0) {
                    foreach(var i in batch.SequenceCount.AsRange().Reverse()) {
                        var data = _source._reverseBackpropagation[i];
                        var reverseContext = _source._contextTable[i];
                        reverseContext.ClearForBackpropagation();
                        yield return (data.Data, reverseContext, data.Node);
                    }
                    _source._reverseBackpropagation.Clear();
                    _source._contextTable.Clear();
                }
            }
        }
        Dictionary<uint, (IMatrix<float> Data, uint ReversedSize, NodeBase ForwardParent)> _input = new();
        Dictionary<uint, (IMatrix<float> Data, NodeBase ReverseParent)> _reverseInput = new();

        Dictionary<uint, (NodeBase Node, IGraphData Data)> _reverseBackpropagation = new();
        Dictionary<uint, IGraphContext> _contextTable = new();

        public ReverseTemporalJoin(string? name, WireBuilder forwardInput, WireBuilder reverseInput) 
            : base(name, forwardInput, reverseInput)
        {
        }

        public override void OnDeserialise(IReadOnlyDictionary<string, NodeBase> graph)
        {
            _input = new Dictionary<uint, (IMatrix<float> Data, uint ReversedSize, NodeBase ForwardParent)>();
            _reverseInput = new Dictionary<uint, (IMatrix<float> Data, NodeBase ReverseParent)>();

            _reverseBackpropagation = new Dictionary<uint, (NodeBase, IGraphData)>();
            _contextTable = new Dictionary<uint, IGraphContext>();
        }

        void Continue(IGraphContext context)
        {
            var sequenceIndex = context.BatchSequence.SequenceIndex;
            var (data, reversedSize, forwardParent) = _input[sequenceIndex];
            var (floatMatrix, reverseParent) = _reverseInput[sequenceIndex];

            _input.Remove(sequenceIndex);
            _reverseInput.Remove(sequenceIndex);

            // concatenate the inputs
            var next = data.ConcatRight(floatMatrix).AsGraphData();
            context.AddForwardHistory(this, next, () => new Backpropagation(this, reversedSize, forwardParent, reverseParent));
            foreach(var wire in Output)
                wire.SendTo.Forward(next, context, wire.Channel, this);
        }

        protected override (IMatrix<float>? Next, Func<IBackpropagate>? BackProp) Activate(IGraphContext context, List<IncomingChannel> data)
        {
            if (data.Count != 2)
                throw new Exception("Expected two incoming channels");

            var forward = data.First();
            var backward = data.Last();

            if (forward.Data == null || backward.Data == null)
                throw new Exception("Expected incoming channels to have data");

            var sequenceIndex = context.BatchSequence.SequenceIndex;
            var reversedSequenceIndex = context.BatchSequence.MiniBatch.SequenceCount - sequenceIndex - 1;

            _input.Add(sequenceIndex, (forward.Data, backward.Size, forward.Source!));
            _reverseInput.Add(reversedSequenceIndex, (backward.Data, backward.Source!));

            context.ExecutionContext.RegisterContinuation(context.BatchSequence, Continue);
            return (null, null);
        }
    }
}
