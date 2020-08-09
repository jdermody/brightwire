using BrightTable;
using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes a row based classifier
    /// </summary>
    class RowClassifier : NodeBase
    {
		readonly IRowOrientedDataTable _dataTable;
        readonly ILinearAlgebraProvider _lap;
        readonly IRowClassifier _classifier;
        readonly Dictionary<string, int> _targetLabel;

        public RowClassifier(ILinearAlgebraProvider lap, IRowClassifier classifier, IRowOrientedDataTable dataTable, string name = null) 
            : base(name)
        {
            _lap = lap;
	        _dataTable = dataTable;
            _classifier = classifier;
            var targetColumn = dataTable.GetTargetColumn();
            //_targetLabel = analysis.ColumnInfo
            //    .First(ci => dataTable.Columns[ci.ColumnIndex].IsTarget)
            //    .DistinctValues
            //    .Select((v, i) => (v.ToString(), i))
            //    .ToDictionary(d => d.Item1, d => d.Item2)
            //;
        }

        public int OutputSize => _targetLabel.Count;

        public override void ExecuteForward(IContext context)
        {
            //var rowList = _dataTable.GetRows(context.BatchSequence.MiniBatch.Rows);
            //var resultList = new List<Dictionary<int, float>>();
            //foreach (var row in rowList) {
            //    var value = _classifier.Classify(row)
            //        .Select(c => (_targetLabel[c.Label], c.Weight))
            //        .ToDictionary(d => d.Item1, d => d.Item2)
            //    ;
            //    resultList.Add(value);
            //}
            //var output = _lap.CreateMatrix(resultList.Count, _targetLabel.Count, (i, j) => resultList[i].TryGetValue(j, out float temp) ? temp : 0f);
            //_AddNextGraphAction(context, new MatrixGraphData(output), null);
        }
    }
}
