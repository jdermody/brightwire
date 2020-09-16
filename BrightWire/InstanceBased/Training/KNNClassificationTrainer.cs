using BrightWire.Models;
using System.Linq;
using BrightWire.Models.InstanceBased;
using BrightTable;
using System;
using System.Collections.Generic;
using BrightData;
using BrightTable.Transformations;

namespace BrightWire.InstanceBased.Trainer
{
    /// <summary>
    /// K Nearest Neighbour classification trainer
    /// </summary>
    static class KNNClassificationTrainer
    {
        public static KNearestNeighbours Train(IDataTable table)
        {
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var featureColumns = table.ColumnIndicesOfFeatures().ToArray();
            var vectoriser = new DataTableVectoriser(table, featureColumns);
            var data = vectoriser.Enumerate().ToArray();

            return new KNearestNeighbours {
                Instance = data,
                Classification = table.Column(targetColumnIndex).Enumerate().Select(v => v.ToString()).ToArray(),
                DataColumns = featureColumns,
                TargetColumn = targetColumnIndex
            };
        }
    }
}
