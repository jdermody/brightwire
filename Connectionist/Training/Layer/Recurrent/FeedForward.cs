using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace BrightWire.Connectionist.Training.Layer.Recurrent
{
    internal class FeedForward : INeuralNetworkRecurrentLayer
    {
        readonly INeuralNetworkLayerTrainer _trainer;

        class Backpropagation : INeuralNetworkRecurrentBackpropagation
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
                //using (var input = _input)
                using (var output = _output)
                    return _trainer.Backpropagate(_input, output, error, context, calculateOutput, updateAccumulator);
            }
        }

        public FeedForward(INeuralNetworkLayerTrainer trainer)
        {
            _trainer = trainer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _trainer.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsRecurrent { get { return false; } }

        public RecurrentLayer LayerInfo
        {
            get
            {
                return new RecurrentLayer {
                    Type = RecurrentLayerType.FeedForward,
                    Layer = new[] {
                        _trainer.LayerUpdater.Layer.LayerInfo
                    }
                };
            }

            set
            {
                _trainer.LayerUpdater.Layer.LayerInfo = value.Layer[0];
            }
        }

        public INeuralNetworkRecurrentBackpropagation Execute(List<IMatrix> curr, bool backpropagate)
        {
            var input = curr[0];
            if (backpropagate) {
                var layer = _trainer.LayerUpdater.Layer;
                var layerOutput = input.Multiply(layer.Weight);
                layerOutput.AddToEachRow(layer.Bias);
                curr[0] = layer?.Activation.Calculate(layerOutput) ?? layerOutput.Clone();

                return new Backpropagation(_trainer, input, layerOutput);
            }

            var output = _trainer.FeedForward(input, false);
            curr[0].Dispose();
            curr[0] = output;
            return null;
        }
    }
}
