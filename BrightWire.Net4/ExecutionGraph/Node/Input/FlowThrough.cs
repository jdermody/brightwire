using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Input
{
    class FlowThrough : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase
        {
            readonly FlowThrough _node;
            readonly int _lastColumns, _lastRows;

            public Backpropagation(FlowThrough node, int lastColumns, int lastRows)
            {
                _node = node;
                _lastColumns = lastColumns;
                _lastRows = lastRows;
            }

            protected override IGraphData _Backward(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                var errorMatrix = errorSignal.GetMatrix();
                if (errorMatrix.ColumnCount == _lastColumns && errorMatrix.RowCount == _lastRows)
                    _node._backpropagation = errorMatrix;
                return errorSignal;
            }
        }
        readonly bool _captureBackpropagation;
        IMatrix _backpropagation = null;

        public FlowThrough(bool captureBackpropagation = false) : base(null)
        {
            _captureBackpropagation = captureBackpropagation;
        }

        public override void ExecuteForward(IContext context)
        {
            Func<IBackpropagation> backProp = null;
            if (_captureBackpropagation) {
                _backpropagation?.Dispose();
                _backpropagation = null;
                var data = context.Data.GetMatrix();
                backProp = () => new Backpropagation(this, data.ColumnCount, data.RowCount);
            }

            _AddNextGraphAction(context, context.Data, backProp);
        }

        public IMatrix ErrorSignal
        {
            get
            {
                return _backpropagation;
            }
        }
    }
}
