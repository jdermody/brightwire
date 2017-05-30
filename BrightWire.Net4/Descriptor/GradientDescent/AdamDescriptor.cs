using BrightWire.ExecutionGraph.GradientDescent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.GradientDescent
{
    class AdamDescriptor : ICreateTemplateBasedGradientDescent
    {
        readonly float _decay, _decay2;

        public AdamDescriptor(float decay = 0.9f, float decay2 = 0.99f)
        {
            _decay = decay;
            _decay2 = decay2;
        }

        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateZeroMatrix(template.RowCount, template.ColumnCount);
            var cache2 = propertySet.LinearAlgebraProvider.CreateZeroMatrix(template.RowCount, template.ColumnCount);
            return new Adam(_decay, _decay2, cache, cache2, prev);
        }
    }
}
