using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable CA2012

namespace BrightData
{
    public readonly record struct TableRow<T1, T2>(T1 C1, T2 C2) where T1: notnull where T2: notnull;
    public readonly record struct TableRow<T1, T2, T3>(T1 C1, T2 C2, T3 C3) where T1: notnull where T2: notnull where T3: notnull;
    public readonly record struct TableRow<T1, T2, T3, T4>(T1 C1, T2 C2, T3 C3, T4 C4) where T1: notnull where T2: notnull where T3: notnull where T4: notnull;
    public readonly record struct TableRow<T1, T2, T3, T4, T5>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull;
    public readonly record struct TableRow<T1, T2, T3, T4, T5, T6>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull;
    public readonly record struct TableRow<T1, T2, T3, T4, T5, T6, T7>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull;
    public readonly record struct TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull;
    public readonly record struct TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull;
    public readonly record struct TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull where T10: notnull;

    public partial interface IDataTable
    {
        IAsyncEnumerable<TableRow<T1, T2>> Enumerate<T1, T2>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            CancellationToken ct = default
        ) where T1: notnull where T2: notnull;
        IAsyncEnumerable<TableRow<T1, T2, T3>> Enumerate<T1, T2, T3>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            CancellationToken ct = default
        ) where T1: notnull where T2: notnull where T3: notnull;
        IAsyncEnumerable<TableRow<T1, T2, T3, T4>> Enumerate<T1, T2, T3, T4>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            CancellationToken ct = default
        ) where T1: notnull where T2: notnull where T3: notnull where T4: notnull;
        IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5>> Enumerate<T1, T2, T3, T4, T5>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            CancellationToken ct = default
        ) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull;
        IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6>> Enumerate<T1, T2, T3, T4, T5, T6>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            CancellationToken ct = default
        ) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull;
        IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7>> Enumerate<T1, T2, T3, T4, T5, T6, T7>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            CancellationToken ct = default
        ) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull;
        IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            uint columnIndex8 = 7, 
            CancellationToken ct = default
        ) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull;
        IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            uint columnIndex8 = 7, 
            uint columnIndex9 = 8, 
            CancellationToken ct = default
        ) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull;
        IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
            CancellationToken ct = default
        ) where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull where T10: notnull;
    }
}

namespace BrightData.DataTable
{
    internal partial class ColumnOrientedDataTable
    {
      
        public async IAsyncEnumerable<TableRow<T1, T2>> Enumerate<T1, T2>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull where T2: notnull
        {
            const int size = 2;
            var e1 = GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var e2 = GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

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
                    var row = new TableRow<T1, T2>(e1.Current, e2.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2> : IReadOnlyBuffer<TableRow<T1, T2>> where T1: notnull where T2: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input2:{input2.BlockSize})");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input2:{input2.BlockCount})");
                if(input1.Size != input2.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input2:{input2.Size})");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(TableRow<T1, T2>);
                _input1 = input1;
                _input2 = input2;
            }

            public uint Size { get; }
            public uint BlockSize { get; }
            public uint BlockCount { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
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
                Copy(block1.Span, block2.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, Span<TableRow<T1, T2>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2>(span1[i], span2[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2>> EnumerateAllTyped()
            {
                const int size = 2;
                var e1 = _input1.GetAsyncEnumerator();
                var e2 = _input2.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;

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
                        var row = new TableRow<T1, T2>(e1.Current, e2.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<TableRow<T1, T2>> GetRowReader<T1, T2>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1
        ) 
            where T1: notnull where T2: notnull
        {
            return new RowReader<T1, T2>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2));
        }
      
        public async IAsyncEnumerable<TableRow<T1, T2, T3>> Enumerate<T1, T2, T3>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull where T2: notnull where T3: notnull
        {
            const int size = 3;
            var e1 = GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var e2 = GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var e3 = GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

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
                    var row = new TableRow<T1, T2, T3>(e1.Current, e2.Current, e3.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3> : IReadOnlyBuffer<TableRow<T1, T2, T3>> where T1: notnull where T2: notnull where T3: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input2:{input2.BlockSize})");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input2:{input2.BlockCount})");
                if(input1.Size != input2.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input2:{input2.Size})");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input3:{input3.BlockSize})");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input3:{input3.BlockCount})");
                if(input1.Size != input3.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input3:{input3.Size})");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(TableRow<T1, T2, T3>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
            }

