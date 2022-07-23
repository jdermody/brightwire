using System;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    class Lstm : NodeBase, IHaveMemoryNode
    {
        uint _inputSize;
        FlowThrough _input;
        MemoryFeeder _memory, _previous;
        NodeBase _output;
        OneToMany _start;

#pragma warning disable 8618
        public Lstm(GraphFactory graph, uint inputSize, float[] memory, string? name) : base(name)
#pragma warning restore 8618
        {
            Create(graph, inputSize, memory, null);
        }

        void Create(GraphFactory graph, uint inputSize, float[] memory, string? memoryId)
        {
            _inputSize = inputSize;
            var hiddenLayerSize = (uint)memory.Length;

            _input = new FlowThrough(Name != null ? $"{Name}_start" : null);
            _memory = new MemoryFeeder(graph.Context, memory, Name ?? Id, null, memoryId);
            _previous = new MemoryFeeder(graph.Context, new float[hiddenLayerSize], null);

            var combined = graph.Join(graph.Connect(inputSize, _input), graph.Connect(hiddenLayerSize, _previous), "combined").LastNode!;
            var ft = graph.Connect(hiddenLayerSize + inputSize, combined).AddFeedForward(hiddenLayerSize, "Wf").Add(graph.SigmoidActivation("sigmoid(Wf)"));
            var it = graph.Connect(hiddenLayerSize + inputSize, combined).AddFeedForward(hiddenLayerSize, "Wi").Add(graph.SigmoidActivation("sigmoid(Wi)"));
            var ct = graph.Connect(hiddenLayerSize + inputSize, combined).AddFeedForward(hiddenLayerSize, "Wc").Add(graph.TanhActivation("tanh(Wc)"));
            var ot = graph.Connect(hiddenLayerSize + inputSize, combined).AddFeedForward(hiddenLayerSize, "Wo").Add(graph.SigmoidActivation("sigmoid(Wo)"));

            var forget = graph.Multiply(ft, graph.Connect(hiddenLayerSize, _memory), "forget");
            var input = graph.Multiply(it, ct, "input");
            var hidden = graph
                .Add(forget, input)
                .AddForwardAction(_memory.SetMemoryAction)
                .Add(graph.TanhActivation())
            ;

            _output = graph
                .Multiply(hidden, ot, "output")
                .AddForwardAction(_previous.SetMemoryAction, Name != null ? $"{Name}_last" : null)
                .LastNode!;
            _start = new OneToMany(SubNodes, Name != null ? $"{Name}_start" : null);
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            return _start.ForwardSingleStep(signal, channel, context, source);
        }
        
        public override List<WireToNode> Output => _output.Output;

        public override IEnumerable<NodeBase> SubNodes
        {
            get
            {
                yield return _input;
                yield return _previous;
                yield return _memory;
            }
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("LSTM", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.WriteTo(writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = {"Wf", "Wi", "Wo", "Wc"};

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = (uint)reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memoryData = factory.Context.ReadVectorAndThenGetArrayFrom(reader);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_memory == null)
                Create(factory, inputSize, memoryData, memoryId);
            else
                _memory.Data = memoryData;

            foreach(var item in SerializedNodes)
                ReadSubNode(item, factory, reader);
        }

        public NodeBase Memory => _memory;
    }
}
