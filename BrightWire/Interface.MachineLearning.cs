using BrightData;
using BrightTable;
using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using BrightWire.ExecutionGraph;
using BrightWire.Models.Linear;

namespace BrightWire
{
	/// <summary>
	/// Error metrics used to quantify machine learning
	/// </summary>
	public interface IErrorMetric
	{
		/// <summary>
		/// True if the result should be formatted as a percentage
		/// </summary>
		bool DisplayAsPercentage { get; }

		/// <summary>
		/// Computes the error between the output vector and target vector
		/// </summary>
		/// <param name="output">The vector that was the output of the model</param>
		/// <param name="targetOutput">The vector that the model was expected to output</param>
		/// <returns></returns>
		float Compute(Vector<float> output, Vector<float> targetOutput);

		/// <summary>
		/// Calculates the gradient of the error function
		/// </summary>
		/// <param name="context">The graph context</param>
		/// <param name="output">The mini batch of output vectors</param>
		/// <param name="targetOutput">The mini batch of expected target vectors</param>
		/// <returns></returns>
		IFloatMatrix CalculateGradient(IGraphContext context, IFloatMatrix output, IFloatMatrix targetOutput);
	}

	/// <summary>
	/// Random projection
	/// </summary>
	public interface IRandomProjection : IDisposable
	{
		/// <summary>
		/// The size to reduce to
		/// </summary>
		uint Size { get; }

		/// <summary>
		/// The transformation matrix
		/// </summary>
		IFloatMatrix Matrix { get; }

		/// <summary>
		/// Reduces a vector
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		IFloatVector Compute(IFloatVector vector);

		/// <summary>
		/// Reduces a matrix
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		IFloatMatrix Compute(IFloatMatrix matrix);
	}

	/// <summary>
	/// Markov model trainer
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IMarkovModelTrainer<in T> : ICanSerialiseToStream
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
	public interface IMarkovModelTrainer2<T> : IMarkovModelTrainer<T> where T: notnull
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
	public interface IMarkovModelTrainer3<T> : IMarkovModelTrainer<T> where T : notnull
	{
		/// <summary>
		/// Gets all current observations
		/// </summary>
		MarkovModel3<T> Build();
	}

	/// <summary>
	/// Describes how to calculate the training error
	/// </summary>
	public enum TrainingErrorCalculation
	{
		/// <summary>
		/// Do not calculate the training error
		/// </summary>
		None,

		/// <summary>
		/// Compare the output against the target output and calculate the euclidean distance
		/// </summary>
		Fast,

		/// <summary>
		/// Execute the model against the training data
		/// </summary>
		TrainingData
	}

	/// <summary>
	/// Graph learning context
	/// </summary>
	public interface ILearningContext
	{
		/// <summary>
		/// The duration in seconds of the last epoch
		/// </summary>
		double EpochSeconds { get; }

		/// <summary>
		/// The duration in milliseconds of the last epoch
		/// </summary>
		long EpochMilliseconds { get; }

		/// <summary>
		/// The linear algebra provider
		/// </summary>
		ILinearAlgebraProvider LinearAlgebraProvider { get; }

		/// <summary>
		/// The index of the current epoch (starting from one)
		/// </summary>
		uint CurrentEpoch { get; }

		/// <summary>
		/// The current learning/training rate
		/// </summary>
		float LearningRate { get; set; }

		/// <summary>
		/// The learning rate adjusted with the current batch size
		/// </summary>
		float BatchLearningRate { get; }

		/// <summary>
		/// The current mini batch size
		/// </summary>
		uint BatchSize { get; set; }

		/// <summary>
		/// The total number of rows per epoch
		/// </summary>
		uint RowCount { get; }

		/// <summary>
		/// Stores an update to the model parameters
		/// </summary>
		/// <typeparam name="T">The type of update</typeparam>
		/// <param name="fromNode">The node that is affected by this update</param>
		/// <param name="update">The update</param>
		/// <param name="updater">Callback to execute the update</param>
		void StoreUpdate<T>(INode fromNode, T update, Action<T> updater);

		/// <summary>
		/// True if the graph should calculate training error
		/// </summary>
		TrainingErrorCalculation TrainingErrorCalculation { get; }

		/// <summary>
		/// True if updates are deferred until the mini batch is complete
		/// </summary>
		bool DeferUpdates { get; }

		/// <summary>
		/// Apply any deferred updates
		/// </summary>
		void ApplyUpdates();

		/// <summary>
		/// Start a new epoch
		/// </summary>
		void StartEpoch();

		/// <summary>
		/// End the current epoch
		/// </summary>
		void EndEpoch();

		/// <summary>
		/// Sets the number of rows
		/// </summary>
		/// <param name="rowCount">The number of rows per epoch</param>
		void SetRowCount(uint rowCount);

		/// <summary>
		/// Register the backpropagation to be deferred
		/// </summary>
		/// <param name="errorSignal">The error signal associated with this backpropagation (optional, can be null)</param>
		/// <param name="update">The callback to execute the backpropagation</param>
		void DeferBackpropagation(IGraphData? errorSignal, Action<IGraphData> update);

