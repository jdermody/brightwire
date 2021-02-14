using System;
using System.Collections.Generic;
using System.Diagnostics;
using BrightData;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Graph engine learning context
    /// </summary>
    internal class LearningContext : ILearningContext
    {
	    readonly Dictionary<uint, float> _learningRateSchedule = new Dictionary<uint, float>();
            readonly Stack<(IGraphData? Data, Func<IGraphData?, IGraphData?> Callback)> _deferredBackpropagation = new Stack<(IGraphData?, Func<IGraphData?, IGraphData?>)>();
            readonly List<(NodeBase Node, IFloatMatrix Error, Action<IFloatMatrix> Updater)> _layerMatrixUpdate = new List<(NodeBase, IFloatMatrix, Action<IFloatMatrix>)>();
            readonly List<(NodeBase Node, IFloatVector Error, Action<IFloatVector> Updater)> _layerVectorUpdate = new List<(NodeBase, IFloatVector, Action<IFloatVector>)>();
            readonly HashSet<NodeBase> _updatesDisabled = new HashSet<NodeBase>();
            readonly Stopwatch _timer = new Stopwatch();

            public LearningContext(GraphFactory graphFactory, IErrorMetric errorMetric)
            {
                GraphFactory = graphFactory;
                ErrorMetric = errorMetric;
                CurrentEpoch = 0;
                RowCount = 0;
            }

        public GraphFactory GraphFactory { get; }
	    public IErrorMetric? ErrorMetric { get; set; }
        public ILinearAlgebraProvider LinearAlgebraProvider { get; }
	    public uint RowCount => _rowCount;
	    public uint CurrentEpoch => _currentEpoch;
	    public float LearningRate { get; set; }
	    public float BatchLearningRate => LearningRate / BatchSize;
        public uint BatchSize { get; set; }
	    public TrainingErrorCalculation TrainingErrorCalculation { get; }
	    public long EpochMilliseconds => _timer.ElapsedMilliseconds;
	    public double EpochSeconds => EpochMilliseconds / 1000.0;
	    public bool DeferUpdates { get; }
	    public void ScheduleLearningRate(uint atEpoch, float newLearningRate) => _learningRateSchedule[atEpoch] = newLearningRate;
	    public Action<string> MessageLog { get; set; } = Console.WriteLine;

            public void StoreUpdate(NodeBase fromNode, IFloatMatrix update, Action<IFloatMatrix> updater)
            {
                if (!_updatesDisabled.Contains(fromNode)) {
                    _layerMatrixUpdate.Add((fromNode, update, updater));
                }

                //var groups = _layerMatrixUpdate.Where(d => d.Node == fromNode).GroupBy(d => (d.Node, d.Error.RowCount));
                //foreach (var group in groups) {
                //    var count = group.Count();
                //    if (count > 1) {
                //        int i = 0;
                //    }
                //}
            }

            public void StoreUpdate(NodeBase fromNode, IFloatVector update, Action<IFloatVector> updater)
            {
                if (!_updatesDisabled.Contains(fromNode)) {
                    _layerVectorUpdate.Add((fromNode, update, updater));
                }
            }

            void Update<T>(
                List<(NodeBase Node, T Error, Action<T> Updater)> updates, 
                Func<T, uint> getSize,
                Func<T, T> cloner, 
                Action<T, T> addInPlace, 
                Action<int, T> divider
            ) where T: class
            {
                //foreach (var nodeGroup in updates.GroupBy(d => (d.Node, getSize(d.Error)))) {
                //    T? update = null;
                //    Action<T>? updater = null;
                //    int count = 0;
                //    foreach (var item in nodeGroup) {
                //        if (update == null) {
                //            updater = item.Updater;
                //            update = item.Error;
                //            count = 1;
                //        }
                //        else {
                //            if (count == 1)
                //                update = cloner(update!);
                //            addInPlace(update, item.Error);
                //            ++count;
                //        }
                //    }

                //    if (count > 1)
                //        divider(count, update!);
                //    if (update != null)
                //        updater!(update);
                //}

                foreach(var (fromNode, error, updater) in _layerMatrixUpdate)
                    updater(error);
                updates.Clear();
            }

            public void ApplyUpdates()
            {
                Update(_layerMatrixUpdate, m => m.RowCount, m => m.Clone(), (m, m2) => m.AddInPlace(m2), (m, c) => c.Multiply(1f / m));
                Update(_layerVectorUpdate, m => m.Count, m => m.Clone(), (m, m2) => m.AddInPlace(m2), (m, c) => c.Multiply(1f / m));
            }

            public void StartEpoch()
            {
                BeforeEpochStarts?.Invoke(this);
                if (_learningRateSchedule.TryGetValue(++CurrentEpoch, out float newLearningRate)) {
                    LearningRate = newLearningRate;
                    MessageLog($"Learning rate changed to {newLearningRate}");
                }

                RowCount = 0;
                _timer.Restart();
                _layerVectorUpdate.Clear();
                _layerMatrixUpdate.Clear();
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

            public Action<string> MessageLog { get; set; } = Console.WriteLine;
            public event Action<ILearningContext>? BeforeEpochStarts;
            public event Action<ILearningContext>? AfterEpochEnds;

            // TODO: remove set
            public IErrorMetric ErrorMetric { get; set; }
            public GraphFactory GraphFactory { get; }
    }
}
