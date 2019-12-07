using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;
using BrightTable.Segments;
using BrightTable.Input;

namespace BrightTable.Builders
{
    class ColumnOrientedTableBuilder : IDisposable
    {
        readonly Stream _stream;
        readonly BinaryWriter _writer;
        bool _hasClosedStream = false;

        public ColumnOrientedTableBuilder(string filePath = null)
        {
            _stream = filePath != null ? (Stream)new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite) : new MemoryStream();
            _writer = new BinaryWriter(_stream, Encoding.UTF8, true);
        }

        public void Dispose()
        {
            _writer?.Dispose();
            if (!_hasClosedStream)
                _stream.Dispose();
        }

        public void Write(uint numRows, IReadOnlyList<StringColumn> columns)
        {
            WriteHeader((uint)columns.Count, numRows);

            var columnOffsets = new List<(long Position, long EndOfColumnOffset)>();
            foreach (var column in columns) {
                var metadata = column.MetaData;
                metadata.Set(Consts.Index, column.ColumnIndex);
                metadata.Set(Consts.HasUnique, column.HasUnique);
                metadata.Set(Consts.Type, ColumnType.String.ToString());
                if (column.Header != null)
                    metadata.Set(Consts.Name, column.Header);

                var position = _Write(metadata, ColumnType.String, column, column.HasUnique);
                columnOffsets.Add((position, GetCurrentPosition()));
            }
            WriteColumnOffsets(columnOffsets);
        }

        public void WriteColumnOffsets(IReadOnlyList<(long Position, long EndOfColumnOffset)> offsets)
        {
            _writer.Flush();
            foreach (var offset in offsets) {
                _stream.Seek(offset.Position, SeekOrigin.Begin);
                _writer.Write(offset.EndOfColumnOffset);
                _writer.Flush();
            }
            _stream.Seek(0, SeekOrigin.End);
        }

        long _Write(IMetaData metadata, ColumnType type, ICanWriteToBinaryWriter column, bool isEncoded)
        {
            // create space to write the column size
            _writer.Flush();
            var position = _stream.Position;
            _writer.Write((long)0);

            // write the column type
            _writer.Write((byte)type);

            // write the metadata
            metadata.WriteTo(_writer);

            // write if the column is encoded
            _writer.Write(isEncoded);

            // write the column data
            column.WriteTo(_writer);

            return position;
        }

        public void WriteHeader(uint columnCount, uint rowCount)
        {
            _writer.Write(Consts.DataTableVersion);
            _writer.Write((byte)DataTableOrientation.ColumnOriented);
            _writer.Write(columnCount);
            _writer.Write(rowCount);
        }

        public long Write(ISingleTypeTableSegment column)
        {
            return _Write(column.MetaData, column.SingleType, column, column.IsEncoded);
        }

        public long GetCurrentPosition()
        {
            _writer.Flush();
            return _stream.Position;
        }

        public IColumnOrientedDataTable Build(IBrightDataContext context)
        {
            InputData inputData;
            if (_stream is FileStream file) {
                var filePath = file.Name;
                file.Dispose();
                inputData = new InputData(filePath);
            } else {
                inputData = new InputData((MemoryStream) _stream);
                _stream.Seek(0, SeekOrigin.Begin);
            }

            _hasClosedStream = true;
            return new ColumnOrientedDataTable(context, inputData, true);
        }
    }
}
