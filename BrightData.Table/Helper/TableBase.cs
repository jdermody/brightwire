using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BrightData.Table.Helper
{
    internal class TableBase : IDataTable
    {
        internal struct Column
        {
            public BrightDataType DataType;
            public uint DataTypeSize;
        }
        protected readonly IByteReader   _reader;
        protected readonly TableHeader   _header;
        protected readonly Column[]      _columns;
        Lazy<Task<ReadOnlyMemory<byte>>> _strings;
        Lazy<Task<ReadOnlyMemory<byte>>> _tensors;
        Lazy<Task<ReadOnlyMemory<byte>>> _binaryData;
        Lazy<Task<ReadOnlyMemory<byte>>> _indexLists;
        Lazy<Task<ReadOnlyMemory<byte>>> _weightedIndexLists;
        Lazy<Task<ReadOnlyMemory<byte>>> _metaData;

        protected unsafe TableBase(IByteReader reader, DataTableOrientation expectedOrientation)
        {
            _reader = reader;
            _header = _reader.GetBlock(0, (uint)sizeof(TableHeader)).Result.Span.Cast<byte, TableHeader>()[0];
            if (_header.Orientation != expectedOrientation)
                throw new ArgumentException($"Expected to read a {expectedOrientation} table");

            // read the columns
            _columns = new Column[ColumnCount];
            _reader.GetBlock(_header.InfoOffset, _header.InfoSizeBytes).Result.Cast<byte, Column>().CopyTo(_columns);

            // get column types
            ColumnTypes = new BrightDataType[ColumnCount];
            for(var i = 0; i < ColumnCount; i++)
                ColumnTypes[i] = _columns[i].DataType;

            _strings            = new(() => GetBlock(_header.StringOffset, _header.StringSizeBytes));
            _tensors            = new(() => GetBlock(_header.TensorOffset, _header.TensorSizeBytes));
            _binaryData         = new(() => GetBlock(_header.BinaryDataOffset, _header.BinaryDataSizeBytes));
            _indexLists         = new(() => GetBlock(_header.IndexOffset, _header.IndexSizeBytes));
            _weightedIndexLists = new(() => GetBlock(_header.WeightedIndexOffset, _header.WeightedIndexSizeBytes));
            _metaData           = new(() => GetBlock(_header.MetaDataOffset, _header.MetaDataSizeBytes));
        }

        public uint RowCount => _header.RowCount;
        public uint ColumnCount => _header.ColumnCount;
        public DataTableOrientation Orientation => _header.Orientation;
        public BrightDataType[] ColumnTypes { get; }

        ~TableBase()
        {
            InternalDispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            
        }

        void InternalDispose()
        {
            _reader.Dispose();
        }

        Task<ReadOnlyMemory<byte>> GetBlock(uint offset, uint size)
        {
            if (size == 0)
                return Task.FromResult(ReadOnlyMemory<byte>.Empty);
            return _reader.GetBlock(offset, size);
        }
    }
}
