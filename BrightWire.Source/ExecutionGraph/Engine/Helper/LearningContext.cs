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
        readonly ILinearAlgebraProvider _lap;
        readonly Dictionary<int, float> _learningRateSchedule = new Dictionary<int, float>();
        readonly List<(object Error, Action<object> Updater)> _layerUpdate = new List<(object, Action<object>)>();
        readonly Stack<(IGraphData Data, Action<IGraphData> Callback)> _deferredBackpropagation = new Stack<(IGraphData, Action<IGraphData>)>();
        readonly TrainingErrorCalculation _trainingErrorCalculation;
        bool _deferUpdates;
        readonly Stopwatch _timer = new Stopwatch();
        readonly HashSet<INode> _noUpdateNodeSet = new HashSet<INode>();
        float _learningRate;
        int _batchSize, _rowCount = 0, _currentEpoch = 0;

        public LearningContext(ILinearAlgebraProvider lap, float learningRate, int batchSize, TrainingErrorCalculation trainingErrorCalculation, bool deferUpdates)
        {
            _lap = lap;
            _trainingErrorCalculation = trainingErrorCalculation;
            _learningRate = learningRate;
            _batchSize = batchSize;
            _deferUpdates = deferUpdates;
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

        public ILinearAlgebraProvider LinearAlgebraProvider { get { return _lap; } }
        public int RowCount { get { return _rowCount; } }
        public int CurrentEpoch { get { return _currentEpoch; } }
        public float LearningRate { get { return _learningRate; } set { _learningRate = value; } }
        public float BatchLearningRate => LearningRate / BatchSize;
        public int BatchSize { get { return _batchSize; } set { _batchSize = value; } }
        public TrainingErrorCalculation TrainingErrorCalculation { get { return _trainingErrorCalculation; } }
        public long EpochMilliseconds { get { return _timer.ElapsedMilliseconds; } }
        public double EpochSeconds { get { return EpochMilliseconds / 1000.0; } }
        public bool DeferUpdates => _deferUpdates;
        public void ScheduleLearningRate(int atEpoch, float newLearningRate) => _learningRateSchedule[atEpoch] = newLearningRate;

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
                if (_deferUpdates)
                    _layerUpdate.Add((error, o => updater((T)o)));
                else
                    updater(error);
            }
        }

        public void StartEpoch()
        {
            BeforeEpochStarts?.Invoke(this);
            if (_learningRateSchedule.TryGetValue(++_currentEpoch, out float newLearningRate)) {
                _learningRate = newLearningRate;
                Console.WriteLine($"Learning rate changed to {newLearningRate}");
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
