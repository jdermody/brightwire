using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire
{
    public interface ILinearAlgebraProvider : IDisposable
    {
        IVector Create(float[] data);
        IVector Create(IEnumerable<float> data);
        IVector Create(int length, float value);
        IVector Create(int length, Func<int, float> init);
        IVector Create(IIndexableVector vector);

        IMatrix Create(int rows, int columns, Func<int, int, float> init);
        IMatrix Create(int rows, int columns, float value);
        IMatrix Create(int rows, int columns, IList<IIndexableVector> vectorData);
        IMatrix Create(IIndexableMatrix matrix);

        IMatrix CreateMatrix(IReadOnlyList<FloatArray> data);
        IVector CreateVector(FloatArray data);

        IIndexableVector CreateIndexable(int length);
        IIndexableVector CreateIndexable(int length, Func<int, float> init);
        IIndexableMatrix CreateIndexable(int rows, int columns);
        IIndexableMatrix CreateIndexable(int rows, int columns, Func<int, int, float> init);
        IIndexableMatrix CreateIndexable(int rows, int columns, float value);

        INeuralNetworkFactory NN { get; }
    }

    public enum NormalisationType
    {
        Standard,
        Manhattan,
        Euclidean,
        Infinity,
        FeatureScale,
    }

    public interface IVector : IDisposable
    {
        bool IsValid { get; }
        IMatrix ToColumnMatrix(int numCols = 1);
        IMatrix ToRowMatrix(int numRows = 1);
        int Count { get; }
        FloatArray Data { get; set; }
        IVector Add(IVector vector);
        IVector Subtract(IVector vector);
        float L2Norm();
        int MaximumIndex();
        int MinimumIndex();
        object WrappedObject { get; }
        void Multiply(float scalar);
        void AddInPlace(IVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        void SubtractInPlace(IVector vector, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        IIndexableVector AsIndexable();
        IVector PointwiseMultiply(IVector vector);
        float DotProduct(IVector vector);
        IVector GetNewVectorFromIndexes(int[] indexes);
        IVector Clone();
        IVector Sqrt();
        void CopyFrom(IVector vector);
        float EuclideanDistance(IVector vector);
        float CosineDistance(IVector vector);
        float ManhattanDistance(IVector vector);
        float MeanSquaredDistance(IVector vector);
        float SquaredEuclidean(IVector vector);
    }

    public interface IIndexableVector : IVector
    {
        float this[int index] { get; set; }
        IEnumerable<float> Values { get; }
        float[] ToArray();
        IIndexableVector Softmax();
        IIndexableVector Softmax2();
        void Normalise(NormalisationType type);
    }

    public enum MatrixGrouping
    {
        ByRow,
        ByColumn,
    }

    public interface IMatrix : IDisposable
    {
        bool IsValid { get; }
        IMatrix Multiply(IMatrix matrix);
        int ColumnCount { get; }
        int RowCount { get; }
        IVector Column(int index);
        IVector Diagonal();
        IVector Row(int index);
        IMatrix Add(IMatrix matrix);
        IMatrix Subtract(IMatrix matrix);
        IMatrix PointwiseMultiply(IMatrix matrix);
        IMatrix TransposeAndMultiply(IMatrix matrix);
        IMatrix TransposeThisAndMultiply(IMatrix matrix);
        IVector RowSums(float coefficient = 1f);
        IVector ColumnSums(float coefficient = 1f);
        object WrappedObject { get; }
        IMatrix Transpose();
        void Multiply(float scalar);
        void AddInPlace(IMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        void SubtractInPlace(IMatrix matrix, float coefficient1 = 1.0f, float coefficient2 = 1.0f);
        IMatrix SigmoidActivation();
        IMatrix SigmoidDerivative();
        IMatrix TanhActivation();
        IMatrix TanhDerivative();
        void AddToEachRow(IVector vector);
        void AddToEachColumn(IVector vector);
        FloatArray[] Data { get; set; }
        IIndexableMatrix AsIndexable();
        IMatrix GetNewMatrixFromRows(int[] rowIndexes);
        IMatrix GetNewMatrixFromColumns(int[] columnIndexes);
        void ClearRows(int[] indexes);
        void ClearColumns(int[] indexes);
        IMatrix ReluActivation();
        IMatrix ReluDerivative();
        IMatrix LeakyReluActivation();
        IMatrix LeakyReluDerivative();
        IMatrix Clone();
        void Clear();
        IMatrix Sqrt(float valueAdjustment = 0);
        IMatrix Pow(float power);
        IMatrix PointwiseDivide(IMatrix matrix);
        void L1Regularisation(float coefficient);
        IVector ColumnL2Norm();
        IVector RowL2Norm();
        void PointwiseDivideRows(IVector vector);
        void PointwiseDivideColumns(IVector vector);
        void Constrain(float min, float max);
        void UpdateRow(int index, IIndexableVector vector, int columnIndex);
        void UpdateColumn(int index, IIndexableVector vector, int rowIndex);
        IVector GetRowSegment(int index, int columnIndex, int length);
        IVector GetColumnSegment(int index, int rowIndex, int length);
        IMatrix ConcatColumns(IMatrix bottom);
        IMatrix ConcatRows(IMatrix right);
        Tuple<IMatrix, IMatrix> SplitRows(int position);
        Tuple<IMatrix, IMatrix> SplitColumns(int position);
    }

    public interface IIndexableMatrix : IMatrix
    {
        float this[int row, int column] { get; set; }
        IEnumerable<IIndexableVector> Rows { get; }
        IEnumerable<IIndexableVector> Columns { get; }
        IEnumerable<float> Values { get; }
        void Normalise(MatrixGrouping group, NormalisationType type);
        IIndexableMatrix Map(Func<float, float> mutator);
        IIndexableMatrix MapIndexed(Func<int, int, float, float> mutator);
    }

    public interface IActivationFunction
    {
        IMatrix Calculate(IMatrix data);
        IMatrix Derivative(IMatrix data);
        float Calculate(float val);
        ActivationType Type { get; }
    }

    public interface ICostFunction
    {
        float Calculate(IIndexableVector output, IIndexableVector expectedOutput);
    }

    public interface INeuralNetworkLayer : IDisposable
    {
        int InputSize { get; }
        int OutputSize { get; }
        INeuralNetworkLayerDescriptor Descriptor { get; }
        IVector Bias { get; }
        IMatrix Weight { get; }
        IActivationFunction Activation { get; }
        IMatrix Execute(IMatrix input);
        IMatrix Activate(IMatrix input);
        void Update(IMatrix biasDelta, IMatrix weightDelta, float weightCoefficient, float learningRate);
        NetworkLayer LayerInfo { get; set; }
    }

    public interface INeuralNetworkBidirectionalLayer : IDisposable
    {
        INeuralNetworkRecurrentLayer Forward { get; }
        INeuralNetworkRecurrentLayer Backward { get; }
        BidirectionalLayer LayerInfo { get; set; }
    }

    public interface INeuralNetworkRecurrentLayer : IDisposable
    {
        bool IsRecurrent { get; }
        INeuralNetworkRecurrentBackpropagation Execute(List<IMatrix> curr, bool backpropagate);
        RecurrentLayer LayerInfo { get; set; }
    }

    public interface INeuralNetworkRecurrentBackpropagation
    {
        IMatrix Execute(IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updateAccumulator);
    }

    public interface INeuralNetworkUpdateAccumulator : IDisposable
    {
        void Record(INeuralNetworkLayerUpdater updater, IMatrix bias, IMatrix weights);
        ITrainingContext Context { get; }
        IMatrix GetData(string name);
        void SetData(string name, IMatrix data);
        void Clear();
    }

    public interface INeuralNetworkLayerUpdater : IDisposable
    {
        INeuralNetworkLayer Layer { get; }
        void Update(IMatrix biasDelta, IMatrix weightDelta, ITrainingContext context);
        void Reset();
    }

    public interface IRecurrentTrainingContext
    {
        ITrainingContext TrainingContext { get; }
        void AddFilter(INeuralNetworkRecurrentTrainerFilter filter);
        void ExecuteForward(Tuple<IMatrix[], IMatrix[], int[]> miniBatch, float[] memory, Action<int, List<IMatrix>> onT);
        void ExecuteForwardSingle(Tuple<float[], float[]>[] data, float[] memory, int dataIndex, Action<int, List<IMatrix>> onT);
        void ExecuteBidirectional(
            ITrainingContext context,
            Tuple<IMatrix[], IMatrix[], int[]> miniBatch,
            IReadOnlyList<INeuralNetworkBidirectionalLayer> layers,
            float[] memoryForward,
            float[] memoryBackward,
            int padding,
            Stack<Tuple<Stack<Tuple<INeuralNetworkRecurrentBackpropagation, INeuralNetworkRecurrentBackpropagation>>, IMatrix, IMatrix, int[], int>> updateStack,
            Action<List<IIndexableVector[]>, List<IMatrix>> onFinished
        );
    }

    public interface ITrainingContext
    {
        float TrainingRate { get; }
        int TrainingSamples { get; }
        int CurrentEpoch { get; }
        double LastTrainingError { get; }
        long EpochMilliseconds { get; }
        double EpochSeconds { get; }
        int MiniBatchSize { get; }

        XmlWriter Logger { get; set; }

        void Reset();
        void StartEpoch(int trainingSamples);
        void EndEpoch(double trainingError);
        void EndBatch();
        void EndRecurrentEpoch(double trainingError, IRecurrentTrainingContext context);
        void WriteScore(double score, bool asPercentage, bool flag = false);
        IMatrix AddToGarbage(IMatrix matrix);
        void Cleanup();

        event Action<ITrainingContext> EpochComplete;
        event Action<ITrainingContext, IRecurrentTrainingContext> RecurrentEpochComplete;
    }

    public interface INeuralNetworkLayerTrainer : IDisposable
    {
        INeuralNetworkLayerUpdater LayerUpdater { get; }
        IMatrix FeedForward(IMatrix input, bool storeForBackpropagation);
        IMatrix Backpropagate(IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates = null);
        IMatrix Backpropagate(IMatrix input, IMatrix output, IMatrix errorSignal, ITrainingContext context, bool calculateOutput, INeuralNetworkUpdateAccumulator updates = null);
    }

    public enum ActivationType
    {
        None = 0,
        Relu,
        LeakyRelu,
        Sigmoid,
        Tanh
    }

    public enum WeightInitialisationType
    {
        Gaussian,
        Xavier,
        Identity,
        Identity01,
        Identity001
    }

    public enum RegularisationType
    {
        None,
        L2,
        L1,
        MaxNorm
    }

    public enum WeightUpdateType
    {
        Simple,
        Momentum,
        NesterovMomentum,
        Adagrad,
        RMSprop,
        Adam
    }

    public enum LayerTrainerType
    {
        Standard,
        Dropout,
        DropConnect
    }

    public interface INeuralNetworkLayerDescriptor
    {
        ActivationType Activation { get; set; }
        WeightInitialisationType WeightInitialisation { get; set; }
        RegularisationType Regularisation { get; set; }
        WeightUpdateType WeightUpdate { get; set; }
        LayerTrainerType LayerTrainer { get; set; }
        float Lambda { get; set; }
        float Momentum { get; set; }
        float DecayRate { get; set; }
        float DecayRate2 { get; set; }
        float Dropout { get; set; }
        INeuralNetworkLayerDescriptor Clone();
        NetworkLayer AsLayer { get; }
    }

    public interface INeuralNetworkFactory
    {
        ILinearAlgebraProvider LinearAlgebraProvider { get; }

        IStandardExecution CreateFeedForward(IReadOnlyList<NetworkLayer> layers);
        IRecurrentExecution CreateRecurrent(RecurrentNetwork network);
        IBidirectionalRecurrentExecution CreateBidirectional(BidirectionalNetwork network);

        IActivationFunction GetActivation(ActivationType activation);
        IWeightInitialisation GetWeightInitialisation(WeightInitialisationType type);
        ITrainingContext CreateTrainingContext(float learningRate, int batchSize);

        INeuralNetworkLayer CreateLayer(
            int inputSize, 
            int outputSize, 
            INeuralNetworkLayerDescriptor descriptor
        );
        INeuralNetworkRecurrentLayer CreateSimpleRecurrentLayer(
            int inputSize,
            int outputSize,
            INeuralNetworkLayerDescriptor descriptor
        );
        INeuralNetworkRecurrentLayer CreateFeedForwardRecurrentLayer(
            int inputSize, 
            int outputSize, 
            INeuralNetworkLayerDescriptor descriptor
        );
        INeuralNetworkRecurrentLayer CreateLstmRecurrentLayer(
            int inputSize, 
            int outputSize, 
            INeuralNetworkLayerDescriptor descriptor
        );

        INeuralNetworkTrainer CreateBatchTrainer(
            IReadOnlyList<INeuralNetworkLayerTrainer> layer, 
            bool calculateTrainingError = true
        );
        INeuralNetworkTrainer CreateBatchTrainer(
            INeuralNetworkLayerDescriptor descriptor, 
            params int[] layerSizes
        );
        INeuralNetworkRecurrentBatchTrainer CreateRecurrentBatchTrainer(
            IReadOnlyList<INeuralNetworkRecurrentLayer> layer, 
            bool calculateTrainingError = true
        );
        INeuralNetworkBidirectionalBatchTrainer CreateBidirectionalBatchTrainer(
            IReadOnlyList<INeuralNetworkBidirectionalLayer> layer, 
            bool calculateTrainingError = true, 
            int padding = 0
        );

        INeuralNetworkLayerUpdater CreateUpdater(
            int inputSize, 
            int outputSize, 
            INeuralNetworkLayerDescriptor descriptor
        );
        INeuralNetworkLayerUpdater CreateUpdater(
            INeuralNetworkLayer layer, 
            INeuralNetworkLayerDescriptor descriptor
        );
        INeuralNetworkLayerTrainer CreateTrainer(
            int inputSize, 
            int outputSize, 
            INeuralNetworkLayerDescriptor descriptor
        );

        IFeedForwardTrainingManager CreateFeedForwardManager(
            INeuralNetworkTrainer trainer,
            string dataFile,
            ITrainingDataProvider testData,
            IErrorMetric errorMetric
        );
        IRecurrentTrainingManager CreateRecurrentManager(
            INeuralNetworkRecurrentBatchTrainer trainer, 
            string dataFile, 
            IReadOnlyList<Tuple<float[], float[]>[]> testData, 
            IErrorMetric errorMetric,
            int memorySize
        );
        IBidirectionalRecurrentTrainingManager CreateBidirectionalManager(
            INeuralNetworkBidirectionalBatchTrainer trainer, 
            string dataFile, 
            IReadOnlyList<Tuple<float[], float[]>[]> testData, 
            IErrorMetric errorMetric,
            int memorySize
        );
    }

    public interface INeuralNetworkTrainer : IDisposable
    {
        IReadOnlyList<INeuralNetworkLayerTrainer> Layer { get; }

        float CalculateCost(ITrainingDataProvider data, ICostFunction costFunction);
        void Train(ITrainingDataProvider trainingData, int numEpochs, ITrainingContext context);
        IReadOnlyList<Tuple<IIndexableVector, IIndexableVector>> Execute(ITrainingDataProvider data);
        FeedForwardNetwork NetworkInfo { get; set; }
        IEnumerable<IIndexableVector[]> ExecuteToLayer(ITrainingDataProvider data, int layerDepth);
    }

    public class RecurrentExecutionResults
    {
        public IIndexableVector Output { get; private set; }
        public IIndexableVector Target { get; private set; }
        public IIndexableVector Memory { get; private set; }
        public RecurrentExecutionResults(IIndexableVector output, IIndexableVector target, IIndexableVector memory) { Output = output; Target = target; Memory = memory; }
    }

    public interface INeuralNetworkRecurrentTrainerFilter
    {
        void BeforeFeedForward(int[] dataIndex, int sequenceIndex, int sequenceLength, List<IMatrix> context);
        void AfterBackPropagation(int[] dataIndex, int sequenceIndex, int sequenceLength, IMatrix errorSignal);
    }

    public interface INeuralNetworkRecurrentBatchTrainer : IDisposable
    {
        IReadOnlyList<INeuralNetworkRecurrentLayer> Layer { get; }
        ILinearAlgebraProvider LinearAlgebraProvider { get; }

        float CalculateCost(IReadOnlyList<Tuple<float[], float[]>[]> data, float[] memory, ICostFunction costFunction, IRecurrentTrainingContext context);
        float[] Train(IReadOnlyList<Tuple<float[], float[]>[]> trainingData, float[] memory, int numEpochs, IRecurrentTrainingContext context);
        IReadOnlyList<RecurrentExecutionResults[]> Execute(IReadOnlyList<Tuple<float[], float[]>[]> trainingData, float[] memory, IRecurrentTrainingContext context);
        RecurrentExecutionResults[] ExecuteSingle(Tuple<float[], float[]>[] data, float[] memory, IRecurrentTrainingContext context, int dataIndex = 0);
        RecurrentExecutionResults ExecuteSingleStep(float[] input, float[] memory);
        RecurrentNetwork NetworkInfo { get; set; }
    }

    public interface INeuralNetworkBidirectionalBatchTrainer : IDisposable
    {
        IReadOnlyList<INeuralNetworkBidirectionalLayer> Layer { get; }
        float CalculateCost(IReadOnlyList<Tuple<float[], float[]>[]> data, float[] forwardMemory, float[] backwardMemory, ICostFunction costFunction, IRecurrentTrainingContext context);
        IReadOnlyList<RecurrentExecutionResults[]> Execute(IReadOnlyList<Tuple<float[], float[]>[]> trainingData, float[] forwardMemory, float[] backwardMemory, IRecurrentTrainingContext context);
        Tuple<float[], float[]> Train(IReadOnlyList<Tuple<float[], float[]>[]> trainingData, float[] forwardMemory, float[] backwardMemory, int numEpochs, IRecurrentTrainingContext context);
        BidirectionalNetwork NetworkInfo { get; set; }
    }

    public enum DistanceMetric
    {
        Euclidean,
        Cosine,
        Manhattan,
        MeanSquared,
        SquaredEuclidean
    }

    public interface ITrainingDataProvider
    {
        Tuple<IMatrix, IMatrix> GetTrainingData(IReadOnlyList<int> rows, int inputSize, int outputSize);
        int Count { get; }
    }

    public interface IDataProvider
    {
        IMatrix GetData(IReadOnlyList<int> rows, int inputSize);
        int Count { get; }
    }

    public interface IWeightInitialisation
    {
        float GetWeight(int inputSize, int outputSize, int i, int j);
        float GetBias();
    }

    public interface IStandardExecution : IDisposable
    {
        IVector Execute(float[] inputData);
        IVector Execute(IVector inputData);
        IMatrix Execute(IMatrix inputData);
    }

    public interface IBidirectionalRecurrentExecution : IDisposable
    {
        IReadOnlyList<Tuple<IIndexableVector, IIndexableVector>> Execute(IReadOnlyList<float[]> inputData);
        IReadOnlyList<Tuple<IIndexableVector, IIndexableVector>> Execute(IReadOnlyList<IVector> inputData);
    }

    public interface IRecurrentExecution : IBidirectionalRecurrentExecution
    {
        Tuple<IIndexableVector, IIndexableVector> ExecuteSingle(float[] data, float[] memory);
        float[] InitialMemory { get; }
    }

    public interface IDisposableMatrixExecutionLine : IDisposable
    {
        IMatrix Current { get; }
        void Assign(IMatrix matrix);
        void Replace(IMatrix matrix);
        IMatrix Pop();
    }

    public interface IRecurrentLayerExecution : IDisposable
    {
        void Activate(List<IDisposableMatrixExecutionLine> curr);
    }

    public enum RecurrentLayerType
    {
        FeedForward,
        SimpleRecurrent,
        Lstm
    }

    public interface IFeedForwardTrainingManager : IDisposable
    {
        INeuralNetworkTrainer Trainer { get; }
        void Train(ITrainingDataProvider trainingData, int numEpochs, ITrainingContext context);
    }

    public interface IRecurrentTrainingManager
    {
        INeuralNetworkRecurrentBatchTrainer Trainer { get; }
        void Train(IReadOnlyList<Tuple<float[], float[]>[]> trainingData, int numEpochs, ITrainingContext context, IRecurrentTrainingContext recurrentContext = null);
        float[] Memory { get; }
    }

    public interface IBidirectionalRecurrentTrainingManager
    {
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        void Train(IReadOnlyList<Tuple<float[], float[]>[]> trainingData, int numEpochs, ITrainingContext context, IRecurrentTrainingContext recurrentContext = null);
        Tuple<float[], float[]> Memory { get; }
    }

    public enum ErrorMetricType
    {
        OneHot,
        RMSE,
        BinaryClassification,
        CrossEntropy
    }

    public interface IErrorMetric
    {
        float Compute(IIndexableVector output, IIndexableVector expectedOutput);
        bool HigherIsBetter { get; }
        bool DisplayAsPercentage { get; }
    }
}
