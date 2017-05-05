using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class Tanh : NodeBase
    {
        class Backpropagation : IBackpropagation
        {
            readonly IMatrix _input;
            readonly Tanh _source;

            public Backpropagation(Tanh source, IMatrix matrix)
            {
                _source = source;
                _input = matrix;
            }

            public void Dispose()
            {
                _input.Dispose();
            }

            public IMatrix Backward(IMatrix errorSignal, IContext context, bool calculateOutput)
            {
                using (var od = _input.TanhDerivative()) {
                    var delta = errorSignal.PointwiseMultiply(od);
                    //context.LearningContext.Log("tanh-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
                    return delta;
                }
            }
        }

        public Tanh(string name = null) : base(name) { }

        public override void SetPrimaryInput(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            var output = input.TanhActivation();
            context.Add(new GraphAction(this, new MatrixGraphData(output)), () => new Backpropagation(this, input));
        }

        //public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        //{
        //    context.RegisterBackpropagation(new Backpropagation(this, input), channel);
        //    var output = Execute(input, channel, context);
        //    context.LearningContext.Log("tanh", channel, GetHashCode(), input, output);
        //    return output;
        //}

        //public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        //{
        //    return input.TanhActivation();
        //}
    }
}
