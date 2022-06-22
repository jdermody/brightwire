using BrightData;
using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates a momentum gradient descent optimiser
    /// </summary>
    internal class MomentumDescriptor : ICreateTemplateBasedGradientDescent
    {
        protected readonly float _momentum;

        public MomentumDescriptor(float momentum = 0.9f)
        {
            _momentum = momentum;
        }

        public virtual IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount);
            return new Momentum(_momentum, cache, prev);
        }
    }
}
