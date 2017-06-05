using System;
using System.Runtime.Serialization;
using System.Text;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes an action when executing forward
    /// </summary>
    class ExecuteForwardAction : NodeBase, IHaveAction
    {
        IAction _action;

        public ExecuteForwardAction(IAction action, string name = null) : base(name) { _action = action; }

        public IAction Action => _action;

        public override void ExecuteForward(IContext context)
        {
            var input = context.Data;
            var output = _action.Execute(input, context);
            _AddNextGraphAction(context, output ?? input, null);
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
