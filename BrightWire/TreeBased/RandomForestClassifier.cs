using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.Models.TreeBased;

namespace BrightWire.TreeBased
{
    /// <summary>
    /// Classifies rows based on a previously trained model
    /// </summary>
    internal class RandomForestClassifier : IRowClassifier
    {
        readonly List<DecisionTreeClassifier> _forest;

        public RandomForestClassifier(RandomForest forest)
        {
            _forest = forest.Forest.Select(m => new DecisionTreeClassifier(m)).ToList();
        }

        public (string Label, float Weight)[] Classify(IDataTableRow row)
        {
            return _forest
                .Select(t => t.Classify(row).Single())
                .GroupBy(d => d.Label)
                .Select(g => (Label: g.Key, CombinedScore: g.Sum(d => d.Weight)))
                .OrderByDescending(d => d.CombinedScore)
                .ToArray()
            ;
        }
    }
}
