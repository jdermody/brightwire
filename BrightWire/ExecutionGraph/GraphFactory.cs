using BrightWire.Descriptor;
using BrightWire.ExecutionGraph.DataSource;
using BrightWire.ExecutionGraph.DataTableAdapter;
using BrightWire.ExecutionGraph.Engine;
using BrightWire.ExecutionGraph.GradientDescent;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Filter;
using BrightWire.ExecutionGraph.Node.Gate;
using BrightWire.ExecutionGraph.Node.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.ExecutionGraph.Node.Layer;
using BrightWire.ExecutionGraph.Node.Operation;
using BrightWire.ExecutionGraph.WeightInitialisation;
using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.Node;
using BrightWire.ExecutionGraph.Node.Output;

namespace BrightWire.ExecutionGraph
{
	/// <summary>
	/// Creates graph nodes
	/// </summary>
	public class GraphFactory : IHaveLinearAlgebraProvider
	{
        readonly ICreateTemplateBasedGradientDescent _rmsProp = new RmsPropDescriptor();
		readonly Stack<IPropertySet> _propertySetStack = new();
		readonly IPropertySet _defaultPropertySet;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="lap">The linear algebra provider to use</param>
		/// <param name="propertySet">A property set with initialisation data (optional)</param>
		public GraphFactory(LinearAlgebraProvider lap, IPropertySet? propertySet = null)
		{
			LinearAlgebraProvider = lap;
			WeightInitialisation = new WeightInitialisationProvider(LinearAlgebraProvider);
			GraphOperation = new GraphOperationProvider();
			GraphAction = new GraphActionProvider();
			_defaultPropertySet = propertySet ?? new PropertySet(LinearAlgebraProvider) {
				WeightInitialisation = WeightInitialisation.Gaussian,
				TemplateGradientDescentDescriptor = _rmsProp
			};
        }

        /// <inheritdoc />
        public LinearAlgebraProvider LinearAlgebraProvider { get; }

        /// <summary>
        /// Bright data context
        /// </summary>
        public BrightDataContext Context => LinearAlgebraProvider.Context;

        /// <summary>
		/// The current property set
		/// </summary>
		public IPropertySet CurrentPropertySet => _propertySetStack.Any()
			? _propertySetStack.Peek()
			: _defaultPropertySet
		;

		/// <summary>
		/// Clones the current property set with an optional mutator and then pushes it onto the stack
		/// </summary>
		/// <param name="mutator">Callback that can modify the cloned property set</param>
		public void PushPropertySet(Action<IPropertySet>? mutator)
		{
			var newPropertySet = CurrentPropertySet.Clone();
			mutator?.Invoke(newPropertySet);
			_propertySetStack.Push(newPropertySet);
		}

		/// <summary>
		/// Pops the last property set from the stack
		/// </summary>
		public void PopPropertyStack()
		{
			if (_propertySetStack.Any())
				_propertySetStack.Pop();
		}

		/// <summary>
		/// Creates a gradient descent optimiser for the given matrix
		/// </summary>
		/// <param name="weight"></param>
		/// <returns></returns>
		public IGradientDescentOptimisation CreateWeightUpdater(IMatrix weight)
		{
			var propertySet = CurrentPropertySet;

			// look for the interface directly
			var ret = propertySet.GradientDescent;

			// look for a descriptor
			if (ret == null) {
				var createGradientDescent = propertySet.GradientDescentDescriptor;
				if (createGradientDescent != null)
					ret = createGradientDescent.Create(propertySet);
			}

			// look for a template based descriptor
			var createTemplateGradientDescent = propertySet.TemplateGradientDescentDescriptor;
			if (createTemplateGradientDescent != null)
				ret = createTemplateGradientDescent.Create(ret ?? SimpleGradientDescent, weight, propertySet);

			return ret ?? SimpleGradientDescent;
		}

		/// <summary>
		/// Returns an object that can initialize weights in the graph
		/// </summary>
		/// <returns></returns>
		public IWeightInitialisation GetWeightInitialisation()
		{
			var propertySet = CurrentPropertySet;
			var ret = propertySet.WeightInitialisation;
			return ret ?? WeightInitialisation.Gaussian;
		}

