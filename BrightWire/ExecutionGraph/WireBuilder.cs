using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Activation;
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
        readonly IGraphInput _input;
        readonly Stack<(ILayer Layer, IComponent Component)> _stack = new Stack<(ILayer, IComponent)>();
        int _lastInputSize;

        public WireBuilder(GraphFactory factory, IGraphInput input, IPropertySet propertySet)
        {
            _factory = factory;
            _propertySet = propertySet;
            _input = input;
            _lastInputSize = input.InputSize;
        }

        public WireBuilder AddFeedForward(int? layerSize)
        {
            var layer = _factory.CreateFeedForward(_lastInputSize, layerSize ?? _input.OutputSize, _propertySet);
            _stack.Push((layer, null));
            _lastInputSize = layerSize ?? _input.OutputSize;
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
                    last = new WireToLayer(next.Layer, last);
                else
                    last = new WireToComponent(next.Component, last);
            }
            if (last != null)
                _input.AddTarget(last);
            return last;
        }
    }
}
