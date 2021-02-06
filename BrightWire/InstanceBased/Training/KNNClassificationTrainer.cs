using System;
using System.Linq;
using BrightData;
using BrightWire.Models.InstanceBased;

namespace BrightWire.InstanceBased.Training
{
    /// <summary>
    /// K Nearest Neighbour classification trainer
    /// </summary>
    internal static class KNNClassificationTrainer
    {
        public static KNearestNeighbours Train(IDataTable table)
        {
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var featureColumns = table.ColumnIndicesOfFeatures().ToArray();
            var vectoriser = table.GetVectoriser(featureColumns);
            var data = vectoriser.Enumerate().ToArray();

            return new KNearestNeighbours {
                Instance = data,
                Classification = table.Column(targetColumnIndex).Enumerate().Select(v => v.ToString() ?? throw new Exception("Cannot convert to string")).ToArray(),
                DataColumns = featureColumns,
                TargetColumn = targetColumnIndex
            };
        }
    }
}
