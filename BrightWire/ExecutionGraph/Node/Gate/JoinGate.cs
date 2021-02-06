using System;
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

            public override void BackwardInternal(INode? fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
            {
                IFloatMatrix split, residual = errorSignal.GetMatrix();
                int index = parents.Length-1;
                foreach(var item in _channels) {
                    (residual, split) = residual.SplitAtColumn(residual.ColumnCount - item.Size);
                    context.AddBackward(errorSignal.ReplaceWith(split), parents[index--], _source);
                }
                context.AddBackward(errorSignal.ReplaceWith(residual), parents[index], _source);
            }
        }

        public JoinGate(string? name, params WireBuilder[] incoming) : base(name, incoming)
        {
        }

        protected override void Activate(IGraphContext context, List<IncomingChannel> data)
        {
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
            AddHistory(context, data, curr, () => new Backpropagation(this, list));
        }
    }
}
