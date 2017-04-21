using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Layer
{
    internal class SimpleRecurrent : IComponent
    {
        readonly IWire _output, _input, _memory;
        readonly MemoryFeeder _memoryFeeder;

        public SimpleRecurrent(GraphFactory graph, IPropertySet propertySet, int inputChannel, int inputSize, int outputSize, float[] memory)
        {
            // create the network
            int hiddenLayerSize = memory.Length;
            int memoryChannel = graph.NextAvailableChannel;
            _memoryFeeder = new MemoryFeeder(propertySet.LinearAlgebraProvider, memoryChannel, memory);

            // create the input channel
            _input = graph.Build(inputChannel, inputSize, propertySet)
                .AddFeedForward(hiddenLayerSize)
                .Build()
            ;

            // create the memory channel
            _memory = graph.Build(memoryChannel, hiddenLayerSize, propertySet)
                .AddFeedForward(hiddenLayerSize)
                .Build()
            ;

            // extend the input channel with the merged results
            _output = graph.Add(inputChannel, propertySet, _input, _memory)
                .Add(graph.Activation.Relu)
                .AddAction(new SetMemory(memoryChannel))
                .Build()
            ;
        }

        public void Dispose()
        {
            _memoryFeeder.Dispose();
        }

        public IMatrix Execute(IMatrix input, int channel, IBatchContext context)
        {
            // send the hidden state into the memory channel
            if (context.Batch.CurrentSequence.SequenceIndex == 0)
                _memory.Send(_memoryFeeder.OnStart(context), _memoryFeeder.Channel, context);
            else
                _memory.Send(_memoryFeeder.OnNext(context), _memoryFeeder.Channel, context);

            // activate the input channel
            _input.Send(input, channel, context);

            // retrieve the output
            var ret = context.ExecutionContext.GetMemory(_memoryFeeder.Channel);
            return ret;
        }

        public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        {
            return Execute(input, channel, context);
        }
    }
}
