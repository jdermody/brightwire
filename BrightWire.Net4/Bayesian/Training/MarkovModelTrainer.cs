﻿using BrightWire.Models.Bayesian;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.IO;
using ProtoBuf;

namespace BrightWire.Bayesian.Training
{
    /// <summary>
    /// Builds markov models with a window size of 2
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MarkovModelTrainer2<T> : IMarkovModelTrainer2<T>
    {
        readonly Dictionary<Tuple<T, T>, List<T>> _data = new Dictionary<Tuple<T, T>, List<T>>();
        readonly int _minObservations;

        public MarkovModelTrainer2(int minObservations = 1)
        {
            _minObservations = minObservations;
        }

        public void Add(IEnumerable<T> items)
        {
            List<T> tempList;
            if (!items.Any())
                return;

            T prevPrev = default(T), prev = default(T);
            foreach (var item in items) {
                var head = Tuple.Create(prevPrev, prev);
                if (!_data.TryGetValue(head, out tempList))
                    _data.Add(head, tempList = new List<T>());
                tempList.Add(item);
                prevPrev = prev;
                prev = item;
            }
            var last = Tuple.Create(prevPrev, prev);
            if (!_data.TryGetValue(last, out tempList))
                _data.Add(last, tempList = new List<T>());
            tempList.Add(default(T));
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
                    }).ToList()));
                }
            }
            return new MarkovModel2<T> {
                Observation = ret.ToArray()
            };
        }
    }

    internal class MarkovModelTrainer3<T> : IMarkovModelTrainer3<T>
    {
        readonly Dictionary<Tuple<T, T, T>, List<T>> _data = new Dictionary<Tuple<T, T, T>, List<T>>();
        readonly int _minObservations;

        public MarkovModelTrainer3(int minObservations = 1)
        {
            _minObservations = minObservations;
        }

        public void Add(IEnumerable<T> items)
        {
            List<T> tempList;
            if (!items.Any())
                return;

            T prevPrevPrev = default(T), prevPrev = default(T), prev = default(T);
            foreach (var item in items) {
                var head = Tuple.Create(prevPrevPrev, prevPrev, prev);
                if (!_data.TryGetValue(head, out tempList))
                    _data.Add(head, tempList = new List<T>());
                tempList.Add(item);
                prevPrevPrev = prevPrev;
                prevPrev = prev;
                prev = item;
            }
            var last = Tuple.Create(prevPrevPrev, prevPrev, prev);
            if (!_data.TryGetValue(last, out tempList))
                _data.Add(last, tempList = new List<T>());
            tempList.Add(default(T));
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
                    }).ToList()));
                }
            }
            return new MarkovModel3<T> {
                Observation = ret.ToArray()
            };
        }

        public void Save(Stream stream)
        {
            Serializer.Serialize(stream, _data);
        }

        public void Load(Stream stream)
        {
            var loaded = Serializer.Deserialize<Dictionary<Tuple<T, T, T>, List<T>>>(stream);

            foreach (var item in loaded)
            {
                _data.Add(item.Key,item.Value);
            }
        }
    }
}
