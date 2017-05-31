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

        public virtual void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            _Update(source, delta, context, 1f);
        }

        protected void _Update(IMatrix source, IMatrix delta, ILearningContext context, float coefficient)
        {
            source.AddInPlace(delta, coefficient, context.LearningRate);
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
