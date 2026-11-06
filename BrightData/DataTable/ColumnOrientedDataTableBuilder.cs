using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Buffer.Operations;
using BrightData.DataTable.Meta;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Types;

namespace BrightData.DataTable
{
    /// <summary>
    /// Builds a data table dynamically
    /// </summary>
    internal class ColumnOrientedDataTableBuilder(
        BrightDataContext context,
        IProvideByteBlocks? tempData = null,
        int blockSize = Consts.DefaultInitialBlockSize,
        int maxBlockSize = Consts.DefaultMaxBlockSize,
        uint? maxInMemoryBlocks = Consts.DefaultMaxBlocksInMemory,
        uint? maxDistinctItems = null)
        : IBuildDataTables
    {
        readonly List<ICompositeBuffer> _columns = [];
        readonly uint? _defaultMaxDistinctItems = maxDistinctItems;

        public MetaData TableMetaData { get; } = new();
        public MetaData[] ColumnMetaData => [.._columns.Select(x => x.MetaData)];
        public uint RowCount { get; private set; }
        public uint ColumnCount => (uint)_columns.Count;
        public BrightDataContext Context { get; } = context;

        internal static string DefaultColumnName(string? name, int numColumns)
        {
            return String.IsNullOrWhiteSpace(name) ? $"Column {numColumns + 1}" : name;
        }

        public ICompositeBuffer CreateColumn(BrightDataType type, string? name = null)
        {
            return CreateColumn(type, name, null);
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="type">New column type</param>
        /// <param name="name">New column name</param>
        /// <param name="maxDistinctItems">Enables distinct item tracking (eg string pools) up to this limit, overrides the builder-wide setting if specified</param>
        /// <returns></returns>
        public ICompositeBuffer CreateColumn(BrightDataType type, string? name, uint? maxDistinctItems)
        {
            var columnMetaData = new MetaData();
            columnMetaData.Set(Consts.Name, DefaultColumnName(name, _columns.Count));
            columnMetaData.Set(Consts.Type, type);
            columnMetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            if (type.IsNumeric())
                columnMetaData.Set(Consts.IsNumeric, true);

            return CreateColumn(type, columnMetaData, maxDistinctItems);
        }

        /// <summary>
        /// Adds a new column, inheriting the provided meta data.
        /// </summary>
        /// <param name="type">New column type</param>
        /// <param name="metaData">Column meta data to inherit</param>
        /// <returns></returns>
        public ICompositeBuffer CreateColumn(BrightDataType type, MetaData metaData)
        {
            return CreateColumn(type, metaData, null);
        }

        /// <summary>
        /// Adds a new column, inheriting the provided meta data.
        /// </summary>
        /// <remarks>
        /// The provided meta data is copied wholesale into the new column, so any pre-existing keys
        /// (column flags, analysis stats, normalisation models, etc) are carried over unexamined.
        /// Only <c>Consts.ColumnIndex</c> is reassigned to the new column's position. Callers
        /// passing meta data from another table (eg via CreateColumnsFrom) should be aware that
        /// index-referential keys such as <c>Consts.VectorisationSourceColumnIndex</c> refer to
        /// the original table's layout, as do analysis stats computed on the original data.
        /// </remarks>
        /// <param name="type">New column type</param>
        /// <param name="metaData">Column meta data to inherit</param>
        /// <param name="maxDistinctItems">Enables distinct item tracking (eg string pools) up to this limit, overrides the builder-wide setting if specified</param>
        /// <returns></returns>
        public ICompositeBuffer CreateColumn(BrightDataType type, MetaData metaData, uint? maxDistinctItems)
        {
            // distinct tracking is only enabled when explicitly requested (per column or builder-wide),
            // it allows the writer to deduplicate repeated items (eg string pools)
            var buffer = type.CreateCompositeBuffer(tempData, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems ?? _defaultMaxDistinctItems);
            // the caller's meta data is intentionally inherited wholesale - projection/split/concat
            // operations rely on the new column keeping its flags (IsTarget, IsCategorical, ...),
            // vectorisation wiring and analysis state; only ColumnIndex is reassigned below
            metaData.CopyTo(buffer.MetaData);
            buffer.MetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            _columns.Add(buffer);
            return buffer;
        }

        /// <summary>
        /// Copies column definitions (including their meta data) from another table.
        /// </summary>
        /// <remarks>
        /// Each new column inherits the source column's meta data unexamined (name, type, flags,
        /// analysis stats, normalisation models) - this is intentional, projection/split/concat
        /// operations depend on it. Only <c>Consts.ColumnIndex</c> is reassigned to the new
        /// position. Note: index-referential meta data such as <c>Consts.VectorisationSourceColumnIndex</c>
        /// still refers to the source table's layout.
        /// </remarks>
        /// <param name="table">Other table</param>
        /// <param name="columnIndices">Indices of column definitions to copy (all if empty)</param>
        /// <returns></returns>
        public ICompositeBuffer[] CreateColumnsFrom(IDataTable table, params uint[] columnIndices)
        {
            var columnSet = new HashSet<uint>(table.AllOrSpecifiedColumnIndices(false, columnIndices));
            var columnTypes = table.ColumnTypes.Zip(table.ColumnMetaData, (t, m) => (Type: t, MetaData: m))
                .Select((c, i) => (Column: c, Index: (uint) i));

            var wantedColumnTypes = columnTypes
                .Where(c => columnSet.Contains(c.Index))
                .Select(c => c.Column)
                .ToList()
            ;

            var index = 0;
            var ret = new ICompositeBuffer[wantedColumnTypes.Count];
            foreach (var column in wantedColumnTypes)
                ret[index++] = CreateColumn(column.Type, column.MetaData);
            return ret;
        }

        /// <summary>
        /// Creates a new empty column based on the type and meta data of an existing buffer.
        /// </summary>
        /// <remarks>
        /// The existing buffer's data is NOT copied - a new empty column is created with the
        /// same type, inheriting the buffer's meta data (only <c>Consts.ColumnIndex</c> is
        /// reassigned). If the builder was created with a default <c>maxDistinctItems</c>
        /// (eg via CreateTableBuilder(maxDistinctItems)), the new column will track distinct
        /// items as rows are appended to it.
        /// </remarks>
        /// <param name="buffer">Buffer providing the column type and meta data</param>
        /// <returns></returns>
        public ICompositeBuffer CreateEmptyColumnBasedOn(IReadOnlyBufferWithMetaData buffer)
        {
            return CreateColumn(buffer.DataType.GetBrightDataType(), buffer.MetaData);
        }

        /// <summary>
        /// Copies column definitions (including their meta data) from buffers.
        /// </summary>
        /// <remarks>
        /// Each new column inherits the buffer's meta data unexamined - only
        /// <c>Consts.ColumnIndex</c> is reassigned to the new position.
        /// </remarks>
        /// <param name="buffers">Buffers to copy columns from</param>
        /// <returns></returns>
        public ICompositeBuffer[] CreateColumnsFrom(params IReadOnlyBufferWithMetaData[] buffers)
        {
            return [.. buffers.Select(x => CreateColumn(x.DataType.GetBrightDataType(), x.MetaData))];
        }

        /// <summary>
        /// Copies column definitions (including their meta data) from buffers.
        /// </summary>
        /// <remarks>
        /// Each new column inherits the buffer's meta data unexamined - only
        /// <c>Consts.ColumnIndex</c> is reassigned to the new position.
        /// </remarks>
        /// <param name="buffers">Buffers to copy columns from</param>
        /// <returns></returns>
        public ICompositeBuffer[] CreateColumnsFrom(IEnumerable<IReadOnlyBufferWithMetaData> buffers)
        {
            return [.. buffers.Select(x => CreateColumn(x.DataType.GetBrightDataType(), x.MetaData))];
        }

        public ICompositeBuffer<T> CreateColumn<T>(string? name = null)
            where T : notnull
        {
            return CreateColumn<T>(name, null);
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <typeparam name="T">Type of the column</typeparam>
        /// <param name="name">New column name</param>
        /// <param name="maxDistinctItems">Enables distinct item tracking (eg string pools) up to this limit, overrides the builder-wide setting if specified</param>
        /// <returns></returns>
        public ICompositeBuffer<T> CreateColumn<T>(string? name, uint? maxDistinctItems)
            where T : notnull
        {
            var type = typeof(T).GetBrightDataType();
            return (ICompositeBuffer<T>)CreateColumn(type, name, maxDistinctItems);
        }

        public void AddRow(params object[] items)
        {
            for(int i = 0, len = items.Length; i < len; i++)
                _columns[i].AppendObject(items[i]);
            ++RowCount;
        }

        public async Task AddRows(IReadOnlyList<IReadOnlyBuffer> buffers, CancellationToken ct = default)
        {
            var copy = new ManyToManyCopy(buffers, _columns);
            await copy.Execute(null, null, ct);
            RowCount += copy.CopiedCount;
        }

        public async Task AddRows(IReadOnlyList<IReadOnlyBufferWithMetaData> buffers, CancellationToken ct = default)
        {
            var copy = new ManyToManyCopy(buffers, _columns);
            await copy.Execute(null, null, ct);
            RowCount += copy.CopiedCount;
        }

        public Task WriteTo(Stream stream)
        {
            var writer = new ColumnOrientedDataTableWriter(tempData, blockSize, maxBlockSize, maxInMemoryBlocks);
            return writer.Write(
                TableMetaData,
                [.._columns],
                stream
            );
        }

        public ICompositeBuffer<ReadOnlyVector<float>> CreateFixedSizeVectorColumn(uint size, string? name)
        {
            var ret = CreateColumn<ReadOnlyVector<float>>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyVector<float>>((in tensor) => tensor.Size == size);
            return ret;
        }

        
        public ICompositeBuffer<ReadOnlyMatrix<float>> CreateFixedSizeMatrixColumn(uint rows, uint columns, string? name)
        {
            var ret = CreateColumn<ReadOnlyMatrix<float>>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyMatrix<float>>((in tensor) => 
                tensor.RowCount == rows 
                && tensor.ColumnCount == columns 
            );
            return ret;
        }

        public ICompositeBuffer<ReadOnlyTensor3D<float>> CreateFixedSize3DTensorColumn(uint depth, uint rows, uint columns, string? name)
        {
            var ret = CreateColumn<ReadOnlyTensor3D<float>>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyTensor3D<float>>((in tensor) => 
                tensor.RowCount == rows 
                && tensor.ColumnCount == columns 
                && tensor.Depth == depth
            );
            return ret;
        }

        public ICompositeBuffer<ReadOnlyTensor4D<float>> CreateFixedSize4DTensorColumn(uint count, uint depth, uint rows, uint columns, string? name)
        {
            var ret = CreateColumn<ReadOnlyTensor4D<float>>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyTensor4D<float>>((in tensor) => 
                tensor.RowCount == rows 
                && tensor.ColumnCount == columns 
                && tensor.Depth == depth 
                && tensor.Count == count
            );
            return ret;
        }
    }
}
