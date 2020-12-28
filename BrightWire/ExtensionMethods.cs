using BrightTable;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.Adaptors;
using BrightWire.Bayesian.Training;
using BrightWire.ExecutionGraph;
using BrightWire.Models;

namespace BrightWire
{
    /// <summary>
    /// Static extension methods
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Create a markov model trainer of window size 2
        /// </summary>
        /// <typeparam name="T">The markov chain data type</typeparam>
        /// <param name="context"></param>
        /// <param name="minObservations">Minimum number of data points to record an observation</param>
        public static IMarkovModelTrainer2<T> CreateMarkovTrainer2<T>(this IBrightDataContext context, int minObservations = 1)
        {
            return new MarkovModelTrainer2<T>(minObservations);
        }

        /// <summary>
        /// Create a markov model trainer of window size 3
        /// </summary>
        /// <typeparam name="T">The markov chain data type</typeparam>
        /// <param name="context"></param>
        /// <param name="minObservations">Minimum number of data points to record an observation</param>
        public static IMarkovModelTrainer3<T> CreateMarkovTrainer3<T>(this IBrightDataContext context, int minObservations = 1)
        {
            return new MarkovModelTrainer3<T>(minObservations);
        }

        public static T[] GetFields<T>(this IConvertibleRow row, params uint[] indices)
        {
            return indices.Select(row.GetTyped<T>).ToArray();
        }

        public static IEnumerable<(IConvertibleRow Row, (string Label, float Weight)[] Classification)> Classify(this IRowOrientedDataTable dataTable, IRowClassifier classifier)
        {
            return Classify(dataTable.AsConvertible(), classifier);
        }

        public static IEnumerable<(IConvertibleRow Row, (string Label, float Weight)[] Classification)> Classify(this IConvertibleTable convertible, IRowClassifier classifier)
        {
            for (uint i = 0, len = convertible.DataTable.RowCount; i < len; i++) {
                var row = convertible.Row(i);
                yield return (row, classifier.Classify(row));
            }
        }

        public static IEnumerable<(IFloatVector Vector, uint RowIndex, string Label)> GetRowsAsLabeledFeatures(this IDataTable dataTable)
        {
            var lap = dataTable.Context.LinearAlgebraProvider;
            return dataTable.GetVectorisedFeatures()
                .Select((r, i) => (Vector: lap.CreateVector(r.Numeric), RowIndex: (uint) i, r.Label));
        }

        public static IEnumerable<(uint RowIndex, string Label)[]> HierachicalCluster(this IDataTable dataTable, int k)
        {
            var data = dataTable.GetRowsAsLabeledFeatures()
                .ToDictionary(d => d.Vector);
            return data.Keys.HierachicalCluster(k)
                .Select(c => c.Select(v => (data[v].RowIndex, data[v].Label)).ToArray());
        }

        public static IEnumerable<(uint RowIndex, string Label)[]> KMeans(this IDataTable dataTable, int k, int maxIterations = 1000, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            var data = dataTable.GetRowsAsLabeledFeatures()
                .ToDictionary(d => d.Vector);
            return data.Keys.KMeans(dataTable.Context, k, maxIterations, distanceMetric)
                .Select(c => c.Select(v => (data[v].RowIndex, data[v].Label)).ToArray());
        }

        public static IEnumerable<(uint RowIndex, string Label)[]> NonNegativeMatrixFactorisation(this IDataTable dataTable, int k, int maxIterations = 1000)
        {
            var lap = dataTable.Context.LinearAlgebraProvider;
            var data = dataTable.GetRowsAsLabeledFeatures()
                .ToDictionary(d => d.Vector);
            return data.Keys.NNMF(lap, k, maxIterations)
                .Select(c => c.Select(v => (data[v].RowIndex, data[v].Label)).ToArray());
        }

        public static uint GetOutputSizeOrThrow(this IDataSource dataSource) => dataSource.OutputSize ?? throw new Exception("Output size not defined");

