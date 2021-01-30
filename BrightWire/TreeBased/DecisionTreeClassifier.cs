using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.Models.TreeBased;

namespace BrightWire.TreeBased
{
    /// <summary>
    /// Classifies rows based on a previously trained model
    /// </summary>
    internal class DecisionTreeClassifier : IRowClassifier
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
                if (p.ColumnIndex.HasValue) {
                    string? findChild = null;
                    if(p.Split.HasValue) {
                        var val = row.GetTyped<double>(p.ColumnIndex.Value);
                        findChild = val < p.Split.Value ? "-" : "+";
                    }else
                        findChild = row.GetTyped<string>(p.ColumnIndex.Value);

                    var child = p.Children.FirstOrDefault(c => c.MatchLabel == findChild);
                    if (child != null)
                    {
                        p = child;
                        continue;
                    }
                }
                yield return p.Classification;
                break;
            }
        }

        public (string Label, float Weight)[] Classify(IConvertibleRow row)
        {
            var classification = _Classify(row).First();
            return new[] {
                (classification, 1f)
            };
        }
    }
}
