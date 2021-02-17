﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    internal class JoinGate : MultiGateBase
    {
        class Backpropagation : BackpropagationBase<JoinGate>
        {
            readonly List<IncomingChannel> _channels;

            public Backpropagation(JoinGate source, List<IncomingChannel> channels) : base(source)
            {
                _channels = channels;
            }

            public override IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents)
            {
                IFloatMatrix split, residual = errorSignal.GetMatrix();
                int index = parents.Length-1;
                foreach(var item in _channels) {
                    (residual, split) = residual.SplitAtColumn(residual.ColumnCount - item.Size);
                    yield return (errorSignal.ReplaceWith(split), context, parents[index--]);
                }
                yield return (errorSignal.ReplaceWith(residual), context, parents[index]);
            }
        }

        public JoinGate(string? name, params WireBuilder[] incoming) : base(name, incoming)
        {
        }

        protected override (IFloatMatrix Next, Func<IBackpropagate>? BackProp) Activate(IGraphSequenceContext context, List<IncomingChannel> data)
        {
            data.Reverse();
            var curr = data.First().Data;
            if (curr?.ColumnCount != data.First().Size)
                throw new Exception("Sizes are different");

            var list = new List<IncomingChannel>();
            foreach(var item in data.Skip(1)) {
                var next = curr.ConcatRows(item.Data!);
                //curr.Dispose();
                curr = next;
                list.Add(item);
            }
            return (curr, () => new Backpropagation(this, list));
        }
    }
}
