using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightWire.TabularData.Helper
{
    /// <summary>
    /// Manages writing data tables to streams and building the row index within the saved stream
    /// </summary>
    class DataTableWriter : IRowProcessor, IDataTableBuilder
    {
        readonly List<long> _index = new List<long>();
        readonly DataTableBuilder _dataTableBuilder;
        readonly Stream _stream;
        int _rowCount = 0;
        bool _hasWrittenHeader = false;

        public DataTableWriter(Stream stream, bool validate = false)
        {
            _dataTableBuilder = new DataTableBuilder(validate);
            _stream = stream ?? new MemoryStream();
        }
        public DataTableWriter(IEnumerable<IColumn> columns, Stream stream, bool validate = false)
        {
            _dataTableBuilder = new DataTableBuilder(columns, validate);
            _stream = stream ?? new MemoryStream();
        }

        public void Dispose()
        {
            Flush();
        }

		public IReadOnlyList<DataTableBuilder.Column> Columns => _dataTableBuilder.Columns2;
	    public IReadOnlyList<long> Index => _index;
	    public int RowCount => _rowCount;
	    IReadOnlyList<IColumn> IDataTableBuilder.Columns => _dataTableBuilder.Columns;
        int IDataTableBuilder.ColumnCount => _dataTableBuilder.ColumnCount;

        public void Flush()
        {
            _Write();
        }

        public DataTable GetDataTable()
        {
            Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return new DataTable(_stream, _index, _rowCount);
        }

        public void AddColumn(string name, ColumnType type, bool isTarget = false)
        {
            if (_hasWrittenHeader)
                throw new Exception();

            _dataTableBuilder.AddColumn(type, name, isTarget);
        }

        public IRow AddRow(IRow row)
        {
            var ret = _dataTableBuilder.AddRow(row);
            ++_rowCount;
            if (_rowCount % _dataTableBuilder.BlockSize == 0)
                _Write();
            return ret;
        }

        public IRow AddRow(IReadOnlyList<object> data)
        {
            return AddRow(_dataTableBuilder.CreateRow(data));
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

        IColumn IDataTableBuilder.AddColumn(ColumnType column, string name, bool isTarget)
        {
            return _dataTableBuilder.AddColumn(column, name, isTarget);
        }

	    IColumn IDataTableBuilder.AddVectorColumn(int size, string name, bool isTarget)
	    {
		    return _dataTableBuilder.AddVectorColumn(size, name, isTarget);
	    }

	    IColumn IDataTableBuilder.AddMatrixColumn(int rows, int columns, string name, bool isTarget)
	    {
		    return _dataTableBuilder.AddMatrixColumn(rows, columns, name, isTarget);
	    }

	    IColumn IDataTableBuilder.AddTensorColumn(int rows, int columns, int depth, string name, bool isTarget)
	    {
		    return _dataTableBuilder.AddTensorColumn(rows, columns, depth, name, isTarget);
	    }

	    IRow IDataTableBuilder.Add(params object[] data)
        {
            return AddRow(data);
        }

        IRow IDataTableBuilder.Add(IReadOnlyList<object> data)
        {
            return AddRow(data);
        }

        IDataTable IDataTableBuilder.Build()
        {
            return GetDataTable();
        }

        public void WriteIndexTo(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
                writer.Write(_rowCount);
                writer.Write(_index.Count);
                foreach (var item in _index)
                    writer.Write(item);
            }
        }
    }
}
