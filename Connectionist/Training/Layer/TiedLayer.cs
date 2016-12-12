using System;
using System.Collections.Generic;
using System.Text;
using BrightWire.Models;

namespace BrightWire.Connectionist.Training.Layer
{
    internal class TiedLayer : INeuralNetworkLayer
    {
        readonly IVector _bias;
        readonly IMatrix _weight;
        readonly INeuralNetworkLayer _layer;
        readonly int _inputSize, _outputSize;
        IMatrix _weightTranspose;

        public TiedLayer(ILinearAlgebraProvider lap, INeuralNetworkLayer layer, IWeightInitialisation weightInit)
        {
            _inputSize = layer.OutputSize;
            _outputSize = layer.InputSize;
            _layer = layer;
            _weight = layer.Weight;
            _bias = lap.Create(_outputSize, x => weightInit.GetBias());
            _weightTranspose = _weight.Transpose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                _bias.Dispose();
                _weightTranspose.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IActivationFunction Activation
        {
            get
            {
                return _layer.Activation;
            }
        }

        public IVector Bias
        {
            get
            {
                return _bias;
            }
        }

        public LayerDescriptor Descriptor
        {
            get
            {
                return _layer.Descriptor;
            }
        }

        public int InputSize
        {
            get
            {
                return _inputSize;
            }
        }

        public NetworkLayer LayerInfo
        {
            get
            {
                var ret = _layer.Descriptor.AsLayer;
                ret.InputSize = InputSize;
                ret.OutputSize = OutputSize;
                ret.Bias = _bias.Data;
                ret.Weight = _weightTranspose.Data;
                return ret;
            }

            set
            {
                _bias.Data = value.Bias;
                _weightTranspose.Data = value.Weight;
            }
        }

        public int OutputSize
        {
            get
            {
                return _outputSize;
            }
        }

        public IMatrix Weight
        {
            get
            {
                return _weightTranspose;
            }
        }

        public IMatrix Activate(IMatrix input)
        {
            var activation = _layer.Activation;
            if (activation == null)
                return Execute(input);

            using (var preActivation = Execute(input))
                return activation.Calculate(preActivation);
        }

        public IMatrix Execute(IMatrix input)
        {
            var ret = input.TransposeAndMultiply(_weight);
            ret.AddToEachRow(_bias);
            return ret;
        }

        public void Update(IMatrix biasDelta, IMatrix weightDelta, float weightCoefficient, float learningRate)
        {
            if (biasDelta != null) {
                using (var columnSums = biasDelta.ColumnSums())
                    _bias.AddInPlace(columnSums, 1f / columnSums.Count, learningRate);
            }
            using(var transpose = weightDelta.Transpose())
                _weight.AddInPlace(transpose, weightCoefficient, learningRate);

            _weightTranspose.Dispose();
            _weightTranspose = _weight.Transpose();
        }

        public IMatrix CalculateErrorSignal(IMatrix delta)
        {
            return delta.TransposeAndMultiply(_weight);
        }
    }
}
