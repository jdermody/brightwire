using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.DataTable.Rows;
using BrightWire.Models.TreeBased;

namespace BrightWire.TreeBased
{
    /// <summary>
    /// Classifies rows based on a previously trained model
    /// </summary>
    internal class DecisionTreeClassifier(DecisionTree tree) : IRowClassifier
    {
        public IEnumerable<string> ClassifyInternal(GenericTableRow row)
        {
            var p = tree.Root;
            while(true) {
                if (p.ColumnIndex.HasValue) {
                    string? findChild;
                    if(p.Split.HasValue) {
                        var val = row.Get<float>(p.ColumnIndex.Value);
                        findChild = val < p.Split.Value ? "-" : "+";
                    }else
                        findChild = row.Get<string>(p.ColumnIndex.Value);

                    var child = p.Children?.FirstOrDefault(c => c.MatchLabel == findChild);
                    if (child != null)
                    {
                        p = child;
                        continue;
                    }
                }
                yield return p.Classification ?? throw new Exception("Classification was null");
                break;
            }
        }

        public (string Label, float Weight)[] Classify(GenericTableRow row)
        {
            var classification = ClassifyInternal(row).First();
            return [
                (classification, 1f)
            ];
        }
    }
}
