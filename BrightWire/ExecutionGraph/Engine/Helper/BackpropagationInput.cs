using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    class BackpropagationInput
    {
        readonly HashSet<NodeBase> _input = new();
        readonly Dictionary<NodeBase, IGraphData?> _error = new();
        readonly IGraphData? _nodeOutput;

        public BackpropagationInput(ExecutionHistory? history, ExecutionNode[] input)
        {
            _nodeOutput = history?.Data;
            foreach (var item in input)
                AddInput(item);
        }

        public void AddInput(ExecutionNode executionNode)
        {
            _input.Add(executionNode.Node ?? throw new ArgumentException("Node not found"));
        }

        public void Add(ExecutionNode fromNode, IGraphData? error)
        {
            var node = fromNode.Node ?? throw new ArgumentException("Node not found");
            if (!_input.Contains(node))
                throw new ArgumentException("Unexpected node");

            if (error?.HasValue == true && _nodeOutput?.HasValue == true) {
                if (error.Columns != _nodeOutput.Columns || error.Count != _nodeOutput.Count || error.Depth != _nodeOutput.Depth || error.Rows != _nodeOutput.Rows)
                    throw new ArgumentException("Unexpected delta size");
            }

            _error.Add(node, error);
            if (_error.Count == _input.Count)
                IsComplete = _input.All(n => _error.ContainsKey(n));
            else if (_error.Count > _input.Count)
                throw new Exception("Errors do not match input");
        }

        public bool IsComplete { get; private set; } = false;
        public int InputCount => _input.Count;
        public int ErrorCount => _error.Count;

        public IGraphData? GetError()
        {
            if (_error.Count == 1)
                return _error.Single().Value;

            IGraphData? ret = null;
            IMatrix? matrix = null;
            var count = 0;
            foreach (var item in _error.Values) {
                if (item?.HasValue != true)
                    continue;

                ret ??= item;
                if (matrix == null) {
                    matrix = item.GetMatrix();
                    count = 1;
                }
                else {
                    matrix.AddInPlace(item.GetMatrix());
                    ++count;
                }
            }

            if (matrix != null) {
                if (count > 1)
                    matrix.MultiplyInPlace(1f / count);
                return ret!.ReplaceWith(matrix);
            }

            return null;
        }

        public void ClearForBackpropagation()
        {
            _error.Clear();
        }
    }
}