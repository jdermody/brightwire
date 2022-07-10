using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BrightData;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Graph engine learning context
    /// </summary>
    internal class LearningContext : ILearningContext
    {
        class NodeError
        {
            public NodeError(NodeBase node, ErrorType errorType, ITensor2 error)
            {
                Node = node;
                ErrorType = errorType;
                Error = error;
            }

            public NodeBase Node { get; }
            public ErrorType ErrorType { get; }
            public ITensor2 Error { get; }
        }

	    readonly Dictionary<uint, float> _learningRateSchedule = new();
        readonly Stack<(IGraphData? Data, Func<IGraphData?, IGraphData?> Callback)> _deferredBackpropagation = new();
        readonly List<NodeError> _nodeError = new();
        readonly HashSet<NodeBase> _updatesDisabled = new();
        readonly Stopwatch _timer = new();

        public LearningContext(GraphFactory graphFactory, IErrorMetric errorMetric)
        {
            GraphFactory = graphFactory;
            ErrorMetric = errorMetric;
            ResetEpoch();
        }

        public GraphFactory GraphFactory { get; }
        public void ResetEpoch()
        {
            CurrentEpoch = 0;
            RowCount = 0;
            _learningRateSchedule.Clear();  
        }

        public IErrorMetric ErrorMetric { get; }
	    public uint RowCount { get; private set; }
	    public uint CurrentEpoch { get; private set; }
	    public float LearningRate { get; set; }
	    //public float BatchLearningRate => LearningRate / BatchSize;
        public uint BatchSize { get; set; }
        public long EpochMilliseconds => _timer.ElapsedMilliseconds;
	    public double EpochSeconds => EpochMilliseconds / 1000.0;

        public void AddError(ErrorType errorType, NodeBase fromNode, ITensor2 error)
        {
            if (!_updatesDisabled.Contains(fromNode))
                _nodeError.Add(new NodeError(fromNode, errorType, error));
        }

        //public void StoreUpdate(NodeBase fromNode, IVector update, Action<IVector> updater)
        //{
        //    if (!_updatesDisabled.Contains(fromNode)) {
        //        _layerVectorUpdate.Add((fromNode, update, updater));
        //    }
        //}

        public void ApplyUpdates()
        {
            if(_deferredBackpropagation.Any())
                BackpropagateThroughTime(null);
            
            // group on each node and updated combination and apply average error if multiple errors are defined
            foreach(var group in _nodeError.GroupBy(d => (d.Node, d.ErrorType))) {
                if(group.Count() > 1) {
                    using var averageError = group.Select(d => d.Error).Average(true);
                    group.Key.Node.ApplyError(group.Key.ErrorType, averageError, this);
                }else {
                    using var error = group.Single().Error;
                    group.Key.Node.ApplyError(group.Key.ErrorType, error, this);
                }
            }
            _nodeError.Clear();
        }

        public void StartEpoch()
        {
            BeforeEpochStarts?.Invoke(this);
            if (_learningRateSchedule.TryGetValue(++CurrentEpoch, out float newLearningRate)) {
                LearningRate = newLearningRate;
                GraphFactory.Context.UserNotifications?.OnMessage($"Learning rate changed to {newLearningRate}");
            }

            RowCount = 0;
            _timer.Restart();
            _nodeError.Clear();
        }

        public void EndEpoch()
        {
            AfterEpochEnds?.Invoke(this);
            _timer.Stop();
            RowCount = 0;
        }

        public void SetRowCount(uint rowCount)
        {
            RowCount = rowCount;
        }

        public void DeferBackpropagation(IGraphData? errorSignal, Func<IGraphData?, IGraphData?> update)
        {
            _deferredBackpropagation.Push((errorSignal, update));
        }

        public IGraphData? BackpropagateThroughTime(IGraphData? signalOverride, int maxDepth = Int32.MaxValue)
        {
            int depth = 0;
            IGraphData? lastError = null;
            // TOD: average the error?
            while (_deferredBackpropagation.Count > 0 && depth < maxDepth) {
                var (data, callback) = _deferredBackpropagation.Pop();
                var error = callback(signalOverride ?? data ?? lastError);
                if (error?.HasValue == true)
                    lastError = error;
                ++depth;
            }
            _deferredBackpropagation.Clear();
            return lastError;
        }

        public void ScheduleLearningRate(uint atEpoch, float newLearningRate)
        {
            _learningRateSchedule[atEpoch] = newLearningRate;
        }

        public void EnableNodeUpdates(NodeBase node, bool enableUpdates)
        {
            if (enableUpdates)
                _updatesDisabled.Remove(node);
            else
                _updatesDisabled.Add(node);
        }

        public event Action<ILearningContext>? BeforeEpochStarts;
        public event Action<ILearningContext>? AfterEpochEnds;
    }
}
