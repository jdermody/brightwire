using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph
{
    public class WireBuilder
    {
        readonly GraphFactory _factory;
        INode _node;
        int _size;

        public WireBuilder(GraphFactory factory, IGraphEngine engine)
            : this(factory, engine.DataSource.InputSize, null)
        {
            _node = new FlowThrough();
            engine.Add(_node);
        }

        public WireBuilder(GraphFactory factory, int size, INode node)
        {
            _factory = factory;
            _node = node;
            _size = size;
        }

        void _SetNode(INode node)
        {
            if (_node != null)
                _node.AddOutput(new WireToNode(node));
            _node = node;
        }

        public WireBuilder AddFeedForward(int outputSize)
        {
            INode node = _factory.CreateFeedForward(_size, outputSize);
            _SetNode(node);
            _size = outputSize;
            return this;
        }

        public WireBuilder Add(INode node)
        {
            _SetNode(node);
            return this;
        }

        public WireBuilder Add(IAction action)
        {
            _SetNode(new GraphExecuteAction(action));
            return this;
        }
    }
}
