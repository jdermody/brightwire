using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using BrightWire.Source.Helper;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes an action when backpropagating
    /// </summary>
    class ExecuteBackwardAction : NodeBase, IHaveAction
    {
        class Backpropagation : SingleBackpropagationBase<ExecuteBackwardAction>
        {
            public Backpropagation(ExecuteBackwardAction source) : base(source) { }

            protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                return _source.Action.Execute(errorSignal, context);
            }
        }

	    public ExecuteBackwardAction(IAction action, string name = null) : base(name) { Action = action; }

        public IAction Action { get; set; }

	    public override void ExecuteForward(IContext context)
        {
            _AddNextGraphAction(context, context.Data, () => new Backpropagation(this));
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return (Action.GetType().AssemblyQualifiedName, Encoding.UTF8.GetBytes(Action.Serialise()));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            Action = (IAction)FormatterServices.GetUninitializedObject(TypeLoader.LoadType(description));
            Action.Initialise(Encoding.UTF8.GetString(data));
        }
    }
}
