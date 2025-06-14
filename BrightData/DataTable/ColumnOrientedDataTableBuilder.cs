﻿using System;
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
        uint? maxInMemoryBlocks = Consts.DefaultMaxBlocksInMemory)
        : IBuildDataTables
    {
        readonly List<ICompositeBuffer> _columns = [];

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
            var columnMetaData = new MetaData();
            columnMetaData.Set(Consts.Name, DefaultColumnName(name, _columns.Count));
            columnMetaData.Set(Consts.Type, type);
            columnMetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            if (type.IsNumeric())
                columnMetaData.Set(Consts.IsNumeric, true);

            return CreateColumn(type, columnMetaData);
        }

        public ICompositeBuffer CreateColumn(BrightDataType type, MetaData metaData)
        {
            var buffer = type.CreateCompositeBuffer(tempData, blockSize, maxBlockSize, maxInMemoryBlocks);
            metaData.CopyTo(buffer.MetaData);
            buffer.MetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            _columns.Add(buffer);
            return buffer;
        }

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

        public ICompositeBuffer CreateColumn(IReadOnlyBufferWithMetaData buffer)
        {
            return CreateColumn(buffer.DataType.GetBrightDataType(), buffer.MetaData);
        }

        public ICompositeBuffer[] CreateColumnsFrom(params IReadOnlyBufferWithMetaData[] buffers)
        {
            return [.. buffers.Select(x => CreateColumn(x.DataType.GetBrightDataType(), x.MetaData))];
        }

        public ICompositeBuffer[] CreateColumnsFrom(IEnumerable<IReadOnlyBufferWithMetaData> buffers)
        {
            return [.. buffers.Select(x => CreateColumn(x.DataType.GetBrightDataType(), x.MetaData))];
        }

        public ICompositeBuffer<T> CreateColumn<T>(string? name = null)
            where T : notnull
        {
            var type = typeof(T).GetBrightDataType();
            return (ICompositeBuffer<T>)CreateColumn(type, name);
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
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyVector<float>>((in ReadOnlyVector<float> tensor) => tensor.Size == size);
            return ret;
        }

        
        public ICompositeBuffer<ReadOnlyMatrix<float>> CreateFixedSizeMatrixColumn(uint rows, uint columns, string? name)
        {
            var ret = CreateColumn<ReadOnlyMatrix<float>>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyMatrix<float>>((in ReadOnlyMatrix<float> tensor) => 
                tensor.RowCount == rows 
                && tensor.ColumnCount == columns 
            );
            return ret;
        }

        public ICompositeBuffer<ReadOnlyTensor3D<float>> CreateFixedSize3DTensorColumn(uint depth, uint rows, uint columns, string? name)
        {
            var ret = CreateColumn<ReadOnlyTensor3D<float>>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyTensor3D<float>>((in ReadOnlyTensor3D<float> tensor) => 
                tensor.RowCount == rows 
                && tensor.ColumnCount == columns 
                && tensor.Depth == depth
            );
            return ret;
        }

        public ICompositeBuffer<ReadOnlyTensor4D<float>> CreateFixedSize4DTensorColumn(uint count, uint depth, uint rows, uint columns, string? name)
        {
            var ret = CreateColumn<ReadOnlyTensor4D<float>>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyTensor4D<float>>((in ReadOnlyTensor4D<float> tensor) => 
                tensor.RowCount == rows 
                && tensor.ColumnCount == columns 
                && tensor.Depth == depth 
                && tensor.Count == count
            );
            return ret;
        }
    }
}
