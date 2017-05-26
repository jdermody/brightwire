using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    class RowClassifier : NodeBase
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IRowClassifier _classifier;
        readonly Dictionary<string, int> _targetLabel;
        readonly List<IRow> _data = new List<IRow>();

        public RowClassifier(ILinearAlgebraProvider lap, IRowClassifier classifier, IDataTable dataTable, IDataTableAnalysis analysis, string name = null) 
            : base(name)
        {
            _lap = lap;
            _classifier = classifier;
            _targetLabel = analysis.ColumnInfo
                .First(ci => dataTable.Columns[ci.ColumnIndex].IsTarget)
                .DistinctValues
                .Select((v, i) => (v.ToString(), i))
                .ToDictionary(d => d.Item1, d => d.Item2)
            ;

            // read the entire data table into memory
            dataTable.ForEach(row => _data.Add(row));
        }

        public int OutputSize => _targetLabel.Count;

        public override void ExecuteForward(IContext context)
        {
            var rowList = context.BatchSequence.MiniBatch.Rows.Select(i => _data[i]).ToList();
            var resultList = new List<Dictionary<int, float>>();
            foreach (var row in rowList) {
                var value = _classifier.Classify(row)
                    .Select(c => (_targetLabel[c.Label], c.Weight))
                    .ToDictionary(d => d.Item1, d => d.Item2)
                ;
                resultList.Add(value);
            }
            float temp;
            var output = _lap.CreateMatrix(resultList.Count, _targetLabel.Count, (i, j) => resultList[i].TryGetValue(j, out temp) ? temp : 0f);
            _AddNextGraphAction(context, new MatrixGraphData(output), null);
        }

        public override Models.ExecutionGraph.Node SerialiseTo(List<Models.ExecutionGraph.Node> connectedTo, List<Models.ExecutionGraph.Wire> wireList)
        {
            throw new Exception("Row classifiers cannot be serialised as they contain potentially huge data tables - try creating the graph dynamically and insert the row classifier dynamically");
        }
    }
}
