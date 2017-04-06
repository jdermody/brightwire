using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.Component;
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
        readonly IWire _wire;
        readonly Stack<(IComponent Component, int InputSize, int OutputSize)> _stack = new Stack<(IComponent Component, int InputSize, int OutputSize)>();
        int _lastInputSize;

        public WireBuilder(GraphFactory factory, IGraphInput input, IPropertySet propertySet)
        {
            _factory = factory;
            _propertySet = propertySet;
            _input = input;
            _lastInputSize = input.InputSize;
        }

        public WireBuilder(GraphFactory factory, int inputSize, IPropertySet propertySet)
        {
            _factory = factory;
            _propertySet = propertySet;
            _lastInputSize = inputSize;
        }

        public WireBuilder(GraphFactory factory, IWire wire, IPropertySet propertySet)
        {
            _factory = factory;
            _propertySet = propertySet;
            _lastInputSize = wire.InputSize;
            _wire = wire;
        }

        public WireBuilder AddFeedForward(int? layerSize = null)
        {
            var outputSize = layerSize ?? _input?.OutputSize ?? _lastInputSize;
            var layer = _factory.CreateFeedForward(_lastInputSize, outputSize, _propertySet);
            _stack.Push((layer, _lastInputSize, outputSize));
            _lastInputSize = outputSize;
            return this;
        }

        public WireBuilder AddBackpropagation(IErrorMetric errorMetric)
        {
            Add(new Backpropagate(errorMetric));
            return this;
        }

        public WireBuilder AddSetMemory(int channel)
        {
            Add(new SetMemory(channel));
            return this;
        }

        public WireBuilder Add(IComponent component)
        {
            _stack.Push((component, _lastInputSize, _lastInputSize));
            return this;
        }

        public IWire Build(int channel = 0)
        {
            IWire last = null;
            while(_stack.Any()) {
                var next = _stack.Pop();
                last = new WireToComponent(next.InputSize, next.OutputSize, channel, next.Component, last);
            }
            if (last != null) {
                _input?.AddTarget(last);
                _wire?.SetDestination(last);
            }
            return last;
        }
    }
}
