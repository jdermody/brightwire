using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Component
{
    class GraphAction : IComponent
    {
        readonly IReadOnlyList<IAction> _actionList = new List<IAction>();

        public GraphAction(IReadOnlyList<IAction> actionList)
        {
            _actionList = actionList;
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        {
            foreach (var action in _actionList)
                action.Execute(input, channel, context);
            return input;
        }

        public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        {
            return Execute(input, channel, context);
        }
    }
}
