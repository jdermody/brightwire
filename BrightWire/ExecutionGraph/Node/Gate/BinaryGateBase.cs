﻿using BrightWire.ExecutionGraph.Helper;
using System;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Base class for nodes that accept two input signals and output one signal
    /// </summary>
    public abstract class BinaryGateBase : NodeBase
    {
        IGraphData? _primary = null, _secondary = null;
        NodeBase? _primarySource = null, _secondarySource = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        protected BinaryGateBase(string? name) : base(name) { }

        /// <inheritdoc />
        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            IGraphData next;
            Func<IBackpropagate>? backProp;

            if (channel == 0) {
                _primarySource = source;
                _primary = signal;
                (next, backProp) = TryComplete(context);
            }else if (channel == 1) {
                _secondarySource = source;
                _secondary = signal;
                (next, backProp) = TryComplete(context);
            }
            else
                throw new NotImplementedException();

            return (this, next, backProp);
        }

        (IGraphData Next, Func<IBackpropagate>? BackProp) TryComplete(IGraphContext context)
        {
            if (_primary != null && _secondary != null && _primarySource != null && _secondarySource != null) {
                var (next, backProp) = Activate(context, _primary, _secondary, _primarySource, _secondarySource);
                _primary = _secondary = null;
                _primarySource = _secondarySource = null;
                return (next, backProp);
            }

            return (GraphData.Null, null);
        }

        /// <summary>
        /// When both the primary and secondary inputs have arrived
        /// </summary>
        /// <param name="context">Graph context</param>
        /// <param name="primary">Primary signal</param>
        /// <param name="secondary">Secondary signal</param>
        /// <param name="primarySource">Primary source node</param>
        /// <param name="secondarySource">Secondary source node</param>
        protected abstract (IGraphData Next, Func<IBackpropagate>? BackProp) Activate(IGraphContext context, IGraphData primary, IGraphData secondary, NodeBase primarySource, NodeBase secondarySource);
    }
}
