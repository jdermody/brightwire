using BrightWire.Descriptor;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.DataSource;
using BrightWire.ExecutionGraph.DataTableAdaptor;
using BrightWire.ExecutionGraph.Engine;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.GradientDescent;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Filter;
using BrightWire.ExecutionGraph.Node.Gate;
using BrightWire.ExecutionGraph.Node.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.ExecutionGraph.Node.Layer;
using BrightWire.ExecutionGraph.WeightInitialisation;
using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace BrightWire.ExecutionGraph
{
    /// <summary>
    /// Creates graph nodes
    /// </summary>
    public class GraphFactory
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IGradientDescentOptimisation _simpleGradientDescent = new StochasticGradientDescent();
        readonly ICreateTemplateBasedGradientDescent _rmsProp = new RmsPropDescriptor(0.9f);
        readonly List<(TypeInfo, Type, string)> _queryTypes = new List<(TypeInfo, Type, string)>();
        readonly Stack<IPropertySet> _propertySetStack = new Stack<IPropertySet>();
        IPropertySet _defaultPropertySet;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lap">The linear algebra provider to use</param>
        /// <param name="propertySet">A property set with initialisation data (optional)</param>
        public GraphFactory(ILinearAlgebraProvider lap, IPropertySet propertySet = null)
        {
            _lap = lap;
            WeightInitialisation = new WeightInitialisationProvider(_lap);
            _defaultPropertySet = propertySet ?? new PropertySet(_lap) {
                WeightInitialisation = WeightInitialisation.Gaussian,
                TemplateGradientDescentDescriptor = _rmsProp
            };

            // add the gradient descent descriptors
            _Add(typeof(L1RegularisationDescriptor), PropertySet.GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(L2RegularisationDescriptor), PropertySet.GRADIENT_DESCENT_DESCRIPTOR);

            // add the template based gradient descent descriptors
            _Add(typeof(AdaGradDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(AdamDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(MomentumDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(NesterovMomentumDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(RmsPropDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
        }

        /// <summary>
        /// Linear algebra provider
        /// </summary>
        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;

        /// <summary>
        /// The current property set
        /// </summary>
        public IPropertySet CurrentPropertySet
        {
            get { return _propertySetStack.Any() ? _propertySetStack.Peek() : _defaultPropertySet; }
        }

        /// <summary>
        /// Clones the current property set with an optional mutator and then pushes it onto the stack
        /// </summary>
        /// <param name="mutator">Callback that can modify the cloned property set</param>
        public void PushPropertySet(Action<IPropertySet> mutator)
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

        void _Add(Type type, string name)
        {
            _queryTypes.Add((type.GetTypeInfo(), type, name));
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
                ret = createTemplateGradientDescent.Create(ret ?? _simpleGradientDescent, weight, propertySet);

            return ret ?? _simpleGradientDescent;
        }

        IWeightInitialisation _GetWeightInitialisation()
        {
            var propertySet = CurrentPropertySet;
            var ret = propertySet.WeightInitialisation;
            return ret ?? WeightInitialisation.Gaussian;
        }

        /// <summary>
        /// Creates a graph execution context
        /// </summary>
        /// <returns></returns>
        public IExecutionContext CreateExecutionContext()
        {
            return new ExecutionContext(_lap);
        }

        /// <summary>
        /// Creates a graph learning context
        /// </summary>
        /// <param name="learningRate">Initial learning rate</param>
        /// <param name="batchSize">Mini batch size</param>
        /// <param name="trainingErrorCalculation">How to calculate the training error</param>
        /// <param name="deferUpdates">True to defer updates (used when training recurrent neural networks)</param>
        /// <returns></returns>
        public ILearningContext CreateLearningContext(float learningRate, int batchSize, TrainingErrorCalculation trainingErrorCalculation = TrainingErrorCalculation.Fast, bool deferUpdates = false)
        {
            return new LearningContext(_lap, learningRate, batchSize, trainingErrorCalculation, deferUpdates);
        }

        /// <summary>
        /// Creates a graph training engine
        /// </summary>
        /// <param name="dataSource">Data source with training data</param>
        /// <param name="learningRate">Initial learning rate</param>
        /// <param name="batchSize">Mini batch size</param>
        /// <param name="trainingErrorCalculation">How to calculate the training error</param>
        /// <returns></returns>
        public IGraphTrainingEngine CreateTrainingEngine(IDataSource dataSource, float learningRate = 0.1f, int batchSize = 128, TrainingErrorCalculation trainingErrorCalculation = TrainingErrorCalculation.Fast)
        {
            var learningContext = new LearningContext(_lap, learningRate, batchSize, trainingErrorCalculation, dataSource.IsSequential);
            return new TrainingEngine(_lap, dataSource, learningContext, null);
        }

        /// <summary>
        /// Creates a graph training engine
        /// </summary>
        /// <param name="dataSource">Data source with training data</param>
        /// <param name="graph">The serialised graph to execute</param>
        /// <param name="trainingRate">Initial learning rate</param>
        /// <param name="batchSize">Mini batch size</param>
        /// <param name="trainingErrorCalculation">How to calculate the training error</param>
        /// <returns></returns>
        public IGraphTrainingEngine CreateTrainingEngine(IDataSource dataSource, Models.ExecutionGraph graph, float trainingRate = 0.1f, int batchSize = 128, TrainingErrorCalculation trainingErrorCalculation = TrainingErrorCalculation.Fast)
        {
            var learningContext = new LearningContext(_lap, trainingRate, batchSize, trainingErrorCalculation, dataSource.IsSequential);
            var input = this.CreateFrom(graph);
            return new TrainingEngine(_lap, dataSource, learningContext, input);
        }

        /// <summary>
        /// Creates a graph training engine
        /// </summary>
        /// <param name="dataSource">Data source with training data</param>
        /// <param name="learningContext">Previously created training context</param>
        /// <param name="graph">The serialised graph to execute (optional)</param>
        /// <returns></returns>
        public IGraphTrainingEngine CreateTrainingEngine(IDataSource dataSource, ILearningContext learningContext, Models.ExecutionGraph graph = null)
        {
            var input = this.CreateFrom(graph);
            return new TrainingEngine(_lap, dataSource, learningContext, input);
        }

        /// <summary>
        /// Creates a graph execution engine
        /// </summary>
        /// <param name="graph">The serialised graph to execute</param>
        /// <returns></returns>
        public IGraphEngine CreateEngine(Models.ExecutionGraph graph)
        {
            var input = this.CreateFrom(graph);
            return new ExecutionEngine(_lap, graph, input);
        }

        /// <summary>
        /// Creates a data source from a list of vectors
        /// </summary>
        /// <param name="vectorList">The list of vectors that will be the rows in the data source</param>
        public IDataSource CreateDataSource(IReadOnlyList<FloatVector> vectorList)
        {
            return new VectorDataSource(_lap, vectorList);
        }

        /// <summary>
        /// Creates a data source from a list of matrices (sequential vectors)
        /// </summary>
        /// <param name="sequenceList">The list of matrices that will be the rows in the data source</param>
        /// <returns></returns>
        public IDataSource CreateDataSource(IReadOnlyList<FloatMatrix> sequenceList)
        {
            return new SequentialDataSource(_lap, sequenceList);
        }

        /// <summary>
        /// Creates a data source from a data table
        /// </summary>
        /// <param name="dataTable">The data table to convert</param>
        /// <param name="vectoriser">Optional data table vectoriser (if the data table contains categorical or index based data)</param>
        /// <returns></returns>
        public IDataSource CreateDataSource(IDataTable dataTable, IDataTableVectoriser vectoriser = null)
        {
            var columns = dataTable.Columns;
            var dataColumnTypes = columns
                .Where((c, i) => i != dataTable.TargetColumnIndex)
                .Select(c => c.Type)
                .ToList()
            ;
            var targetColumnType = columns[dataTable.TargetColumnIndex].Type;
            var firstDataColumnType = dataColumnTypes.FirstOrDefault();

            if (firstDataColumnType != ColumnType.Null && dataColumnTypes.All(ct => ct == firstDataColumnType)) {
                // many to many
                if (firstDataColumnType == ColumnType.Matrix && targetColumnType == ColumnType.Matrix)
                    return new SequentialDataTableAdaptor(_lap, dataTable);

                // one to one
                else if (firstDataColumnType == ColumnType.Vector && targetColumnType == ColumnType.Vector)
                    return new VectorBasedDataTableAdaptor(_lap, dataTable);

                // one to many
                else if (firstDataColumnType == ColumnType.Vector && targetColumnType == ColumnType.Matrix)
                    return new OneToManyDataTableAdaptor(_lap, dataTable);

                // many to one
                else if (firstDataColumnType == ColumnType.Matrix && targetColumnType == ColumnType.Vector)
                    return new ManyToOneDataTableAdaptor(_lap, dataTable);

                // volume classification
                else if (firstDataColumnType == ColumnType.Tensor && targetColumnType == ColumnType.Vector)
                    return new TensorBasedDataTableAdaptor(_lap, dataTable);

                // index list
                else if (firstDataColumnType == ColumnType.IndexList)
                    return new IndexListDataTableAdaptor(_lap, dataTable, vectoriser);

                // weighted index list
                else if (firstDataColumnType == ColumnType.WeightedIndexList)
                    return new WeightedIndexListDataTableAdaptor(_lap, dataTable, vectoriser);
            }
            
            // default adapator
            return new DefaultDataTableAdaptor(_lap, dataTable, vectoriser);
        }

        /// <summary>
        /// Creates an adaptive data source (that uses the output from a preliminary graph)
        /// </summary>
        /// <param name="dataTable">Data that will be sent to the preliminary graph</param>
        /// <param name="learningContext">Learning context to use while training the preliminary graph</param>
        /// <param name="dataConversionBuilder">Callback to build the preliminary graph</param>
        /// <returns></returns>
        public IDataSource CreateDataSource(IDataTable dataTable, ILearningContext learningContext, Action<WireBuilder> dataConversionBuilder)
        {
            var columns = dataTable.Columns;
            if (columns.Count == 2) {
                var column1 = columns[0].Type;
                var column2 = columns[1].Type;

                // sequence to sequence
                if (column1 == ColumnType.Matrix && column2 == ColumnType.Matrix)
                    return new SequenceToSequenceDataTableAdaptor(_lap, learningContext, this, dataTable, dataConversionBuilder);
            }
            throw new ArgumentException($"{nameof(dataTable)} does not contain a recognised data format");
        }

        /// <summary>
        /// Creates an adaptive data source from a serialised model
        /// </summary>
        /// <param name="dataTable">Data that will be sent to the preliminary graph</param>
        /// <param name="dataSource">The serialised preliminary graph</param>
        /// <param name="learningContext">Learning context to use while training the preliminary graph</param>
        /// <returns></returns>
        public IDataSource CreateDataSource(IDataTable dataTable, DataSourceModel dataSource, ILearningContext learningContext = null)
        {
            var input = this.CreateFrom(dataSource.Graph);

            var columns = dataTable.Columns;
            if (columns.Count == 2) {
                var column1 = columns[0].Type;
                var column2 = columns[1].Type;

                if (column1 == ColumnType.Matrix && column2 == ColumnType.Matrix)
                    return new SequenceToSequenceDataTableAdaptor(_lap, learningContext, dataTable, input, dataSource);
            }
            throw new ArgumentException($"{nameof(dataTable)} does not contain a recognised data format");
        }

        /// <summary>
        /// Create a row classifier node
        /// </summary>
        /// <param name="classifier">The classifier for each row</param>
        /// <param name="dataTable">The data table that contains the rows to classify (linked by mini batch index)</param>
        /// <param name="analysis">Optional data table analysis data</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public (INode RowClassifier, int OutputSize) CreateClassifier(IRowClassifier classifier, IDataTable dataTable, IDataTableAnalysis analysis = null, string name = null)
        {
            var ret = new RowClassifier(_lap, classifier, dataTable, analysis ?? dataTable.GetAnalysis(), name);
            return (ret, ret.OutputSize);
        }

        /// <summary>
        /// Creates a feed forward layer
        /// </summary>
        /// <param name="inputSize">Number of incoming connections</param>
        /// <param name="outputSize">Number of outgoing connections</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateFeedForward(int inputSize, int outputSize, string name = null)
        {
            // create weights and bias
            var weightInit = _GetWeightInitialisation();
            var bias = weightInit.CreateBias(outputSize);
            var weight = weightInit.CreateWeight(inputSize, outputSize);

            // get the gradient descent optimisations
            var optimisation = CreateWeightUpdater( weight);

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
        public INode CreateDropConnect(float dropoutPercentage, int inputSize, int outputSize, string name = null)
        {
            // create weights and bias
            var weightInit = _GetWeightInitialisation();
            var bias = weightInit.CreateBias(outputSize);
            var weight = weightInit.CreateWeight(inputSize, outputSize);

            // get the gradient descent optimisations
            var optimisation = CreateWeightUpdater(weight);

            return new DropConnect(dropoutPercentage, inputSize, outputSize, bias, weight, _lap.IsStochastic, optimisation, name);
        }

        /// <summary>
        /// Creates a layer whose weights are shared with another layer (but transposed)
        /// </summary>
        /// <param name="layer">The layer that shares weights</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateTiedFeedForward(IFeedForward layer, string name = null)
        {
            var weightInit = _GetWeightInitialisation();
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
        /// <param name="stride">Convolutional stride</param>
        /// <param name="shouldBackpropagate">True to backpropagate an error signal</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateConvolutional(int inputDepth, int filterCount, int padding, int filterWidth, int filterHeight, int stride, bool shouldBackpropagate = true, string name = null)
        {
            var weightInit = _GetWeightInitialisation();
            return new Convolutional(shouldBackpropagate, weightInit, weight => CreateWeightUpdater(weight), inputDepth, filterCount, padding, filterWidth, filterHeight, stride, name);
        }

        /// <summary>
        /// Creates a max pooling convolutional layer
        /// </summary>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="stride"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateMaxPool(int filterWidth, int filterHeight, int stride, string name = null)
        {
            return new MaxPool(filterWidth, filterHeight, stride, name);
        }

        /// <summary>
        /// Creates a simple recurrent layer
        /// </summary>
        /// <param name="inputSize">Number of incoming connections</param>
        /// <param name="memory">Size of the layer memory</param>
        /// <param name="activation">Activation layer</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateSimpleRecurrent(int inputSize, float[] memory, INode activation, string name = null)
        {
            return new SimpleRecurrent(this, inputSize, memory, activation, name);
        }

        /// <summary>
        /// Creates an Elman recurrent layer
        /// </summary>
        /// <param name="inputSize">Number of incoming connections</param>
        /// <param name="memory">Size of the layer memory</param>
        /// <param name="activation">First activation layer</param>
        /// <param name="activation2">Second activation layer</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateElman(int inputSize, float[] memory, INode activation, INode activation2, string name = null)
        {
            return new ElmanJordan(this, true, inputSize, memory, activation, activation2, name);
        }

        /// <summary>
        /// Creates a Jordan recurrent layer
        /// </summary>
        /// <param name="inputSize">Number of incoming connections</param>
        /// <param name="memory">Size of the layer memory</param>
        /// <param name="activation">First activation layer</param>
        /// <param name="activation2">Second activation layer</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateJordan(int inputSize, float[] memory, INode activation, INode activation2, string name = null)
        {
            return new ElmanJordan(this, false, inputSize, memory, activation, activation2, name);
        }

        /// <summary>
        /// Creates a node that subtracts each input from 1 (1-x)
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateOneMinusInput(string name = null)
        {
            return new OneMinusInput(name);
        }

        /// <summary>
        /// Creates a node that outputs the reversed index of the current sequence (for bidirectional recurrent networks)
        /// </summary>
        /// <param name="index">Input index to reverse</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateSequenceReverser(int index = 0, string name = null)
        {
            return new ReverseSequence(index, name);
        }

        /// <summary>
        /// Creates a GRU recurrent layer
        /// </summary>
        /// <param name="inputSize">Number of incoming connections</param>
        /// <param name="memory">Size of the layer memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateGru(int inputSize, float[] memory, string name = null)
        {
            return new GatedRecurrentUnit(this, inputSize, memory, name);
        }

        /// <summary>
        /// Creates a LSTM recurrent layer
        /// </summary>
        /// <param name="inputSize">Number of incoming connections</param>
        /// <param name="memory">Size of the layer memory</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateLstm(int inputSize, float[] memory, string name = null)
        {
            return new LongShortTermMemory(this, inputSize, memory, name);
        }

        /// <summary>
        /// Creates a layer that drops random connections
        /// </summary>
        /// <param name="dropoutPercentage">Percentage to drop (0..1)</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public INode CreateDropOut(float dropoutPercentage, string name = null)
        {
            return new DropOut(dropoutPercentage, _lap.IsStochastic, name);
        }

        /// <summary>
        /// Builds a new wire from the engine's input node
        /// </summary>
        /// <param name="engine">Graph engine to build with</param>
        /// <param name="inputIndex">Input index to connect to</param>
        /// <returns></returns>
        public WireBuilder Connect(IGraphTrainingEngine engine, int inputIndex = 0)
        {
            return new WireBuilder(this, engine);
        }

        /// <summary>
        /// Builds a new wire from the selected node
        /// </summary>
        /// <param name="inputSize">Number of outgoing connections</param>
        /// <param name="node">The node to build from</param>
        /// <returns></returns>
        public WireBuilder Connect(int inputSize, INode node)
        {
            return new WireBuilder(this, inputSize, node);
        }

        /// <summary>
        /// Builds a new wire from the selected node
        /// </summary>
        /// <param name="width">Volume width</param>
        /// <param name="height">Volume height</param>
        /// <param name="depth">Volume depth</param>
        /// <param name="node">The node to build from</param>
        /// <returns></returns>
        public WireBuilder Connect(int width, int height, int depth, INode node)
        {
            return new WireBuilder(this, width, height, depth, node);
        }

        /// <summary>
        /// Adds the output of two wires into a new wire
        /// </summary>
        /// <param name="input1">First wire</param>
        /// <param name="input2">Second wire</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Add(WireBuilder input1, WireBuilder input2, string name = null)
        {
            Debug.Assert(input1.CurrentSize == input2.CurrentSize);
            return Add(input1.CurrentSize, input1.LastNode, input2.LastNode, name);
        }

        /// <summary>
        /// Adds the output of two nodes together into a new wire
        /// </summary>
        /// <param name="inputSize">The number of connections</param>
        /// <param name="input1">First node</param>
        /// <param name="input2">Second node</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Add(int inputSize, INode input1, INode input2, string name = null)
        {
            var add = new AddGate(name);
            var wireToPrimary = new WireToNode(add);
            var wireToSecondary = new WireToNode(add, 1);

            input1.Output.Add(wireToPrimary);
            input2.Output.Add(wireToSecondary);

            return new WireBuilder(this, inputSize, add);
        }

        /// <summary>
        /// Multiplies the output of two wires into a new wire
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Multiply(WireBuilder input1, WireBuilder input2, string name = null)
        {
            Debug.Assert(input1.CurrentSize == input2.CurrentSize);
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
        public WireBuilder Multiply(int inputSize, INode input1, INode input2, string name = null)
        {
            var multiply = new MultiplyGate(name);
            var wireToPrimary = new WireToNode(multiply);
            var wireToSecondary = new WireToNode(multiply, 1);

            input1.Output.Add(wireToPrimary);
            input2.Output.Add(wireToSecondary);

            return new WireBuilder(this, inputSize, multiply);
        }

        /// <summary>
        /// Concatenates two wires together into a new wire
        /// </summary>
        /// <param name="input1">First wire to join</param>
        /// <param name="input2">Second wire to join</param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Join(WireBuilder input1, WireBuilder input2, string name = null)
        {
            var ret = new JoinGate(name, input1, input2);
            var wireToPrimary = new WireToNode(ret);
            var wireToSecondary = new WireToNode(ret, 1);

            input1.LastNode.Output.Add(wireToPrimary);
            input2.LastNode.Output.Add(wireToSecondary);

            return new WireBuilder(this, input1.CurrentSize + input2.CurrentSize, ret);
        }

        /// <summary>
        /// Wraps an existing node, enabling that node to be used at multiple locations in the same graph
        /// </summary>
        /// <param name="nodeToWrap">Node to wrap</param>
        /// <param name="name">Optional name to give the wrapping node</param>
        /// <returns></returns>
        public INode CreateWrapper(INode nodeToWrap, string name = null)
        {
            return new NodeWrapper(nodeToWrap, name);
        }

        /// <summary>
        /// Creates a node from it's serialised model
        /// </summary>
        /// <param name="node">The node model</param>
        public INode Create(Models.ExecutionGraph.Node node)
        {
            var type = Type.GetType(node.TypeName);
            var ret = (INode)FormatterServices.GetUninitializedObject(type);
            ret.Initialise(this, node.Id, node.Name, node.Description, node.Data);
            return ret;
        }

        /// <summary>
        /// Creates a new leaky relu activation layer
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        public INode LeakyReluActivation(string name = null) => new LeakyRelu(name);

        /// <summary>
        /// Creates a new relu activation layer
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        public INode ReluActivation(string name = null) => new Relu(name);

        /// <summary>
        /// Creates a new sigmoid activation layer
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        public INode SigmoidActivation(string name = null) => new Sigmoid(name);

        /// <summary>
        /// Creates a new tanh activation layer
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        public INode TanhActivation(string name = null) => new Tanh(name);

        /// <summary>
        /// Creates a new softmax activation layer
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        public INode SoftMaxActivation(string name = null) => new SoftMax(name);

        /// <summary>
        /// Creates a constant weight initialiser
        /// </summary>
        /// <param name="biasValue">Single bias value</param>
        /// <param name="weightValue">Single weight value</param>
        public IWeightInitialisation ConstantWeightInitialisation(float biasValue = 0f, float weightValue = 1f) => new Constant(_lap, biasValue, weightValue);

        /// <summary>
        /// Creates a gaussian weight initialiser
        /// </summary>
        /// <param name="zeroBias">True to set bias values to zero, otherwise bias initialisation is treated the same as weight initialisation</param>
        /// <param name="stdDev">Standard deviation of gaussian distribution</param>
        public IWeightInitialisation GaussianWeightInitialisation(bool zeroBias = true, float stdDev = 0.1f) => new Gaussian(_lap, zeroBias, stdDev);

        /// <summary>
        /// Creates an identity weight initialiser
        /// </summary>
        /// <param name="identityValue">The value to give to each diagonal value</param>
        public IWeightInitialisation IdentityWeightInitialisation(float identityValue = 1f) => new Identity(_lap, identityValue);

        /// <summary>
        /// Creates a xavier weight initialiser
        /// </summary>
        /// <param name="parameter">Xavier parameter</param>
        public IWeightInitialisation XavierWeightInitialisation(float parameter = 6) => new Xavier(_lap, parameter);

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
        /// Creats a momentum gradient descent optimiser
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
        public RegularisationProvider Regularisation { get; } = new RegularisationProvider();

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
            public ICreateTemplateBasedGradientDescent Adam { get; } = new AdamDescriptor(0.9f, 0.99f);

            /// <summary>
            /// Momentum gradient descent
            /// </summary>
            public ICreateTemplateBasedGradientDescent Momentum { get; } = new MomentumDescriptor(0.9f);

            /// <summary>
            /// Nesterov momentum gradient descent
            /// </summary>
            public ICreateTemplateBasedGradientDescent NesterovMomentum { get; } = new NesterovMomentumDescriptor(0.9f);

            /// <summary>
            /// Rms prop gradient descent
            /// </summary>
            public ICreateTemplateBasedGradientDescent RmsProp { get; } = new RmsPropDescriptor(0.9f);
        }
        /// <summary>
        /// Prebuilt gradient descent optimisers
        /// </summary>
        public GradientDescentProvider GradientDescent { get; } = new GradientDescentProvider();

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
        public ErrorMetricProvider ErrorMetric { get; } = new ErrorMetricProvider();

        /// <summary>
        /// Prebuilt weight initialisers
        /// </summary>
        public class WeightInitialisationProvider
        {
            /// <summary>
            /// All weights are initialised to 1
            /// </summary>
            public IWeightInitialisation Ones { get; private set; }

            /// <summary>
            /// All weights are initialised to 0
            /// </summary>
            public IWeightInitialisation Zeroes { get; private set; }

            /// <summary>
            /// Weights are randomly initialised using a gaussian distribution
            /// </summary>
            public IWeightInitialisation Gaussian { get; private set; }

            /// <summary>
            /// Weights are randomly initialised using the xavier algorithm
            /// </summary>
            public IWeightInitialisation Xavier { get; private set; }

            /// <summary>
            /// Weights are initialised to the identity matrix
            /// </summary>
            public IWeightInitialisation Identity { get; private set; }

            /// <summary>
            /// Weights are initialised to the identity matrix / 10
            /// </summary>
            public IWeightInitialisation Identity01 { get; private set; }

            internal WeightInitialisationProvider(ILinearAlgebraProvider lap)
            {
                Ones = new Constant(lap, 0f, 1f);
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
        public WeightInitialisationProvider WeightInitialisation { get; private set; }
    }
}
