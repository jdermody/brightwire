using System;
using System.Linq;
using BrightData;
using BrightWire.Models.InstanceBased;

namespace BrightWire.InstanceBased.Training
{
    /// <summary>
    /// K Nearest Neighbour classification trainer
    /// </summary>
    internal static class KnnClassificationTrainer
    {
        public static KNearestNeighbours Train(IDataTable table)
        {
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var featureColumns = table.ColumnIndicesOfFeatures().ToArray();
            var vectoriser = table.GetVectoriser(true, featureColumns);
            var data = vectoriser.Enumerate().ToArray();

            return new KNearestNeighbours {
                Instance = data.Select(d => d.AsFloatVector()).ToArray(),
                Classification = table.Column(targetColumnIndex).Enumerate().Select(v => v.ToString() ?? throw new Exception("Cannot convert to string")).ToArray(),
                DataColumns = featureColumns,
                TargetColumn = targetColumnIndex
            };
        }
    }
}
