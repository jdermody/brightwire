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

        protected override (IFloatMatrix Next, Func<IBackpropagate>? BackProp) Activate(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary, NodeBase primarySource, NodeBase secondarySource)
        {
            var output = primary.Add(secondary);

            // default backpropagation behaviour is to pass the error signal to all ancestors, which is correct for addition
            return (output, null);
        }
    }
}
