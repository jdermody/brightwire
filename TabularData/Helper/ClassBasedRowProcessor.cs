using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Helper
{
    internal class ClassBasedRowProcessor : IRowProcessor
    {
        readonly int _classColumnIndex;
        readonly Dictionary<string, List<IRowProcessor>> _columnProcessor = new Dictionary<string, List<IRowProcessor>>();
        readonly Dictionary<string, uint> _classCount = new Dictionary<string, uint>();
        uint _total = 0;

        public ClassBasedRowProcessor(IEnumerable<Tuple<string, IRowProcessor>> classBasedProcessors, int classColumnIndex)
        {
            _classColumnIndex = classColumnIndex;

            List<IRowProcessor> temp;
            foreach (var item in classBasedProcessors) {
                if (!_columnProcessor.TryGetValue(item.Item1, out temp))
                    _columnProcessor.Add(item.Item1, temp = new List<IRowProcessor>());
                temp.Add(item.Item2);
            }
        }

        public bool Process(IRow row)
        {
            uint temp2;
            List<IRowProcessor> temp;
            var classValue = row.GetField<string>(_classColumnIndex);

            if (classValue != null) {
                if(_columnProcessor.TryGetValue(classValue, out temp)) {
                    foreach(var item in temp)
                        item.Process(row);
                    if (_classCount.TryGetValue(classValue, out temp2))
                        _classCount[classValue] = temp2 + 1;
                    else
                        _classCount[classValue] = 1;
                }
                ++_total;
            }
            return true;
        }

        public IEnumerable<Tuple<string, IRowProcessor>> All { get { return _columnProcessor.SelectMany(d => d.Value.Select(d2 => Tuple.Create(d.Key, d2))); } }

        public double GetProbability(string className)
        {
            uint temp;
            if (_classCount.TryGetValue(className, out temp))
                return temp / (double)_total;
            return 0;
        }
    }
}
