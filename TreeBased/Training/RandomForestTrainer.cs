using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TreeBased.Training
{
    public static class RandomForestTrainer
    {
        public static RandomForest Train(IIndexableDataTable table, int classColumnIndex, int b = 100, DecisionTreeTrainer.Config config = null)
        {
            config = config ?? new DecisionTreeTrainer.Config();

            // set the feature bag count as the square root of the total number of features
            if(!config.FeatureBagCount.HasValue)
                config.FeatureBagCount = Convert.ToInt32(Math.Round(Math.Sqrt(table.Columns.Sum(c => c.IsContinuous ? 1 : c.NumDistinct))));

            // repeatedly train a decision tree on a bagged subset of features
            var ret = new List<DecisionTree>();
            for(var i = 0; i < b; i++) {
                var baggedTree = table.Bag();
                ret.Add(DecisionTreeTrainer.Train(baggedTree, classColumnIndex, config));
            }
            return new RandomForest {
                Forest = ret.ToArray()
            };
        }
    }
}
