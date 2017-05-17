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
using BrightWire.ExecutionGraph.Action;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    internal class SimpleRecurrent : NodeBase, IHaveMemoryNode
    {
        IReadOnlyDictionary<INode, IGraphData> _lastBackpropagation = null;
        MemoryFeeder _memory;
        INode _input, _output = null, _activation;
        OneToMany _start;
        int _inputSize;

        public SimpleRecurrent(GraphFactory graph, int inputSize, float[] memory, INode activation, string name = null)
            : base(name)
        {
            _Create(graph, inputSize, memory, activation, null);
        }

        void _Create(GraphFactory graph, int inputSize, float[] memory, INode activation, string memoryId)
        {
            _inputSize = inputSize;
            _activation = activation;
            int hiddenLayerSize = memory.Length;
            _memory = new MemoryFeeder(memory, null, memoryId);
            _input = new FlowThrough();

            var inputChannel = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var memoryChannel = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uh");

            _output = graph.Add(inputChannel, memoryChannel)
                .AddBackwardAction(new ConstrainErrorSignal())
                .Add(activation)
                .AddForwardAction(_memory.SetMemoryAction)
                .Add(new RestoreErrorSignal(context => {
                    if(_lastBackpropagation != null) {
                        foreach(var item in _lastBackpropagation)
                            context.AppendErrorSignal(item.Value, item.Key);
                        _lastBackpropagation = null;
                    }
                }))
                .Build()
            ;
            _start = new OneToMany(SubNodes, bp => _lastBackpropagation = bp);
        }

        public override List<IWire> Output => _output.Output;
        public INode Memory => _memory;

        public override void ExecuteForward(IContext context)
        {
            _start.ExecuteForward(context);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("SRN", _WriteData(WriteTo));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wh = _input.SearchFor("Wh") as FeedForward;
            var Uh = _memory.SearchFor("Uh") as FeedForward;

            writer.Write(_inputSize);
            writer.Write(_memory.Id);
            _memory.Data.WriteTo(writer);
            _Serialise(_activation, writer);

            Wh.WriteTo(writer);
            Uh.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memory = FloatVector.ReadFrom(reader);
            var activation = _Hydrate(factory, reader);

            if (_memory == null)
                _Create(factory, inputSize, memory.Data, activation, memoryId);
            else
                _memory.Data = memory;

            var Wh = _input.SearchFor("Wh") as INode;
            var Uh = _memory.SearchFor("Uh") as INode;

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
