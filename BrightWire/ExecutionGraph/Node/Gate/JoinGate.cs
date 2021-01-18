using System.Collections.Generic;
using System.Diagnostics;
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

            public override void _Backward(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
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

        public JoinGate(string name, params WireBuilder[] incoming) : base(name, incoming)
        {
        }

        protected override void _Activate(IGraphContext context, List<IncomingChannel> data)
        {
            var curr = data.First().Data;
            Debug.Assert(curr.ColumnCount == data.First().Size);
            var list = new List<IncomingChannel>();
            foreach(var item in data.Skip(1)) {
                Debug.Assert(item.Data.ColumnCount == item.Size);
                var next = curr.ConcatRows(item.Data);
                //curr.Dispose();
                curr = next;
                list.Add(item);
            }
            _AddHistory(context, data, curr, () => new Backpropagation(this, list));
        }
    }
}
