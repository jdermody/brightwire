using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightWire.Models.TreeBased;

namespace BrightWire.TreeBased.Training
{
    /// <summary>
    /// Random forest classifier
    /// https://en.wikipedia.org/wiki/Random_forest
    /// </summary>
    internal static class RandomForestTrainer
    {
        public static async Task<RandomForest> Train(IDataTable table, uint b = 100, uint? baggedRowCount = null, DecisionTreeTrainer.Config? config = null)
        {
            config ??= new DecisionTreeTrainer.Config();

            // set the feature bag count as the square root of the total number of features
            if (!config.FeatureBagCount.HasValue) {
                var columnAnalysis = await table.GetColumnAnalysis();
                var numValues = columnAnalysis.Sum(c => c.GetColumnType().IsNumeric() ? 1 : c.GetNumDistinct());
                config.FeatureBagCount = Convert.ToUInt32(Math.Round(Math.Sqrt(numValues)));
            }

            // repeatedly train a decision tree
            var ret = new List<DecisionTree>();
            for(uint i = 0; i < b; i++) {
                var baggedTree = await table.Bag(null, baggedRowCount ?? table.RowCount);
                ret.Add(DecisionTreeTrainer.Train(baggedTree, config));
            }

            return new RandomForest {
                Forest = ret.ToArray()
            };
        }
    }
}
