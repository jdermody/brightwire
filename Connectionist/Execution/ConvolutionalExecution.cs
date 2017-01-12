using BrightWire.Connectionist.Execution.Layer;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.Connectionist.Execution
{
    internal class ConvolutionalExecution : IConvolutionalExecution
    {
        readonly List<IConvolutionalLayerExecution> _convolutional = new List<IConvolutionalLayerExecution>();
        readonly IStandardExecution _feedForward;

        public ConvolutionalExecution(ILinearAlgebraProvider lap, ConvolutionalNetwork network)
        {
            foreach (var layer in network.ConvolutionalLayer)
                _convolutional.Add(new Convolutional(lap, layer));
            _feedForward = lap.NN.CreateFeedForward(network.FeedForward);
        }

        public void Dispose()
        {
            _feedForward.Dispose();
            foreach (var layer in _convolutional)
                layer.Dispose();
        }

        public IVector Execute(I3DTensor tensor)
        {
            foreach(var layer in _convolutional.Take(_convolutional.Count-1)) {
                var next = layer.ExecuteToTensor(tensor);
                tensor.Dispose();
                tensor = next;
            }
            using (var matrix = _convolutional.Last().ExecuteToMatrix(tensor))
            using (var vector = matrix.ConvertInPlaceToVector()) {
                tensor.Dispose();
                return _feedForward.Execute(vector);
            }
        }
    }
}
