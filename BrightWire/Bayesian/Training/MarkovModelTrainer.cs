using BrightWire.Models.Bayesian;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Bayesian.Training
{
    /// <summary>
    /// Builds markov models with a window size of 2
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MarkovModelTrainer2<T> : IMarkovModelTrainer2<T> where T : notnull
    {
        readonly Dictionary<(T, T), List<T>> _data = new();
        readonly int _minObservations;
        readonly T _empty;

        public MarkovModelTrainer2(T empty, int minObservations = 1)
        {
            _empty = empty;
            _minObservations = minObservations;
        }

        public void Add(IEnumerable<T> items)
        {
            var foundInput = false;
            T prevPrev = _empty, prev = _empty;
            foreach (var item in items) {
                var head = (prevPrev, prev);
                if (!_data.TryGetValue(head, out var tempList))
                    _data.Add(head, tempList = new List<T>());
                tempList.Add(item);
                prevPrev = prev;
                prev = item;
                foundInput = true;
            }

            if (foundInput) {
                var last = (prevPrev, prev);
                if (!_data.TryGetValue(last, out var tempList))
                    _data.Add(last, tempList = new List<T>());
                tempList.Add(_empty);
            }
        }

        public MarkovModel2<T> Build()
        {
            var ret = new List<MarkovModelObservation2<T>>();
            foreach (var item in _data) {
                var transitions = item.Value
                    .GroupBy(v => v)
                    .Select(g => (g.Key, g.Count()))
                    .Where(d => d.Item2 >= _minObservations)
                    .ToList()
                ;
                var total = (float)transitions.Sum(t => t.Item2);
                if (total > 0) {
                    ret.Add(new MarkovModelObservation2<T>(item.Key.Item1, item.Key.Item2, transitions.Select(t => new MarkovModelStateTransition<T>(t.Item1, t.Item2 / total))));
                }
            }

            return new MarkovModel2<T>(ret);
        }
    }

    internal class MarkovModelTrainer3<T> : IMarkovModelTrainer3<T> where T : notnull
    {
        readonly Dictionary<(T, T, T), List<T>> _data = new();
        readonly int _minObservations;
        readonly T _empty;

        public MarkovModelTrainer3(T empty, int minObservations = 1)
        {
            _empty = empty;
            _minObservations = minObservations;
        }

        public void Add(IEnumerable<T> items)
        {
            var foundInput = false;
            T prevPrevPrev = _empty, prevPrev = _empty, prev = _empty;
            foreach (var item in items) {
                var head = (prevPrevPrev, prevPrev, prev);
                if (!_data.TryGetValue(head, out var tempList))
                    _data.Add(head, tempList = new List<T>());
                tempList.Add(item);
                prevPrevPrev = prevPrev;
                prevPrev = prev;
                prev = item;
                foundInput = true;
            }

            if (foundInput) {
                var last = (prevPrevPrev, prevPrev, prev);
                if (!_data.TryGetValue(last, out var tempList))
                    _data.Add(last, tempList = new List<T>());
                tempList.Add(_empty);
            }
        }

        public MarkovModel3<T> Build()
        {
            var ret = new List<MarkovModelObservation3<T>>();
            foreach (var item in _data) {
                var transitions = item.Value
                    .GroupBy(v => v)
                    .Select(g => (g.Key, g.Count()))
                    .Where(d => d.Item2 >= _minObservations)
                    .ToList()
                ;
                var total = (float)transitions.Sum(t => t.Item2);
                if (total > 0) {
                    ret.Add(new MarkovModelObservation3<T>(item.Key.Item1, item.Key.Item2, item.Key.Item3, transitions.Select(t => new MarkovModelStateTransition<T>(t.Item1, t.Item2 / total))));
                }
            }

            return new MarkovModel3<T>(ret);
        }
    }
}