		/// <summary>
		/// Backpropagates the error signal across all deferred backpropagations
		/// </summary>
		/// <param name="signal">The backpropagation signal</param>
		/// <param name="maxDepth">The maximum depth to backpropagate the signal</param>
		void BackpropagateThroughTime(IGraphData signal, int maxDepth = int.MaxValue);

		/// <summary>
		/// Schedules a change in the learning rate the specified epoch
		/// </summary>
		/// <param name="atEpoch">The epoch to change the learning rate</param>
		/// <param name="newLearningRate">The learning rate to use at that epoch</param>
		void ScheduleLearningRate(uint atEpoch, float newLearningRate);

		/// <summary>
		/// Enable or disable node parameter updates
		/// </summary>
		/// <param name="node">The node to modify</param>
		/// <param name="enableUpdates">True if the node can make updates via backpropagation</param>
		void EnableNodeUpdates(INode node, bool enableUpdates);

		/// <summary>
		/// Resets the learning context
		/// </summary>
		void Clear();

		/// <summary>
		/// Sends the message to some output
		/// </summary>
		Action<string> MessageLog { get; set; }

		/// <summary>
		/// Fired before each epoch starts
		/// </summary>
		event Action<ILearningContext> BeforeEpochStarts;

		/// <summary>
		/// Fired after each epoch ends
		/// </summary>
		event Action<ILearningContext> AfterEpochEnds;

		/// <summary>
		/// Error metric to use when evaluating trainging progress
		/// </summary>
		IErrorMetric? ErrorMetric { get; set; }

		GraphFactory GraphFactory { get; }
	}

	/// <summary>
	/// Gradient descent optimisation
	/// </summary>
	public interface IGradientDescentOptimisation : IDisposable, ICanSerialise
	{
		/// <summary>
		/// Updates the matrix with the delta
		/// </summary>
		/// <param name="source">The matrix to update</param>
		/// <param name="delta">The delta matrix</param>
		/// <param name="context">The graph learning context</param>
		void Update(IFloatMatrix source, IFloatMatrix delta, ILearningContext context);
	}

	/// <summary>
	/// Creates a gradient descent optimisation
	/// </summary>
	public interface ICreateGradientDescent
	{
		/// <summary>
		/// Creates the gradient descent optimisation
		/// </summary>
		/// <param name="propertySet">The property set that contains initialisation parameters</param>
		IGradientDescentOptimisation Create(IPropertySet propertySet);
	}

	/// <summary>
	/// Creates gradient descent optimisations based on a matrix
	/// </summary>
	public interface ICreateTemplateBasedGradientDescent
	{
		/// <summary>
		/// Creates the gradient descent optimisation for a particular target matrix
		/// </summary>
		/// <param name="prev">Any other previously created gradient descent optimisation in this context</param>
		/// <param name="template">The instance of the matrix that will be updated</param>
		/// <param name="propertySet">The property set that contains initialisation parameters</param>
		/// <returns></returns>
		IGradientDescentOptimisation Create(IGradientDescentOptimisation prev, IFloatMatrix template, IPropertySet propertySet);
	}

	/// <summary>
	/// The current set of graph initialisation parameters
	/// </summary>
	public interface IPropertySet
	{
		/// <summary>
		/// The linear algebra provider to use
		/// </summary>
		ILinearAlgebraProvider LinearAlgebraProvider { get; }

		/// <summary>
		/// The weight initialiser to use
		/// </summary>
		IWeightInitialisation WeightInitialisation { get; set; }

		/// <summary>
		/// The gradient descent optimisation to use
		/// </summary>
		IGradientDescentOptimisation? GradientDescent { get; set; }

		/// <summary>
		/// The template based gradient descent optimisation to use
		/// </summary>
		ICreateTemplateBasedGradientDescent? TemplateGradientDescentDescriptor { get; set; }

		/// <summary>
		/// The descriptor to create new gradient descent optimisations
		/// </summary>
		ICreateGradientDescent? GradientDescentDescriptor { get; set; }

		/// <summary>
		/// Use the specified template based gradient descent optimisation
		/// </summary>
		/// <param name="descriptor"></param>
		/// <returns></returns>
		IPropertySet Use(ICreateTemplateBasedGradientDescent descriptor);

		/// <summary>
		/// Use the specified gradient descent optimisation
		/// </summary>
		/// <param name="descriptor"></param>
		/// <returns></returns>
		IPropertySet Use(ICreateGradientDescent descriptor);

		/// <summary>
		/// Use the specified gradient descent optimisation
		/// </summary>
		/// <param name="optimisation"></param>
		/// <returns></returns>
		IPropertySet Use(IGradientDescentOptimisation optimisation);

		/// <summary>
		/// Use the specified weight initialiser
		/// </summary>
		/// <param name="weightInit"></param>
		/// <returns></returns>
		IPropertySet Use(IWeightInitialisation weightInit);

