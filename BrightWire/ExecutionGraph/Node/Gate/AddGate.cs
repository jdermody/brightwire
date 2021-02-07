using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Outputs the two input signals added together
    /// </summary>
    internal class AddGate : BinaryGateBase
    {
        public AddGate(string? name = null) : base(name) { }

        protected override void Activate(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary)
        {
            var output = primary.Add(secondary);

            // default is to pass the error signal through, which is correct for addition
            AddHistory(context, output, null);
        }
    }
}
