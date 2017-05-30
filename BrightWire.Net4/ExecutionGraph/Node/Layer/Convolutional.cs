using BrightWire.ExecutionGraph.Helper;
using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
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

            //IGraphData _CalculatePreviousError(I3DTensor errorSignal, IContext context)
            //{
            //    var filters = _source._filter.AsIndexable().Columns.ToList();
            //    var lap = context.LinearAlgebraProvider;
            //    var columns = _inputHeight + _source._padding * 2;
            //    var rows = _inputWidth + _source._padding * 2;
            //    var stride = _source._stride;
            //    var matrixList = Enumerable.Range(0, _source._inputDepth)
            //        .Select(i => lap.CreateMatrix(rows, columns, 0f).AsIndexable())
            //        .ToList()
            //    ;
            //    var convolutions = ConvolutionHelper.Default(columns, rows, _source._filterHeight, _source._filterHeight, stride);
                
            //    for (var k = 0; k < errorSignal.Depth; k++) {
            //        var slice = errorSignal.GetDepthSlice(k).AsIndexable();
            //        var filterList = filters[k]
            //            .Split(_source._inputDepth)
            //            .Select(f => f.Rotate(_source._filterWidth).AsIndexable())
            //            .ToList()
            //        ;

            //        foreach (var convolution in convolutions) {
            //            var first = convolution.First();
            //            var error = slice[first.X / stride, first.Y / stride];
            //            foreach (var item in convolution) {
            //                var i = item.X - first.X;
            //                var j = item.Y - first.Y;
            //                for (var z = 0; z < filterList.Count; z++) {
            //                    var filter = filterList[z];
            //                    var output = matrixList[z];
            //                    output[item.Y, item.X] += filter[i * _source._filterHeight + j] * error;
            //                }
            //            }
            //        }
            //    }

            //    if(_source._padding > 0)
            //        return lap.CreateTensor(matrixList.Select(m => m.RemovePadding(_source._padding)).ToList()).ToGraphData();
            //    else
            //        return lap.CreateTensor(matrixList).ToGraphData();
            //}

            //void _CalculateWeightUpdate(I3DTensor errorSignal, IContext context)
            //{
            //    //var lap = context.LinearAlgebraProvider;
            //    //var multiplyWith = lap.CreateMatrix(errorSignal.RowCount * errorSignal.ColumnCount, errorSignal.Depth, 0f).AsIndexable();
            //    //var biasUpdate = new float[errorSignal.Depth];

            //    //for(var k = 0; k < errorSignal.Depth; k++) {
            //    //    var total = 0f;
            //    //    int count = 0;
            //    //    var slice = errorSignal.GetDepthSlice(k).AsIndexable();
            //    //    for (var x = 0; x < slice.ColumnCount; x++) {
            //    //        for (var y = 0; y < slice.RowCount; y++) {
            //    //            var value = slice[x, y];
            //    //            multiplyWith[x * slice.RowCount + y, k] = value;
            //    //            total += value;
            //    //            ++count;
            //    //        }
            //    //    }
            //    //    biasUpdate[k] = total / count;
            //    //}
            //    //var delta = _im2Col.TransposeThisAndMultiply(lap.CreateMatrix(multiplyWith));
            //    //var biasUpdateVector = lap.CreateVector(biasUpdate);
            //    (var delta, var biasUpdate) = errorSignal.CalculateWeightUpdate(_im2Col);

            //    context.LearningContext.Store(delta, err => _source.Update(err, context.LearningContext));
            //    context.LearningContext.Store(biasUpdate, bu => _UpdateBias(bu, context.LearningContext));
            //}

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var lap = context.LinearAlgebraProvider;
                var tensor = lap.CreateTensor(errorSignal.GetMatrix(), _newHeight, _newWidth, _source._filter.ColumnCount);
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
                //var multiplyWith = tensor.ConvertToMatrix();
                //var im2Col = _im2Col.ConvertToMatrix();

                //var weightUpdate = im2Col.TransposeAndMultiply(multiplyWith);
                var biasUpdate = multiplyWithAll.ColumnSums();
                biasUpdate.Multiply(1f / multiplyWithAll.RowCount);
                context.LearningContext.Store(weightUpdate, err => _source.Update(err, context.LearningContext));
                context.LearningContext.Store(biasUpdate, bu => _UpdateBias(bu, context.LearningContext));

                if (_source._shouldBackpropagate) {
                    var filters = _source._filter;
                    var inputDepth = _source._inputDepth;
                    var filterWidth = _source._filterWidth;
                    var filterHeight = _source._filterHeight;
                    var stride = _source._stride;
                    var filterList = new List<IReadOnlyList<IVector>>();
                    for (var i = 0; i < filters.ColumnCount; i++)
                        filterList.Add(filters.Column(i).Split(inputDepth).Select(v => v.Rotate(v.Count / filterWidth)).ToList());

                    using(var reverseIm2Col = tensor.ReverseIm2Col(filterList, _inputHeight, _inputWidth, inputDepth, padding, filterHeight, filterWidth, stride)) {
                        var delta = lap.CreateTensor(reverseIm2Col.ConvertToMatrix(), _inputHeight + padding * 2, _inputWidth + padding * 2, inputDepth);
                        //var delta = context.LinearAlgebraProvider.CreateTensor(reverseIm2Col, _inputHeight + padding * 2, _inputWidth + padding * 2);
                        if (padding > 0)
                            delta = delta.RemovePadding(padding);
                        return new Tensor4DGraphData(delta.ConvertToMatrix(), _inputHeight, _inputWidth, inputDepth);
                    }
                }
                return errorSignal;

                //return tensor.CalculatePreviousError(
                //    _source._filter,
                //    _inputHeight,
                //    _inputWidth,
                //    _source._inputDepth,
                //    _source._padding,
                //    _source._filterHeight,
                //    _source._filterWidth,
                //    _source._stride
                //).ToGraphData();
                //return errorSignal;

                //_CalculateWeightUpdate(tensor, context);
                //return _CalculatePreviousError(tensor, context);
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
            var tensor = lap.CreateTensor(input.GetMatrix(), input.Rows, input.Columns, input.Depth);

            var inputWidth = tensor.ColumnCount;
            var inputHeight = tensor.RowCount;
            var newWidth = ((inputWidth - _filterWidth + (2 * _padding)) / _stride) + 1;
            var newHeight = ((inputHeight - _filterHeight + (2 * _padding)) / _stride) + 1;

            I3DTensor im2Col = null;
            if (context.Data.RowId.HasValue)
                im2Col = context.ExecutionContext.GetInputTransfomation(context.Data.RowId.Value);
            if (im2Col == null) {
                I4DTensor tensor2;
                if (_padding > 0)
                    tensor2 = tensor.AddPadding(_padding);
                else
                    tensor2 = tensor;

                im2Col = tensor2.Im2Col(_filterWidth, _filterHeight, _stride);
                if (context.Data.RowId.HasValue)
                    context.ExecutionContext.SetInputTransformation(context.Data.RowId.Value, im2Col);
            }
            var outputSignal = im2Col.Multiply(_filter);
            outputSignal.AddToEachRow(_bias);

            var tensorList = new List<I3DTensor>();
            for (var i = 0; i < outputSignal.Depth; i++) {
                var matrixList2 = new List<IMatrix>();
                var slice = outputSignal.GetMatrixAt(i);
                tensorList.Add(lap.CreateTensor(slice, newHeight, newWidth));
            }
            var outputTensor = lap.CreateTensor(tensorList);

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
                //var savedUpdater = factory.CreateGradientDescentOptimisation(reader);
                _updater = factory.GetWeightUpdater(_filter);
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
