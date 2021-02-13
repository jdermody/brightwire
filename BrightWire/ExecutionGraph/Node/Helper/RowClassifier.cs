using System;
using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes a row based classifier
    /// </summary>
    internal class RowClassifier : NodeBase
    {
        class DefaultIndexer : IIndexStrings
        {
            readonly Dictionary<string, uint> _targetLabel;

            public DefaultIndexer(IRowOrientedDataTable dataTable)
            {
                var targetColumn = dataTable.GetTargetColumnOrThrow();
                _targetLabel = dataTable.Column(targetColumn).Enumerate()
                    .Select(o => o.ToString()!)
                    .Distinct()
                    .Select((v, i) => (Classification: v, Index: (uint)i))
                    .ToDictionary(d => d.Classification, d => d.Index)
                ;
            }

            public uint GetIndex(string str) => _targetLabel[str];
            public uint OutputSize => (uint)_targetLabel.Count;
        }

        readonly IConvertibleTable _dataTable;
        readonly ILinearAlgebraProvider _lap;
        readonly IRowClassifier _classifier;
        readonly IIndexStrings _indexer;

        public RowClassifier(ILinearAlgebraProvider lap, IRowClassifier classifier, IRowOrientedDataTable dataTable, string? name = null)
            : base(name)
        {
            _lap = lap;
            _dataTable = dataTable.AsConvertible();
            _classifier = classifier;
            _indexer = (classifier as IHaveIndexer)?.Indexer ?? new DefaultIndexer(dataTable);
        }

        public uint OutputSize => _indexer.OutputSize;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardInternal(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            var resultList = _dataTable
                .Rows(context.BatchSequence.MiniBatch.Rows)
                .Select(row => _classifier.Classify(row)
                    .Select(c => (Index: _indexer.GetIndex(c.Label), c.Weight))
                    .ToDictionary(d => d.Index, d => d.Weight)
                ).ToArray();
            var output = _lap.CreateMatrix((uint)resultList.Length, _indexer.OutputSize, (i, j) => resultList[i].TryGetValue(j, out var temp) ? temp : 0f);
            return (this, new MatrixGraphData(output), null);
        }
    }
}
