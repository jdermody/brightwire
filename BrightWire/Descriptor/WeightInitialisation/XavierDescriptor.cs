using BrightWire.ExecutionGraph.WeightInitialisation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.WeightInitialisation
{
    /// <summary>
    /// http://andyljones.tumblr.com/post/110998971763/an-explanation-of-xavier-initialization
    /// </summary>
    public class XavierDescriptor : ICreateWeightInitialisation
    {
        readonly float _parameter;

        public XavierDescriptor(float parameter = 6)
        {
            _parameter = parameter;
        }

        public IWeightInitialisation Create(IPropertySet propertySet)
        {
            return new Xavier(propertySet.LinearAlgebraProvider, _parameter);
        }
    }
}
