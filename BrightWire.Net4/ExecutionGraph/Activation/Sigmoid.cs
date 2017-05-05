using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class Sigmoid : NodeBase
    {
        class Backpropagation : IBackpropagation
        {
            readonly IMatrix _input;
            readonly Sigmoid _source;

            public Backpropagation(Sigmoid source, IMatrix matrix)
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
                using (var od = _input.SigmoidDerivative()) {
                    var delta = errorSignal.PointwiseMultiply(od);
                    //context.LearningContext.Log("sigmoid-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
                    return delta;
                }
            }
        }

        public Sigmoid(string name = null) : base(name) { }

        public override void SetPrimaryInput(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            var output = input.SigmoidActivation();
            context.Add(new GraphAction(this, new MatrixGraphData(output)), () => new Backpropagation(this, input));
        }

        //public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        //{
        //    context.RegisterBackpropagation(new Backpropagation(this, input), channel);
        //    var output = Execute(input, channel, context);
        //    context.LearningContext.Log("sigmoid", channel, GetHashCode(), input, output);
        //    return output;
        //}

        //public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        //{
        //    return input.SigmoidActivation();
        //}
    }
}
