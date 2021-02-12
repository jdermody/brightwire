﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    class TrainingGraphSequenceContext : IGraphSequenceContext, ICanTrace
        {
            readonly List<ExecutionHistory> _pendingForward = new List<ExecutionHistory>();
            readonly Dictionary<INode, ExecutionNode> _nodeExecution = new Dictionary<INode, ExecutionNode>();

            public TrainingGraphSequenceContext(
                ILearningContext? learningContext, 
                IGraphExecutionContext executionContext,
                IMiniBatchSequence batchSequence)
            {
                LearningContext = learningContext;
                ExecutionContext = executionContext;
                BatchSequence = batchSequence;
                Data = NullGraphData.Instance;
            }

            public void Dispose()
            {
            }

            public INode? Source { get; private set; } = null;
            public IGraphData Data { get; private set; }
            public IGraphExecutionContext ExecutionContext { get; }
            public ILearningContext? LearningContext { get; }
            public ILinearAlgebraProvider LinearAlgebraProvider => ExecutionContext.LinearAlgebraProvider;
            public IMiniBatchSequence BatchSequence { get; }

            public void AddForward(ExecutionHistory action, Func<IBackpropagate>? callback)
            {
                if (callback != null && LearningContext != null)
                    action.Backpropagation = callback();
                _pendingForward.Add(action);

                // add the history to the execution node
                if (!_nodeExecution.TryGetValue(action.Source, out var executionNode))
                    _nodeExecution.Add(action.Source, executionNode = new ExecutionNode());
                executionNode.Add(action);

                // connect the node to its parents in the graph
                foreach (var parent in action.Parents)
                    _nodeExecution[parent].AddDescendant(executionNode);
            }

            public ExecutionNode GetExecutionNode(INode node) => _nodeExecution[node];

            public IGraphData? Backpropagate(IGraphData? delta)
            {
                var curr = _nodeExecution[Source ?? throw new Exception("No target node")];
                var errors = curr.Backpropagate(this, delta, curr).ToList();
                ErrorSignal = errors.Single();
                return ErrorSignal;
            }

            public IGraphData? ErrorSignal { get; private set; } = null;
            public bool HasNext => _pendingForward.Any();
            public bool ExecuteNext()
            {
                if (HasNext) {
                    var next = _pendingForward.ElementAt(0);
                    _pendingForward.RemoveAt(0);

                    Data = next.Data;
                    Source = next.Source;
                    foreach (var output in next.Source.Output)
                        output.SendTo.ExecuteForward(this, output.Channel);

                    return true;
                }
                return false;
            }

            public void SetOutput(IGraphData data, int channel = 0)
            {
                throw new NotImplementedException();
            }

            public IGraphData? GetOutput(int channel = 0)
            {
                throw new NotImplementedException();
            }

            public IGraphData[] Output { get; } = new IGraphData[0];
            public ExecutionResult Result 
            {
                get
                {
                    var matrixOutput = Output.Any()
                        ? Output.Select(o => o.GetMatrix().Data)
                        : new[] {Data.GetMatrix().Data};

                    var ret = new ExecutionResult(BatchSequence, matrixOutput.SelectMany(m => m.Rows).ToArray());
                    if (ErrorSignal != null)
                        ret.Error = ErrorSignal.GetMatrix().Data.Rows.ToArray();
                    return ret;
                }
            }

            public string Trace()
            {
                var target = BatchSequence.Target;
                var output = Data;
                var error = ErrorSignal;
                var ret = new StringBuilder();

                if (target?.HasValue == true && output.HasValue && error?.HasValue == true) {
                    var t = target.GetMatrix().ReshapeAsVector().Data;
                    var o = output.GetMatrix().ReshapeAsVector().Data;
                    var e = error.GetMatrix().ReshapeAsVector().Data;
                    if (t.Size == o.Size && o.Size == e.Size) {
                        for (uint i = 0, len = t.Size; i < len; i++) {
                            ret.AppendLine($"{i}) output: {o[i]}, target: {t[i]}, error: {e[i]}");
                        }
                    }
                }

                return ret.ToString();
            }
        }
}
