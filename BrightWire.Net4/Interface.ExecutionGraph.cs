using BrightWire.ExecutionGraph;
using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    public enum GraphDataType
    {
        Matrix,
        Tensor
    }

    public interface IGraphData
    {
        GraphDataType DataType { get; }
        IMatrix GetMatrix();
        I3DTensor GetTensor();
    }

    public interface IAction
    {
        void Execute(IGraphData input, IContext context);
        string Serialise();
        void Initialise(string data);
    }

    public interface ICanInitialiseNode
    {
        void Initialise(GraphFactory factory, string id, string name, string description, byte[] data);
    }

    public interface INode : ICanInitialiseNode, IDisposable
    {
        string Id { get; }
        string Name { get; }
        List<IWire> Output { get; }
        void ExecuteForward(IContext context, int channel);
        INode SearchFor(string name);
        IEnumerable<INode> SubNodes { get; }
        Models.ExecutionGraph.Node SerialiseTo(List<Models.ExecutionGraph.Node> connectedTo, List<Models.ExecutionGraph.Wire> wireList);
        void WriteTo(BinaryWriter writer);
        void ReadFrom(GraphFactory factory, BinaryReader reader);
    }

    public interface IWire
    {
        INode SendTo { get; }
        int Channel { get; }
    }

    public interface IExecutionHistory
    {
        INode Source { get; }
        IReadOnlyList<INode> Parents { get; }
        IGraphData Data { get; }
        IBackpropagation Backpropagation { get; set; }
    }

    public interface IContext
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
        void Backpropagate(IGraphData delta);
        IGraphData ErrorSignal { get; }
        IGraphData Data { get; }
        IMatrix Output { get; set; }
    }

    public interface IExecutionContext
    {
        IGraphData Data { get; set; }
        void SetMemory(string index, IMatrix memory);
        IMatrix GetMemory(string index);
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
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
        IMiniBatch Get(IReadOnlyList<int> rows);
        IReadOnlyList<IReadOnlyList<int>> GetBuckets();
        void OnBatchProcessed(IContext context);
        IDataSource GetFor(IDataTable dataTable);
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
        IMatrix Input { get; }
        IMatrix Target { get; }
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
        int SequenceLength { get; }
        IMiniBatchSequence GetSequenceAtIndex(int index);
    }

    //public interface IBatchContext
    //{
    //    double TrainingError { get; }
    //    IMiniBatch Batch { get; }
    //    bool IsTraining { get; }
    //    ILearningContext LearningContext { get; }
    //    IExecutionContext ExecutionContext { get; }
    //    ILinearAlgebraProvider LinearAlgebraProvider { get; }
    //    void SetOutput(IMatrix output, IMatrix target, IMatrix delta, int channel);
    //    void RegisterBackpropagation(IBackpropagation backProp, int channel);
    //    void Backpropagate(IMatrix delta, int channel);
    //    IEnumerable<ISequenceResult> Results { get; }
    //}

    public interface IGraphOperation
    {
        void Execute();
    }

    public interface IGraphEngine
    {
        Models.ExecutionGraph Graph { get; }
        IDataSource DataSource { get; }
        INode Input { get; }
        IReadOnlyList<ExecutionResult> Execute(IDataSource dataSource, int batchSize = 128);
    }

    public interface IGraphTrainingEngine : IGraphEngine
    {
        double Train();
        void WriteTestResults(IDataSource testDataSource, IErrorMetric errorMetric, int batchSize = 128);
        ILearningContext LearningContext { get; }
    }
}
