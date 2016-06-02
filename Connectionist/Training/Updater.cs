using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Connectionist.Training
{
    internal class UpdaterBase : INeuralNetworkLayerUpdater
    {
        protected readonly INeuralNetworkLayer _layer;

        public UpdaterBase(INeuralNetworkLayer layer)
        {
            _layer = layer;
        }

        public INeuralNetworkLayer Layer { get { return _layer; } }

        public virtual void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context)
        {
            Update(biasDelta, weightDelta, 1, context);
        }

        protected void Update(IMatrix biasDelta, IMatrix weightDelta, float weightCoefficient, ITrainingContext context)
        {
            _layer.Update(biasDelta, weightDelta, weightCoefficient, context.TrainingRate / context.MiniBatchSize);
        }

        public void Reset()
        {
            // nop 
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _layer.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    internal class L2RegularisationUpdater : UpdaterBase
    {
        readonly float _lambda;

        public L2RegularisationUpdater(INeuralNetworkLayer layer, float lambda) : base(layer)
        {
            _lambda = lambda;
        }

        public override void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context)
        {
            var l2 = 1.0f - (context.TrainingRate * _lambda / context.TrainingSamples);
            Update(biasDelta, weightDelta, l2, context);
        }
    }

    internal class L1RegularisationUpdater : UpdaterBase
    {
        readonly float _lambda;

        public L1RegularisationUpdater(INeuralNetworkLayer layer, float lambda) : base(layer)
        {
            _lambda = lambda;
        }

        public override void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context)
        {
            var l1 = context.TrainingRate * _lambda / context.TrainingSamples;
            _layer.Weight.L1Regularisation(l1);
            Update(biasDelta, weightDelta, 1f, context);
        }
    }

    internal class MaxNormRegularisationUpdater : UpdaterBase
    {
        readonly float _lambda;
        readonly ILinearAlgebraProvider _lap;

        public MaxNormRegularisationUpdater(ILinearAlgebraProvider lap, INeuralNetworkLayer layer, float lambda) : base(layer)
        {
            _lambda = lambda;
            _lap = lap;
        }

        public override void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context)
        {
            base.Update(biasDelta, weightDelta, context);
            var norms = _layer.Weight.RowL2Norm().AsIndexable();
            var updates = new Dictionary<int, float>();
            var threshold = _lambda * norms.Count;
            for (var i = 0; i < norms.Count; i++) {
                if (norms[i] > threshold)
                    updates.Add(i, norms[i] / _lambda);
            }
            if (updates.Any()) {
                float temp;
                using (var update = _lap.Create(norms.Count, i => updates.TryGetValue(i, out temp) ? temp : 1f))
                    _layer.Weight.PointwiseDivideRows(update);
            }
        }
    }

    internal abstract class PerWeightUpdateBase : INeuralNetworkLayerUpdater
    {
        protected readonly IMatrix _cache;
        protected readonly INeuralNetworkLayerUpdater _layerUpdater;

        internal PerWeightUpdateBase(INeuralNetworkLayerUpdater layerUpdater, ILinearAlgebraProvider lap)
        {
            _layerUpdater = layerUpdater;
            var targetWeight = layerUpdater.Layer.Weight;
            _cache = lap.Create(targetWeight.RowCount, targetWeight.ColumnCount, (x, y) => 0f);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                _cache.Dispose();
                _layerUpdater.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public INeuralNetworkLayer Layer
        {
            get
            {
                return _layerUpdater.Layer;
            }
        }

        public abstract void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context);

        public virtual void Reset()
        {
        }
    }

    internal class MomentumUpdater : PerWeightUpdateBase
    {
        protected readonly float _momentum;

        public MomentumUpdater(INeuralNetworkLayerUpdater layerUpdater, ILinearAlgebraProvider lap, float momentum) : base(layerUpdater, lap)
        {
            _momentum = momentum;
        }

        public override void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context)
        {
            _cache.AddInPlace(weightDelta, _momentum);
            _layerUpdater.Update(biasDelta, _cache, context);
        }
    }

    internal class NesterovMomentumUpdater : MomentumUpdater
    {
        public NesterovMomentumUpdater(INeuralNetworkLayerUpdater layerUpdater, ILinearAlgebraProvider lap, float momentum) : base(layerUpdater, lap, momentum)
        {
        }

        public override void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context)
        {
            using (var previousVelocity = _cache.Clone()) {
                _cache.AddInPlace(weightDelta, _momentum);
                previousVelocity.AddInPlace(_cache, -_momentum, 1 + _momentum); // TODO: verify polarity
                _layerUpdater.Update(biasDelta, previousVelocity, context);
            }
        }
    }

    internal class AdagradUpdater : PerWeightUpdateBase
    {
        public AdagradUpdater(INeuralNetworkLayerUpdater layerUpdater, ILinearAlgebraProvider lap) : base(layerUpdater, lap)
        {
        }

        public override void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context)
        {
            using (var deltaSquared = weightDelta.PointwiseMultiply(weightDelta)) {
                _cache.AddInPlace(deltaSquared);

                using (var cachedSqrt = _cache.Sqrt(1e-8f))
                using (var delta2 = weightDelta.PointwiseDivide(cachedSqrt)) {
                    _layerUpdater.Update(biasDelta, delta2, context);
                }
            }
        }
    }

    internal class RMSpropUpdater : PerWeightUpdateBase
    {
        readonly float _decayRate;

        public RMSpropUpdater(INeuralNetworkLayerUpdater layerUpdater, ILinearAlgebraProvider lap, float decayRate) : base(layerUpdater, lap)
        {
            _decayRate = decayRate;
        }

        public override void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context)
        {
            using (var deltaSquared = weightDelta.PointwiseMultiply(weightDelta)) {
                _cache.AddInPlace(deltaSquared, _decayRate, 1 - _decayRate);

                using (var cachedSqrt = _cache.Sqrt(1e-8f))
                using (var delta2 = weightDelta.PointwiseDivide(cachedSqrt)) {
                    _layerUpdater.Update(biasDelta, delta2, context);
                }
            }
        }
    }

    internal class AdamUpdater : PerWeightUpdateBase
    {
        readonly IMatrix _cache2;
        readonly float _decay, _decay2;

        public AdamUpdater(INeuralNetworkLayerUpdater layerUpdater, ILinearAlgebraProvider lap, float decay, float decay2) : base(layerUpdater, lap)
        {
            _decay = decay;
            _decay2 = decay2;
            var targetWeight = layerUpdater.Layer.Weight;
            _cache2 = lap.Create(targetWeight.RowCount, targetWeight.ColumnCount, (x, y) => 0f);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                _cache2.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context)
        {
            var t = context.CurrentEpoch;
            using (var deltaSquared = weightDelta.PointwiseMultiply(weightDelta)) {
                _cache.AddInPlace(weightDelta, _decay, 1 - _decay);
                _cache2.AddInPlace(deltaSquared, _decay2, 1 - _decay2);

                using (var mb = _cache.Clone())
                using (var vb = _cache2.Clone()) {
                    mb.Multiply(1f / (1f - Convert.ToSingle(Math.Pow(_decay, t))));
                    vb.Multiply(1f / (1f - Convert.ToSingle(Math.Pow(_decay2, t))));
                    using (var vbSqrt = vb.Sqrt(1e-8f))
                    using (var delta = mb.PointwiseDivide(vbSqrt)) {
                        _layerUpdater.Update(biasDelta, delta, context);
                    }
                }
            }
        }
    }

    public class UpdaterFactory
    {
        readonly ILinearAlgebraProvider _lap;

        public UpdaterFactory(ILinearAlgebraProvider lap)
        {
            _lap = lap;
        }

        public INeuralNetworkLayerUpdater Simple(INeuralNetworkLayer layer)
        {
            return new UpdaterBase(layer);
        }

        public INeuralNetworkLayerUpdater L2(INeuralNetworkLayer layer, float lambda)
        {
            return new L2RegularisationUpdater(layer, lambda);
        }

        public INeuralNetworkLayerUpdater L1(INeuralNetworkLayer layer, float lambda)
        {
            return new L1RegularisationUpdater(layer, lambda);
        }

        public INeuralNetworkLayerUpdater MaxNorm(INeuralNetworkLayer layer, float lambda)
        {
            return new MaxNormRegularisationUpdater(_lap, layer, lambda);
        }

        public INeuralNetworkLayerUpdater Momentum(INeuralNetworkLayerUpdater primary, float momentumAmount)
        {
            return new MomentumUpdater(primary, _lap, momentumAmount);
        }

        public INeuralNetworkLayerUpdater NesterovMomentum(INeuralNetworkLayerUpdater primary, float momentumAmount)
        {
            return new NesterovMomentumUpdater(primary, _lap, momentumAmount);
        }

        public INeuralNetworkLayerUpdater Adagrad(INeuralNetworkLayerUpdater primary)
        {
            return new AdagradUpdater(primary, _lap);
        }

        public INeuralNetworkLayerUpdater RMSprop(INeuralNetworkLayerUpdater primary, float decayRate)
        {
            return new RMSpropUpdater(primary, _lap, decayRate);
        }

        public INeuralNetworkLayerUpdater Adam(INeuralNetworkLayerUpdater primary, float beta1, float beta2)
        {
            return new AdamUpdater(primary, _lap, beta1, beta2);
        }
    }
}
