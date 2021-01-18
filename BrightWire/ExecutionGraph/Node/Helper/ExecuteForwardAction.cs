using System.Runtime.Serialization;
using System.Text;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes an action when executing forward
    /// </summary>
    internal class ExecuteForwardAction : NodeBase, IHaveAction
    {
	    public ExecuteForwardAction(IAction action, string name = null) : base(name) { Action = action; }

        public IAction Action { get; set; }

	    public override void ExecuteForward(IGraphContext context)
        {
            var input = context.Data;
            var output = Action.Execute(input, context);
            _AddNextGraphAction(context, output ?? input, null);
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
