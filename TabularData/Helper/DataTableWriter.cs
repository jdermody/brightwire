using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Helper
{
    public class DataTableWriter : IRowProcessor
    {
        readonly List<long> _index = new List<long>();
        readonly MutableDataTable _dataTable;
        int _rowCount = 0;
        bool _hasWrittenHeader = false;

        public DataTableWriter()
        {
            _dataTable = new MutableDataTable();
        }
        public DataTableWriter(IEnumerable<IColumn> columns)
        {
            _dataTable = new MutableDataTable(columns);
        }

        public IReadOnlyList<MutableDataTable.Column> Columns { get { return _dataTable.Columns2; } }
        public IReadOnlyList<long> Index { get { return _index; } }
        public int RowCount { get { return _rowCount; } }

        public IndexedDataTable GetIndexedTable(Stream stream = null)
        {
            if (stream == null)
                stream = new MemoryStream();

            WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new IndexedDataTable(stream, _index, _rowCount);
        }

        public void AddColumn(string name, ColumnType type)
        {
            _dataTable.Add(type, name);
        }

        public void AddRow(IEnumerable<object> row)
        {
            _dataTable.AddRow(row);
            ++_rowCount;
        }

        public void WriteTo(Stream stream)
        {
            if (!_hasWrittenHeader) {
                _dataTable.WriteMetadata(stream);
                _hasWrittenHeader = true;
            }
            if (_dataTable.RowCount > 0) {
                _index.Add(stream.Position);
                _dataTable.WriteData(stream);
                _dataTable.Clear();
            }
        }

        public bool Process(IRow row)
        {
            AddRow(row.Data);
            return true;
        }
    }
}
