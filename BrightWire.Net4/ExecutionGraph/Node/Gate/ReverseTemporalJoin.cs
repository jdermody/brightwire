using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class ReverseTemporalJoin : MultiGateBase
    {
        class Backpropagation : BackpropagationBase<ReverseTemporalJoin>
        {
            readonly int _reverseSize;

            public Backpropagation(ReverseTemporalJoin source, int reverseSize) : base(source)
            {
                _reverseSize = reverseSize;
            }

            public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                IMatrix split, residual = errorSignal.GetMatrix();
                (split, residual) = residual.SplitAtColumn(residual.ColumnCount - _reverseSize);
                //context.AddBackward(errorSignal.ReplaceWith(split), parents[1], _source);
                context.AddBackward(errorSignal.ReplaceWith(split), parents[0], _source);

                var batch = context.BatchSequence.MiniBatch;
                var sequenceIndex = context.BatchSequence.SequenceIndex;
                var reversedSequenceIndex = batch.SequenceCount - sequenceIndex - 1;
                _source._reverseBackpropagation.Add(reversedSequenceIndex, (parents[1], errorSignal.ReplaceWith(residual)));
                if(sequenceIndex == 0) {
                    for(var i = 0; i < batch.SequenceCount; i++) {
                        var data = _source._reverseBackpropagation[i];
                        context.AddBackward(data.Item2, data.Item1, _source);
                    }
                    _source._reverseBackpropagation.Clear();
                }
            }
        }
        Dictionary<int, (IMatrix Data, int ReversedSize, INode ForwardParent)> _input = new Dictionary<int, (IMatrix Data, int ReversedSize, INode ForwardParent)>();
        Dictionary<int, IMatrix> _reverseInput = new Dictionary<int, IMatrix>();
        Dictionary<int, INode> _reverseParent = new Dictionary<int, INode>();
        Dictionary<int, (INode, IGraphData)> _reverseBackpropagation = new Dictionary<int, (INode, IGraphData)>();

        public ReverseTemporalJoin(string name, WireBuilder forwardInput, WireBuilder reverseInput) 
            : base(name, new[] { forwardInput, reverseInput })
        {
        }

        public override void OnDeserialise(IReadOnlyDictionary<string, INode> graph)
        {
            _input = new Dictionary<int, (IMatrix Data, int ReversedSize, INode ForwardParent)>();
            _reverseInput = new Dictionary<int, IMatrix>();
            _reverseParent = new Dictionary<int, INode>();
            _reverseBackpropagation = new Dictionary<int, (INode, IGraphData)>();
        }

        void _Continue(IContext context)
        {
            var batch = context.BatchSequence.MiniBatch;
            var sequenceIndex = context.BatchSequence.SequenceIndex;

            var input = _input[sequenceIndex];
            var input2 = _reverseInput[sequenceIndex];
            var reverseParent = _reverseParent[sequenceIndex];
            _input.Remove(sequenceIndex);
            _reverseInput.Remove(sequenceIndex);
            _reverseParent.Remove(sequenceIndex);

            // concatenate the inputs
            var next = input.Data.ConcatRows(input2);

            context.AddForward(new TrainingAction(
                this, 
                new MatrixGraphData(next), 
                new[] { input.ForwardParent, reverseParent }), 
                () => new Backpropagation(this, input.ReversedSize)
            );
        }

        protected override void _Activate(IContext context, IReadOnlyList<IncomingChannel> data)
        {
            Debug.Assert(data.Count == 2);
            var batch = context.BatchSequence.MiniBatch;
            var sequenceIndex = context.BatchSequence.SequenceIndex;
            var reversedSequenceIndex = batch.SequenceCount - sequenceIndex - 1;
            var forward = data.First();
            var backward = data.Last();

            _input.Add(sequenceIndex, (forward.Data, backward.Size, forward.Source));
            _reverseInput.Add(reversedSequenceIndex, data.Last().Data);
            _reverseParent.Add(sequenceIndex, backward.Source);

            context.ExecutionContext.RegisterContinuation(context.BatchSequence, _Continue);
            //if(_input.Count == batch.SequenceCount) {
            //    for (var i = 0; i < batch.SequenceCount; i++) {
            //        var reversedSequenceIndex2 = batch.SequenceCount - i - 1;
            //        var input = _input[i];
            //        var input2 = _reverseInput[i];
            //        var data2 = new[] {
            //            input.Input,
            //            input2
            //        };
            //        var list = new[] {
            //            _reverseInput[reversedSequenceIndex2]
            //        };

            //        var curr = input.Input.Data;
            //        curr = curr.ConcatRows(input2.Data);

            //        _AddHistory(input.Context, data2, curr, () => new Backpropagation(this, list));
            //    }
            //    _input.Clear();
            //    _reverseInput.Clear();
            //}
        }
    }
}
