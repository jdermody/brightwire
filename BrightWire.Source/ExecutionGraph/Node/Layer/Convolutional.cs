using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Convolutional neural network layer
    /// https://en.wikipedia.org/wiki/Convolutional_neural_network
    /// </summary>
    class Convolutional : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<Convolutional>
        {
            readonly I3DTensor _im2Col;
            readonly int _inputWidth, _inputHeight, _inputDepth, _inputCount, _newWidth, _newHeight;

            public Backpropagation(Convolutional source, I3DTensor im2Col, int inputWidth, int inputHeight, int inputDepth, int inputCount, int newWidth, int newHeight)
                : base(source)
            {
                _im2Col = im2Col;
                _newWidth = newWidth;
                _newHeight = newHeight;
                _inputWidth = inputWidth;
                _inputHeight = inputHeight;
	            _inputDepth = inputDepth;
	            _inputCount = inputCount;
            }

            void _UpdateBias(IVector delta, ILearningContext context)
            {
                _source._bias.AddInPlace(delta, 1f, context.BatchLearningRate);
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var tensor = errorSignal.GetMatrix().As4DTensor(_newHeight, _newWidth, _source._filter.ColumnCount);
                var padding = _source._padding;

                // calculate the weight and bias updates
                var weightUpdate = _im2Col.TransposeThisAndMultiply(tensor).CombineDepthSlices();
                var biasUpdate = tensor.ColumnSums();
                context.LearningContext.StoreUpdate(_source, weightUpdate, err => _source.Update(err, context.LearningContext));
                context.LearningContext.StoreUpdate(_source, biasUpdate, bu => _UpdateBias(bu, context.LearningContext));

                if (_source._shouldBackpropagate) {
                    var filters = _source._filter;
                    var inputDepth = _source._inputDepth;
                    var filterWidth = _source._filterWidth;
                    var filterHeight = _source._filterHeight;
                    var stride = _source._stride;
                    var filterList = new List<IReadOnlyList<IVector>>();
                    for (var i = 0; i < filters.ColumnCount; i++)
                        filterList.Add(filters.Column(i).Split(inputDepth).Select(v => v.Rotate()).ToList());

	                var outputRows = _inputHeight + padding * 2;
	                var outputColumns = _inputWidth + padding * 2;
                    using(var reverseIm2Col = tensor.ReverseIm2Col(filterList, outputRows, outputColumns, filterWidth, filterHeight, stride)) {
                        var delta = reverseIm2Col;
                        if (padding > 0)
                            delta = delta.RemovePadding(padding);
                        return new Tensor4DGraphData(delta.AsMatrix(), _inputHeight, _inputWidth, inputDepth);
                    }
                }
                return errorSignal;
            }
        }
        IGradientDescentOptimisation _updater;
        int _padding, _filterWidth, _filterHeight, _stride, _inputDepth;
        IMatrix _filter;
        IVector _bias;
        bool _shouldBackpropagate;

        public Convolutional(
            bool shouldBackpropagate,
            IWeightInitialisation weightInitialisation,
            Func<IMatrix, IGradientDescentOptimisation> updater,
            int inputDepth,
            int filterCount,
            int padding, 
            int filterWidth, 
            int filterHeight, 
            int stride, 
            string name = null) : base(name)
        {
            _shouldBackpropagate = shouldBackpropagate;
            _padding = padding;
            _filterWidth = filterWidth;
            _filterHeight = filterHeight;
            _stride = stride;
            _inputDepth = inputDepth;

            _bias = weightInitialisation.CreateBias(filterCount);
            _filter = weightInitialisation.CreateWeight(_filterWidth * _filterHeight * _inputDepth, filterCount);
            _updater = updater(_filter);
        }

	    public int FilterCount => _filter.ColumnCount;

        protected override void _Dispose(bool isDisposing)
        {
            _filter.Dispose();
            _bias.Dispose();
        }

        public void Update(IMatrix delta, ILearningContext context)
        {
            _updater.Update(_filter, delta, context);
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data;
            var tensor = input.GetMatrix().As4DTensor(input.Rows, input.Columns, input.Depth);

            var inputWidth = tensor.ColumnCount;
            var inputHeight = tensor.RowCount;
            var newWidth = ((inputWidth - _filterWidth + (2 * _padding)) / _stride) + 1;
            var newHeight = ((inputHeight - _filterHeight + (2 * _padding)) / _stride) + 1;

            if (_padding > 0)
                tensor = tensor.AddPadding(_padding);

            var im2Col = tensor.Im2Col(_filterWidth, _filterHeight, _stride);
            var outputSignal = im2Col.Multiply(_filter);
            outputSignal.AddToEachRow(_bias);
            var outputTensor = outputSignal.As4DTensor(newHeight, newWidth);
			Debug.Assert(outputTensor.Depth == FilterCount && outputTensor.Count == tensor.Count);

            var graphData = new Tensor4DGraphData(outputTensor);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(this, im2Col, inputWidth, inputHeight, tensor.Depth, tensor.Count, newWidth, newHeight));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("CON", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var lap = factory?.LinearAlgebraProvider;

            _padding = reader.ReadInt32();
            _filterWidth = reader.ReadInt32();
            _filterHeight = reader.ReadInt32();
            _stride = reader.ReadInt32();
            _inputDepth = reader.ReadInt32();
            _shouldBackpropagate = reader.ReadBoolean();

            // read the bias parameters
            var bias = FloatVector.ReadFrom(reader);
            if (_bias == null)
                _bias = lap.CreateVector(bias);
            else
                _bias.Data = bias;

            // read the weight parameters
            var weight = FloatMatrix.ReadFrom(reader);
            if (_filter == null)
                _filter = lap.CreateMatrix(weight);
            else
                _filter.Data = weight;

            // read the updater
            if (_updater == null) {
                _updater = factory?.CreateWeightUpdater(_filter);
            }
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_padding);
            writer.Write(_filterWidth);
            writer.Write(_filterHeight);
            writer.Write(_stride);
            writer.Write(_inputDepth);
            writer.Write(_shouldBackpropagate);

            _bias.Data.WriteTo(writer);
            _filter.Data.WriteTo(writer);
        }
    }
}
