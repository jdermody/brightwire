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
        public static T[] GetFields<T>(this IConvertibleRow row, params uint[] indices)
        {
            return indices.Select(row.GetField<T>).ToArray();
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

        public static IEnumerable<uint[]> HierachicalCluster(this IDataTable dataTable, int k)
        {
            var lap = dataTable.Context.LinearAlgebraProvider;
            var data = dataTable.ForEachAsFloat()
                .Select((r, i) => (Vector: lap.CreateVector(r.Numeric), RowIndex: (uint)i))
                .ToDictionary(d => d.Vector);
            return data.Keys.HierachicalCluster(k)
                .Select(c => c.Select(v => data[v].RowIndex).ToArray());
        }

        public static IEnumerable<uint[]> KMeans(this IDataTable dataTable, int k, int maxIterations = 1000, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            var lap = dataTable.Context.LinearAlgebraProvider;
            var data = dataTable.ForEachAsFloat()
                .Select((r, i) => (Vector: lap.CreateVector(r.Numeric), RowIndex: (uint)i))
                .ToDictionary(d => d.Vector);
            return data.Keys.KMeans(k, maxIterations, distanceMetric)
                .Select(c => c.Select(v => data[v].RowIndex).ToArray());
        }

        public static IEnumerable<uint[]> NonNegativeMatrixFactorisation(this IDataTable dataTable, int k, int maxIterations = 1000)
        {
            var lap = dataTable.Context.LinearAlgebraProvider;
            var data = dataTable.ForEachAsFloat()
                .Select((r, i) => (Vector: lap.CreateVector(r.Numeric), RowIndex: (uint)i))
                .ToDictionary(d => d.Vector);
            return data.Keys.NNMF(lap, k, maxIterations)
                .Select(c => c.Select(v => data[v].RowIndex).ToArray());
        }
    }
}