        /// <summary>
        /// Creates a graph training engine
        /// </summary>
        /// <param name="dataSource">Segment source with training data</param>
        /// <param name="errorMetric">Error metric to train with</param>
        /// <param name="learningRate">Initial learning rate</param>
        /// <param name="batchSize">Mini batch size</param>
        /// <returns></returns>
        public IGraphTrainingEngine CreateTrainingEngine(
            IDataSource dataSource, 
			IErrorMetric errorMetric,
            float learningRate = 0.1f, 
            uint batchSize = 128
        )
		{
            var ret = new TrainingEngine(this, dataSource, errorMetric);
            var learningContext = ret.LearningContext;
            learningContext.LearningRate = learningRate;
            learningContext.BatchSize = batchSize;
            return ret;
        }

        /// <summary>
        /// Creates a graph execution engine
        /// </summary>
        /// <param name="graph">The serialised graph to execute</param>
        /// <param name="lap">Linear algebra provider (optional)</param>
        /// <returns></returns>
        public IGraphExecutionEngine CreateExecutionEngine(ExecutionGraphModel graph, LinearAlgebraProvider? lap = null)
		{
			var input = this.CreateFrom(graph);
			return new ExecutionEngine(lap ?? LinearAlgebraProvider, graph, input);
		}

		/// <summary>
		/// Creates a data source from a list of vectors
		/// </summary>
		/// <param name="vectorList">The list of vectors that will be the rows in the data source</param>
		public IDataSource CreateDataSource(IVector[] vectorList)
		{
			return new VectorDataSource(LinearAlgebraProvider, vectorList);
		}

		/// <summary>
		/// Creates a data source from a list of matrices (sequential vectors)
		/// </summary>
		/// <param name="sequenceList">The list of matrices that will be the rows in the data source</param>
		/// <returns></returns>
		public IDataSource CreateDataSource(IMatrix[] sequenceList)
		{
			return new SequentialDataSource(LinearAlgebraProvider, sequenceList);
		}

		/// <summary>
		/// Creates a data source from a list of tensors
		/// </summary>
		/// <param name="tensorList">The list of tensors that will be the rows in the data source</param>
		/// <returns></returns>
		public IDataSource CreateDataSource(ITensor3D[] tensorList)
		{
			return new TensorDataSource(LinearAlgebraProvider, tensorList);
		}

        /// <summary>
        /// Creates a data source from a data table
        /// </summary>
        /// <param name="dataTable">The data table to convert</param>
        /// <param name="featureColumns">Column indices to use as features (or none to use all non target columns)</param>
        /// <returns></returns>
        public IDataSource CreateDataSource(IDataTable dataTable, params uint[] featureColumns)
		{
			var columns = dataTable.ColumnTypes;
			var targetColumn = dataTable.GetTargetColumnOrThrow();
            if (featureColumns.Length == 0) {
                featureColumns = columns
                    .Select((_, i) => (uint) i)
                    .Where(d => d != targetColumn)
                    .ToArray();
            }

            var featureColumnTypes = featureColumns.Select(i => columns[i]).ToArray();
			var targetColumnType = columns[targetColumn];
			var featureColumnType = featureColumnTypes.FirstOrDefault();

			if (featureColumnType != BrightDataType.Unknown && featureColumnTypes.All(ct => ct == featureColumnType)) {
				// many to many
				if (featureColumnType == BrightDataType.Matrix && targetColumnType == BrightDataType.Matrix)
					return new SequentialDataTableAdapter(dataTable, featureColumns);

				// one to one
				if (featureColumnType == BrightDataType.Vector && targetColumnType == BrightDataType.Vector)
					return new VectorBasedDataTableAdapter(dataTable, featureColumns);

				// one to many
				if (featureColumnType == BrightDataType.Vector && targetColumnType == BrightDataType.Matrix)
					return new OneToManyDataTableAdapter(dataTable, featureColumns);

				// many to one
				if (featureColumnType == BrightDataType.Matrix && targetColumnType == BrightDataType.Vector)
					return new ManyToOneDataTableAdapter(dataTable, featureColumns);

				// volume classification
				if (featureColumnType == BrightDataType.Tensor3D && targetColumnType == BrightDataType.Vector)
					return new Tensor3DBasedDataTableAdapter(dataTable, featureColumns);

				// index list
				if (featureColumnType == BrightDataType.IndexList)
					return new IndexListDataTableAdapter(dataTable, null, featureColumns);

				// weighted index list
				if (featureColumnType == BrightDataType.WeightedIndexList)
					return new WeightedIndexListDataTableAdapter(dataTable, null, featureColumns);
			}

			// default adapter
			return new DefaultDataTableAdapter(dataTable, null, null, featureColumns);
		}

