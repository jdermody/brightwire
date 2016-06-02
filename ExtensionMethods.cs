using Icbld.BrightWire.ErrorMetrics;
using Icbld.BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Icbld.BrightWire
{
    public static class ExtensionMethods
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq)
        {
            var rnd = new Random();
            return seq.OrderBy(e => rnd.Next()).ToList();
        }

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
            }
            throw new NotImplementedException();
        }

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

        public static void WriteTo(this IMatrix matrix, XmlWriter writer, string name = null)
        {
            writer.WriteStartElement(name ?? "matrix");
            foreach (var item in matrix.Data)
                item.WriteTo("row", writer);
            writer.WriteEndElement();
        }
    }
}
