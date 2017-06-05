using System.IO;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Rms prop gradient descent
    /// https://en.wikipedia.org/wiki/Stochastic_gradient_descent#RMSProp
    /// </summary>
    class RmsProp : AdaGrad
    {
        protected float _decayRate;

        public RmsProp(float decayRate, IMatrix cache, IGradientDescentOptimisation updater) : base(cache, updater)
        {
            _decayRate = decayRate;
        }

        public override void Update(IMatrix source, IMatrix delta, ILearningContext context, bool hasAveragedBatchSize)
        {
            //if(!hasAveragedBatchSize)
            //    delta.Multiply(1f / context.BatchSize);

            using (var deltaSquared = delta.PointwiseMultiply(delta)) {
                _cache.AddInPlace(deltaSquared, _decayRate, 1 - _decayRate);

                using (var cachedSqrt = _cache.Sqrt(1e-8f))
                using (var delta2 = delta.PointwiseDivide(cachedSqrt)) {
                    _updater.Update(source, delta2, context, false);
                }
            }
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
