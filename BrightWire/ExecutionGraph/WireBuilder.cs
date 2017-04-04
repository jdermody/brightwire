using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Wire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph
{
    public class WireBuilder
    {
        readonly IPropertySet _propertySet;
        readonly GraphFactory _factory;
        readonly int _inputSize;
        readonly Stack<(ILayer Layer, IComponent Component)> _stack = new Stack<(ILayer, IComponent)>();
        int _lastInputSize;

        public WireBuilder(GraphFactory factory, int inputSize, IPropertySet propertySet)
        {
            _factory = factory;
            _propertySet = propertySet;
            _lastInputSize = inputSize;
        }

        public WireBuilder AddFeedForward(int layerSize)
        {
            var layer = _factory.CreateFeedForward(_lastInputSize, layerSize, _propertySet);
            _stack.Push((layer, null));
            _lastInputSize = layerSize;
            return this;
        }

        public WireBuilder Add(ILayer layer)
        {
            _stack.Push((layer, null));
            return this;
        }

        public WireBuilder Add(IComponent component)
        {
            _stack.Push((null, component));
            return this;
        }

        public IWire Build()
        {
            IWire last = null;
            while(_stack.Any()) {
                var next = _stack.Pop();
                if (next.Layer != null)
                    last = new LayerToWire(next.Layer, last);
                else
                    last = new ToComponent(next.Component);
            }
            return last;
        }
    }
}
