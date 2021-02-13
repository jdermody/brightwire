using System;
using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData;

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
        INode _input, _output;
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

            _memory = new MemoryFeeder(graph.Context, memory, null, memoryId);
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
                .AddForwardAction(_memory.SetMemoryAction)
                .LastNode!
            ;
            _start = new OneToMany(SubNodes);
        }

        /// <summary>
        /// Expose the output node so that we can append nodes to it
        /// </summary>
        public override List<IWire> Output => _output.Output;

        public INode Memory => _memory;

        /// <summary>
        /// The list of sub nodes
        /// </summary>
        public override IEnumerable<INode> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memory;
            }
        }

        /// <summary>
        /// Execute on the the start node, which will execute each sub node in turn
        /// </summary>
        /// <param name="context"></param>
        public override void ExecuteForward(IGraphSequenceContext context)
        {
            _start.ExecuteForward(context);
        }

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source) => _start.Forward(signal, channel, context, source);

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("RAN", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.Data.WriteTo(writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = {"Wx", "Wi", "Wf", "Ui", "Uf"};

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
