using BrightWire.ExecutionGraph.GradientDescent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.GradientDescent
{
    class MomentumDescriptor : ICreateTemplateBasedGradientDescent
    {
        protected readonly float _momentum;

        public MomentumDescriptor(float momentum = 0.9f)
        {
            _momentum = momentum;
        }

        public virtual IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateZeroMatrix(template.RowCount, template.ColumnCount);
            return new Momentum(_momentum, cache, prev);
        }
    }
}
