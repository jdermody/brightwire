using System;
using System.Collections.Generic;
using System.Text;
using BrightWire.Models;
using System.Linq;

namespace BrightWire.Connectionist.Training.Layer.Convolutional
{
    internal class ConvolutionalLayer : IConvolutionalLayer
    {
        public class Backpropagation : IConvolutionalLayerBackpropagation
        {
            readonly ConvolutionalLayer _layer;
            readonly INeuralNetworkLayerTrainer _trainer;
            readonly IMatrix _input, _output;

            public Backpropagation(ConvolutionalLayer layer, INeuralNetworkLayerTrainer trainer, IMatrix input, IMatrix output)
            {
                _layer = layer;
                _trainer = trainer;
                _input = input;
                _output = output;
            }

            public IMatrix Execute(IMatrix error, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updateAccumulator)
            {
                using (var output = _output)
                using (var input = _input) {
                    return Convert(_trainer.Backpropagate(input, output, error, context, calculateOutput, updateAccumulator));
                }
            }

            public IMatrix Convert(IMatrix matrix)
            {
                if (matrix == null)
                    return null;

                var ret = new List<IVector>();
                IVector curr = null;
                var size = _layer._descriptor.FilterSize;

                for (int i = 0, len = matrix.ColumnCount; i < len; i++) {
                    var column = matrix.Column(i);
                    if (i % size == 0) {
                        curr = column;
                        ret.Add(curr);
                    }
                    else
                        curr.AddInPlace(column);
                }
                using(var ret2 = _layer._lap.Create(ret))
                    return ret2.Transpose();
            }

            public int RowCount { get { return _output.RowCount; } }
            public int ColumnCount { get { return _output.ColumnCount; } }
        }
        class InternalLayer : INeuralNetworkLayer
        {
            readonly IMatrix _weight;
            readonly IVector _bias;
            readonly int _inputSize, _outputSize;
            readonly IActivationFunction _activation;
            readonly ConvolutionDescriptor _descriptor;
            readonly bool _disableUpdate;

            public InternalLayer(ILinearAlgebraProvider lap, int inputSize, int outputSize, IActivationFunction activation, ConvolutionDescriptor descriptor, bool disableUpdate)
            {
                _inputSize = inputSize;
                _outputSize = outputSize;
                _activation = activation;
                _descriptor = descriptor;
                _disableUpdate = disableUpdate;

                var weightInit = lap.NN.GetWeightInitialisation(descriptor.WeightInitialisation);
                _bias = lap.Create(outputSize, x => weightInit.GetBias());
                _weight = lap.Create(inputSize, outputSize, (x, y) => weightInit.GetWeight(inputSize, outputSize, x, y));
            }

            public IActivationFunction Activation
            {
                get
                {
                    return _activation;
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
                    return _descriptor;
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
                    return _weight;
                }
            }

            public IMatrix Activate(IMatrix input)
            {
                if (_activation == null)
                    return Execute(input);

                using (var preActivation = Execute(input))
                    return _activation.Calculate(preActivation);
            }

            public void Dispose()
            {
                _bias.Dispose();
                _weight.Dispose();
            }

            public IMatrix Execute(IMatrix input)
            {
                var ret = input.Multiply(_weight);
                ret.AddToEachRow(_bias);
                return ret;
            }

            public void Update(IMatrix biasDelta, IMatrix weightDelta, float weightCoefficient, float learningRate)
            {
                if (!_disableUpdate) {
                    if (biasDelta != null) {
                        using (var columnSums = biasDelta.ColumnSums())
                            _bias.AddInPlace(columnSums, 1f / columnSums.Count, learningRate);
                    }
                    _weight.AddInPlace(weightDelta, weightCoefficient, learningRate);
                }
            }

            public IMatrix CalculateErrorSignal(IMatrix delta)
            {
                //using (var rotatedFilter = _weight.RotateColumns180(_descriptor.FilterWidth)) {
                //    var ret = delta.TransposeAndMultiply(rotatedFilter);
                //    return ret;
                //}
                return delta.TransposeAndMultiply(_weight);
            }
        }

        readonly INeuralNetworkLayerTrainer _trainer;
        readonly ConvolutionDescriptor _descriptor;
        readonly ILinearAlgebraProvider _lap;
        readonly int _inputWidth;

        public ConvolutionalLayer(INeuralNetworkFactory factory, ConvolutionDescriptor descriptor, int inputDepth, int inputWidth, bool disableUpdate = false)
        {
            _inputWidth = inputWidth;
            _lap = factory.LinearAlgebraProvider;
            _descriptor = descriptor;
            var activation = factory.GetActivation(descriptor.Activation);
            var layer = new InternalLayer(factory.LinearAlgebraProvider, descriptor.FilterSize * inputDepth, descriptor.FilterDepth, activation, descriptor, disableUpdate);
            _trainer = factory.CreateTrainer(layer, descriptor);
        }

        IMatrix _Execute(I3DTensor tensor, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            var layer = _trainer.LayerUpdater.Layer;
            IMatrix input;
            if (_descriptor.Padding > 0) {
                using (var padded = tensor.AddPadding(_descriptor.Padding))
                    input = padded.Im2Col(_descriptor.FilterWidth, _descriptor.FilterHeight, _descriptor.Stride);
            } else
                input = tensor.Im2Col(_descriptor.FilterWidth, _descriptor.FilterHeight, _descriptor.Stride);

            var output = layer.Execute(input);
            if (backpropagation != null) {
                backpropagation?.Push(new Backpropagation(this, _trainer, input, output));
                return layer.Activation?.Calculate(output) ?? output;
            }
            else {
                input.Dispose();
                if (layer.Activation == null)
                    return output;
                else {
                    var ret = layer.Activation.Calculate(output);
                    output.Dispose();
                    return ret;
                }
            }
        }

        public I3DTensor ConvertToTensor(IMatrix matrix)
        {
            var sliceList = new List<IMatrix>();
            for (int i = 0, len = matrix.ColumnCount; i < len; i++) {
                using (var vector = matrix.Column(i)) {
                    var parts = vector.Split(_inputWidth);
                    //var sliceMatrix = _lap.Create(parts).Transpose();
                    var sliceMatrix = _lap.Create(parts);
                    sliceList.Add(sliceMatrix);
                    foreach (var part in parts)
                        part.Dispose();
                }
            }
            var ret = _lap.CreateTensor(sliceList);
            foreach (var slice in sliceList)
                slice.Dispose();
            return ret;
        }

        public I3DTensor ExecuteToTensor(I3DTensor tensor, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            using (var outputMatrix = _Execute(tensor, backpropagation))
                return ConvertToTensor(outputMatrix);
        }

        public IVector ExecuteToVector(I3DTensor tensor, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            var outputMatrix = _Execute(tensor, backpropagation);
            return outputMatrix.ConvertInPlaceToVector();
        }

        public void Dispose()
        {
            _trainer.Dispose();
        }

        public ConvolutionalNetwork.Layer Layer
        {
            get
            {
                return new ConvolutionalNetwork.Layer {
                    Type = ConvolutionalNetwork.ConvolutionalLayerType.Convolutional,
                    Data = _trainer.LayerUpdater.Layer.LayerInfo,
                    FilterHeight = _descriptor.FilterHeight,
                    FilterWidth = _descriptor.FilterWidth,
                    Padding = _descriptor.Padding,
                    Stride = _descriptor.Stride
                };
            }
        }
    }
}
