using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor.GradientDescent
{
    /// <summary>
    /// Creates an AdaGrad gradient descent optimisation
    /// </summary>
    class AdaGradDescriptor : ICreateTemplateBasedGradientDescent
    {
        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateZeroMatrix(template.RowCount, template.ColumnCount);
            return new AdaGrad(cache, prev);
        }
    }
}
