using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Buffer;

namespace BrightData.DataTable2
{
    internal class ColumnSegment<CT, T> : IDataTableSegment<T>, ICanEnumerateWithSize<T>, ICanEnumerateDisposable
        where CT : unmanaged
        where T: notnull
    {
        readonly SequentialColumnReader<CT, T> _reader;

        public ColumnSegment(
            BrightDataContext context,
            BrightDataType dataType,
            uint size,
            SequentialColumnReader<CT, T> reader, 
            IMetaData metaData)
        {
            _reader = reader;
            SingleType = dataType;
            MetaData = metaData;
            Context = context;
            Size = size;
        }

        public IEnumerable<T> EnumerateTyped() => _reader.EnumerateTyped();
        public IMetaData MetaData { get; }
        public void WriteTo(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public BrightDataType SingleType { get; }
        public IEnumerable<object> Enumerate()
        {
            foreach (var item in EnumerateTyped())
                yield return item;
        }

        public uint Size { get; }
        public BrightDataContext Context { get; }
    }
}
