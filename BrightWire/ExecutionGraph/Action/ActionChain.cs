using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Action
{
    public class ActionChain : IAction
    {
        readonly IReadOnlyList<IAction> _actionList;

        public ActionChain(params IAction[] actions)
        {
            _actionList = actions;
        }

        public void Execute(IMatrix input, int channel, IBatchContext context)
        {
            foreach (var action in _actionList)
                action.Execute(input, channel, context);
        }
    }
}
