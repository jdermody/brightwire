using System;
using System.Collections.Generic;
using System.Diagnostics;
using BrightData;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Graph engine learning context
    /// </summary>
    internal class LearningContext : ILearningContext
    {
	    readonly Dictionary<uint, float> _learningRateSchedule = new Dictionary<uint, float>();
        readonly List<(IFloatMatrix Error, Action<IFloatMatrix> Updater)> _layerMatrixUpdate = new List<(IFloatMatrix, Action<IFloatMatrix>)>();
        readonly List<(IFloatVector Error, Action<IFloatVector> Updater)> _layerVectorUpdate = new List<(IFloatVector, Action<IFloatVector>)>();

        readonly Stack<(IGraphData? Data, Func<IGraphData?, IGraphData?> Callback)> _deferredBackpropagation = new Stack<(IGraphData?, Func<IGraphData?, IGraphData?>)>();
	    readonly Stopwatch _timer = new Stopwatch();
        readonly HashSet<INode> _noUpdateNodeSet = new HashSet<INode>();
	    uint _rowCount = 0, _currentEpoch = 0;

        public LearningContext(
	        ILinearAlgebraProvider lap, 
	        float learningRate, 
	        uint batchSize,
            GraphFactory graph
        ) {
            LinearAlgebraProvider = lap;
            LearningRate = learningRate;
            BatchSize = batchSize;
            GraphFactory = graph;
        }

        public event Action<ILearningContext>? BeforeEpochStarts;
        public event Action<ILearningContext>? AfterEpochEnds;

	    public void Clear()
        {
            _layerVectorUpdate.Clear();
            _layerMatrixUpdate.Clear();
            _deferredBackpropagation.Clear();
            _currentEpoch = 0;
            _rowCount = 0;
        }

        public GraphFactory GraphFactory { get; }
	    public IErrorMetric? ErrorMetric { get; set; }
        public ILinearAlgebraProvider LinearAlgebraProvider { get; }
	    public uint RowCount => _rowCount;
	    public uint CurrentEpoch => _currentEpoch;
	    public float LearningRate { get; set; }
	    public float BatchLearningRate => LearningRate / BatchSize;
        public uint BatchSize { get; set; }
        public long EpochMilliseconds => _timer.ElapsedMilliseconds;
	    public double EpochSeconds => EpochMilliseconds / 1000.0;
        public void ScheduleLearningRate(uint atEpoch, float newLearningRate) => _learningRateSchedule[atEpoch] = newLearningRate;
	    public Action<string> MessageLog { get; set; } = Console.WriteLine;

        public void EnableNodeUpdates(INode node, bool enableUpdates)
        {
            if (enableUpdates)
                _noUpdateNodeSet.Remove(node);
            else
                _noUpdateNodeSet.Add(node);
        }

        public void StoreUpdate(INode fromNode, IFloatMatrix error, Action<IFloatMatrix> updater)
        {
            if (!_noUpdateNodeSet.Contains(fromNode)) {
                //if (DeferUpdates)
                    _layerMatrixUpdate.Add((error, updater));
                //else
                //    updater(error);
            }
        }

        public void StoreUpdate(INode fromNode, IFloatVector error, Action<IFloatVector> updater)
        {
            if (!_noUpdateNodeSet.Contains(fromNode)) {
                //if (DeferUpdates)
                _layerVectorUpdate.Add((error, updater));
                //else
                //    updater(error);
            }
        }

        public void StartEpoch()
        {
            BeforeEpochStarts?.Invoke(this);
            if (_learningRateSchedule.TryGetValue(++_currentEpoch, out float newLearningRate)) {
                LearningRate = newLearningRate;
                MessageLog($"Learning rate changed to {newLearningRate}");
            }

            _rowCount = 0;
            _timer.Restart();
            _layerVectorUpdate.Clear();
            _layerMatrixUpdate.Clear();
        }

        public void SetRowCount(uint rowCount)
        {
            _rowCount = rowCount;
        }

        public void EndEpoch()
        {
            AfterEpochEnds?.Invoke(this);
            ApplyUpdates();
            _timer.Stop();
            _rowCount = 0;
        }

        public void ApplyUpdates()
        {
            foreach(var (error, updater) in _layerMatrixUpdate)
                updater(error);
            foreach(var (error, updater) in _layerVectorUpdate)
                updater(error);
            _layerMatrixUpdate.Clear();
            _layerVectorUpdate.Clear();
        }

        public void DeferBackpropagation(IGraphData? data, Func<IGraphData?, IGraphData?> update)
        {
            _deferredBackpropagation.Push((data, update));
        }

        public IGraphData? BackpropagateThroughTime(IGraphData? signal, int maxDepth = int.MaxValue)
        {
            int depth = 0;
            IGraphData? currentSignal = null;
            while (_deferredBackpropagation.Count > 0 && depth < maxDepth) {
                var (data, callback) = _deferredBackpropagation.Pop();
                if (depth == 0)
                    callback(signal ?? data);
                else
                    callback(data ?? currentSignal);
                if (data != null)
                    currentSignal = data;
                ++depth;
            }
            _deferredBackpropagation.Clear();
            return currentSignal;
        }
    }
}
