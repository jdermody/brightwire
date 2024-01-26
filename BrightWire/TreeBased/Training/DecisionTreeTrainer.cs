using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.DataTable;
using BrightData.DataTable.Columns;
using BrightData.Helper;
using BrightWire.Models.TreeBased;

namespace BrightWire.TreeBased.Training
{
    /// <summary>
    /// Decision tree classifier
    /// https://en.wikipedia.org/wiki/Decision_tree_learning
    /// </summary>
    public static class DecisionTreeTrainer
    {
        class Attribute
        {
            readonly uint _columnIndex;
            readonly string? _category;
            readonly double? _split;

            public Attribute(uint columnIndex, string category)
            {
                _columnIndex = columnIndex;
                _category = category;
                _split = null;
            }
            public Attribute(uint columnIndex, double split)
            {
                _columnIndex = columnIndex;
                _category = null;
                _split = split;
            }
            public override bool Equals(object? obj)
            {
                if (obj is Attribute other)
                    return other._columnIndex == _columnIndex && other._category == _category && DoubleMath.AreApproximatelyEqual(_split, other._split);
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
            public uint ColumnIndex => _columnIndex;
	        public string? Category => _category;
	        public double? Split => _split;

	        public IReadOnlyDictionary<string, List<InMemoryRow>> Partition(IEnumerable<InMemoryRow> rows)
            {
                List<InMemoryRow>? temp;
                var ret = new Dictionary<string, List<InMemoryRow>>();
                if (_category != null) {
                    foreach (var item in rows) {
                        var val = item.GetCategory(_columnIndex);
                        if (!ret.TryGetValue(val, out temp))
                            ret.Add(val, temp = []);
                        temp.Add(item);
                    }
                    return ret;
                }else {
                    var splitVal = _split ?? 0;
                    foreach (var item in rows) {
                        var val = item.GetValue(_columnIndex);
                        var label = (val < splitVal) ? "-" : "+";
                        if (!ret.TryGetValue(label, out temp))
                            ret.Add(label, temp = []);
                        temp.Add(item);
                    }
                }
                return ret;
            }
        }
        class InMemoryRow
        {
	        readonly Dictionary<uint, string> _category = [];
            readonly Dictionary<uint, double> _continuous = [];

            public InMemoryRow(TableRow row, HashSet<uint> categorical, HashSet<uint> continuous, uint classColumnIndex)
            {
                ClassificationLabel = row.Get<string>(classColumnIndex);
                foreach (var columnIndex in categorical)
                    _category.Add(columnIndex, row.Get<string>(columnIndex));
                foreach(var columnIndex in continuous)
                    _continuous.Add(columnIndex, row.Get<float>(columnIndex));
            }
            public string ClassificationLabel { get; }

	        public string GetCategory(uint columnIndex)
            {
                return _category[columnIndex];
            }
            public double GetValue(uint columnIndex)
            {
                return _continuous[columnIndex];
            }
        }
        class TableInfo
        {
            readonly HashSet<uint> _categorical = [];
            readonly HashSet<uint> _continuous = [];

            public TableInfo(IDataTable table)
            {
                ClassColumnIndex = table.GetTargetColumnOrThrow();
                var metaData = table.ColumnMetaData;
                for (uint i = 0, len = table.ColumnCount; i < len; i++) {
                    if (i != ClassColumnIndex) {
                        var columnType = table.ColumnTypes[i];
                        var columnMetaData = metaData[i];
                        var columnClass = ColumnTypeClassifier.GetClass(columnType, columnMetaData);
                        if ((columnClass & ColumnClass.Categorical) != 0)
                            _categorical.Add(i);
                        else if((columnClass & ColumnClass.Numeric) != 0)
                            _continuous.Add(i);
                    }
                }
                foreach(var row in table.EnumerateRows().ToBlockingEnumerable()) {
                    Data.Add(new InMemoryRow(row, _categorical, _continuous, ClassColumnIndex));
                }
            }
            public IEnumerable<uint> CategoricalColumns => _categorical;
	        public IEnumerable<uint> ContinuousColumns => _continuous;
	        public List<InMemoryRow> Data { get; } = [];
			public uint ClassColumnIndex { get; }
		}
        class Node(TableInfo tableInfo, List<InMemoryRow> data, string? matchLabel)
        {
	        readonly Dictionary<string, int> _classCount = data.GroupBy(d => d.ClassificationLabel).ToDictionary(g => g.Key, g => g.Count());
            Node? _parent = null;
            Attribute? _attribute = null;
            Node[]? _children = null;

