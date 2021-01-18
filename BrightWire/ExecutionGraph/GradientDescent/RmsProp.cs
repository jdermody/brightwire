using System.IO;
using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Rms prop gradient descent
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent#RMSProp
    /// </summary>
    internal class RmsProp : AdaGrad
    {
        protected float _decayRate;

        public RmsProp(float decayRate, IFloatMatrix cache, IGradientDescentOptimisation updater) : base(cache, updater)
        {
            _decayRate = decayRate;
        }

        public override void Update(IFloatMatrix source, IFloatMatrix delta, ILearningContext context)
        {
            using var deltaSquared = delta.PointwiseMultiply(delta);
            _cache.AddInPlace(deltaSquared, _decayRate, 1 - _decayRate);

            using var cachedSqrt = _cache.Sqrt();
            using var delta2 = delta.PointwiseDivide(cachedSqrt);
            _updater.Update(source, delta2, context);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            base.ReadFrom(factory, reader);
            _decayRate = reader.ReadSingle();
        }

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(_decayRate);
        }
    }
}
