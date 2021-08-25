using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BrightData.DataTable.Builders
{
    /// <summary>
    /// Builds row oriented data tables
    /// </summary>
    internal class RowOrientedTableBuilder : IDisposable
    {
        class Column
        {
            public Column(uint index, BrightDataType type, IMetaData metadata, string name)
            {
                Type = type;
                Metadata = metadata;
                Metadata.Set(Consts.Name, name);
                Metadata.Set(Consts.Index, index);
            }
            public BrightDataType Type { get; }
            public IMetaData Metadata { get; }
        }
        readonly List<Column> _columns = new();
        readonly List<uint> _rowOffset = new();
        readonly Stream _stream;
        readonly BinaryWriter _writer;
        readonly IMetaData _metaData;
        readonly uint _rowCount;
        bool _hasWrittenHeader = false;
        long _rowIndexPosition = -1;
        bool _hasClosedStream = false;

        public RowOrientedTableBuilder(IMetaData metaData, uint rowCount, string? filePath = null)
        {
            _metaData = metaData;
            _rowCount = rowCount;
            _stream = filePath != null ? new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite) : new MemoryStream();
            _writer = new BinaryWriter(_stream, Encoding.UTF8, true);

            if (filePath is not null)
                _metaData.Set(Consts.FilePath, filePath);
        }

        public void Dispose()
        {
            _writer.Dispose();
            if(!_hasClosedStream)
                _stream.Dispose();
        }

        public IMetaData AddColumn(BrightDataType type, IMetaData metaData)
        {
            string name = DataTableBase.DefaultColumnName(metaData.GetName(), _columns.Count);
            var column = new Column((uint) _columns.Count, type, metaData, name);
            _columns.Add(column);
            return column.Metadata;
        }

        public IMetaData TableMetaData => _metaData;
        public IMetaData AddColumn(BrightDataType type, string name) => Add(type, name);
        public IMetaData AddColumn(BrightDataType type) => Add(type, null);
        IMetaData Add(BrightDataType type, string? name)
        {
            var metadata = new MetaData();
            metadata.Set(Consts.Name, DataTableBase.DefaultColumnName(name, _columns.Count));
            return AddColumn(type, metadata);
        }

        public IMetaData AddFixedSizeVectorColumn(uint size, string? name = null)
        {
            var metaData = Add(BrightDataType.Vector, name);
            metaData.Set(Consts.XDimension, size);
            return metaData;
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
                _metaData.WriteTo(_writer);
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

        void WriteValueTo(BinaryWriter writer, BrightDataType type, object val)
        {
            if (type == BrightDataType.Date)
                writer.Write(((DateTime)val).Ticks);
            else if (type == BrightDataType.Boolean)
                writer.Write((bool)val);
            else if (type == BrightDataType.Double)
                writer.Write((double)val);
            else if (type == BrightDataType.Decimal)
                writer.Write((decimal)val);
            else if (type == BrightDataType.Float)
                writer.Write((float)val);
            else if (type == BrightDataType.Short)
                writer.Write((short)val);
            else if (type == BrightDataType.Int)
                writer.Write((int)val);
            else if (type == BrightDataType.Long)
                writer.Write((long)val);
            else if (type == BrightDataType.String)
                writer.Write((string)val);
            else if (type == BrightDataType.Byte)
                writer.Write((sbyte)val);
            else if (val is ICanWriteToBinaryWriter canWrite)
                canWrite.WriteTo(writer);
            else
                throw new NotImplementedException();
        }
    }
}
