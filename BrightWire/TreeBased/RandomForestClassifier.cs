using BrightTable;
using BrightWire.Models;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.TreeBased
{
    /// <summary>
    /// Classifies rows based on a previously trained model
    /// </summary>
    class RandomForestClassifier : IRowClassifier
    {
        readonly List<DecisionTreeClassifier> _forest;

        public RandomForestClassifier(RandomForest forest)
        {
            _forest = forest.Forest.Select(m => new DecisionTreeClassifier(m)).ToList();
        }

        public IReadOnlyList<(string Label, float Weight)> Classify(IConvertibleRow row)
        {
            return _forest
                .Select(t => t.Classify(row).Single())
                .GroupBy(d => d.Label)
                .Select(g => (g.Key, g.Average(d => d.Weight)))
                .ToList()
            ;
        }
    }
}
