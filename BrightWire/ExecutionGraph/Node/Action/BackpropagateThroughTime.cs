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

        public IGraphData Execute(IGraphData input, IGraphSequenceContext context, NodeBase node)
        {
            context.Data = input;

            if (context.LearningContext != null) {
                var batchSequence = context.BatchSequence;
                var target = batchSequence.Target?.GetMatrix();

                if (target == null)
                    context.LearningContext.DeferBackpropagation(null, context.Backpropagate);
                else {
                    var gradient = context.LearningContext.ErrorMetric.CalculateGradient(context, input.GetMatrix(), target);
                    context.LearningContext.DeferBackpropagation(input.ReplaceWith(gradient), context.Backpropagate);
                }
            }
            return input;
        }
    }
}
