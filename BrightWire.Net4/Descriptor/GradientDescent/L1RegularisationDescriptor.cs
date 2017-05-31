using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor.GradientDescent
{
    /// <summary>
    /// Creates L1 regularisation
    /// </summary>
    class L1RegularisationDescriptor : ICreateGradientDescent
    {
        readonly float _lambda;

        public L1RegularisationDescriptor(float lambda)
        {
            _lambda = lambda;
        }

        public IGradientDescentOptimisation Create(IPropertySet propertySet)
        {
            return new L1Regularisation(_lambda);
        }
    }
}
