using BrightWire.ExecutionGraph.GradientDescent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.GradientDescent
{
    public class L2RegularisationDescriptor : ICreateGradientDescent
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
