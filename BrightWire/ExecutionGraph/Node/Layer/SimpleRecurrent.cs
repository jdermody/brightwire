using System;
using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Node.Action;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Simple recurrent neural network
    /// </summary>
    internal class SimpleRecurrent : NodeBase, IHaveMemoryNode
    {
        MemoryFeeder _memory;
        INode _input, _activation, _output;
        OneToMany _start;
        uint _inputSize;

#pragma warning disable 8618
        public SimpleRecurrent(GraphFactory graph, uint inputSize, float[] memory, INode activation, string? name = null)
#pragma warning restore 8618
            : base(name)
        {
            Create(graph, inputSize, memory, activation, null);
        }

        void Create(GraphFactory graph, uint inputSize, float[] memory, INode activation, string? memoryId)
        {
            _inputSize = inputSize;
            _activation = activation;
            var hiddenLayerSize = (uint)memory.Length;
            _memory = new MemoryFeeder(graph.Context, memory, null, memoryId);
            _input = new FlowThrough();

            var inputChannel = graph.Connect(inputSize, _input)
                .AddFeedForward(hiddenLayerSize, "Wh");
            var memoryChannel = graph.Connect(hiddenLayerSize, _memory)
                .AddFeedForward(hiddenLayerSize, "Uh");

            _output = graph.Add(inputChannel, memoryChannel)
                .AddBackwardAction(new ConstrainSignal())
                .Add(activation)
                .AddForwardAction(_memory.SetMemoryAction)
                .LastNode!
			;
            _start = new OneToMany(SubNodes);
        }

        public override List<IWire> Output => _output.Output;
        public INode Memory => _memory;

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            _start.ExecuteForward(context);
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source) => _start.Forward(signal, channel, context, source);

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("SRN", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.Data.WriteTo(writer);
            Serialise(_activation, writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = {"Wh", "Uh"};

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = (uint)reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memory = factory.Context.ReadVectorFrom(reader);
            var activation = Hydrate(factory, reader);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_memory == null)
                Create(factory, inputSize, memory.Segment.ToArray(), activation, memoryId);
            else
                _memory.Data = memory;

            foreach(var item in SerializedNodes)
                ReadSubNode(item, factory, reader);
        }

        public override IEnumerable<INode> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memory;
            }
        }
    }
}