            public uint Size { get; }
            public uint BlockSize { get; }
            public uint BlockCount { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
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
                Copy(block1.Span, block2.Span, block3.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, Span<TableRow<T1, T2, T3>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3>(span1[i], span2[i], span3[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3>> EnumerateAllTyped()
            {
                const int size = 3;
                var e1 = _input1.GetAsyncEnumerator();
                var e2 = _input2.GetAsyncEnumerator();
                var e3 = _input3.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;

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
                        var row = new TableRow<T1, T2, T3>(e1.Current, e2.Current, e3.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<TableRow<T1, T2, T3>> GetRowReader<T1, T2, T3>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2
        ) 
            where T1: notnull where T2: notnull where T3: notnull
        {
            return new RowReader<T1, T2, T3>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3));
        }
      
        public async IAsyncEnumerable<TableRow<T1, T2, T3, T4>> Enumerate<T1, T2, T3, T4>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull
        {
            const int size = 4;
            var e1 = GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var e2 = GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var e3 = GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            var e4 = GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

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
                    var row = new TableRow<T1, T2, T3, T4>(e1.Current, e2.Current, e3.Current, e4.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4>> where T1: notnull where T2: notnull where T3: notnull where T4: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input2:{input2.BlockSize})");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input2:{input2.BlockCount})");
                if(input1.Size != input2.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input2:{input2.Size})");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input3:{input3.BlockSize})");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input3:{input3.BlockCount})");
                if(input1.Size != input3.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input3:{input3.Size})");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input4:{input4.BlockSize})");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input4:{input4.BlockCount})");
                if(input1.Size != input4.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input4:{input4.Size})");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(TableRow<T1, T2, T3, T4>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
            }

            public uint Size { get; }
            public uint BlockSize { get; }
            public uint BlockCount { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
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
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, Span<TableRow<T1, T2, T3, T4>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4>(span1[i], span2[i], span3[i], span4[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4>> EnumerateAllTyped()
            {
                const int size = 4;
                var e1 = _input1.GetAsyncEnumerator();
                var e2 = _input2.GetAsyncEnumerator();
                var e3 = _input3.GetAsyncEnumerator();
                var e4 = _input4.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;

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
                        var row = new TableRow<T1, T2, T3, T4>(e1.Current, e2.Current, e3.Current, e4.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<TableRow<T1, T2, T3, T4>> GetRowReader<T1, T2, T3, T4>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3
        ) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull
        {
            return new RowReader<T1, T2, T3, T4>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4));
        }
      
        public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5>> Enumerate<T1, T2, T3, T4, T5>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull
        {
            const int size = 5;
            var e1 = GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var e2 = GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var e3 = GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            var e4 = GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            var e5 = GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

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
                    var row = new TableRow<T1, T2, T3, T4, T5>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5>> where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input2:{input2.BlockSize})");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input2:{input2.BlockCount})");
                if(input1.Size != input2.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input2:{input2.Size})");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input3:{input3.BlockSize})");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input3:{input3.BlockCount})");
                if(input1.Size != input3.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input3:{input3.Size})");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input4:{input4.BlockSize})");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input4:{input4.BlockCount})");
                if(input1.Size != input4.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input4:{input4.Size})");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input5:{input5.BlockSize})");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input5:{input5.BlockCount})");
                if(input1.Size != input5.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input5:{input5.Size})");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(TableRow<T1, T2, T3, T4, T5>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
                _input5 = input5;
            }

            public uint Size { get; }
            public uint BlockSize { get; }
            public uint BlockCount { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
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
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, Span<TableRow<T1, T2, T3, T4, T5>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5>(span1[i], span2[i], span3[i], span4[i], span5[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5>> EnumerateAllTyped()
            {
                const int size = 5;
                var e1 = _input1.GetAsyncEnumerator();
                var e2 = _input2.GetAsyncEnumerator();
                var e3 = _input3.GetAsyncEnumerator();
                var e4 = _input4.GetAsyncEnumerator();
                var e5 = _input5.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;

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
                        var row = new TableRow<T1, T2, T3, T4, T5>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5>> GetRowReader<T1, T2, T3, T4, T5>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4
        ) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5));
        }
      
        public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6>> Enumerate<T1, T2, T3, T4, T5, T6>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull
        {
            const int size = 6;
            var e1 = GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var e2 = GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var e3 = GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            var e4 = GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            var e5 = GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            var e6 = GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

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
                    var row = new TableRow<T1, T2, T3, T4, T5, T6>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6>> where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;
            readonly IReadOnlyBuffer<T6> _input6;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input2:{input2.BlockSize})");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input2:{input2.BlockCount})");
                if(input1.Size != input2.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input2:{input2.Size})");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input3:{input3.BlockSize})");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input3:{input3.BlockCount})");
                if(input1.Size != input3.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input3:{input3.Size})");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input4:{input4.BlockSize})");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input4:{input4.BlockCount})");
                if(input1.Size != input4.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input4:{input4.Size})");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input5:{input5.BlockSize})");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input5:{input5.BlockCount})");
                if(input1.Size != input5.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input5:{input5.Size})");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input6:{input6.BlockSize})");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input6:{input6.BlockCount})");
                if(input1.Size != input6.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input6:{input6.Size})");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(TableRow<T1, T2, T3, T4, T5, T6>);
                _input1 = input1;
                _input2 = input2;
                _input3 = input3;
                _input4 = input4;
                _input5 = input5;
                _input6 = input6;
            }

            public uint Size { get; }
            public uint BlockSize { get; }
            public uint BlockCount { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
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
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, Span<TableRow<T1, T2, T3, T4, T5, T6>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6>> EnumerateAllTyped()
            {
                const int size = 6;
                var e1 = _input1.GetAsyncEnumerator();
                var e2 = _input2.GetAsyncEnumerator();
                var e3 = _input3.GetAsyncEnumerator();
                var e4 = _input4.GetAsyncEnumerator();
                var e5 = _input5.GetAsyncEnumerator();
                var e6 = _input6.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;

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
                        var row = new TableRow<T1, T2, T3, T4, T5, T6>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6>> GetRowReader<T1, T2, T3, T4, T5, T6>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5
        ) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6));
        }
      
        public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7>> Enumerate<T1, T2, T3, T4, T5, T6, T7>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull
        {
            const int size = 7;
            var e1 = GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var e2 = GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var e3 = GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            var e4 = GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            var e5 = GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            var e6 = GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            var e7 = GetColumn<T7>(columnIndex7).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

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
                    var row = new TableRow<T1, T2, T3, T4, T5, T6, T7>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7>> where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;
            readonly IReadOnlyBuffer<T6> _input6;
            readonly IReadOnlyBuffer<T7> _input7;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input2:{input2.BlockSize})");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input2:{input2.BlockCount})");
                if(input1.Size != input2.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input2:{input2.Size})");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input3:{input3.BlockSize})");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input3:{input3.BlockCount})");
                if(input1.Size != input3.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input3:{input3.Size})");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input4:{input4.BlockSize})");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input4:{input4.BlockCount})");
                if(input1.Size != input4.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input4:{input4.Size})");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input5:{input5.BlockSize})");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input5:{input5.BlockCount})");
                if(input1.Size != input5.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input5:{input5.Size})");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input6:{input6.BlockSize})");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input6:{input6.BlockCount})");
                if(input1.Size != input6.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input6:{input6.Size})");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input7:{input7.BlockSize})");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input7:{input7.BlockCount})");
                if(input1.Size != input7.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input7:{input7.Size})");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
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
            public uint BlockSize { get; }
            public uint BlockCount { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6, T7>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
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
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, Span<TableRow<T1, T2, T3, T4, T5, T6, T7>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7>> EnumerateAllTyped()
            {
                const int size = 7;
                var e1 = _input1.GetAsyncEnumerator();
                var e2 = _input2.GetAsyncEnumerator();
                var e3 = _input3.GetAsyncEnumerator();
                var e4 = _input4.GetAsyncEnumerator();
                var e5 = _input5.GetAsyncEnumerator();
                var e6 = _input6.GetAsyncEnumerator();
                var e7 = _input7.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;

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
                        var row = new TableRow<T1, T2, T3, T4, T5, T6, T7>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6, T7>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7>> GetRowReader<T1, T2, T3, T4, T5, T6, T7>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6
        ) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6, T7>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7));
        }
      
        public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            uint columnIndex8 = 7, 
            [EnumeratorCancellation] CancellationToken ct = default) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull
        {
            const int size = 8;
            var e1 = GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var e2 = GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var e3 = GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            var e4 = GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            var e5 = GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            var e6 = GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            var e7 = GetColumn<T7>(columnIndex7).GetAsyncEnumerator(ct);
            var e8 = GetColumn<T8>(columnIndex8).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

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
                    var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;
            readonly IReadOnlyBuffer<T6> _input6;
            readonly IReadOnlyBuffer<T7> _input7;
            readonly IReadOnlyBuffer<T8> _input8;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input2:{input2.BlockSize})");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input2:{input2.BlockCount})");
                if(input1.Size != input2.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input2:{input2.Size})");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input3:{input3.BlockSize})");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input3:{input3.BlockCount})");
                if(input1.Size != input3.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input3:{input3.Size})");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input4:{input4.BlockSize})");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input4:{input4.BlockCount})");
                if(input1.Size != input4.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input4:{input4.Size})");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input5:{input5.BlockSize})");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input5:{input5.BlockCount})");
                if(input1.Size != input5.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input5:{input5.Size})");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input6:{input6.BlockSize})");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input6:{input6.BlockCount})");
                if(input1.Size != input6.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input6:{input6.Size})");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input7:{input7.BlockSize})");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input7:{input7.BlockCount})");
                if(input1.Size != input7.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input7:{input7.Size})");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input8:{input8.BlockSize})");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input8:{input8.BlockCount})");
                if(input1.Size != input8.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input8:{input8.Size})");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
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
            public uint BlockSize { get; }
            public uint BlockCount { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
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
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, Span<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> EnumerateAllTyped()
            {
                const int size = 8;
                var e1 = _input1.GetAsyncEnumerator();
                var e2 = _input2.GetAsyncEnumerator();
                var e3 = _input3.GetAsyncEnumerator();
                var e4 = _input4.GetAsyncEnumerator();
                var e5 = _input5.GetAsyncEnumerator();
                var e6 = _input6.GetAsyncEnumerator();
                var e7 = _input7.GetAsyncEnumerator();
                var e8 = _input8.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;

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
                        var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7
        ) 
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8));
        }
      
        public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull
        {
            const int size = 9;
            var e1 = GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var e2 = GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var e3 = GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            var e4 = GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            var e5 = GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            var e6 = GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            var e7 = GetColumn<T7>(columnIndex7).GetAsyncEnumerator(ct);
            var e8 = GetColumn<T8>(columnIndex8).GetAsyncEnumerator(ct);
            var e9 = GetColumn<T9>(columnIndex9).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

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
                    var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;
            readonly IReadOnlyBuffer<T6> _input6;
            readonly IReadOnlyBuffer<T7> _input7;
            readonly IReadOnlyBuffer<T8> _input8;
            readonly IReadOnlyBuffer<T9> _input9;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input2:{input2.BlockSize})");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input2:{input2.BlockCount})");
                if(input1.Size != input2.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input2:{input2.Size})");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input3:{input3.BlockSize})");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input3:{input3.BlockCount})");
                if(input1.Size != input3.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input3:{input3.Size})");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input4:{input4.BlockSize})");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input4:{input4.BlockCount})");
                if(input1.Size != input4.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input4:{input4.Size})");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input5:{input5.BlockSize})");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input5:{input5.BlockCount})");
                if(input1.Size != input5.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input5:{input5.Size})");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input6:{input6.BlockSize})");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input6:{input6.BlockCount})");
                if(input1.Size != input6.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input6:{input6.Size})");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input7:{input7.BlockSize})");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input7:{input7.BlockCount})");
                if(input1.Size != input7.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input7:{input7.Size})");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input8:{input8.BlockSize})");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input8:{input8.BlockCount})");
                if(input1.Size != input8.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input8:{input8.Size})");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input9:{input9.BlockSize})");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input9:{input9.BlockCount})");
                if(input1.Size != input9.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input9:{input9.Size})");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
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
            public uint BlockSize { get; }
            public uint BlockCount { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
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
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, Span<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> EnumerateAllTyped()
            {
                const int size = 9;
                var e1 = _input1.GetAsyncEnumerator();
                var e2 = _input2.GetAsyncEnumerator();
                var e3 = _input3.GetAsyncEnumerator();
                var e4 = _input4.GetAsyncEnumerator();
                var e5 = _input5.GetAsyncEnumerator();
                var e6 = _input6.GetAsyncEnumerator();
                var e7 = _input7.GetAsyncEnumerator();
                var e8 = _input8.GetAsyncEnumerator();
                var e9 = _input9.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;

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
                        var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9));
        }
      
        public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull where T10: notnull
        {
            const int size = 10;
            var e1 = GetColumn<T1>(columnIndex1).GetAsyncEnumerator(ct);
            var e2 = GetColumn<T2>(columnIndex2).GetAsyncEnumerator(ct);
            var e3 = GetColumn<T3>(columnIndex3).GetAsyncEnumerator(ct);
            var e4 = GetColumn<T4>(columnIndex4).GetAsyncEnumerator(ct);
            var e5 = GetColumn<T5>(columnIndex5).GetAsyncEnumerator(ct);
            var e6 = GetColumn<T6>(columnIndex6).GetAsyncEnumerator(ct);
            var e7 = GetColumn<T7>(columnIndex7).GetAsyncEnumerator(ct);
            var e8 = GetColumn<T8>(columnIndex8).GetAsyncEnumerator(ct);
            var e9 = GetColumn<T9>(columnIndex9).GetAsyncEnumerator(ct);
            var e10 = GetColumn<T10>(columnIndex10).GetAsyncEnumerator(ct);
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

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
                    var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current);
                    yield return row;
                }
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull where T10: notnull
        {
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

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input2:{input2.BlockSize})");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input2:{input2.BlockCount})");
                if(input1.Size != input2.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input2:{input2.Size})");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input3:{input3.BlockSize})");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input3:{input3.BlockCount})");
                if(input1.Size != input3.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input3:{input3.Size})");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input4:{input4.BlockSize})");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input4:{input4.BlockCount})");
                if(input1.Size != input4.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input4:{input4.Size})");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input5:{input5.BlockSize})");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input5:{input5.BlockCount})");
                if(input1.Size != input5.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input5:{input5.Size})");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input6:{input6.BlockSize})");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input6:{input6.BlockCount})");
                if(input1.Size != input6.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input6:{input6.Size})");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input7:{input7.BlockSize})");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input7:{input7.BlockCount})");
                if(input1.Size != input7.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input7:{input7.Size})");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input8:{input8.BlockSize})");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input8:{input8.BlockCount})");
                if(input1.Size != input8.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input8:{input8.Size})");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input9:{input9.BlockSize})");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input9:{input9.BlockCount})");
                if(input1.Size != input9.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input9:{input9.Size})");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException($"Expected all buffers to have same block size (input1:{input1.BlockSize} vs input10:{input10.BlockSize})");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException($"Expected all buffers to have same block count (input1:{input1.BlockCount} vs input10:{input10.BlockCount})");
                if(input1.Size != input10.Size)
                    throw new ArgumentException($"Expected all buffers to have same size (input1:{input1.Size} vs input10:{input10.Size})");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
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
            public uint BlockSize { get; }
            public uint BlockCount { get; }
            public Type DataType { get; }
            public async IAsyncEnumerable<object> EnumerateAll()
            {
                await foreach(var item in EnumerateAllTyped())
                    yield return item;
            }

            public async Task ForEachBlock(BlockCallback<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
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
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, Span<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i]);
            }

            public async IAsyncEnumerable<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> EnumerateAllTyped()
            {
                const int size = 10;
                var e1 = _input1.GetAsyncEnumerator();
                var e2 = _input2.GetAsyncEnumerator();
                var e3 = _input3.GetAsyncEnumerator();
                var e4 = _input4.GetAsyncEnumerator();
                var e5 = _input5.GetAsyncEnumerator();
                var e6 = _input6.GetAsyncEnumerator();
                var e7 = _input7.GetAsyncEnumerator();
                var e8 = _input8.GetAsyncEnumerator();
                var e9 = _input9.GetAsyncEnumerator();
                var e10 = _input10.GetAsyncEnumerator();
                var currentTasks = new ValueTask<bool>[size];
                var isValid = true;

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
                        var row = new TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current);
                        yield return row;
                    }
                }
            }

            public IAsyncEnumerator<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<TableRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
            where T1: notnull where T2: notnull where T3: notnull where T4: notnull where T5: notnull where T6: notnull where T7: notnull where T8: notnull where T9: notnull where T10: notnull
        {
            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10));
        }
    }
}