        /// <summary>
        /// Create a row classifier node
        /// </summary>
        /// <param name="classifier">The classifier for each row</param>
        /// <param name="dataTable">The data table that contains the rows to classify (linked by mini batch index)</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public (NodeBase RowClassifier, uint OutputSize) CreateClassifier(IRowClassifier classifier, IDataTable dataTable, string? name = null)
        {
            var ret = new RowClassifier(LinearAlgebraProvider, classifier, dataTable, name);
            return (ret, ret.OutputSize);
        }

        /// <summary>
        /// Creates a feed forward layer
        /// </summary>
        /// <param name="inputSize">Number of incoming connections</param>
        /// <param name="outputSize">Number of outgoing connections</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public NodeBase CreateFeedForward(uint inputSize, uint outputSize, string? name = null)
		{
			// create weights and bias
			var weightInit = GetWeightInitialisation();
			var bias = weightInit.CreateBias(outputSize);
			var weight = weightInit.CreateWeight(inputSize, outputSize);

			// get the gradient descent optimisations
			var optimisation = CreateWeightUpdater(weight);

			// create the layer
			return new FeedForward(inputSize, outputSize, bias, weight, optimisation, name);
		}

		/// <summary>
		/// Creates a new drop connect layer (a feed forward layer with drop out applied to the weights)
		/// </summary>
		/// <param name="dropoutPercentage">Percentage of connections to drop (0..1)</param>
		/// <param name="inputSize">Number of incoming connections</param>
		/// <param name="outputSize">Number of outgoing connections</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateDropConnect(float dropoutPercentage, uint inputSize, uint outputSize, string? name = null)
		{
			// create weights and bias
			var weightInit = GetWeightInitialisation();
			var bias = weightInit.CreateBias(outputSize);
			var weight = weightInit.CreateWeight(inputSize, outputSize);

			// get the gradient descent optimisations
			var optimisation = CreateWeightUpdater(weight);

			return new DropConnect(Context, dropoutPercentage, inputSize, outputSize, bias, weight, optimisation, name);
		}

		/// <summary>
		/// Creates a layer whose weights are shared with another layer (but transposed)
		/// </summary>
		/// <param name="layer">The layer that shares weights</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateTiedFeedForward(IFeedForward layer, string? name = null)
		{
			var weightInit = GetWeightInitialisation();
			return new TiedFeedForward(layer, weightInit, name);
		}

		/// <summary>
		/// Creates a convolutional layer
		/// </summary>
		/// <param name="inputDepth">Input depth</param>
		/// <param name="filterCount">Number of convolutional filters</param>
		/// <param name="padding">Padding to apply before convolutions</param>
		/// <param name="filterWidth">Width of each filter</param>
		/// <param name="filterHeight">Height of each filter</param>
		/// <param name="xStride">X stride</param>
		/// <param name="yStride">Y stride</param>
		/// <param name="shouldBackpropagate">True to calculate the backpropagation error signal</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateConvolutional(uint inputDepth, uint filterCount, uint padding, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool shouldBackpropagate = true, string? name = null)
		{
			var weightInit = GetWeightInitialisation();
			return new Convolutional(shouldBackpropagate, weightInit, CreateWeightUpdater, inputDepth, filterCount, padding, filterWidth, filterHeight, xStride, yStride, name);
		}

		/// <summary>
		/// Creates a max pooling convolutional layer
		/// </summary>
		/// <param name="filterWidth">Width of each filter</param>
		/// <param name="filterHeight">Height of each filter</param>
		/// <param name="xStride">X stride</param>
		/// <param name="yStride">Y stride</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateMaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, string? name = null) => new MaxPool(filterWidth, filterHeight, xStride, yStride, name);

        /// <summary>
        /// Creates a bridge between two recurrent nodes that will copy the hidden state from one to another and copy the error signal backwards between the two
        /// </summary>
        /// <param name="fromName">Name of the first recurrent node</param>
        /// <param name="toName">Name of the second recurrent node</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public NodeBase CreateRecurrentBridge(string fromName, string toName, string? name) => new RecurrentBridge(fromName, toName, name);

