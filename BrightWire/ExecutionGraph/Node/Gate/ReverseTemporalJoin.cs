﻿using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class ReverseTemporalJoin : MultiGateBase
    {
        class Backpropagation : BackpropagationBase<ReverseTemporalJoin>
        {
            readonly uint _reverseSize;

            public Backpropagation(ReverseTemporalJoin source, uint reverseSize) : base(source)
            {
                _reverseSize = reverseSize;
            }

            public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var matrix = errorSignal.GetMatrix();
                (IMatrix left, IMatrix right) = matrix.SplitAtColumn(matrix.ColumnCount - _reverseSize);
                context.AddBackward(errorSignal.ReplaceWith(left), parents[0], _source);

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
                        reverseContext.AddBackward(data.Item2, data.Item1, _source);
                    }
                    _source._reverseBackpropagation.Clear();
                    _source._contextTable.Clear();
                }
            }
        }
        Dictionary<uint, (IMatrix Data, uint ReversedSize, INode ForwardParent)> _input = new Dictionary<uint, (IMatrix Data, uint ReversedSize, INode ForwardParent)>();
        Dictionary<uint, (IMatrix Data, INode ReverseParent)> _reverseInput = new Dictionary<uint, (IMatrix Data, INode ReverseParent)>();

        Dictionary<uint, (INode, IGraphData)> _reverseBackpropagation = new Dictionary<uint, (INode, IGraphData)>();
        Dictionary<uint, IContext> _contextTable = new Dictionary<uint, IContext>();

        public ReverseTemporalJoin(string name, WireBuilder forwardInput, WireBuilder reverseInput) 
            : base(name, forwardInput, reverseInput)
        {
        }

        public override void OnDeserialise(IReadOnlyDictionary<string, INode> graph)
        {
            _input = new Dictionary<uint, (IMatrix Data, uint ReversedSize, INode ForwardParent)>();
            _reverseInput = new Dictionary<uint, (IMatrix Data, INode ReverseParent)>();

            _reverseBackpropagation = new Dictionary<uint, (INode, IGraphData)>();
            _contextTable = new Dictionary<uint, IContext>();
        }

        void _Continue(IContext context)
        {
            var sequenceIndex = context.BatchSequence.SequenceIndex;
            var input = _input[sequenceIndex];
            var input2 = _reverseInput[sequenceIndex];

            _input.Remove(sequenceIndex);
            _reverseInput.Remove(sequenceIndex);

            // concatenate the inputs
            var next = input.Data.ConcatRows(input2.Data);

            context.AddForward(new TrainingAction(
                this, 
                new MatrixGraphData(next), 
                new[] { input.ForwardParent, input2.ReverseParent }), 
                () => new Backpropagation(this, input.ReversedSize)
            );
        }

        protected override void _Activate(IContext context, IReadOnlyList<IncomingChannel> data)
        {
            Debug.Assert(data.Count == 2);
            var forward = data.First();
            var backward = data.Last();
            var sequenceIndex = context.BatchSequence.SequenceIndex;
            var reversedSequenceIndex = context.BatchSequence.MiniBatch.SequenceCount - sequenceIndex - 1;

            _input.Add(sequenceIndex, (forward.Data, backward.Size, forward.Source));
            _reverseInput.Add(reversedSequenceIndex, (data.Last().Data, backward.Source));

            context.ExecutionContext.RegisterContinuation(context.BatchSequence, _Continue);
        }
    }
}
