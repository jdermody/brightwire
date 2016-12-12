using BrightWire.Models.Convolutional;
using System;
using System.Collections.Generic;
using System.Text;
using BrightWire.Models;

namespace BrightWire.Connectionist.Training.Layer.Convolutional
{
    public class ConvolutionalLayer : IConvolutionalLayer
    {
        public class Backpropagation : IConvolutionalLayerBackpropagation
        {
            readonly INeuralNetworkLayerTrainer _trainer;
            readonly IMatrix _input, _output;

            public Backpropagation(INeuralNetworkLayerTrainer trainer, IMatrix input, IMatrix output)
            {
                _trainer = trainer;
                _input = input;
                _output = output;
            }

            public IMatrix Execute(IMatrix error, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updateAccumulator)
            {
                using (var output = _output)
                    return _trainer.Backpropagate(_input, output, error, context, calculateOutput, updateAccumulator);
            }

            public int RowCount { get { return _output.RowCount; } }
            public int ColumnCount { get { return _output.ColumnCount; } }
        }
        class Layer : INeuralNetworkLayer
        {
            readonly IMatrix _weight;
            readonly IVector _bias;
            readonly int _inputSize, _outputSize;
            readonly IActivationFunction _activation;
            readonly ConvolutionDescriptor _descriptor;

            public Layer(ILinearAlgebraProvider lap, int inputSize, int outputSize, IActivationFunction activation, ConvolutionDescriptor descriptor)
            {
                _inputSize = inputSize;
                _outputSize = outputSize;
                _activation = activation;
                _descriptor = descriptor;

                var weightInit = lap.NN.GetWeightInitialisation(descriptor.WeightInitialisation);
                _bias = lap.Create(outputSize, x => weightInit.GetBias());
                _weight = lap.Create(inputSize, outputSize, (x, y) => weightInit.GetWeight(inputSize, outputSize, x, y));
                //_weight = lap.Create(inputSize, outputSize, (x, y) => x);
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
                if (biasDelta != null) {
                    using (var columnSums = biasDelta.ColumnSums())
                        _bias.AddInPlace(columnSums, 1f / columnSums.Count, learningRate);
                }
                //using (var weightDelta2 = weightDelta.RotateColumns180(_descriptor.FieldSize))
                    _weight.AddInPlace(weightDelta, weightCoefficient, learningRate);
            }

            public IMatrix CalculateErrorSignal(IMatrix delta)
            {
                using(var rotatedFilter = _weight.RotateColumns180(_descriptor.FieldSize))
                    return delta.TransposeAndMultiply(rotatedFilter);
                //return delta.TransposeAndMultiply(_weight);
            }
        }

        readonly INeuralNetworkLayerTrainer _trainer;
        readonly ConvolutionDescriptor _descriptor;

        public ConvolutionalLayer(INeuralNetworkFactory factory, ConvolutionDescriptor descriptor, int inputSize)
        {
            _descriptor = descriptor;
            var activation = factory.GetActivation(descriptor.Activation);
            var layer = new Layer(factory.LinearAlgebraProvider, inputSize, descriptor.FilterDepth, activation, descriptor);
            _trainer = factory.CreateTrainer(layer, descriptor);
        }

        public IMatrix Execute(IMatrix matrix, Stack<IConvolutionalLayerBackpropagation> backpropagation)
        {
            if (backpropagation == null)
                return _trainer.FeedForward(matrix, false);
            else {
                var layer = _trainer.LayerUpdater.Layer;
                var output = layer.Execute(matrix);
                var ret = layer?.Activation.Calculate(output) ?? output;
                backpropagation.Push(new Backpropagation(_trainer, matrix, output));
                return ret;
            }
        }

        public int OutputSize
        {
            get
            {
                return _descriptor.LocationCount * _descriptor.LocationCount * _descriptor.FilterDepth;
            }
        }

        public void Dispose()
        {
            _trainer.Dispose();
        }
    }
}
