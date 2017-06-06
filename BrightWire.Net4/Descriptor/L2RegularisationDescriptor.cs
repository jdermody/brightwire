using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates L2 regularisation
    /// </summary>
    class L2RegularisationDescriptor : ICreateGradientDescent
    {
        readonly float _lambda;

        public L2RegularisationDescriptor(float lambda)
        {
            _lambda = lambda;
        }

        public IGradientDescentOptimisation Create(IPropertySet propertySet)
        {
            return new L2Regularisation(_lambda);
        }
    }
}
