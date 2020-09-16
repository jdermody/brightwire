using BrightTable;
using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes a row based classifier
    /// </summary>
    class RowClassifier : NodeBase
    {
        readonly IConvertibleTable _dataTable;
        readonly ILinearAlgebraProvider _lap;
        readonly IRowClassifier _classifier;
        readonly Dictionary<string, uint> _targetLabel;

        public RowClassifier(ILinearAlgebraProvider lap, IRowClassifier classifier, IRowOrientedDataTable dataTable, string name = null)
            : base(name)
        {
            _lap = lap;
            _dataTable = dataTable.AsConvertible();
            _classifier = classifier;
            var targetColumn = dataTable.GetTargetColumnOrThrow();
            _targetLabel = dataTable.Column(targetColumn).Enumerate()
                .Select(o => o.ToString())
                .Distinct()
                .Select((v, i) => (v, (uint)i))
                .ToDictionary(d => d.Item1, d => d.Item2)
            ;
        }

        public uint OutputSize => (uint)_targetLabel.Count;

        public override void ExecuteForward(IContext context)
        {
            var resultList = _dataTable
                .Rows(context.BatchSequence.MiniBatch.Rows)
                .Select(row => _classifier.Classify(row)
                    .Select(c => (_targetLabel[c.Label], c.Weight))
                    .ToDictionary(d => d.Item1, d => d.Item2)
                ).ToArray();
            var output = _lap.CreateMatrix((uint)resultList.Length, (uint)_targetLabel.Count, (i, j) => resultList[i].TryGetValue(j, out var temp) ? temp : 0f);
            _AddNextGraphAction(context, new MatrixGraphData(output), null);
        }
    }
}
