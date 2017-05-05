using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.Component;
using BrightWire.ExecutionGraph.Layer;
using BrightWire.ExecutionGraph.Output;
using BrightWire.ExecutionGraph.Wire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph
{
    public class WireBuilder
    {
        enum Action
        {
            Add,
            Multiply
        }
        class Component
        {
            readonly IComponent _component;
            readonly int _inputSize, _outputSize;
            readonly List<(Action, IWire)> _action = new List<(Action, IWire)>();

            public Component(IComponent component, int inputSize, int outputSize)
            {
                _component = component;
                _inputSize = inputSize;
                _outputSize = outputSize;
            }

            public void Add(Action action, IWire wire)
            {
                _action.Add((action, wire));
            }

            public IWire Get(GraphFactory factory, int channel, IWire destination)
            {
                IWire ret;
                if(_component != null)
                    ret = new WireToComponent(_inputSize, _outputSize, channel, _component, destination);
                else
                    ret = new WireToWire(_inputSize, _outputSize, channel, destination);

                //foreach(var action in _action) {
                //    IWire mutation = null;
                //    var lastWire = action.Item2.LastWire;

                //    if (action.Item1 == Action.Multiply)
                //        mutation = new MultiplyWires(_inputSize, channel, ret, lastWire);
                //    else if(action.Item1 == Action.Add)
                //        mutation = new AddWires(_inputSize, channel, ret, lastWire);

                //    mutation?.LastWire.SetDestination(destination);
                //    lastWire.SetDestination(mutation);
                //    ret.SetDestination(mutation);
                //    ret = mutation;
                //}
                return ret;
            }

            public bool HasActions => _action.Any();
            public IReadOnlyList<(Action, IWire)> Actions => _action;
            public int InputSize => _inputSize;
        }

        readonly GraphFactory _factory;
        readonly IGraphInput _input;
        readonly IWire _wire;
        readonly int _channel;
        readonly Stack<Component> _stack = new Stack<Component>();
        int _lastInputSize;

        public WireBuilder(GraphFactory factory, IGraphInput input)
        {
            _factory = factory;
            _input = input;
            _lastInputSize = input.InputSize;
            _channel = factory.NextAvailableChannel;
        }

        public WireBuilder(GraphFactory factory, int channel, int inputSize)
        {
            _channel = channel;
            _factory = factory;
            _lastInputSize = inputSize;
        }

        public WireBuilder(GraphFactory factory, IWire wire)
        {
            _factory = factory;
            _lastInputSize = wire.InputSize;
            _channel = wire.Channel;
            _wire = wire;
        }

        public WireBuilder AddFeedForward(int? layerSize = null)
        {
            var outputSize = layerSize ?? _input?.OutputSize ?? _lastInputSize;
            var layer = _factory.CreateFeedForward(_lastInputSize, outputSize);
            _stack.Push(new Component(layer, _lastInputSize, outputSize));
            _lastInputSize = outputSize;
            return this;
        }

        public WireBuilder AddSimpleRecurrent(IComponent activation, float[] memory)
        {
            var memorySize = memory.Length;
            var layer = new SimpleRecurrent(_factory, _channel, _lastInputSize, memorySize, memory, activation);
            _stack.Push(new Component(layer, _lastInputSize, memorySize));
            _lastInputSize = memorySize;
            return this;
        }

        public WireBuilder AddGru(float[] memory)
        {
            var memorySize = memory.Length;
            var layer = new GruLayer(_factory, _channel, _lastInputSize, memorySize, memory);
            _stack.Push(new Component(layer, _lastInputSize, memorySize));
            _lastInputSize = memorySize;
            return this;
        }

        WireBuilder _Mutate(Action action, IWire wire)
        {
            if (!_stack.Any())
                _stack.Push(new Component(null, _lastInputSize, _lastInputSize));
            _stack.Peek().Add(action, wire);
            return this;
        }

        public WireBuilder Multiply(IWire wire)
        {
            return _Mutate(Action.Multiply, wire);
        }

        public WireBuilder Add(IWire wire)
        {
            return _Mutate(Action.Add, wire);
        }

        public WireBuilder AddAction(IAction action)
        {
            return AddActions(action);
        }

        public WireBuilder AddActions(params IAction[] actionList)
        {
            Add(new GraphAction(actionList));
            return this;
        }

        public WireBuilder Add(IComponent component)
        {
            _stack.Push(new Component(component, _lastInputSize, _lastInputSize));
            return this;
        }

        public WireBuilder AddOneMinusInput()
        {
            _stack.Push(new Component(new OneMinusInput(_factory.LinearAlgebraProvider), _lastInputSize, _lastInputSize));
            return this;
        }

        public WireBuilder AddLogger(Action<IMatrix> capture)
        {
            Add(new Logger(capture));
            return this;
        }

        public IWire Build()
        {
            IWire last = null;
            Component pending = null;
            while(_stack.Any()) {
                var next = _stack.Pop();
                var nextWire = next.Get(_factory, _channel, last);
                if (pending != null) {
                    var savedNext = nextWire;
                    foreach(var action in pending.Actions) {
                        IWire actionWire = null;
                        if (action.Item1 == Action.Add)
                            actionWire = new AddWires(pending.InputSize, nextWire.Channel, nextWire, action.Item2);
                        else
                            actionWire = new MultiplyWires(pending.InputSize, nextWire.Channel, nextWire, action.Item2);
                        nextWire.SetDestination(actionWire);
                        actionWire.SetDestination(last);
                        nextWire = actionWire;
                    }
                    nextWire = savedNext;
                }

                last = nextWire;
                if (next.HasActions)
                    pending = next;
                else
                    pending = null;

                //last = new WireToComponent(next.InputSize, next.OutputSize, _channel, next.Component, last);
            }

            if (last == null)
                last = new WireToWire(_lastInputSize, _lastInputSize, _channel, null);
            else {
                _input?.AddTarget(last);
                _wire?.SetDestination(last);
            }
            return last;
        }
    }
}
