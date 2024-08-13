using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BrightData.DataTable.Rows;

namespace BrightData
{
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1>(IDataTable Table, uint RowIndex, T1 C1) : TableRowBase(Table, RowIndex)
        where T1: notnull
    {
        /// <inheritdoc />
        public override uint Size => 1;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            _ => throw new Exception("Column index was out of range")
        };
    }
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1, T2>(IDataTable Table, uint RowIndex, T1 C1, T2 C2) : TableRowBase(Table, RowIndex)
        where T1: notnull where T2: notnull
    {
        /// <inheritdoc />
        public override uint Size => 2;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            1 => C2,
            _ => throw new Exception("Column index was out of range")
        };
    }
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1, T2, T3>(IDataTable Table, uint RowIndex, T1 C1, T2 C2, T3 C3) : TableRowBase(Table, RowIndex)
        where T1: notnull where T2: notnull where T3: notnull
    {
        /// <inheritdoc />
        public override uint Size => 3;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            1 => C2,
            2 => C3,
            _ => throw new Exception("Column index was out of range")
        };
    }
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1, T2, T3, T4>(IDataTable Table, uint RowIndex, T1 C1, T2 C2, T3 C3, T4 C4) : TableRowBase(Table, RowIndex)
        where T1: notnull where T2: notnull where T3: notnull where T4: notnull
    {
        /// <inheritdoc />
        public override uint Size => 4;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            1 => C2,
            2 => C3,
            3 => C4,
            _ => throw new Exception("Column index was out of range")
        };
    }
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1, T2, T3, T4, T5>(IDataTable Table, uint RowIndex, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5) : TableRowBase(Table, RowIndex)
        where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull
    {
        /// <inheritdoc />
        public override uint Size => 5;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            1 => C2,
            2 => C3,
            3 => C4,
            4 => C5,
            _ => throw new Exception("Column index was out of range")
        };
    }
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1, T2, T3, T4, T5, T6>(IDataTable Table, uint RowIndex, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6) : TableRowBase(Table, RowIndex)
        where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull
    {
        /// <inheritdoc />
        public override uint Size => 6;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            1 => C2,
            2 => C3,
            3 => C4,
            4 => C5,
            5 => C6,
            _ => throw new Exception("Column index was out of range")
        };
    }
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1, T2, T3, T4, T5, T6, T7>(IDataTable Table, uint RowIndex, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7) : TableRowBase(Table, RowIndex)
        where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull
    {
        /// <inheritdoc />
        public override uint Size => 7;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            1 => C2,
            2 => C3,
            3 => C4,
            4 => C5,
            5 => C6,
            6 => C7,
            _ => throw new Exception("Column index was out of range")
        };
    }
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(IDataTable Table, uint RowIndex, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8) : TableRowBase(Table, RowIndex)
        where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull
    {
        /// <inheritdoc />
        public override uint Size => 8;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            1 => C2,
            2 => C3,
            3 => C4,
            4 => C5,
            5 => C6,
            6 => C7,
            7 => C8,
            _ => throw new Exception("Column index was out of range")
        };
    }
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IDataTable Table, uint RowIndex, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9) : TableRowBase(Table, RowIndex)
        where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull
    {
        /// <inheritdoc />
        public override uint Size => 9;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            1 => C2,
            2 => C3,
            3 => C4,
            4 => C5,
            5 => C6,
            6 => C7,
            7 => C8,
            8 => C9,
            _ => throw new Exception("Column index was out of range")
        };
    }
    /// <summary>
    /// Typed data table row
    /// </summary>
    public record TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IDataTable Table, uint RowIndex, T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10) : TableRowBase(Table, RowIndex)
        where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull where T10: notnull
    {
        /// <inheritdoc />
        public override uint Size => 10;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => columnIndex switch {
            0 => C1,
            1 => C2,
            2 => C3,
            3 => C4,
            4 => C5,
            5 => C6,
            6 => C7,
            7 => C8,
            8 => C9,
            9 => C10,
            _ => throw new Exception("Column index was out of range")
        };
    }
}

namespace BrightData
{
    public partial class ExtensionMethods
    {      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1>> Enumerate<T1>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
        {
            const int size = 1;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1>(dataTable, rowIndex++, e1.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1> : IReadOnlyBuffer<TableRow<T1>> where T1: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1>);
                _input1 = input1;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1);
                var block1 = b1.Result;
                var ret = new TableRow<T1>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, Span<TableRow<T1>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1>(_dataTable, firstIndex + (uint)i, span1[i]);
            }

