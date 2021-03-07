using System;
using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// LSTM recurrent neural network
    /// https://en.wikipedia.org/wiki/Long_short-term_memory
    /// </summary>
    internal class LongShortTermMemory : NodeBase, IHaveMemoryNode
    {
        uint _inputSize;
        MemoryFeeder _memory;
        NodeBase _input, _output;
        OneToMany _start;

#pragma warning disable 8618
        public LongShortTermMemory(GraphFactory graph, uint inputSize, float[] memory, string? name = null) : base(name)
#pragma warning restore 8618
        {
            Create(graph, inputSize, memory, null);
        }

        void Create(GraphFactory graph, uint inputSize, float[] memory, string? memoryId)
        {
            _inputSize = inputSize;
            var hiddenLayerSize = (uint)memory.Length;
            _memory = new MemoryFeeder(graph.Context, memory, Name ?? Id, null, memoryId);
            _input = new FlowThrough();

            var wf = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wf");
            var wi = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wi");
            var wo = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wo");
            var wc = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wc");

            var uf = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uf");
            var ui = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Ui");
            var uo = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uo");
            var uc = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uc");

            var ft = graph.Add(wf, uf).Add(graph.SigmoidActivation("Ft"));
            var it = graph.Add(wi, ui).Add(graph.SigmoidActivation("It"));
            var ot = graph.Add(wo, uo).Add(graph.SigmoidActivation("Ot"));

            var ftCt1 = graph.Multiply(hiddenLayerSize, ft.LastNode!, _memory);
            var ct = graph.Add(ftCt1, graph.Multiply(it, graph.Add(wc, uc).Add(graph.TanhActivation())))
                .AddForwardAction(_memory.SetMemoryAction)
            ;

            _output = graph.Multiply(ot, ct.Add(graph.TanhActivation()), Name != null ? $"{Name}_last" : null).LastNode!;
            _start = new OneToMany(SubNodes, Name != null ? $"{Name}_start" : null);
        }

        public override List<WireToNode> Output => _output.Output;
        public NodeBase Memory => _memory;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source) => _start.ForwardSingleStep(signal, channel, context, source);

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
            return ("LSTM", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.Data.WriteTo(writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = {"Wf", "Wi", "Wo", "Wc", "Uf", "Ui", "Uo", "Uc"};

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = (uint)reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memory = factory.Context.ReadVectorFrom(reader);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_memory == null)
                Create(factory, inputSize, memory.Segment.ToArray(), memoryId);
            else
                _memory.Data = memory;

            foreach(var item in SerializedNodes)
                ReadSubNode(item, factory, reader);
        }
    }
}
