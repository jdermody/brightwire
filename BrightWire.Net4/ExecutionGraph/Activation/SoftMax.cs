using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class SoftMax : NodeBase
    {
        class Backpropagation : IBackpropagation
        {
            readonly IReadOnlyList<IVector> _rows;
            readonly SoftMax _source;

            public Backpropagation(SoftMax source, IReadOnlyList<IVector> rows)
            {
                _source = source;
                _rows = rows;
            }

            public IMatrix Backward(IMatrix errorSignal, IContext context, bool calculateOutput)
            {
                var lap = context.LinearAlgebraProvider;
                var rowList = new List<IVector>();
                for (var i = 0; i < errorSignal.RowCount; i++) {
                    using (var derivative = _rows[i].SoftmaxDerivative()) {
                        var sm = derivative.Multiply(errorSignal.Row(i));
                        rowList.Add(sm.ConvertInPlaceToVector());
                    }
                }
                var ret = lap.Create(rowList);
                foreach (var item in rowList)
                    item.Dispose();
                //context.LearningContext.Log("softmax-backpropagation", channel, _source.GetHashCode(), errorSignal, ret);
                return ret;
            }

            public void Dispose()
            {
                foreach (var item in _rows)
                    item.Dispose();
            }
        }

        public SoftMax(string name = null) : base(name) { }

        public override void SetPrimaryInput(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            var rowList = new List<IVector>();

            for (var i = 0; i < input.RowCount; i++) {
                using (var row = input.Row(i))
                    rowList.Add(row.Softmax());
            }

            var output = context.LinearAlgebraProvider.Create(rowList);
            context.Add(new GraphAction(this, new MatrixGraphData(output)), () => new Backpropagation(this, rowList));
        }

        //public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        //{
        //    var ret = _Execute(input, context);
        //    foreach (var item in ret.Item1)
        //        item.Dispose();

        //    return ret.Item2;
        //}

        //(IReadOnlyList<IVector>, IMatrix) _Execute(IMatrix input, IBatchContext context)
        //{
        //    var rowList = new List<IVector>();
        //    for (var i = 0; i < input.RowCount; i++) {
        //        using (var row = input.Row(i))
        //            rowList.Add(row.Softmax());
        //    }

        //    var ret = context.LinearAlgebraProvider.Create(rowList);
        //    return (rowList, ret);
        //}

        //public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        //{
        //    var ret = _Execute(input, context);
        //    context.RegisterBackpropagation(new Backpropagation(this, ret.Item1), channel);
        //    context.LearningContext.Log("softmax", channel, GetHashCode(), input, ret.Item2);
        //    return ret.Item2;
        //}
    }
}
