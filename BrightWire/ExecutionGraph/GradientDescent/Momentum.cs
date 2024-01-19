using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Gradient descent with momentum
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent#Momentum
    /// </summary>
    internal class Momentum(float momentum, IMatrix cache, IGradientDescentOptimisation updater)
        : AdaGrad(cache, updater)
    {
        protected float _momentum = momentum;

        public override void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            _cache.AddInPlace(delta, 1f, _momentum);
            _updater.Update(source, _cache, context);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            base.ReadFrom(factory, reader);
            _momentum = reader.ReadSingle();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(_momentum);
        }
    }
}
