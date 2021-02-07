using System;
using System.Text;
using BrightData.Helper;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes an action when backpropagating
    /// </summary>
    internal class ExecuteBackwardAction : NodeBase, IHaveAction
    {
        class Backpropagation : SingleBackpropagationBase<ExecuteBackwardAction>
        {
            public Backpropagation(ExecuteBackwardAction source) : base(source) { }

            protected override IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
            {
                return _source.Action.Execute(errorSignal, context);
            }
        }

	    public ExecuteBackwardAction(IAction action, string? name = null) : base(name) { Action = action; }

        public IAction Action { get; set; }

	    public override void ExecuteForward(IGraphSequenceContext context)
        {
            AddNextGraphAction(context, context.Data, () => new Backpropagation(this));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return (TypeLoader.GetTypeName(Action), Encoding.UTF8.GetBytes(Action.Serialise()));
        }

        protected override void Initalise(GraphFactory factory, string? description, byte[]? data)
        {
            if (description == null)
                throw new Exception("Description cannot be null");

            Action = GenericActivator.CreateUninitialized<IAction>(TypeLoader.LoadType(description));
            Action.Initialise(data != null ? Encoding.UTF8.GetString(data) : "");
        }
    }
}
