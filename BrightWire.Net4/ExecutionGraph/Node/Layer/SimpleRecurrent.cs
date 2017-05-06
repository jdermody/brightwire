using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    internal class SimpleRecurrent : NodeBase
    {
        readonly MemoryFeeder _memoryFeeder;
        readonly INode _output;

        public SimpleRecurrent(GraphFactory graph, int inputSize, float[] memory, INode activation, string name = null)
            : base(name)
        {
            int hiddenLayerSize = memory.Length;
            _memoryFeeder = new MemoryFeeder(_id, graph.LinearAlgebraProvider, memory);

            var inputChannel = graph.Build(inputSize, this).AddFeedForward(hiddenLayerSize);
            var memoryChannel = graph.Build(hiddenLayerSize, _memoryFeeder.MemoryInput).AddFeedForward(hiddenLayerSize);

            _output = graph.Add(inputChannel, memoryChannel)
                .Add(activation)
                .Add(_memoryFeeder.SetMemoryAction)
                .Build()
            ;
        }

        public override List<IWire> Output => _output.Output;

        public override void SetPrimaryInput(IContext context)
        {
            // fire the memory channel
            if (context.BatchSequence.SequenceIndex == 0)
                _memoryFeeder.OnStart(context);
            else
                _memoryFeeder.OnNext(context);


        }

        //public IMatrix Execute(IMatrix input, int channel, IContext context)
        //{
        //    // send the hidden state into the memory channel
        //    if (context.Batch.CurrentSequence.SequenceIndex == 0)
        //        _memory.Send(_memoryFeeder.OnStart(context), _memoryFeeder.Channel, context);
        //    else
        //        _memory.Send(_memoryFeeder.OnNext(context), _memoryFeeder.Channel, context);

        //    // activate the input channel
        //    _input.Send(input, channel, context);

        //    // retrieve the output
        //    var ret = context.ExecutionContext.GetMemory(_memoryFeeder.Channel);
        //    return ret;
        //}

        //public IMatrix Train(IMatrix input, int channel, IBatchContext context)
        //{
        //    return Execute(input, channel, context);
        //}
    }
}
