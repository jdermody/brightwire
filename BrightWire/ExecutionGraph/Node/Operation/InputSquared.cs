using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Operation
{
    internal class InputSquared : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<InputSquared>
        {
			readonly IFloatMatrix _input;

            public Backpropagation(InputSquared source, IFloatMatrix input) : base(source)
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

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            var input = context.Data.GetMatrix();
            var inputSquared = input.PointwiseMultiply(input);
            AddNextGraphAction(context, context.Data.ReplaceWith(inputSquared), () => new Backpropagation(this, input));
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            var input = context.Data.GetMatrix();
            var inputSquared = input.PointwiseMultiply(input);
            return (context.Data.ReplaceWith(inputSquared), () => new Backpropagation(this, input));
        }
    }
}
