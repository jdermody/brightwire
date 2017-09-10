using BrightWire.ExecutionGraph;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace BrightWire
{
    /// <summary>
    /// Wrapper around the data that is used as a signal between nodes in the graph
    /// </summary>
    public interface IGraphData
    {
        /// <summary>
        /// Row count
        /// </summary>
        int Rows { get; }

        /// <summary>
        /// Column count
        /// </summary>
        int Columns { get; }

        /// <summary>
        /// 3D Tensor depth (1 if the signal is a matrix)
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Count of 3D tensors (1 of the signal is a matrix or 3D tensor)
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the signal as a matrix
        /// </summary>
        /// <returns></returns>
        IMatrix GetMatrix();

        /// <summary>
        /// Gets the signal as a 4D tensor
        /// </summary>
        /// <returns></returns>
        I4DTensor Get4DTensor();

        /// <summary>
        /// Replaces the data with the specified matrix (but preserves any tensor meta data)
        /// </summary>
        /// <param name="matrix">The matrix to use as a replacement</param>
        IGraphData ReplaceWith(IMatrix matrix);

        /// <summary>
        /// Returns the list of matrices that compose the signal (single item if the signal is a matrix)
        /// </summary>
        IReadOnlyList<IMatrix> GetSubMatrices();
    }

    /// <summary>
    /// An action to perform when a signal reaches a node
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Executes the action
        /// </summary>
        /// <param name="input">Current graph signal</param>
        /// <param name="context">Graph context</param>
        /// <returns>Optional new graph signal to propagate</returns>
        IGraphData Execute(IGraphData input, IContext context);
        
        /// <summary>
        /// Serialises the action to a string
        /// </summary>
        string Serialise();

        /// <summary>
        /// Initialises the action
        /// </summary>
        /// <param name="data">Previously serialised data</param>
        void Initialise(string data);
    }

    /// <summary>
    /// Interface that allows the node to be initialised
    /// </summary>
    public interface ICanInitialiseNode
    {
        /// <summary>
        /// Initialise the node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="id">Node unique id</param>
        /// <param name="name">Friendly name</param>
        /// <param name="description">Node description</param>
        /// <param name="data">Serialisation data</param>
        void Initialise(GraphFactory factory, string id, string name, string description, byte[] data);
    }

    /// <summary>
    /// Serialisation interface for graph components
    /// </summary>
    public interface ICanSerialise
    {
        /// <summary>
        /// Writes the node state to the binary writer
        /// </summary>
        void WriteTo(BinaryWriter writer);

        /// <summary>
        /// Reads the node state
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="reader">Binary reader that holds the node's state</param>
        void ReadFrom(GraphFactory factory, BinaryReader reader);
    }

    /// <summary>
    /// Graph node
    /// </summary>
    public interface INode : ICanInitialiseNode, IDisposable, ICanSerialise
    {
        /// <summary>
        /// Unique id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Friendly name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// List of outgoing wires
        /// </summary>
        List<IWire> Output { get; }

        /// <summary>
        /// Executes the node forward
        /// </summary>
        /// <param name="context">Graph context</param>
        /// <param name="channel">Channel the signal was received on</param>
        void ExecuteForward(IContext context, int channel);

        /// <summary>
        /// Searches for a node by friendly name
        /// </summary>
        /// <param name="name">Friendly name of the node to find</param>
        /// <returns></returns>
        INode FindByName(string name);

        /// <summary>
        /// Searches for a node by id
        /// </summary>
        /// <param name="id">Unique id of the node</param>
        /// <returns></returns>
        INode FindById(string id);

        /// <summary>
        /// Sub-nodes of the current node
        /// </summary>
        IEnumerable<INode> SubNodes { get; }

        /// <summary>
        /// Serialise the node
        /// </summary>
        /// <param name="existing">Set of nodes that have already been serialised in the current context</param>
        /// <param name="connectedTo">List of nodes this node is connected to</param>
        /// <param name="wireList">List of wires between all connected nodes</param>
        /// <returns>Serialisation model</returns>
        Models.ExecutionGraph.Node SerialiseTo(HashSet<INode> existing, List<Models.ExecutionGraph.Node> connectedTo, HashSet<Models.ExecutionGraph.Wire> wireList);

        /// <summary>
        /// Called after the graph has been completely deserialised
        /// </summary>
        /// <param name="graph">Dictionary of nodes with their associated unique ids</param>
        void OnDeserialise(IReadOnlyDictionary<string, INode> graph);

        /// <summary>
        /// Loads parameters into an existing node
        /// </summary>
        /// <param name="nodeData">Serialised node parameters</param>
        void LoadParameters(Models.ExecutionGraph.Node nodeData);
    }

    /// <summary>
    /// Wires connect nodes in the graph
    /// </summary>
    public interface IWire
    {
        /// <summary>
        /// The node to send a signal to
        /// </summary>
        INode SendTo { get; }

        /// <summary>
        /// The channel
        /// </summary>
        int Channel { get; }
    }

    /// <summary>
    /// Record of node execution
    /// </summary>
    public interface IExecutionHistory
    {
        /// <summary>
        /// Node that was executed
        /// </summary>
        INode Source { get; }

        /// <summary>
        /// The node's parents
        /// </summary>
        IReadOnlyList<INode> Parents { get; }

        /// <summary>
        /// Node output signal
        /// </summary>
        IGraphData Data { get; }

        /// <summary>
        /// Optional backpropagation
        /// </summary>
        IBackpropagation Backpropagation { get; set; }
    }

    /// <summary>
    /// Graph context
    /// </summary>
    public interface IContext : IDisposable
    {
        /// <summary>
        /// True if the graph is currently training
        /// </summary>
        bool IsTraining { get; }

        /// <summary>
        /// Node that sent the current signal
        /// </summary>
        INode Source { get; }

        /// <summary>
        /// Current signal
        /// </summary>
        IGraphData Data { get; }

        /// <summary>
        /// Current execution context
        /// </summary>
        IExecutionContext ExecutionContext { get; }

        /// <summary>
        /// Current learning context (optional)
        /// </summary>
        ILearningContext LearningContext { get; }

        /// <summary>
        /// Linear algebra provider
        /// </summary>
        ILinearAlgebraProvider LinearAlgebraProvider { get; }

        /// <summary>
        /// Current mini batch sequence
        /// </summary>
        IMiniBatchSequence BatchSequence { get; }

        /// <summary>
        /// Records node execution
        /// </summary>
        /// <param name="action">Record of node execution</param>
        /// <param name="callback">Optional callback to add backpropagation</param>
        void AddForward(IExecutionHistory action, Func<IBackpropagation> callback);

        /// <summary>
        /// Sends a backward error signal
        /// </summary>
        /// <param name="errorSignal">Error signal</param>
        /// <param name="target">Node to send to</param>
        /// <param name="source">Node that sent the error</param>
        void AddBackward(IGraphData errorSignal, INode target, INode source);

        /// <summary>
        /// Records an error signal against a node
        /// </summary>
        /// <param name="errorSignal">Error signal</param>
        /// <param name="forNode">Node to record against</param>
        void AppendErrorSignal(IGraphData errorSignal, INode forNode);

        /// <summary>
        /// Backpropagates the signal
        /// </summary>
        /// <param name="delta">Error signal</param>
        void Backpropagate(IGraphData delta);

        /// <summary>
        /// Current error signal
        /// </summary>
        IGraphData ErrorSignal { get; }

        /// <summary>
        /// Checks if there is a pending forward graph operation
        /// </summary>
        bool HasNext { get; }

        /// <summary>
        /// Executes the next pending forward graph operation
        /// </summary>
        /// <returns>True if an operation was executed</returns>
        bool ExecuteNext();
    }

    /// <summary>
    /// Graph execution context
    /// </summary>
    public interface IExecutionContext : IDisposable
    {
        /// <summary>
        /// Writes to a named memory slot
        /// </summary>
        /// <param name="slotName">Slot name</param>
        /// <param name="memory">Data</param>
        void SetMemory(string slotName, IMatrix memory);

        /// <summary>
        /// Reads from a named memory slot
        /// </summary>
        /// <param name="slotName">Slot name</param>
        /// <returns></returns>
        IMatrix GetMemory(string slotName);

        /// <summary>
        /// Gets the next queued graph operation (if any)
        /// </summary>
        /// <returns></returns>
        IGraphOperation GetNextOperation();

        /// <summary>
        /// Adds a list of graph operations to the queue
        /// </summary>
        /// <param name="operationList">List of operations</param>
        void Add(IReadOnlyList<IGraphOperation> operationList);

        /// <summary>
        /// Linear algebra provider
        /// </summary>
        ILinearAlgebraProvider LinearAlgebraProvider { get; }

        /// <summary>
        /// How many operations remain queued
        /// </summary>
        int RemainingOperationCount { get; }

        /// <summary>
        /// Registers a continuation that will be executed after the current sequence has been processed in full
        /// </summary>
        /// <param name="sequence">Sequence index</param>
        /// <param name="callback">Continuation</param>
        void RegisterContinuation(IMiniBatchSequence sequence, Action<IContext> callback);

        /// <summary>
        /// True if there are registered continuations
        /// </summary>
        bool HasContinuations { get; }

        /// <summary>
        /// Execute any registered continuation for this context
        /// </summary>
        /// <param name="context">Context with an associated IMiniBatchSequence</param>
        void Continue(IContext context);
    }

    /// <summary>
    /// Backpropagation handler
    /// </summary>
    public interface IBackpropagation : IDisposable
    {
        /// <summary>
        /// Backpropagate
        /// </summary>
        /// <param name="fromNode">Node that sent the signal</param>
        /// <param name="errorSignal">Error signal</param>
        /// <param name="context">Graph context</param>
        /// <param name="parents">The current node's parents</param>
        void Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents);
    }

    /// <summary>
    /// Data sources feed data into a graph
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// True if the data is sequential
        /// </summary>
        bool IsSequential { get; }

        /// <summary>
        /// The size of the input data
        /// </summary>
        int InputSize { get; }

        /// <summary>
        /// The size of the output data
        /// </summary>
        int OutputSize { get; }

        /// <summary>
        /// The number of inputs that can feed into the graph
        /// </summary>
        int InputCount { get; }

        /// <summary>
        /// Number of rows
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Gets a mini batch with the specified rows
        /// </summary>
        /// <param name="executionContext">Graph execution context</param>
        /// <param name="rows">List of rows</param>
        IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows);

        /// <summary>
        /// For sequential data, returns the row indexes grouped by sequence length
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<IReadOnlyList<int>> GetBuckets();

        /// <summary>
        /// Called when the current batch has completed
        /// </summary>
        /// <param name="context"></param>
        void OnBatchProcessed(IContext context);

        /// <summary>
        /// Creates a new data source, using the current as a template but replacing the data table
        /// </summary>
        /// <param name="dataTable">The new data table</param>
        /// <returns></returns>
        IDataSource CloneWith(IDataTable dataTable);
    }

    /// <summary>
    /// Adaptive data sources apply the output from a preliminary graph
    /// </summary>
    public interface IAdaptiveDataSource
    {
        /// <summary>
        /// The input node of the preliminary graph
        /// </summary>
        INode AdaptiveInput { get; }

        /// <summary>
        /// Gets the serialised preliminary graph
        /// </summary>
        /// <param name="name">Optional name to give the data source</param>
        /// <returns></returns>
        DataSourceModel GetModel(string name = null);
    }

    /// <summary>
    /// Mini batch type
    /// </summary>
    public enum MiniBatchSequenceType
    {
        /// <summary>
        /// Standard batch type (non sequential batches have a single standard sequence item)
        /// </summary>
        Standard,

        /// <summary>
        /// Start of a sequence
        /// </summary>
        SequenceStart,

        /// <summary>
        /// End of a sequence
        /// </summary>
        SequenceEnd
    }

    /// <summary>
    /// A sequence within a mini batch
    /// </summary>
    public interface IMiniBatchSequence
    {
        /// <summary>
        /// Mini batch
        /// </summary>
        IMiniBatch MiniBatch { get; }

        /// <summary>
        /// Index of the sequence
        /// </summary>
        int SequenceIndex { get; }

        /// <summary>
        /// Sequence type
        /// </summary>
        MiniBatchSequenceType Type { get; }

        /// <summary>
        /// Input data
        /// </summary>
        IReadOnlyList<IGraphData> Input { get; }

        /// <summary>
        /// Training target data
        /// </summary>
        IGraphData Target { get; }
    }

    /// <summary>
    /// Mini batch
    /// </summary>
    public interface IMiniBatch
    {
        /// <summary>
        /// Row indexes of the current batch
        /// </summary>
        IReadOnlyList<int> Rows { get; }

        /// <summary>
        /// Data source
        /// </summary>
        IDataSource DataSource { get; }

        /// <summary>
        /// True if the data is sequential
        /// </summary>
        bool IsSequential { get; }

        /// <summary>
        /// Number of items in the batch
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        /// Current sequence (non sequential batches have a single sequence)
        /// </summary>
        IMiniBatchSequence CurrentSequence { get; }

        /// <summary>
        /// True if there is another item in the sequence after the current item
        /// </summary>
        bool HasNextSequence { get; }

        /// <summary>
        /// Gets the next item in the sequence
        /// </summary>
        /// <returns></returns>
        IMiniBatchSequence GetNextSequence();

        /// <summary>
        /// Gets the length of the sequence
        /// </summary>
        int SequenceCount { get; }

        /// <summary>
        /// Gets a sequence item
        /// </summary>
        /// <param name="index">The index to retrieve</param>
        /// <returns></returns>
        IMiniBatchSequence GetSequenceAtIndex(int index);

        /// <summary>
        /// Resets the sequence iterator
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// A pending graph operation (mini batch)
    /// </summary>
    public interface IGraphOperation
    {
        /// <summary>
        /// Executes the operation
        /// </summary>
        /// <param name="executionContext">Graph execution context</param>
        void Execute(IExecutionContext executionContext);
    }

    /// <summary>
    /// Graph engines drive execution within a graph
    /// </summary>
    public interface IGraphEngine
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        ILinearAlgebraProvider LinearAlgebraProvider { get; }

        /// <summary>
        /// Serialised version of the current graph and its parameters
        /// </summary>
        Models.ExecutionGraph Graph { get; }

        /// <summary>
        /// Data source that feeds into the graph
        /// </summary>
        IDataSource DataSource { get; }

        /// <summary>
        /// Executes a data source on the current graph
        /// </summary>
        /// <param name="dataSource">Data source to process</param>
        /// <param name="batchSize">Initial size of each mini batch</param>
        /// <param name="batchCompleteCallback">Optional callback to be notifiied after each mini batch has completed</param>
        /// <returns></returns>
        IReadOnlyList<ExecutionResult> Execute(IDataSource dataSource, int batchSize = 128, Action<float> batchCompleteCallback = null);

        /// <summary>
        /// Executes a single vector on the current graph
        /// </summary>
        /// <param name="input">Vector to execute</param>
        /// <returns></returns>
        ExecutionResult Execute(float[] input);

        /// <summary>
        /// Executes a sequential input on the current graph
        /// </summary>
        /// <param name="sequenceIndex">Index of the current sequence (starting from 0)</param>
        /// <param name="input">Input vector</param>
        /// <param name="executionContext">Graph execution context</param>
        /// <param name="sequenceType">The sequence type (start, standard, end)</param>
        /// <returns></returns>
        ExecutionResult ExecuteSequential(int sequenceIndex, float[] input, IExecutionContext executionContext, MiniBatchSequenceType sequenceType);

        /// <summary>
        /// The graph's single start node
        /// </summary>
        INode Start { get; }
    }

    /// <summary>
    /// A graph engine that also trains the graph's parameters against training data
    /// </summary>
    public interface IGraphTrainingEngine : IGraphEngine
    {
        /// <summary>
        /// Returns the specified input node
        /// </summary>
        /// <param name="index">Index of the input node to retrieve</param>
        INode GetInput(int index);

        /// <summary>
        /// Executes a training epoch on the graph
        /// </summary>
        /// <param name="executionContext">Graph execution context</param>
        /// <param name="batchCompleteCallback">Optional callback to be notifiied after each mini batch has completed</param>
        /// <returns>Graph training error</returns>
        double Train(IExecutionContext executionContext, Action<float> batchCompleteCallback = null);

        /// <summary>
        /// Executes test data on the current graph
        /// </summary>
        /// <param name="testDataSource">Data source with test data</param>
        /// <param name="errorMetric">Error metric to use to evaluate the test score</param>
        /// <param name="batchSize">Initial size of each mini batch</param>
        /// <param name="batchCompleteCallback">Optional callback to be notifiied after each mini batch has completed</param>
        /// <returns>True if the model performance has improved since the last test</returns>
        bool Test(IDataSource testDataSource, IErrorMetric errorMetric, int batchSize = 128, Action<float> batchCompleteCallback = null);

        /// <summary>
        /// Graph learning context
        /// </summary>
        ILearningContext LearningContext { get; }

        /// <summary>
        /// Loads model parameters into the existing graph
        /// </summary>
        /// <param name="graph">Model to load parameters from</param>
        void LoadParametersFrom(Models.ExecutionGraph graph);
    }

    /// <summary>
    /// Nodes that have a memory feeder sub-node
    /// </summary>
    public interface IHaveMemoryNode
    {
        /// <summary>
        /// The memory feed sub node
        /// </summary>
        INode Memory { get; }
    }

    /// <summary>
    /// Feed forward layer
    /// </summary>
    public interface IFeedForward : INode
    {
        /// <summary>
        /// Size of incoming connections
        /// </summary>
        int InputSize { get; }

        /// <summary>
        /// Size of outgoing connections
        /// </summary>
        int OutputSize { get; }

        /// <summary>
        /// Bias vector
        /// </summary>
        IVector Bias { get; }

        /// <summary>
        /// Weight matrix
        /// </summary>
        IMatrix Weight { get; }

        /// <summary>
        /// Updates the weights
        /// </summary>
        /// <param name="delta">Weight delta matrix</param>
        /// <param name="context">Graph learning context</param>
        void UpdateWeights(IMatrix delta, ILearningContext context);
    }


    /// <summary>
    /// Node that exposes an action
    /// </summary>
    public interface IHaveAction
    {
        /// <summary>
        /// The node's action
        /// </summary>
        IAction Action { get; }
    }

    /// <summary>
    /// Volume (3D tensor) based data sources
    /// </summary>
    public interface IVolumeDataSource
    {
        /// <summary>
        /// Width of each input volume
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Height of each input volume
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Depth of each input volume
        /// </summary>
        int Depth { get; }
    }
}
