using BrightWire.Bayesian;
using BrightWire.Bayesian.Training;
using BrightWire.DimensionalityReduction;
using BrightWire.ErrorMetrics;
using BrightWire.Helper;
using BrightWire.InstanceBased.Trainer;
using BrightWire.Linear;
using BrightWire.Linear.Training;
using BrightWire.Models;
using BrightWire.Models.Simple;
using BrightWire.TabularData.Helper;
using BrightWire.TreeBased.Training;
using BrightWire.Unsupervised.Clustering;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BrightWire
{
    /// <summary>
    /// Static extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Shuffles the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq">The sequence to shuffle</param>
        /// <param name="randomSeed">The random seed or null initialise randomlu</param>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq, int? randomSeed = null)
        {
            var rnd = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            return seq.OrderBy(e => rnd.Next()).ToList();
        }

        /// <summary>
        /// Splits a sequence into training and test splits
        /// </summary>
        /// <typeparam name="T">The type of the sequence</typeparam>
        /// <param name="seq">The sequence to split</param>
        /// <param name="trainPercentage">The percentage of the sequence to add to the training set</param>
        public static SequenceSplit<T> Split<T>(this IReadOnlyList<T> seq, double trainPercentage = 0.8)
        {
            var input = Enumerable.Range(0, seq.Count).ToList();
            int trainingCount = Convert.ToInt32(seq.Count * trainPercentage);
            return new SequenceSplit<T> {
                Training = input.Take(trainingCount).Select(i => seq[i]).ToArray(),
                Test = input.Skip(trainingCount).Select(i => seq[i]).ToArray()
            };
        }

        /// <summary>
        /// Bags (select with replacement) the input sequence
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The input sequence</param>
        /// <param name="count">The size of the output sequence</param>
        /// <param name="randomSeed">The random seed or null initialise randomlu</param>
        /// <returns></returns>
        public static IReadOnlyList<T> Bag<T>(this IReadOnlyList<T> list, int count, int? randomSeed = null)
        {
            var rnd = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            return Enumerable.Range(0, count)
                .Select(i => list[rnd.Next(0, list.Count)])
                .ToList()
            ;
        }

        /// <summary>
        /// Creates an error metric provider from an error metric descriptor
        /// </summary>
        /// <param name="type">The type of error metric</param>
        /// <returns></returns>
        public static IErrorMetric Create(this ErrorMetricType type)
        {
            switch(type) {
                case ErrorMetricType.OneHot:
                    return new OneHot();
                case ErrorMetricType.RMSE:
                    return new RMSE();
                case ErrorMetricType.BinaryClassification:
                    return new BinaryClassification();
                case ErrorMetricType.CrossEntropy:
                    return new CrossEntropy();
                case ErrorMetricType.Quadratic:
                    return new Quadratic();
                case ErrorMetricType.None:
                    return null;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the distance between two vectors
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        public static float Calculate(this DistanceMetric distance, IVector vector1, IVector vector2)
        {
            switch (distance) {
                case DistanceMetric.Cosine:
                    return vector1.CosineDistance(vector2);
                case DistanceMetric.Euclidean:
                    return vector1.EuclideanDistance(vector2);
                case DistanceMetric.Manhattan:
                    return vector1.ManhattanDistance(vector2);
                case DistanceMetric.SquaredEuclidean:
                    return vector1.SquaredEuclidean(vector2);
                default:
                    return vector1.MeanSquaredDistance(vector2);
            }
        }

        /// <summary>
        /// Calculates the distance between two matrices
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="matrix1"></param>
        /// <param name="matrix2"></param>
        /// <returns></returns>
        public static IVector Calculate(this DistanceMetric distance, IMatrix matrix1, IMatrix matrix2)
        {
            switch (distance) {
                case DistanceMetric.Euclidean:
                    using (var diff = matrix1.Subtract(matrix2))
                    using (var diffSquared = diff.PointwiseMultiply(diff))
                    using (var rowSums = diffSquared.RowSums()) {
                        return rowSums.Sqrt();
                    }
                case DistanceMetric.SquaredEuclidean:
                    using (var diff = matrix1.Subtract(matrix2))
                    using (var diffSquared = diff.PointwiseMultiply(diff)) {
                        return diffSquared.RowSums();
                    }
                case DistanceMetric.Cosine:
                case DistanceMetric.Manhattan:
                case DistanceMetric.MeanSquared:
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Writes a matrix to an XmlWriter
        /// </summary>
        /// <param name="matrix">The matrix to write</param>
        /// <param name="writer"></param>
        /// <param name="name">The name to give the matrix</param>
        public static void WriteTo(this IMatrix matrix, XmlWriter writer, string name = null)
        {
            writer.WriteStartElement(name ?? "matrix");
            foreach (var item in matrix.Data)
                item.WriteTo("row", writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Linear regression fits a line to a set of data that allows you predict future values
        /// </summary>
        /// <param name="table">The training data table</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns>A trainer that can be used to build a linear regression model</returns>
        public static ILinearRegressionTrainer CreateLinearRegressionTrainer(this IDataTable table, ILinearAlgebraProvider lap)
        {
            return new RegressionTrainer(lap, table);
        }

        /// <summary>
        /// Logistic regression learns a sigmoid function over a set of data that learns to classify future values into positive or negative samples
        /// </summary>
        /// <param name="table">The training data provider</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns>A trainer that can be used to build a logistic regression model</returns>
        public static ILogisticRegressionTrainer CreateLogisticRegressionTrainer(this IDataTable table, ILinearAlgebraProvider lap)
        {
            return new LogisticRegressionTrainer(lap, table);
        }

        /// <summary>
        /// Trains a logistic regression model on a data table
        /// </summary>
        /// <param name="table">The training data</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="iterations">Number of iterations to train for</param>
        /// <param name="learningRate">The learning rate</param>
        /// <param name="lambda">Regularisation lambda</param>
        /// <returns>The trained model</returns>
        public static LogisticRegression TrainLogisticRegression(this IDataTable table, ILinearAlgebraProvider lap, int iterations, float learningRate, float lambda = 0.1f)
        {
            var trainer = table.CreateLogisticRegressionTrainer(lap);
            return trainer.GradientDescent(iterations, learningRate, lambda);
        }

        /// <summary>
        /// Naive bayes is a classifier that assumes conditional independence between all features
        /// </summary>
        /// <param name="table">The training data provider</param>
        /// <returns>A naive bayes model</returns>
        public static NaiveBayes TrainNaiveBayes(this IDataTable table)
        {
            return NaiveBayesTrainer.Train(table);
        }

        /// <summary>
        /// Random projections allow you to reduce the dimensions of a matrix while still preserving significant information
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="fixedSize">The vector size to reduce from</param>
        /// <param name="reducedSize">The vector size to reduce to</param>
        /// <param name="s"></param>
        public static IRandomProjection CreateRandomProjection(this ILinearAlgebraProvider lap, int fixedSize, int reducedSize, int s = 3)
        {
            return new RandomProjection(lap, fixedSize, reducedSize, s);
        }

        ///// <summary>
        ///// Random projections allow you to reduce the dimensions of a matrix while still preserving significant information
        ///// </summary>
        ///// <param name="lap">Linear algebra provider</param>
        ///// <param name="inputSize">The vector size to reduce from</param>
        ///// <returns></returns>
        //public static IRandomProjection CreateRandomProjection(this ILinearAlgebraProvider lap, int inputSize)
        //{
        //    var reducedSize = RandomProjection.MinDim(inputSize);
        //    return CreateRandomProjection(lap, inputSize, reducedSize);
        //}

        ///// <summary>
        ///// Markov models summarise sequential data (over a window of size 2)
        ///// </summary>
        ///// <typeparam name="T">The data type within the model</typeparam>
        ///// <param name="data">An enumerable of sequences of type T</param>
        ///// <returns>A sequence of markov model observations</returns>
        //public static MarkovModel2<T> TrainMarkovModel2<T>(this IEnumerable<IEnumerable<T>> data)
        //{
        //    var trainer = new MarkovModelTrainer2<T>();
        //    foreach (var sequence in data)
        //        trainer.Add(sequence);
        //    return trainer.Build();
        //}

        ///// <summary>
        ///// Markov models summarise sequential data (over a window of size 3)
        ///// </summary>
        ///// <typeparam name="T">The data type within the model</typeparam>
        ///// <param name="data">An enumerable of sequences of type T</param>
        ///// <returns>A sequence of markov model observations</returns>
        //public static MarkovModel3<T> TrainMarkovModel3<T>(this IEnumerable<IEnumerable<T>> data)
        //{
        //    var trainer = new MarkovModelTrainer3<T>();
        //    foreach (var sequence in data)
        //        trainer.Add(sequence);
        //    return trainer.Build();
        //}

        /// <summary>
        /// Bernoulli naive bayes treats each feature as either 1 or 0 - all feature counts are discarded. Useful for short documents.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <returns>A model that can be used for classification</returns>
        public static BernoulliNaiveBayes TrainBernoulliNaiveBayes(this ClassificationBag data)
        {
            var trainer = new BernoulliNaiveBayesTrainer();
            foreach(var classification in data.Classifications)
                trainer.AddClassification(classification.Name, classification.Data);
            return trainer.Train();
        }

        /// <summary>
        /// Multinomial naive bayes preserves the count of each feature within the model. Useful for long documents.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <returns>A model that can be used for classification</returns>
        public static MultinomialNaiveBayes TrainMultinomialNaiveBayes(this ClassificationBag data)
        {
            var trainer = new MultinomialNaiveBayesTrainer();
            foreach (var classification in data.Classifications)
                trainer.AddClassification(classification.Name, classification.Data);
            return trainer.Train();
        }

        /// <summary>
        /// Decision trees build a logical tree to classify data. Various measures can be specified to prevent overfitting.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="minDataPerNode">Minimum number of data points per node to continue splitting</param>
        /// <param name="maxDepth">The maximum depth of each leaf</param>
        /// <param name="minInformationGain">The minimum information gain to continue splitting</param>
        /// <returns>A model that can be used for classification</returns>
        public static DecisionTree TrainDecisionTree(this IDataTable data, int? minDataPerNode = null, int? maxDepth = null, double? minInformationGain = null)
        {
            var config = new DecisionTreeTrainer.Config {
                MinDataPerNode = minDataPerNode,
                MaxDepth = maxDepth,
                MinInformationGain = minInformationGain
            };
            return DecisionTreeTrainer.Train(data, config);
        }

        /// <summary>
        /// Random forests are built on a bagged collection of features to try to capture the most salient points of the training data without overfitting
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="b">The number of trees in the forest</param>
        /// <returns>A model that can be used for classification</returns>
        public static RandomForest TrainRandomForest(this IDataTable data, int b = 100)
        {
            return RandomForestTrainer.Train(data, b);
        }

        /// <summary>
        /// Multinomial Logistic Regression generalises Logistic Regression to multi-class classification
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="trainingIterations">Number of training iterations</param>
        /// <param name="trainingRate">Training rate</param>
        /// <param name="lambda">L2 regularisation</param>
        /// <returns></returns>
        public static MultinomialLogisticRegression TrainMultinomialLogisticRegression(this IDataTable data, ILinearAlgebraProvider lap, int trainingIterations, float trainingRate, float lambda = 0.1f)
        {
            return MultinomialLogisticRegressionTrainner.Train(data, lap, trainingIterations, trainingRate, lambda);
        }

        /// <summary>
        /// K Nearest Neighbours is an instance based classification method that uses examples from training data to predict classifications
        /// </summary>
        /// <param name="data">The training data</param>
        public static KNearestNeighbours TrainKNearestNeighbours(this IDataTable data)
        {
            return KNNClassificationTrainer.Train(data);
        }

        /// <summary>
        /// K Means uses coordinate descent and the euclidean distance between randomly selected centroids to cluster the data
        /// </summary>
        /// <param name="data">The list of vectors to cluster</param>
        /// <param name="k">The number of clusters to find</param>
        /// <param name="maxIterations">The maximum number of iterations</param>
        /// <returns>A list of k clusters</returns>
        public static IReadOnlyList<IReadOnlyList<IVector>> KMeans(this IReadOnlyList<IVector> data, int k, int maxIterations = 1000)
        {
            using(var clusterer = new KMeans(k, data, DistanceMetric.Euclidean)) {
                clusterer.ClusterUntilConverged(maxIterations);
                return clusterer.Clusters;
            }
        }

        /// <summary>
        /// Hierachical clustering successively finds the closest distance between pairs of centroids until k is reached
        /// </summary>
        /// <param name="data">The list of vectors to cluster</param>
        /// <returns>A list of k clusters</returns>
        public static IReadOnlyList<IReadOnlyList<IVector>> HierachicalCluster(this IReadOnlyList<IVector> data, int k)
        {
            using (var clusterer = new Hierachical(k, data, DistanceMetric.Euclidean)) {
                clusterer.Cluster();
                return clusterer.Clusters;
            }
        }

        /// <summary>
        /// Non negative matrix factorisation - clustering based on the factorisation of non-negative matrices. Only applicable for training data that is non-negative.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="lap">Linear alegbra provider</param>
        /// <param name="k">The number of clusters</param>
        /// <param name="maxIterations">The maximum number of iterations</param>
        /// <returns>A list of k clusters</returns>
        public static IReadOnlyList<IReadOnlyList<IVector>> NNMF(this IReadOnlyList<IVector> data, ILinearAlgebraProvider lap, int k, int maxIterations = 1000)
        {
            var clusterer = new NonNegativeMatrixFactorisation(lap, k);
            return clusterer.Cluster(data, maxIterations);
        }

        /// <summary>
        /// Parses a CSV file into a data table
        /// </summary>
        /// <param name="streamReader">The stream of CSV data</param>
        /// <param name="delimeter">The CSV delimeter</param>
        /// <param name="hasHeader">True if there is a header</param>
        /// <param name="output">A stream to write the data table to (for file based processing) - null for in memory processing</param>
        public static IDataTable ParseCSV(this StreamReader streamReader, char delimeter = ',', bool? hasHeader = null, Stream output = null)
        {
            var builder = new CSVDataTableBuilder(delimeter);
            return builder.Parse(streamReader, output, hasHeader);
        }

        /// <summary>
        /// Find the next set of state transitions from a pair of observations
        /// </summary>
        /// <typeparam name="T">The type of the model</typeparam>
        /// <param name="model">A markov model saved to a dictionary</param>
        /// <param name="item1">The first observation</param>
        /// <param name="item2">The second observation</param>
        /// <returns>The list of state transitions or null if nothing was found</returns>
        public static List<MarkovModelStateTransition<T>> GetTransitions<T>(this Dictionary<MarkovModelObservation2<T>, List<MarkovModelStateTransition<T>>> model, T item1, T item2)
        {
            var observation = new MarkovModelObservation2<T> {
                Item1 = item1,
                Item2 = item2
            };
            List<MarkovModelStateTransition<T>> ret;
            if (model.TryGetValue(observation, out ret))
                return ret;
            return null;
        }

        /// <summary>
        /// Find the next set of state transitions from a tuple of observations
        /// </summary>
        /// <typeparam name="T">The type of the model</typeparam>
        /// <param name="model">A markov model saved to a dictionary</param>
        /// <param name="item1">The first observation</param>
        /// <param name="item2">The second observation</param>
        /// <param name="item3">The third observation</param>
        /// <returns>The list of state transitions or null if nothing was found</returns>
        public static List<MarkovModelStateTransition<T>> GetTransitions<T>(this Dictionary<MarkovModelObservation3<T>, List<MarkovModelStateTransition<T>>> model, T item1, T item2, T item3)
        {
            var observation = new MarkovModelObservation3<T> {
                Item1 = item1,
                Item2 = item2,
                Item3 = item3
            };
            List<MarkovModelStateTransition<T>> ret;
            if (model.TryGetValue(observation, out ret))
                return ret;
            return null;
        }
    }
}
