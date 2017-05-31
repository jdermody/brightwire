using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor.GradientDescent
{
    /// <summary>
    /// Creates a rms prop gradient descent optimiser
    /// </summary>
    class RmsPropDescriptor : ICreateTemplateBasedGradientDescent
    {
        readonly float _decay;

        public RmsPropDescriptor(float decay = 0.9f)
        {
            _decay = decay;
        }

        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateZeroMatrix(template.RowCount, template.ColumnCount);
            return new RmsProp(_decay, cache, prev);
        }
    }
}
