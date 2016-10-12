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
        readonly Stream _stream;
        int _rowCount = 0;
        bool _hasWrittenHeader = false;

        public DataTableWriter(Stream stream)
        {
            _dataTable = new MutableDataTable();
            _stream = stream ?? new MemoryStream();
        }
        public DataTableWriter(IEnumerable<IColumn> columns, Stream stream)
        {
            _dataTable = new MutableDataTable(columns);
            _stream = stream ?? new MemoryStream();
        }

        public IReadOnlyList<MutableDataTable.Column> Columns { get { return _dataTable.Columns2; } }
        public IReadOnlyList<long> Index { get { return _index; } }
        public int RowCount { get { return _rowCount; } }

        public DataTable GetDataTable()
        {
            _Write();
            _stream.Seek(0, SeekOrigin.Begin);
            return new DataTable(_stream, _index, _rowCount);
        }

        public void AddColumn(string name, ColumnType type, bool isTarget = false)
        {
            if (_hasWrittenHeader)
                throw new Exception();

            _dataTable.AddColumn(type, name, isTarget);
        }

        public void AddRow(IEnumerable<object> row)
        {
            _dataTable.AddRow(row);
            ++_rowCount;
            if ((_rowCount % 1024) == 0)
                _Write();
        }

        void _Write()
        {
            if (!_hasWrittenHeader) {
                _dataTable.WriteMetadata(_stream);
                _hasWrittenHeader = true;
            }
            if (_dataTable.RowCount > 0) {
                _index.Add(_stream.Position);
                _dataTable.WriteData(_stream);
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
