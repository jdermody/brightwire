using System;
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
            readonly ITensor4D _tensor;

            public Backpropagation(TransposeAndCombineSignal source, ITensor4D tensor) : base(source)
            {
                _tensor = tensor;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                var matrix = errorSignal.GetMatrix();
                var lap = context.GetLinearAlgebraProvider();

                var rowList = new IVector[matrix.RowCount];
                for(uint i = 0; i < matrix.RowCount; i++) {
                    using var rowMatrix = matrix.GetRow(i).Segment.ToMatrix(lap, _tensor.RowCount, _tensor.ColumnCount);
                    var matrixList = Enumerable.Repeat(rowMatrix, (int)_tensor.Depth).ToArray();
                    var tensor = lap.CreateTensor3D(matrixList);
                    rowList[i] = tensor.Reshape();
                }
                var errorMatrix = lap.CreateMatrixFromRows(rowList);
                rowList.DisposeAll();
                return errorSignal.ReplaceWith(errorMatrix.Transpose());
            }
        }

        public TransposeAndCombineSignal(string? name = null) : base(name)
        {
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var tensor = signal.Get4DTensor() ?? throw new Exception("No data");
            var rowList = new IVector[tensor.Count];

            for(uint i = 0; i < tensor.Count; i++) {
                using var matrix = tensor.GetTensor(i).CombineDepthSlices();
                rowList[i] = matrix.Reshape();
            }
            var output = context.GetLinearAlgebraProvider().CreateMatrixFromRows(rowList);
            rowList.DisposeAll();
            return (this, output.AsGraphData(), () => new Backpropagation(this, tensor));
        }
    }
}
