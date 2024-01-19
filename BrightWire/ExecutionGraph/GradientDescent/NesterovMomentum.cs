using BrightData;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    /// <summary>
    /// Gradient descent with nesterov momentum
    /// </summary>
    internal class NesterovMomentum(float momentum, IMatrix cache, IGradientDescentOptimisation updater)
        : Momentum(momentum, cache, updater)
    {
        public override void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            using var previousVelocity = _cache.Clone();
            _cache.AddInPlace(delta, _momentum, 1f);
            previousVelocity.AddInPlace(_cache, -_momentum, 1 + _momentum);
            _updater.Update(source, previousVelocity, context);
        }
    }
}
