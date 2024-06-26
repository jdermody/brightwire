﻿using BrightData;
using BrightWire.ExecutionGraph.GradientDescent;

namespace BrightWire.Descriptor
{
    /// <summary>
    /// Creates an AdaGrad gradient descent optimisation
    /// </summary>
    internal class AdaGradDescriptor : ICreateTemplateBasedGradientDescent
    {
        public IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix<float> template, IPropertySet propertySet)
        {
            var cache = propertySet.LinearAlgebraProvider.CreateMatrix(template.RowCount, template.ColumnCount, true);
            return new AdaGrad(cache, prev);
        }
    }
}