            public DecisionTree.Node AsDecisionTreeNode()
            {
                var ret = new DecisionTree.Node {
                    ColumnIndex = _attribute?.ColumnIndex,
                    MatchLabel = MatchLabel,
                    Split = _attribute?.Split,
                    Children = _children?.Select(c => c.AsDecisionTreeNode()).ToArray(),
                    Classification = PredictedClass
                };
                return ret;
            }

            public Attribute[] Attributes
            {
                get
                {
                    var continuousValues = new Dictionary<uint, HashSet<double>>();
                    var categoricalValues = new Dictionary<uint, HashSet<string>>();
                    foreach(var item in Data) {
                        foreach(var column in tableInfo.CategoricalColumns) {
                            if (!categoricalValues.TryGetValue(column, out var temp2))
                                categoricalValues.Add(column, temp2 = []);
                            temp2.Add(item.GetCategory(column));
                        }
                        foreach (var column in tableInfo.ContinuousColumns) {
                            if (!continuousValues.TryGetValue(column, out var temp))
                                continuousValues.Add(column, temp = []);
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
                    return [.. ret];
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

            public Node[] SetAttribute(Attribute attribute, Node[] children)
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
	        public string? PredictedClass => _classCount.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).FirstOrDefault();
            public List<InMemoryRow> Data { get; } = data;
            public string? MatchLabel { get; } = matchLabel;
        }

        /// <summary>
        /// Decision tree configuration
        /// </summary>
        public class Config
        {
            public uint? FeatureBagCount { get; set; } = null;
            public int? MinDataPerNode { get; set; } = null;
            public int? MaxDepth { get; set; } = null;
            public double? MinInformationGain { get; set; } = null;
            public uint? MaxAttributes { get; set; } = null;
        }

        /// <summary>
        /// Trains a decision tree
        /// </summary>
        /// <param name="table">Training data</param>
        /// <param name="config">Decision tree configuration</param>
        /// <returns></returns>
        public static DecisionTree Train(IDataTable table, Config? config = null)
        {
            var tableInfo = new TableInfo(table);
            var root = new Node(tableInfo, tableInfo.Data, null);
            var stack = new Stack<Node>();
            stack.Push(root);

            var maxDepth = config?.MaxDepth;
            var minDataPerNode = config?.MinDataPerNode;
            var featureBagCount = config?.FeatureBagCount;
            double? minInformationGain = config?.MinInformationGain;
            var maxAttributes = config?.MaxAttributes;

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
                    attributes = attributes.Bag(maxAttributes ?? featureBagCount.Value, table.Context.Random);

                // randomly select a subset of attributes if configured
                else if (maxAttributes.HasValue)
                    attributes = attributes.Shuffle(table.Context.Random).Take((int)maxAttributes.Value).ToArray();
                
                var nodeEntropy = node.Entropy;
                double nodeTotal = node.Data.Count;
                var scoreTable = new List<(Attribute Attribute, Node[] Nodes, double Score)>();
                foreach (var item in attributes) {
                    var newChildren = item.Partition(node.Data).Select(d => new Node(tableInfo, d.Value, d.Key)).ToArray();
                    var informationGain = GetInformationGain(nodeEntropy, nodeTotal, newChildren);
                    if (informationGain < minInformationGain)
                        continue;
                    scoreTable.Add((item, newChildren, informationGain));
                }

                if (scoreTable.Any()) {
                    var bestSplit = scoreTable.MaxBy(kv => kv.Score);
                    foreach (var child in node.SetAttribute(bestSplit.Attribute, bestSplit.Nodes))
                        stack.Push(child);
                }
            }

            return new DecisionTree {
                ClassColumnIndex = tableInfo.ClassColumnIndex,
                Root = root.AsDecisionTreeNode()
            };
        }

        static double GetInformationGain(double setEntity, double setCount, Node[] splits)
        {
            var total = setEntity;
            foreach(var item in splits) {
                total -= item.Data.Count / setCount * item.Entropy;
            }
            return total;
        }
    }
}
