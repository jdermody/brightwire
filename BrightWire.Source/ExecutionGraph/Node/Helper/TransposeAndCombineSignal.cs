using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Transpose the input and combine each depth slice - used when translating between tensor and matrix based signals
    /// </summary>
    class TransposeAndCombineSignal : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<TransposeAndCombineSignal>
        {
            readonly I4DTensor _tensor;

            public Backpropagation(TransposeAndCombineSignal source, I4DTensor tensor) : base(source)
            {
                _tensor = tensor;
            }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var matrix = errorSignal.GetMatrix();
                var lap = context.LinearAlgebraProvider;

                var rowList = new List<IVector>();
                for(var i = 0; i < matrix.RowCount; i++) {
                    var rowMatrix = matrix.Row(i).ConvertInPlaceToMatrix(_tensor.RowCount, _tensor.ColumnCount);
                    var matrixList = Enumerable.Repeat(rowMatrix, _tensor.Depth).ToList();
                    var tensor = lap.Create3DTensor(matrixList);
                    rowList.Add(tensor.ConvertToVector());
                }
                var errorMatrix = lap.CreateMatrix(rowList);

                return errorSignal.ReplaceWith(errorMatrix.Transpose());
            }
        }

        public TransposeAndCombineSignal(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var tensor = context.Data.Get4DTensor();
            var rowList = new List<IVector>();
            for(var i = 0; i < tensor.Count; i++) {
                var row = tensor.GetTensorAt(i).CombineDepthSlices().ConvertInPlaceToVector();
                rowList.Add(row);
            }
            var output = context.LinearAlgebraProvider.CreateMatrix(rowList);

            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, tensor));
        }
    }
}
