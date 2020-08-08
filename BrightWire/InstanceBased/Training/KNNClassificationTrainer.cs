using BrightWire.Models;
using System.Linq;
using BrightWire.Models.InstanceBased;
using BrightTable;
using System.Collections.Generic;

namespace BrightWire.InstanceBased.Trainer
{
    /// <summary>
    /// K Nearest Neighbour classification trainer
    /// </summary>
    static class KNNClassificationTrainer
    {
        public static KNearestNeighbours Train(IDataTable table)
        {
            var data = new List<float[]>();
            var labels = new List<string>();
            var ret = new KNearestNeighbours();
            foreach(var (numeric, other) in table.ForEachAsFloat(numeric => ret.DataColumns = numeric.ToArray(), other => ret.OtherColumns = other.ToArray())) {
                data.Add(numeric);
                labels.Add(other);
            }
            ret.Instance = data.Select(v => new FloatVector { Data = v }).ToArray().ToArray();
            ret.Classification = labels.ToArray();
            return ret;
        }
    }
}
