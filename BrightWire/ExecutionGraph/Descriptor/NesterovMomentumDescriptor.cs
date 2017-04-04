using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Descriptor
{
    class NesterovMomentumDescriptor : MomentumDescriptor
    {
        public override IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            return base.Create(prev, template, propertySet);
        }
    }
}
