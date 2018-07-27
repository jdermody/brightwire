using BrightWire.Models;
using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.TreeBased.Training
{
    /// <summary>
    /// Decision tree classifier
    /// https://en.wikipedia.org/wiki/Decision_tree_learning
    /// </summary>
    internal static class DecisionTreeTrainer
    {
        class Attribute
        {
            readonly int _columnIndex;
            readonly string _category;
            readonly double? _split;

            public Attribute(int columnIndex, string category)
            {
                _columnIndex = columnIndex;
                _category = category;
                _split = null;
            }
            public Attribute(int columnIndex, double split)
            {
                _columnIndex = columnIndex;
                _category = null;
                _split = split;
            }
            public override bool Equals(object obj)
            {
                if (obj is Attribute other)
                    return other._columnIndex == _columnIndex && other._category == _category && _split == other._split;
                return false;
            }
            public override int GetHashCode()
            {
                if (_category != null)
                    return _columnIndex.GetHashCode() ^ _category.GetHashCode();
                return _columnIndex.GetHashCode() ^ _split.GetHashCode();
            }
            public override string ToString()
            {
                if (_category != null)
                    return $"{_category} ({_columnIndex})";
                return $"threshold: {_split} ({_columnIndex}";
            }
            public int ColumnIndex => _columnIndex;
	        public string Category => _category;
	        public double? Split => _split;

	        public IReadOnlyDictionary<string, List<InMemoryRow>> Partition(IEnumerable<InMemoryRow> rows)
            {
                List<InMemoryRow> temp;
                var ret = new Dictionary<string, List<InMemoryRow>>();
                if (_category != null) {
                    foreach (var item in rows) {
                        var val = item.GetCategory(_columnIndex);
                        if (!ret.TryGetValue(val, out temp))
                            ret.Add(val, temp = new List<InMemoryRow>());
                        temp.Add(item);
                    }
                    return ret;
                }else {
                    var splitVal = _split ?? 0;
                    foreach (var item in rows) {
                        var val = item.GetValue(_columnIndex);
                        var label = (val < splitVal) ? "-" : "+";
                        if (!ret.TryGetValue(label, out temp))
                            ret.Add(label, temp = new List<InMemoryRow>());
                        temp.Add(item);
                    }
                }
                return ret;
            }
        }
        class InMemoryRow
        {
	        readonly Dictionary<int, string> _category = new Dictionary<int, string>();
            readonly Dictionary<int, double> _continuous = new Dictionary<int, double>();

            public InMemoryRow(IRow row, HashSet<int> categorical, HashSet<int> continuous, int classColumnIndex)
            {
                ClassificationLabel = row.GetField<string>(classColumnIndex);
                foreach (var columnIndex in categorical)
                    _category.Add(columnIndex, row.GetField<string>(columnIndex));
                foreach(var columnIndex in continuous)
                    _continuous.Add(columnIndex, row.GetField<double>(columnIndex));
            }
            public string ClassificationLabel { get; }

	        public string GetCategory(int columnIndex)
            {
                return _category[columnIndex];
            }
            public double GetValue(int columnIndex)
            {
                return _continuous[columnIndex];
            }
        }
        class TableInfo
        {
            readonly HashSet<int> _categorical = new HashSet<int>();
            readonly HashSet<int> _continuous = new HashSet<int>();
            readonly List<InMemoryRow> _data = new List<InMemoryRow>();

			public TableInfo(IDataTable table)
            {
                ClassColumnIndex = table.TargetColumnIndex;
                for (int i = 0, len = table.ColumnCount; i < len; i++) {
                    if (i != ClassColumnIndex) {
                        var column = table.Columns[i];
                        if (column.IsContinuous)
                            _continuous.Add(i);
                        else if (ColumnTypeClassifier.IsCategorical(column))
                            _categorical.Add(i);
                    }
                }
                table.ForEach(row => _data.Add(new InMemoryRow(row, _categorical, _continuous, ClassColumnIndex)));
            }
            public IEnumerable<int> CategoricalColumns => _categorical;
	        public IEnumerable<int> ContinuousColumns => _continuous;
	        public IReadOnlyList<InMemoryRow> Data => _data;
			public int ClassColumnIndex { get; }
		}
        class Node
        {
            readonly TableInfo _tableInfo;
	        readonly Dictionary<string, int> _classCount;
            Node _parent = null;
            Attribute _attribute = null;
            IReadOnlyList<Node> _children = null;
            readonly string _matchLabel;

            public Node(TableInfo tableInfo, IReadOnlyList<InMemoryRow> data, string matchLabel)
            {
                Data = data;
                _tableInfo = tableInfo;
                _matchLabel = matchLabel;
                _classCount = data.GroupBy(d => d.ClassificationLabel).ToDictionary(g => g.Key, g => g.Count());
            }

            public DecisionTree.Node AsDecisionTreeNode()
            {
                var ret = new DecisionTree.Node {
                    ColumnIndex = _attribute?.ColumnIndex ?? -1,
                    MatchLabel = MatchLabel,
                    Split = _attribute?.Split,
                    Children = _children?.Select(c => c.AsDecisionTreeNode()).ToArray(),
                    Classification = PredictedClass
                };
                return ret;
            }

            public IReadOnlyList<Attribute> Attributes
            {
                get
                {
                    var continuousValues = new Dictionary<int, HashSet<double>>();
                    var categoricalValues = new Dictionary<int, HashSet<string>>();
                    foreach(var item in Data) {
                        foreach(var column in _tableInfo.CategoricalColumns) {
                            if (!categoricalValues.TryGetValue(column, out HashSet<string> temp2))
                                categoricalValues.Add(column, temp2 = new HashSet<string>());
                            temp2.Add(item.GetCategory(column));
                        }
                        foreach (var column in _tableInfo.ContinuousColumns) {
                            if (!continuousValues.TryGetValue(column, out HashSet<double> temp))
                                continuousValues.Add(column, temp = new HashSet<double>());
                            temp.Add(item.GetValue(column));
                        }
                    }

                    var ret = new HashSet<Attribute>();
                    foreach(var column in categoricalValues) {
                        if(column.Value.Count > 1) {
                            foreach (var item in column.Value)
                                ret.Add(new Attribute(column.Key, item));
                        }
                    }
                    foreach (var column in continuousValues) {
                        if (column.Value.Count > 1) {
                            var orderedContinuous = column.Value.OrderBy(v => v).ToList();
                            for (var i = 1; i < orderedContinuous.Count; i++) {
                                var mid = (orderedContinuous[i - 1] + orderedContinuous[i]) / 2;
                                ret.Add(new Attribute(column.Key, mid));
                            }
                        }
                    }
                    return ret.ToList();
                }
            }

            public double Entropy
            {
                get
                {
                    double total = _classCount.Sum(d => d.Value);
                    double ret = 0;
                    foreach(var item in _classCount) {
                        var probability = item.Value / total;
                        ret -= probability * Math.Log(probability, 2);
                    }
                    return ret;
                }
            }

            public IReadOnlyList<Node> SetAttribute(Attribute attribute, IReadOnlyList<Node> children)
            {
                _attribute = attribute;
                _children = children;
                foreach (var child in children)
                    child._parent = this;
                return children;
            }

            public int Depth
            {
                get
                {
                    var ret = 0;
                    var p = _parent;
                    while(p != null) {
                        ++ret;
                        p = p._parent;
                    }
                    return ret;
                }
            }

            public int Leaves
            {
                get
                {
                    if (_children == null)
                        return 1;
                    else {
                        var ret = 0;
                        foreach (var child in _children)
                            ret += child.Leaves;
                        return ret;
                    }
                }
            }
            public bool IsLeaf => _classCount.Count <= 1;
	        public string PredictedClass => _classCount.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).FirstOrDefault();
            public IReadOnlyList<InMemoryRow> Data { get; }
	        public string MatchLabel => _matchLabel;
        }

        public class Config
        {
            public int? FeatureBagCount { get; set; } = null;
            public int? MinDataPerNode { get; set; } = null;
            public int? MaxDepth { get; set; } = null;
            public double? MinInformationGain { get; set; } = null;
            public int? MaxAttributes { get; set; } = null;
        }

        public static DecisionTree Train(IDataTable table, Config config = null)
        {
            var tableInfo = new TableInfo(table);
            var root = new Node(tableInfo, tableInfo.Data, null);
            var stack = new Stack<Node>();
            stack.Push(root);

            int? maxDepth = config?.MaxDepth;
            int? minDataPerNode = config?.MinDataPerNode;
            int? featureBagCount = config?.FeatureBagCount;
            double? minInformationGain = config?.MinInformationGain;
            int? maxAttributes = config?.MaxAttributes;

            while (stack.Any()) {
                var node = stack.Pop();

                // stop at leaf nodes
                if (node.IsLeaf)
                    continue;

                // stop when there are no more features left to split
                var attributes = node.Attributes;
                if (!attributes.Any())
                    continue;

                // check if max depth exceeded
                if (maxDepth.HasValue && node.Depth >= maxDepth.Value)
                    continue;

                // check if the node has too little data to worry about splitting further
                if (minDataPerNode.HasValue && node.Data.Count < minDataPerNode.Value)
                    continue;

                // bag the features if configured
                if (featureBagCount.HasValue)
                    attributes = attributes.Bag(maxAttributes ?? featureBagCount.Value);

                // randomly select a subset of attributes if configured
                else if (maxAttributes.HasValue)
                    attributes = attributes.Shuffle().Take(maxAttributes.Value).ToList();
                
                var nodeEntropy = node.Entropy;
                double nodeTotal = node.Data.Count;
                var scoreTable = new Dictionary<Tuple<Attribute, List<Node>>, double>();
                foreach (var item in attributes) {
                    var newChildren = item.Partition(node.Data).Select(d => new Node(tableInfo, d.Value, d.Key)).ToList();
                    var informationGain = _GetInformationGain(nodeEntropy, nodeTotal, newChildren);
                    if (minInformationGain.HasValue && informationGain < minInformationGain.Value)
                        continue;
                    scoreTable.Add(Tuple.Create(item, newChildren), informationGain);
                }
                var bestSplit = scoreTable.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).FirstOrDefault();
                if (bestSplit != null) {
                    foreach (var child in node.SetAttribute(bestSplit.Item1, bestSplit.Item2))
                        stack.Push(child);
                }
            }

            return new DecisionTree {
                ClassColumnIndex = tableInfo.ClassColumnIndex,
                Root = root.AsDecisionTreeNode()
            };
        }

        static double _GetInformationGain(double setEntity, double setCount, IReadOnlyList<Node> splits)
        {
            var total = setEntity;
            foreach(var item in splits) {
                total -= item.Data.Count / setCount * item.Entropy;
            }
            return total;
        }
    }
}
