using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TreeBased
{
    public class RandomForestClassifier : IRowProcessor
    {
        readonly List<DecisionTreeClassifier> _forest;
        readonly List<Tuple<string, string>> _resultList = new List<Tuple<string, string>>();
        readonly int? _classColumnIndex;

        public RandomForestClassifier(RandomForest forest, int? classColumnIndex)
        {
            _forest = forest.Forest.Select(m => new DecisionTreeClassifier(m, classColumnIndex)).ToList();
            _classColumnIndex = classColumnIndex;
        }

        public string Classify(IRow row)
        {
            return _forest
                .Select(t => t.Classify(row))
                .GroupBy(d => d)
                .Select(g => Tuple.Create(g.Key, g.Count()))
                .OrderByDescending(d => d.Item2)
                .Select(d => d.Item1)
                .First()
            ;
        }

        public bool Process(IRow row)
        {
            string expectedValue = null;
            if (_classColumnIndex.HasValue)
                expectedValue = row.GetField<string>(_classColumnIndex.Value);
            _resultList.Add(Tuple.Create(Classify(row), expectedValue));
            return true;
        }

        public void Clear()
        {
            _resultList.Clear();
        }

        public IReadOnlyList<Tuple<string, string>> Results { get { return _resultList; } }
    }
}
