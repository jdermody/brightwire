using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Graph engine learning context
    /// </summary>
    class LearningContext : ILearningContext
    {
	    readonly Dictionary<int, float> _learningRateSchedule = new Dictionary<int, float>();
        readonly List<(object Error, Action<object> Updater)> _layerUpdate = new List<(object, Action<object>)>();
        readonly Stack<(IGraphData Data, Action<IGraphData> Callback)> _deferredBackpropagation = new Stack<(IGraphData, Action<IGraphData>)>();
	    readonly Stopwatch _timer = new Stopwatch();
        readonly HashSet<INode> _noUpdateNodeSet = new HashSet<INode>();
	    int _rowCount = 0, _currentEpoch = 0;

        public LearningContext(
	        ILinearAlgebraProvider lap, 
	        float learningRate, 
	        int batchSize, 
	        TrainingErrorCalculation trainingErrorCalculation, 
	        bool deferUpdates
	    ) {
            LinearAlgebraProvider = lap;
            TrainingErrorCalculation = trainingErrorCalculation;
            LearningRate = learningRate;
            BatchSize = batchSize;
            DeferUpdates = deferUpdates;
        }

        public event Action<ILearningContext> BeforeEpochStarts;
        public event Action<ILearningContext> AfterEpochEnds;

	    public void Clear()
        {
            _layerUpdate.Clear();
            _deferredBackpropagation.Clear();
            _currentEpoch = 0;
            _rowCount = 0;
        }

	    public IErrorMetric ErrorMetric { get; set; }
        public ILinearAlgebraProvider LinearAlgebraProvider { get; }
	    public int RowCount => _rowCount;
	    public int CurrentEpoch => _currentEpoch;
	    public float LearningRate { get; set; }
	    public float BatchLearningRate => LearningRate / BatchSize;
        public int BatchSize { get; set; }
	    public TrainingErrorCalculation TrainingErrorCalculation { get; }
	    public long EpochMilliseconds => _timer.ElapsedMilliseconds;
	    public double EpochSeconds => EpochMilliseconds / 1000.0;
	    public bool DeferUpdates { get; }
	    public void ScheduleLearningRate(int atEpoch, float newLearningRate) => _learningRateSchedule[atEpoch] = newLearningRate;
	    public Action<string> MessageLog { get; set; } = Console.WriteLine;

        public void EnableNodeUpdates(INode node, bool enableUpdates)
        {
            if (enableUpdates)
                _noUpdateNodeSet.Remove(node);
            else
                _noUpdateNodeSet.Add(node);
        }

        public void StoreUpdate<T>(INode fromNode, T error, Action<T> updater)
        {
            if (!_noUpdateNodeSet.Contains(fromNode)) {
                if (DeferUpdates)
                    _layerUpdate.Add((error, o => updater((T)o)));
                else
                    updater(error);
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
            _layerUpdate.Clear();
        }

        public void SetRowCount(int rowCount)
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
            BackpropagateThroughTime(null);
            foreach(var item in _layerUpdate)
                item.Updater(item.Error);
            _layerUpdate.Clear();
        }

        public void DeferBackpropagation(IGraphData data, Action<IGraphData> update)
        {
            _deferredBackpropagation.Push((data, update));
        }

        public void BackpropagateThroughTime(IGraphData signal, int maxDepth = int.MaxValue)
        {
            int depth = 0;
            while (_deferredBackpropagation.Count > 0 && depth < maxDepth) {
                var next = _deferredBackpropagation.Pop();
                // TODO: add signal to the data?
                next.Callback(signal ?? next.Data);
                signal = null;
                ++depth;
            }
            _deferredBackpropagation.Clear();
        }
    }
}
