using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Helper;

namespace BrightWire.ExecutionGraph
{
    /// <summary>
    /// Wires nodes together to build strands of a graph
    /// </summary>
    public class WireBuilder
    {
        readonly GraphFactory _factory;
        readonly INode _first;
        INode _node;
        int _width, _height, _depth;

        /// <summary>
        /// Connects new nodes starting from the specified node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="size">Initial wire size</param>
        /// <param name="node">The node to build from</param>
        public WireBuilder(GraphFactory factory, int size, INode node)
        {
            _factory = factory;
            _first = _node = node;
            _width = size;
            _height = 1;
            _depth = 1;
        }

        /// <summary>
        /// Connects new nodes starting from the specified node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="width">Initial input width</param>
        /// <param name="height">Initial input height</param>
        /// <param name="depth">Initial input depth</param>
        /// <param name="node">The node to build from</param>
        public WireBuilder(GraphFactory factory, int width, int height, int depth, INode node)
        {
            _factory = factory;
            _first = _node = node;
            _width = width;
            _height = height;
            _depth = depth;
        }

        /// <summary>
        /// Connects new nodes to the engine output node
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="engine"></param>
        public WireBuilder(GraphFactory factory, IGraphEngine engine) 
            : this(factory, engine.DataSource.InputSize, engine.Input)
        {
            if(engine.DataSource is IVolumeDataSource volumeDataSource) {
                _width = volumeDataSource.Width;
                _height = volumeDataSource.Height;
                _depth = volumeDataSource.Depth;
            }
        }

        /// <summary>
        /// The current wire size
        /// </summary>
        public int CurrentSize => _width * _height * _depth;

        void _SetNewSize(int newSize)
        {
            _width = newSize;
            _height = 1;
            _depth = 1;
        }

        /// <summary>
        /// Changes the current wire's input size
        /// </summary>
        /// <param name="delta">Amount to add to the current wire size</param>
        /// <returns></returns>
        public WireBuilder IncrementSizeBy(int delta)
        {
            _SetNewSize(_width + delta);
            return this;
        }

        void _SetNode(INode node)
        {
            if (_node != null)
                _node.Output.Add(new WireToNode(node));
            _node = node;
        }

        /// <summary>
        /// Connects a row classifier
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="dataTable"></param>
        /// <param name="analysis"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddClassifier(IRowClassifier classifier, IDataTable dataTable, IDataTableAnalysis analysis = null, string name = null)
        {
            var node = _factory.CreateClassifier(classifier, dataTable, analysis, name);
            _SetNode(node.RowClassifier);
            _SetNewSize(node.OutputSize);
            return this;
        }

        /// <summary>
        /// Adds a feed forward layer
        /// </summary>
        /// <param name="outputSize">Number of outgoing connections</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddFeedForward(int outputSize, string name = null)
        {
            INode node = _factory.CreateFeedForward(CurrentSize, outputSize, name);
            _SetNode(node);
            _SetNewSize(outputSize);
            return this;
        }

        /// <summary>
        /// Adds a feed forward layer whose weights are tied to a previous layer
        /// </summary>
        /// <param name="layer">The layer whose weights are tied</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddTiedFeedForward(IFeedForward layer, string name = null)
        {
            _SetNode(_factory.CreateTiedFeedForward(layer, name));
            _SetNewSize(layer.InputSize);
            return this;
        }

        /// <summary>
        /// Adds a drop out layer
        /// </summary>
        /// <param name="dropOutPercentage">Percentage of connections to drop</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddDropOut(float dropOutPercentage, string name = null)
        {
            _SetNode(_factory.CreateDropOut(dropOutPercentage, name));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dropOutPercentage">Percentage of connections to drop</param>
        /// <param name="outputSize">Number of outgoing connections</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddDropConnect(float dropOutPercentage, int outputSize, string name = null)
        {
            _SetNode(_factory.CreateDropConnect(dropOutPercentage, CurrentSize, outputSize, name));
            _SetNewSize(outputSize);
            return this;
        }

        /// <summary>
        /// Adds a node
        /// </summary>
        /// <returns></returns>
        public WireBuilder Add(INode node)
        {
            _SetNode(node);
            return this;
        }

        /// <summary>
        /// Adds an action that will be executed in the forward pass
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddForwardAction(IAction action, string name = null)
        {
            _SetNode(new ExecuteForwardAction(action, name));
            return this;
        }

        /// <summary>
        /// Adds an action that will be executed in the backward pass
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddBackwardAction(IAction action, string name = null)
        {
            _SetNode(new ExecuteBackwardAction(action, name));
            return this;
        }

        /// <summary>
        /// Adds a simple recurrent neural network layer
        /// </summary>
        /// <param name="activation">Activation layer</param>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddSimpleRecurrent(INode activation, float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateSimpleRecurrent(CurrentSize, initialMemory, activation, name));
            _SetNewSize(initialMemory.Length);
            return this;
        }

        /// <summary>
        /// Adds an Elman recurrent neural network layer
        /// </summary>
        /// <param name="activation">First activation layer</param>
        /// <param name="activation2">Second activation layer</param>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddElman(INode activation, INode activation2, float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateElman(CurrentSize, initialMemory, activation, activation2, name));
            _SetNewSize(initialMemory.Length);
            return this;
        }

