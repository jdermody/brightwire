using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    class GraphExecuteAction : NodeBase
    {
        readonly IAction _action;

        public GraphExecuteAction(IAction action) : base(null) { _action = action; }

        public override void SetPrimaryInput(IContext context)
        {
            var input = context.Data.GetAsMatrix();
            _action.Execute(input, context);
            context.Add(new GraphAction(this, context.Data), null);
        }
    }
}
