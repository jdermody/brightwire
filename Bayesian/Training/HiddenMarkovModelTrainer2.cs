using BrightWire.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Bayesian
{
    public class HiddenMarkovModelTrainer2<T>
    {
        readonly Dictionary<Tuple<T, T>, List<T>> _data = new Dictionary<Tuple<T, T>, List<T>>();
        readonly int _minObservations;

        public HiddenMarkovModelTrainer2(int minObservations = 1)
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

        public IEnumerable<HMMObservation2<T>> All
        {
            get
            {
                foreach(var item in _data) {
                    var transitions = item.Value
                        .GroupBy(v => v)
                        .Select(g => Tuple.Create(g.Key, g.Count()))
                        .Where(d => d.Item2 >= _minObservations)
                        .ToList()
                    ;
                    var total = (float)transitions.Sum(t => t.Item2);
                    if (total > 0) {
                        yield return new HMMObservation2<T>(item.Key.Item1, item.Key.Item2, transitions.Select(t => new HMMStateTransition<T> {
                            NextState = t.Item1,
                            Probability = t.Item2 / total
                        }).ToList());
                    }
                }
            }
        }
    }
}
