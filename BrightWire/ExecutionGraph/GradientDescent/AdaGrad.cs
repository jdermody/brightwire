using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    class AdaGrad : IGradientDescentOptimisation
    {
        protected readonly IMatrix _cache;
        protected readonly IGradientDescentOptimisation _updater;

        public AdaGrad(IMatrix cache, IGradientDescentOptimisation updater)
        {
            _cache = cache;
            _updater = updater;
        }

        public virtual void Dispose()
        {
            _cache.Dispose();
        }

        public virtual void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            using (var deltaSquared = delta.PointwiseMultiply(delta)) {
                _cache.AddInPlace(deltaSquared);

                using (var cachedSqrt = _cache.Sqrt(1e-8f))
                using (var delta2 = delta.PointwiseDivide(cachedSqrt)) {
                    _updater.Update(source, delta2, context);
                }
            }
        }
    }
}
