using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.GradientDescent
{
    class Momentum : AdaGrad
    {
        protected readonly float _momentum;
        
        public Momentum(float momentum, IMatrix cache, IGradientDescentOptimisation updater) : base(cache, updater)
        {
            _momentum = momentum;
        }

        public override void Update(IMatrix source, IMatrix delta, ILearningContext context)
        {
            _cache.AddInPlace(delta, 1f, _momentum);
            _updater.Update(source, _cache, context);
        }
    }
}
