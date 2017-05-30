using BrightWire.ExecutionGraph.GradientDescent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Descriptor.GradientDescent
{
    class AdaGradDescriptor : ICreateTemplateBasedGradientDescent
    {
        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateZeroMatrix(template.RowCount, template.ColumnCount);
            return new AdaGrad(cache, prev);
        }
    }
}
