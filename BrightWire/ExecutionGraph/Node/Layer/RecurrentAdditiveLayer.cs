using System;
using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Implementation of Recurrent Additive Network
    /// http://www.kentonl.com/pub/llz.2017.pdf
    /// </summary>
    internal class RecurrentAdditiveLayer : NodeBase, IHaveMemoryNode
    {
        uint _inputSize;
        MemoryFeeder _memory;
        NodeBase _input, _output;
        OneToMany _start;

#pragma warning disable 8618
        public RecurrentAdditiveLayer(GraphFactory graph, uint inputSize, float[] memory, string? name = null) : base(name)
#pragma warning restore 8618
        {
            Create(graph, inputSize, memory, null);
        }

        void Create(GraphFactory graph, uint inputSize, float[] memory, string? memoryId)
        {
            var hiddenLayerSize = (uint)memory.Length;
            _inputSize = inputSize;

            _memory = new MemoryFeeder(graph.Context, memory, Name ?? Id, null, memoryId);
            _input = new FlowThrough();

            var wx = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wx").LastNode!;
            var wi = graph.Connect(hiddenLayerSize, wx).AddFeedForward(hiddenLayerSize, "Wi");
            var ui = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Ui");
            var wf = graph.Connect(hiddenLayerSize, wx).AddFeedForward(hiddenLayerSize, "Wf");
            var uf = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uf");
            var it = graph.Add(wi, ui).Add(graph.SigmoidActivation());
            var ft = graph.Add(wf, uf).Add(graph.SigmoidActivation());

            _output = graph
                .Add(graph.Multiply(hiddenLayerSize, wx, it.LastNode!), graph.Multiply(hiddenLayerSize, _memory, ft.LastNode!))
                .AddForwardAction(_memory.SetMemoryAction, Name != null ? $"{Name}_last" : null)
                .LastNode!
            ;
            _start = new OneToMany(SubNodes, Name != null ? $"{Name}_start" : null);
        }

        /// <summary>
        /// Expose the output node so that we can append nodes to it
        /// </summary>
        public override List<WireToNode> Output => _output.Output;

        public NodeBase Memory => _memory;

        /// <summary>
        /// The list of sub nodes
        /// </summary>
        public override IEnumerable<NodeBase> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memory;
            }
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source) => _start.ForwardSingleStep(signal, channel, context, source);

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("RAN", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.WriteTo(writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = ["Wx", "Wi", "Wf", "Ui", "Uf"];

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
