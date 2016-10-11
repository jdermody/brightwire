using BrightWire.Bayesian;
using BrightWire.Bayesian.Training;
using BrightWire.DimensionalityReduction;
using BrightWire.ErrorMetrics;
using BrightWire.Helper;
using BrightWire.Linear;
using BrightWire.Linear.Training;
using BrightWire.Models;
using System;
using System.Collections.Generic;
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

        public static ILinearRegressionTrainer CreateLinearRegressionTrainer(this IDataTable table, ILinearAlgebraProvider lap, int classColumnIndex)
        {
            return new RegressionTrainer(lap, table, classColumnIndex);
        }

        public static ILogisticRegressionTrainer CreateLogisticRegressionTrainer(this IDataTable table, ILinearAlgebraProvider lap, int classColumnIndex)
        {
            return new LogisticRegressionTrainer(lap, table, classColumnIndex);
        }

        public static NaiveBayes TrainNaiveBayes(this IDataTable table, int classColumnIndex)
        {
            return NaiveBayesTrainer.Train(table, classColumnIndex);
        }

        public static IRandomProjection CreateRandomProjection(this ILinearAlgebraProvider lap, int fixedSize, int reducedSize, int s = 3)
        {
            return new RandomProjection(lap, fixedSize, reducedSize, s);
        }

        public static IEnumerable<MarkovModelObservation2<T>> TrainMarkovModel2<T>(this IEnumerable<IEnumerable<T>> data)
        {
            var trainer = new MarkovModelTrainer2<T>();
            foreach (var sequence in data)
                trainer.Add(sequence);
            return trainer.All;
        }

        public static IEnumerable<MarkovModelObservation3<T>> TrainMarkovModel3<T>(this IEnumerable<IEnumerable<T>> data)
        {
            var trainer = new MarkovModelTrainer3<T>();
            foreach (var sequence in data)
                trainer.Add(sequence);
            return trainer.All;
        }

        public static BernoulliNaiveBayes TrainBernoulliNaiveBayes(this ClassificationSet data)
        {
            var trainer = new BernoulliNaiveBayesTrainer();
            foreach(var classification in data.Classifications)
                trainer.AddClassification(classification.Name, classification.Data);
            return trainer.Train();
        }

        public static MultinomialNaiveBayes TrainMultinomicalNaiveBayes(this ClassificationSet data)
        {
            var trainer = new MultinomialNaiveBayesTrainer();
            foreach (var classification in data.Classifications)
                trainer.AddClassification(classification.Name, classification.Data);
            return trainer.Train();
        }
    }
}
