using BrightData;
using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates a nesterov momentum gradient descent optimiser
    /// </summary>
    internal class NesterovMomentumDescriptor(float momentum = 0.9f) : MomentumDescriptor(momentum)
    {
        public override IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, true);
            return new NesterovMomentum(_momentum, cache, prev);
        }
    }
}
