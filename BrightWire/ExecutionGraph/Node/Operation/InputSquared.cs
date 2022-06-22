using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    internal class InputSquared : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<InputSquared>
        {
			readonly IMatrix _input;

            public Backpropagation(InputSquared source, IMatrix input) : base(source)
            {
				_input = input;
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                var es = errorSignal.GetMatrix();
                var err = es.PointwiseMultiply(_input);
                err.Multiply(2f);
                return errorSignal.ReplaceWith(err);
            }
        }

        public InputSquared(string? name = null) : base(name)
        {
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var input = signal.GetMatrix();
            var inputSquared = input.PointwiseMultiply(input);
            return (this, signal.ReplaceWith(inputSquared), () => new Backpropagation(this, input));
        }
    }
}
