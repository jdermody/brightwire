using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    class Convolutional : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            readonly Convolutional _layer;
            readonly IMatrix _im2Col;
            readonly IIndexable3DTensor _input;
            readonly int _inputWidth, _inputHeight, _newWidth, _newHeight;

            public Backpropagation(Convolutional layer, IMatrix im2Col, IIndexable3DTensor input, int inputWidth, int inputHeight, int newWidth, int newHeight)
            {
                _layer = layer;
                _input = input;
                _im2Col = im2Col;
                _newWidth = newWidth;
                _newHeight = newHeight;
                _inputWidth = inputWidth;
                _inputHeight = inputHeight;
            }

            protected override void _Dispose(bool isDisposing)
            {
                _input.Dispose();
            }

            void _UpdateBias(IVector delta, ILearningContext context)
            {
                _layer._bias.AddInPlace(delta, 1f, context.LearningRate);
            }

            IGraphData _CalculatePreviousError(IIndexable3DTensor errorSignal, IContext context)
            {
                var filters = _layer._filter.AsIndexable().Columns.ToList();
                var lap = context.LinearAlgebraProvider;
                var columns = _inputHeight + _layer._padding * 2;
                var rows = _inputWidth + _layer._padding * 2;
                var matrixList = Enumerable.Range(0, _layer._inputDepth)
                    .Select(i => lap.Create(rows, columns, 0f).AsIndexable())
                    .ToList()
                ;
                
                for (var k = 0; k < errorSignal.Depth; k++) {
                    var slice = errorSignal.GetDepthSlice(k).AsIndexable();
                    var filterList = filters[k].Split(_layer._inputDepth).Select(f => f.Rotate(_layer._filterWidth).AsIndexable()).ToList();

                    int y = 0, x = 0, sliceX = 0, sliceY = 0;
                    while (x <= columns - _layer._filterWidth) {
                        var error = slice[sliceX, sliceY];
                        for (var i = 0; i < _layer._filterWidth; i++) {
                            for (var j = 0; j < _layer._filterHeight; j++) {
                                for (var z = 0; z < filterList.Count; z++) {
                                    var filter = filterList[z];
                                    var output = matrixList[z];
                                    output[x + i, y + j] += filter[i * _layer._filterHeight + j] * error;
                                }
                            }
                        }

                        // move the window
                        y += _layer._stride;
                        sliceY++;
                        if (y > rows - _layer._filterHeight) {
                            y = 0;
                            sliceY = 0;
                            x += _layer._stride;
                            sliceX++;
                        }
                    }
                }
                if(_layer._padding > 0)
                    return lap.CreateTensor(matrixList.Select(m => m.RemovePadding(_layer._padding)).ToList()).ToGraphData();
                else
                    return lap.CreateTensor(matrixList).ToGraphData();
            }

                //IGraphData _CalculatePreviousError(IIndexable3DTensor errorSignal, IContext context)
                //{
                //    if (_layer._padding > 0)
                //        errorSignal = errorSignal.AddPadding(_layer._padding).AsIndexable();

                //    var rows = errorSignal.RowCount;
                //    var columns = errorSignal.ColumnCount;
                //    var lap = context.LinearAlgebraProvider;

                //    var outputList = new List<List<float>>();
                //    var filters = _layer._filter.AsIndexable().Columns.ToList();

                //    IVector temp;
                //    var output = new Dictionary<int, IVector>();
                //    for (var k = 0; k < errorSignal.Depth; k++) {
                //        var matrix = errorSignal.GetDepthSlice(k).AsIndexable();
                //        var filter = filters[k];
                //        var parts = filter.Split(_layer._inputDepth);

                //        int index = 0;
                //        var im2Col = matrix.Im2Col(_layer._filterWidth, _layer._filterHeight, _layer._stride);
                //        foreach(var part in parts) {
                //            var result = im2Col.Multiply(part.Rotate(_layer._filterWidth));
                //            var vector = result.ConvertInPlaceToVector();
                //            if (!output.TryGetValue(index, out temp))
                //                output.Add(index, vector);
                //            else
                //                temp.AddInPlace(vector);
                //            ++index;
                //        }

                //        //int xOffset = 0, yOffset = 0;
                //        //var rowList = new List<List<float>>();

                //        //while (yOffset <= rows - _layer._filterHeight) {
                //        //    var row = new List<float>();
                //        //    for (var j = 0; j < _layer._filterHeight; j++) {
                //        //        for (var i = 0; i < _layer._filterWidth; i++) {
                //        //            row.Add(matrix[yOffset + j, xOffset + i]);
                //        //        }
                //        //    }
                //        //    rowList.Add(row);

                //        //    // move the window
                //        //    xOffset += _layer._stride;
                //        //    if (xOffset > columns - _layer._filterWidth) {
                //        //        xOffset = 0;
                //        //        yOffset += _layer._stride;
                //        //    }
                //        //}
                //        //var vectorList = rowList.Select(r => lap.Create(r));

                //        //var flippedFilter = lap.Create(filter.Values.Reverse());
                //        //var output = new List<float>();
                //        //foreach (var vector in vectorList) {
                //        //    output.Add(flippedFilter.DotProduct(vector));
                //        //}
                //        //outputList.Add(output);
                //    }
                //    var matrixList = new List<IMatrix>();
                //    foreach(var item in output) {
                //        item.Value.Multiply(1f / errorSignal.Depth);
                //        var matrix = item.Value.ConvertInPlaceToMatrix(_inputWidth, _inputHeight);
                //        matrixList.Add(matrix);
                //    }
                //    return lap.CreateTensor(matrixList).ToGraphData();
                //    //var ret = lap.CreateMatrix(Enumerable.Range(0, outputList.First().Count).Select(i => outputList.Average(o => o[i])).ToList(), _inputWidth);
                //    //return ret;
                //}

            void _CalculateWeightUpdate(IIndexable3DTensor errorSignal, IContext context)
            {
                var lap = context.LinearAlgebraProvider;

                //if (_layer._padding > 0)
                //    tensor2 = _input.AddPadding(_layer._padding).AsIndexable();
                //else
                //    tensor2 = _input.AsIndexable();

                //var matrix = tensor2.Im2Col(_layer._filterWidth, _layer._filterHeight, _layer._stride);

                //int xOffset = 0, yOffset = 0;
                //var rowList = new List<List<float>>();
                //var rows = tensor2.RowCount;
                //var columns = tensor2.ColumnCount;
                //var depth = tensor2.Depth;

                //while (yOffset <= rows - _layer._filterHeight) {
                //    var row = new List<float>();
                //    for (var k = 0; k < depth; k++) {
                //        for (var j = 0; j < _layer._filterHeight; j++) {
                //            for (var i = 0; i < _layer._filterWidth; i++) {
                //                row.Add(tensor2[yOffset + j, xOffset + i, k]);
                //            }
                //        }
                //    }
                //    rowList.Add(row);

                //    // move the window
                //    xOffset += _layer._stride;
                //    if (xOffset > columns - _layer._filterWidth) {
                //        xOffset = 0;
                //        yOffset += _layer._stride;
                //    }
                //}
                //var vectorList = rowList.Select(r => lap.Create(r).AsIndexable()).ToList();
                //var matrix = lap.Create(vectorList);

                var multiplyWith = lap.Create(errorSignal.RowCount * errorSignal.ColumnCount, errorSignal.Depth, 0f).AsIndexable();
                var biasUpdate = new float[errorSignal.Depth];
                for(var k = 0; k < errorSignal.Depth; k++) {
                    var total = 0f;
                    int count = 0;
                    var slice = errorSignal.GetDepthSlice(k).AsIndexable();
                    for (var x = 0; x < slice.ColumnCount; x++) {
                        for (var y = 0; y < slice.RowCount; y++) {
                            var value = slice[x, y];
                            multiplyWith[x * slice.RowCount + y, k] = value;
                            total += value;
                            ++count;
                        }
                    }
                    biasUpdate[k] = total / count;
                }
                var delta = _im2Col.TransposeThisAndMultiply(multiplyWith);
                var biasUpdateVector = lap.Create(biasUpdate);

                context.LearningContext.Store(delta, err => _layer.Update(err, context.LearningContext));
                context.LearningContext.Store(biasUpdateVector, bu => _UpdateBias(bu, context.LearningContext));
            }

            protected override IGraphData _Backward(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var tensor = errorSignal.GetTensor().AsIndexable();

                //var previousError = _CalculatePreviousError(tensor, context);
                _CalculateWeightUpdate(tensor, context);
                return null;

                //// work out the next error signal
                //IMatrix ret = errorSignal.TransposeAndMultiply(_layer._filter);

                //// calculate the update to the weights
                //var weightUpdate = _input.TransposeThisAndMultiply(errorSignal);

                //// store the updates
                //var learningContext = context.LearningContext;
                ////learningContext.Store(errorSignal, err => _UpdateBias(err, learningContext));
                //learningContext.Store(weightUpdate, err => _layer.Update(err, learningContext));

                //return ret;

                //return previousError;
            }
        }
        readonly IGradientDescentOptimisation _updater;
        readonly int _padding, _filterWidth, _filterHeight, _stride, _inputDepth;
        readonly IMatrix _filter;
        readonly IVector _bias;

        public Convolutional(
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
            _padding = padding;
            _filterWidth = filterWidth;
            _filterHeight = filterHeight;
            _stride = stride;
            _inputDepth = inputDepth;

            _bias = weightInitialisation.CreateBias(filterCount);
            _filter = weightInitialisation.CreateWeight(_filterWidth * _filterHeight * _inputDepth, filterCount);
            _updater = updater(_filter);
        }

        public void Update(IMatrix delta, ILearningContext context)
        {
            _updater.Update(_filter, delta, context);
        }

        public override void ExecuteForward(IContext context)
        {
            var tensor = context.Data.GetTensor();
            var lap = context.LinearAlgebraProvider;

            var inputWidth = tensor.ColumnCount;
            var inputHeight = tensor.RowCount;
            var newWidth = ((inputWidth - _filterWidth + (2 * _padding)) / _stride) + 1;
            var newHeight = ((inputHeight - _filterHeight + (2 * _padding)) / _stride) + 1;

            IIndexable3DTensor tensor2;
            if (_padding > 0)
                tensor2 = tensor.AddPadding(_padding).AsIndexable();
            else
                tensor2 = tensor.AsIndexable();

            //int xOffset = 0, yOffset = 0;
            //var rowList = new List<List<float>>();
            //var rows = tensor2.RowCount;
            //var columns = tensor2.ColumnCount;
            //var depth = tensor2.Depth;

            //while (yOffset <= rows - _filterHeight) {
            //    var row = new List<float>();
            //    for (var k = 0; k < depth; k++) {
            //        for (var j = 0; j < _filterHeight; j++) {
            //            for (var i = 0; i < _filterWidth; i++) {
            //                row.Add(tensor2[yOffset + j, xOffset + i, k]);
            //            }
            //        }
            //    }
            //    rowList.Add(row);

            //    // move the window
            //    xOffset += _stride;
            //    if (xOffset > columns - _filterWidth) {
            //        xOffset = 0;
            //        yOffset += _stride;
            //    }
            //}
            //var vectorList = rowList.Select(r => lap.Create(r));

            //var filters = _filter.AsIndexable().Columns.ToList();
            //var matrixList = new List<IMatrix>();
            //foreach (var filter in filters) {
            //    var output = new List<float>();
            //    foreach (var vector in vectorList) {
            //        output.Add(filter.DotProduct(vector) + _bias);
            //    }
            //    var outputMatrix = lap.CreateMatrix(output, newWidth);
            //    matrixList.Add(outputMatrix);
            //}
            //var outputTensor = lap.CreateTensor(matrixList);

            var matrix = tensor2.Im2Col(_filterWidth, _filterHeight, _stride);
            var outputSignal = matrix.Multiply(_filter);
            outputSignal.AddToEachRow(_bias);

            var matrixList2 = new List<IMatrix>();
            for (var i = 0; i < outputSignal.ColumnCount; i++)
                matrixList2.Add(outputSignal.Column(i).ConvertInPlaceToMatrix(newWidth, newHeight));
            var outputTensor = lap.CreateTensor(matrixList2);

            var graphData = new TensorGraphData(outputTensor);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(this, matrix, tensor.AsIndexable(), inputWidth, inputHeight, newWidth, newHeight));
        }
    }
}
