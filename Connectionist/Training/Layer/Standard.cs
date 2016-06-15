using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace BrightWire.Connectionist.Training.Layer
{
    internal class Standard : INeuralNetworkLayer
    {
        protected readonly IVector _bias;
        protected readonly IMatrix _weight;
        protected readonly IActivationFunction _activation;
        protected readonly INeuralNetworkLayerDescriptor _descriptor;

        public Standard(ILinearAlgebraProvider lap, int inputSize, int outputSize, INeuralNetworkLayerDescriptor init, IActivationFunction activation, IWeightInitialisation weightInit)
        {
            _descriptor = init;
            _activation = activation;

            // initialise weights and bias
            _bias = lap.Create(outputSize, x => weightInit.GetBias());
            _weight = lap.Create(inputSize, outputSize, (x, y) => weightInit.GetWeight(inputSize, outputSize, x, y));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                _bias.Dispose();
                _weight.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int InputSize { get { return _weight.RowCount; } }
        public int OutputSize { get { return _weight.ColumnCount; } }
        public IVector Bias { get { return _bias; } }
        public IMatrix Weight { get { return _weight; } }
        public IActivationFunction Activation { get { return _activation; } }
        public INeuralNetworkLayerDescriptor Descriptor { get { return _descriptor; } }

        public NetworkLayer LayerInfo
        {
            get
            {
                var ret = _descriptor.AsLayer;
                ret.InputSize = InputSize;
                ret.OutputSize = OutputSize;
                ret.Bias = _bias.Data;
                ret.Weight = _weight.Data;
                return ret;
            }

            set
            {
                // only load the weights and bias - assume that other parameters are "tweakable"
                _bias.Data = value.Bias;
                _weight.Data = value.Weight;
            }
        }

        public IMatrix Activate(IMatrix input)
        {
            if (_activation == null)
                return Execute(input);

            using (var preActivation = Execute(input))
                return _activation.Calculate(preActivation);
        }

        public IMatrix Execute(IMatrix input)
        {
            var ret = input.Multiply(_weight);
            ret.AddToEachRow(_bias);
            return ret;
        }

        public void Update(IMatrix biasDelta, IMatrix weightDelta, float weightCoefficient, float learningRate)
        {
            if (biasDelta != null) {
                using (var columnSums = biasDelta.ColumnSums())
                    _bias.AddInPlace(columnSums, 1f / columnSums.Count, learningRate);
            }
            _weight.AddInPlace(weightDelta, weightCoefficient, learningRate);
        }
    }
}
