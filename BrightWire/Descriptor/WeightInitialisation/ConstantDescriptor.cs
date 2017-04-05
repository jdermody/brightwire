using BrightWire.ExecutionGraph.WeightInitialisation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.WeightInitialisation
{
    public class ConstantDescriptor : ICreateWeightInitialisation
    {
        readonly float _biasValue, _weightValue;

        public ConstantDescriptor(float biasValue, float weightValue)
        {
            _biasValue = biasValue;
            _weightValue = weightValue;
        }

        public IWeightInitialisation Create(IPropertySet propertySet)
        {
            return new Constant(propertySet.LinearAlgebraProvider, _biasValue, _weightValue);
        }
    }
}
