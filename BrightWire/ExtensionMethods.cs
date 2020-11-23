using BrightTable;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.Bayesian.Training;
using BrightWire.ExecutionGraph;

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

            return builder.Build();
        }
    }
}
