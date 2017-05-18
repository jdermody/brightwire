using BrightWire.ExecutionGraph.GradientDescent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.GradientDescent
{
    class NesterovMomentumDescriptor : MomentumDescriptor
    {
        public NesterovMomentumDescriptor(float momentum = 0.9f) : base(momentum)
        {
        }

        public override IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, 0f);
            return new NesterovMomentum(_momentum, cache, prev);
        }
    }
}
