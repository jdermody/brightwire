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
            var cache = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, 0f);
            var cache2 = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, 0f);
            return new Adam(_decay, _decay2, cache, cache2, prev);
        }
    }
}
