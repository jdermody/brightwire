using BrightWire.Models;
using System.Linq;
using BrightWire.Models.InstanceBased;
using BrightTable;
using System;

namespace BrightWire.InstanceBased.Trainer
{
    /// <summary>
    /// K Nearest Neighbour classification trainer
    /// </summary>
    static class KNNClassificationTrainer
    {
        public static KNearestNeighbours Train(IDataTable table)
        {
            throw new NotImplementedException();
            //var featureColumns = Enumerable.Range(0, table.ColumnCount).Where(i => i != table.TargetColumnIndex).ToList();
            //var data = table.GetNumericRows(featureColumns);
            //var labels = table.GetColumn<string>(table.TargetColumnIndex);

            //return new KNearestNeighbours {
            //    Instance = data.Select(v => new FloatVector { Segment = v }).ToArray(),
            //    Classification = labels.ToArray(),
            //    FeatureColumn = featureColumns.ToArray()
            //};
        }
    }
}