            public async IAsyncEnumerable<TableRow<T1>> EnumerateAllTyped()
            {
                const int size = 1;
                await using var e1 = _input1.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1>(_dataTable, rowIndex++, e1.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                await Task.WhenAll(b1);
                var a1 = (T1[])b1.Result;
                var len = a1.Length;
                var ret = new TableRow<T1>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1>(_dataTable, i + offset, a1[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1>> GetRowsBuffer<T1>(
            this IDataTable dataTable,
            uint columnIndex1 = 0
        ) 
            where T1: notnull
        {
            return new RowReader<T1>(dataTable, dataTable.GetColumn<T1>(columnIndex1));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1>> GetRow<T1>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0
        )
            where T1: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var getTask1 = column1.GetItem(rowIndex);
            await Task.WhenAll(getTask1);
            return new TableRow<T1>(dataTable, rowIndex, getTask1.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1>[]> GetRows<T1>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            params uint[] rowIndices
        )
            where T1: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            return dataTable.CopyRows<TableRow<T1>>(column1, rowIndices, x => new TableRow<T1>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1> row) => {
                    var span1 = getTask1.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex]);
                }); 
            });
        }
      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1, T2>> Enumerate<T1, T2>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
			where T2: notnull
        {
            const int size = 2;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            await using var e2 = dataTable.GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                currentTasks[1] = e2.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1, T2>(dataTable, rowIndex++, e1.Current, e2.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2> : IReadOnlyBuffer<TableRow<T1, T2>> where T1: notnull
			where T2: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                if (!BlockSizes.SequenceEqual(input2.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1, T2>);
                _input1 = input1;
                _input2 = input2;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1, T2>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2);
                var block1 = b1.Result;
                var block2 = b2.Result;
                if (block1.Length != block2.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block2:{block2.Length})");
                var ret = new TableRow<T1, T2>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, block2.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, Span<TableRow<T1, T2>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2>(_dataTable, firstIndex + (uint)i, span1[i], span2[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2>> EnumerateAllTyped()
            {
                const int size = 2;
                await using var e1 = _input1.GetAsyncEnumerator();
                await using var e2 = _input2.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    currentTasks[1] = e2.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1, T2>(_dataTable, rowIndex++, e1.Current, e2.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                var b2 = _input2.GetBlock(blockIndex);
                await Task.WhenAll(b1, b2);
                var a1 = (T1[])b1.Result;
                var a2 = (T2[])b2.Result;
                var len = a1.Length;
                if (len != a2.Length)
                    throw new Exception($"Expected blocks to have same size - block {2} had length {a2.Length} but expected {len}");
                var ret = new TableRow<T1, T2>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1, T2>(_dataTable, i + offset, a1[i], a2[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1, T2>> GetRowsBuffer<T1, T2>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1
        ) 
            where T1: notnull
			where T2: notnull
        {
            return new RowReader<T1, T2>(dataTable, dataTable.GetColumn<T1>(columnIndex1), dataTable.GetColumn<T2>(columnIndex2));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1, T2>> GetRow<T1, T2>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1
        )
            where T1: notnull
			where T2: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var getTask1 = column1.GetItem(rowIndex);
            var getTask2 = column2.GetItem(rowIndex);
            await Task.WhenAll(getTask1, getTask2);
            return new TableRow<T1, T2>(dataTable, rowIndex, getTask1.Result, getTask2.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1, T2>[]> GetRows<T1, T2>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            params uint[] rowIndices
        )
            where T1: notnull
			where T2: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            return dataTable.CopyRows<TableRow<T1, T2>>(column1, rowIndices, x => new TableRow<T1, T2>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                var getTask2 = column2.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1, getTask2);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1, T2> row) => {
                    var span1 = getTask1.Result.Span;
                    var span2 = getTask2.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex], span2[(int)relativeBlockIndex]);
                }); 
            });
        }
      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1, T2, T3>> Enumerate<T1, T2, T3>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
        {
            const int size = 3;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            await using var e2 = dataTable.GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            await using var e3 = dataTable.GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                currentTasks[1] = e2.MoveNextAsync();
                currentTasks[2] = e3.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1, T2, T3>(dataTable, rowIndex++, e1.Current, e2.Current, e3.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3> : IReadOnlyBuffer<TableRow<T1, T2, T3>> where T1: notnull
			where T2: notnull
			where T3: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                if (!BlockSizes.SequenceEqual(input2.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input3.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1, T2, T3>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1, T2, T3>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                var b3 = _input3.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3);
                var block1 = b1.Result;
                var block2 = b2.Result;
                var block3 = b3.Result;
                if (block1.Length != block2.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block2:{block2.Length})");
                if (block1.Length != block3.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block3:{block3.Length})");
                var ret = new TableRow<T1, T2, T3>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, block2.Span, block3.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, Span<TableRow<T1, T2, T3>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3>(_dataTable, firstIndex + (uint)i, span1[i], span2[i], span3[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3>> EnumerateAllTyped()
            {
                const int size = 3;
                await using var e1 = _input1.GetAsyncEnumerator();
                await using var e2 = _input2.GetAsyncEnumerator();
                await using var e3 = _input3.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    currentTasks[1] = e2.MoveNextAsync();
                    currentTasks[2] = e3.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1, T2, T3>(_dataTable, rowIndex++, e1.Current, e2.Current, e3.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                var b2 = _input2.GetBlock(blockIndex);
                var b3 = _input3.GetBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3);
                var a1 = (T1[])b1.Result;
                var a2 = (T2[])b2.Result;
                var a3 = (T3[])b3.Result;
                var len = a1.Length;
                if (len != a2.Length)
                    throw new Exception($"Expected blocks to have same size - block {2} had length {a2.Length} but expected {len}");
                if (len != a3.Length)
                    throw new Exception($"Expected blocks to have same size - block {3} had length {a3.Length} but expected {len}");
                var ret = new TableRow<T1, T2, T3>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1, T2, T3>(_dataTable, i + offset, a1[i], a2[i], a3[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1, T2, T3>> GetRowsBuffer<T1, T2, T3>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2
        ) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
        {
            return new RowReader<T1, T2, T3>(dataTable, dataTable.GetColumn<T1>(columnIndex1), dataTable.GetColumn<T2>(columnIndex2), dataTable.GetColumn<T3>(columnIndex3));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1, T2, T3>> GetRow<T1, T2, T3>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var getTask1 = column1.GetItem(rowIndex);
            var getTask2 = column2.GetItem(rowIndex);
            var getTask3 = column3.GetItem(rowIndex);
            await Task.WhenAll(getTask1, getTask2, getTask3);
            return new TableRow<T1, T2, T3>(dataTable, rowIndex, getTask1.Result, getTask2.Result, getTask3.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1, T2, T3>[]> GetRows<T1, T2, T3>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            params uint[] rowIndices
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            return dataTable.CopyRows<TableRow<T1, T2, T3>>(column1, rowIndices, x => new TableRow<T1, T2, T3>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                var getTask2 = column2.GetTypedBlock(blockIndex);
                var getTask3 = column3.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1, getTask2, getTask3);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1, T2, T3> row) => {
                    var span1 = getTask1.Result.Span;
                    var span2 = getTask2.Result.Span;
                    var span3 = getTask3.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex], span2[(int)relativeBlockIndex], span3[(int)relativeBlockIndex]);
                }); 
            });
        }
      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1, T2, T3, T4>> Enumerate<T1, T2, T3, T4>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
        {
            const int size = 4;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            await using var e2 = dataTable.GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            await using var e3 = dataTable.GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            await using var e4 = dataTable.GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                currentTasks[1] = e2.MoveNextAsync();
                currentTasks[2] = e3.MoveNextAsync();
                currentTasks[3] = e4.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1, T2, T3, T4>(dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4>> where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                if (!BlockSizes.SequenceEqual(input2.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input3.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input4.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1, T2, T3, T4>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1, T2, T3, T4>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                var b3 = _input3.GetTypedBlock(blockIndex);
                var b4 = _input4.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4);
                var block1 = b1.Result;
                var block2 = b2.Result;
                var block3 = b3.Result;
                var block4 = b4.Result;
                if (block1.Length != block2.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block2:{block2.Length})");
                if (block1.Length != block3.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block3:{block3.Length})");
                if (block1.Length != block4.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block4:{block4.Length})");
                var ret = new TableRow<T1, T2, T3, T4>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, block2.Span, block3.Span, block4.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, Span<TableRow<T1, T2, T3, T4>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4>(_dataTable, firstIndex + (uint)i, span1[i], span2[i], span3[i], span4[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4>> EnumerateAllTyped()
            {
                const int size = 4;
                await using var e1 = _input1.GetAsyncEnumerator();
                await using var e2 = _input2.GetAsyncEnumerator();
                await using var e3 = _input3.GetAsyncEnumerator();
                await using var e4 = _input4.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    currentTasks[1] = e2.MoveNextAsync();
                    currentTasks[2] = e3.MoveNextAsync();
                    currentTasks[3] = e4.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1, T2, T3, T4>(_dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                var b2 = _input2.GetBlock(blockIndex);
                var b3 = _input3.GetBlock(blockIndex);
                var b4 = _input4.GetBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4);
                var a1 = (T1[])b1.Result;
                var a2 = (T2[])b2.Result;
                var a3 = (T3[])b3.Result;
                var a4 = (T4[])b4.Result;
                var len = a1.Length;
                if (len != a2.Length)
                    throw new Exception($"Expected blocks to have same size - block {2} had length {a2.Length} but expected {len}");
                if (len != a3.Length)
                    throw new Exception($"Expected blocks to have same size - block {3} had length {a3.Length} but expected {len}");
                if (len != a4.Length)
                    throw new Exception($"Expected blocks to have same size - block {4} had length {a4.Length} but expected {len}");
                var ret = new TableRow<T1, T2, T3, T4>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1, T2, T3, T4>(_dataTable, i + offset, a1[i], a2[i], a3[i], a4[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1, T2, T3, T4>> GetRowsBuffer<T1, T2, T3, T4>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3
        ) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
        {
            return new RowReader<T1, T2, T3, T4>(dataTable, dataTable.GetColumn<T1>(columnIndex1), dataTable.GetColumn<T2>(columnIndex2), dataTable.GetColumn<T3>(columnIndex3), dataTable.GetColumn<T4>(columnIndex4));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1, T2, T3, T4>> GetRow<T1, T2, T3, T4>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var getTask1 = column1.GetItem(rowIndex);
            var getTask2 = column2.GetItem(rowIndex);
            var getTask3 = column3.GetItem(rowIndex);
            var getTask4 = column4.GetItem(rowIndex);
            await Task.WhenAll(getTask1, getTask2, getTask3, getTask4);
            return new TableRow<T1, T2, T3, T4>(dataTable, rowIndex, getTask1.Result, getTask2.Result, getTask3.Result, getTask4.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1, T2, T3, T4>[]> GetRows<T1, T2, T3, T4>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            params uint[] rowIndices
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            return dataTable.CopyRows<TableRow<T1, T2, T3, T4>>(column1, rowIndices, x => new TableRow<T1, T2, T3, T4>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                var getTask2 = column2.GetTypedBlock(blockIndex);
                var getTask3 = column3.GetTypedBlock(blockIndex);
                var getTask4 = column4.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1, getTask2, getTask3, getTask4);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1, T2, T3, T4> row) => {
                    var span1 = getTask1.Result.Span;
                    var span2 = getTask2.Result.Span;
                    var span3 = getTask3.Result.Span;
                    var span4 = getTask4.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex], span2[(int)relativeBlockIndex], span3[(int)relativeBlockIndex], span4[(int)relativeBlockIndex]);
                }); 
            });
        }
      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5>> Enumerate<T1, T2, T3, T4, T5>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
        {
            const int size = 5;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            await using var e2 = dataTable.GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            await using var e3 = dataTable.GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            await using var e4 = dataTable.GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            await using var e5 = dataTable.GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                currentTasks[1] = e2.MoveNextAsync();
                currentTasks[2] = e3.MoveNextAsync();
                currentTasks[3] = e4.MoveNextAsync();
                currentTasks[4] = e5.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1, T2, T3, T4, T5>(dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5>> where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                if (!BlockSizes.SequenceEqual(input2.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input3.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input4.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input5.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1, T2, T3, T4, T5>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
                _input5 = input5;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1, T2, T3, T4, T5>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                var b3 = _input3.GetTypedBlock(blockIndex);
                var b4 = _input4.GetTypedBlock(blockIndex);
                var b5 = _input5.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5);
                var block1 = b1.Result;
                var block2 = b2.Result;
                var block3 = b3.Result;
                var block4 = b4.Result;
                var block5 = b5.Result;
                if (block1.Length != block2.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block2:{block2.Length})");
                if (block1.Length != block3.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block3:{block3.Length})");
                if (block1.Length != block4.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block4:{block4.Length})");
                if (block1.Length != block5.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block5:{block5.Length})");
                var ret = new TableRow<T1, T2, T3, T4, T5>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, Span<TableRow<T1, T2, T3, T4, T5>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5>(_dataTable, firstIndex + (uint)i, span1[i], span2[i], span3[i], span4[i], span5[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5>> EnumerateAllTyped()
            {
                const int size = 5;
                await using var e1 = _input1.GetAsyncEnumerator();
                await using var e2 = _input2.GetAsyncEnumerator();
                await using var e3 = _input3.GetAsyncEnumerator();
                await using var e4 = _input4.GetAsyncEnumerator();
                await using var e5 = _input5.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    currentTasks[1] = e2.MoveNextAsync();
                    currentTasks[2] = e3.MoveNextAsync();
                    currentTasks[3] = e4.MoveNextAsync();
                    currentTasks[4] = e5.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1, T2, T3, T4, T5>(_dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                var b2 = _input2.GetBlock(blockIndex);
                var b3 = _input3.GetBlock(blockIndex);
                var b4 = _input4.GetBlock(blockIndex);
                var b5 = _input5.GetBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5);
                var a1 = (T1[])b1.Result;
                var a2 = (T2[])b2.Result;
                var a3 = (T3[])b3.Result;
                var a4 = (T4[])b4.Result;
                var a5 = (T5[])b5.Result;
                var len = a1.Length;
                if (len != a2.Length)
                    throw new Exception($"Expected blocks to have same size - block {2} had length {a2.Length} but expected {len}");
                if (len != a3.Length)
                    throw new Exception($"Expected blocks to have same size - block {3} had length {a3.Length} but expected {len}");
                if (len != a4.Length)
                    throw new Exception($"Expected blocks to have same size - block {4} had length {a4.Length} but expected {len}");
                if (len != a5.Length)
                    throw new Exception($"Expected blocks to have same size - block {5} had length {a5.Length} but expected {len}");
                var ret = new TableRow<T1, T2, T3, T4, T5>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1, T2, T3, T4, T5>(_dataTable, i + offset, a1[i], a2[i], a3[i], a4[i], a5[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5>> GetRowsBuffer<T1, T2, T3, T4, T5>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4
        ) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5>(dataTable, dataTable.GetColumn<T1>(columnIndex1), dataTable.GetColumn<T2>(columnIndex2), dataTable.GetColumn<T3>(columnIndex3), dataTable.GetColumn<T4>(columnIndex4), dataTable.GetColumn<T5>(columnIndex5));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1, T2, T3, T4, T5>> GetRow<T1, T2, T3, T4, T5>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var getTask1 = column1.GetItem(rowIndex);
            var getTask2 = column2.GetItem(rowIndex);
            var getTask3 = column3.GetItem(rowIndex);
            var getTask4 = column4.GetItem(rowIndex);
            var getTask5 = column5.GetItem(rowIndex);
            await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5);
            return new TableRow<T1, T2, T3, T4, T5>(dataTable, rowIndex, getTask1.Result, getTask2.Result, getTask3.Result, getTask4.Result, getTask5.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1, T2, T3, T4, T5>[]> GetRows<T1, T2, T3, T4, T5>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            params uint[] rowIndices
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            return dataTable.CopyRows<TableRow<T1, T2, T3, T4, T5>>(column1, rowIndices, x => new TableRow<T1, T2, T3, T4, T5>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                var getTask2 = column2.GetTypedBlock(blockIndex);
                var getTask3 = column3.GetTypedBlock(blockIndex);
                var getTask4 = column4.GetTypedBlock(blockIndex);
                var getTask5 = column5.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1, T2, T3, T4, T5> row) => {
                    var span1 = getTask1.Result.Span;
                    var span2 = getTask2.Result.Span;
                    var span3 = getTask3.Result.Span;
                    var span4 = getTask4.Result.Span;
                    var span5 = getTask5.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex], span2[(int)relativeBlockIndex], span3[(int)relativeBlockIndex], span4[(int)relativeBlockIndex], span5[(int)relativeBlockIndex]);
                }); 
            });
        }
      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6>> Enumerate<T1, T2, T3, T4, T5, T6>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
        {
            const int size = 6;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            await using var e2 = dataTable.GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            await using var e3 = dataTable.GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            await using var e4 = dataTable.GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            await using var e5 = dataTable.GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            await using var e6 = dataTable.GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                currentTasks[1] = e2.MoveNextAsync();
                currentTasks[2] = e3.MoveNextAsync();
                currentTasks[3] = e4.MoveNextAsync();
                currentTasks[4] = e5.MoveNextAsync();
                currentTasks[5] = e6.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1, T2, T3, T4, T5, T6>(dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6>> where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;
            readonly IReadOnlyBuffer<T6> _input6;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                if (!BlockSizes.SequenceEqual(input2.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input3.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input4.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input5.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input6.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1, T2, T3, T4, T5, T6>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
                _input5 = input5;
                _input6 = input6;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1, T2, T3, T4, T5, T6>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                var b3 = _input3.GetTypedBlock(blockIndex);
                var b4 = _input4.GetTypedBlock(blockIndex);
                var b5 = _input5.GetTypedBlock(blockIndex);
                var b6 = _input6.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6);
                var block1 = b1.Result;
                var block2 = b2.Result;
                var block3 = b3.Result;
                var block4 = b4.Result;
                var block5 = b5.Result;
                var block6 = b6.Result;
                if (block1.Length != block2.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block2:{block2.Length})");
                if (block1.Length != block3.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block3:{block3.Length})");
                if (block1.Length != block4.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block4:{block4.Length})");
                if (block1.Length != block5.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block5:{block5.Length})");
                if (block1.Length != block6.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block6:{block6.Length})");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, Span<TableRow<T1, T2, T3, T4, T5, T6>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6>(_dataTable, firstIndex + (uint)i, span1[i], span2[i], span3[i], span4[i], span5[i], span6[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6>> EnumerateAllTyped()
            {
                const int size = 6;
                await using var e1 = _input1.GetAsyncEnumerator();
                await using var e2 = _input2.GetAsyncEnumerator();
                await using var e3 = _input3.GetAsyncEnumerator();
                await using var e4 = _input4.GetAsyncEnumerator();
                await using var e5 = _input5.GetAsyncEnumerator();
                await using var e6 = _input6.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    currentTasks[1] = e2.MoveNextAsync();
                    currentTasks[2] = e3.MoveNextAsync();
                    currentTasks[3] = e4.MoveNextAsync();
                    currentTasks[4] = e5.MoveNextAsync();
                    currentTasks[5] = e6.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1, T2, T3, T4, T5, T6>(_dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                var b2 = _input2.GetBlock(blockIndex);
                var b3 = _input3.GetBlock(blockIndex);
                var b4 = _input4.GetBlock(blockIndex);
                var b5 = _input5.GetBlock(blockIndex);
                var b6 = _input6.GetBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6);
                var a1 = (T1[])b1.Result;
                var a2 = (T2[])b2.Result;
                var a3 = (T3[])b3.Result;
                var a4 = (T4[])b4.Result;
                var a5 = (T5[])b5.Result;
                var a6 = (T6[])b6.Result;
                var len = a1.Length;
                if (len != a2.Length)
                    throw new Exception($"Expected blocks to have same size - block {2} had length {a2.Length} but expected {len}");
                if (len != a3.Length)
                    throw new Exception($"Expected blocks to have same size - block {3} had length {a3.Length} but expected {len}");
                if (len != a4.Length)
                    throw new Exception($"Expected blocks to have same size - block {4} had length {a4.Length} but expected {len}");
                if (len != a5.Length)
                    throw new Exception($"Expected blocks to have same size - block {5} had length {a5.Length} but expected {len}");
                if (len != a6.Length)
                    throw new Exception($"Expected blocks to have same size - block {6} had length {a6.Length} but expected {len}");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1, T2, T3, T4, T5, T6>(_dataTable, i + offset, a1[i], a2[i], a3[i], a4[i], a5[i], a6[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6>> GetRowsBuffer<T1, T2, T3, T4, T5, T6>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5
        ) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6>(dataTable, dataTable.GetColumn<T1>(columnIndex1), dataTable.GetColumn<T2>(columnIndex2), dataTable.GetColumn<T3>(columnIndex3), dataTable.GetColumn<T4>(columnIndex4), dataTable.GetColumn<T5>(columnIndex5), dataTable.GetColumn<T6>(columnIndex6));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1, T2, T3, T4, T5, T6>> GetRow<T1, T2, T3, T4, T5, T6>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            var getTask1 = column1.GetItem(rowIndex);
            var getTask2 = column2.GetItem(rowIndex);
            var getTask3 = column3.GetItem(rowIndex);
            var getTask4 = column4.GetItem(rowIndex);
            var getTask5 = column5.GetItem(rowIndex);
            var getTask6 = column6.GetItem(rowIndex);
            await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6);
            return new TableRow<T1, T2, T3, T4, T5, T6>(dataTable, rowIndex, getTask1.Result, getTask2.Result, getTask3.Result, getTask4.Result, getTask5.Result, getTask6.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1, T2, T3, T4, T5, T6>[]> GetRows<T1, T2, T3, T4, T5, T6>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            params uint[] rowIndices
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            return dataTable.CopyRows<TableRow<T1, T2, T3, T4, T5, T6>>(column1, rowIndices, x => new TableRow<T1, T2, T3, T4, T5, T6>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                var getTask2 = column2.GetTypedBlock(blockIndex);
                var getTask3 = column3.GetTypedBlock(blockIndex);
                var getTask4 = column4.GetTypedBlock(blockIndex);
                var getTask5 = column5.GetTypedBlock(blockIndex);
                var getTask6 = column6.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1, T2, T3, T4, T5, T6> row) => {
                    var span1 = getTask1.Result.Span;
                    var span2 = getTask2.Result.Span;
                    var span3 = getTask3.Result.Span;
                    var span4 = getTask4.Result.Span;
                    var span5 = getTask5.Result.Span;
                    var span6 = getTask6.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex], span2[(int)relativeBlockIndex], span3[(int)relativeBlockIndex], span4[(int)relativeBlockIndex], span5[(int)relativeBlockIndex], span6[(int)relativeBlockIndex]);
                }); 
            });
        }
      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7>> Enumerate<T1, T2, T3, T4, T5, T6, T7>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
        {
            const int size = 7;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            await using var e2 = dataTable.GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            await using var e3 = dataTable.GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            await using var e4 = dataTable.GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            await using var e5 = dataTable.GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            await using var e6 = dataTable.GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            await using var e7 = dataTable.GetColumn<T7>(columnIndex7).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                currentTasks[1] = e2.MoveNextAsync();
                currentTasks[2] = e3.MoveNextAsync();
                currentTasks[3] = e4.MoveNextAsync();
                currentTasks[4] = e5.MoveNextAsync();
                currentTasks[5] = e6.MoveNextAsync();
                currentTasks[6] = e7.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1, T2, T3, T4, T5, T6, T7>(dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7>> where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;
            readonly IReadOnlyBuffer<T6> _input6;
            readonly IReadOnlyBuffer<T7> _input7;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                if (!BlockSizes.SequenceEqual(input2.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input3.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input4.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input5.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input6.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input7.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1, T2, T3, T4, T5, T6, T7>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
                _input5 = input5;
                _input6 = input6;
                _input7 = input7;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6, T7>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1, T2, T3, T4, T5, T6, T7>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                var b3 = _input3.GetTypedBlock(blockIndex);
                var b4 = _input4.GetTypedBlock(blockIndex);
                var b5 = _input5.GetTypedBlock(blockIndex);
                var b6 = _input6.GetTypedBlock(blockIndex);
                var b7 = _input7.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7);
                var block1 = b1.Result;
                var block2 = b2.Result;
                var block3 = b3.Result;
                var block4 = b4.Result;
                var block5 = b5.Result;
                var block6 = b6.Result;
                var block7 = b7.Result;
                if (block1.Length != block2.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block2:{block2.Length})");
                if (block1.Length != block3.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block3:{block3.Length})");
                if (block1.Length != block4.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block4:{block4.Length})");
                if (block1.Length != block5.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block5:{block5.Length})");
                if (block1.Length != block6.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block6:{block6.Length})");
                if (block1.Length != block7.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block7:{block7.Length})");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6, T7>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, Span<TableRow<T1, T2, T3, T4, T5, T6, T7>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7>(_dataTable, firstIndex + (uint)i, span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7>> EnumerateAllTyped()
            {
                const int size = 7;
                await using var e1 = _input1.GetAsyncEnumerator();
                await using var e2 = _input2.GetAsyncEnumerator();
                await using var e3 = _input3.GetAsyncEnumerator();
                await using var e4 = _input4.GetAsyncEnumerator();
                await using var e5 = _input5.GetAsyncEnumerator();
                await using var e6 = _input6.GetAsyncEnumerator();
                await using var e7 = _input7.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    currentTasks[1] = e2.MoveNextAsync();
                    currentTasks[2] = e3.MoveNextAsync();
                    currentTasks[3] = e4.MoveNextAsync();
                    currentTasks[4] = e5.MoveNextAsync();
                    currentTasks[5] = e6.MoveNextAsync();
                    currentTasks[6] = e7.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1, T2, T3, T4, T5, T6, T7>(_dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6, T7>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                var b2 = _input2.GetBlock(blockIndex);
                var b3 = _input3.GetBlock(blockIndex);
                var b4 = _input4.GetBlock(blockIndex);
                var b5 = _input5.GetBlock(blockIndex);
                var b6 = _input6.GetBlock(blockIndex);
                var b7 = _input7.GetBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7);
                var a1 = (T1[])b1.Result;
                var a2 = (T2[])b2.Result;
                var a3 = (T3[])b3.Result;
                var a4 = (T4[])b4.Result;
                var a5 = (T5[])b5.Result;
                var a6 = (T6[])b6.Result;
                var a7 = (T7[])b7.Result;
                var len = a1.Length;
                if (len != a2.Length)
                    throw new Exception($"Expected blocks to have same size - block {2} had length {a2.Length} but expected {len}");
                if (len != a3.Length)
                    throw new Exception($"Expected blocks to have same size - block {3} had length {a3.Length} but expected {len}");
                if (len != a4.Length)
                    throw new Exception($"Expected blocks to have same size - block {4} had length {a4.Length} but expected {len}");
                if (len != a5.Length)
                    throw new Exception($"Expected blocks to have same size - block {5} had length {a5.Length} but expected {len}");
                if (len != a6.Length)
                    throw new Exception($"Expected blocks to have same size - block {6} had length {a6.Length} but expected {len}");
                if (len != a7.Length)
                    throw new Exception($"Expected blocks to have same size - block {7} had length {a7.Length} but expected {len}");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6, T7>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7>(_dataTable, i + offset, a1[i], a2[i], a3[i], a4[i], a5[i], a6[i], a7[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7>> GetRowsBuffer<T1, T2, T3, T4, T5, T6, T7>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6
        ) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6, T7>(dataTable, dataTable.GetColumn<T1>(columnIndex1), dataTable.GetColumn<T2>(columnIndex2), dataTable.GetColumn<T3>(columnIndex3), dataTable.GetColumn<T4>(columnIndex4), dataTable.GetColumn<T5>(columnIndex5), dataTable.GetColumn<T6>(columnIndex6), dataTable.GetColumn<T7>(columnIndex7));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1, T2, T3, T4, T5, T6, T7>> GetRow<T1, T2, T3, T4, T5, T6, T7>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            var column7 = dataTable.GetColumn<T7>(columnIndex7);
            var getTask1 = column1.GetItem(rowIndex);
            var getTask2 = column2.GetItem(rowIndex);
            var getTask3 = column3.GetItem(rowIndex);
            var getTask4 = column4.GetItem(rowIndex);
            var getTask5 = column5.GetItem(rowIndex);
            var getTask6 = column6.GetItem(rowIndex);
            var getTask7 = column7.GetItem(rowIndex);
            await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6, getTask7);
            return new TableRow<T1, T2, T3, T4, T5, T6, T7>(dataTable, rowIndex, getTask1.Result, getTask2.Result, getTask3.Result, getTask4.Result, getTask5.Result, getTask6.Result, getTask7.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1, T2, T3, T4, T5, T6, T7>[]> GetRows<T1, T2, T3, T4, T5, T6, T7>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            params uint[] rowIndices
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            var column7 = dataTable.GetColumn<T7>(columnIndex7);
            return dataTable.CopyRows<TableRow<T1, T2, T3, T4, T5, T6, T7>>(column1, rowIndices, x => new TableRow<T1, T2, T3, T4, T5, T6, T7>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                var getTask2 = column2.GetTypedBlock(blockIndex);
                var getTask3 = column3.GetTypedBlock(blockIndex);
                var getTask4 = column4.GetTypedBlock(blockIndex);
                var getTask5 = column5.GetTypedBlock(blockIndex);
                var getTask6 = column6.GetTypedBlock(blockIndex);
                var getTask7 = column7.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6, getTask7);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1, T2, T3, T4, T5, T6, T7> row) => {
                    var span1 = getTask1.Result.Span;
                    var span2 = getTask2.Result.Span;
                    var span3 = getTask3.Result.Span;
                    var span4 = getTask4.Result.Span;
                    var span5 = getTask5.Result.Span;
                    var span6 = getTask6.Result.Span;
                    var span7 = getTask7.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex], span2[(int)relativeBlockIndex], span3[(int)relativeBlockIndex], span4[(int)relativeBlockIndex], span5[(int)relativeBlockIndex], span6[(int)relativeBlockIndex], span7[(int)relativeBlockIndex]);
                }); 
            });
        }
      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            uint columnIndex8 = 7, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
        {
            const int size = 8;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            await using var e2 = dataTable.GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            await using var e3 = dataTable.GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            await using var e4 = dataTable.GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            await using var e5 = dataTable.GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            await using var e6 = dataTable.GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            await using var e7 = dataTable.GetColumn<T7>(columnIndex7).GetAsyncEnumerator(ct);
            await using var e8 = dataTable.GetColumn<T8>(columnIndex8).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                currentTasks[1] = e2.MoveNextAsync();
                currentTasks[2] = e3.MoveNextAsync();
                currentTasks[3] = e4.MoveNextAsync();
                currentTasks[4] = e5.MoveNextAsync();
                currentTasks[5] = e6.MoveNextAsync();
                currentTasks[6] = e7.MoveNextAsync();
                currentTasks[7] = e8.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;
            readonly IReadOnlyBuffer<T6> _input6;
            readonly IReadOnlyBuffer<T7> _input7;
            readonly IReadOnlyBuffer<T8> _input8;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                if (!BlockSizes.SequenceEqual(input2.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input3.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input4.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input5.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input6.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input7.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input8.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1, T2, T3, T4, T5, T6, T7, T8>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
                _input5 = input5;
                _input6 = input6;
                _input7 = input7;
                _input8 = input8;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                var b3 = _input3.GetTypedBlock(blockIndex);
                var b4 = _input4.GetTypedBlock(blockIndex);
                var b5 = _input5.GetTypedBlock(blockIndex);
                var b6 = _input6.GetTypedBlock(blockIndex);
                var b7 = _input7.GetTypedBlock(blockIndex);
                var b8 = _input8.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8);
                var block1 = b1.Result;
                var block2 = b2.Result;
                var block3 = b3.Result;
                var block4 = b4.Result;
                var block5 = b5.Result;
                var block6 = b6.Result;
                var block7 = b7.Result;
                var block8 = b8.Result;
                if (block1.Length != block2.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block2:{block2.Length})");
                if (block1.Length != block3.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block3:{block3.Length})");
                if (block1.Length != block4.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block4:{block4.Length})");
                if (block1.Length != block5.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block5:{block5.Length})");
                if (block1.Length != block6.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block6:{block6.Length})");
                if (block1.Length != block7.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block7:{block7.Length})");
                if (block1.Length != block8.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block8:{block8.Length})");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, Span<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(_dataTable, firstIndex + (uint)i, span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> EnumerateAllTyped()
            {
                const int size = 8;
                await using var e1 = _input1.GetAsyncEnumerator();
                await using var e2 = _input2.GetAsyncEnumerator();
                await using var e3 = _input3.GetAsyncEnumerator();
                await using var e4 = _input4.GetAsyncEnumerator();
                await using var e5 = _input5.GetAsyncEnumerator();
                await using var e6 = _input6.GetAsyncEnumerator();
                await using var e7 = _input7.GetAsyncEnumerator();
                await using var e8 = _input8.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    currentTasks[1] = e2.MoveNextAsync();
                    currentTasks[2] = e3.MoveNextAsync();
                    currentTasks[3] = e4.MoveNextAsync();
                    currentTasks[4] = e5.MoveNextAsync();
                    currentTasks[5] = e6.MoveNextAsync();
                    currentTasks[6] = e7.MoveNextAsync();
                    currentTasks[7] = e8.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(_dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                var b2 = _input2.GetBlock(blockIndex);
                var b3 = _input3.GetBlock(blockIndex);
                var b4 = _input4.GetBlock(blockIndex);
                var b5 = _input5.GetBlock(blockIndex);
                var b6 = _input6.GetBlock(blockIndex);
                var b7 = _input7.GetBlock(blockIndex);
                var b8 = _input8.GetBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8);
                var a1 = (T1[])b1.Result;
                var a2 = (T2[])b2.Result;
                var a3 = (T3[])b3.Result;
                var a4 = (T4[])b4.Result;
                var a5 = (T5[])b5.Result;
                var a6 = (T6[])b6.Result;
                var a7 = (T7[])b7.Result;
                var a8 = (T8[])b8.Result;
                var len = a1.Length;
                if (len != a2.Length)
                    throw new Exception($"Expected blocks to have same size - block {2} had length {a2.Length} but expected {len}");
                if (len != a3.Length)
                    throw new Exception($"Expected blocks to have same size - block {3} had length {a3.Length} but expected {len}");
                if (len != a4.Length)
                    throw new Exception($"Expected blocks to have same size - block {4} had length {a4.Length} but expected {len}");
                if (len != a5.Length)
                    throw new Exception($"Expected blocks to have same size - block {5} had length {a5.Length} but expected {len}");
                if (len != a6.Length)
                    throw new Exception($"Expected blocks to have same size - block {6} had length {a6.Length} but expected {len}");
                if (len != a7.Length)
                    throw new Exception($"Expected blocks to have same size - block {7} had length {a7.Length} but expected {len}");
                if (len != a8.Length)
                    throw new Exception($"Expected blocks to have same size - block {8} had length {a8.Length} but expected {len}");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(_dataTable, i + offset, a1[i], a2[i], a3[i], a4[i], a5[i], a6[i], a7[i], a8[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> GetRowsBuffer<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7
        ) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8>(dataTable, dataTable.GetColumn<T1>(columnIndex1), dataTable.GetColumn<T2>(columnIndex2), dataTable.GetColumn<T3>(columnIndex3), dataTable.GetColumn<T4>(columnIndex4), dataTable.GetColumn<T5>(columnIndex5), dataTable.GetColumn<T6>(columnIndex6), dataTable.GetColumn<T7>(columnIndex7), dataTable.GetColumn<T8>(columnIndex8));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> GetRow<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            var column7 = dataTable.GetColumn<T7>(columnIndex7);
            var column8 = dataTable.GetColumn<T8>(columnIndex8);
            var getTask1 = column1.GetItem(rowIndex);
            var getTask2 = column2.GetItem(rowIndex);
            var getTask3 = column3.GetItem(rowIndex);
            var getTask4 = column4.GetItem(rowIndex);
            var getTask5 = column5.GetItem(rowIndex);
            var getTask6 = column6.GetItem(rowIndex);
            var getTask7 = column7.GetItem(rowIndex);
            var getTask8 = column8.GetItem(rowIndex);
            await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6, getTask7, getTask8);
            return new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(dataTable, rowIndex, getTask1.Result, getTask2.Result, getTask3.Result, getTask4.Result, getTask5.Result, getTask6.Result, getTask7.Result, getTask8.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>[]> GetRows<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7,
            params uint[] rowIndices
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            var column7 = dataTable.GetColumn<T7>(columnIndex7);
            var column8 = dataTable.GetColumn<T8>(columnIndex8);
            return dataTable.CopyRows<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>>(column1, rowIndices, x => new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                var getTask2 = column2.GetTypedBlock(blockIndex);
                var getTask3 = column3.GetTypedBlock(blockIndex);
                var getTask4 = column4.GetTypedBlock(blockIndex);
                var getTask5 = column5.GetTypedBlock(blockIndex);
                var getTask6 = column6.GetTypedBlock(blockIndex);
                var getTask7 = column7.GetTypedBlock(blockIndex);
                var getTask8 = column8.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6, getTask7, getTask8);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1, T2, T3, T4, T5, T6, T7, T8> row) => {
                    var span1 = getTask1.Result.Span;
                    var span2 = getTask2.Result.Span;
                    var span3 = getTask3.Result.Span;
                    var span4 = getTask4.Result.Span;
                    var span5 = getTask5.Result.Span;
                    var span6 = getTask6.Result.Span;
                    var span7 = getTask7.Result.Span;
                    var span8 = getTask8.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex], span2[(int)relativeBlockIndex], span3[(int)relativeBlockIndex], span4[(int)relativeBlockIndex], span5[(int)relativeBlockIndex], span6[(int)relativeBlockIndex], span7[(int)relativeBlockIndex], span8[(int)relativeBlockIndex]);
                }); 
            });
        }
      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            uint columnIndex8 = 7, 
            uint columnIndex9 = 8, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
        {
            const int size = 9;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            await using var e2 = dataTable.GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            await using var e3 = dataTable.GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            await using var e4 = dataTable.GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            await using var e5 = dataTable.GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            await using var e6 = dataTable.GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            await using var e7 = dataTable.GetColumn<T7>(columnIndex7).GetAsyncEnumerator(ct);
            await using var e8 = dataTable.GetColumn<T8>(columnIndex8).GetAsyncEnumerator(ct);
            await using var e9 = dataTable.GetColumn<T9>(columnIndex9).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                currentTasks[1] = e2.MoveNextAsync();
                currentTasks[2] = e3.MoveNextAsync();
                currentTasks[3] = e4.MoveNextAsync();
                currentTasks[4] = e5.MoveNextAsync();
                currentTasks[5] = e6.MoveNextAsync();
                currentTasks[6] = e7.MoveNextAsync();
                currentTasks[7] = e8.MoveNextAsync();
                currentTasks[8] = e9.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;
            readonly IReadOnlyBuffer<T6> _input6;
            readonly IReadOnlyBuffer<T7> _input7;
            readonly IReadOnlyBuffer<T8> _input8;
            readonly IReadOnlyBuffer<T9> _input9;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                if (!BlockSizes.SequenceEqual(input2.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input3.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input4.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input5.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input6.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input7.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input8.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input9.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
                _input5 = input5;
                _input6 = input6;
                _input7 = input7;
                _input8 = input8;
                _input9 = input9;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                var b3 = _input3.GetTypedBlock(blockIndex);
                var b4 = _input4.GetTypedBlock(blockIndex);
                var b5 = _input5.GetTypedBlock(blockIndex);
                var b6 = _input6.GetTypedBlock(blockIndex);
                var b7 = _input7.GetTypedBlock(blockIndex);
                var b8 = _input8.GetTypedBlock(blockIndex);
                var b9 = _input9.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9);
                var block1 = b1.Result;
                var block2 = b2.Result;
                var block3 = b3.Result;
                var block4 = b4.Result;
                var block5 = b5.Result;
                var block6 = b6.Result;
                var block7 = b7.Result;
                var block8 = b8.Result;
                var block9 = b9.Result;
                if (block1.Length != block2.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block2:{block2.Length})");
                if (block1.Length != block3.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block3:{block3.Length})");
                if (block1.Length != block4.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block4:{block4.Length})");
                if (block1.Length != block5.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block5:{block5.Length})");
                if (block1.Length != block6.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block6:{block6.Length})");
                if (block1.Length != block7.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block7:{block7.Length})");
                if (block1.Length != block8.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block8:{block8.Length})");
                if (block1.Length != block9.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block9:{block9.Length})");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, Span<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(_dataTable, firstIndex + (uint)i, span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> EnumerateAllTyped()
            {
                const int size = 9;
                await using var e1 = _input1.GetAsyncEnumerator();
                await using var e2 = _input2.GetAsyncEnumerator();
                await using var e3 = _input3.GetAsyncEnumerator();
                await using var e4 = _input4.GetAsyncEnumerator();
                await using var e5 = _input5.GetAsyncEnumerator();
                await using var e6 = _input6.GetAsyncEnumerator();
                await using var e7 = _input7.GetAsyncEnumerator();
                await using var e8 = _input8.GetAsyncEnumerator();
                await using var e9 = _input9.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    currentTasks[1] = e2.MoveNextAsync();
                    currentTasks[2] = e3.MoveNextAsync();
                    currentTasks[3] = e4.MoveNextAsync();
                    currentTasks[4] = e5.MoveNextAsync();
                    currentTasks[5] = e6.MoveNextAsync();
                    currentTasks[6] = e7.MoveNextAsync();
                    currentTasks[7] = e8.MoveNextAsync();
                    currentTasks[8] = e9.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(_dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                var b2 = _input2.GetBlock(blockIndex);
                var b3 = _input3.GetBlock(blockIndex);
                var b4 = _input4.GetBlock(blockIndex);
                var b5 = _input5.GetBlock(blockIndex);
                var b6 = _input6.GetBlock(blockIndex);
                var b7 = _input7.GetBlock(blockIndex);
                var b8 = _input8.GetBlock(blockIndex);
                var b9 = _input9.GetBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9);
                var a1 = (T1[])b1.Result;
                var a2 = (T2[])b2.Result;
                var a3 = (T3[])b3.Result;
                var a4 = (T4[])b4.Result;
                var a5 = (T5[])b5.Result;
                var a6 = (T6[])b6.Result;
                var a7 = (T7[])b7.Result;
                var a8 = (T8[])b8.Result;
                var a9 = (T9[])b9.Result;
                var len = a1.Length;
                if (len != a2.Length)
                    throw new Exception($"Expected blocks to have same size - block {2} had length {a2.Length} but expected {len}");
                if (len != a3.Length)
                    throw new Exception($"Expected blocks to have same size - block {3} had length {a3.Length} but expected {len}");
                if (len != a4.Length)
                    throw new Exception($"Expected blocks to have same size - block {4} had length {a4.Length} but expected {len}");
                if (len != a5.Length)
                    throw new Exception($"Expected blocks to have same size - block {5} had length {a5.Length} but expected {len}");
                if (len != a6.Length)
                    throw new Exception($"Expected blocks to have same size - block {6} had length {a6.Length} but expected {len}");
                if (len != a7.Length)
                    throw new Exception($"Expected blocks to have same size - block {7} had length {a7.Length} but expected {len}");
                if (len != a8.Length)
                    throw new Exception($"Expected blocks to have same size - block {8} had length {a8.Length} but expected {len}");
                if (len != a9.Length)
                    throw new Exception($"Expected blocks to have same size - block {9} had length {a9.Length} but expected {len}");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(_dataTable, i + offset, a1[i], a2[i], a3[i], a4[i], a5[i], a6[i], a7[i], a8[i], a9[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> GetRowsBuffer<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7,
            uint columnIndex9 = 8
        ) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(dataTable, dataTable.GetColumn<T1>(columnIndex1), dataTable.GetColumn<T2>(columnIndex2), dataTable.GetColumn<T3>(columnIndex3), dataTable.GetColumn<T4>(columnIndex4), dataTable.GetColumn<T5>(columnIndex5), dataTable.GetColumn<T6>(columnIndex6), dataTable.GetColumn<T7>(columnIndex7), dataTable.GetColumn<T8>(columnIndex8), dataTable.GetColumn<T9>(columnIndex9));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> GetRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7,
            uint columnIndex9 = 8
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            var column7 = dataTable.GetColumn<T7>(columnIndex7);
            var column8 = dataTable.GetColumn<T8>(columnIndex8);
            var column9 = dataTable.GetColumn<T9>(columnIndex9);
            var getTask1 = column1.GetItem(rowIndex);
            var getTask2 = column2.GetItem(rowIndex);
            var getTask3 = column3.GetItem(rowIndex);
            var getTask4 = column4.GetItem(rowIndex);
            var getTask5 = column5.GetItem(rowIndex);
            var getTask6 = column6.GetItem(rowIndex);
            var getTask7 = column7.GetItem(rowIndex);
            var getTask8 = column8.GetItem(rowIndex);
            var getTask9 = column9.GetItem(rowIndex);
            await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6, getTask7, getTask8, getTask9);
            return new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(dataTable, rowIndex, getTask1.Result, getTask2.Result, getTask3.Result, getTask4.Result, getTask5.Result, getTask6.Result, getTask7.Result, getTask8.Result, getTask9.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>[]> GetRows<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7,
            uint columnIndex9 = 8,
            params uint[] rowIndices
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            var column7 = dataTable.GetColumn<T7>(columnIndex7);
            var column8 = dataTable.GetColumn<T8>(columnIndex8);
            var column9 = dataTable.GetColumn<T9>(columnIndex9);
            return dataTable.CopyRows<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(column1, rowIndices, x => new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                var getTask2 = column2.GetTypedBlock(blockIndex);
                var getTask3 = column3.GetTypedBlock(blockIndex);
                var getTask4 = column4.GetTypedBlock(blockIndex);
                var getTask5 = column5.GetTypedBlock(blockIndex);
                var getTask6 = column6.GetTypedBlock(blockIndex);
                var getTask7 = column7.GetTypedBlock(blockIndex);
                var getTask8 = column8.GetTypedBlock(blockIndex);
                var getTask9 = column9.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6, getTask7, getTask8, getTask9);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9> row) => {
                    var span1 = getTask1.Result.Span;
                    var span2 = getTask2.Result.Span;
                    var span3 = getTask3.Result.Span;
                    var span4 = getTask4.Result.Span;
                    var span5 = getTask5.Result.Span;
                    var span6 = getTask6.Result.Span;
                    var span7 = getTask7.Result.Span;
                    var span8 = getTask8.Result.Span;
                    var span9 = getTask9.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex], span2[(int)relativeBlockIndex], span3[(int)relativeBlockIndex], span4[(int)relativeBlockIndex], span5[(int)relativeBlockIndex], span6[(int)relativeBlockIndex], span7[(int)relativeBlockIndex], span8[(int)relativeBlockIndex], span9[(int)relativeBlockIndex]);
                }); 
            });
        }
      
        /// <summary>
        /// Enumerates typed data rows
        /// </summary>
        public static async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDataTable dataTable,
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            uint columnIndex8 = 7, 
            uint columnIndex9 = 8, 
            uint columnIndex10 = 9, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
			where T10: notnull
        {
            const int size = 10;
            await using var e1 = dataTable.GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            await using var e2 = dataTable.GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            await using var e3 = dataTable.GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            await using var e4 = dataTable.GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            await using var e5 = dataTable.GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            await using var e6 = dataTable.GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            await using var e7 = dataTable.GetColumn<T7>(columnIndex7).GetAsyncEnumerator(ct);
            await using var e8 = dataTable.GetColumn<T8>(columnIndex8).GetAsyncEnumerator(ct);
            await using var e9 = dataTable.GetColumn<T9>(columnIndex9).GetAsyncEnumerator(ct);
            await using var e10 = dataTable.GetColumn<T10>(columnIndex10).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;
            uint rowIndex = 0;

            while (!ct.IsCancellationRequested && isValid) {
                currentTasks[0] = e1.MoveNextAsync();
                currentTasks[1] = e2.MoveNextAsync();
                currentTasks[2] = e3.MoveNextAsync();
                currentTasks[3] = e4.MoveNextAsync();
                currentTasks[4] = e5.MoveNextAsync();
                currentTasks[5] = e6.MoveNextAsync();
                currentTasks[6] = e7.MoveNextAsync();
                currentTasks[7] = e8.MoveNextAsync();
                currentTasks[8] = e9.MoveNextAsync();
                currentTasks[9] = e10.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                if(isValid) {
                    var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
			where T10: notnull
        {
            readonly IDataTable _dataTable;
            readonly uint _blockSize, _blockCount;
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;
            readonly IReadOnlyBuffer<T6> _input6;
            readonly IReadOnlyBuffer<T7> _input7;
            readonly IReadOnlyBuffer<T8> _input8;
            readonly IReadOnlyBuffer<T9> _input9;
            readonly IReadOnlyBuffer<T10> _input10;

            public RowReader(IDataTable dataTable, IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10)
            {
                _dataTable = dataTable;
                BlockSizes = input1.BlockSizes;
                if (!BlockSizes.SequenceEqual(input2.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input3.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input4.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input5.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input6.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input7.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input8.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input9.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                if (!BlockSizes.SequenceEqual(input10.BlockSizes))
                    throw new ArgumentException($"Expected all buffers to have same block sizes");
                _blockCount = (uint)BlockSizes.Length;
                _blockSize = BlockSizes[0];
                Size = input1.Size;
                DataType = typeof(TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
                _input5 = input5;
                _input6 = input6;
                _input7 = input7;
                _input8 = input8;
                _input9 = input9;
                _input10 = input10;
            }

            public uint Size { get; }
            public uint[] BlockSizes { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> callback, CancellationToken ct = default)
            {
                for (uint i = 0; i < _blockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                var b3 = _input3.GetTypedBlock(blockIndex);
                var b4 = _input4.GetTypedBlock(blockIndex);
                var b5 = _input5.GetTypedBlock(blockIndex);
                var b6 = _input6.GetTypedBlock(blockIndex);
                var b7 = _input7.GetTypedBlock(blockIndex);
                var b8 = _input8.GetTypedBlock(blockIndex);
                var b9 = _input9.GetTypedBlock(blockIndex);
                var b10 = _input10.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10);
                var block1 = b1.Result;
                var block2 = b2.Result;
                var block3 = b3.Result;
                var block4 = b4.Result;
                var block5 = b5.Result;
                var block6 = b6.Result;
                var block7 = b7.Result;
                var block8 = b8.Result;
                var block9 = b9.Result;
                var block10 = b10.Result;
                if (block1.Length != block2.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block2:{block2.Length})");
                if (block1.Length != block3.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block3:{block3.Length})");
                if (block1.Length != block4.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block4:{block4.Length})");
                if (block1.Length != block5.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block5:{block5.Length})");
                if (block1.Length != block6.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block6:{block6.Length})");
                if (block1.Length != block7.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block7:{block7.Length})");
                if (block1.Length != block8.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block8:{block8.Length})");
                if (block1.Length != block9.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block9:{block9.Length})");
                if (block1.Length != block10.Length)
                    throw new Exception($"Expected all blocks to have same size (block1:{block1.Length} vs block10:{block10.Length})");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>[block1.Length];
                Copy(blockIndex * _blockSize, block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, ret);
                return ret;
            }

            void Copy(uint firstIndex, ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, Span<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(_dataTable, firstIndex + (uint)i, span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> EnumerateAllTyped()
            {
                const int size = 10;
                await using var e1 = _input1.GetAsyncEnumerator();
                await using var e2 = _input2.GetAsyncEnumerator();
                await using var e3 = _input3.GetAsyncEnumerator();
                await using var e4 = _input4.GetAsyncEnumerator();
                await using var e5 = _input5.GetAsyncEnumerator();
                await using var e6 = _input6.GetAsyncEnumerator();
                await using var e7 = _input7.GetAsyncEnumerator();
                await using var e8 = _input8.GetAsyncEnumerator();
                await using var e9 = _input9.GetAsyncEnumerator();
                await using var e10 = _input10.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;
                uint rowIndex = 0;

                while (isValid) {
                    currentTasks[0] = e1.MoveNextAsync();
                    currentTasks[1] = e2.MoveNextAsync();
                    currentTasks[2] = e3.MoveNextAsync();
                    currentTasks[3] = e4.MoveNextAsync();
                    currentTasks[4] = e5.MoveNextAsync();
                    currentTasks[5] = e6.MoveNextAsync();
                    currentTasks[6] = e7.MoveNextAsync();
                    currentTasks[7] = e8.MoveNextAsync();
                    currentTasks[8] = e9.MoveNextAsync();
                    currentTasks[9] = e10.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    if(isValid) {
                        var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(_dataTable, rowIndex++, e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);

            public async Task<Array> GetBlock(uint blockIndex)
            {
                var b1 = _input1.GetBlock(blockIndex);
                var b2 = _input2.GetBlock(blockIndex);
                var b3 = _input3.GetBlock(blockIndex);
                var b4 = _input4.GetBlock(blockIndex);
                var b5 = _input5.GetBlock(blockIndex);
                var b6 = _input6.GetBlock(blockIndex);
                var b7 = _input7.GetBlock(blockIndex);
                var b8 = _input8.GetBlock(blockIndex);
                var b9 = _input9.GetBlock(blockIndex);
                var b10 = _input10.GetBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10);
                var a1 = (T1[])b1.Result;
                var a2 = (T2[])b2.Result;
                var a3 = (T3[])b3.Result;
                var a4 = (T4[])b4.Result;
                var a5 = (T5[])b5.Result;
                var a6 = (T6[])b6.Result;
                var a7 = (T7[])b7.Result;
                var a8 = (T8[])b8.Result;
                var a9 = (T9[])b9.Result;
                var a10 = (T10[])b10.Result;
                var len = a1.Length;
                if (len != a2.Length)
                    throw new Exception($"Expected blocks to have same size - block {2} had length {a2.Length} but expected {len}");
                if (len != a3.Length)
                    throw new Exception($"Expected blocks to have same size - block {3} had length {a3.Length} but expected {len}");
                if (len != a4.Length)
                    throw new Exception($"Expected blocks to have same size - block {4} had length {a4.Length} but expected {len}");
                if (len != a5.Length)
                    throw new Exception($"Expected blocks to have same size - block {5} had length {a5.Length} but expected {len}");
                if (len != a6.Length)
                    throw new Exception($"Expected blocks to have same size - block {6} had length {a6.Length} but expected {len}");
                if (len != a7.Length)
                    throw new Exception($"Expected blocks to have same size - block {7} had length {a7.Length} but expected {len}");
                if (len != a8.Length)
                    throw new Exception($"Expected blocks to have same size - block {8} had length {a8.Length} but expected {len}");
                if (len != a9.Length)
                    throw new Exception($"Expected blocks to have same size - block {9} had length {a9.Length} but expected {len}");
                if (len != a10.Length)
                    throw new Exception($"Expected blocks to have same size - block {10} had length {a10.Length} but expected {len}");
                var ret = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>[len];
                var offset = blockIndex * _blockSize;
                for (uint i = 0; i < len; i++)
                    ret[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(_dataTable, i + offset, a1[i], a2[i], a3[i], a4[i], a5[i], a6[i], a7[i], a8[i], a9[i], a10[i]);
                return ret;
            }
        }

        /// <summary>
        /// Creates a typed buffer to access rows
        /// </summary>
        public static IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> GetRowsBuffer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7,
            uint columnIndex9 = 8,
            uint columnIndex10 = 9
        ) 
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
			where T10: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(dataTable, dataTable.GetColumn<T1>(columnIndex1), dataTable.GetColumn<T2>(columnIndex2), dataTable.GetColumn<T3>(columnIndex3), dataTable.GetColumn<T4>(columnIndex4), dataTable.GetColumn<T5>(columnIndex5), dataTable.GetColumn<T6>(columnIndex6), dataTable.GetColumn<T7>(columnIndex7), dataTable.GetColumn<T8>(columnIndex8), dataTable.GetColumn<T9>(columnIndex9), dataTable.GetColumn<T10>(columnIndex10));
        }

        /// <summary>
        /// Returns a single typed row from the data table
        /// </summary>
        public static async Task<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> GetRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDataTable dataTable,
            uint rowIndex,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7,
            uint columnIndex9 = 8,
            uint columnIndex10 = 9
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
			where T10: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            var column7 = dataTable.GetColumn<T7>(columnIndex7);
            var column8 = dataTable.GetColumn<T8>(columnIndex8);
            var column9 = dataTable.GetColumn<T9>(columnIndex9);
            var column10 = dataTable.GetColumn<T10>(columnIndex10);
            var getTask1 = column1.GetItem(rowIndex);
            var getTask2 = column2.GetItem(rowIndex);
            var getTask3 = column3.GetItem(rowIndex);
            var getTask4 = column4.GetItem(rowIndex);
            var getTask5 = column5.GetItem(rowIndex);
            var getTask6 = column6.GetItem(rowIndex);
            var getTask7 = column7.GetItem(rowIndex);
            var getTask8 = column8.GetItem(rowIndex);
            var getTask9 = column9.GetItem(rowIndex);
            var getTask10 = column10.GetItem(rowIndex);
            await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6, getTask7, getTask8, getTask9, getTask10);
            return new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(dataTable, rowIndex, getTask1.Result, getTask2.Result, getTask3.Result, getTask4.Result, getTask5.Result, getTask6.Result, getTask7.Result, getTask8.Result, getTask9.Result, getTask10.Result);
        }

        /// <summary>
        /// Returns an array of typed rows from the data table
        /// </summary>
        public static Task<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>[]> GetRows<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDataTable dataTable,
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7,
            uint columnIndex9 = 8,
            uint columnIndex10 = 9,
            params uint[] rowIndices
        )
            where T1: notnull
			where T2: notnull
			where T3: notnull
			where T4: notnull
			where T5: notnull
			where T6: notnull
			where T7: notnull
			where T8: notnull
			where T9: notnull
			where T10: notnull
        {
            var column1 = dataTable.GetColumn<T1>(columnIndex1);
            var column2 = dataTable.GetColumn<T2>(columnIndex2);
            var column3 = dataTable.GetColumn<T3>(columnIndex3);
            var column4 = dataTable.GetColumn<T4>(columnIndex4);
            var column5 = dataTable.GetColumn<T5>(columnIndex5);
            var column6 = dataTable.GetColumn<T6>(columnIndex6);
            var column7 = dataTable.GetColumn<T7>(columnIndex7);
            var column8 = dataTable.GetColumn<T8>(columnIndex8);
            var column9 = dataTable.GetColumn<T9>(columnIndex9);
            var column10 = dataTable.GetColumn<T10>(columnIndex10);
            return dataTable.CopyRows<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(column1, rowIndices, x => new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>[x.Length], async (blockIndex, rowCallback) => {
                var getTask1 = column1.GetTypedBlock(blockIndex);
                var getTask2 = column2.GetTypedBlock(blockIndex);
                var getTask3 = column3.GetTypedBlock(blockIndex);
                var getTask4 = column4.GetTypedBlock(blockIndex);
                var getTask5 = column5.GetTypedBlock(blockIndex);
                var getTask6 = column6.GetTypedBlock(blockIndex);
                var getTask7 = column7.GetTypedBlock(blockIndex);
                var getTask8 = column8.GetTypedBlock(blockIndex);
                var getTask9 = column9.GetTypedBlock(blockIndex);
                var getTask10 = column10.GetTypedBlock(blockIndex);
                await Task.WhenAll(getTask1, getTask2, getTask3, getTask4, getTask5, getTask6, getTask7, getTask8, getTask9, getTask10);
                rowCallback((uint rowIndex, uint relativeBlockIndex, ref TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> row) => {
                    var span1 = getTask1.Result.Span;
                    var span2 = getTask2.Result.Span;
                    var span3 = getTask3.Result.Span;
                    var span4 = getTask4.Result.Span;
                    var span5 = getTask5.Result.Span;
                    var span6 = getTask6.Result.Span;
                    var span7 = getTask7.Result.Span;
                    var span8 = getTask8.Result.Span;
                    var span9 = getTask9.Result.Span;
                    var span10 = getTask10.Result.Span;
                    row = new(dataTable, rowIndex, span1[(int)relativeBlockIndex], span2[(int)relativeBlockIndex], span3[(int)relativeBlockIndex], span4[(int)relativeBlockIndex], span5[(int)relativeBlockIndex], span6[(int)relativeBlockIndex], span7[(int)relativeBlockIndex], span8[(int)relativeBlockIndex], span9[(int)relativeBlockIndex], span10[(int)relativeBlockIndex]);
                }); 
            });
        }
    }
}