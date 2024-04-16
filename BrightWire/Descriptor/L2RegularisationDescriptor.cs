using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates L2 regularisation
    /// </summary>
    internal class L2RegularisationDescriptor(float lambda) : ICreateGradientDescent
    {
        public IGradientDescentOptimisation Create(IPropertySet propertySet)
        {
            return new L2Regularisation(lambda);
        }
    }
}