		/// <summary>
		/// Creates a simple recurrent layer
		/// </summary>
		/// <param name="inputSize">Number of incoming connections</param>
		/// <param name="memorySize">Size of the layer memory</param>
		/// <param name="activation">Activation layer</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateSimpleRecurrent(uint inputSize, uint memorySize, NodeBase activation, string? name = null) => new SimpleRecurrent(this, inputSize, new float[memorySize], activation, name);

        /// <summary>
		/// Creates an Elman recurrent layer
		/// </summary>
		/// <param name="inputSize">Number of incoming connections</param>
		/// <param name="memorySize">Size of the layer memory</param>
		/// <param name="activation">First activation layer</param>
		/// <param name="activation2">Second activation layer</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateElman(uint inputSize, uint memorySize, NodeBase activation, NodeBase activation2, string? name = null)
		{
			return new ElmanJordan(this, true, inputSize, new float[memorySize], activation, activation2, name);
		}

		/// <summary>
		/// Creates a Jordan recurrent layer
		/// </summary>
		/// <param name="inputSize">Number of incoming connections</param>
		/// <param name="memorySize">Size of the layer memory</param>
		/// <param name="activation">First activation layer</param>
		/// <param name="activation2">Second activation layer</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateJordan(uint inputSize, uint memorySize, NodeBase activation, NodeBase activation2, string? name = null)
		{
			return new ElmanJordan(this, false, inputSize, new float[memorySize], activation, activation2, name);
		}

		/// <summary>
		/// Creates a node that subtracts each input from 1 (1-x)
		/// </summary>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateOneMinusInput(string? name = null)
		{
			return GraphOperation.OneMinusInput(name);
		}

		/// <summary>
		/// Creates a node that outputs the reversed index of the current sequence (for bidirectional recurrent networks)
		/// </summary>
        /// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateSequenceReverser(string? name = null)
		{
			return new ReverseSequence(0, name);
		}

		/// <summary>
		/// Creates a node that performs batch normalization
		/// </summary>
		/// <param name="inputSize">Number of incoming connections</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateBatchNormalisation(uint inputSize, string? name)
		{
			return new BatchNorm(this, inputSize, name);
		}

		/// <summary>
		/// Creates a GRU recurrent layer
		/// </summary>
		/// <param name="inputSize">Number of incoming connections</param>
		/// <param name="memorySize">Size of the layer memory</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateGru(uint inputSize, uint memorySize, string? name = null)
		{
			return new GatedRecurrentUnit(this, inputSize, new float[memorySize], name);
		}

		/// <summary>
		/// Creates a Recurrent Additive Layer (recurrent)
		/// </summary>
		/// <param name="inputSize">Number of incoming connections</param>
		/// <param name="memorySize">Size of the layer memory</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateRan(uint inputSize, uint memorySize, string? name = null)
		{
			return new RecurrentAdditiveLayer(this, inputSize, new float[memorySize], name);
		}

		/// <summary>
		/// Creates a LSTM recurrent layer
		/// </summary>
		/// <param name="inputSize">Number of incoming connections</param>
		/// <param name="memorySize">Size of the layer memory</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateLstm(uint inputSize, uint memorySize, string? name = null)
		{
            return new Lstm(this, inputSize, new float[memorySize], name);
			//return new LongShortTermMemory(this, inputSize, new float[memorySize], name);
		}

		/// <summary>
		/// Creates a layer that drops random connections
		/// </summary>
		/// <param name="dropoutPercentage">Percentage to drop (0..1)</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public NodeBase CreateDropOut(float dropoutPercentage, string? name = null)
		{
			return new DropOut(Context, dropoutPercentage, name);
		}

		/// <summary>
		/// Creates a node that writes the current forward signal as an output of the graph
		/// </summary>
		/// <param name="channel">Output channel</param>
		/// <param name="name">Optional name to give the node</param>
		public NodeBase CreateOutputNode(int channel = 0, string? name = null)
		{
			return new StoreOutput(channel, name);
		}

		/// <summary>
		/// Builds a new wire from the engine's input node
		/// </summary>
		/// <param name="engine">Graph engine to build with</param>
        /// <returns></returns>
		public WireBuilder Connect(IGraphTrainingEngine engine)
		{
			return new(this, engine);
		}

		/// <summary>
		/// Builds a new wire from the selected node
		/// </summary>
		/// <param name="inputSize">Number of outgoing connections</param>
		/// <param name="node">The node to build from</param>
		/// <returns></returns>
		public WireBuilder Connect(uint inputSize, NodeBase node)
		{
			return new(this, inputSize, node);
		}

