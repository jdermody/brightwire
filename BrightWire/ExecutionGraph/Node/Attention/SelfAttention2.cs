using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Layer;

namespace BrightWire.ExecutionGraph.Node.Attention
{
    internal class SelfAttention2 : NodeBase
    {
        readonly GetAttentionInput _start;
        readonly FeedForward _key, _query, _value;

        public SelfAttention2(
            GraphFactory graph, 
            uint inputSize, 
            uint encoderSize, 
            uint decoderSize,
            uint attentionSize,
            string? encoderName, 
            string? decoderName, 
            string? name, 
            string? id = null
            ) : base(name, id)
        {
            _start = new GetAttentionInput(graph.LinearAlgebraProvider, inputSize, encoderSize, decoderSize, encoderName, decoderName, $"{Name}_input");
            _key = (FeedForward)graph.CreateFeedForward(_start.BlockSize, attentionSize, $"{Name}_key");
            _query = (FeedForward)graph.CreateFeedForward(_start.BlockSize, attentionSize, $"{Name}_key");
            _value = (FeedForward)graph.CreateFeedForward(_start.BlockSize, _start.BlockSize, $"{Name}_key");
            Create(graph);
        }

        void Create(GraphFactory graph)
        {
            var graphStart = graph.Connect(_start.BlockSize, _start);
            graphStart.Add(_key);
            graphStart.Add(_query);
            graphStart.Add(_value);
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            return _start.ForwardSingleStep(signal, channel, context, source);
        }

        public override IEnumerable<NodeBase> SubNodes
        {
            get
            {
                yield return _start;
            }
        }

        public override List<WireToNode> Output => _start.Output;

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("SA", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            _start.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _start.ReadFrom(factory, reader);
            Create(factory);
        }
    }
}
