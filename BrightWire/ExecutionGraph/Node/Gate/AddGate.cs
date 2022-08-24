using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Outputs the two input signals added together
    /// </summary>
    internal class AddGate : BinaryGateBase
    {
        public AddGate(string? name = null) : base(name) { }

        protected override (IGraphData Next, Func<IBackpropagate>? BackProp) Activate(IGraphContext context, IGraphData primary, IGraphData secondary, NodeBase primarySource, NodeBase secondarySource)
        {
            var output = primary.GetMatrix().Add(secondary.GetMatrix());

            // default backpropagation behaviour is to pass the error signal to all ancestors, which is correct for addition
            return (output.AsGraphData(), null);
        }
    }
}
