using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.DataTable;
using BrightData.LinearAlgebra;
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
        /// <param name="_"></param>
        /// <param name="empty">Null value for T</param>
        /// <param name="minObservations">Minimum number of data points to record an observation</param>
        public static IMarkovModelTrainer2<T> CreateMarkovTrainer2<T>(this BrightDataContext _, T empty, int minObservations = 1) where T : notnull
        {
            return new MarkovModelTrainer2<T>(empty, minObservations);
        }

        /// <summary>
        /// Create a markov model trainer of window size 3
        /// </summary>
        /// <typeparam name="T">The markov chain data type</typeparam>
        /// <param name="_"></param>
        /// <param name="empty">Null value for T</param>
        /// <param name="minObservations">Minimum number of data points to record an observation</param>
        public static IMarkovModelTrainer3<T> CreateMarkovTrainer3<T>(this BrightDataContext _, T empty, int minObservations = 1) where T : notnull
        {
            return new MarkovModelTrainer3<T>(empty, minObservations);
        }

        /// <summary>
        /// Classifies each row in the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="classifier"></param>
        /// <returns></returns>
        public static IEnumerable<(TableRow Row, (string Label, float Weight)[] Classification)> Classify(this IDataTable dataTable, IRowClassifier classifier)
        {
            for (uint i = 0, len = dataTable.RowCount; i < len; i++) {
                var row = dataTable[i];
                yield return (row, classifier.Classify(row));
            }
        }

        /// <summary>
        /// Enumerates rows in the table as vectorized rows
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="oneHotEncode"></param>
        /// <returns></returns>
        public static IEnumerable<(IReadOnlyNumericSegment<float> Vector, uint RowIndex, string? Label)> GetRowsAsLabeledFeatures(this IDataTable dataTable, bool oneHotEncode)
        {
            return dataTable.GetVectorisedFeatures(oneHotEncode)
                .ToBlockingEnumerable()
                .Select((r, i) => (Vector: r.Numeric, RowIndex: (uint) i, r.Label));
        }

        /// <summary>
        /// Clusters the rows in the data table using hierarchical clustering
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="k">Number of clusters</param>
        /// <returns></returns>
        public static IEnumerable<(uint RowIndex, string? Label)[]> HierarchicalCluster(this IDataTable dataTable, uint k)
        {
            var vectors = new IReadOnlyVector[dataTable.RowCount];
            var labels = new string?[dataTable.RowCount];
            foreach (var (vector, rowIndex, label) in dataTable.GetRowsAsLabeledFeatures(true)) {
                vectors[rowIndex] = vector.ToReadOnlyVector();
                labels[rowIndex] = label;
            }
            return vectors.HierarchicalCluster(k)
                .Select(c => c.Select(v => (v, labels[v])).ToArray());
        }

        /// <summary>
        /// Clusters the rows in the data table using k-means clustering
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxIterations">Maximum number of iterations</param>
        /// <param name="distanceMetric">Distance metric to use</param>
        /// <returns></returns>
        public static IEnumerable<(uint RowIndex, string? Label)[]> KMeans(this IDataTable dataTable, uint k, uint maxIterations = 1000, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            var vectors = new IReadOnlyVector[dataTable.RowCount];
            var labels = new string?[dataTable.RowCount];
            foreach (var (vector, rowIndex, label) in dataTable.GetRowsAsLabeledFeatures(true)) {
                vectors[rowIndex] = vector.ToReadOnlyVector();
                labels[rowIndex] = label;
            }
            var clusters = vectors.KMeansCluster(dataTable.Context, k, maxIterations, distanceMetric);
            var ret = clusters.Select(c => c.Select(v => (v, labels[v])).ToArray());
            return ret;
        }

        /// <summary>
        /// Clusters the rows in the data table using non-negative matrix factorisation clustering
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxIterations">Maximum number of iterations</param>
        /// <returns></returns>
        public static IEnumerable<(uint RowIndex, string? Label)[]> NonNegativeMatrixFactorisation(this IDataTable dataTable, uint k, uint maxIterations = 1000)
        {
            var lap = dataTable.Context.LinearAlgebraProvider;
            var vectors = new IReadOnlyVector[dataTable.RowCount];
            var labels = new string?[dataTable.RowCount];
            foreach (var (vector, rowIndex, label) in dataTable.GetRowsAsLabeledFeatures(true)) {
                vectors[rowIndex] = vector.ToReadOnlyVector();
                labels[rowIndex] = label;
            }
            return vectors.Nnmf(lap, k, maxIterations)
                .Select(c => c.Select(v => (v, labels[v])).ToArray());
        }

        /// <summary>
        /// Returns the output size (throw exception if not set)
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public static uint GetOutputSizeOrThrow(this IDataSource? dataSource) => dataSource?.OutputSize ?? throw new Exception("Output size not defined");

        /// <summary>
        /// Creates a graph factory
        /// </summary>
        /// <param name="context"></param>
        /// <param name="lap">Linear algebra provider (optional)</param>
        /// <returns></returns>
        public static GraphFactory CreateGraphFactory(this BrightDataContext context, LinearAlgebraProvider? lap = null) => new(lap ?? context.LinearAlgebraProvider);


        /// <summary>
        /// Creates a graph factory
        /// </summary>
        /// <param name="lap"></param>
        /// <returns></returns>
        public static GraphFactory CreateGraphFactory(this LinearAlgebraProvider lap) => new(lap);

        /// <summary>
        /// Creates a matrix to vector training table in which the matrix contains a window of sequentially ordered rows
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="windowSize">The number of rows in each matrix</param>
        /// <param name="columnIndices">Column indices to select</param>
        /// <returns></returns>
        public static async Task<IDataTable> CreateSequentialWindow(this IDataTable dataTable, uint windowSize, params uint[] columnIndices)
        {
            var builder = dataTable.Context.CreateTableBuilder();
            var hasAddedColumns = false;
            var context = dataTable.Context;
            var rows = await dataTable.GetRows();
            for (uint i = 0; i < dataTable.RowCount - windowSize - 1; i++) {
                var past = context.CreateReadOnlyMatrixFromRows(windowSize.AsRange(i).Select(j => rows[j])
                    .Select(r => context.CreateReadOnlyVector(r.GetMany<float>(columnIndices)))
                    .ToArray()
                );
                var targetRow = rows[i + windowSize];
                var target = context.CreateReadOnlyVector(targetRow.GetMany<float>(columnIndices));
                if (!hasAddedColumns) {
                    hasAddedColumns = true;
                    builder.CreateFixedSizeMatrixColumn(past.RowCount, past.ColumnCount, "Past");
                    builder.CreateFixedSizeVectorColumn(target.Size, "Future").MetaData.SetTarget(true);
                }
                builder.AddRow(past, target);
            }

            return await builder.BuildInMemory();
        }

        /// <summary>
        /// Creates a confusion matrix from two columns of a data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="actualClassificationColumnIndex">The column index of the actual classifications</param>
        /// <param name="expectedClassificationColumnIndex">The column index of the expected classifications</param>
        public static async Task<ConfusionMatrix> CreateConfusionMatrix(this IDataTable dataTable, uint actualClassificationColumnIndex, uint expectedClassificationColumnIndex)
        {
            var labels = new Dictionary<string, int>();
            var classifications = new Dictionary<int, Dictionary<int, uint>>();

            static int GetIndex(string classification, Dictionary<string, int> table)
            {
                if (table.TryGetValue(classification, out var index))
                    return index;
                table.Add(classification, index = table.Count);
                return index;
            }

            await foreach(var row in dataTable.EnumerateRows()) {
                var actual = GetIndex(row.Get<string>(actualClassificationColumnIndex), labels);
                var expected = GetIndex(row.Get<string>(expectedClassificationColumnIndex), labels);
                if (!classifications.TryGetValue(expected, out var expectedClassification))
                    classifications.Add(expected, expectedClassification = new Dictionary<int, uint>());
                if (expectedClassification.TryGetValue(actual, out var actualClassification))
                    expectedClassification[actual] = actualClassification + 1;
                else
                    expectedClassification.Add(actual, 1);
            }

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
        public static (string Label, string Classification, float Score)[] Classify(this IndexListWithLabel<string>[] data, IIndexListClassifier classifier)
        {
            var i = 0;
            var ret = new (string Label, string Classification, float Score)[data.Length];
            foreach (var (label, indexList) in data)
            {
                var classification = classifier.Classify(indexList).GetBestClassification();
                ret[i++] = (label, classification, label == classification ? 1f : 0f);
            }
            return ret;
        }

        /// <summary>
        /// Converts the index list classifier to a row classifier
        /// </summary>
        /// <param name="classifier">Index list classifier</param>
        /// <param name="columnIndex">Column index to classify</param>
        /// <param name="indexer">String indexer (optional)</param>
        /// <returns></returns>
        public static IRowClassifier AsRowClassifier(this IIndexListClassifier classifier, uint columnIndex = 0, IIndexStrings? indexer = null) => new IndexListRowClassifier(classifier, columnIndex, indexer);
    }
}
