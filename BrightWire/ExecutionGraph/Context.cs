using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BrightWire.ExecutionGraph
{
    public class Context : ILearningContext
    {
        readonly List<IMatrix> _context = new List<IMatrix>();
        readonly List<(IMatrix Error, Action<IMatrix> Updater)> _layerUpdate = new List<(IMatrix, Action<IMatrix>)>();
        readonly bool _calculateTrainingError, _deferUpdates;
        readonly Stopwatch _timer = new Stopwatch();
        float _learningRate;
        int _batchSize, _currentEpoch = 0;

        public Context(float learningRate, int batchSize, bool calculateTrainingError, bool deferUpdates)
        {
            _calculateTrainingError = calculateTrainingError;
            _learningRate = learningRate;
            _batchSize = batchSize;
            _deferUpdates = deferUpdates;
        }

        public int CurrentEpoch { get { return _currentEpoch; } }
        public float LearningRate { get { return _learningRate; } }
        public int BatchSize { get { return _batchSize; } }
        public bool CalculateTrainingError { get { return _calculateTrainingError; } }

        public void Store(IMatrix error, Action<IMatrix> updater)
        {
            if (_deferUpdates)
                _layerUpdate.Add((error, updater));
            else
                updater(error);
        }

        public void StartEpoch()
        {
            ++_currentEpoch;
            _timer.Restart();
        }

        public void EndEpoch()
        {
            _timer.Stop();
        }

        public void ApplyUpdates()
        {
            foreach(var item in _layerUpdate)
                item.Updater(item.Error);
            _layerUpdate.Clear();
        }
    }
}
