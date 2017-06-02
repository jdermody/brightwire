using BrightWire.TabularData.Helper;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.TabularData.Analysis
{
    /// <summary>
    /// Collects frequency meta data from data tables
    /// </summary>
    internal class FrequencyAnalysis : IRowProcessor
    {
        readonly List<IRowProcessor> _column = new List<IRowProcessor>();

        public FrequencyAnalysis(IDataTable table, int ignoreColumnIndex = -1)
        {
            int index = 0;
            foreach (var item in table.Columns) {
                if (index != ignoreColumnIndex) {
                    if (item.IsContinuous)
                        _column.Add(new NumberCollector(index));
                    else if (ColumnTypeClassifier.IsCategorical(item))
                        _column.Add(new FrequencyCollector(index));
                }
                ++index;
            }
        }

        public bool Process(IRow row)
        {
            foreach (var item in _column)
                item.Process(row);
            return true;
        }

        public IEnumerable<IColumnInfo> ColumnInfo { get { return _column.Cast<IColumnInfo>(); } }
    }
}
