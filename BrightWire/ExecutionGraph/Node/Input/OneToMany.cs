using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Node.Input
{
    /// <summary>
    /// Waits for all children to finish back-propagating before sending the error further backward
    /// </summary>
    internal sealed class OneToMany : NodeBase
    {
        public OneToMany(IEnumerable<NodeBase> children, string? name = null) : base(name)
        {
            foreach (var child in children)
                Output.Add(new WireToNode(child));
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            // default behaviour is to send backpropagation signal to all ancestors, so no need to do anything
            return (this, signal, null);
        }
    }
}
