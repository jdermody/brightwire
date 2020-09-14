using BrightTable;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightTable.Transformations;
using BrightWire.ExecutionGraph;

namespace BrightWire
{
    /// <summary>
    /// Static extension methods
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Shuffles the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq">The sequence to shuffle</param>
        /// <param name="randomSeed">The random seed to use or null for a random shuffle</param>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq, int? randomSeed = null)
        {
            var rnd = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            return Shuffle(seq, rnd);
        }

        /// <summary>
        /// Shuffles the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq">The sequence to shuffle</param>
        /// <param name="rnd">The random number generator to use</param>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq, Random rnd)
        {
            return seq.OrderBy(e => rnd.Next()).ToList();
        }

        /// <summary>
        /// Splits a sequence into training and test splits
        /// </summary>
        /// <typeparam name="T">The type of the sequence</typeparam>
        /// <param name="seq">The sequence to split</param>
        /// <param name="trainPercentage">The percentage of the sequence to add to the training set</param>
        public static (T[] Training, T[] Test) Split<T>(this T[] seq, double trainPercentage = 0.8)
        {
            var input = seq.Length.AsRange().ToList();
            int trainingCount = Convert.ToInt32(seq.Length * trainPercentage);
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
        public static T[] Bag<T>(this T[] list, int count, int? randomSeed = null)
        {
            var rnd = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            return Enumerable.Range(0, count)
                .Select(i => list[rnd.Next(0, list.Length)])
                .ToArray()
            ;
        }

        public static T[] GetFields<T>(this IConvertibleRow row, params uint[] indices)
        {
            return indices.Select(i => row.GetField<T>(i)).ToArray();
        }

        public static IEnumerable<(IConvertibleRow Row, (string Label, float Weight)[] Classification)> Classify(this IRowOrientedDataTable dataTable, IRowClassifier classifier)
        {
            return Classify(dataTable.AsConvertible(), classifier);
        }

        public static IEnumerable<(IConvertibleRow Row, (string Label, float Weight)[] Classification)> Classify(this IConvertibleTable convertible, IRowClassifier classifier)
        {
            for (uint i = 0, len = convertible.DataTable.RowCount; i < len; i++) {
                var row = convertible.GetRow(i);
                yield return (row, classifier.Classify(row));
            }
        }
    }
}