		/// <summary>
		/// Clones the current property set
		/// </summary>
		/// <returns>Shallow copy of the current properties</returns>
		IPropertySet Clone();

		/// <summary>
		/// Gets a named property
		/// </summary>
		/// <typeparam name="T">The type of the property</typeparam>
		/// <param name="name">The property name</param>
		/// <param name="defaultValue">The value to use if the property has not been supplied</param>
		/// <returns></returns>
		T Get<T>(string name, T defaultValue = default);

		/// <summary>
		/// Sets a named property
		/// </summary>
		/// <typeparam name="T">The type of the property</typeparam>
		/// <param name="name">The property name</param>
		/// <param name="obj">The property value</param>
		/// <returns></returns>
		IPropertySet Set<T>(string name, T obj);

		/// <summary>
		/// Clears the named property
		/// </summary>
		/// <param name="name">The property name</param>
		void Clear(string name);
	}

	/// <summary>
	/// Gaussian weight initialisation type
	/// </summary>
	public enum GaussianVarianceCalibration
	{
		/// <summary>
		/// Variances are calibrated by dividing by the square root of the connection count
		/// </summary>
		SquareRootN,

		/// <summary>
		/// Variances are calibrated by multiplying by twice the square root of the connection count
		/// </summary>
		SquareRoot2N
	}

	/// <summary>
	/// Gaussian variance count
	/// </summary>
	public enum GaussianVarianceCount
	{
		/// <summary>
		/// No variance calibration is applied
		/// </summary>
		None,

		/// <summary>
		/// The count of incoming connections is used
		/// </summary>
		FanIn,

		/// <summary>
		/// The count of outgoing connections is used
		/// </summary>
		FanOut,

		/// <summary>
		/// The count incoming and outgoing connections is used
		/// </summary>
		FanInFanOut
	}

	/// <summary>
	/// Neural network weight initialiser
	/// </summary>
	public interface IWeightInitialisation
	{
		/// <summary>
		/// Creates the bias vector
		/// </summary>
		/// <param name="size">The size of the vector</param>
		IFloatVector CreateBias(uint size);

		/// <summary>
		/// Creates the weight matrix
		/// </summary>
		/// <param name="rows">Row count</param>
		/// <param name="columns">Column count</param>
		IFloatMatrix CreateWeight(uint rows, uint columns);
	}

	/// <summary>
	/// Logistic regression classifier
	/// </summary>
	public interface ILogisticRegressionClassifier : IDisposable
	{
        /// <summary>
		/// Outputs a list of values from 0 to 1 for each input data
		/// </summary>
		/// <param name="input">Input data</param>
		Vector<float> Predict(Matrix<float> input);
	}

	/// <summary>
	/// Linear regression predictor
	/// </summary>
	public interface ILinearRegressionPredictor : IDisposable
	{
		/// <summary>
		/// Predicts a value from input data
		/// </summary>
		/// <param name="input">The input data</param>
		float Predict(params float[] input);

        /// <summary>
		/// Bulk value prediction
		/// </summary>
		/// <param name="input">List of data to predict</param>
		/// <returns>List of predictions</returns>
		float[] Predict(float[][] input);
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
		LogisticRegression GradientDescent(uint iterations, float learningRate, float lambda = 0.1f, Func<float, bool>? costCallback = null);

		/// <summary>
		/// Computes the cost of the specified parameters
		/// </summary>
		/// <param name="theta">The model parameters</param>
		/// <param name="lambda">Regularisation lambda</param>
		/// <returns></returns>
		float ComputeCost(IFloatVector theta, float lambda);
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
		float ComputeCost(IFloatVector theta, float lambda);
	}

	/// <summary>
	/// Encodes index lists to dense vectors
	/// </summary>
	public interface IIndexListEncoder
	{
		/// <summary>
		/// Encodes the index lists to a dense vector
		/// </summary>
		/// <param name="indexList">The index list to encode</param>
		float[] Encode(IndexList indexList);
	}

	/// <summary>
	/// Encodes weighted index lists to dense vectors
	/// </summary>
	public interface IWeightedIndexListEncoder
	{
		/// <summary>
		/// Encodes the weighted index list to a dense vector
		/// </summary>
		/// <param name="indexList"></param>
		/// <returns></returns>
		float[] Encode(WeightedIndexList indexList);
	}

    /// <summary>
	/// A classifier that classifies index lists
	/// </summary>
	public interface IIndexListClassifier
	{
		/// <summary>
		/// Classifies the input data and returns the classifications with their weights
		/// </summary>
		/// <param name="indexList">The index list to classify</param>
        (string Label, float Weight)[] Classify(IndexList indexList);
	}

	public interface IRowClassifier
	{
        (string Label, float Weight)[] Classify(IConvertibleRow row);
	}

    public interface ITableClassifier
    {
        IEnumerable<(uint RowIndex, (string Classification, float Weight)[] Predictions)> Classify(IDataTable table);
	}
}
