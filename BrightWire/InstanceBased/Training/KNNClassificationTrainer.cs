using System;
using System.Linq;
using BrightData;
using BrightData.DataTable2;
using BrightWire.Models.InstanceBased;

namespace BrightWire.InstanceBased.Training
{
    /// <summary>
    /// K Nearest Neighbour classification trainer
    /// </summary>
    internal static class KnnClassificationTrainer
    {
        public static KNearestNeighbours Train(BrightDataTable table)
        {
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var featureColumns = table.ColumnIndicesOfFeatures().ToArray();
            using var vectoriser = table.GetVectoriser(true, featureColumns);
            using var columnReader = table.ReadColumn(targetColumnIndex);

            return new KNearestNeighbours {
                Instance = vectoriser.Enumerate().Select(d => d.Segment.ToNewArray()).ToArray(),
                Classification = columnReader.Enumerate().Select(v => v.ToString() ?? throw new Exception("Cannot convert to string")).ToArray(),
                DataColumns = featureColumns,
                TargetColumn = targetColumnIndex
            };
        }
    }
}
