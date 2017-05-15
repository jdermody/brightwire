using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Input
{
    class FlowThrough : NodeBase
    {
        public FlowThrough() : base(null)
        {
        }

        public override void ExecuteForward(IContext context)
        {
            _AddNextGraphAction(context, context.Data, null);
        }
    }
}
