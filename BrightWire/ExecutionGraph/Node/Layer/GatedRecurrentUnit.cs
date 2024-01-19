using System;
using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// GRU recurrent neural network
    /// https://en.wikipedia.org/wiki/Gated_recurrent_unit
    /// </summary>
    internal class GatedRecurrentUnit : NodeBase, IHaveMemoryNode
    {
        uint _inputSize;
        MemoryFeeder _memory;
        NodeBase _input, _output;
        OneToMany _start;

#pragma warning disable 8618
        public GatedRecurrentUnit(GraphFactory graph, uint inputSize, float[] memory, string? name = null)
#pragma warning restore 8618
            : base(name)
        {
            Create(graph, inputSize, memory, null);
        }

        void Create(GraphFactory graph, uint inputSize, float[] memory, string? memoryId)
        {
            _inputSize = inputSize;
            var hiddenLayerSize = (uint)memory.Length;
            _memory = new MemoryFeeder(graph.Context, memory, Name ?? Id, null, memoryId);
            _input = new FlowThrough(Name != null ? $"{Name}_start" : null);

            var wz = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wz");
            var uz = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uz");

            var wr = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wr");
            var ur = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Ur");

            // add sigmoids to the gates
            var rt = graph.Add(wr, ur).AddBackwardAction(new ConstrainSignal()).Add(graph.SigmoidActivation("Rt")).LastNode!;
            var zt = graph.Add(wz, uz).AddBackwardAction(new ConstrainSignal()).Add(graph.SigmoidActivation("Zt")).LastNode!;

            // h1 = tanh(Wh(x) + Uh(Ht1xRt))
            var wh = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var uh = graph.Multiply(hiddenLayerSize, rt, _memory).AddFeedForward(hiddenLayerSize, "Uh");
            var h1 = graph.Add(wh, uh).AddBackwardAction(new ConstrainSignal()).Add(graph.TanhActivation());

            // h2 = h1x(1-Zt)
            var h2 = graph.Multiply(h1, graph.Connect(hiddenLayerSize, zt).Add(graph.GraphOperation.OneMinusInput()));

            // h = h1xh2
            var previous = graph.Multiply(hiddenLayerSize, zt, _memory);
            _output = graph
                .Add(h2, previous)
                .AddForwardAction(_memory.SetMemoryAction, Name != null ? $"{Name}_last" : null)
                .LastNode!
            ;
            _start = new OneToMany(SubNodes);
        }

        public override List<WireToNode> Output => _output.Output;
        public NodeBase Memory => _memory;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source) => _start.ForwardSingleStep(signal, channel, context, source);

        public override IEnumerable<NodeBase> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memory;
            }
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("GRU", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.WriteTo(writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = ["Wh", "Wr", "Wz", "Uh", "Ur", "Uz"];

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = (uint)reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memoryData = factory.Context.LoadReadOnlyVectorAndThenGetArrayFrom(reader);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_memory == null)
                Create(factory, inputSize, memoryData, memoryId);
            else
                _memory.Data = memoryData;

            foreach(var item in SerializedNodes)
                ReadSubNode(item, factory, reader);
        }
    }
}
