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

        protected override (IMatrix Next, Func<IBackpropagate>? BackProp) Activate(IGraphContext context, IMatrix primary, IMatrix secondary, NodeBase primarySource, NodeBase secondarySource)
        {
            var output = primary.Add(secondary);

            // default backpropagation behaviour is to pass the error signal to all ancestors, which is correct for addition
            return (output, null);
        }
    }
}
