using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BrightWire.ExecutionGraph.Layer
{
    class GruLayer : IComponent
    {
        class AddGate
        {
            readonly IWire _input, _memory;

            public AddGate(GraphFactory graph, int inputSize, int memorySize, IComponent activation)
            {
                var inputChannel = graph.NextAvailableChannel;
                var memoryChannel = graph.NextAvailableChannel;

                (_input, _memory) = graph.Add(inputChannel, inputSize, memoryChannel, memorySize,
                    b => b.AddFeedForward(memorySize),
                    b => b.AddFeedForward(memorySize),
                    b => b.Add(activation).AddAction(new SetMemory(inputChannel))
                );
            }

            public IMatrix Activate(IMatrix input, IMatrix hidden, IBatchContext context)
            {
                _memory.Send(hidden, _memory.Channel, context);
                _input.Send(input, _input.Channel, context);
                return context.ExecutionContext.GetMemory(_input.Channel);
            }
        }
        class MultiplyGate
        {
            readonly IWire _input1, _input2, _output;

            public MultiplyGate(GraphFactory graph, int memorySize, Action<WireBuilder> builder)
            {
                var inputChannel = graph.NextAvailableChannel;
                var memoryChannel = graph.NextAvailableChannel;

                _input1 = graph.CreateWire(inputChannel, memorySize);
                _input2 = graph.CreateWire(memoryChannel, memorySize);

                var wireBuilder = graph.Multiply(inputChannel, _input1, _input2);
                builder?.Invoke(wireBuilder);
                _output = wireBuilder.Build();
            }

            public IWire OutputWire { get { return _output.LastWire; } }

            public void Multiply(IMatrix primary, IMatrix secondary, IBatchContext context)
            {
                _input2.Send(secondary, _input2.Channel, context);
                _input1.Send(primary, _input1.Channel, context);
            }
        }

        readonly MemoryFeeder _memoryFeeder;
        readonly AddGate _updateGate, _resetGate;
        readonly MultiplyGate _resetTimesPreviousHidden, _updateTimesPreviousHidden;
        readonly IWire _oneMinusUpdate, _input;

        public GruLayer(GraphFactory graph, int inputChannel, int inputSize, int outputSize, float[] memory)
        {
            var hiddenLayerSize = memory.Length;
            _memoryFeeder = new MemoryFeeder(graph.LinearAlgebraProvider, graph.NextAvailableChannel, memory);
            _updateGate = new AddGate(graph, inputSize, hiddenLayerSize, graph.Activation.Sigmoid);
            _resetGate = new AddGate(graph, inputSize, hiddenLayerSize, graph.Activation.Sigmoid);

            _resetTimesPreviousHidden = new MultiplyGate(graph, hiddenLayerSize, b => b
                .AddFeedForward(hiddenLayerSize)
                //.Add(graph.Activation.Tanh)
            );
            _updateTimesPreviousHidden = new MultiplyGate(graph, hiddenLayerSize, null);

            _oneMinusUpdate = graph.Build(graph.NextAvailableChannel, hiddenLayerSize)
                .AddOneMinusInput()
                .Build()
            ;

            _input = graph.Build(inputChannel, inputSize)
                .AddFeedForward(hiddenLayerSize)
                //.Add(graph.Activation.Tanh)
                .Add(_resetTimesPreviousHidden.OutputWire)
                .Add(graph.Activation.Tanh)
                .Multiply(_oneMinusUpdate)
                .Add(_updateTimesPreviousHidden.OutputWire)
                .AddAction(new SetMemory(_memoryFeeder.Channel))
                .Build()
            ;
        }

        public void Dispose()
        {
            // nop
        }

        public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        {
            // get the hidden state
            IMatrix hidden;
            if (context.Batch.CurrentSequence.SequenceIndex == 0)
                hidden = _memoryFeeder.OnStart(context);
            else
                hidden = _memoryFeeder.OnNext(context);

            // get the update and reset signals
            var update = _updateGate.Activate(input, hidden, context);
            var reset = _resetGate.Activate(input, hidden, context);

            // multiple reset and update signals with the previous hidden state
            _updateTimesPreviousHidden.Multiply(update, hidden, context);
            _resetTimesPreviousHidden.Multiply(reset, hidden, context);

            // subtract one from the update signal
            _oneMinusUpdate.Send(update, _oneMinusUpdate.Channel, context);

            // execute the primary channel
            _input.Send(input, channel, context);

            return context.ExecutionContext.GetMemory(_memoryFeeder.Channel);
        }

        public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        {
            return Execute(input, channel, context);
        }
    }
}
