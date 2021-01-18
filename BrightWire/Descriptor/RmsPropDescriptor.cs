using BrightData;
using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates a rms prop gradient descent optimiser
    /// </summary>
    internal class RmsPropDescriptor : ICreateTemplateBasedGradientDescent
    {
        readonly float _decay;

        public RmsPropDescriptor(float decay = 0.9f)
        {
            _decay = decay;
        }

        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IFloatMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateZeroMatrix(template.RowCount, template.ColumnCount);
            return new RmsProp(_decay, cache, prev);
        }
    }
}
