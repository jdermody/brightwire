﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    class ExecutionNode
    {
        readonly List<ExecutionNode> _ancestors = new List<ExecutionNode>();
        readonly List<ExecutionNode> _descendants = new List<ExecutionNode>();
        readonly Lazy<BackpropagationInput> _inputError;
        ExecutionHistory? _history = null;

        public ExecutionNode()
        {
            _inputError = new Lazy<BackpropagationInput>(() => new BackpropagationInput(_history, _descendants.Any() ? _descendants.ToArray() : new []{this}));
        }

        public void Add(ExecutionHistory history)
        {
#if DEBUG
            if (_history?.Data.HasValue == true)
                throw new Exception("History was repeated");
#endif

            _history = history;
        }

        public NodeBase? Node => _history?.Source;

        public void AddDescendant(ExecutionNode executionNode)
        {
            _descendants.Add(executionNode);
            executionNode._ancestors.Add(this);
            if (_inputError.IsValueCreated) {
                _inputError.Value.AddInput(executionNode);
            }
        }

        public IEnumerable<IGraphData> Backpropagate(IGraphSequenceContext context, IGraphData? delta, ExecutionNode fromNode)
        {
            var input = _inputError.Value;
            input.Add(fromNode, delta);

            // if all inputs have been received
            if (input.IsComplete) {
                var error = input.GetError();
                if (error != null) {
                    var parents = _ancestors.Select(d => d._history!.Source).ToArray();
                    var sendTo = _history!.Backpropagation?.Backward(error, context, parents);
                    if(sendTo != null) {
                        foreach (var (signal, nextContext, toNode) in sendTo) {
                            var context2 = (TrainingGraphSequenceContext) nextContext;
                            var executionNode = context2.GetExecutionNode(toNode);
                            foreach (var ret in executionNode.Backpropagate(nextContext, signal, this))
                                yield return ret;
                        }
                    }
                    else {
                        foreach (var item in _ancestors) {
                            foreach(var ret in item.Backpropagate(context, error, this))
                                yield return ret;
                        }
                    }

                    if (!_ancestors.Any())
                        yield return error;
                }
            }
        }

        public override string ToString() => $"{Node} ({_ancestors.Count:N0} ancestors, {_descendants.Count:N0} descendants)";

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("node");
            writer.WriteAttributeString("type", Node?.ToString() ?? "???");
            if(Node?.Name != null)
                writer.WriteAttributeString("name", Node.Name);
            if (_inputError.IsValueCreated) {
                var error = _inputError.Value;
                writer.WriteAttributeString("input-count", error.InputCount.ToString());
                writer.WriteAttributeString("error-count", error.ErrorCount.ToString());
            }
            foreach(var item in _descendants)
                item.WriteTo(writer);
            writer.WriteEndElement();
        }

        public void ClearForBackpropagation()
        {
            if (_inputError.IsValueCreated)
                _inputError.Value.ClearForBackpropagation();
        }
    }
}