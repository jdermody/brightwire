using BrightWire.ExecutionGraph.GradientDescent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Descriptor
{
    public class MomentumDescriptor : ITemplateBasedCreateGradientDescent
    {
        readonly float _momentum;

        public MomentumDescriptor(float momentum = 0.9f)
        {
            _momentum = momentum;
        }

        public virtual IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.Create(template.RowCount, template.ColumnCount, 0f);
            return new Momentum(_momentum, cache, prev);
        }
    }
}
