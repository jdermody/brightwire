using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Gradient descent with nesterov momentum
    /// </summary>
    internal class NesterovMomentum : Momentum
    {
        public NesterovMomentum(float momentum, IFloatMatrix cache, IGradientDescentOptimisation updater) : base(momentum, cache, updater)
        {
        }

        public override void Update(IFloatMatrix source, IFloatMatrix delta, ILearningContext context)
        {
            using var previousVelocity = _cache.Clone();
            _cache.AddInPlace(delta, _momentum);
            previousVelocity.AddInPlace(_cache, -_momentum, 1 + _momentum);
            _updater.Update(source, previousVelocity, context);
        }
    }
}
