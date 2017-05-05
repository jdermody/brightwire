using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    class Adam : RmsProp
    {
        readonly float _decayRate2;
        readonly IMatrix _cache2;

        public Adam(float decay, float decay2, IMatrix cache, IMatrix cache2, IGradientDescentOptimisation updater) : base(decay, cache, updater)
        {
            _decayRate2 = decay2;
            _cache2 = cache2;
        }

        public override void Dispose()
        {
            _cache2.Dispose();
            base.Dispose();
        }

        public override void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            var t = context.CurrentEpoch;
            using (var deltaSquared = delta.PointwiseMultiply(delta)) {
                _cache.AddInPlace(delta, _decayRate, 1 - _decayRate);
                _cache2.AddInPlace(deltaSquared, _decayRate2, 1 - _decayRate2);

                using (var mb = _cache.Clone())
                using (var vb = _cache2.Clone()) {
                    mb.Multiply(1f / (1f - Convert.ToSingle(Math.Pow(_decayRate, t))));
                    vb.Multiply(1f / (1f - Convert.ToSingle(Math.Pow(_decayRate2, t))));
                    using (var vbSqrt = vb.Sqrt(1e-8f))
                    using (var delta2 = mb.PointwiseDivide(vbSqrt)) {
                        _updater.Update(source, delta2, context);
                    }
                }
            }
        }
    }
}
