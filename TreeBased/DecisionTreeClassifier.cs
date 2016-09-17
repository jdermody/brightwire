using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TreeBased
{
    public class DecisionTreeClassifier : IRowProcessor
    {
        readonly DecisionTree _tree;
        readonly List<Tuple<string, string>> _resultList = new List<Tuple<string, string>>();
        readonly int? _classColumnIndex;

        public DecisionTreeClassifier(DecisionTree tree, int? classColumnIndex)
        {
            _tree = tree;
            _classColumnIndex = classColumnIndex;
        }

        public string Classify(IRow row)
        {
            var p = _tree.Root;
            while(p != null) {
                if (p.ColumnIndex >= 0) {
                    string findChild = null;
                    if(p.Split.HasValue) {
                        var val = row.GetField<double>(p.ColumnIndex);
                        findChild = (val < p.Split.Value) ? "-" : "+";
                    }else
                        findChild = row.GetField<string>(p.ColumnIndex);
                    if (findChild != null) {
                        var child = p.Children.FirstOrDefault(c => c.MatchLabel == findChild);
                        if (child != null) {
                            p = child;
                            continue;
                        }
                    }
                }
                return p.Classification;
            }
            return null;
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
