using BrightWire.ExecutionGraph.GradientDescent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Descriptor
{
    class RmsPropDescriptor : ITemplateBasedCreateGradientDescent
    {
        readonly float _decay;

        public RmsPropDescriptor(float decay)
        {
            _decay = decay;
        }

        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.Create(template.RowCount, template.ColumnCount, 0f);
            return new RmsProp(_decay, cache, prev);
        }
    }
}
