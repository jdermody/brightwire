using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.ExecutionGraph.Action;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Elman or Joran style recurrent neural network
    /// https://en.wikipedia.org/wiki/Recurrent_neural_network#Elman_networks_and_Jordan_networks
    /// </summary>
    internal class ElmanJordan : NodeBase, IHaveMemoryNode
    {
        MemoryFeeder _memory;
        INode _input, _activation, _activation2, _output;
        OneToMany _start;
        uint _inputSize;
        bool _isElman;

#pragma warning disable 8618
        public ElmanJordan(GraphFactory graph, bool isElman, uint inputSize, float[] memory, INode activation, INode activation2, string? name = null)
#pragma warning restore 8618
            : base(name)
        {
            Create(graph, isElman, inputSize, memory, activation, activation2, null);
        }

        void Create(GraphFactory graph, bool isElman, uint inputSize, float[] memory, INode activation, INode activation2, string? memoryName)
        {
            _isElman = isElman;
            _inputSize = inputSize;
            _activation = activation;
            _activation2 = activation2;
            var hiddenLayerSize = (uint)memory.Length;
            _memory = new MemoryFeeder(graph.Context, memory, null, memoryName);
            _input = new FlowThrough();

            var inputChannel = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var memoryChannel = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uh");

            var h = graph.Add(inputChannel, memoryChannel)
                .AddBackwardAction(new ConstrainSignal())
                .Add(activation)
            ;
            if (isElman)
                h = h.AddForwardAction(_memory.SetMemoryAction);

            h = h.AddFeedForward(hiddenLayerSize, "Wy")
                .AddBackwardAction(new ConstrainSignal())
                .Add(activation2)
            ;
            if (!isElman)
                h.AddForwardAction(_memory.SetMemoryAction);

            _output = h.LastNode!;
            _start = new OneToMany(SubNodes);
        }

        public override List<IWire> Output => _output.Output;
        public INode Memory => _memory;

        public override void ExecuteForward(IGraphContext context)
        {
            _start.ExecuteForward(context);
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return (_isElman ? "ERN" : "JRN", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_isElman);
            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.Data.WriteTo(writer);
            Serialise(_activation, writer);
            Serialise(_activation2, writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = {"Wh", "Wy", "Uh"};

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var isElman = reader.ReadBoolean();
            var inputSize = (uint)reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memory = factory.Context.ReadVectorFrom(reader);
            var activation = Hydrate(factory, reader);
            var activation2 = Hydrate(factory, reader);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_memory == null)
                Create(factory, isElman, inputSize, memory.Segment.ToArray(), activation, activation2, memoryId);
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
