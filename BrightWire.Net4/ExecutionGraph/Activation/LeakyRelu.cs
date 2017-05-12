using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Activation
{
    class LeakyRelu : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            readonly IReadOnlyList<IMatrix> _input;
            readonly LeakyRelu _source;

            public Backpropagation(LeakyRelu source, IReadOnlyList<IMatrix> matrix)
            {
                _input = matrix;
                _source = source;
            }

            protected override void _Dispose(bool isDisposing)
            {
                foreach(var item in _input)
                    item.Dispose();
            }

            protected override IGraphData _Backward(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                return context.ToGraphData(_input.Zip(errorSignal.Decompose(), (input, es) => {
                    using (var od = input.LeakyReluDerivative()) {
                        var delta = es.PointwiseMultiply(od);
                        //context.LearningContext.Log("leaky-relu-backpropagation", channel, _source.GetHashCode(), errorSignal, delta);
                        return delta;
                    }
                }));
            }
        }

        public LeakyRelu(string name = null) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data.Decompose();
            var output = context.ToGraphData(input.Select(m => m.LeakyReluActivation()));
            _AddNextGraphAction(context, output, () => new Backpropagation(this, input));
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