        /// <summary>
        /// Adds a Jordan recurrent neural network layer
        /// </summary>
        /// <param name="activation">First activation layer</param>
        /// <param name="activation2">Second activation layer</param>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddJordan(INode activation, INode activation2, float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateJordan(CurrentSize, initialMemory, activation, activation2, name));
            _SetNewSize(initialMemory.Length);
            return this;
        }

        /// <summary>
        /// Adds a gated recurrent unit recurrent neural network layer
        /// </summary>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddGru(float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateGru(CurrentSize, initialMemory, name));
            _SetNewSize(initialMemory.Length);
            return this;
        }

        /// <summary>
        /// Adds a long short term memory recurrent neural network layer
        /// </summary>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddLstm(float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateLstm(CurrentSize, initialMemory, name));
            _SetNewSize(initialMemory.Length);
            return this;
        }

        /// <summary>
        /// Adds a node that will reverse the sequence (for bidirectional recurrent neural networks)
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder ReverseSequence(string name = null)
        {
            _SetNode(_factory.CreateSequenceReverser(name));
            return this;
        }

        /// <summary>
        /// Adds a max pooling convolutional layer
        /// </summary>
        /// <param name="filterWidth">Width of max pooliing filter</param>
        /// <param name="filterHeight">Height of max pooling filter</param>
        /// <param name="stride">Filter stride</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddMaxPooling(int filterWidth, int filterHeight, int stride, string name = null)
        {
            _SetNode(_factory.CreateMaxPool(filterWidth, filterHeight, stride, name));

            _width = (_width - filterWidth) / stride + 1;
            _height = (_height - filterHeight) / stride + 1;

            return this;
        }

        /// <summary>
        /// Adds a convolutional layer
        /// </summary>
        /// <param name="filterCount">Number of filters in the layer</param>
        /// <param name="padding">Padding to add before applying the convolutions</param>
        /// <param name="filterWidth">Width of each filter</param>
        /// <param name="filterHeight">Height of each filter</param>
        /// <param name="stride">Filter stride</param>
        /// <param name="shouldBackpropagate">True to calculate a backpropagation signal</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddConvolutional(int filterCount, int padding, int filterWidth, int filterHeight, int stride, bool shouldBackpropagate = true, string name = null)
        {
            _SetNode(_factory.CreateConvolutional(_depth, filterCount, padding, filterWidth, filterHeight, stride, shouldBackpropagate, name));

            _width = (_width + (2 * padding) - filterWidth) / stride + 1;
            _height = (_height + (2 * padding) - filterHeight) / stride + 1;
            _depth = filterCount;

            return this;
        }

        /// <summary>
        /// Transposes the graph signal to move between convolutional and non-convolutional layers
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Transpose(string name = null)
        {
            _SetNode(new TransposeSignal(name));
            return this;
        }

        /// <summary>
        /// Adds backpropagation - when executed an error signal will be calculated and flow backwards to previous nodes
        /// </summary>
        /// <param name="errorMetric">Error metric to calculate the error signal</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddBackpropagation(IErrorMetric errorMetric, string name = null)
        {
            AddForwardAction(new Backpropagate(errorMetric), name);
            return this;
        }

        /// <summary>
        /// Adds backpropagation through time
        /// </summary>
        /// <param name="errorMetric">Error metric to calculate the error signal</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddBackpropagationThroughTime(IErrorMetric errorMetric, string name = null)
        {
            AddForwardAction(new BackpropagateThroughTime(errorMetric), name);
            return this;
        }

        /// <summary>
        /// Constrains the error signal in the forward direction
        /// </summary>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder ConstrainForwardSignal(float min = -1f, float max = 1f, string name = null)
        {
            AddForwardAction(new ConstrainSignal(min, max), name);
            return this;
        }

        /// <summary>
        /// Constrains the error signal in the backward direction
        /// </summary>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder ConstrainBackwardSignal(float min = -1f, float max = 1f, string name = null)
        {
            AddBackwardAction(new ConstrainSignal(min, max), name);
            return this;
        }

        /// <summary>
        /// Writes node memory to a named memory slot
        /// </summary>
        /// <param name="slotName">Memory slot name</param>
        /// <param name="node">The node to read</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder WriteNodeMemoryToSlot(string slotName, IHaveMemoryNode node, string name = null)
        {
            AddForwardAction(new CopyNamedMemory(slotName, node), name);
            return this;
        }

        /// <summary>
        /// Concatenates the named memory slot with the input signal
        /// </summary>
        /// <param name="slotName">Memory slot name</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder JoinInputWithMemory(string slotName, string name = null)
        {
            AddForwardAction(new JoinInputWithMemory(slotName), name);
            return this;
        }

        /// <summary>
        /// Tries to find the specified node
        /// </summary>
        /// <param name="name">The friendly name of the node</param>
        /// <returns></returns>
        public INode Find(string name)
        {
            return _first.FindByName(name);
        }

        /// <summary>
        /// The last added node
        /// </summary>
        public INode LastNode => _node;
    }
}
