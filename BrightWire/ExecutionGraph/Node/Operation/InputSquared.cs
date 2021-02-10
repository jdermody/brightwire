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

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                var es = errorSignal.GetMatrix();
	            var err = es.PointwiseMultiply(_input);
	            err.Multiply(2f);
                return errorSignal.ReplaceWith(err);
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
    }
}
