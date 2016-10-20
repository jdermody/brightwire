using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.InstanceBased.Trainer
{
    /// <summary>
    /// K Nearest Neighbour classification trainer
    /// </summary>
    internal static class KNNClassificationTrainer
    {
        public static KNearestNeighbours Train(IDataTable table)
        {
            var featureColumns = Enumerable.Range(0, table.ColumnCount).Where(i => i != table.TargetColumnIndex).ToList();
            var data = table.GetNumericRows(featureColumns);
            var labels = table.GetColumn<string>(table.TargetColumnIndex);

            return new KNearestNeighbours {
                Instance = data.Select(v => new FloatArray { Data = v }).ToArray(),
                Classification = labels.ToArray(),
                FeatureColumn = featureColumns.ToArray()
            };
        }
    }
}
