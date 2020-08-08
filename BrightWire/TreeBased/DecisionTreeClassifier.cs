using BrightTable;
using BrightWire.Models;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.TreeBased
{
    /// <summary>
    /// Classifies rows based on a previously trained model
    /// </summary>
    class DecisionTreeClassifier : IRowClassifier
    {
        readonly DecisionTree _tree;

        public DecisionTreeClassifier(DecisionTree tree)
        {
            _tree = tree;
        }

        public IEnumerable<string> _Classify(IConvertibleRow row)
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
                yield return p.Classification;
                break;
            }
        }

        public IReadOnlyList<(string Label, float Weight)> Classify(IConvertibleRow row)
        {
            var classification = _Classify(row).First();
            return new[] {
                (classification, 1f)
            };
        }
    }
}
