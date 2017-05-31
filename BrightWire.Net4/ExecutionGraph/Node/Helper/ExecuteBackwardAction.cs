using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes an action when backpropagating
    /// </summary>
    class ExecuteBackwardAction : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<ExecuteBackwardAction>
        {
            public Backpropagation(ExecuteBackwardAction source) : base(source) { }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                return _source._action.Execute(errorSignal, context);
            }
        }
        IAction _action;

        public ExecuteBackwardAction(IAction action, string name = null) : base(name) { _action = action; }

        public override void ExecuteForward(IContext context)
        {
            _AddNextGraphAction(context, context.Data, () => new Backpropagation(this));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return (_action.GetType().FullName, Encoding.UTF8.GetBytes(_action.Serialise()));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _action = (IAction)FormatterServices.GetUninitializedObject(Type.GetType(description));
            _action.Initialise(Encoding.UTF8.GetString(data));
        }
    }
}
