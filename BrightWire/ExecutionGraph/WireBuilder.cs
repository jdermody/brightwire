using System;
using BrightData;
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
        uint _width, _height, _depth;

        /// <summary>
        /// Connects new nodes starting from the specified node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="size">Initial wire size</param>
        /// <param name="node">The node to build from</param>
        public WireBuilder(GraphFactory factory, uint size, INode node)
        {
            _factory = factory;
            _first = LastNode = node;
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
        public WireBuilder(GraphFactory factory, uint width, uint height, uint depth, INode node)
        {
            _factory = factory;
            _first = LastNode = node;
            _width = width;
            _height = height;
            _depth = depth;
        }

        /// <summary>
        /// Connects new nodes to the engine output node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="engine">Graph engine</param>
        /// <param name="inputIndex">Input index to connect</param>
        public WireBuilder(GraphFactory factory, IGraphTrainingEngine engine, uint inputIndex = 0) 
            : this(factory, engine.DataSource?.InputSize ?? throw new Exception("Engine has no data source"), engine.GetInput(inputIndex))
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
        public uint CurrentSize => _width * _height * _depth;

        /// <summary>
        /// Changes the current size of the builder
        /// </summary>
        /// <param name="newSize">New wire builder size</param>
        public WireBuilder SetNewSize(uint newSize)
        {
            _width = newSize;
            _height = 1;
            _depth = 1;
            return this;
        }

        void SetNode(INode node)
        {
	        LastNode?.Output.Add(new WireToNode(node));
	        LastNode = node;
        }

        /// <summary>
        /// Connects a row classifier
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="dataTable"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddClassifier(IRowClassifier classifier, IRowOrientedDataTable dataTable, string? name = null)
        {
            var node = _factory.CreateClassifier(classifier, dataTable, name);
            SetNode(node.RowClassifier);
            return SetNewSize(node.OutputSize);
        }

        /// <summary>
        /// Adds a feed forward layer
        /// </summary>
        /// <param name="outputSize">Number of outgoing connections</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddFeedForward(uint outputSize, string? name = null)
        {
            INode node = _factory.CreateFeedForward(CurrentSize, outputSize, name);
            SetNode(node);
            return SetNewSize(outputSize);
        }

        /// <summary>
        /// Adds a feed forward layer whose weights are tied to a previous layer
        /// </summary>
        /// <param name="layer">The layer whose weights are tied</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddTiedFeedForward(IFeedForward layer, string? name = null)
        {
            SetNode(_factory.CreateTiedFeedForward(layer, name));
            return SetNewSize(layer.InputSize);
        }

        /// <summary>
        /// Adds a drop out layer
        /// </summary>
        /// <param name="dropOutPercentage">Percentage of connections to drop</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddDropOut(float dropOutPercentage, string? name = null)
        {
            SetNode(_factory.CreateDropOut(dropOutPercentage, name));
            return this;
        }

        /// <summary>
        /// Adds a drop connect layer
        /// </summary>
        /// <param name="dropOutPercentage">Percentage of connections to drop</param>
        /// <param name="outputSize">Number of outgoing connections</param>
        /// <param name="name">Optional name to give the node</param>
        public WireBuilder AddDropConnect(float dropOutPercentage, uint outputSize, string? name = null)
        {
            SetNode(_factory.CreateDropConnect(dropOutPercentage, CurrentSize, outputSize, name));
            return SetNewSize(outputSize);
        }

		/// <summary>
		/// Creates a node that writes the current forward signal as an output of the graph
		/// </summary>
		/// <param name="channel">Output channel</param>
		/// <param name="name">Optional name to give the node</param>
	    public WireBuilder AddOutput(int channel = 0, string? name = null)
	    {
		    SetNode(_factory.CreateOutputNode(channel, name));
		    return this;
	    }

        /// <summary>
        /// Adds a node
        /// </summary>
        /// <returns></returns>
        public WireBuilder Add(INode node)
        {
            SetNode(node);
            return this;
        }

        /// <summary>
        /// Adds an action that will be executed in the forward pass
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddForwardAction(IAction action, string? name = null)
        {
            SetNode(new ExecuteForwardAction(action, name));
            return this;
        }

        /// <summary>
        /// Adds an action that will be executed in the backward pass
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddBackwardAction(IAction action, string? name = null)
        {
            SetNode(new ExecuteBackwardAction(action, name));
            return this;
        }

		/// <summary>
		/// Adds a batch normalisation layer
		/// </summary>
		/// <param name="name">Optional name to give the node</param>
		public WireBuilder AddBatchNormalisation(string? name = null)
		{
			var size = CurrentSize;
			if (_depth > 1)
				throw new NotImplementedException("Currently only implemented for non convolutional networks");

            SetNode(_factory.CreateBatchNormalisation(size, name));
			return this;
		}

		/// <summary>
		/// Adds a simple recurrent neural network layer
		/// </summary>
		/// <param name="activation">Activation layer</param>
		/// <param name="initialMemory">Initial memory</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public WireBuilder AddSimpleRecurrent(INode activation, float[] initialMemory, string? name = null)
        {
            SetNode(_factory.CreateSimpleRecurrent(CurrentSize, initialMemory, activation, name));
            return SetNewSize((uint)initialMemory.Length);
        }

	    /// <summary>
	    /// Adds a simple recurrent neural network layer
	    /// </summary>
	    /// <param name="activation">Activation layer</param>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    /// <returns></returns>
	    public WireBuilder AddSimpleRecurrent(INode activation, uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateSimpleRecurrent(CurrentSize, new float[memorySize], activation, name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
        /// Adds an Elman recurrent neural network layer
        /// </summary>
        /// <param name="activation">First activation layer</param>
        /// <param name="activation2">Second activation layer</param>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddElman(INode activation, INode activation2, float[] initialMemory, string? name = null)
        {
            SetNode(_factory.CreateElman(CurrentSize, initialMemory, activation, activation2, name));
            return SetNewSize((uint)initialMemory.Length);
        }

	    /// <summary>
	    /// Adds an Elman recurrent neural network layer
	    /// </summary>
	    /// <param name="activation">First activation layer</param>
	    /// <param name="activation2">Second activation layer</param>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    /// <returns></returns>
	    public WireBuilder AddElman(INode activation, INode activation2, uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateElman(CurrentSize, new float[memorySize], activation, activation2, name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
        /// Adds a Jordan recurrent neural network layer
        /// </summary>
        /// <param name="activation">First activation layer</param>
        /// <param name="activation2">Second activation layer</param>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddJordan(INode activation, INode activation2, float[] initialMemory, string? name = null)
        {
            SetNode(_factory.CreateJordan(CurrentSize, initialMemory, activation, activation2, name));
            return SetNewSize((uint)initialMemory.Length);
        }

	    /// <summary>
	    /// Adds a Jordan recurrent neural network layer
	    /// </summary>
	    /// <param name="activation">First activation layer</param>
	    /// <param name="activation2">Second activation layer</param>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    /// <returns></returns>
	    public WireBuilder AddJordan(INode activation, INode activation2, uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateJordan(CurrentSize, new float[memorySize], activation, activation2, name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
        /// Adds a gated recurrent unit recurrent neural network layer
        /// </summary>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddGru(float[] initialMemory, string? name = null)
        {
            SetNode(_factory.CreateGru(CurrentSize, initialMemory, name));
            return SetNewSize((uint)initialMemory.Length);
        }

	    /// <summary>
	    /// Adds a gated recurrent unit recurrent neural network layer
	    /// </summary>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    /// <returns></returns>
	    public WireBuilder AddGru(uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateGru(CurrentSize, new float[memorySize], name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
        /// Adds a recurrent additive layer (recurrent)
        /// </summary>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        public WireBuilder AddRan(float[] initialMemory, string? name = null)
        {
            SetNode(_factory.CreateRan(CurrentSize, initialMemory, name));
            return SetNewSize((uint)initialMemory.Length);
        }

	    /// <summary>
	    /// Adds a recurrent additive layer (recurrent)
	    /// </summary>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    public WireBuilder AddRan(uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateRan(CurrentSize, new float[memorySize], name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
        /// Adds a long short term memory recurrent neural network layer
        /// </summary>
        /// <param name="initialMemory">Initial memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddLstm(float[] initialMemory, string? name = null)
        {
            SetNode(_factory.CreateLstm(CurrentSize, initialMemory, name));
            return SetNewSize((uint)initialMemory.Length);
        }

	    /// <summary>
	    /// Adds a long short term memory recurrent neural network layer
	    /// </summary>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    /// <returns></returns>
	    public WireBuilder AddLstm(uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateLstm(CurrentSize, new float[memorySize], name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
        /// Adds a node that will reverse the sequence (for bidirectional recurrent neural networks)
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <param name="index">Input index to reverse</param>
        /// <returns></returns>
        public WireBuilder ReverseSequence(int index = 0, string? name = null)
        {
            SetNode(_factory.CreateSequenceReverser(index, name));
            return this;
        }

        /// <summary>
        /// Adds a max pooling convolutional layer
        /// </summary>
        /// <param name="filterWidth">Width of max pooliing filter</param>
        /// <param name="filterHeight">Height of max pooling filter</param>
        /// <param name="xStride">X stride</param>
        /// <param name="yStride">Y stride</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddMaxPooling(uint filterWidth, uint filterHeight, uint xStride, uint yStride, string? name = null)
        {
            SetNode(_factory.CreateMaxPool(filterWidth, filterHeight, xStride, yStride, name));

            _width = (_width - filterWidth) / xStride + 1;
            _height = (_height - filterHeight) / yStride + 1;

            return this;
        }

        /// <summary>
        /// Adds a convolutional layer
        /// </summary>
        /// <param name="filterCount">Number of filters in the layer</param>
        /// <param name="padding">Padding to add before applying the convolutions</param>
        /// <param name="filterWidth">Width of each filter</param>
        /// <param name="filterHeight">Height of each filter</param>
        /// <param name="xStride">Filter x stride</param>
        /// <param name="yStride">Filter y stride</param>
        /// <param name="shouldBackpropagate">True to calculate a backpropagation signal</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddConvolutional(uint filterCount, uint padding, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool shouldBackpropagate = true, string? name = null)
        {
            SetNode(_factory.CreateConvolutional(_depth, filterCount, padding, filterWidth, filterHeight, xStride, yStride, shouldBackpropagate, name));

            _width = (_width + (2 * padding) - filterWidth) / xStride + 1;
            _height = (_height + (2 * padding) - filterHeight) / yStride + 1;
            _depth = filterCount;

            return this;
        }

        /// <summary>
        /// Transposes the graph signal to move between convolutional and non-convolutional layers
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Transpose(string? name = null)
        {
            SetNode(new TransposeSignal(name));
            return this;
        }

        /// <summary>
        /// Transposes the graph signal and merges each depth slice
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder TransposeAndCombine(string? name = null)
        {
            SetNode(new TransposeAndCombineSignal(name));
            _depth = 1;
            return this;
        }

        /// <summary>
        /// Adds backpropagation - when executed an error signal will be calculated and flow backwards to previous nodes
        /// </summary>
        /// <param name="errorMetric">Error metric to calculate the error signal</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddBackpropagation(string? name = null)
        {
            AddForwardAction(new Backpropagate(), name);
            return this;
        }

        /// <summary>
        /// Adds backpropagation through time
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddBackpropagationThroughTime(string? name = null)
        {
            AddForwardAction(new BackpropagateThroughTime(), name);
            return this;
        }

        /// <summary>
        /// Constrains the error signal in the forward direction
        /// </summary>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder ConstrainForwardSignal(float min = -1f, float max = 1f, string? name = null)
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
        public WireBuilder ConstrainBackwardSignal(float min = -1f, float max = 1f, string? name = null)
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
        public WireBuilder WriteNodeMemoryToSlot(string slotName, IHaveMemoryNode node, string? name = null)
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
        public WireBuilder JoinInputWithMemory(string slotName, string? name = null)
        {
            AddForwardAction(new JoinInputWithMemory(slotName), name);
            return this;
        }

        /// <summary>
        /// Tries to find the specified node
        /// </summary>
        /// <param name="name">The friendly name of the node</param>
        /// <returns></returns>
        public INode? Find(string name)
        {
            return _first.FindByName(name);
        }

        /// <summary>
        /// The last added node
        /// </summary>
        public INode? LastNode { get; private set; }
    }
}
