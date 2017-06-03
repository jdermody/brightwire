using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Node.Input
{
    /// <summary>
    /// Inputs the opposite sequential item from the input index (for bidirectional recurrent neural networks)
    /// https://en.wikipedia.org/wiki/Bidirectional_recurrent_neural_networks
    /// </summary>
    class ReverseSequence : NodeBase
    {
        public ReverseSequence(string name = null) : base(name)
        {

        }

        public override void ExecuteForward(IContext context)
        {
            var curr = context.BatchSequence;
            var batch = curr.MiniBatch;
            var reversed = batch.GetSequenceAtIndex(batch.SequenceCount - curr.SequenceIndex - 1).Input;

            context.AddForward(new TrainingAction(this, reversed, context.Source), null);
        }
    }
}
