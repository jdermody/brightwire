using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models.Output;

namespace BrightWire.TreeBased
{
    internal class DecisionTreeClassifier : IRowClassifier
    {
        readonly DecisionTree _tree;

        public DecisionTreeClassifier(DecisionTree tree)
        {
            _tree = tree;
        }

        public IEnumerable<string> Classify(IRow row)
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

        public IReadOnlyList<WeightedClassification> GetWeightedClassifications(IRow row)
        {
            throw new NotImplementedException();
        }
    }
}
