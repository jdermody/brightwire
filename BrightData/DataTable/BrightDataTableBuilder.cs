using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightData.DataTable
{
    /// <summary>
    /// Builds data tables
    /// </summary>
    public class BrightDataTableBuilder : IDisposable, IHaveBrightDataContext
    {
        readonly uint                   _inMemoryBufferSize;
        readonly ushort                 _maxUniqueItemCount;
        readonly IProvideTempStreams    _tempStreams;
        readonly List<ICompositeBuffer> _columns = new();

        /// <summary>
        /// Creates a data table builder
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="inMemoryBufferSize">Size of in memory buffers</param>
        /// <param name="maxUniqueItemCount">Maximum number of unique items for dictionary based columns</param>
        public BrightDataTableBuilder(
            BrightDataContext context, 
            uint inMemoryBufferSize = Consts.DefaultInMemoryBufferSize, 
            ushort maxUniqueItemCount = Consts.DefaultMaxDistinctCount)
        {
            Context             = context;
            _inMemoryBufferSize = inMemoryBufferSize;
            _maxUniqueItemCount = maxUniqueItemCount;
            _tempStreams        = context.CreateTempStreamProvider();
        }

        /// <summary>
        /// Table level meta data
        /// </summary>
        public MetaData TableMetaData { get; } = new();

        /// <inheritdoc />
        public BrightDataContext Context { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _tempStreams.Dispose();
        }

        internal static string DefaultColumnName(string? name, int numColumns)
        {
            return String.IsNullOrWhiteSpace(name) ? $"Column {numColumns + 1}" : name;
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="type">New column type</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        public ICompositeBufferWithMetaData AddColumn(BrightDataType type, string? name = null)
        {
            var columnMetaData = new MetaData();
            columnMetaData.Set(Consts.Name, DefaultColumnName(name, _columns.Count));
            columnMetaData.Set(Consts.Type, type);
            columnMetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            if (type.IsNumeric())
                columnMetaData.Set(Consts.IsNumeric, true);

            return AddColumn(type, columnMetaData);
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="type">New column type</param>
        /// <param name="metaData">Column meta data</param>
        /// <returns></returns>
        public ICompositeBufferWithMetaData AddColumn(BrightDataType type, MetaData metaData)
        {
            var buffer = type.GetCompositeBufferWithMetaData(new MetaData(metaData, Consts.StandardMetaData), Context, _tempStreams, _inMemoryBufferSize, _maxUniqueItemCount);
            buffer.MetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            _columns.Add(buffer);
            return buffer;
        }

        /// <summary>
        /// Copies existing column definitions from another table
        /// </summary>
        /// <param name="table">Other table</param>
        /// <param name="columnIndices">Indices of column definitions to copy</param>
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

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        public ICompositeBufferWithMetaData<T> AddColumn<T>(string? name = null)
            where T : notnull
        {
            var type = typeof(T).GetBrightDataType();
            return (ICompositeBufferWithMetaData<T>)AddColumn(type, name);
        }

        /// <summary>
        /// Adds a row to the table
        /// </summary>
        /// <param name="items"></param>
        public void AddRow(params object[] items)
        {
            for(int i = 0, len = items.Length; i < len; i++)
                _columns[i].AddObject(items[i]);
        }

        /// <summary>
        /// Writes the data table to a stream
        /// </summary>
        /// <param name="stream"></param>
        public void WriteTo(Stream stream)
        {
            var writer = new BrightDataTableWriter(Context, _tempStreams, stream, _inMemoryBufferSize, _maxUniqueItemCount);
            writer.Write(
                TableMetaData,
                _columns.Cast<ITableSegment>().ToArray()
            );
        }

        /// <summary>
        /// Adds a fixed size vector column
        /// </summary>
        /// <param name="size">Size of the vector</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        public ICompositeBufferWithMetaData<IVectorData> AddFixedSizeVectorColumn(uint size, string? name)
        {
            var ret = AddColumn<IVectorData>(name);
            ret.ConstraintValidator = vector => vector.Size == size;
            return ret;
        }

        /// <summary>
        /// Adds a fixed size matrix column
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        public ICompositeBufferWithMetaData<IMatrixData> AddFixedSizeMatrixColumn(uint rows, uint columns, string? name)
        {
            var ret = AddColumn<IMatrixData>(name);
            ret.ConstraintValidator = matrix => matrix.RowCount == rows && matrix.ColumnCount == columns;
            return ret;
        }

        /// <summary>
        /// Adds a fixed size 3D tensor column 
        /// </summary>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        public ICompositeBufferWithMetaData<ITensor3DData> AddFixedSize3DTensorColumn(uint depth, uint rows, uint columns, string? name)
        {
            var ret = AddColumn<ITensor3DData>(name);
            ret.ConstraintValidator = tensor => tensor.RowCount == rows && tensor.ColumnCount == columns && tensor.Depth == depth;
            return ret;
        }

        /// <summary>
        /// Adds a fixed size 4D tensor column
        /// </summary>
        /// <param name="count">Number of 3D tensors</param>
        /// <param name="depth">Number of matrices</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="name">New column name</param>
        /// <returns></returns>
        public ICompositeBufferWithMetaData<ITensor4DData> AddFixedSize4DTensorColumn(uint count, uint depth, uint rows, uint columns, string? name)
        {
            var ret = AddColumn<ITensor4DData>(name);
            ret.ConstraintValidator = tensor => tensor.RowCount == rows && tensor.ColumnCount == columns && tensor.Depth == depth && tensor.Count == count;
            return ret;
        }
    }
}
