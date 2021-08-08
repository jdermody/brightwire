using BrightWire.ExecutionGraph;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;

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
        uint Rows { get; }

        /// <summary>
        /// Column count
        /// </summary>
        uint Columns { get; }

        /// <summary>
        /// 3D Tensor depth (1 if the signal is a matrix)
        /// </summary>
        uint Depth { get; }

        /// <summary>
        /// Count of 3D tensors (1 of the signal is a matrix or 3D tensor)
        /// </summary>
        uint Count { get; }

        /// <summary>
        /// Gets the signal as a matrix
        /// </summary>
        /// <returns></returns>
        IFloatMatrix GetMatrix();

        /// <summary>
        /// Gets the signal as a 4D tensor
        /// </summary>
        /// <returns></returns>
        I4DFloatTensor? Get4DTensor();

        /// <summary>
        /// Replaces the data with the specified matrix (but preserves any tensor meta data)
        /// </summary>
        /// <param name="matrix">The matrix to use as a replacement</param>
        IGraphData ReplaceWith(IFloatMatrix matrix);

        /// <summary>
        /// Returns the value at the specified index
        /// </summary>
        /// <param name="index"></param>
        float this[uint index] { get; }

        /// <summary>
        /// True if this graph data has been set (false for null)
        /// </summary>
        public bool HasValue { get; }
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
        /// <param name="node"></param>
        /// <returns>Optional new graph signal to propagate</returns>
        IGraphData Execute(IGraphData input, IGraphSequenceContext context, NodeBase node);

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
        void Initialise(GraphFactory factory, string id, string? name, string? description, byte[]? data);
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
    /// Represents a single pass through the graph, from a single mini batch sequence
    /// </summary>
    public interface IGraphSequenceContext : IDisposable
    {
        /// <summary>
        /// Current signal
        /// </summary>
        IGraphData Data { get; set; }

        /// <summary>
        /// Current execution context
        /// </summary>
        IGraphExecutionContext ExecutionContext { get; }

        /// <summary>
        /// Current learning context (optional)
        /// </summary>
        ILearningContext? LearningContext { get; }

        /// <summary>
        /// Linear algebra provider
        /// </summary>
        ILinearAlgebraProvider LinearAlgebraProvider => ExecutionContext.LinearAlgebraProvider;

        /// <summary>
        /// Current mini batch sequence
        /// </summary>
        IMiniBatchSequence BatchSequence { get; }

        /// <summary>
        /// Records forward node execution
        /// </summary>
        /// <param name="source">Node that was executed</param>
        /// <param name="data">Output from the node</param>
        /// <param name="callback">Optional callback to add backpropagation</param>
        /// <param name="prev">Ancestors that fed input into the node</param>
        public void AddForwardHistory(NodeBase source, IGraphData data, Func<IBackpropagate>? callback, params NodeBase[] prev);

        /// <summary>
        /// Backpropagates the signal
        /// </summary>
        /// <param name="delta">Error signal</param>
        IGraphData? Backpropagate(IGraphData? delta);

        /// <summary>
        /// Final error signal
        /// </summary>
        IGraphData? ErrorSignal { get; }

        /// <summary>
        /// Saves the data as an output of the graph
        /// </summary>
        /// <param name="data">Segment to save</param>
        /// <param name="channel">Channel to save against (optional)</param>
        void SetOutput(IGraphData data, int channel = 0);

        /// <summary>
        /// Returns a saved output
        /// </summary>
        /// <param name="channel">Output channel (optional)</param>
        IGraphData? GetOutput(int channel = 0);

        /// <summary>
        /// Returns all stored output
        /// </summary>
        IGraphData[] Output { get; }

        /// <summary>
        /// Execution results
        /// </summary>
        IEnumerable<ExecutionResult> Results { get; }

        /// <summary>
        /// Resets the context for another run of backpropagation
        /// </summary>
        void ClearForBackpropagation();

        /// <summary>
        /// Stores sequence specific data
        /// </summary>
        /// <param name="name">Source node name</param>
        /// <param name="type">Data type</param>
        /// <param name="data"></param>
        void SetData(string name, string type, IGraphData data);

        /// <summary>
        /// Retrieves sequence specific data
        /// </summary>
        /// <param name="type">Data type</param>
        /// <returns></returns>
        IEnumerable<(string Name, IGraphData Data)> GetData(string type);
    }

    /// <summary>
    /// Graph execution context
    /// </summary>
    public interface IGraphExecutionContext : IDisposable
    {
        /// <summary>
        /// Writes to a named memory slot
        /// </summary>
        /// <param name="slotName">Slot name</param>
        /// <param name="memory">Segment</param>
        void SetMemory(string slotName, IFloatMatrix memory);

        /// <summary>
        /// Reads from a named memory slot
        /// </summary>
        /// <param name="slotName">Slot name</param>
        /// <returns></returns>
        IFloatMatrix GetMemory(string slotName);

        /// <summary>
        /// Gets the next queued graph operation (if any)
        /// </summary>
        /// <returns></returns>
        IGraphOperation? GetNextOperation();

        /// <summary>
        /// Adds a list of graph operations to the queue
        /// </summary>
        /// <param name="operationList">List of operations</param>
        void Add(IEnumerable<IGraphOperation> operationList);

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
        void RegisterContinuation(IMiniBatchSequence sequence, Action<IGraphSequenceContext> callback);

        /// <summary>
        /// Registers an additional mini batch to execute after the current mini batch has completed
        /// </summary>
        /// <param name="miniBatch">Mini batch to execute</param>
        /// <param name="data">Initial data</param>
        /// <param name="startCallback">Callback when starting the batch</param>
        /// <param name="endCallback">Callback when ending the batch</param>
        void RegisterAdditionalMiniBatch(IMiniBatch miniBatch, IGraphData data, Action<IGraphSequenceContext, IGraphData> startCallback, Action<IGraphSequenceContext[]> endCallback);

        /// <summary>
        /// True if there are registered continuations or additional mini batches to execute
        /// </summary>
        bool HasContinuations { get; }

        /// <summary>
        /// Execute any registered continuation for this context
        /// </summary>
        /// <param name="context">Context with an associated IMiniBatchSequence</param>
        void Continue(IGraphSequenceContext context);

        /// <summary>
        /// Executes any additionally registered mini batches
        /// </summary>
        /// <param name="learningContext">Learning context (null if executing without training)</param>
        IEnumerable<(IGraphSequenceContext Context, Action<IGraphSequenceContext[]> Callback)> ExecuteAdditionalMiniBatch(ILearningContext? learningContext);
    }

    /// <summary>
    /// A type that can create a graph context
    /// </summary>
    public interface ICreateGraphContext
    {
        /// <summary>
        /// Creates a graph context
        /// </summary>
        /// <param name="executionContext">Graph execution cointext</param>
        /// <param name="sequence">Mini batch sequence</param>
        /// <param name="learningContext">Learning context (null if executing without training)</param>
        /// <returns></returns>
        IGraphSequenceContext Create(IGraphExecutionContext executionContext, IMiniBatchSequence sequence, ILearningContext? learningContext);
    }

    /// <summary>
    /// Backpropagation handler
    /// </summary>
    public interface IBackpropagate : IDisposable
    {
        /// <summary>
        /// Backpropagate
        /// </summary>
        /// <param name="errorSignal">Error signal</param>
        /// <param name="context">Graph context</param>
        /// <param name="parents"></param>
        IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents);
    }

    /// <summary>
    /// Segment sources feed data into a graph
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// The size of the input data
        /// </summary>
        uint InputSize { get; }

        /// <summary>
        /// The size of the output data
        /// </summary>
        uint? OutputSize { get; }

        /// <summary>
        /// Number of rows
        /// </summary>
        uint RowCount { get; }

        /// <summary>
        /// Gets a mini batch with the specified rows
        /// </summary>
        /// <param name="rows">List of rows</param>
        IMiniBatch Get(uint[] rows);

        /// <summary>
        /// For sequential data, returns the row indexes grouped by sequence length
        /// </summary>
        /// <returns></returns>
        uint[][] GetSequentialBatches();

        /// <summary>
        /// Creates a new data source, using the current as a template but replacing the data table
        /// </summary>
        /// <param name="dataTable">The new data table</param>
        /// <returns></returns>
        IDataSource CloneWith(IRowOrientedDataTable dataTable);

        /// <summary>
        /// Table vectoriser to create a feature vector
        /// </summary>
        IDataTableVectoriser? InputVectoriser { get; }

        /// <summary>
        /// Table vectoriser to create a target vector
        /// </summary>
        IDataTableVectoriser? OutputVectoriser { get; }
    }

    /// <summary>
    /// Adaptive data sources apply the output from a preliminary graph
    /// </summary>
    public interface IAdaptiveDataSource
    {
        /// <summary>
        /// The input node of the preliminary graph
        /// </summary>
        NodeBase AdaptiveInput { get; }

        /// <summary>
        /// Gets the serialised preliminary graph
        /// </summary>
        /// <param name="name">Optional name to give the data source</param>
        /// <returns></returns>
        DataSourceModel GetModel(string? name = null);
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
        uint SequenceIndex { get; }

        /// <summary>
        /// Sequence type
        /// </summary>
        MiniBatchSequenceType Type { get; }

        /// <summary>
        /// Input data
        /// </summary>
        IGraphData? Input { get; }

        /// <summary>
        /// Training target data
        /// </summary>
        IGraphData? Target { get; }

        /// <summary>
        /// Graph sequence context that has been executed for this sequence
        /// </summary>
        IGraphSequenceContext? GraphContext { get; set; }
    }

    /// <summary>
    /// Information about the current mini batch
    /// </summary>
    public interface IMiniBatch
    {
        /// <summary>
        /// Row indexes of the current batch
        /// </summary>
        uint[] Rows { get; }

        /// <summary>
        /// Segment source
        /// </summary>
        IDataSource DataSource { get; }

        /// <summary>
        /// True if the data is sequential
        /// </summary>
        bool IsSequential { get; }

        /// <summary>
        /// Number of items in the batch
        /// </summary>
        uint BatchSize { get; }

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
        IMiniBatchSequence? GetNextSequence();

        /// <summary>
        /// Gets the length of the sequence
        /// </summary>
        uint SequenceCount { get; }

        /// <summary>
        /// Gets a sequence item
        /// </summary>
        /// <param name="index">The index to retrieve</param>
        /// <returns></returns>
        IMiniBatchSequence GetSequenceAtIndex(uint index);

        /// <summary>
        /// Resets the sequence iterator
        /// </summary>
        void Reset();

        /// <summary>
        /// Subsequent mini bach
        /// </summary>
        IMiniBatch? NextMiniBatch { get; }

        /// <summary>
        /// Previous mini batch
        /// </summary>
        IMiniBatch? PreviousMiniBatch { get; }
    }

    /// <summary>
    /// A pending graph operation (mini batch)
    /// </summary>
    public interface IGraphOperation
    {
        /// <summary>
        /// Executes the operation
        /// </summary>
        IEnumerable<IGraphSequenceContext> Execute();
    }

    /// <summary>
    /// Graph engines drive execution within a graph
    /// </summary>
    public interface IGraphEngine : ICreateGraphContext
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        ILinearAlgebraProvider LinearAlgebraProvider { get; }

        /// <summary>
        /// Serialised version of the current graph and its parameters
        /// </summary>
        ExecutionGraphModel Graph { get; }

        /// <summary>
        /// Segment source that feeds into the graph
        /// </summary>
        IDataSource? DataSource { get; }

        /// <summary>
		/// The graph's single start node
		/// </summary>
        NodeBase Start { get; }
    }

    /// <summary>
    /// Graph engine that is optimised for inference
    /// </summary>
    public interface IGraphExecutionEngine : IGraphEngine
    {
        /// <summary>
        /// Executes a data source on the current graph
        /// </summary>
        /// <param name="dataSource">Segment source to process</param>
        /// <param name="batchSize">Initial size of each mini batch</param>
        /// <param name="batchCompleteCallback">Optional callback to be notifiied after each mini batch has completed</param>
        /// <returns></returns>
        IEnumerable<ExecutionResult> Execute(IDataSource dataSource, uint batchSize = 128, Action<float>? batchCompleteCallback = null);

        /// <summary>
        /// Executes a single vector on the current graph
        /// </summary>
        /// <param name="input">Vector to execute</param>
        /// <returns></returns>
        ExecutionResult? Execute(float[] input);

        /// <summary>
        /// Executes a sequential input on the current graph
        /// </summary>
        /// <param name="executionContext">Execution context</param>
        /// <param name="sequenceIndex">Index of the current sequence (starting from 0)</param>
        /// <param name="input">Input vector</param>
        /// <param name="sequenceType">The sequence type (start, standard, end)</param>
        /// <returns></returns>
        ExecutionResult? ExecuteSingleSequentialStep(IGraphExecutionContext executionContext, uint sequenceIndex, float[] input, MiniBatchSequenceType sequenceType);

        /// <summary>
        /// Executes a sequence of inputs on the current graph
        /// </summary>
        /// <param name="input">List of vector inputs</param>
        /// <returns>List of execution results</returns>
        IEnumerable<ExecutionResult> ExecuteSequential(float[][] input);

        /// <summary>
        /// Creates a graph execution context
        /// </summary>
        /// <returns></returns>
        IGraphExecutionContext CreateExecutionContext();
    }

    /// <summary>
    /// A graph engine that can train the graph from training data
    /// </summary>
    public interface IGraphTrainingEngine : IGraphEngine
    {
        /// <summary>
	    /// Executes a training epoch on the graph
	    /// </summary>
	    /// <param name="executionContext">Graph execution context</param>
	    /// <param name="batchCompleteCallback">Optional callback to be notifiied after each mini batch has completed</param>
	    /// <returns>Graph training error</returns>
	    void Train(IGraphExecutionContext executionContext, Action<float>? batchCompleteCallback = null);

        /// <summary>
        /// Executes test data on the current graph
        /// </summary>
        /// <param name="testDataSource">Segment source with test data</param>
        /// <param name="batchSize">Initial size of each mini batch</param>
        /// <param name="batchCompleteCallback">Optional callback to be notifiied after each mini batch has completed</param>
        /// <param name="values">Optional callback to get the (testError, trainingRate, isPercentage, isImprovedScore) data</param>
        /// <returns>True if the model performance has improved since the last test</returns>
        bool Test(
            IDataSource testDataSource,
            uint batchSize = 128,
            Action<float>? batchCompleteCallback = null,
            Action<float, bool, bool>? values = null
        );

        /// <summary>
        /// Graph learning context
        /// </summary>
        ILearningContext LearningContext { get; }

        /// <summary>
        /// Loads model parameters into the existing graph (based on matching ids or names)
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="graph">Model to load parameters from</param>
        void LoadParametersFrom(GraphFactory factory, ExecutionGraphModel graph);

        /// <summary>
        /// Creates an inference only engine from the current graph
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        IGraphExecutionEngine CreateExecutionEngine(ExecutionGraphModel? model);

        /// <summary>
        /// Resets the learning context epoch and the best test result
        /// </summary>
        void Reset();

        /// <summary>
        /// Sets the engine's start node (can be used to load a previously saved graph)
        /// </summary>
        /// <param name="startNode">Node to use as the engine's start node</param>
        void SetStartNode(NodeBase? startNode);
    }

    /// <summary>
    /// Nodes that have a memory feeder sub-node
    /// </summary>
    public interface IHaveMemoryNode
    {
        /// <summary>
        /// The memory feed sub node
        /// </summary>
        NodeBase Memory { get; }
    }

    /// <summary>
    /// Recurrent neural networks memory node
    /// </summary>
    public interface IMemoryNode
    {
        /// <summary>
        /// The current state of the memory node
        /// </summary>
        Vector<float> Data { get; set; }
    }

    /// <summary>
    /// Feed forward layer
    /// </summary>
    public interface IFeedForward
    {
        /// <summary>
        /// Node id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Size of incoming connections
        /// </summary>
        uint InputSize { get; }

        /// <summary>
        /// Size of outgoing connections
        /// </summary>
        uint OutputSize { get; }

        /// <summary>
        /// Bias vector
        /// </summary>
        IFloatVector Bias { get; }

        /// <summary>
        /// Weight matrix
        /// </summary>
        IFloatMatrix Weight { get; }

        /// <summary>
        /// Updates the weights
        /// </summary>
        /// <param name="delta">Weight delta matrix</param>
        /// <param name="context">Graph learning context</param>
        void UpdateWeights(IFloatMatrix delta, ILearningContext context);
    }

    /// <summary>
    /// Nodes that contain a feed forward layer
    /// </summary>
    public interface IHaveFeedForward
    {
        /// <summary>
        /// Feed forward layer
        /// </summary>
        IFeedForward FeedForward { get; }
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
        uint Width { get; }

        /// <summary>
        /// Height of each input volume
        /// </summary>
        uint Height { get; }

        /// <summary>
        /// Depth of each input volume
        /// </summary>
        uint Depth { get; }
    }
}