        /// <summary>
		/// Adds the output of two wires into a new wire
		/// </summary>
		/// <param name="input1">First wire</param>
		/// <param name="input2">Second wire</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public WireBuilder Add(WireBuilder input1, WireBuilder input2, string? name = null)
        {
            if (input1.CurrentSize != input2.CurrentSize)
                throw new Exception("Input wires have different sizes");
            if (input1.LastNode == null || input2.LastNode == null)
                throw new Exception("Two nodes not found");
			return Add(input1.CurrentSize, input1.LastNode, input2.LastNode, name);
		}

        /// <summary>
		/// Subtracts the second input from the first input and sends the result to a new wire
		/// </summary>
		/// <param name="input1">Wire to subtract from</param>
		/// <param name="input2">Wire to subtract</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public WireBuilder Subtract(WireBuilder input1, WireBuilder input2, string? name = null)
		{
            if (input1.CurrentSize != input2.CurrentSize)
                throw new Exception("Input wires have different sizes");
            if (input1.LastNode == null || input2.LastNode == null)
                throw new Exception("Two nodes not found");
			return Subtract(input1.CurrentSize, input1.LastNode, input2.LastNode, name);
		}

        /// <summary>
        /// Subtracts the second input from the first input and sends the result to a new wire
        /// </summary>
        /// <param name="inputSize">The number of connections</param>
        /// <param name="input1">The node to subtract from</param>
        /// <param name="input2">The node to subtract</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Subtract(uint inputSize, NodeBase input1, NodeBase input2, string? name = null) => Join(inputSize, input1, input2, new SubtractGate(name));

        /// <summary>
		/// Adds the output of two nodes together into a new wire
		/// </summary>
		/// <param name="inputSize">The number of connections</param>
		/// <param name="input1">First node</param>
		/// <param name="input2">Second node</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public WireBuilder Add(uint inputSize, NodeBase input1, NodeBase input2, string? name = null) => Join(inputSize, input1, input2, new AddGate(name));

        WireBuilder Join(uint inputSize, NodeBase input1, NodeBase input2, NodeBase joinWith)
        {
            var wireToPrimary = new WireToNode(joinWith);
            var wireToSecondary = new WireToNode(joinWith, 1);

            input1.Output.Add(wireToPrimary);
            input2.Output.Add(wireToSecondary);

            return new WireBuilder(this, inputSize, joinWith);
        }

		/// <summary>
		/// Multiplies the output of two wires into a new wire
		/// </summary>
		/// <param name="input1"></param>
		/// <param name="input2"></param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public WireBuilder Multiply(WireBuilder input1, WireBuilder input2, string? name = null)
		{
            if (input1.CurrentSize != input2.CurrentSize)
                throw new Exception("Input wires have different sizes");
            if (input1.LastNode == null || input2.LastNode == null)
                throw new Exception("Two nodes not found");
			return Multiply(input1.CurrentSize, input1.LastNode, input2.LastNode, name);
		}

        /// <summary>
        /// Multiplies the output of two nodes together into a new wire
        /// </summary>
        /// <param name="inputSize">The number of connections</param>
        /// <param name="input1">First node</param>
        /// <param name="input2">Second node</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Multiply(uint inputSize, NodeBase input1, NodeBase input2, string? name = null) => Join(inputSize, input1, input2, new MultiplyGate(name));

        /// <summary>
		/// Concatenates two wires together into a new wire
		/// </summary>
		/// <param name="input1">First wire to join</param>
		/// <param name="input2">Second wire to join</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public WireBuilder Join(WireBuilder input1, WireBuilder input2, string? name = null)
		{
			var ret = new JoinGate(name, input1, input2);
			var wireToPrimary = new WireToNode(ret);
			var wireToSecondary = new WireToNode(ret, 1);

			input1.LastNode?.Output.Add(wireToPrimary);
			input2.LastNode?.Output.Add(wireToSecondary);

			return new WireBuilder(this, input1.CurrentSize + input2.CurrentSize, ret);
		}

        /// <summary>
        /// Joins multiple wires into a new wire
        /// </summary>
        /// <param name="wires">Wires to join</param>
        /// <returns></returns>
        public WireBuilder Join(params WireBuilder[] wires) => Join(null, wires);

