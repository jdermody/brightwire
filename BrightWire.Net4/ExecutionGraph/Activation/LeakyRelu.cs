using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class LeakyRelu : NodeBase
    {
        class Backpropagation : IBackpropagation
        {
            readonly IMatrix _input;
            readonly LeakyRelu _source;

            public Backpropagation(LeakyRelu source, IMatrix matrix)
            {
                _input = matrix;
                _source = source;
            }

            public void Dispose()
            {
                _input.Dispose();
            }

            public IMatrix Backward(IMatrix errorSignal, IContext context, bool calculateOutput)
            {
                using (var od = _input.LeakyReluDerivative()) {
                    var delta = errorSignal.PointwiseMultiply(od);
                    //context.LearningContext.Log("leaky-relu-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
                    return delta;
                }
            }
        }

        public LeakyRelu(string name = null) : base(name) { }

        public override void SetPrimaryInput(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            var output = input.LeakyReluActivation();
            _AddNextGraphAction(context, new MatrixGraphData(output), () => new Backpropagation(this, input));
        }

        //public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        //{
        //    context.RegisterBackpropagation(new Backpropagation(this, input), channel);
        //    var output = Execute(input, channel, context);
        //    context.LearningContext.Log("leaky-relu", channel, GetHashCode(), input, output);
        //    return output;
        //}

        //public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        //{
        //    return input.LeakyReluActivation();
        //}
    }
}
