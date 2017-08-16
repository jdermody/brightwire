using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.TabularData.Helper
{
    /// <summary>
    /// Processes rows based on each row's classification label
    /// </summary>
    internal class ClassificationBasedRowProcessor : IRowProcessor
    {
        readonly int _classificationColumnIndex;
        readonly Dictionary<string, List<IRowProcessor>> _columnProcessor = new Dictionary<string, List<IRowProcessor>>();
        readonly Dictionary<string, uint> _classCount = new Dictionary<string, uint>();
        uint _total = 0;

        public ClassificationBasedRowProcessor(IEnumerable<Tuple<string, IRowProcessor>> classificationBasedProcessors, int classificationColumnIndex)
        {
            _classificationColumnIndex = classificationColumnIndex;

            foreach (var item in classificationBasedProcessors) {
                if (!_columnProcessor.TryGetValue(item.Item1, out List<IRowProcessor> temp))
                    _columnProcessor.Add(item.Item1, temp = new List<IRowProcessor>());
                temp.Add(item.Item2);
            }
        }

        public bool Process(IRow row)
        {
            var classValue = row.GetField<string>(_classificationColumnIndex);

            if (classValue != null) {
                if (_columnProcessor.TryGetValue(classValue, out List<IRowProcessor> temp)) {
                    foreach (var item in temp)
                        item.Process(row);
                    if (_classCount.TryGetValue(classValue, out uint temp2))
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
            if (_classCount.TryGetValue(className, out uint temp))
                return temp / (double)_total;
            return 0;
        }
    }
}
