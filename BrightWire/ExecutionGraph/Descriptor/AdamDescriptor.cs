using BrightWire.ExecutionGraph.GradientDescent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Descriptor
{
    public class AdamDescriptor : ITemplateBasedCreateGradientDescent
    {
        readonly float _decay, _decay2;

        public AdamDescriptor(float decay, float decay2)
        {
            _decay = decay;
            _decay2 = decay2;
        }

        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.Create(template.RowCount, template.ColumnCount, 0f);
            var cache2 = propertySet.LinearAlgebraProvider.Create(template.RowCount, template.ColumnCount, 0f);
            return new Adam(_decay, _decay2, cache, cache2, prev);
        }
    }
}
