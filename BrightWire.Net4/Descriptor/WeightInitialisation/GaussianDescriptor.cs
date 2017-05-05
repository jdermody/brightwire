using BrightWire.ExecutionGraph.WeightInitialisation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.WeightInitialisation
{
    public class GaussianDescriptor : ICreateWeightInitialisation
    {
        readonly float _stdDev;

        public GaussianDescriptor(float stdDev = 0.1f)
        {
            _stdDev = stdDev;
        }

        public IWeightInitialisation Create(IPropertySet propertySet)
        {
            return new Gaussian(propertySet.LinearAlgebraProvider, _stdDev);
        }
    }
}
