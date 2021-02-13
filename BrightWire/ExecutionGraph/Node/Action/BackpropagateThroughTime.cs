namespace BrightWire.ExecutionGraph.Node.Action
{
    /// <summary>
    /// Backpropagates through time (for recurrent neural networks)
    /// </summary>
    internal class BackpropagateThroughTime : IAction
    {
        public BackpropagateThroughTime()
        {
        }

        public void Initialise(string data)
        {
            //_errorMetric = GenericActivator.Create<IErrorMetric>(TypeLoader.LoadType(data));
        }

        public string Serialise()
        {
            return "";
            //return TypeLoader.GetTypeName(_errorMetric);
        }

        public IGraphData Execute(IGraphData input, IGraphSequenceContext context, INode node)
        {
            var output = input.GetMatrix();
            if (context.LearningContext != null) {
                var batchSequence = context.BatchSequence;
                var target = batchSequence.Target?.GetMatrix();
                if (target == null)
                    context.LearningContext.DeferBackpropagation(null, delta => context.Backpropagate(node, delta));
                else {
                    var gradient = context.LearningContext.ErrorMetric.CalculateGradient(context, output, target);
                    context.LearningContext.DeferBackpropagation(input.ReplaceWith(gradient), delta => context.Backpropagate(node, delta));
                }
            }
            return input;
        }
    }
}
