using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Helper
{
    public class ClassBasedRowProcessor : IRowProcessor
    {
        readonly int _classColumnIndex;
        readonly Dictionary<string, List<IRowProcessor>> _columnProcessor = new Dictionary<string, List<IRowProcessor>>();

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
            var classValue = row.GetField<string>(_classColumnIndex);
            if (classValue != null) {
                List<IRowProcessor> temp;
                if(_columnProcessor.TryGetValue(classValue, out temp)) {
                    foreach(var item in temp)
                        item.Process(row);
                }
            }
            return true;
        }

        public IEnumerable<Tuple<string, IRowProcessor>> All { get { return _columnProcessor.SelectMany(d => d.Value.Select(d2 => Tuple.Create(d.Key, d2))); } }
    }
}
