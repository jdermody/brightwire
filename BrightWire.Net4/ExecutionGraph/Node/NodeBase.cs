using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node
{
    public abstract class NodeBase : INode
    {
        readonly string _id, _name;
        readonly List<IWire> _output = new List<IWire>();

        public NodeBase(string name)
        {
            _id = Guid.NewGuid().ToString("n");
            _name = name;
        }

        #region Disposal
        ~NodeBase()
        {
            _Dispose(false);
        }
        public void Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void _Dispose(bool isDisposing) { } 
        #endregion

        public string Id => _id;
        public string Name => _name;
        public IReadOnlyList<IWire> Output => _output;

        public void AddOutput(IWire wire) => _output.Add(wire);
        public abstract void SetPrimaryInput(IContext context);
        public virtual void SetSecondaryInput(IContext context)
        {
            SetPrimaryInput(context);
        }
    }
}
