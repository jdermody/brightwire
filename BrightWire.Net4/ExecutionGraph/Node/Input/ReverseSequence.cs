using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Input
{
    class ReverseSequence : NodeBase
    {
        public ReverseSequence(string name = null) : base(name)
        {

        }

        public override void SetPrimaryInput(IContext context)
        {
            var curr = context.BatchSequence;
            var batch = curr.MiniBatch;
            var reversed = batch.GetSequenceAtIndex(batch.SequenceLength - curr.SequenceIndex).Input;

            context.Forward(new GraphAction(this, new MatrixGraphData(reversed), context.Source), null);
        }
    }
}
