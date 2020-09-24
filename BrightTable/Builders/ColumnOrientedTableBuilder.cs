using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;

namespace BrightTable.Builders
{
    class ColumnOrientedTableBuilder : IDisposable
    {
        readonly Stream _stream;
        readonly BinaryWriter _writer;
        bool _hasClosedStream = false;

        public ColumnOrientedTableBuilder(string filePath = null)
        {
            _stream = filePath != null 
                ? (Stream)new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite) 
                : new MemoryStream();
            _writer = new BinaryWriter(_stream, Encoding.UTF8, true);
        }

        public void Dispose()
        {
            if (!_hasClosedStream) {
                _writer.Dispose();
                _stream.Dispose();
                _hasClosedStream = true;
            }
        }

        public void WriteColumnOffsets(IEnumerable<(long Position, long EndOfColumnOffset)> offsets)
        {
            _writer.Flush();
            foreach (var (position, endOfColumnOffset) in offsets) {
                _stream.Seek(position, SeekOrigin.Begin);
                _writer.Write(endOfColumnOffset);
                _writer.Flush();
            }
            _stream.Seek(0, SeekOrigin.End);
        }

        long _Write(IMetaData metadata, ColumnType type, ICanWriteToBinaryWriter column)
        {
            _writer.Flush();
            var position = _stream.Position;

            _writer.Write((long)0);

            // write the column type
            _writer.Write((byte)type);

            // write the metadata
            metadata.WriteTo(_writer);

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
            return _Write(column.MetaData, column.SingleType, column);
        }

        public long GetCurrentPosition()
        {
            _writer.Flush();
            return _stream.Position;
        }

        public IColumnOrientedDataTable Build(IBrightDataContext context)
        {
            _writer.Flush();
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            _hasClosedStream = true;
            return new ColumnOrientedDataTable(context, _stream, true);
        }
    }
}
