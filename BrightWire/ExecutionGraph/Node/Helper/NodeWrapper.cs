using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    internal class NodeWrapper : NodeBase
    {
        class ContextProxy : IGraphSequenceContext
        {
            readonly IGraphSequenceContext _context;
            readonly NodeBase _wrapper;

            public ContextProxy(IGraphSequenceContext context, NodeBase wrapper)
            {
                _context = context;
                _wrapper = wrapper;
            }

            public NodeBase? Source => _context.Source;

            public IGraphData Data
            {
                get =>_context.Data;
                set => _context.Data = value;
            }
            public IGraphExecutionContext ExecutionContext => _context.ExecutionContext;
            public ILearningContext? LearningContext => _context.LearningContext;
            public ILinearAlgebraProvider LinearAlgebraProvider => _context.LinearAlgebraProvider;
            public IMiniBatchSequence BatchSequence => _context.BatchSequence;
            public IGraphData? ErrorSignal => _context.ErrorSignal;

            public void AddForward(ExecutionHistory action, Func<IBackpropagate>? callback)
            {
                
                
            }

            public void AddForward(NodeBase source, IGraphData data, Func<IBackpropagate>? callback, params NodeBase[] prev)
            {
                // TODO: wrap the backpropagation?
                _context.AddForward(_wrapper, data, callback, prev);
            }

            public IGraphData? Backpropagate(IGraphData? delta) => _context.Backpropagate(delta);

            public void Dispose()
            {
            }

            public void SetOutput(IGraphData data, int channel = 0)
	        {
		        _context.SetOutput(data, channel);
	        }

	        public IGraphData? GetOutput(int channel = 0)
	        {
		        return _context.GetOutput(channel);
	        }

	        public IGraphData[] Output => _context.Output;
            public ExecutionResult Result => _context.Result;
        }
        NodeBase _node;
        string _nodeId;

        public NodeWrapper(NodeBase node, string? name = null) : base(name)
        {
            _node = node;
            _nodeId = node.Id;
        }

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardInternal(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            return _node.ForwardInternal(signal, channel, context, source);
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("WRAPPER", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_nodeId);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _nodeId = reader.ReadString();
        }

        public override void OnDeserialise(IReadOnlyDictionary<string, NodeBase> graph)
        {
            _node = graph[_nodeId];
        }
    }
}