        public static GraphFactory CreateGraphFactory(this IBrightDataContext context) => new GraphFactory(context.LinearAlgebraProvider);
        public static GraphFactory CreateGraphFactory(this ILinearAlgebraProvider lap) => new GraphFactory(lap);

        public static IRowOrientedDataTable CreateSequentialWindow(this IRowOrientedDataTable dataTable, uint windowSize, params uint[] columnIndices)
        {
            var builder = dataTable.Context.BuildTable();
            builder.AddColumn(ColumnType.Matrix, "Past");
            builder.AddColumn(ColumnType.Vector, "Future").SetTargetColumn(true);
            var convertible = dataTable.AsConvertible();
            var context = dataTable.Context;
            for (uint i = 0; i < dataTable.RowCount - windowSize - 1; i++) {
                var past = context.CreateMatrixFromRows(convertible
                    .Rows(windowSize.AsRange(i).ToArray())
                    .Select(r => context.CreateVector(r.GetFields<float>(columnIndices)))
                    .ToArray()
                );
                var target = context.CreateVector(convertible.Row(i + windowSize).GetFields<float>(columnIndices));
                builder.AddRow(past, target);
            }

            return builder.BuildRowOriented();
        }

        /// <summary>
        /// Creates a confusion matrix from two columns of a data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="actualClassificationColumnIndex">The column index of the actual classifications</param>
        /// <param name="expectedClassificationColumnIndex">The column index of the expected classifications</param>
        public static ConfusionMatrix CreateConfusionMatrix(this IConvertibleTable dataTable, uint actualClassificationColumnIndex, uint expectedClassificationColumnIndex)
        {
            var labels = new Dictionary<string, int>();
            var classifications = new Dictionary<int, Dictionary<int, uint>>();

            static int _GetIndex(string classification, Dictionary<string, int> table)
            {
                if (table.TryGetValue(classification, out var index))
                    return index;
                table.Add(classification, index = table.Count);
                return index;
            }

            dataTable.ForEachRow(r => {
                var actual = _GetIndex(r.GetTyped<string>(actualClassificationColumnIndex), labels);
                var expected = _GetIndex(r.GetTyped<string>(expectedClassificationColumnIndex), labels);
                if (!classifications.TryGetValue(expected, out var expectedClassification))
                    classifications.Add(expected, expectedClassification = new Dictionary<int, uint>());
                if (expectedClassification.TryGetValue(actual, out var actualClassification))
                    expectedClassification[actual] = actualClassification + 1;
                else
                    expectedClassification.Add(actual, 1);
            });

            return new ConfusionMatrix
            {
                ClassificationLabels = labels.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray(),
                Classifications = classifications.OrderBy(kv => kv.Key).Select(c => new ConfusionMatrix.ExpectedClassification
                {
                    ClassificationIndex = c.Key,
                    ActualClassifications = c.Value.OrderBy(kv => kv.Key).Select(c2 => new ConfusionMatrix.ActualClassification
                    {
                        ClassificationIndex = c2.Key,
                        Count = c2.Value
                    }).ToArray()
                }).ToArray()
            };
        }

        /// <summary>
        /// Classifies each row of the index classification list
        /// </summary>
        /// <param name="data"></param>
        /// <param name="classifier">The classifier to classify each item in the list</param>
        /// <returns></returns>
        public static IReadOnlyList<(string Label, string Classification, float Score)> Classify(this IReadOnlyList<(string Label, IndexList Data)> data, IIndexListClassifier classifier)
        {
            var ret = new List<(string Label, string Classification, float Score)>();
            foreach (var item in data)
            {
                var classification = classifier.Classify(item.Data).GetBestClassification();
                ret.Add((item.Label, classification, item.Label == classification ? 1f : 0f));
            }
            return ret;
        }

        public static IRowClassifier AsRowClassifier(this IIndexListClassifier classifier, uint columnIndex = 0, IIndexStrings indexer = null) => new IndexListRowClassifier(classifier, columnIndex, indexer);
    }
}
