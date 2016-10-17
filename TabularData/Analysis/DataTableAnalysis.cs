using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Analysis
{
    internal class DataTableAnalysis : IRowProcessor, IDataTableAnalysis
    {
        readonly List<IRowProcessor> _column = new List<IRowProcessor>();

        public DataTableAnalysis(IDataTable table)
        {
            int index = 0;
            foreach (var item in table.Columns) {
                if (ColumnTypeClassifier.IsNumeric(item))
                    _column.Add(new NumberCollector(index));
                else if (item.Type == ColumnType.String)
                    _column.Add(new StringCollector(index));
                ++index;
            }
        }

        public DataTableAnalysis(IDataTable table, int columnIndex)
        {
            var item = table.Columns[columnIndex];
            if (ColumnTypeClassifier.IsNumeric(item))
                _column.Add(new NumberCollector(columnIndex));
            else if (item.Type == ColumnType.String)
                _column.Add(new StringCollector(columnIndex));
        }

        public bool Process(IRow row)
        {
            foreach (var item in _column)
                item.Process(row);
            return true;
        }

        public IEnumerable<IColumnInfo> ColumnInfo { get { return _column.Cast<IColumnInfo>(); } }

        public IColumnInfo this[int columnIndex]
        {
            get
            {
                return _column
                    .Cast<IColumnInfo>()
                    .Where(c => c.ColumnIndex == columnIndex)
                    .FirstOrDefault()
                ;
            }
        }
    }
}
