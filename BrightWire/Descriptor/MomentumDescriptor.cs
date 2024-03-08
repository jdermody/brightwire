using BrightData;
using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates a momentum gradient descent optimiser
    /// </summary>
    internal class MomentumDescriptor(float momentum = 0.9f) : ICreateTemplateBasedGradientDescent
    {
        protected readonly float _momentum = momentum;

        public virtual IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix<float> template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, true);
            return new Momentum(_momentum, cache, prev);
        }
    }
}
