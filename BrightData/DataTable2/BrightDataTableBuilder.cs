using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.DataTable;
using BrightData.Helper;

namespace BrightData.DataTable2
{
    public class BrightDataTableBuilder : IDisposable
    {
        readonly BrightDataContext   _context;
        readonly uint                _inMemoryBufferSize;
        readonly ushort              _maxUniqueItemCount;
        readonly IProvideTempStreams _tempStreams;
        readonly List<IHybridBuffer> _columns = new();

        public BrightDataTableBuilder(
            BrightDataContext context, 
            uint inMemoryBufferSize = 32768 * 1024, 
            ushort maxUniqueItemCount = 32768)
        {
            _context            = context;
            _inMemoryBufferSize = inMemoryBufferSize;
            _maxUniqueItemCount = maxUniqueItemCount;
            _tempStreams        = new TempStreamManager(context.Get<string>(Consts.BaseTempPath));
        }

        public MetaData TableMetaData { get; } = new();

        public void Dispose()
        {
            _tempStreams.Dispose();
        }

        public IHybridBuffer AddColumn(BrightDataType type, string? name = null)
        {
            var columnMetaData = new MetaData();
            columnMetaData.Set(Consts.Name, DataTableBase.DefaultColumnName(name, _columns.Count));
            columnMetaData.Set(Consts.Type, type);
            columnMetaData.Set(Consts.Index, (uint)_columns.Count);
            if (type.IsNumeric())
                columnMetaData.Set(Consts.IsNumeric, true);

            var buffer = columnMetaData.GetGrowableSegment(type, _context, _tempStreams, _inMemoryBufferSize, _maxUniqueItemCount);
            _columns.Add(buffer);
            return buffer;
        }

        public IHybridBuffer<T> AddColumn<T>(string? name = null)
            where T : notnull
        {
            var type = typeof(T).GetBrightDataType();
            return (IHybridBuffer<T>)AddColumn(type, name);
        }

        public void AddRow(params object[] items)
        {
            foreach (var (buffer, value) in _columns.Zip(items))
                buffer.AddObject(value);
        }

        public void WriteTo(Stream stream)
        {
            var writer = new DataTableWriter(_context, _tempStreams, stream, _inMemoryBufferSize, _maxUniqueItemCount);
            writer.Write(
                TableMetaData,
                _columns.Cast<ISingleTypeTableSegment>().ToArray()
            );
        }
    }
}
