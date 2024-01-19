using BrightData;
using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates an adam gradient descent optimisation
    /// </summary>
    internal class AdamDescriptor(float decay = 0.9f, float decay2 = 0.99f) : ICreateTemplateBasedGradientDescent
    {
        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, true);
            var cache2 = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, true);
            return new Adam(decay, decay2, cache, cache2, prev);
        }
    }
}
