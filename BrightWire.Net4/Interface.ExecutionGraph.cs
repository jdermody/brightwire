using BrightWire.ExecutionGraph;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace BrightWire
{
    /// <summary>
    /// 4D tensor that is used as a signal between nodes in the graph
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
        /// 3D Tensor depth
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Count of 3D tensors
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the tensor as a matrix
        /// </summary>
        /// <returns></returns>
        IMatrix GetMatrix();

        /// <summary>
        /// Replaces the data with the specified matrix (but preserves the tensor meta data)
        /// </summary>
        /// <param name="matrix">The tensor to use as a replacement</param>
        IGraphData ReplaceWith(IMatrix matrix);
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
        /// Searches for a node
        /// </summary>
        /// <param name="name">Friendly name of the node to find</param>
        /// <returns></returns>
        INode Find(string name);

        /// <summary>
        /// Sub-nodes of the current node
        /// </summary>
        IEnumerable<INode> SubNodes { get; }

        /// <summary>
        /// Serialise the node
        /// </summary>
        /// <param name="connectedTo">List of nodes that are connected to the current node</param>
        /// <param name="wireList">List of wires connecting this and any other connected node together</param>
        /// <returns>Serialisation model</returns>
        Models.ExecutionGraph.Node SerialiseTo(List<Models.ExecutionGraph.Node> connectedTo, List<Models.ExecutionGraph.Wire> wireList);

        /// <summary>
        /// Called after the graph has been completely deserialised
        /// </summary>
        /// <param name="graph">Dictionary of nodes with their associated unique ids</param>
        void OnDeserialise(IReadOnlyDictionary<string, INode> graph);
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

    public interface IExecutionHistory
    {
        INode Source { get; }
        IReadOnlyList<INode> Parents { get; }
        IGraphData Data { get; }
        IBackpropagation Backpropagation { get; set; }
    }

    public interface IContext : IDisposable
    {
        bool IsTraining { get; }
        INode Source { get; }
        IExecutionContext ExecutionContext { get; }
        ILearningContext LearningContext { get; }
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        IMiniBatchSequence BatchSequence { get; }
        void AddForward(IExecutionHistory action, Func<IBackpropagation> callback);
        void AddBackward(IGraphData errorSignal, INode target, INode source);
        void AppendErrorSignal(IGraphData errorSignal, INode forNode);
        void Backpropagate(IGraphData delta, IErrorMetric errorMetric);
        IGraphData ErrorSignal { get; }
        IGraphData Data { get; }
    }

    public interface IExecutionContext : IDisposable
    {
        void SetMemory(string index, IMatrix memory);
        IMatrix GetMemory(string index);
        IGraphOperation GetNextOperation();
        void Add(IReadOnlyList<IGraphOperation> operationList);
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        int RemainingOperationCount { get; }
    }

    public interface IBackpropagation : IDisposable
    {
        void Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents);
    }

    public interface IDataSource
    {
        bool IsSequential { get; }
        int InputSize { get; }
        int OutputSize { get; }
        int RowCount { get; }
        IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows);
        IReadOnlyList<IReadOnlyList<int>> GetBuckets();
        void OnBatchProcessed(IContext context);
        IDataSource CloneWith(IDataTable dataTable);
    }

    public interface IAdaptiveDataSource
    {
        INode AdaptiveInput { get; }
        DataSourceModel GetModel(string name = null);
    }

    public enum MiniBatchType
    {
        Standard,
        SequenceStart,
        SequenceEnd
    }

    public interface IMiniBatchSequence
    {
        IMiniBatch MiniBatch { get; }
        int SequenceIndex { get; }
        MiniBatchType Type { get; }
        IGraphData Input { get; }
        IGraphData Target { get; }
    }

    public interface IMiniBatch
    {
        IReadOnlyList<int> Rows { get; }
        IDataSource DataSource { get; }
        bool IsSequential { get; }
        int BatchSize { get; }
        IMiniBatchSequence CurrentSequence { get; }
        bool HasNextSequence { get; }
        IMiniBatchSequence GetNextSequence();
        int SequenceCount { get; }
        IMiniBatchSequence GetSequenceAtIndex(int index);
    }

    public interface IGraphOperation
    {
        void Execute(IExecutionContext executionContext);
    }

    public interface IGraphEngine
    {
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        Models.ExecutionGraph Graph { get; }
        IDataSource DataSource { get; }
        INode Input { get; }
        IReadOnlyList<ExecutionResult> Execute(IDataSource dataSource, int batchSize = 128);
        ExecutionResult Execute(float[] input);
    }

    public interface IGraphTrainingEngine : IGraphEngine
    {
        double Train(IExecutionContext executionContext, Action<float> batchCompleteCallback = null);
        bool Test(IDataSource testDataSource, IErrorMetric errorMetric, int batchSize = 128);
        ILearningContext LearningContext { get; }
    }

    public interface IHaveMemoryNode
    {
        INode Memory { get; }
    }

    public interface IFeedForward : INode
    {
        int InputSize { get; }
        int OutputSize { get; }
        IVector Bias { get; }
        IMatrix Weight { get; }
        void UpdateWeights(IMatrix delta, ILearningContext context);
    }

    public interface IVolumeDataSource
    {
        int Width { get; }
        int Height { get; }
        int Depth { get; }
    }
}