		/// <summary>
		/// Joins multiple wires into a new wire
		/// </summary>
		/// <param name="name">Optional name to give the node</param>
		/// <param name="wires">Wires to join</param>
		/// <returns></returns>
        public WireBuilder Join(string? name, params WireBuilder[] wires)
        {
            var ret = new JoinGate(name, wires);
            uint channel = 0;
            uint newSize = 0;
            foreach (var wire in wires) {
                wire.LastNode?.Output.Add(new WireToNode(ret, channel++));
                newSize += wire.CurrentSize;
            }
            return new WireBuilder(this, newSize, ret);
        }

        /// <summary>
		/// Concatenates two wires together into a new wire, but reverses the sequence index of the second input to join
		/// </summary>
		/// <param name="forwardInput">Forward wire to join</param>
		/// <param name="backwardInput">Backward wire to join</param>
		/// <param name="name">Optional name to give the node</param>
		/// <returns></returns>
		public WireBuilder BidirectionalJoin(WireBuilder forwardInput, WireBuilder backwardInput, string? name = null)
		{
			var ret = new ReverseTemporalJoin(name, forwardInput, backwardInput);
			var wireToPrimary = new WireToNode(ret);
			var wireToSecondary = new WireToNode(ret, 1);

			forwardInput.LastNode?.Output.Add(wireToPrimary);
			backwardInput.LastNode?.Output.Add(wireToSecondary);

			return new WireBuilder(this, forwardInput.CurrentSize + backwardInput.CurrentSize, ret);
		}

		/// <summary>
		/// Wraps an existing node, enabling that node to be used at multiple locations in the same graph
		/// </summary>
		/// <param name="nodeToWrap">Node to wrap</param>
		/// <param name="name">Optional name to give the wrapping node</param>
		/// <returns></returns>
		public NodeBase CreateWrapper(NodeBase nodeToWrap, string? name = null)
		{
			return new NodeWrapper(nodeToWrap, name);
		}

		/// <summary>
		/// Creates a node from it's serialised model
		/// </summary>
		/// <param name="node">The node model</param>
		public NodeBase Create(ExecutionGraphModel.Node node)
		{
			var type = TypeLoader.LoadType(node.TypeName);
			var ret = GenericActivator.CreateUninitialized<NodeBase>(type);
			ret.Initialise(this, node.Id, node.Name, node.Description, node.Data);
			return ret;
		}

		/// <summary>
		/// Creates a new leaky relu activation layer
		/// </summary>
		/// <param name="name">Optional name to give the node</param>
		public NodeBase LeakyReluActivation(string? name = null) => new LeakyRelu(name);

		/// <summary>
		/// Creates a new relu activation layer
		/// </summary>
		/// <param name="name">Optional name to give the node</param>
		public NodeBase ReluActivation(string? name = null) => new Relu(name);

		/// <summary>
		/// Creates a new sigmoid activation layer
		/// </summary>
		/// <param name="name">Optional name to give the node</param>
		public NodeBase SigmoidActivation(string? name = null) => new Sigmoid(name);

		/// <summary>
		/// Creates a new tanh activation layer
		/// </summary>
		/// <param name="name">Optional name to give the node</param>
		public NodeBase TanhActivation(string? name = null) => new Tanh(name);

		/// <summary>
		/// Creates a new softmax activation layer
		/// </summary>
		/// <param name="name">Optional name to give the node</param>
		public NodeBase SoftMaxActivation(string? name = null) => new SoftMax(name);

		/// <summary>
		/// Creates a constant weight initialiser
		/// </summary>
		/// <param name="biasValue">Single bias value</param>
		/// <param name="weightValue">Single weight value</param>
		public IWeightInitialisation ConstantWeightInitialisation(float biasValue = 0f, float weightValue = 1f) => new Constant(LinearAlgebraProvider, biasValue, weightValue);

		/// <summary>
		/// Creates a gaussian weight initialiser
		/// </summary>
		/// <param name="zeroBias">True to set bias values to zero, otherwise bias initialisation is treated the same as weight initialisation</param>
		/// <param name="stdDev">Standard deviation of gaussian distribution</param>
		/// <param name="varianceCalibration">How to calibrate the variance</param>
		/// <param name="varianceCount">How to count connections while calibrating connections</param>
		public IWeightInitialisation GaussianWeightInitialisation(
			bool zeroBias = true,
			float stdDev = 0.1f,
			GaussianVarianceCalibration varianceCalibration = GaussianVarianceCalibration.SquareRootN,
			GaussianVarianceCount varianceCount = GaussianVarianceCount.FanIn
		) => new Gaussian(LinearAlgebraProvider, zeroBias, stdDev, varianceCalibration, varianceCount);

