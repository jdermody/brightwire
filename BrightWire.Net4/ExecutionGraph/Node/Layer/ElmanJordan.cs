using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BrightWire.Models;
using ProtoBuf;
using BrightWire.ExecutionGraph.Node.Helper;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    internal class ElmanJordan : NodeBase
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
            _Create(graph, isElman, inputSize, memory, activation, activation2);
        }

        void _Create(GraphFactory graph, bool isElman, int inputSize, float[] memory, INode activation, INode activation2)
        {
            _isElman = isElman;
            _inputSize = inputSize;
            _activation = activation;
            _activation2 = activation2;
            int hiddenLayerSize = memory.Length;
            _memory = new MemoryFeeder(memory);
            _input = new FlowThrough();

            var inputChannel = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var memoryChannel = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uh");

            var h = graph.Add(inputChannel, memoryChannel).Add(activation);
            if (isElman)
                h = h.Add(_memory.SetMemoryAction);

            h = h.AddFeedForward(hiddenLayerSize, "Wy").Add(activation2);
            if (!isElman)
                h.Add(_memory.SetMemoryAction);

            _output = h
                .Add(new RestoreErrorSignal(context => {
                    if (_lastBackpropagation != null) {
                        foreach (var item in _lastBackpropagation)
                            context.AppendErrorSignal(item.Value, item.Key);
                    }
                    _lastBackpropagation = null;
                }))
                .Build();

            _start = new OneToMany(SubNodes, bp => _lastBackpropagation = bp);
        }

        public override List<IWire> Output => _output.Output;

        public override void ExecuteForward(IContext context)
        {
            _start.ExecuteForward(context);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return (_isElman ? "ERN" : "JRN", _WriteData(WriteTo));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wh = _input.SearchFor("Wh") as FeedForward;
            var Wy = _input.SearchFor("Wy") as FeedForward;
            var Uh = _memory.SearchFor("Uh") as FeedForward;

            writer.Write(_isElman);
            writer.Write(_inputSize);
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
            var memory = FloatVector.ReadFrom(reader);
            var activation = _Hydrate(factory, reader);
            var activation2 = _Hydrate(factory, reader);

            if (_memory == null)
                _Create(factory, isElman, inputSize, memory.Data, activation, activation2);
            else
                _memory.Data = memory;

            var Wh = _input.SearchFor("Wh") as INode;
            var Wy = _input.SearchFor("Wy") as INode;
            var Uh = _memory.SearchFor("Uh") as INode;

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
