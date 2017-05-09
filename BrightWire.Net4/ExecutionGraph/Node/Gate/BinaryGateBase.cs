using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    public abstract class BinaryGateBase : NodeBase
    {
        IMatrix _primary = null, _secondary = null;
        INode _primarySource, _secondarySource = null;

        public BinaryGateBase(string name) : base(name) { }

        public override void ExecuteForward(IContext context)
        {
            _primarySource = context.Source;
            _primary = context.Data.GetAsMatrix();
            _TryComplete(context);
        }

        public override void _ExecuteForward(IContext context, int channel)
        {
            if (channel == 1) {
                _secondarySource = context.Source;
                _secondary = context.Data.GetAsMatrix();
                _TryComplete(context);
            }
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
            context.Forward(new GraphAction(this, new MatrixGraphData(output), new[] { _primarySource, _secondarySource }), backpropagation);
        }
    }
}
