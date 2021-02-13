using BrightWire.ExecutionGraph.Helper;
using System;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Base class for nodes that accept two input signals and output one signal
    /// </summary>
    public abstract class BinaryGateBase : NodeBase
    {
        IFloatMatrix? _primary = null, _secondary = null;
        INode? _primarySource = null, _secondarySource = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        protected BinaryGateBase(string? name) : base(name) { }

        /// <summary>
        /// Executes on the primary channel
        /// </summary>
        /// <param name="context">The graph context</param>
        public override void ExecuteForward(IGraphSequenceContext context)
        {
            _primarySource = context.Source;
            _primary = context.Data.GetMatrix();
            TryComplete(context);
        }

        /// <summary>
        /// Executes on a secondary channel
        /// </summary>
        /// <param name="context">The graph context</param>
        /// <param name="channel">The channel</param>
        protected override void ExecuteForwardInternal(IGraphSequenceContext context, uint channel)
        {
            if (channel == 1) {
                _secondarySource = context.Source;
                _secondary = context.Data.GetMatrix();
                TryComplete(context);
            }
        }

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            IGraphData next;
            Func<IBackpropagate>? backProp = null;

            if (channel == 0) {
                _primarySource = source;
                _primary = signal.GetMatrix();
                (next, backProp) = TryComplete2(signal, context);
            }else if (channel == 1) {
                _secondarySource = source;
                _secondary = signal.GetMatrix();
                (next, backProp) = TryComplete2(signal, context);
            }
            else
                throw new NotImplementedException();

            return (this, next, backProp);
        }

        void TryComplete(IGraphSequenceContext context)
        {
            if (_primary != null && _secondary != null) {
                Activate(context, _primary, _secondary);
                _primary = _secondary = null;
                _primarySource = _secondarySource = null;
            }
        }

        (IGraphData Next, Func<IBackpropagate>? BackProp) TryComplete2(IGraphData signal, IGraphSequenceContext context)
        {
            if (_primary != null && _secondary != null) {
                var (next, backProp) = Activate2(context, _primary, _secondary);
                _primary = _secondary = null;
                _primarySource = _secondarySource = null;
                return (signal.ReplaceWith(next), backProp);
            }

            return (NullGraphData.Instance, null);
        }

        /// <summary>
        /// When both the primary and secondary inputs have arrived
        /// </summary>
        /// <param name="context">Graph context</param>
        /// <param name="primary">Primary signal</param>
        /// <param name="secondary">Secondary signal</param>
        protected abstract void Activate(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary);

        protected abstract (IFloatMatrix Next, Func<IBackpropagate>? BackProp) Activate2(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary);

        /// <summary>
        /// Records the network activity
        /// </summary>
        /// <param name="context">Graph context</param>
        /// <param name="output">The output signal</param>
        /// <param name="backpropagation">Backpropagation creator (optional)</param>
        protected void AddHistory(IGraphSequenceContext context, IFloatMatrix output, Func<IBackpropagate>? backpropagation)
        {
            if (_primarySource == null || _secondarySource == null)
                throw new Exception("Source nodes cannot be null");

            context.AddForward(this, new MatrixGraphData(output), backpropagation, _primarySource, _secondarySource);
        }
    }
}
