using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.ExecutionGraph.Action;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Simple recurrent neural network
    /// </summary>
    internal class SimpleRecurrent : NodeBase, IHaveMemoryNode
    {
        IReadOnlyDictionary<INode, IGraphData> _lastBackpropagation = null;
        MemoryFeeder _memory;
        INode _input, _output = null, _activation;
        OneToMany _start;
        uint _inputSize;

        public SimpleRecurrent(GraphFactory graph, uint inputSize, float[] memory, INode activation, string name = null)
            : base(name)
        {
            _Create(graph, inputSize, memory, activation, null);
        }

        void _Create(GraphFactory graph, uint inputSize, float[] memory, INode activation, string memoryId)
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
                //.Add(new HookErrorSignal(context => {
                //    if (_lastBackpropagation != null) {
                //        foreach (var item in _lastBackpropagation)
                //            context.AppendErrorSignal(item.Value, item.Key);
                //        _lastBackpropagation = null;
                //    }
                //}))
                .LastNode
			;
            _start = new OneToMany(SubNodes, bp => _lastBackpropagation = bp);
        }

        public override List<IWire> Output => _output.Output;
        public INode Memory => _memory;

        public override void ExecuteForward(IGraphContext context)
        {
            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart)
                _lastBackpropagation = null;

            _start.ExecuteForward(context);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("SRN", _WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wh = (FeedForward)_input.FindByName("Wh");
            var Uh = (FeedForward)_memory.FindByName("Uh");

            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.Data.WriteTo(writer);
            _Serialise(_activation, writer);

            Wh.WriteTo(writer);
            Uh.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = (uint)reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memory = factory.Context.ReadVectorFrom(reader);
            var activation = _Hydrate(factory, reader);

            if (_memory == null)
                _Create(factory, inputSize, memory.Segment.ToArray(), activation, memoryId);
            else
                _memory.Data = memory;

            var Wh = _input.FindByName("Wh");
            var Uh = _memory.FindByName("Uh");

            Wh.ReadFrom(factory, reader);
            Uh.ReadFrom(factory, reader);
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
