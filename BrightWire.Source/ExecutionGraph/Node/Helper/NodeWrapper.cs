using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    class NodeWrapper : NodeBase
    {
        class ContextProxy : IContext
        {
            readonly IContext _context;
            readonly INode _wrapper;

            public ContextProxy(IContext context, INode wrapper)
            {
                _context = context;
                _wrapper = wrapper;
            }

            public bool IsTraining => _context.IsTraining;
            public INode Source => _context.Source;
            public IGraphData Data => _context.Data;
            public IExecutionContext ExecutionContext => _context.ExecutionContext;
            public ILearningContext LearningContext => _context.LearningContext;
            public ILinearAlgebraProvider LinearAlgebraProvider => _context.LinearAlgebraProvider;
            public IMiniBatchSequence BatchSequence => _context.BatchSequence;
            public IGraphData ErrorSignal => _context.ErrorSignal;
            public bool HasNext => _context.HasNext;

            public void AddBackward(IGraphData errorSignal, INode target, INode source)
            {
                _context.AddBackward(errorSignal, target, source);
            }

            public void AddForward(IExecutionHistory action, Func<IBackpropagation> callback)
            {
                // TODO: wrap the backpropagation?
                _context.AddForward(new TrainingAction(_wrapper, action.Data, action.Source), callback);
            }

            public void AppendErrorSignal(IGraphData errorSignal, INode forNode)
            {
                _context.AppendErrorSignal(errorSignal, forNode);
            }

            public void Backpropagate(IGraphData delta)
            {
                _context.Backpropagate(delta);
            }

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

	        public IGraphData GetOutput(int channel = 0)
	        {
		        return _context.GetOutput(channel);
	        }

	        public IReadOnlyList<IGraphData> Output => _context.Output;
        }
        INode _node;
        string _nodeId;

        public NodeWrapper(INode node, string name = null) : base(name)
        {
            _node = node;
            _nodeId = node.Id;
        }

        public override void ExecuteForward(IContext context)
        {
            _node.ExecuteForward(new ContextProxy(context, this), 0);
        }

        protected override void _ExecuteForward(IContext context, int channel)
        {
            _node.ExecuteForward(new ContextProxy(context, this), channel);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("WRAPPER", _WriteData(WriteTo));
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