		/// <summary>
		/// Creates an identity weight initialiser
		/// </summary>
		/// <param name="identityValue">The value to give to each diagonal value</param>
		public IWeightInitialisation IdentityWeightInitialisation(float identityValue = 1f) => new Identity(LinearAlgebraProvider, identityValue);

		/// <summary>
		/// Creates a xavier weight initialiser
		/// </summary>
		/// <param name="parameter">Xavier parameter</param>
		public IWeightInitialisation XavierWeightInitialisation(float parameter = 6) => new Xavier(LinearAlgebraProvider, parameter);

		/// <summary>
		/// Creates an AdaGrad gradient descent optimiser
		/// </summary>
		public ICreateTemplateBasedGradientDescent AdaGrad() => new AdaGradDescriptor();

		/// <summary>
		/// Creates an Adam gradient descent optimiser
		/// </summary>
		/// <param name="decay">Decay parameter</param>
		/// <param name="decay2">Second decay parameter</param>
		public ICreateTemplateBasedGradientDescent Adam(float decay = 0.9f, float decay2 = 0.99f) => new AdamDescriptor(decay, decay2);

		/// <summary>
		/// Creates a L1 regularisation gradient descent optimiser
		/// </summary>
		/// <param name="lambda">L1 parameter</param>
		public ICreateGradientDescent L1(float lambda) => new L1RegularisationDescriptor(lambda);

		/// <summary>
		/// Creates a L2 regularisation gradient descent optimiser
		/// </summary>
		/// <param name="lambda">L2 parameter</param>
		public ICreateGradientDescent L2(float lambda) => new L2RegularisationDescriptor(lambda);

		/// <summary>
		/// Creates a momentum gradient descent optimiser
		/// </summary>
		/// <param name="momentum">Momentum parameter</param>
		public ICreateTemplateBasedGradientDescent Momentum(float momentum = 0.9f) => new MomentumDescriptor(momentum);

		/// <summary>
		/// Creates a nesterov momentum gradient descent optimiser
		/// </summary>
		/// <param name="momentum">Nesterov momentum parameter</param>
		/// <returns></returns>
		public ICreateTemplateBasedGradientDescent NesterovMomentum(float momentum = 0.9f) => new NesterovMomentumDescriptor(momentum);

		/// <summary>
		/// Creates a rms prop gradient descent optimiser
		/// </summary>
		/// <param name="decay">Rms decay</param>
		/// <returns></returns>
		public ICreateTemplateBasedGradientDescent RmsProp(float decay = 0.9f) => new RmsPropDescriptor(decay);

        /// <summary>
        /// Uses vanilla stochastic gradient descent
        /// </summary>
        public IGradientDescentOptimisation SimpleGradientDescent { get; } = new StochasticGradientDescent();

        /// <summary>
        /// Prebuilt regularisation
        /// </summary>
        public class RegularisationProvider
		{
			/// <summary>
			/// L1 regularisation
			/// </summary>
			public ICreateGradientDescent L1 { get; } = new L1RegularisationDescriptor(0.1f);

			/// <summary>
			/// L2 regularisation
			/// </summary>
			public ICreateGradientDescent L2 { get; } = new L1RegularisationDescriptor(0.5f);
		}
		/// <summary>
		/// Prebuilt regularisation
		/// </summary>
		public RegularisationProvider Regularisation { get; } = new();

		/// <summary>
		/// Prebuilt gradient descent optimisers
		/// </summary>
		public class GradientDescentProvider
		{
			/// <summary>
			/// Adagrad gradient descent
			/// </summary>
			public ICreateTemplateBasedGradientDescent AdaGrad { get; } = new AdaGradDescriptor();

			/// <summary>
			/// Adam gradient descent
			/// </summary>
			public ICreateTemplateBasedGradientDescent Adam { get; } = new AdamDescriptor();

			/// <summary>
			/// Momentum gradient descent
			/// </summary>
			public ICreateTemplateBasedGradientDescent Momentum { get; } = new MomentumDescriptor();

			/// <summary>
			/// Nesterov momentum gradient descent
			/// </summary>
			public ICreateTemplateBasedGradientDescent NesterovMomentum { get; } = new NesterovMomentumDescriptor();

