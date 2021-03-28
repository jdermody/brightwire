using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Outputs the two input signals multiplied together
    /// </summary>
    internal class MultiplyGate : BinaryGateBase
    {
        class Backpropagation : BackpropagationBase<MultiplyGate>
        {
            readonly IFloatMatrix _primary, _secondary;
            readonly NodeBase _primarySource;
            readonly NodeBase _secondarySource;

            public Backpropagation(MultiplyGate source, IFloatMatrix primary, IFloatMatrix secondary, NodeBase primarySource, NodeBase secondarySource) : base(source)
            {
                _primary = primary;
                _secondary = secondary;
                _primarySource = primarySource;
                _secondarySource = secondarySource;
            }

            protected override void DisposeMemory(bool isDisposing)
            {
                //_input1.Dispose();
                //_input2.Dispose();
            }

            public override IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents)
            {
                var es = errorSignal.GetMatrix();
                var delta1 = es.PointwiseMultiply(_secondary);
                var delta2 = es.PointwiseMultiply(_primary);
                yield return (errorSignal.ReplaceWith(delta1), context, _primarySource);
                yield return (errorSignal.ReplaceWith(delta2), context, _secondarySource);
            }
        }
        public MultiplyGate(string? name = null) : base(name) { }

        protected override (IFloatMatrix Next, Func<IBackpropagate>? BackProp) Activate(IGraphSequenceContext context, IFloatMatrix primary, IFloatMatrix secondary, NodeBase primarySource, NodeBase secondarySource)
        {
            var output = primary.PointwiseMultiply(secondary);

            return (output, () => new Backpropagation(this, primary,  secondary, primarySource, secondarySource));
        }
    }
}
