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

        public WireBuilder(GraphFactory factory, int size, INode node)
        {
            _factory = factory;
            _node = node;
            _size = size;
        }

        public WireBuilder(GraphFactory factory, IGraphEngine engine) 
            : this(factory, engine.DataSource.InputSize, engine.Input)
        {
        }

        void _SetNode(INode node)
        {
            if (_node != null)
                _node.Output.Add(new WireToNode(node));
            _node = node;
        }

        public WireBuilder AddFeedForward(int outputSize, string name = null)
        {
            INode node = _factory.CreateFeedForward(_size, outputSize, name);
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
            _SetNode(new ExecuteAction(action));
            return this;
        }

        public WireBuilder AddSimpleRecurrent(INode activation, float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateSimpleRecurrent(_size, initialMemory, activation, name));
            _size = initialMemory.Length;
            return this;
        }

        public WireBuilder AddElman(INode activation, INode activation2, float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateElman(_size, initialMemory, activation, activation2, name));
            _size = initialMemory.Length;
            return this;
        }

        public WireBuilder AddJordan(INode activation, INode activation2, float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateJordan(_size, initialMemory, activation, activation2, name));
            _size = initialMemory.Length;
            return this;
        }

        public WireBuilder AddGru(float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateGru(_size, initialMemory, name));
            _size = initialMemory.Length;
            return this;
        }

        public WireBuilder AddLstm(float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateLstm(_size, initialMemory, name));
            _size = initialMemory.Length;
            return this;
        }

        public WireBuilder AddMaxPooling(int width, int height, int stride, string name = null)
        {
            _SetNode(_factory.CreateMaxPool(width, height, stride, name));
            return this;
        }

        public WireBuilder ReverseSequence(string name = null)
        {
            _SetNode(_factory.CreateSequenceReverser(name));
            return this;
        }

        public WireBuilder AddConvolutional(int inputDepth, int filterCount, int padding, int filterWidth, int filterHeight, int stride, string name = null)
        {
            _SetNode(_factory.CreateConvolutional(inputDepth, filterCount, padding, filterWidth, filterHeight, stride, name));
            // work out the new size here
            return this;
        }

        public INode Build() => _node;

        public INode LastNode => _node;
        public int CurrentSize => _size;
    }
}
