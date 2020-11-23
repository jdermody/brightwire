using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BrightWire.Bayesian.Training
{
    /// <summary>
    /// Builds markov models with a window size of 2
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MarkovModelTrainer2<T> : IMarkovModelTrainer2<T>
    {
        readonly Dictionary<(T, T), List<T>> _data = new Dictionary<(T, T), List<T>>();
        readonly int _minObservations;

        public MarkovModelTrainer2(int minObservations = 1)
        {
            _minObservations = minObservations;
        }

        public void Add(IEnumerable<T> items)
        {
			var foundInput = false;
            T prevPrev = default, prev = default;
            foreach (var item in items) {
                var head = (prevPrev, prev);
                if (!_data.TryGetValue(head, out var tempList))
                    _data.Add(head, tempList = new List<T>());
                tempList.Add(item);
                prevPrev = prev;
                prev = item;
	            foundInput = true;
            }

	        if (foundInput)
	        {
		        var last = (prevPrev, prev);
		        if (!_data.TryGetValue(last, out var tempList))
			        _data.Add(last, tempList = new List<T>());
		        tempList.Add(default);
	        }
        }

        public MarkovModel2<T> Build()
        {
            var ret = new List<MarkovModelObservation2<T>>();
            foreach (var item in _data) {
                var transitions = item.Value
                    .GroupBy(v => v)
                    .Select(g => Tuple.Create(g.Key, g.Count()))
                    .Where(d => d.Item2 >= _minObservations)
                    .ToList()
                ;
                var total = (float)transitions.Sum(t => t.Item2);
                if (total > 0) {
                    ret.Add(new MarkovModelObservation2<T>(item.Key.Item1, item.Key.Item2, transitions.Select(t => new MarkovModelStateTransition<T> {
                        NextState = t.Item1,
                        Probability = t.Item2 / total
                    }).ToArray()));
                }
            } 
            return new MarkovModel2<T> {
                Observation = ret.ToArray()
            };
        }

        public void DeserialiseFrom(Stream stream, bool clear)
        {
            var formatter = new BinaryFormatter();
            var state = (Dictionary<Tuple<T, T>, List<T>>)formatter.Deserialize(stream);

            if(clear)
                _data.Clear();
            foreach (var item in state)
                _data.Add((item.Key.Item1, item.Key.Item2), item.Value);
        }

        public void SerialiseTo(Stream stream)
        {
            var formatter = new BinaryFormatter();

            // serialise as Tuple as ValueTuple is not binary serializable
            var tempDictionary = _data.ToDictionary(kv => Tuple.Create(kv.Key.Item1, kv.Key.Item2), kv => kv.Value);
            formatter.Serialize(stream, tempDictionary);
        }
    }

    internal class MarkovModelTrainer3<T> : IMarkovModelTrainer3<T>
    {
        readonly Dictionary<(T, T, T), List<T>> _data = new Dictionary<(T, T, T), List<T>>();
        readonly int _minObservations;

        public MarkovModelTrainer3(int minObservations = 1)
        {
            _minObservations = minObservations;
        }

        public void Add(IEnumerable<T> items)
        {
            var foundInput = false;
            T prevPrevPrev = default, prevPrev = default, prev = default;
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

	        if (foundInput)
	        {
		        var last = (prevPrevPrev, prevPrev, prev);
		        if (!_data.TryGetValue(last, out var tempList))
			        _data.Add(last, tempList = new List<T>());
		        tempList.Add(default);
	        }
        }

        public MarkovModel3<T> Build()
        {
            var ret = new List<MarkovModelObservation3<T>>();
            foreach (var item in _data) {
                var transitions = item.Value
                    .GroupBy(v => v)
                    .Select(g => Tuple.Create(g.Key, g.Count()))
                    .Where(d => d.Item2 >= _minObservations)
                    .ToList()
                ;
                var total = (float)transitions.Sum(t => t.Item2);
                if (total > 0) {
                    ret.Add(new MarkovModelObservation3<T>(item.Key.Item1, item.Key.Item2, item.Key.Item3, transitions.Select(t => new MarkovModelStateTransition<T> {
                        NextState = t.Item1,
                        Probability = t.Item2 / total
                    }).ToArray()));
                }
            }
            return new MarkovModel3<T> {
                Observation = ret.ToArray()
            };
        }

        public void DeserialiseFrom(Stream stream, bool clear)
        {
            var formatter = new BinaryFormatter();
            var state = (Dictionary<Tuple<T, T, T>, List <T>>)formatter.Deserialize(stream);

            if(clear)
                _data.Clear();
            foreach (var item in state)
                _data.Add((item.Key.Item1, item.Key.Item2, item.Key.Item3), item.Value);
        }

        public void SerialiseTo(Stream stream)
        {
            var formatter = new BinaryFormatter();

            // serialise as Tuple as ValueTuple is not binary serializable
            var tempDictionary = _data.ToDictionary(kv => Tuple.Create(kv.Key.Item1, kv.Key.Item2, kv.Key.Item3), kv => kv.Value);
            formatter.Serialize(stream, tempDictionary);
        }
    }
}
