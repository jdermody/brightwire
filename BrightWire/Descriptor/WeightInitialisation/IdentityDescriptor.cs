using BrightWire.ExecutionGraph.WeightInitialisation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.WeightInitialisation
{
    /// <summary>
    /// Identity matrix: https://arxiv.org/abs/1504.00941
    /// </summary>
    public class IdentityDescriptor : ICreateWeightInitialisation
    {
        readonly ILinearAlgebraProvider _lap;
        readonly float _value;

        public IdentityDescriptor(ILinearAlgebraProvider lap, float value)
        {
            _value = value;
            _lap = lap;
        }

        public IWeightInitialisation Create(IPropertySet propertySet)
        {
            return new Identity(_lap, _value);
        }
    }
}
