using System;
using System.Collections.Generic;
using System.IO;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    internal class NodeWrapper(NodeBase node, string? name = null) : NodeBase(name)
    {
        class ContextProxy(IGraphContext context, NodeBase wrapper) : IGraphContext
        {
            public IGraphData Data
            {
                get =>context.Data;
                set => context.Data = value;
            }
            public GraphExecutionContext ExecutionContext => context.ExecutionContext;
            public ILearningContext? LearningContext => context.LearningContext;
            public MiniBatch.Sequence BatchSequence => context.BatchSequence;
            public IGraphData? ErrorSignal => context.ErrorSignal;

            public void AddForwardHistory(NodeBase source, IGraphData data, Func<IBackpropagate>? callback, params NodeBase[] prev)
            {
                context.AddForwardHistory(wrapper, data, callback, prev);
            }

            public IGraphData? Backpropagate(IGraphData? delta) => context.Backpropagate(delta);

            public void Dispose()
            {
            }

            public void SetOutput(IGraphData data, int channel = 0)
	        {
		        context.SetOutput(data, channel);
	        }

	        public IGraphData? GetOutput(int channel = 0)
	        {
		        return context.GetOutput(channel);
	        }

	        public IGraphData[] Output => context.Output;
            public IEnumerable<ExecutionResult> Results => context.Results;
            public void ClearForBackpropagation() => context.ClearForBackpropagation();
            public void SetData(string name, string type, IGraphData data) => context.SetData(name, type, data);
            public IEnumerable<(string Name, IGraphData Data)> GetData(string type) => context.GetData(type);
        }

        string _nodeId = node.Id;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            return node.ForwardSingleStep(signal, channel, context, source);
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
            node = graph[_nodeId];
        }
    }
}
