using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BrightData.DataTable.Builders
{
    internal class RowOrientedTableBuilder : IDisposable
    {
        class Column
        {
            public Column(uint index, ColumnType type, IMetaData metadata, string name)
            {
                Type = type;
                Metadata = metadata;
                Metadata.Set(Consts.Name, name);
                Metadata.Set(Consts.Index, index);
            }
            public ColumnType Type { get; }
            public IMetaData Metadata { get; }
        }
        readonly List<Column> _columns = new List<Column>();
        readonly List<uint> _rowOffset = new List<uint>();
        readonly Stream _stream;
        readonly BinaryWriter _writer;
        readonly uint _rowCount;
        bool _hasWrittenHeader = false;
        long _rowIndexPosition = -1;
        bool _hasClosedStream = false;

        public RowOrientedTableBuilder(uint rowCount, string? filePath = null)
        {
            _rowCount = rowCount;
            _stream = filePath != null ? (Stream)new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite) : new MemoryStream();
            _writer = new BinaryWriter(_stream, Encoding.UTF8, true);
        }

        public void Dispose()
        {
            _writer.Dispose();
            if(!_hasClosedStream)
                _stream.Dispose();
        }

        public IMetaData AddColumn(ColumnType type, IMetaData metaData)
        {
            string name = DataTableBase.DefaultColumnName(metaData.GetName(), _columns.Count);
            var column = new Column((uint) _columns.Count, type, metaData, name);
            _columns.Add(column);
            return column.Metadata;
        }

        public IMetaData AddColumn(ColumnType type, string name)
        {
            var metadata = new MetaData();
            metadata.Set(Consts.Name, DataTableBase.DefaultColumnName(name, _columns.Count));
            return AddColumn(type, metadata);
        }

        public IMetaData AddColumn(ColumnType type)
        {
            var metadata = new MetaData();
            metadata.Set(Consts.Name, DataTableBase.DefaultColumnName(null, _columns.Count));
            return AddColumn(type, metadata);
        }

        public void AddColumnsFrom(IDataTable dataTable)
        {
            var types = dataTable.ColumnTypes;
            for(uint i = 0; i < dataTable.ColumnCount; i++) {
                var type = types[(int)i];
                var metadata = dataTable.ColumnMetaData(i);
                AddColumn(type, metadata);
            }
        }

        public void AddRow(params object[] values)
        {
            if (!_hasWrittenHeader)
            {
                _hasWrittenHeader = true;
                _writer.Write(Consts.DataTableVersion); // format version
                _writer.Write((byte)DataTableOrientation.RowOriented);
                _writer.Write((uint)_columns.Count); // write the number of columns
                foreach (var column in _columns)
                {
                    var type = column.Type;
                    _writer.Write((byte)type);
                    column.Metadata.WriteTo(_writer);
                }

                // create space for the row indices
                _writer.Write(_rowCount);
                _writer.Flush();
                _rowIndexPosition = _writer.BaseStream.Position;
                for (uint i = 0; i < _rowCount; i++)
                    _writer.Write((uint) 0);
            }

            var hasWrittenIndex = false;
            for (int i = 0, len = _columns.Count, len2 = values.Length; i < len; i++)
            {
                var column = _columns[i];
                var type = column.Type;
                var val = i < len2 
                    ? values[i] 
                    : type.GetDefaultValue();
                if (val == null)
                    throw new Exception("Values cannot be null");

                if (!hasWrittenIndex)
                {
                    hasWrittenIndex = true;
                    _rowOffset.Add((uint)_stream.Position);
                    if (_rowOffset.Count > _rowCount)
                        throw new Exception("Added more rows than specified");
                }
                WriteValueTo(_writer, type, val);
            }
        }

        public IRowOrientedDataTable Build(IBrightDataContext context)
        {
            // write the actual row indices
            _stream.Seek(_rowIndexPosition, SeekOrigin.Begin);
            Debug.Assert(_rowOffset.Count == _rowCount);
            foreach(var offset in _rowOffset)
                _writer.Write(offset);

            _writer.Flush();
            _stream.Flush();

            _hasClosedStream = true;
            _stream.Seek(0, SeekOrigin.Begin);
            return new RowOrientedDataTable(context, _stream, true);
        }

        void WriteValueTo(BinaryWriter writer, ColumnType type, object val)
        {
            if (type == ColumnType.Date)
                writer.Write(((DateTime)val).Ticks);
            else if (type == ColumnType.Boolean)
                writer.Write((bool)val);
            else if (type == ColumnType.Double)
                writer.Write((double)val);
            else if (type == ColumnType.Decimal)
                writer.Write((decimal)val);
            else if (type == ColumnType.Float)
                writer.Write((float)val);
            else if (type == ColumnType.Short)
                writer.Write((short)val);
            else if (type == ColumnType.Int)
                writer.Write((int)val);
            else if (type == ColumnType.Long)
                writer.Write((long)val);
            else if (type == ColumnType.String)
                writer.Write((string)val);
            else if (type == ColumnType.Byte)
                writer.Write((sbyte)val);
            else if (val is ICanWriteToBinaryWriter canWrite)
                canWrite.WriteTo(writer);
            else
                throw new NotImplementedException();
        }
    }
}
