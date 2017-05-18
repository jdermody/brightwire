using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TreeBased
{
    internal class RandomForestClassifier : IRowClassifier
    {
        readonly List<DecisionTreeClassifier> _forest;

        public RandomForestClassifier(RandomForest forest)
        {
            _forest = forest.Forest.Select(m => new DecisionTreeClassifier(m)).ToList();
        }

        //public IEnumerable<string> Classify(IRow row)
        //{
        //    return _forest
        //        .Select(t => t.Classify(row).Single())
        //        .GroupBy(d => d)
        //        .Select(g => Tuple.Create(g.Key, g.Count()))
        //        .OrderByDescending(d => d.Item2)
        //        .Select(d => d.Item1)
        //    ;
        //}

        public IReadOnlyList<(string Label, float Weight)> Classify(IRow row)
        {
            var size = (float)_forest.Count;
            return _forest
                .Select(t => t.Classify(row).Single())
                .GroupBy(d => d.Label)
                .Select(g => (g.Key, g.Average(d => d.Weight)))
                .ToList()
            ;
        }
    }
}
