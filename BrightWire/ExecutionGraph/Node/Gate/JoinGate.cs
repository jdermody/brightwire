using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    internal class JoinGate(string? name, params WireBuilder[] incoming) : MultiGateBase(name, incoming)
    {
        class Backpropagation(JoinGate source, List<IncomingChannel> channels, NodeBase[] ancestors)
            : BackpropagationBase<JoinGate>(source)
        {
            public override IEnumerable<(IGraphData Signal, IGraphContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphContext context, NodeBase[] parents)
            {
                var residual = errorSignal.GetMatrix();
                var index = parents.Length-1;
                foreach(var item in channels) {
                    (residual, var split) = residual.SplitAtColumn(residual.ColumnCount - item.Size);
                    yield return (errorSignal.ReplaceWith(split), context, ancestors[index--]);
                }
                yield return (errorSignal.ReplaceWith(residual), context, ancestors[index]);
            }
        }

        protected override (IMatrix<float> Next, Func<IBackpropagate>? BackProp) Activate(IGraphContext context, List<IncomingChannel> data)
        {
            data.Reverse();
            var curr = data.First().Data;
            if (curr?.ColumnCount != data.First().Size)
                throw new Exception("Sizes are different");

            var list = new List<IncomingChannel>();
            var nodes = data.Select(d => d.Source!).ToArray();
            foreach(var item in data.Skip(1)) {
                var next = curr.ConcatRight(item.Data!);
                //curr.Dispose();
                curr = next;
                list.Add(item);
            }
            return (curr, () => new Backpropagation(this, list, nodes));
        }
    }
}
