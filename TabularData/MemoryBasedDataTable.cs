using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData
{
    public class MemoryBasedDataTable : IDataTable
    {
        readonly IReadOnlyList<IColumn> _columns;
        readonly IReadOnlyList<IRow> _data;

        public MemoryBasedDataTable(IReadOnlyList<IColumn> columns, IReadOnlyList<IRow> data)
        {
            _columns = columns;
            _data = data;
        }

        public int ColumnCount
        {
            get
            {
                return _columns.Count;
            }
        }

        public IReadOnlyList<IColumn> Columns
        {
            get
            {
                return _columns;
            }
        }

        public int RowCount
        {
            get
            {
                return _data.Count;
            }
        }

        public void Process(IRowProcessor rowProcessor)
        {
            foreach(var item in _data) {
                if (!rowProcessor.Process(item))
                    break;
            }
        }
    }
}
