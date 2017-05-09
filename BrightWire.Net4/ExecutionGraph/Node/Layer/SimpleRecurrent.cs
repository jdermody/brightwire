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
        readonly INode _output, _input;

        public SimpleRecurrent(GraphFactory graph, int inputSize, float[] memory, INode activation, string name = null)
            : base(name)
        {
            int hiddenLayerSize = memory.Length;
            _memoryFeeder = new MemoryFeeder(_id, graph.LinearAlgebraProvider, memory);

            var inputChannel = graph.Build(inputSize, this).AddFeedForward(hiddenLayerSize);
            var memoryChannel = graph.Build(hiddenLayerSize, _memoryFeeder.MemoryInput).AddFeedForward(hiddenLayerSize);

            _input = inputChannel.Build();
            _output = graph.Add(inputChannel, memoryChannel)
                .Add(activation)
                .Add(_memoryFeeder.SetMemoryAction)
                .Build()
            ;
        }

        public override List<IWire> Output => _output?.Output ?? base.Output;

        public override void SetPrimaryInput(IContext context)
        {
            // fire the memory channel
            if (context.BatchSequence.SequenceIndex == 1)
                _memoryFeeder.OnStart(context);
            else
                _memoryFeeder.OnNext(context);

            // fire the input channel
            _input.ExecuteForward(context, 0);
        }
    }
}
