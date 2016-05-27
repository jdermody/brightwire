using Icbld.BrightWire.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Bayesian
{
    public class HiddenMarkovModel3<T>
    {
        readonly Dictionary<Tuple<T, T, T>, List<T>> _data = new Dictionary<Tuple<T, T, T>, List<T>>();

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

        public IEnumerable<HMMObservation3<T>> All
        {
            get
            {
                foreach (var item in _data) {
                    var transitions = item.Value
                        .GroupBy(v => v)
                        .Select(g => Tuple.Create(g.Key, g.Count()))
                        .Where(d => d.Item2 > 1)
                        .ToList()
                    ;
                    var total = (float)transitions.Sum(t => t.Item2);
                    if (total > 0) {
                        yield return new HMMObservation3<T>(item.Key.Item1, item.Key.Item2, item.Key.Item3, transitions.Select(t => new HMMStateTransition<T> {
                            NextState = t.Item1,
                            Probability = t.Item2 / total
                        }).ToList());
                    }
                }
            }
        }
    }
}
