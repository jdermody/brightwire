using System;
using BrightWire.ExecutionGraph.Helper;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes a row based classifier
    /// </summary>
    internal class RowClassifier(LinearAlgebraProvider<float> lap, IRowClassifier classifier, IDataTable dataTable, string? name = null)
        : NodeBase(name)
    {
        class Backpropagation(RowClassifier source) : SingleBackpropagationBase<RowClassifier>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                return GraphData.Null;
            }
        }
        class DefaultIndexer : IIndexStrings
        {
            readonly Dictionary<string, uint> _targetLabel;

            public DefaultIndexer(IDataTable dataTable)
            {
                var targetColumn = dataTable.GetTargetColumnOrThrow();
                var column = dataTable.GetColumn(targetColumn);
                _targetLabel = column.Enumerate()
                    .Select(o => o.ToString()!)
                    .Distinct()
                    .Select((v, i) => (Classification: v, Index: (uint)i))
                    .ToDictionary(d => d.Classification, d => d.Index)
                ;
            }

            public uint GetIndex(string str) => _targetLabel[str];
            public IEnumerable<string> OrderedStrings => _targetLabel.OrderBy(x => x.Value).Select(x => x.Key);
            public uint Size => (uint)_targetLabel.Count;
        }

        readonly IIndexStrings _indexer = (classifier as IHaveStringIndexer)?.Indexer ?? new DefaultIndexer(dataTable);

        public uint OutputSize => _indexer.Size;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var rows = dataTable.GetRows(context.BatchSequence.MiniBatch.Rows).Result;
            var resultList = rows
                .Select(row => classifier.Classify(row)
                    .Select(c => (Index: _indexer.GetIndex(c.Label), c.Weight))
                    .ToDictionary(d => d.Index, d => d.Weight)
                ).ToArray();
            var output = lap.CreateMatrix((uint)resultList.Length, OutputSize, (i, j) => resultList[i].GetValueOrDefault(j, 0f));
            return (this, output.AsGraphData(), () => new Backpropagation(this));
        }
    }
}
