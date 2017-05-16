using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    class ExecuteAction : NodeBase
    {
        IAction _action;

        public ExecuteAction(IAction action) : base(null) { _action = action; }

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
