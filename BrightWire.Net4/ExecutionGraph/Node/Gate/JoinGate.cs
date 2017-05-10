using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class JoinGate : MultiGateBase
    {
        class Backpropagation : BackpropagationBase
        {
            readonly IReadOnlyList<IncomingChannel> _channels;

            public Backpropagation(IReadOnlyList<IncomingChannel> channels)
            {
                _channels = channels;
            }

            public override void Backward(IMatrix errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                IMatrix split, residual = errorSignal;
                int index = 0;
                foreach(var item in _channels) {
                    (split, residual) = residual.SplitAtColumn(item.Size);
                    context.AddBackward(split, parents[index++]);
                }
                context.AddBackward(residual, parents[index]);
            }
        }

        public JoinGate(string name, params WireBuilder[] incoming) : base(name, incoming)
        {
        }

        protected override void _Activate(IContext context, IEnumerable<IncomingChannel> data)
        {
            var curr = data.First().Data;
            Debug.Assert(curr.ColumnCount == data.First().Size);
            var list = new List<IncomingChannel>();
            foreach(var item in data.Skip(1)) {
                Debug.Assert(item.Data.ColumnCount == item.Size);
                var next = curr.ConcatRows(item.Data);
                curr.Dispose();
                curr = next;
                list.Add(item);
            }
            _AddHistory(context, curr, () => new Backpropagation(list));
        }
    }
}
