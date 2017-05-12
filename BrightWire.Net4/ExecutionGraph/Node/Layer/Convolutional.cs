using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
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
            readonly IIndexable3DTensor _input;
            readonly int _newWidth, _newHeight;

            public Backpropagation(Convolutional layer, IIndexable3DTensor input, int newWidth, int newHeight)
            {
                _layer = layer;
                _input = input;
                _newWidth = newWidth;
                _newHeight = newHeight;
            }

            protected override void _Dispose(bool isDisposing)
            {
                _input.Dispose();
            }

            protected override IGraphData _Backward(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                //// work out the next error signal
                //IMatrix ret = errorSignal.TransposeAndMultiply(_layer._filter);

                //// calculate the update to the weights
                //var weightUpdate = _input.TransposeThisAndMultiply(errorSignal);

                //// store the updates
                //var learningContext = context.LearningContext;
                ////learningContext.Store(errorSignal, err => _UpdateBias(err, learningContext));
                //learningContext.Store(weightUpdate, err => _layer.Update(err, learningContext));

                //return ret;

                return errorSignal;
            }
        }
        readonly IGradientDescentOptimisation _updater;
        readonly int _padding, _filterWidth, _filterHeight, _stride, _inputDepth;
        readonly IMatrix _filter;
        readonly float _bias;

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

            _bias = 0f;
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

            if (_padding > 0)
                tensor = tensor.AddPadding(_padding);

            var tensor2 = tensor.AsIndexable();
            int xOffset = 0, yOffset = 0;
            var rowList = new List<List<float>>();
            var rows = tensor2.RowCount;
            var columns = tensor2.ColumnCount;
            var depth = tensor2.Depth;

            while (yOffset <= rows - _filterHeight) {
                var row = new List<float>();
                for (var k = 0; k < depth; k++) {
                    for (var j = 0; j < _filterHeight; j++) {
                        for (var i = 0; i < _filterWidth; i++) {
                            row.Add(tensor2[yOffset + j, xOffset + i, k]);
                        }
                    }
                }
                rowList.Add(row);

                // move the window
                xOffset += _stride;
                if (xOffset > columns - _filterWidth) {
                    xOffset = 0;
                    yOffset += _stride;
                }
            }
            var vectorList = rowList.Select(r => lap.Create(r));

            var filters = _filter.AsIndexable().Columns.ToList();
            var matrixList = new List<IMatrix>();
            foreach(var filter in filters) {
                var output = new List<float>();
                foreach(var vector in vectorList) {
                    output.Add(filter.DotProduct(vector) + _bias);
                }
                var outputMatrix = lap.CreateMatrix(output, newWidth);
                matrixList.Add(outputMatrix);
            }
            var outputTensor = lap.CreateTensor(matrixList);

            var graphData = new TensorGraphData(outputTensor);
            _AddNextGraphAction(context, graphData, () => new Backpropagation(this, tensor2, newWidth, newHeight));

            //var matrix = tensor.Im2Col(_filterWidth, _filterHeight, _stride);
            //var output = matrix.Multiply(_filter);

            //var graphData = new MatrixTensorGraphData(context.LinearAlgebraProvider, output, newWidth, newHeight);
            //_AddNextGraphAction(context, graphData, () => new Backpropagation(this, matrix, newWidth, newHeight));
        }
    }
}
