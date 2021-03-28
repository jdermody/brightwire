using System;
using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    internal class ReverseTemporalJoin : MultiGateBase
    {
        class Backpropagation : BackpropagationBase<ReverseTemporalJoin>
        {
            readonly uint _reverseSize;
            readonly NodeBase _forward;
            readonly NodeBase _backward;

            public Backpropagation(ReverseTemporalJoin source, uint reverseSize, NodeBase forward, NodeBase backward) : base(source)
            {
                _reverseSize = reverseSize;
                _forward = forward;
                _backward = backward;
            }

            public override IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents)
            {
                var matrix = errorSignal.GetMatrix();
                (IFloatMatrix left, IFloatMatrix right) = matrix.SplitAtColumn(matrix.ColumnCount - _reverseSize);
                yield return (errorSignal.ReplaceWith(left), context, _forward);

                var batch = context.BatchSequence.MiniBatch;
                var sequenceIndex = context.BatchSequence.SequenceIndex;
                var reversedSequenceIndex = batch.SequenceCount - sequenceIndex - 1;
                _source._reverseBackpropagation.Add(reversedSequenceIndex, (_backward, errorSignal.ReplaceWith(right)));
                _source._contextTable.Add(sequenceIndex, context);

                if (sequenceIndex == 0) {
                    //for(uint i = batch.SequenceCount-1; i >= 0; i++) {
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
        Dictionary<uint, (IFloatMatrix Data, uint ReversedSize, NodeBase ForwardParent)> _input = new Dictionary<uint, (IFloatMatrix Data, uint ReversedSize, NodeBase ForwardParent)>();
        Dictionary<uint, (IFloatMatrix Data, NodeBase ReverseParent)> _reverseInput = new Dictionary<uint, (IFloatMatrix Data, NodeBase ReverseParent)>();

        Dictionary<uint, (NodeBase Node, IGraphData Data)> _reverseBackpropagation = new Dictionary<uint, (NodeBase, IGraphData)>();
        Dictionary<uint, IGraphSequenceContext> _contextTable = new Dictionary<uint, IGraphSequenceContext>();

        public ReverseTemporalJoin(string? name, WireBuilder forwardInput, WireBuilder reverseInput) 
            : base(name, forwardInput, reverseInput)
        {
        }

        public override void OnDeserialise(IReadOnlyDictionary<string, NodeBase> graph)
        {
            _input = new Dictionary<uint, (IFloatMatrix Data, uint ReversedSize, NodeBase ForwardParent)>();
            _reverseInput = new Dictionary<uint, (IFloatMatrix Data, NodeBase ReverseParent)>();

            _reverseBackpropagation = new Dictionary<uint, (NodeBase, IGraphData)>();
            _contextTable = new Dictionary<uint, IGraphSequenceContext>();
        }

        void Continue(IGraphSequenceContext context)
        {
            var sequenceIndex = context.BatchSequence.SequenceIndex;
            var (data, reversedSize, forwardParent) = _input[sequenceIndex];
            var (floatMatrix, reverseParent) = _reverseInput[sequenceIndex];

            _input.Remove(sequenceIndex);
            _reverseInput.Remove(sequenceIndex);

            // concatenate the inputs
            var next = data.ConcatRows(floatMatrix).AsGraphData();
            context.AddForwardHistory(this, next, () => new Backpropagation(this, reversedSize, forwardParent, reverseParent));
            foreach(var wire in Output)
                wire.SendTo.Forward(next, context, wire.Channel, this);
        }

        protected override (IFloatMatrix? Next, Func<IBackpropagate>? BackProp) Activate(IGraphSequenceContext context, List<IncomingChannel> data)
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
