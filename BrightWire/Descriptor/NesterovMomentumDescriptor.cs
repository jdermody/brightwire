using BrightData;
using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates a nesterov momentum gradient descent optimiser
    /// </summary>
    class NesterovMomentumDescriptor : MomentumDescriptor
    {
        public NesterovMomentumDescriptor(float momentum = 0.9f) : base(momentum)
        {
        }

        public override IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IFloatMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateZeroMatrix(template.RowCount, template.ColumnCount);
            return new NesterovMomentum(_momentum, cache, prev);
        }
    }
}
