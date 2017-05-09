using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    public enum GraphDataType
    {
        Vector,
        Matrix,
        Tensor
    }

    public interface IGraphData
    {
        GraphDataType CurrentType { get; }
        IMatrix GetAsMatrix();
    }

    public interface IAction
    {
        void Execute(IMatrix input, IContext context);
    }

    public interface INode : IDisposable
    {
        string Id { get; }
        string Name { get; }
        List<IWire> Output { get; }
        void ExecuteForward(IContext context, int channel);
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
        IGraphData Data { get; }
        INode Source { get; }
        IExecutionContext ExecutionContext { get; }
        ILearningContext LearningContext { get; }
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        IMiniBatchSequence BatchSequence { get; }
        void Forward(IExecutionHistory action, Func<IBackpropagation> callback);
        void Backward(IMatrix errorSignal, INode target);
        void StartBackpropagation(IMatrix output, IMatrix target, IMatrix delta);
    }

    public interface IExecutionContext
    {
        void SetMemory(string index, IMatrix memory);
        IMatrix GetMemory(string index);
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
    }

    public interface IBackpropagation : IDisposable
    {
        void Backward(IMatrix errorSignal, IContext context, IReadOnlyList<INode> parents);
    }

    public interface IDataSource
    {
        bool IsSequential { get; }
        int InputSize { get; }
        int OutputSize { get; }
        int RowCount { get; }
        IReadOnlyList<(float[], float[])> Get(IReadOnlyList<int> rows);
        IReadOnlyList<(FloatMatrix Input, FloatMatrix Output)> GetSequential(IReadOnlyList<int> rows);
        IReadOnlyList<IReadOnlyList<int>> GetBuckets();
        //string GetOutputLabel(int columnIndex, int vectorIndex);
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

    public interface ISequenceResult
    {
        IMatrix Output { get; }
        IMatrix Target { get; }
        IMatrix Delta { get; }
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
        IDataSource DataSource { get; }
        INode Input { get; }
    }

    public interface IGraphTrainingEngine : IGraphEngine
    {
        double Train();
        void WriteTestResults(IDataSource testDataSource, IErrorMetric errorMetric, int batchSize = 128);
        ILearningContext LearningContext { get; }
    }
}
