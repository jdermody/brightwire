using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    public abstract class GateBase : NodeBase
    {
        IContext _primary = null, _secondary = null;

        public GateBase(string name) : base(name) { }

        public override void SetPrimaryInput(IContext context)
        {
            _primary = context;
        }

        public override void SetSecondaryInput(IContext context)
        {
            _secondary = context;
        }

        void _TryComplete()
        {
            if (_primary != null && _secondary != null) {
                _Activate(_primary, _secondary);
                _primary = _secondary = null;
            }
        }

        protected abstract void _Activate(IContext primary, IContext secondary);
    }
}
