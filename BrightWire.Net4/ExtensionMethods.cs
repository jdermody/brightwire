using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static (IReadOnlyList<T> Training, IReadOnlyList<T> Test) Split<T>(this IReadOnlyList<T> seq, double trainPercentage = 0.8)
        {
            var input = Enumerable.Range(0, seq.Count).ToList();
            int trainingCount = Convert.ToInt32(seq.Count * trainPercentage);
            return (
                input.Take(trainingCount).Select(i => seq[i]).ToArray(),
                input.Skip(trainingCount).Select(i => seq[i]).ToArray()
            );
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
        /// Returns the underlying .net type associated with the column type
        /// </summary>
        /// <param name="type">The column type</param>
        public static Type GetColumnType(this ColumnType type)
        {
            switch (type) {
                case ColumnType.Boolean:
                    return typeof(bool);

                case ColumnType.Byte:
                    return typeof(byte);

                case ColumnType.Date:
                    return typeof(DateTime);

                case ColumnType.Double:
                    return typeof(double);

                case ColumnType.Float:
                    return typeof(float);

                case ColumnType.Int:
                    return typeof(int);

                case ColumnType.Long:
                    return typeof(long);

                case ColumnType.Null:
                    return null;

                case ColumnType.String:
                    return typeof(string);

                case ColumnType.IndexList:
                    return typeof(IndexList);

                case ColumnType.WeightedIndexList:
                    return typeof(WeightedIndexList);

                case ColumnType.Vector:
                    return typeof(FloatVector);

                case ColumnType.Matrix:
                    return typeof(FloatMatrix);

                case ColumnType.Tensor:
                    return typeof(FloatTensor);

                default:
                    throw new NotImplementedException();
            }
        }

        public static IEnumerable<float> Compute(this IErrorMetric errorMetric, IReadOnlyList<(IIndexableVector, IIndexableVector)> output)
        {
            return output.Select(r => errorMetric.Compute(r.Item1, r.Item2));
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
    }
}
