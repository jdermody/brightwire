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
        readonly uint                _inMemoryBufferSize;
        readonly ushort              _maxUniqueItemCount;
        readonly IProvideTempStreams _tempStreams;
        readonly List<IHybridBuffer> _columns = new();

        public BrightDataTableBuilder(
            BrightDataContext context, 
            uint inMemoryBufferSize = 32768 * 1024, 
            ushort maxUniqueItemCount = 32768)
        {
            Context            = context;
            _inMemoryBufferSize = inMemoryBufferSize;
            _maxUniqueItemCount = maxUniqueItemCount;
            _tempStreams        = context.CreateTempStreamProvider();
        }

        public MetaData TableMetaData { get; } = new();
        public BrightDataContext Context { get; }

        public void Dispose()
        {
            _tempStreams.Dispose();
        }

        internal static string DefaultColumnName(string? name, int numColumns)
        {
            return String.IsNullOrWhiteSpace(name) ? $"Column {numColumns + 1}" : name;
        }

        public IHybridBufferWithMetaData AddColumn(BrightDataType type, string? name = null)
        {
            var columnMetaData = new MetaData();
            columnMetaData.Set(Consts.Name, DefaultColumnName(name, _columns.Count));
            columnMetaData.Set(Consts.Type, type);
            columnMetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            if (type.IsNumeric())
                columnMetaData.Set(Consts.IsNumeric, true);

            return AddColumn(type, columnMetaData);
        }

        public IHybridBufferWithMetaData AddColumn(BrightDataType type, MetaData metaData)
        {
            var buffer = type.GetHybridBufferWithMetaData(new MetaData(metaData, Consts.StandardMetaData), Context, _tempStreams, _inMemoryBufferSize, _maxUniqueItemCount);
            buffer.MetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            _columns.Add(buffer);
            return buffer;
        }

        public void CopyColumnsFrom(BrightDataTable table, params uint[] columnIndices)
        {
            var columnSet = new HashSet<uint>(table.AllOrSpecifiedColumnIndices(columnIndices));
            var columnTypes = table.ColumnTypes.Zip(table.ColumnMetaData, (t, m) => (Type: t, MetaData: m))
                .Select((c, i) => (Column: c, Index: (uint) i));

            var wantedColumnTypes = columnTypes
                .Where(c => columnSet.Contains(c.Index))
                .Select(c => c.Column);

            foreach (var column in wantedColumnTypes)
                AddColumn(column.Type, column.MetaData);
        }

        public IHybridBufferWithMetaData<T> AddColumn<T>(string? name = null)
            where T : notnull
        {
            var type = typeof(T).GetBrightDataType();
            return (IHybridBufferWithMetaData<T>)AddColumn(type, name);
        }

        public void AddRow(params object[] items)
        {
            foreach (var (buffer, value) in _columns.Zip(items))
                buffer.AddObject(value);
        }

        public void WriteTo(Stream stream)
        {
            var writer = new BrightDataTableWriter(Context, _tempStreams, stream, _inMemoryBufferSize, _maxUniqueItemCount);
            writer.Write(
                TableMetaData,
                _columns.Cast<ISingleTypeTableSegment>().ToArray()
            );
        }

        public IHybridBufferWithMetaData<IVectorInfo> AddFixedSizeVectorColumn(uint size, string name)
        {
            // TODO: add constraint
            return AddColumn<IVectorInfo>(name);
        }

        public IHybridBufferWithMetaData<IMatrixInfo> AddFixedSizeMatrixColumn(uint rows, uint columns, string name)
        {
            // TODO: add constraint
            return AddColumn<IMatrixInfo>(name);
        }

        public IHybridBufferWithMetaData<ITensor3DInfo> AddFixedSize3DTensorColumn(uint depth, uint rows, uint columns, string name)
        {
            // TODO: add constraint
            return AddColumn<ITensor3DInfo>(name);
        }

        public IHybridBufferWithMetaData<ITensor4DInfo> AddFixedSize4DTensorColumn(uint count, uint depth, uint rows, uint columns, string name)
        {
            // TODO: add constraint
            return AddColumn<ITensor4DInfo>(name);
        }
    }
}
