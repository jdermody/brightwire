using System;
using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Transpose the input and combine each depth slice - used when translating between tensor and matrix based signals
    /// </summary>
    internal class TransposeAndCombineSignal : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<TransposeAndCombineSignal>
        {
            readonly I4DFloatTensor _tensor;

            public Backpropagation(TransposeAndCombineSignal source, I4DFloatTensor tensor) : base(source)
            {
                _tensor = tensor;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var matrix = errorSignal.GetMatrix();
                var lap = context.LinearAlgebraProvider;

                var rowList = new List<IFloatVector>();
                for(uint i = 0; i < matrix.RowCount; i++) {
                    var rowMatrix = matrix.Row(i).ReshapeAsMatrix(_tensor.RowCount, _tensor.ColumnCount);
                    var matrixList = Enumerable.Repeat(rowMatrix, (int)_tensor.Depth).ToArray();
                    var tensor = lap.Create3DTensor(matrixList);
                    rowList.Add(tensor.ReshapeAsVector());
                }
                var errorMatrix = lap.CreateMatrixFromRows(rowList);

                return errorSignal.ReplaceWith(errorMatrix.Transpose());
            }
        }

        public TransposeAndCombineSignal(string? name = null) : base(name)
        {
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var tensor = context.Data.Get4DTensor() ?? throw new Exception("No data");
            var rowList = new List<IFloatVector>();

            for(uint i = 0; i < tensor.Count; i++) {
                var row = tensor.GetTensorAt(i).CombineDepthSlices().ReshapeAsVector();
                rowList.Add(row);
            }
            var output = context.LinearAlgebraProvider.CreateMatrixFromRows(rowList);

            AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, tensor));
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var tensor = signal.Get4DTensor() ?? throw new Exception("No data");
            var rowList = new List<IFloatVector>();

            for(uint i = 0; i < tensor.Count; i++) {
                var row = tensor.GetTensorAt(i).CombineDepthSlices().ReshapeAsVector();
                rowList.Add(row);
            }
            var output = context.LinearAlgebraProvider.CreateMatrixFromRows(rowList);

            return (new MatrixGraphData(output), () => new Backpropagation(this, tensor));
        }
    }
}
