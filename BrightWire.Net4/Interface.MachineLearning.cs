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

        int CurrentEpoch { get; }
        float LearningRate { get; }
        int BatchSize { get; }
        int RowCount { get; }
        void Store(IMatrix error, Action<IMatrix> updater);
        bool CalculateTrainingError { get; }
        void ApplyUpdates();
        void StartEpoch();
        void EndEpoch();
        void SetRowCount(int rowCount);
        bool EnableLogging { get; set; }
        bool LogMatrixValues { get; set; }
        string CurrentLogXml { get; }
        void ClearLog();
        void Log(Action<XmlWriter> callback);
        void Log(string name, int channel, int id, IMatrix input, IMatrix output, Action<XmlWriter> callback = null);
        void Log(string name, IMatrix matrix);
    }

    public interface IGradientDescentOptimisation : IDisposable
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
        void Set<T>(string name, T obj);
        void Clear(string name);
    }

    public interface IWeightInitialisation
    {
        IVector CreateBias(int size);
        IMatrix CreateWeight(int rows, int columns);
    }
}
