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
	    public float BatchLearningRate => LearningRate / BatchSize;
        public uint BatchSize { get; set; }
        public long EpochMilliseconds => _timer.ElapsedMilliseconds;
	    public double EpochSeconds => EpochMilliseconds / 1000.0;

        public void StoreUpdate(NodeBase fromNode, IFloatMatrix update, Action<IFloatMatrix> updater)
        {
            if (!_updatesDisabled.Contains(fromNode)) {
                _layerMatrixUpdate.Add((fromNode, update, updater));
            }
        }

        public void StoreUpdate(NodeBase fromNode, IFloatVector update, Action<IFloatVector> updater)
        {
            if (!_updatesDisabled.Contains(fromNode)) {
                _layerVectorUpdate.Add((fromNode, update, updater));
            }
        }

        void Update<T>(List<(NodeBase Node, T Error, Action<T> Updater)> updates) where T: class
        {
            foreach(var (fromNode, error, updater) in updates)
                updater(error);
            updates.Clear();
        }

        public void ApplyUpdates()
        {
            if(_deferredBackpropagation.Any())
                BackpropagateThroughTime(null);
            Update(_layerMatrixUpdate);
            Update(_layerVectorUpdate);
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
    }
}
