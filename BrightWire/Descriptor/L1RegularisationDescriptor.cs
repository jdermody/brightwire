using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates L1 regularisation
    /// </summary>
    internal class L1RegularisationDescriptor(float lambda) : ICreateGradientDescent
    {
        public IGradientDescentOptimisation Create(IPropertySet propertySet)
        {
            return new L1Regularisation(lambda);
        }
    }
}
