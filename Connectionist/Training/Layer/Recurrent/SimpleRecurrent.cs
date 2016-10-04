using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace BrightWire.Connectionist.Training.Layer.Recurrent
{
    /// <summary>
    /// Simple recurrent layers can be used for elman style recurrent networks
    /// </summary>
    public class SimpleRecurrent : RecurrentLayerBase, INeuralNetworkRecurrentLayer
    {
        readonly IActivationFunction _activation;
        readonly INeuralNetworkLayerUpdater _input, _memory;

        class Backpropagation : INeuralNetworkRecurrentBackpropagation
        {
            readonly IActivationFunction _activation;
            readonly INeuralNetworkLayerUpdater _inputUpdater, _memoryUpdater;
            readonly IMatrix _input, _memory, _output;

            public Backpropagation(IActivationFunction activation, INeuralNetworkLayerUpdater inputUpdater, INeuralNetworkLayerUpdater memoryUpdater, IMatrix input, IMatrix memory, IMatrix output)
            {
                _activation = activation;
                _inputUpdater = inputUpdater;
                _memoryUpdater = memoryUpdater;
                _input = input;
                _memory = memory;
                _output = output;
            }

            public IMatrix Execute(IMatrix curr, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updateAccumulator)
            {
                using (var ad = _activation.Derivative(_output, curr)) {
                    // clip the gradient
                    ad.Constrain(-1f, 1f);

                    var delta = curr.PointwiseMultiply(ad);
                    var prevDelta = delta.TransposeAndMultiply(_memoryUpdater.Layer.Weight);
                    var memoryUpdate = _memory.TransposeThisAndMultiply(delta);
                    var inputUpdate = _input.TransposeThisAndMultiply(delta);
                    
                    //_input.Dispose();
                    _memory.Dispose();
                    _output.Dispose();
                    updateAccumulator.Record(_memoryUpdater, delta, memoryUpdate);
                    updateAccumulator.Record(_inputUpdater, delta.Clone(), inputUpdate);

                    return prevDelta;
                }
            }
        }

        public SimpleRecurrent(int inputSize, int hiddenSize, INeuralNetworkFactory factory, INeuralNetworkLayerDescriptor template)
        {
            _activation = factory.GetActivation(template.Activation);
            _input = CreateLayer(inputSize, hiddenSize, factory, template);
            _memory = CreateLayer(hiddenSize, hiddenSize, factory, template);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                _input.Dispose();
                _memory.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsRecurrent { get { return true; } }

        public RecurrentLayer LayerInfo
        {
            get
            {
                return new RecurrentLayer {
                    Activation = _activation.Type,
                    Type = RecurrentLayerType.SimpleRecurrent,
                    Layer = new[] {
                        _input.Layer.LayerInfo,
                        _memory.Layer.LayerInfo
                    }
                };
            }

            set
            {
                _input.Layer.LayerInfo = value.Layer[0];
                _memory.Layer.LayerInfo = value.Layer[1];
            }
        }

        public INeuralNetworkRecurrentBackpropagation Execute(List<IMatrix> curr, bool backpropagate)
        {
            Debug.Assert(curr.Count == 2);
            var input = curr[0];
            var memory = curr[1];
            var output = _Combine(input, memory, m => _activation.Calculate(m));
            curr[0] = output;
            curr[1] = output.Clone();

            if (backpropagate)
                return new Backpropagation(_activation, _input, _memory, input, memory, output);
            //input.Dispose();
            //memory.Dispose();
            //output.Dispose();
            return null;
        }

        IMatrix _Combine(IMatrix input, IMatrix memory, Func<IMatrix, IMatrix> activation)
        {
            return Combine(input, memory, _input.Layer, _memory.Layer, activation);
        }
    }
}
