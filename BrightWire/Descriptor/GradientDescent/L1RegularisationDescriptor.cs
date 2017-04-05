using BrightWire.ExecutionGraph.GradientDescent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.GradientDescent
{
    public class L1RegularisationDescriptor : ICreateGradientDescent
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
