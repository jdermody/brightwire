using System;
using BrightData;
using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using BrightWire.ExecutionGraph.Node.Attention;
using BrightWire.ExecutionGraph.Node.Gate;
using BrightWire.ExecutionGraph.Node.Helper;
using BrightWire.ExecutionGraph.Node.Input;

namespace BrightWire.ExecutionGraph
{
    /// <summary>
    /// Wires nodes together to build strands of a graph
    /// </summary>
    public class WireBuilder
    {
        readonly GraphFactory _factory;
        readonly NodeBase _first;
        readonly uint _initialWidth;
        uint _width, _height, _depth;

        /// <summary>
        /// Connects new nodes starting from the specified node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="size">Initial wire size</param>
        /// <param name="node">The node to build from</param>
        public WireBuilder(GraphFactory factory, uint size, NodeBase node)
        {
            _factory = factory;
            _first = LastNode = node;
            _initialWidth = _width = size;
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
        public WireBuilder(GraphFactory factory, uint width, uint height, uint depth, NodeBase node)
        {
            _factory = factory;
            _first = LastNode = node;
            _initialWidth = _width = width;
            _height = height;
            _depth = depth;
        }

        /// <summary>
        /// Connects new nodes to the engine output node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="engine">Graph engine</param>
        public WireBuilder(GraphFactory factory, IGraphTrainingEngine engine) 
            : this(factory, engine.DataSource?.InputSize ?? throw new ArgumentException("No data source"), engine.Start)
        {
            if(engine.DataSource is IVolumeDataSource volumeDataSource) {
                _initialWidth = _width = volumeDataSource.Width;
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
        /// <param name="newSize">New size</param>
        public WireBuilder SetNewSize(uint newSize)
        {
            _width = newSize;
            _height = 1;
            _depth = 1;
            return this;
        }

        /// <summary>
        /// Changes the current size of the builder
        /// </summary>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="depth">New depth</param>
        public WireBuilder SetNewSize(uint width, uint height, uint depth)
        {
            _width = width;
            _height = height;
            _depth = depth;
            return this;
        }

        void SetNode(NodeBase node)
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
        public WireBuilder AddClassifier(IRowClassifier classifier, IDataTable dataTable, string? name = null)
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
            NodeBase node = _factory.CreateFeedForward(CurrentSize, outputSize, name);
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
        public WireBuilder Add(NodeBase node)
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
        /// Creates a bridge between two recurrent nodes that will copy the hidden state from one to another and copy the error signal backwards between the two
        /// </summary>
        /// <param name="fromName">Name of the first recurrent node</param>
        /// <param name="toName">Name of the second recurrent node</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddRecurrentBridge(string fromName, string toName, string? name = null)
        {
            SetNode(_factory.CreateRecurrentBridge(fromName, toName, name));
            return this;
        }

		/// <summary>
		/// Adds a simple recurrent neural network layer
		/// </summary>
		/// <param name="activation">Activation layer</param>
		/// <param name="memorySize">Size of the memory layer</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public WireBuilder AddSimpleRecurrent(NodeBase activation, uint memorySize, string? name = null)
        {
            SetNode(_factory.CreateSimpleRecurrent(CurrentSize, memorySize, activation, name));
            return SetNewSize(memorySize);
        }

        /// <summary>
	    /// Adds an Elman recurrent neural network layer
	    /// </summary>
	    /// <param name="activation">First activation layer</param>
	    /// <param name="activation2">Second activation layer</param>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    /// <returns></returns>
	    public WireBuilder AddElman(NodeBase activation, NodeBase activation2, uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateElman(CurrentSize, memorySize, activation, activation2, name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
	    /// Adds a Jordan recurrent neural network layer
	    /// </summary>
	    /// <param name="activation">First activation layer</param>
	    /// <param name="activation2">Second activation layer</param>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    /// <returns></returns>
	    public WireBuilder AddJordan(NodeBase activation, NodeBase activation2, uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateJordan(CurrentSize, memorySize, activation, activation2, name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
	    /// Adds a gated recurrent unit recurrent neural network layer
	    /// </summary>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    /// <returns></returns>
	    public WireBuilder AddGru(uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateGru(CurrentSize, memorySize, name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
	    /// Adds a recurrent additive layer (recurrent)
	    /// </summary>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    public WireBuilder AddRan(uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateRan(CurrentSize, memorySize, name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
	    /// Adds a long short term memory recurrent neural network layer
	    /// </summary>
	    /// <param name="memorySize">Size of the memory buffer</param>
	    /// <param name="name">Optional name to give the node</param>
	    /// <returns></returns>
	    public WireBuilder AddLstm(uint memorySize, string? name = null)
	    {
		    SetNode(_factory.CreateLstm(CurrentSize, memorySize, name));
		    return SetNewSize(memorySize);
	    }

        /// <summary>
        /// Adds a node that will reverse the sequence (for bidirectional recurrent neural networks)
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder ReverseSequence(string? name = null)
        {
            SetNode(_factory.CreateSequenceReverser(name));
            return this;
        }

        /// <summary>
        /// Adds a max pooling convolutional layer
        /// </summary>
        /// <param name="filterWidth">Width of max pooling filter</param>
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
        public WireBuilder TransposeFrom4DTensorToMatrix(string? name = null)
        {
            SetNode(new TransposeFrom4DTensorToMatrix(name));
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
        /// Pivots between the encoder and decoder sequences (seq2seq)
        /// </summary>
        /// <param name="encoderName">Encoder node name</param>
        /// <param name="decoderName">Decoder node name</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddSequenceToSequencePivot(string encoderName, string decoderName, string? name = null)
        {
            SetNode(new SequenceToSequenceGate(encoderName, decoderName, name));
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
        /// <param name="nodeName">The node name to read</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder WriteNodeMemoryToSlot(string slotName, string nodeName, string? name = null)
        {
            AddForwardAction(new CopyNamedMemory(slotName, FindMemoryNode(nodeName)), name);
            return this;
        }

        IHaveMemoryNode FindMemoryNode(string name)
        {
            if (Find(name) is not IHaveMemoryNode memoryNode)
                throw new ArgumentException($"Node not found: {name}");
            return memoryNode;
        }

        /// <summary>
        /// Adds a self attention node
        /// </summary>
        /// <param name="encoderName">Name of encoder node (must be same size as decoder)</param>
        /// <param name="decoderName">Name of decoder node (must be same size as encoder)</param>
        /// <param name="encoderSize">Size of the encoder</param>
        /// <param name="decoderSize">Size of the decoder</param>
        /// <param name="name">Optional name to give the node</param>
        public WireBuilder AddSelfAttention(string encoderName, string decoderName, uint encoderSize, uint decoderSize, string? name = null)
        {
            var newSize = _initialWidth + encoderSize + decoderSize;
            var weightInit = _factory.GetWeightInitialisation();
            SetNode(new SimpleAttention(_factory.LinearAlgebraProvider, encoderName, decoderName, _initialWidth, encoderSize, decoderSize, weightInit, _factory.CreateWeightUpdater, name));
            SetNewSize(_width + newSize);
            return this;
        }

        /// <summary>
        /// Concatenates the named memory slot with the input signal
        /// </summary>
        /// <param name="slotName">Memory slot name</param>
        /// <param name="memorySize">Size of the memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder JoinInputWithMemory(string slotName, uint memorySize, string? name = null)
        {
            SetNode(new JoinSignalWithMemory(slotName, name));
            SetNewSize(CurrentSize + memorySize);
            return this;
        }

        /// <summary>
        /// Tries to find the specified node
        /// </summary>
        /// <param name="name">The friendly name of the node</param>
        /// <returns></returns>
        public NodeBase? Find(string name) => _first.FindByName(name);

        /// <summary>
        /// The last added node
        /// </summary>
        public NodeBase? LastNode { get; private set; }
    }
}
