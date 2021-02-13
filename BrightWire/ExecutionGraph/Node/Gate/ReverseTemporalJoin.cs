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

            public Backpropagation(ReverseTemporalJoin source, uint reverseSize) : base(source)
            {
                _reverseSize = reverseSize;
            }

            public override IEnumerable<(IGraphData Signal, INode ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var matrix = errorSignal.GetMatrix();
                (IFloatMatrix left, IFloatMatrix right) = matrix.SplitAtColumn(matrix.ColumnCount - _reverseSize);
                yield return (errorSignal.ReplaceWith(left), parents[0]);

                var batch = context.BatchSequence.MiniBatch;
                var sequenceIndex = context.BatchSequence.SequenceIndex;
                var reversedSequenceIndex = batch.SequenceCount - sequenceIndex - 1;
                _source._reverseBackpropagation.Add(reversedSequenceIndex, (parents[1], errorSignal.ReplaceWith(right)));
                _source._contextTable.Add(sequenceIndex, context);

                if (sequenceIndex == 0) {
                    // process in order as we are pushing onto a stack (so will be read in reverse order)
                    for(uint i = 0; i < batch.SequenceCount; i++) {
                        var data = _source._reverseBackpropagation[i];
                        var reverseContext = _source._contextTable[i];
                        
                        //TODO: fix this
                        //reverseContext.AddBackward(data.Item2, data.Item1, _source);
                    }
                    _source._reverseBackpropagation.Clear();
                    _source._contextTable.Clear();
                }
                //return ErrorTo(errorSignal, parents);
            }
        }
        Dictionary<uint, (IFloatMatrix Data, uint ReversedSize, INode ForwardParent)> _input = new Dictionary<uint, (IFloatMatrix Data, uint ReversedSize, INode ForwardParent)>();
        Dictionary<uint, (IFloatMatrix Data, INode ReverseParent)> _reverseInput = new Dictionary<uint, (IFloatMatrix Data, INode ReverseParent)>();

        Dictionary<uint, (INode, IGraphData)> _reverseBackpropagation = new Dictionary<uint, (INode, IGraphData)>();
        Dictionary<uint, IGraphSequenceContext> _contextTable = new Dictionary<uint, IGraphSequenceContext>();

        public ReverseTemporalJoin(string? name, WireBuilder forwardInput, WireBuilder reverseInput) 
            : base(name, forwardInput, reverseInput)
        {
        }

        public override void OnDeserialise(IReadOnlyDictionary<string, INode> graph)
        {
            _input = new Dictionary<uint, (IFloatMatrix Data, uint ReversedSize, INode ForwardParent)>();
            _reverseInput = new Dictionary<uint, (IFloatMatrix Data, INode ReverseParent)>();

            _reverseBackpropagation = new Dictionary<uint, (INode, IGraphData)>();
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
            var next = data.ConcatRows(floatMatrix);

            //context.AddForward(new ExecutionHistory(
            //    this, 
            //    new MatrixGraphData(next), 
            //    new[] { forwardParent, reverseParent }), 
            //    () => new Backpropagation(this, reversedSize)
            //);
        }

        protected override void Activate(IGraphSequenceContext context, List<IncomingChannel> data)
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
        }

        protected override (IFloatMatrix Next, Func<IBackpropagate>? BackProp) Activate2(IGraphSequenceContext context, List<IncomingChannel> data)
        {
            throw new NotImplementedException();
        }
    }
}
