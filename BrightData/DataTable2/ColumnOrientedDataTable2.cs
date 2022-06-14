using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2
{
    internal class ColumnOrientedDataTable2 : IDisposable
    {
        internal readonly struct Header
        {
            public readonly uint ColumnCount;
            public readonly uint RowCount;
            public readonly uint MetaDataCount;

            public readonly uint StringOffset;
            public readonly uint StringCount;

            public readonly uint TensorOffset;
            public readonly uint TensorCount;

            public readonly uint BinaryDataOffset;
            public readonly uint BinaryDataCount;

            public readonly uint IndexOffset;
            public readonly uint IndexCount;

            public readonly uint WeightedIndexOffset;
            public readonly uint WeightedIndexCount;
        }
        internal readonly struct Column
        {
            public readonly BrightDataType DataType;
            public readonly uint MetaDataIndex;
            public readonly uint Offset;
            public readonly uint DataTypeSize;
        }

        readonly Stream                         _stream;
        readonly Header                         _header;
        readonly Column[]                       _columns;
        readonly MetaData[]                     _metaData;
        readonly Lazy<string[]>                 _stringTable;
        readonly Lazy<float[]>                  _tensors;
        readonly Lazy<BinaryData[]>             _binaryData;
        readonly Lazy<uint[]>                   _indices;
        readonly Lazy<WeightedIndexList.Item[]> _weightedIndices;
        readonly uint                           _rowSize;

        public ColumnOrientedDataTable2(Stream stream)
        {
            _stream = stream;
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            // read the header
            _header = MemoryMarshal.Cast<byte, Header>(reader.ReadBytes(Unsafe.SizeOf<Header>()))[0];
            var numColumns = (int)_header.ColumnCount;

            // read the columns
            _columns = new Column[numColumns];
            MemoryMarshal
                .Cast<byte, Column>(reader.ReadBytes(Unsafe.SizeOf<Column>() * numColumns))
                .CopyTo(_columns)
            ;
            _rowSize = (uint)_columns.Sum(c => c.DataTypeSize);
            var dataSize = _rowSize * _header.RowCount;

            // read the meta data
            _metaData = new MetaData[_header.MetaDataCount];
            for (uint i = 0; i < _header.MetaDataCount; i++)
                _metaData[i] = new MetaData(reader);

            // setup the lazy readers
            _stringTable     = new(() => ReadStringTable(_stream, _header.StringOffset, _header.StringCount));
            _binaryData      = new(() => ReadBinaryData(_stream, _header.BinaryDataOffset, _header.BinaryDataCount));
            _tensors         = new(() => ReadStruct<float>(_stream, _header.TensorOffset, _header.TensorCount));
            _indices         = new(() => ReadStruct<uint>(_stream, _header.IndexOffset, _header.IndexCount));
            _weightedIndices = new(() => ReadStruct<WeightedIndexList.Item>(_stream, _header.WeightedIndexOffset, _header.WeightedIndexCount));
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        static string[] ReadStringTable(Stream stream, uint offset, uint count)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            lock (stream) {
                stream.Seek(offset, SeekOrigin.Begin);
                var ret = new string[count];
                for (uint i = 0; i < count; i++)
                    ret[i] = reader.ReadString();
                return ret;
            }
        }

        static BinaryData[] ReadBinaryData(Stream stream, uint offset, uint count)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            lock (stream) {
                stream.Seek(offset, SeekOrigin.Begin);
                var ret = new BinaryData[count];
                for (uint i = 0; i < count; i++)
                    ret[i] = new BinaryData(reader);
                return ret;
            }
        }

        static T[] ReadStruct<T>(Stream stream, uint offset, uint count)
            where T : struct
        {
            lock (stream) {
                stream.Seek(offset, SeekOrigin.Begin);
                var ret = new T[count];
                var readBytes = stream.Read(MemoryMarshal.AsBytes(ret.AsSpan()));
                return ret;
            }
        }
    }
}
