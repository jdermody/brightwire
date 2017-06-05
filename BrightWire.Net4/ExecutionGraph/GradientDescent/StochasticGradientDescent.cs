using System.IO;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Simple SGD
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent
    /// </summary>
    class StochasticGradientDescent : IGradientDescentOptimisation
    {
        public void Dispose()
        {
            // nop
        }

        public virtual void Update(IMatrix source, IMatrix delta, ILearningContext context, bool hasAveragedBatchSize)
        {
            float coefficient = 1f;
            //if (!hasAveragedBatchSize)
            //    coefficient /= context.BatchSize;
            _Update(source, delta, context, 1f, coefficient);
        }

        protected void _Update(IMatrix source, IMatrix delta, ILearningContext context, float coefficient1, float coefficient2)
        {
            source.AddInPlace(delta, coefficient1, context.LearningRate * coefficient2);
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
