using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightWire.Models;
using BrightWire.ExecutionGraph.Action;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Elman or Joran style recurrent neural network
    /// https://en.wikipedia.org/wiki/Recurrent_neural_network#Elman_networks_and_Jordan_networks
    /// </summary>
    class ElmanJordan : NodeBase, IHaveMemoryNode
    {
        IReadOnlyDictionary<INode, IGraphData> _lastBackpropagation = null;
        MemoryFeeder _memory;
        INode _input, _output = null, _activation, _activation2;
        OneToMany _start;
        int _inputSize;
        bool _isElman;

        public ElmanJordan(GraphFactory graph, bool isElman, int inputSize, float[] memory, INode activation, INode activation2, string name = null)
            : base(name)
        {
            _Create(graph, isElman, inputSize, memory, activation, activation2, null);
        }

        void _Create(GraphFactory graph, bool isElman, int inputSize, float[] memory, INode activation, INode activation2, string memoryName)
        {
            _isElman = isElman;
            _inputSize = inputSize;
            _activation = activation;
            _activation2 = activation2;
            int hiddenLayerSize = memory.Length;
            _memory = new MemoryFeeder(memory, null, memoryName);
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

            _output = h
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

        public override void ExecuteForward(IContext context)
        {
            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart)
                _lastBackpropagation = null;

            _start.ExecuteForward(context);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return (_isElman ? "ERN" : "JRN", _WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wh = (FeedForward)_input.FindByName("Wh");
            var Wy = (FeedForward)_input.FindByName("Wy");
            var Uh = (FeedForward)_memory.FindByName("Uh");

            writer.Write(_isElman);
            writer.Write(_inputSize);
            writer.Write(_memory.Id);
            _memory.Data.WriteTo(writer);
            _Serialise(_activation, writer);
            _Serialise(_activation2, writer);

            Wh.WriteTo(writer);
            Wy.WriteTo(writer);
            Uh.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var isElman = reader.ReadBoolean();
            var inputSize = reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memory = FloatVector.ReadFrom(reader);
            var activation = _Hydrate(factory, reader);
            var activation2 = _Hydrate(factory, reader);

            if (_memory == null)
                _Create(factory, isElman, inputSize, memory.Data, activation, activation2, memoryId);
            else
                _memory.Data = memory;

            var Wh = _input.FindByName("Wh");
            var Wy = _input.FindByName("Wy");
            var Uh = _memory.FindByName("Uh");

            Wh.ReadFrom(factory, reader);
            Wy.ReadFrom(factory, reader);
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
