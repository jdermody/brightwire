﻿using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Simple SGD
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent
    /// </summary>
    internal class StochasticGradientDescent : IGradientDescentOptimisation
    {
        public void Dispose()
        {
            // nop
        }

        public virtual void Update(IMatrix<float> source, IMatrix<float> delta, ILearningContext context)
        {
            Update(source, delta, context, 1f, context.LearningRate);
        }

        protected void Update(IMatrix<float> source, IMatrix<float> delta, ILearningContext _, float coefficient1, float coefficient2)
        {
            source.AddInPlace(delta, coefficient1, coefficient2);
        }

        public virtual void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            // nop
        }

        public virtual void WriteTo(BinaryWriter writer)
        {
            // nop
        }
    }
}
