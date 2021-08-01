using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BrightData;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    class TrainingGraphSequenceContext : SequenceContextBase, IGraphSequenceContext
    {
        readonly List<ExecutionHistory> _forward = new();
        readonly Dictionary<NodeBase, ExecutionNode> _nodeExecution = new();

        public TrainingGraphSequenceContext(
            ILearningContext? learningContext, 
            IGraphExecutionContext executionContext,
            IMiniBatchSequence batchSequence) : base(batchSequence)
        {
            LearningContext = learningContext;
            ExecutionContext = executionContext;
            BatchSequence.GraphContext = this;
        }

        public void Dispose()
        {
        }

        
        public IGraphExecutionContext ExecutionContext { get; }
        public ILearningContext? LearningContext { get; }
        public ILinearAlgebraProvider LinearAlgebraProvider => ExecutionContext.LinearAlgebraProvider;

        public void AddForwardHistory(NodeBase source, IGraphData data, Func<IBackpropagate>? callback, params NodeBase[] prev)
        {
            var action = new ExecutionHistory(source, data);
            if (callback != null && LearningContext != null)
                action.Backpropagation = callback();
            _forward.Add(action);

            // add the history to the execution node
            if (!_nodeExecution.TryGetValue(source, out var executionNode))
                _nodeExecution.Add(source, executionNode = new ExecutionNode());
            executionNode.Add(action);

            // connect the node to its parents in the graph
            foreach(var item in prev)
                _nodeExecution[item].AddDescendant(executionNode);
        }

        public string AsXml
        {
            get
            {
                var sb = new StringBuilder();
                using (var writer = XmlWriter.Create(sb)) {
                    writer.WriteStartElement("context");
                    var first = _forward.FirstOrDefault();
                    if (first != null) {
                        _nodeExecution[first.Source].WriteTo(writer);
                    }

                    writer.WriteEndElement();
                }

                return sb.ToString();
            }
        }

        public ExecutionNode GetExecutionNode(NodeBase node) => _nodeExecution[node];

        public IGraphData? Backpropagate(IGraphData? delta)
        {
            var source = _forward.LastOrDefault()?.Source;
            var curr = _nodeExecution[source ?? throw new ArgumentException("No target node")];
            foreach (var item in curr.Backpropagate(this, delta, curr)) {
                if (item?.HasValue == true) {
                    #if DEBUG
                    if(ErrorSignal?.HasValue == true && ErrorSignal.GetMatrix() != item.GetMatrix())
                        throw new Exception("Unexpected");
                    #endif
                    ErrorSignal = item;
                }
            }
            return ErrorSignal;
        }

        public IGraphData? ErrorSignal { get; private set; } = null;
        public IEnumerable<ExecutionResult> Results
        {
            get
            {
                if (Data.HasValue) {
                    var ret = new ExecutionResult(BatchSequence, Data.GetMatrix().Data.Rows);
                    if (ErrorSignal != null)
                        ret.Error = ErrorSignal.GetMatrix().Data.Rows.Select(d => d.ToArray()).ToArray();
                    yield return ret;
                }
            }
        }

        public void ClearForBackpropagation()
        {
            foreach (var item in _nodeExecution)
                item.Value.ClearForBackpropagation();
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
