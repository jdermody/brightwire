using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Layer
{
    internal class SimpleRecurrent : IComponent
    {
        readonly IWire _input, _memory;
        readonly MemoryFeeder _memoryFeeder;

        public SimpleRecurrent(GraphFactory graph, int inputChannel, int inputSize, int outputSize, float[] memory, IComponent activation)
        {
            int hiddenLayerSize = memory.Length;
            _memoryFeeder = new MemoryFeeder(graph.LinearAlgebraProvider, graph.NextAvailableChannel, memory);

            // create the network
            (_input, _memory) = graph.Add(inputChannel, inputSize, _memoryFeeder.Channel, hiddenLayerSize, 
                b => b.AddFeedForward(hiddenLayerSize),
                b => b.AddFeedForward(hiddenLayerSize),
                b => b.Add(graph.Activation.Relu).AddAction(new SetMemory(_memoryFeeder.Channel))
            );
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
