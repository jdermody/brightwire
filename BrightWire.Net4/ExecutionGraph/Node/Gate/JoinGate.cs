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
        class Backpropagation : BackpropagationBase<JoinGate>
        {
            readonly IReadOnlyList<IncomingChannel> _channels;

            public Backpropagation(JoinGate source, IReadOnlyList<IncomingChannel> channels) : base(source)
            {
                _channels = channels;
            }

            public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                IMatrix split, residual = errorSignal.GetMatrix();
                int index = parents.Count-1;
                foreach(var item in _channels) {
                    (split, residual) = residual.SplitAtColumn(item.Size);
                    context.AddBackward(errorSignal.ReplaceWith(split), parents[index--], _source);
                }
                context.AddBackward(errorSignal.ReplaceWith(residual), parents[index], _source);
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
            _AddHistory(context, curr, () => new Backpropagation(this, list));
        }
    }
}
