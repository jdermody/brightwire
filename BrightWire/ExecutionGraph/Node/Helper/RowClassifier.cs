using System;
using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.DataTable2;
using BrightData.LinearAlegbra2;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes a row based classifier
    /// </summary>
    internal class RowClassifier : NodeBase
    {
        class Backpropagation : SingleBackpropagationBase<RowClassifier>
        {
            public Backpropagation(RowClassifier source) : base(source)
            {
            }

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                return GraphData.Null;
            }
        }
        class DefaultIndexer : IIndexStrings
        {
            readonly Dictionary<string, uint> _targetLabel;

            public DefaultIndexer(BrightDataTable dataTable)
            {
                var targetColumn = dataTable.GetTargetColumnOrThrow();
                using var column = dataTable.GetColumn(targetColumn);
                _targetLabel = column.Enumerate()
                    .Select(o => o.ToString()!)
                    .Distinct()
                    .Select((v, i) => (Classification: v, Index: (uint)i))
                    .ToDictionary(d => d.Classification, d => d.Index)
                ;
            }

            public uint GetIndex(string str) => _targetLabel[str];
            public uint OutputSize => (uint)_targetLabel.Count;
        }

        readonly BrightDataTable _dataTable;
        readonly LinearAlgebraProvider _lap;
        readonly IRowClassifier _classifier;
        readonly IIndexStrings _indexer;

        public RowClassifier(LinearAlgebraProvider lap, IRowClassifier classifier, BrightDataTable dataTable, string? name = null)
            : base(name)
        {
            _lap = lap;
            _dataTable = dataTable;
            _classifier = classifier;
            _indexer = (classifier as IHaveIndexer)?.Indexer ?? new DefaultIndexer(dataTable);
        }

        public uint OutputSize => _indexer.OutputSize;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var rows = _dataTable.GetRows(context.BatchSequence.MiniBatch.Rows);
            var resultList = rows
                .Select(row => _classifier.Classify(row)
                    .Select(c => (Index: _indexer.GetIndex(c.Label), c.Weight))
                    .ToDictionary(d => d.Index, d => d.Weight)
                ).ToArray();
            var output = _lap.CreateMatrix((uint)resultList.Length, _indexer.OutputSize, (i, j) => resultList[i].TryGetValue(j, out var temp) ? temp : 0f);
            return (this, output.AsGraphData(), () => new Backpropagation(this));
        }
    }
}
