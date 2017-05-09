using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    internal class SimpleRecurrent : NodeBase
    {
        readonly MemoryFeeder _memoryFeeder;
        readonly INode _input, _output = null;

        public SimpleRecurrent(GraphFactory graph, int inputSize, float[] memory, INode activation, string name = null)
            : base(name)
        {
            int hiddenLayerSize = memory.Length;
            _memoryFeeder = new MemoryFeeder(memory);
            _input = new FlowThrough();

            var inputChannel = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var memoryChannel = graph.Build(hiddenLayerSize, _memoryFeeder.MemoryInput).AddFeedForward(hiddenLayerSize, "Uh");

            _output = graph.Add(inputChannel, memoryChannel)
                .Add(activation)
                .Add(_memoryFeeder.SetMemoryAction)
                .Build()
            ;
        }

        public override List<IWire> Output => _output.Output;

        public override void ExecuteForward(IContext context)
        {
            foreach (var node in SubNodes)
                node.ExecuteForward(context, 0);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("RN", _WriteData(WriteTo));
        }

        protected override void _Initalise(byte[] data)
        {
            _ReadFrom(data, ReadFrom);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wh = SearchFor("Wh") as FeedForward;
            var Uh = SearchFor("Uh") as FeedForward;

            _memoryFeeder.Data.WriteTo(writer);
            Wh.WriteTo(writer);
            Uh.WriteTo(writer);
        }

        public override void ReadFrom(BinaryReader reader)
        {
            var Wh = SearchFor("Wh") as FeedForward;
            var Uh = SearchFor("Uh") as FeedForward;

            _memoryFeeder.Data = FloatArray.ReadFrom(reader);
            Wh.ReadFrom(reader);
            Uh.ReadFrom(reader);
        }

        public override IEnumerable<INode> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memoryFeeder;
            }
        }
    }
}
