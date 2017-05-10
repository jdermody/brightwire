using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    class OneMinusInput : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            protected override IMatrix _Backward(IMatrix errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                if (parents?.Any() == true) {
                    using (var minusOne = context.LinearAlgebraProvider.Create(errorSignal.RowCount, errorSignal.ColumnCount, -1f))
                        return errorSignal.PointwiseMultiply(errorSignal);
                }
                return null;
            }
        }

        public OneMinusInput(string name = null) : base(name)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            using (var ones = context.LinearAlgebraProvider.Create(input.RowCount, input.ColumnCount, 1f)) {
                var output = ones.Subtract(input);
                _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation());
            }
        }

        //public IMatrix Execute(IMatrix input, int channel, IContext context)
        //{
        //    using (var ones = _lap.Create(input.RowCount, input.ColumnCount, 1f))
        //        return ones.Subtract(input);
        //}

        //public IMatrix Train(IMatrix input, int channel, IContext context)
        //{
        //    context.RegisterBackpropagation(new Backpropagation(_lap), channel);
        //    return Execute(input, channel, context);
        //}
    }
}
