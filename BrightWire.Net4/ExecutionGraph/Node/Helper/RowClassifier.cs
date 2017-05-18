using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    class RowClassifier : NodeBase
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IRowClassifier _classifier;
        readonly IDataTable _dataTable;
        readonly Dictionary<string, int> _targetLabel;

        public RowClassifier(ILinearAlgebraProvider lap, IRowClassifier classifier, IDataTable dataTable, IDataTableAnalysis analysis, string name = null) : base(name)
        {
            _targetLabel = analysis.ColumnInfo
                .First(ci => dataTable.Columns[ci.ColumnIndex].IsTarget)
                .DistinctValues
                .Select((v, i) => (v.ToString(), i))
                .ToDictionary(d => d.Item1, d => d.Item2)
            ;
        }

        public override void ExecuteForward(IContext context)
        {
            var rowList = _dataTable.GetRows(context.BatchSequence.MiniBatch.Rows);
            var resultList = new List<Dictionary<int, float>>();
            foreach (var row in rowList) {
                var value = _classifier.Classify(row)
                    .Select(c => (_targetLabel[c.Classification], c.Weight))
                    .ToDictionary(d => d.Item1, d => d.Item2)
                ;
                resultList.Add(value);
            }
            float temp;
            var output = _lap.CreateMatrix(resultList.Count, _targetLabel.Count, (i, j) => resultList[i].TryGetValue(j, out temp) ? temp : 0f);
            _AddNextGraphAction(context, new MatrixGraphData(output), null);
        }
    }
}
