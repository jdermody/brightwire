using BrightData;
using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates an adam gradient descent optimisation
    /// </summary>
    internal class AdamDescriptor : ICreateTemplateBasedGradientDescent
    {
        readonly float _decay, _decay2;

        public AdamDescriptor(float decay = 0.9f, float decay2 = 0.99f)
        {
            _decay = decay;
            _decay2 = decay2;
        }

        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, true);
            var cache2 = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, true);
            return new Adam(_decay, _decay2, cache, cache2, prev);
        }
    }
}
