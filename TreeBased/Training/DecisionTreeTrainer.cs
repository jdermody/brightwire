using BrightWire.Models;
using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TreeBased.Training
{
    public static class DecisionTreeTrainer
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
                var other = obj as Attribute;
                if(other != null)
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
            public int ColumnIndex { get { return _columnIndex; } }
            public string Category { get { return _category; } }
            public double? Split { get { return _split; } }
            public IReadOnlyDictionary<string, List<IRow>> Partition(IEnumerable<IRow> rows)
            {
                List<IRow> temp;
                var ret = new Dictionary<string, List<IRow>>();
                if (_category != null) {
                    foreach (var item in rows) {
                        var val = item.GetField<string>(_columnIndex);
                        if (!ret.TryGetValue(val, out temp))
                            ret.Add(val, temp = new List<IRow>());
                        temp.Add(item);
                    }
                    return ret;
                }else {
                    var splitVal = _split.Value;
                    foreach (var item in rows) {
                        var val = item.GetField<double>(_columnIndex);
                        var label = (val < splitVal) ? "-" : "+";
                        if (!ret.TryGetValue(label, out temp))
                            ret.Add(label, temp = new List<IRow>());
                        temp.Add(item);
                    }
                }
                return ret;
            }
        }
        class TableInfo : IRowProcessor
        {
            readonly HashSet<int> _categorical = new HashSet<int>();
            readonly HashSet<int> _continuous = new HashSet<int>();
            readonly List<IRow> _data = new List<IRow>();
            readonly int _classColumnIndex;

            public TableInfo(IDataTable table, int classColumnIndex)
            {
                _classColumnIndex = classColumnIndex;
                for (int i = 0, len = table.ColumnCount; i < len; i++) {
                    if (i != _classColumnIndex) {
                        var column = table.Columns[i];
                        if (column.IsContinuous)
                            _continuous.Add(i);
                        else if (ColumnTypeClassifier.IsCategorical(column))
                            _categorical.Add(i);
                    }
                }
                table.Process(this);
            }
            bool IRowProcessor.Process(IRow row)
            {
                // load the entire dataset into memory
                _data.Add(row);
                return true;
            }
            public IEnumerable<int> CategoricalColumns { get { return _categorical; } }
            public IEnumerable<int> ContinuousColumns { get { return _continuous; } }
            public IReadOnlyList<IRow> Data { get { return _data; } }
            public int ClassColumnIndex { get { return _classColumnIndex; } }
        }
        class Node
        {
            readonly TableInfo _tableInfo;
            readonly IReadOnlyList<IRow> _data;
            readonly Lazy<Dictionary<string, int>> _classCount;
            Node _parent = null;
            Attribute _attribute = null;
            IReadOnlyList<Node> _children = null;
            readonly string _matchLabel;

            public Node(TableInfo tableInfo, IReadOnlyList<IRow> data, string matchLabel)
            {
                _data = data;
                _tableInfo = tableInfo;
                _matchLabel = matchLabel;
                _classCount = new Lazy<Dictionary<string, int>>(() => data.GroupBy(d => d.Data[tableInfo.ClassColumnIndex].ToString()).ToDictionary(g => g.Key, g => g.Count()));
            }

            public DecisionTree.Node AsDecisionTreeNode()
            {
                var ret = new DecisionTree.Node {
                    ColumnIndex = _attribute?.ColumnIndex ?? -1,
                    MatchLabel = MatchLabel,
                    Split = _attribute?.Split,
                    Children = _children?.Select(c => c.AsDecisionTreeNode())?.ToArray(),
                    Classification = PredictedClass
                };
                return ret;
            }

            public IReadOnlyCollection<Attribute> Attributes
            {
                get
                {
                    HashSet<double> temp;
                    HashSet<string> temp2;
                    var continuousValues = new Dictionary<int, HashSet<double>>();
                    var categoricalValues = new Dictionary<int, HashSet<string>>();
                    foreach(var item in _data) {
                        foreach(var column in _tableInfo.CategoricalColumns) {
                            if (!categoricalValues.TryGetValue(column, out temp2))
                                categoricalValues.Add(column, temp2 = new HashSet<string>());
                            temp2.Add(item.GetField<string>(column));
                        }
                        foreach (var column in _tableInfo.ContinuousColumns) {
                            if (!continuousValues.TryGetValue(column, out temp))
                                continuousValues.Add(column, temp = new HashSet<double>());
                            temp.Add(item.GetField<double>(column));
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
                    return ret;
                }
            }

            public double Entropy
            {
                get
                {
                    double total = _classCount.Value.Sum(d => d.Value);
                    double ret = 0;
                    foreach(var item in _classCount.Value) {
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
            public bool IsLeaf { get { return _classCount.Value.Count <= 1; } }
            public string PredictedClass { get { return _classCount.Value.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).FirstOrDefault(); } }
            public IReadOnlyList<IRow> Data { get { return _data; } }
            public string MatchLabel { get { return _matchLabel; } }
        }

        public static DecisionTree Train(IDataTable table, int classColumnIndex)
        {
            var tableInfo = new TableInfo(table, classColumnIndex);
            var root = new Node(tableInfo, tableInfo.Data, null);
            var stack = new Stack<Node>();
            stack.Push(root);

            while(stack.Any()) {
                var node = stack.Pop();
                if (node.IsLeaf)
                    continue;

                var attributes = node.Attributes;
                if (!attributes.Any())
                    continue;

                var nodeEntropy = node.Entropy;
                double nodeTotal = node.Data.Count;
                var scoreTable = new Dictionary<Tuple<Attribute, List<Node>>, double>();
                foreach (var item in attributes) {
                    var newChildren = item.Partition(node.Data).Select(d => new Node(tableInfo, d.Value, d.Key)).ToList();
                    scoreTable.Add(Tuple.Create(item, newChildren), _GetInformationGain(nodeEntropy, nodeTotal, newChildren));
                }
                var bestSplit = scoreTable.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).First();
                foreach (var child in node.SetAttribute(bestSplit.Item1, bestSplit.Item2))
                    stack.Push(child);
            }

            return new DecisionTree {
                ClassColumnIndex = tableInfo.ClassColumnIndex,
                Root = root.AsDecisionTreeNode()
            };
        }

        static double _GetInformationGain(double setEntity, double setCount, IReadOnlyList<Node> splits)
        {
            double total = setEntity;
            foreach(var item in splits) {
                total -= item.Data.Count / setCount * item.Entropy;
            }
            return total;
        }
    }
}
