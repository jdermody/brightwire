using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
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
            readonly int _inputWidth, _inputHeight, _newWidth, _newHeight;

            public Backpropagation(Convolutional source, I3DTensor im2Col, int inputWidth, int inputHeight, int newWidth, int newHeight)
                : base(source)
            {
                _im2Col = im2Col;
                _newWidth = newWidth;
                _newHeight = newHeight;
                _inputWidth = inputWidth;
                _inputHeight = inputHeight;
            }

            void _UpdateBias(IVector delta, ILearningContext context)
            {
                _source._bias.AddInPlace(delta, 1f, context.LearningRate);
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var lap = context.LinearAlgebraProvider;
                var tensor = errorSignal.GetMatrix().ConvertTo4DTensor(_newHeight, _newWidth, _source._filter.ColumnCount);
                var padding = _source._padding;

                var weightUpdate = lap.CreateZeroMatrix(_source._filter.RowCount, _source._filter.ColumnCount);
                var multiplyWithAll = lap.CreateZeroMatrix(_newWidth * _newHeight, _source._filter.ColumnCount);
                for (var i = 0; i < tensor.Count; i++) {
                    var multiplyWith = tensor.GetTensorAt(i).ConvertToMatrix();
                    var im2Col = _im2Col.GetMatrixAt(i);
                    weightUpdate.AddInPlace(im2Col.TransposeThisAndMultiply(multiplyWith));
                    multiplyWithAll.AddInPlace(multiplyWith);
                }

                // calculate the weight and delta updates
                var biasUpdate = multiplyWithAll.ColumnSums();
                biasUpdate.Multiply(1f / multiplyWithAll.RowCount);
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
                        filterList.Add(filters.Column(i).Split(inputDepth).Select(v => v.Rotate(v.Count / filterWidth)).ToList());

                    using(var reverseIm2Col = tensor.ReverseIm2Col(filterList, _inputHeight, _inputWidth, inputDepth, padding, filterWidth, filterHeight, stride)) {
                        var delta = reverseIm2Col.ConvertToMatrix().ConvertTo4DTensor(_inputHeight + padding * 2, _inputWidth + padding * 2, inputDepth);
                        if (padding > 0)
                            delta = delta.RemovePadding(padding);
                        return new Tensor4DGraphData(delta.ConvertToMatrix(), _inputHeight, _inputWidth, inputDepth);
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
            var lap = context.LinearAlgebraProvider;
            var tensor = input.GetMatrix().ConvertTo4DTensor(input.Rows, input.Columns, input.Depth);

            var inputWidth = tensor.ColumnCount;
            var inputHeight = tensor.RowCount;
            var newWidth = ((inputWidth - _filterWidth + (2 * _padding)) / _stride) + 1;
            var newHeight = ((inputHeight - _filterHeight + (2 * _padding)) / _stride) + 1;

            I3DTensor im2Col = null;
            //if (context.Data.RowId.HasValue)
            //    im2Col = context.ExecutionContext.GetInputTransfomation(context.Data.RowId.Value);
            if (im2Col == null) {
                I4DTensor tensor2;
                if (_padding > 0)
                    tensor2 = tensor.AddPadding(_padding);
                else
                    tensor2 = tensor;

                im2Col = tensor2.Im2Col(_filterWidth, _filterHeight, _stride);
                //if (context.Data.RowId.HasValue)
                //    context.ExecutionContext.SetInputTransformation(context.Data.RowId.Value, im2Col);
            }
            var outputSignal = im2Col.Multiply(_filter);
            outputSignal.AddToEachRow(_bias);

            var tensorList = new List<I3DTensor>();
            for (var i = 0; i < outputSignal.Depth; i++) {
                var matrixList2 = new List<IMatrix>();
                var slice = outputSignal.GetMatrixAt(i);
                tensorList.Add(slice.ConvertTo3DTensor(newHeight, newWidth));
            }
            var outputTensor = lap.Create4DTensor(tensorList);

            var graphData = new Tensor4DGraphData(outputTensor);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(this, im2Col, inputWidth, inputHeight, newWidth, newHeight));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("CON", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var lap = factory.LinearAlgebraProvider;

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
                _updater = factory.CreateWeightUpdater(_filter);
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
