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
        public override void ExecuteForward(IGraphContext context)
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
        protected override void ExecuteForwardInternal(IGraphContext context, uint channel)
        {
            if (channel == 1) {
                _secondarySource = context.Source;
                _secondary = context.Data.GetMatrix();
                TryComplete(context);
            }
        }

        void TryComplete(IGraphContext context)
        {
            if (_primary != null && _secondary != null) {
                Activate(context, _primary, _secondary);
                _primary = _secondary = null;
                _primarySource = _secondarySource = null;
            }
        }

        /// <summary>
        /// When both the primary and secondary inputs have arrived
        /// </summary>
        /// <param name="context">Graph context</param>
        /// <param name="primary">Primary signal</param>
        /// <param name="secondary">Secondary signal</param>
        protected abstract void Activate(IGraphContext context, IFloatMatrix primary, IFloatMatrix secondary);

        /// <summary>
        /// Records the network activity
        /// </summary>
        /// <param name="context">Graph context</param>
        /// <param name="output">The output signal</param>
        /// <param name="backpropagation">Backpropagation creator (optional)</param>
        protected void AddHistory(IGraphContext context, IFloatMatrix output, Func<IBackpropagation>? backpropagation)
        {
            if (_primarySource == null || _secondarySource == null)
                throw new Exception("Source nodes cannot be null");

            context.AddForward(new TrainingAction(this, new MatrixGraphData(output), new[] { _primarySource, _secondarySource }), backpropagation);
        }
    }
}
