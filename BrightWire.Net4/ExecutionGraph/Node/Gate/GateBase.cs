using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    public abstract class GateBase : NodeBase
    {
        IMatrix _primary = null, _secondary = null;
        INode _primarySource, _secondarySource = null;

        public GateBase(string name) : base(name) { }

        public override void SetPrimaryInput(IContext context)
        {
            _primarySource = context.Source;
            _primary = context.Data.GetAsMatrix();
            _TryComplete(context);
        }

        public override void SetSecondaryInput(IContext context)
        {
            _secondarySource = context.Source;
            _secondary = context.Data.GetAsMatrix();
            _TryComplete(context);
        }

        void _TryComplete(IContext context)
        {
            if (_primary != null && _secondary != null) {
                _Activate(context, _primary, _secondary);
                _primary = _secondary = null;
                _primarySource = _secondarySource = null;
            }
        }

        protected abstract void _Activate(IContext context, IMatrix primary, IMatrix secondary);

        protected void _AddHistory(IContext context, IMatrix output, Func<IBackpropagation> backpropagation)
        {
            context.Add(new GraphAction(this, new MatrixGraphData(output), _primarySource, _secondarySource), backpropagation);
        }
    }
}
