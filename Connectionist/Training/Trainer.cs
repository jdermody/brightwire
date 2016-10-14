using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Training
{
    internal class StandardTrainer : INeuralNetworkLayerTrainer
    {
        readonly bool _verifyDerivatives;
        protected readonly INeuralNetworkLayerUpdater _layerUpdater;
        protected IMatrix _layerInput = null, _layerOutput = null;

        public StandardTrainer(INeuralNetworkLayerUpdater layerUpdater, bool verifyDerivatives = false)
        {
            _layerUpdater = layerUpdater;
            _verifyDerivatives = verifyDerivatives;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _layerUpdater.Dispose();
                _layerOutput?.Dispose();
                _layerInput?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public INeuralNetworkLayerUpdater LayerUpdater { get { return _layerUpdater; } }

        protected virtual void _Reset()
        {
            Debug.Assert(_layerInput != null && _layerOutput != null);
            _layerOutput?.Dispose();
            //_layerInput?.Dispose();
            _layerInput = _layerOutput = null;
        }

        protected virtual void _ExecuteLayer(IMatrix input, bool backpropagation)
        {
            var layer = _layerUpdater.Layer;
            _layerOutput = input.Multiply(layer.Weight);
            _layerOutput.AddToEachRow(layer.Bias);
        }

        protected virtual IMatrix _ActivateLayer(bool backpropagation)
        {
            if (_layerUpdater.Layer.Activation != null)
                return _layerUpdater.Layer.Activation.Calculate(_layerOutput);
            else
                return _layerOutput.Clone();
        }

        public IMatrix FeedForward(IMatrix input, bool storeForBackpropagation)
        {
            Debug.Assert(_layerInput == null && _layerOutput == null);

            // store the input
            _layerInput = input;

            // execute the layer
            _ExecuteLayer(input, storeForBackpropagation);

            // return the activated layer output
            var ret = _ActivateLayer(storeForBackpropagation);

            if (!storeForBackpropagation)
                _Reset();
            return ret;
        }

        public IMatrix Backpropagate(IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates = null)
        {
            Debug.Assert(_layerInput != null && _layerOutput != null);
            IMatrix ret = Backpropagate(_layerInput, _layerOutput, errorSignal, context, calculateOutput, updates);
            _Reset();
            return ret;
        }

        public IMatrix Backpropagate(IMatrix input, IMatrix output, IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates = null)
        {
            IMatrix ret = null;

            // calculate the derivative to determine the gradients
            using (var od = _layerUpdater.Layer.Activation?.Derivative(output, errorSignal) ?? output) {
                if (_verifyDerivatives) {
                    var dError = _VerifyDerivatives(od);
                    Debug.Assert(dError < 0.001f);
                }

                // use the gradients to determine the direction of each change
                var delta = errorSignal.PointwiseMultiply(od);
                if (calculateOutput)
                    ret = delta.TransposeAndMultiply(_layerUpdater.Layer.Weight);

                // update the layer
                _UpdateLayer(input, delta, context, updates);
            }
            return ret;
        }

        protected virtual void _UpdateLayer(IMatrix input, IMatrix delta, ITrainingContext context, INeuralNetworkUpdateAccumulator updates)
        {
            var weightUpdate = input.TransposeThisAndMultiply(delta);
            if (updates != null)
                updates.Record(_layerUpdater, delta, weightUpdate);
            else {
                _layerUpdater.Update(delta, weightUpdate, context);
                weightUpdate.Dispose();
                delta.Dispose();
            }
        }

        double _VerifyDerivatives(IMatrix activationDerivative)
        {
            const float epsilon = 0.0001f;
            var activation = _layerUpdater.Layer.Activation;

            return _layerOutput.AsIndexable().Values.Zip(activationDerivative.AsIndexable().Values, (val, valD) => {
                var approximatedDerivative = (activation.Calculate(val + epsilon) - activation.Calculate(val - epsilon)) / (2 * epsilon);
                return Math.Abs(valD - approximatedDerivative);
            }).Average();
        }
    }

    internal abstract class FilteredTrainer : StandardTrainer
    {
        protected readonly Bernoulli _probabilityDistribution;
        protected readonly float _invertedMultiplier;
        protected readonly ILinearAlgebraProvider _lap;
        protected IMatrix _filter;
        protected IVector _filter2;

        public FilteredTrainer(ILinearAlgebraProvider lap, INeuralNetworkLayerUpdater layerUpdater, float ratio)
            : base(layerUpdater)
        {
            _lap = lap;
            _invertedMultiplier = 1;// - ratio;
            _probabilityDistribution = new Bernoulli(_invertedMultiplier);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                _filter?.Dispose();
                _filter2?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void _Reset()
        {
            base._Reset();
            _filter?.Dispose();
            _filter2?.Dispose();
            _filter = null;
            _filter2 = null;
        }

        protected void _CreateFilter(IMatrix matrix)
        {
            Debug.Assert(_filter == null);

            // create a row level probability
            //var dropout = Enumerable.Range(0, matrix.ColumnCount).Select(v => _probabilityDistribution.Sample() / _invertedMultiplier).ToArray();

            // create a filter against the dropout probability
            _filter = _lap.Create(matrix.RowCount, matrix.ColumnCount, (x, y) => _probabilityDistribution.Sample() / _invertedMultiplier);
        }

        protected void _CreateFilter(IVector vector)
        {
            Debug.Assert(_filter2 == null);

            // create a filter against the dropout probability
            _filter2 = _lap.Create(vector.Count, i => _probabilityDistribution.Sample() / _invertedMultiplier);
        }
    }

    internal class DropoutTrainer : FilteredTrainer
    {
        public DropoutTrainer(ILinearAlgebraProvider lap, INeuralNetworkLayerUpdater layerUpdater, float ratio)
            : base(lap, layerUpdater, ratio)
        {

        }

        protected override void _ExecuteLayer(IMatrix input, bool backpropagation)
        {
            base._ExecuteLayer(input, backpropagation);
            if (backpropagation) {
                _CreateFilter(_layerOutput);
                var filteredOutput = _filter.PointwiseMultiply(_layerOutput);
                _layerOutput.Dispose();
                _layerOutput = filteredOutput;
            }
        }
    }

    internal class DropConnectTrainer : FilteredTrainer
    {
        public DropConnectTrainer(ILinearAlgebraProvider lap, INeuralNetworkLayerUpdater layerUpdater, float ratio)
            : base(lap, layerUpdater, ratio)
        {
        }

        protected override void _ExecuteLayer(IMatrix input, bool backpropagation)
        {
            if (backpropagation) {
                var layer = _layerUpdater.Layer;

                // create filters for the weights and bias
                _CreateFilter(layer.Weight);
                _CreateFilter(layer.Bias);

                // filter the weights and bias
                using (var filteredWeights = _filter.PointwiseMultiply(layer.Weight))
                using (var filteredBias = _filter2.PointwiseMultiply(layer.Bias)) {
                    _layerOutput = input.Multiply(filteredWeights);
                    _layerOutput.AddToEachRow(filteredBias);
                }
            }
            else
                base._ExecuteLayer(input, backpropagation);
        }

        protected override void _UpdateLayer(IMatrix input, IMatrix delta, ITrainingContext context, INeuralNetworkUpdateAccumulator updates)
        {
            if (_filter != null && _filter2 != null) {
                // filter the updates to the bias against the filter
                using (var columnSums = delta.ColumnSums())
                using (var filteredColumnSums = columnSums.PointwiseMultiply(_filter2))
                    _layerUpdater.Layer.Bias.AddInPlace(filteredColumnSums, 1f / columnSums.Count);

                // filter the weight updates against the filter
                using (var weightUpdate = input.TransposeThisAndMultiply(delta))
                using (var filteredWeightUpdate = weightUpdate.PointwiseMultiply(_filter))
                    _layerUpdater.Update(null, filteredWeightUpdate, context);
            }
            else
                base._UpdateLayer(input, delta, context, updates);
        }
    }

    internal class TrainerFactory
    {
        readonly ILinearAlgebraProvider _lap;

        public TrainerFactory(ILinearAlgebraProvider lap)
        {
            _lap = lap;
        }

        public INeuralNetworkLayerTrainer Standard(INeuralNetworkLayerUpdater layerUpdater)
        {
            return new StandardTrainer(layerUpdater);
        }

        public INeuralNetworkLayerTrainer Dropout(INeuralNetworkLayerUpdater layerUpdater, float ratio)
        {
            return new DropoutTrainer(_lap, layerUpdater, ratio);
        }

        public INeuralNetworkLayerTrainer DropConnect(INeuralNetworkLayerUpdater layerUpdater, float ratio)
        {
            return new DropConnectTrainer(_lap, layerUpdater, ratio);
        }
    }
}
