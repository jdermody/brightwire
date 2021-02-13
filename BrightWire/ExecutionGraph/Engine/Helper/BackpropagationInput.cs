using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    class BackpropagationInput
    {
        readonly ExecutionNode[] _input;
        readonly Dictionary<ExecutionNode, IGraphData?> _error = new Dictionary<ExecutionNode, IGraphData?>();
        readonly IGraphData? _nodeOutput;

        public BackpropagationInput(ExecutionHistory? history, ExecutionNode[] input)
        {
            _nodeOutput = history?.Data;
            _input = input;
        }

        public void Add(ExecutionNode fromNode, IGraphData? error)
        {
            if (!_input.Contains(fromNode))
                throw new ArgumentException("Unexpected node");

            if (error?.HasValue == true && _nodeOutput?.HasValue == true) {
                if (error.Columns != _nodeOutput.Columns || error.Count != _nodeOutput.Count || error.Depth != _nodeOutput.Depth || error.Rows != _nodeOutput.Rows)
                    throw new ArgumentException("Unexpected delta size");
            }

            _error.Add(fromNode, error);
            if (_error.Count == _input.Length)
                IsComplete = _input.All(n => _error.ContainsKey(n));
            else if (_error.Count > _input.Length)
                throw new Exception("Errors do not match input");
        }

        public bool IsComplete { get; private set; } = false;
        public int InputCount => _input.Length;
        public int ErrorCount => _error.Count;

        public IGraphData? GetError()
        {
            if (_error.Count == 1)
                return _error.Single().Value;

            IGraphData? ret = null;
            IFloatMatrix? matrix = null;
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
                    matrix.Multiply(1f / count);
                return ret!.ReplaceWith(matrix);
            }

            return null;
        }
    }
}