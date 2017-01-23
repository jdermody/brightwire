using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Connectionist.Execution.Layer
{
    internal class Convolutional : IConvolutionalLayerExecution
    {
        readonly ILinearAlgebraProvider _lap;
        readonly int _padding, _filterWidth, _filterHeight, _stride;
        readonly StandardFeedForward _layer;

        public Convolutional(ILinearAlgebraProvider lap, ConvolutionalNetwork.Layer layer)
        {
            _lap = lap;
            _padding = layer.Padding;
            _filterWidth = layer.FilterWidth;
            _filterHeight = layer.FilterHeight;
            _stride = layer.Stride;

            var activation = lap.NN.GetActivation(layer.Data.Activation);
            _layer = new StandardFeedForward(lap.CreateMatrix(layer.Data.Weight), lap.CreateVector(layer.Data.Bias), activation);
        }

        public IMatrix ExecuteToMatrix(I3DTensor tensor)
        {
            IMatrix input;
            if (_padding > 0) {
                using (var padded = tensor.AddPadding(_padding))
                    input = padded.Im2Col(_filterWidth, _filterHeight, _stride);
            }
            else
                input = tensor.Im2Col(_filterWidth, _filterHeight, _stride);

            // execute the layer
            var ret = _layer.Activate(input);
            input.Dispose();
            return ret;
        }

        public IVector ExecuteToVector(I3DTensor tensor)
        {
            var ret = ExecuteToMatrix(tensor);
            return ret.ConvertInPlaceToVector();
        }

        public I3DTensor ExecuteToTensor(I3DTensor tensor)
        {
            using (var output = ExecuteToMatrix(tensor)) {
                // convert the matrix to a tensor
                var sliceList = new List<IMatrix>();
                for (int i = 0, len = output.ColumnCount; i < len; i++) {
                    using (var vector = output.Column(i)) {
                        var parts = vector.Split(tensor.ColumnCount);
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
        }

        public void Dispose()
        {
            _layer.Dispose();
        }
    }
}
