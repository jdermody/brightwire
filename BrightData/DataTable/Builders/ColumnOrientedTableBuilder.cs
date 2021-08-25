using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData.Helper;

namespace BrightData.DataTable.Builders
{
    /// <summary>
    /// Builds column oriented data tables
    /// </summary>
    internal class ColumnOrientedTableBuilder : IDisposable
    {
        readonly string? _filePath;
        readonly Stream _stream;
        readonly BinaryWriter _writer;
        bool _hasClosedStream = false;

        public ColumnOrientedTableBuilder(string? filePath = null)
        {
            _filePath = filePath;
            _stream = filePath != null 
                ? new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite) 
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

        long Write(IMetaData metaData, BrightDataType type, ICanWriteToBinaryWriter column)
        {
            _writer.Flush();
            var position = _stream.Position;

            // create some space to write the end of column position later
            _writer.Write((long)0);

            // write the column type
            _writer.Write((byte)type);

            // write the metadata
            metaData.WriteTo(_writer);

            // write the column data
            column.WriteTo(_writer);

            return position;
        }

        public void WriteHeader(uint columnCount, uint rowCount, IMetaData metaData)
        {
            _writer.Write(Consts.DataTableVersion);
            _writer.Write((byte)DataTableOrientation.ColumnOriented);
            _writer.Write(columnCount);
            _writer.Write(rowCount);

            if (_filePath is not null)
                metaData.Set(Consts.FilePath, _filePath);
            metaData.WriteTo(_writer);
        }

        public long Write(ISingleTypeTableSegment column)
        {
            return Write(column.MetaData, column.SingleType, column);
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
            var cloner = new StreamCloner(_stream);
            _hasClosedStream = true;
            return new ColumnOrientedDataTable(context, _stream, true, cloner);
        }
    }
}
