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
            readonly INode _wrapper;

            public ContextProxy(IGraphSequenceContext context, INode wrapper)
            {
                _context = context;
                _wrapper = wrapper;
            }

            public INode? Source => _context.Source;

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
            public bool HasNext => _context.HasNext;

            public void AddForward(ExecutionHistory action, Func<IBackpropagate>? callback)
            {
                
                
            }

            public void AddForward(INode source, IGraphData data, Func<IBackpropagate>? callback, params INode[] prev)
            {
                // TODO: wrap the backpropagation?
                _context.AddForward(_wrapper, data, callback, prev);
            }

            public IGraphData? Backpropagate(INode source, IGraphData? delta) => _context.Backpropagate(source, delta);

            public void Dispose()
            {
            }

            public bool ExecuteNext()
            {
                return _context.ExecuteNext();
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
        INode _node;
        string _nodeId;

        public NodeWrapper(INode node, string? name = null) : base(name)
        {
            _node = node;
            _nodeId = node.Id;
        }

        public override void ExecuteForward(IGraphSequenceContext context)
        {
            _node.ExecuteForward(new ContextProxy(context, this), 0);
        }

        protected override void ExecuteForwardInternal(IGraphSequenceContext context, uint channel)
        {
            _node.ExecuteForward(new ContextProxy(context, this), channel);
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            return _node.Forward(signal, channel, context, source);
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

        public override void OnDeserialise(IReadOnlyDictionary<string, INode> graph)
        {
            _node = graph[_nodeId];
        }
    }
}