			/// <summary>
			/// Rms prop gradient descent
			/// </summary>
			public ICreateTemplateBasedGradientDescent RmsProp { get; } = new RmsPropDescriptor();
		}
		/// <summary>
		/// Prebuilt gradient descent optimisers
		/// </summary>
		public GradientDescentProvider GradientDescent { get; } = new();

		/// <summary>
		/// Error metrics
		/// </summary>
		public class ErrorMetricProvider
		{
			/// <summary>
			/// Binary classification error metric
			/// </summary>
			public IErrorMetric BinaryClassification { get; } = new ErrorMetric.BinaryClassification();

			/// <summary>
			/// Cross entropy error metric
			/// </summary>
			public IErrorMetric CrossEntropy { get; } = new ErrorMetric.CrossEntropy();

			/// <summary>
			/// One hot encoding error metric
			/// </summary>
			public IErrorMetric OneHotEncoding { get; } = new ErrorMetric.OneHotEncoding();

			/// <summary>
			/// Quadratic error metric
			/// </summary>
			public IErrorMetric Quadratic { get; } = new ErrorMetric.Quadratic();
		}
		/// <summary>
		/// Error metrics
		/// </summary>
		public ErrorMetricProvider ErrorMetric { get; } = new();

		/// <summary>
		/// Prebuilt weight initialisers
		/// </summary>
		public class WeightInitialisationProvider
		{
			/// <summary>
			/// All weights are initialised to 1
			/// </summary>
			public IWeightInitialisation Ones { get; }

			/// <summary>
			/// All weights are initialised to 0
			/// </summary>
			public IWeightInitialisation Zeroes { get; }

			/// <summary>
			/// Weights are randomly initialised using a gaussian distribution
			/// </summary>
			public IWeightInitialisation Gaussian { get; }

			/// <summary>
			/// Weights are randomly initialised using the xavier algorithm
			/// </summary>
			public IWeightInitialisation Xavier { get; }

			/// <summary>
			/// Weights are initialised to the identity matrix
			/// </summary>
			public IWeightInitialisation Identity { get; }

			/// <summary>
			/// Weights are initialised to the identity matrix / 10
			/// </summary>
			public IWeightInitialisation Identity01 { get; }

			internal WeightInitialisationProvider(LinearAlgebraProvider lap)
			{
				Ones = new Constant(lap);
				Zeroes = new Constant(lap, 0f, 0f);
				Gaussian = new Gaussian(lap);
				Xavier = new Xavier(lap);
				Identity = new Identity(lap, 1f);
				Identity01 = new Identity(lap, 0.1f);
			}
		}
		/// <summary>
		/// Prebuilt weight initialisers
		/// </summary>
		public WeightInitialisationProvider WeightInitialisation { get; }

		/// <summary>
		/// Provides standard graph operations
		/// </summary>
		public class GraphOperationProvider
		{
			/// <summary>
			/// Calculates inverse (1/x) of graph input
			/// </summary>
			/// <param name="name"></param>
			/// <returns></returns>
			public NodeBase OneDividedBy(string? name = null) => new OneDividedByInput(name);

			/// <summary>
			/// Calculates square (x^2) of graph input
			/// </summary>
			/// <param name="name"></param>
			/// <returns></returns>
			public NodeBase InputSquared(string? name = null) => new InputSquared(name);

			/// <summary>
			/// Calculates square root of graph input
			/// </summary>
			/// <param name="name"></param>
			/// <returns></returns>
			public NodeBase SquareRootOf(string? name = null) => new SquareRootOfInput(name);

			/// <summary>
			/// Calculates one minus graph input (1-x)
			/// </summary>
			/// <param name="name"></param>
			/// <returns></returns>
			public NodeBase OneMinusInput(string? name = null) => new OneMinusInput(name);
		}

		/// <summary>
		/// Standard graph operations
		/// </summary>
		public GraphOperationProvider GraphOperation { get; }

		/// <summary>
		/// Provides standard graph actions
		/// </summary>
		public class GraphActionProvider
		{
			/// <summary>
			/// Constrains the signal through the graph (either forward or backward)
			/// </summary>
			/// <param name="min">Minimum allowed value</param>
			/// <param name="max">Maximum allowed value</param>
			/// <returns></returns>
			public IAction Constrain(float min = -1f, float max = 1f) => new ConstrainSignal(min, max);
		}

		/// <summary>
		/// Standard graph actions
		/// </summary>
		public GraphActionProvider GraphAction { get; }
    }
}
