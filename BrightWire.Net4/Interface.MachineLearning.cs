using BrightWire.Models;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire
{
    public interface IErrorMetric
    {
        bool DisplayAsPercentage { get; }
        float Compute(IIndexableVector output, IIndexableVector targetOutput);
        IMatrix CalculateGradient(IMatrix output, IMatrix targetOutput);
    }

    /// <summary>
    /// Random projection
    /// </summary>
    public interface IRandomProjection : IDisposable
    {
        /// <summary>
        /// The size to reduce to
        /// </summary>
        int Size { get; }

        /// <summary>
        /// The transformation matrix
        /// </summary>
        IMatrix Matrix { get; }

        /// <summary>
        /// Reduces a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        IVector Compute(IVector vector);

        /// <summary>
        /// Reduces a matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        IMatrix Compute(IMatrix matrix);
    }

    /// <summary>
    /// Markov model trainer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMarkovModelTrainer<T>
    {
        /// <summary>
        /// Adds a sequence of items to the trainer
        /// </summary>
        /// <param name="items"></param>
        void Add(IEnumerable<T> items);
    }

    /// <summary>
    /// Markov model trainer (window size 2)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMarkovModelTrainer2<T> : IMarkovModelTrainer<T>
    {
        /// <summary>
        /// Gets all current observations
        /// </summary>
        MarkovModel2<T> Build();
    }

    /// <summary>
    /// Markov model trainer (window size 3)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMarkovModelTrainer3<T> : IMarkovModelTrainer<T>
    {
        /// <summary>
        /// Gets all current observations
        /// </summary>
        MarkovModel3<T> Build();
    }

    public interface ILearningContext
    {
        double EpochSeconds { get; }
        long EpochMilliseconds { get; }
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        int CurrentEpoch { get; }
        float LearningRate { get; }
        int BatchSize { get; }
        int RowCount { get; }
        void Store<T>(T error, Action<T> updater);
        bool CalculateTrainingError { get; }
        bool DeferUpdates { get; }
        void ApplyUpdates();
        void StartEpoch();
        void EndEpoch();
        void SetRowCount(int rowCount);
        //bool EnableLogging { get; set; }
        //bool LogMatrixValues { get; set; }
        //string CurrentLogXml { get; }
        //void ClearLog();
        //void Log(Action<XmlWriter> callback);
        //void Log(string name, int channel, int id, IMatrix input, IMatrix output, Action<XmlWriter> callback = null);
        //void Log(string name, IMatrix matrix);
        void DeferBackpropagation(IGraphData errorSignal, Action<IGraphData> update);
        void BackpropagateThroughTime(IGraphData signal, int maxDepth = int.MaxValue);
        void ScheduleLearningRate(int atEpoch, float newTrainingRate);
        void Clear();
    }

    public interface IGradientDescentOptimisation : IDisposable, ICanSerialise
    {
        void Update(IMatrix source, IMatrix delta, ILearningContext context);
    }

    public interface ICreateGradientDescent
    {
        IGradientDescentOptimisation Create(IPropertySet propertySet);
    }

    public interface ICreateTemplateBasedGradientDescent
    {
        IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IMatrix template, IPropertySet propertySet);
    }

    public interface IPropertySet
    {
        ILinearAlgebraProvider LinearAlgebraProvider { get; }
        IWeightInitialisation WeightInitialisation { get; set; }
        IGradientDescentOptimisation GradientDescent { get; set; }
        ICreateTemplateBasedGradientDescent TemplateGradientDescentDescriptor { get; set; }
        ICreateGradientDescent GradientDescentDescriptor { get; set; }

        IPropertySet Use(ICreateTemplateBasedGradientDescent descriptor);
        IPropertySet Use(ICreateGradientDescent descriptor);
        IPropertySet Use(IGradientDescentOptimisation optimisation);
        IPropertySet Use(IWeightInitialisation weightInit);

        IPropertySet Clone();

        T Get<T>(string name, T defaultValue = default(T));
        IPropertySet Set<T>(string name, T obj);
        void Clear(string name);
    }

    public interface IWeightInitialisation
    {
        IVector CreateBias(int size);
        IMatrix CreateWeight(int rows, int columns);
    }

    /// <summary>
    /// Logistic regression classifier
    /// </summary>
    public interface ILogisticRegressionClassifier : IDisposable
    {
        /// <summary>
        /// Outputs a value from 0 to 1
        /// </summary>
        /// <param name="vals">Input data</param>
        float Predict(params float[] vals);

        /// <summary>
        /// Outputs a value from 0 to 1
        /// </summary>
        /// <param name="vals">Input data</param>
        float Predict(IReadOnlyList<float> vals);

        /// <summary>
        /// Outputs a list of values from 0 to 1 for each input data
        /// </summary>
        /// <param name="input">Input data</param>
        float[] Predict(IReadOnlyList<IReadOnlyList<float>> input);
    }

    /// <summary>
    /// Linear regression predictor
    /// </summary>
    public interface ILinearRegressionPredictor : IDisposable
    {
        /// <summary>
        /// Predicts a value from input data
        /// </summary>
        /// <param name="vals">The input data</param>
        float Predict(params float[] vals);

        /// <summary>
        /// Predicts a value from input data
        /// </summary>
        /// <param name="vals">The input data</param>
        float Predict(IReadOnlyList<float> vals);

        /// <summary>
        /// Bulk value prediction
        /// </summary>
        /// <param name="input">List of data to predict</param>
        /// <returns>List of predictions</returns>
        float[] Predict(IReadOnlyList<IReadOnlyList<float>> input);
    }

    /// <summary>
    /// A logistic regression trainer
    /// </summary>
    public interface ILogisticRegressionTrainer
    {
        /// <summary>
        /// Trains a model using gradient descent
        /// </summary>
        /// <param name="iterations">Number of training epochs</param>
        /// <param name="learningRate">The training rate</param>
        /// <param name="lambda">Regularisation lambda</param>
        /// <param name="costCallback">Callback with current cost - False to stop training</param>
        /// <returns></returns>
        LogisticRegression GradientDescent(int iterations, float learningRate, float lambda = 0.1f, Func<float, bool> costCallback = null);

        /// <summary>
        /// Computes the cost of the specified parameters
        /// </summary>
        /// <param name="theta">The model parameters</param>
        /// <param name="lambda">Regularisation lambda</param>
        /// <returns></returns>
        float ComputeCost(IVector theta, float lambda);
    }

    /// <summary>
    /// Trainer for linear regression models
    /// </summary>
    public interface ILinearRegressionTrainer
    {
        // <summary>
        // Attempt to solve the model using matrix inversion (only applicable for small sets of training data)
        // </summary>
        // <returns></returns>
        //LinearRegression Solve();

        /// <summary>
        /// Solves the model using gradient descent
        /// </summary>
        /// <param name="iterations">Number of training epochs</param>
        /// <param name="learningRate">The training rate</param>
        /// <param name="lambda">Regularisation lambda</param>
        /// <param name="costCallback">Callback with current cost - False to stop training</param>
        /// <returns>A trained model</returns>
        LinearRegression GradientDescent(int iterations, float learningRate, float lambda = 0.1f, Func<float, bool> costCallback = null);

        /// <summary>
        /// Computes the cost of the specified parameters
        /// </summary>
        /// <param name="theta">The model parameters</param>
        /// <param name="lambda">Regularisation lambda</param>
        float ComputeCost(IVector theta, float lambda);
    }
}
