using System;
using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.ExecutionGraph.Node.Action;

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
        INode _input, _output;
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
            _memory = new MemoryFeeder(graph.Context, memory, null, memoryId);
            _input = new FlowThrough();

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

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source) => _start.Forward(signal, channel, context, source);

        public override IEnumerable<INode> SubNodes
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
            _memory.Data.WriteTo(writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = {"Wh", "Wr", "Wz", "Uh", "Ur", "Uz"};

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
