using BrightWire.Helper;
using BrightWire.TabularData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightWire.TabularData.Helper
{
    internal class DataTableWriter : IRowProcessor
    {
        readonly List<long> _index = new List<long>();
        readonly DataTableBuilder _dataTableBuilder;
        readonly Stream _stream;
        int _rowCount = 0;
        bool _hasWrittenHeader = false;

        public DataTableWriter(Stream stream)
        {
            _dataTableBuilder = new DataTableBuilder();
            _stream = stream ?? new MemoryStream();
        }
        public DataTableWriter(IEnumerable<IColumn> columns, Stream stream)
        {
            _dataTableBuilder = new DataTableBuilder(columns);
            _stream = stream ?? new MemoryStream();
        }

        public IReadOnlyList<DataTableBuilder.Column> Columns { get { return _dataTableBuilder.Columns2; } }
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

            _dataTableBuilder.AddColumn(type, name, isTarget);
        }

        public void AddRow(IRow row)
        {
            _dataTableBuilder.AddRow(row);
            ++_rowCount;
            if ((_rowCount % DataTable.BLOCK_SIZE) == 0)
                _Write();
        }

        public void AddRow(IReadOnlyList<object> data)
        {
            AddRow(_dataTableBuilder.CreateRow(data));
        }

        void _Write()
        {
            if (!_hasWrittenHeader) {
                _dataTableBuilder.WriteMetadata(_stream);
                _hasWrittenHeader = true;
            }
            if (_dataTableBuilder.RowCount > 0) {
                _index.Add(_stream.Position);
                _dataTableBuilder.WriteData(_stream);
                _dataTableBuilder.ClearRows();
            }
        }

        public bool Process(IRow row)
        {
            AddRow(row);
            return true;
        }
    }
}
