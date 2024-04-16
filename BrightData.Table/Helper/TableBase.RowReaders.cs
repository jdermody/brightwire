using System.Runtime.CompilerServices;

namespace BrightData.Table.Helper
{
    internal abstract partial class TableBase
    {
        public readonly record struct Row<T1, T2>(T1 C1, T2 C2)  where T1: notnull  where T2: notnull;
        public async IAsyncEnumerable<Row<T1, T2>> Enumerate<T1, T2>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull
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
                var row = new Row<T1, T2>(e1.Current, e2.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2> : IReadOnlyBuffer<Row<T1, T2>>  where T1: notnull  where T2: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2>);
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2);
                var block1 = b1.Result;
                var block2 = b2.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2>[block1.Length];
                Copy(block1.Span, block2.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, Span<Row<T1, T2>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2>(span1[i], span2[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2>> EnumerateAllTyped()
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
                    var row = new Row<T1, T2>(e1.Current, e2.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2>> GetRowReader<T1, T2>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1
        ) 
             where T1: notnull  where T2: notnull
        {

            return new RowReader<T1, T2>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2));
        }
        public readonly record struct Row<T1, T2, T3>(T1 C1, T2 C2, T3 C3)  where T1: notnull  where T2: notnull  where T3: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3>> Enumerate<T1, T2, T3>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull
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
                var row = new Row<T1, T2, T3>(e1.Current, e2.Current, e3.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3> : IReadOnlyBuffer<Row<T1, T2, T3>>  where T1: notnull  where T2: notnull  where T3: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3>);
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3>>> GetTypedBlock(uint blockIndex)
            {
                var b1 = _input1.GetTypedBlock(blockIndex);
                var b2 = _input2.GetTypedBlock(blockIndex);
                var b3 = _input3.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3);
                var block1 = b1.Result;
                var block2 = b2.Result;
                var block3 = b3.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, Span<Row<T1, T2, T3>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3>(span1[i], span2[i], span3[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3>> EnumerateAllTyped()
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
                    var row = new Row<T1, T2, T3>(e1.Current, e2.Current, e3.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3>> GetRowReader<T1, T2, T3>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull
        {

            return new RowReader<T1, T2, T3>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3));
        }
        public readonly record struct Row<T1, T2, T3, T4>(T1 C1, T2 C2, T3 C3, T4 C4)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4>> Enumerate<T1, T2, T3, T4>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull
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
                var row = new Row<T1, T2, T3, T4>(e1.Current, e2.Current, e3.Current, e4.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4> : IReadOnlyBuffer<Row<T1, T2, T3, T4>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4>);
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4>>> GetTypedBlock(uint blockIndex)
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
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, Span<Row<T1, T2, T3, T4>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4>(span1[i], span2[i], span3[i], span4[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4>> EnumerateAllTyped()
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
                    var row = new Row<T1, T2, T3, T4>(e1.Current, e2.Current, e3.Current, e4.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4>> GetRowReader<T1, T2, T3, T4>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull
        {

            return new RowReader<T1, T2, T3, T4>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5>> Enumerate<T1, T2, T3, T4, T5>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull
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
                var row = new Row<T1, T2, T3, T4, T5>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull
        {
            readonly IReadOnlyBuffer<T1> _input1;
            readonly IReadOnlyBuffer<T2> _input2;
            readonly IReadOnlyBuffer<T3> _input3;
            readonly IReadOnlyBuffer<T4> _input4;
            readonly IReadOnlyBuffer<T5> _input5;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5>);
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5>>> GetTypedBlock(uint blockIndex)
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
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, Span<Row<T1, T2, T3, T4, T5>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5>(span1[i], span2[i], span3[i], span4[i], span5[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5>> EnumerateAllTyped()
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
                    var row = new Row<T1, T2, T3, T4, T5>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5>> GetRowReader<T1, T2, T3, T4, T5>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6>> Enumerate<T1, T2, T3, T4, T5, T6>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull
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
                var row = new Row<T1, T2, T3, T4, T5, T6>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull
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
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6>);
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6>>> GetTypedBlock(uint blockIndex)
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
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, Span<Row<T1, T2, T3, T4, T5, T6>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6>> EnumerateAllTyped()
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
                    var row = new Row<T1, T2, T3, T4, T5, T6>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6>> GetRowReader<T1, T2, T3, T4, T5, T6>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7>> Enumerate<T1, T2, T3, T4, T5, T6, T7>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull
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
                var row = new Row<T1, T2, T3, T4, T5, T6, T7>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull
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
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7>);
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7>>> GetTypedBlock(uint blockIndex)
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
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, Span<Row<T1, T2, T3, T4, T5, T6, T7>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7>> EnumerateAllTyped()
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
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7>> GetRowReader<T1, T2, T3, T4, T5, T6, T7>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8>(
            uint columnIndex1 = 0, 
            uint columnIndex2 = 1, 
            uint columnIndex3 = 2, 
            uint columnIndex4 = 3, 
            uint columnIndex5 = 4, 
            uint columnIndex6 = 5, 
            uint columnIndex7 = 6, 
            uint columnIndex8 = 7, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull
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
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull
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
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8>);
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8>>> GetTypedBlock(uint blockIndex)
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
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8>> EnumerateAllTyped()
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
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8>(
            uint columnIndex1 = 0,
            uint columnIndex2 = 1,
            uint columnIndex3 = 2,
            uint columnIndex4 = 3,
            uint columnIndex5 = 4,
            uint columnIndex6 = 5,
            uint columnIndex7 = 6,
            uint columnIndex8 = 7
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull
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
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull
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
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>);
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>>> GetTypedBlock(uint blockIndex)
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
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>> EnumerateAllTyped()
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
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull
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
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull
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
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>);
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>> GetTypedBlock(uint blockIndex)
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
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> EnumerateAllTyped()
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
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
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
            uint columnIndex11 = 10, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull
        {
            const int size = 11;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>);
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
                _input11 = input11;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11);
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
                var block11 = b11.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> EnumerateAllTyped()
            {
                const int size = 11;
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
                var e11 = _input11.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
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
            uint columnIndex11 = 10
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11, T12 C12)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
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
            uint columnIndex11 = 10, 
            uint columnIndex12 = 11, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull
        {
            const int size = 12;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
            var e12 = GetColumn<T12>(columnIndex12).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                currentTasks[11] = e12.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;
            readonly IReadOnlyBuffer<T12> _input12;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11, IReadOnlyBuffer<T12> input12)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input12.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input12.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input12.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>);
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
                _input11 = input11;
                _input12 = input12;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                var b12 = _input12.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12);
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
                var block11 = b11.Result;
                var block12 = b12.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block12.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, block12.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, ReadOnlySpan<T12> span12, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i], span12[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> EnumerateAllTyped()
            {
                const int size = 12;
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
                var e11 = _input11.GetAsyncEnumerator();
                var e12 = _input12.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    currentTasks[11] = e12.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
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
            uint columnIndex11 = 10,
            uint columnIndex12 = 11
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11), GetColumn<T12>(columnIndex12));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11, T12 C12, T13 C13)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
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
            uint columnIndex11 = 10, 
            uint columnIndex12 = 11, 
            uint columnIndex13 = 12, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull
        {
            const int size = 13;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
            var e12 = GetColumn<T12>(columnIndex12).GetAsyncEnumerator(ct);
            var e13 = GetColumn<T13>(columnIndex13).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                currentTasks[11] = e12.MoveNextAsync();
                currentTasks[12] = e13.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;
            readonly IReadOnlyBuffer<T12> _input12;
            readonly IReadOnlyBuffer<T13> _input13;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11, IReadOnlyBuffer<T12> input12, IReadOnlyBuffer<T13> input13)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input12.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input12.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input12.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input13.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input13.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input13.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>);
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
                _input11 = input11;
                _input12 = input12;
                _input13 = input13;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                var b12 = _input12.GetTypedBlock(blockIndex);
                var b13 = _input13.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13);
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
                var block11 = b11.Result;
                var block12 = b12.Result;
                var block13 = b13.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block12.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block13.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, block12.Span, block13.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, ReadOnlySpan<T12> span12, ReadOnlySpan<T13> span13, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i], span12[i], span13[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> EnumerateAllTyped()
            {
                const int size = 13;
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
                var e11 = _input11.GetAsyncEnumerator();
                var e12 = _input12.GetAsyncEnumerator();
                var e13 = _input13.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    currentTasks[11] = e12.MoveNextAsync();
                    currentTasks[12] = e13.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
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
            uint columnIndex11 = 10,
            uint columnIndex12 = 11,
            uint columnIndex13 = 12
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11), GetColumn<T12>(columnIndex12), GetColumn<T13>(columnIndex13));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11, T12 C12, T13 C13, T14 C14)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
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
            uint columnIndex11 = 10, 
            uint columnIndex12 = 11, 
            uint columnIndex13 = 12, 
            uint columnIndex14 = 13, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull
        {
            const int size = 14;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
            var e12 = GetColumn<T12>(columnIndex12).GetAsyncEnumerator(ct);
            var e13 = GetColumn<T13>(columnIndex13).GetAsyncEnumerator(ct);
            var e14 = GetColumn<T14>(columnIndex14).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                currentTasks[11] = e12.MoveNextAsync();
                currentTasks[12] = e13.MoveNextAsync();
                currentTasks[13] = e14.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;
            readonly IReadOnlyBuffer<T12> _input12;
            readonly IReadOnlyBuffer<T13> _input13;
            readonly IReadOnlyBuffer<T14> _input14;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11, IReadOnlyBuffer<T12> input12, IReadOnlyBuffer<T13> input13, IReadOnlyBuffer<T14> input14)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input12.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input12.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input12.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input13.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input13.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input13.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input14.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input14.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input14.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>);
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
                _input11 = input11;
                _input12 = input12;
                _input13 = input13;
                _input14 = input14;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                var b12 = _input12.GetTypedBlock(blockIndex);
                var b13 = _input13.GetTypedBlock(blockIndex);
                var b14 = _input14.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14);
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
                var block11 = b11.Result;
                var block12 = b12.Result;
                var block13 = b13.Result;
                var block14 = b14.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block12.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block13.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block14.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, block12.Span, block13.Span, block14.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, ReadOnlySpan<T12> span12, ReadOnlySpan<T13> span13, ReadOnlySpan<T14> span14, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i], span12[i], span13[i], span14[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> EnumerateAllTyped()
            {
                const int size = 14;
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
                var e11 = _input11.GetAsyncEnumerator();
                var e12 = _input12.GetAsyncEnumerator();
                var e13 = _input13.GetAsyncEnumerator();
                var e14 = _input14.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    currentTasks[11] = e12.MoveNextAsync();
                    currentTasks[12] = e13.MoveNextAsync();
                    currentTasks[13] = e14.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
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
            uint columnIndex11 = 10,
            uint columnIndex12 = 11,
            uint columnIndex13 = 12,
            uint columnIndex14 = 13
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11), GetColumn<T12>(columnIndex12), GetColumn<T13>(columnIndex13), GetColumn<T14>(columnIndex14));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11, T12 C12, T13 C13, T14 C14, T15 C15)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
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
            uint columnIndex11 = 10, 
            uint columnIndex12 = 11, 
            uint columnIndex13 = 12, 
            uint columnIndex14 = 13, 
            uint columnIndex15 = 14, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull
        {
            const int size = 15;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
            var e12 = GetColumn<T12>(columnIndex12).GetAsyncEnumerator(ct);
            var e13 = GetColumn<T13>(columnIndex13).GetAsyncEnumerator(ct);
            var e14 = GetColumn<T14>(columnIndex14).GetAsyncEnumerator(ct);
            var e15 = GetColumn<T15>(columnIndex15).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                currentTasks[11] = e12.MoveNextAsync();
                currentTasks[12] = e13.MoveNextAsync();
                currentTasks[13] = e14.MoveNextAsync();
                currentTasks[14] = e15.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;
            readonly IReadOnlyBuffer<T12> _input12;
            readonly IReadOnlyBuffer<T13> _input13;
            readonly IReadOnlyBuffer<T14> _input14;
            readonly IReadOnlyBuffer<T15> _input15;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11, IReadOnlyBuffer<T12> input12, IReadOnlyBuffer<T13> input13, IReadOnlyBuffer<T14> input14, IReadOnlyBuffer<T15> input15)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input12.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input12.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input12.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input13.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input13.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input13.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input14.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input14.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input14.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input15.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input15.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input15.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>);
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
                _input11 = input11;
                _input12 = input12;
                _input13 = input13;
                _input14 = input14;
                _input15 = input15;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                var b12 = _input12.GetTypedBlock(blockIndex);
                var b13 = _input13.GetTypedBlock(blockIndex);
                var b14 = _input14.GetTypedBlock(blockIndex);
                var b15 = _input15.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15);
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
                var block11 = b11.Result;
                var block12 = b12.Result;
                var block13 = b13.Result;
                var block14 = b14.Result;
                var block15 = b15.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block12.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block13.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block14.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block15.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, block12.Span, block13.Span, block14.Span, block15.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, ReadOnlySpan<T12> span12, ReadOnlySpan<T13> span13, ReadOnlySpan<T14> span14, ReadOnlySpan<T15> span15, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i], span12[i], span13[i], span14[i], span15[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> EnumerateAllTyped()
            {
                const int size = 15;
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
                var e11 = _input11.GetAsyncEnumerator();
                var e12 = _input12.GetAsyncEnumerator();
                var e13 = _input13.GetAsyncEnumerator();
                var e14 = _input14.GetAsyncEnumerator();
                var e15 = _input15.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    currentTasks[11] = e12.MoveNextAsync();
                    currentTasks[12] = e13.MoveNextAsync();
                    currentTasks[13] = e14.MoveNextAsync();
                    currentTasks[14] = e15.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
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
            uint columnIndex11 = 10,
            uint columnIndex12 = 11,
            uint columnIndex13 = 12,
            uint columnIndex14 = 13,
            uint columnIndex15 = 14
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11), GetColumn<T12>(columnIndex12), GetColumn<T13>(columnIndex13), GetColumn<T14>(columnIndex14), GetColumn<T15>(columnIndex15));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11, T12 C12, T13 C13, T14 C14, T15 C15, T16 C16)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
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
            uint columnIndex11 = 10, 
            uint columnIndex12 = 11, 
            uint columnIndex13 = 12, 
            uint columnIndex14 = 13, 
            uint columnIndex15 = 14, 
            uint columnIndex16 = 15, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull
        {
            const int size = 16;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
            var e12 = GetColumn<T12>(columnIndex12).GetAsyncEnumerator(ct);
            var e13 = GetColumn<T13>(columnIndex13).GetAsyncEnumerator(ct);
            var e14 = GetColumn<T14>(columnIndex14).GetAsyncEnumerator(ct);
            var e15 = GetColumn<T15>(columnIndex15).GetAsyncEnumerator(ct);
            var e16 = GetColumn<T16>(columnIndex16).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                currentTasks[11] = e12.MoveNextAsync();
                currentTasks[12] = e13.MoveNextAsync();
                currentTasks[13] = e14.MoveNextAsync();
                currentTasks[14] = e15.MoveNextAsync();
                currentTasks[15] = e16.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;
            readonly IReadOnlyBuffer<T12> _input12;
            readonly IReadOnlyBuffer<T13> _input13;
            readonly IReadOnlyBuffer<T14> _input14;
            readonly IReadOnlyBuffer<T15> _input15;
            readonly IReadOnlyBuffer<T16> _input16;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11, IReadOnlyBuffer<T12> input12, IReadOnlyBuffer<T13> input13, IReadOnlyBuffer<T14> input14, IReadOnlyBuffer<T15> input15, IReadOnlyBuffer<T16> input16)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input12.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input12.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input12.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input13.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input13.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input13.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input14.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input14.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input14.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input15.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input15.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input15.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input16.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input16.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input16.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>);
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
                _input11 = input11;
                _input12 = input12;
                _input13 = input13;
                _input14 = input14;
                _input15 = input15;
                _input16 = input16;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                var b12 = _input12.GetTypedBlock(blockIndex);
                var b13 = _input13.GetTypedBlock(blockIndex);
                var b14 = _input14.GetTypedBlock(blockIndex);
                var b15 = _input15.GetTypedBlock(blockIndex);
                var b16 = _input16.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15, b16);
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
                var block11 = b11.Result;
                var block12 = b12.Result;
                var block13 = b13.Result;
                var block14 = b14.Result;
                var block15 = b15.Result;
                var block16 = b16.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block12.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block13.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block14.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block15.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block16.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, block12.Span, block13.Span, block14.Span, block15.Span, block16.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, ReadOnlySpan<T12> span12, ReadOnlySpan<T13> span13, ReadOnlySpan<T14> span14, ReadOnlySpan<T15> span15, ReadOnlySpan<T16> span16, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i], span12[i], span13[i], span14[i], span15[i], span16[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> EnumerateAllTyped()
            {
                const int size = 16;
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
                var e11 = _input11.GetAsyncEnumerator();
                var e12 = _input12.GetAsyncEnumerator();
                var e13 = _input13.GetAsyncEnumerator();
                var e14 = _input14.GetAsyncEnumerator();
                var e15 = _input15.GetAsyncEnumerator();
                var e16 = _input16.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    currentTasks[11] = e12.MoveNextAsync();
                    currentTasks[12] = e13.MoveNextAsync();
                    currentTasks[13] = e14.MoveNextAsync();
                    currentTasks[14] = e15.MoveNextAsync();
                    currentTasks[15] = e16.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
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
            uint columnIndex11 = 10,
            uint columnIndex12 = 11,
            uint columnIndex13 = 12,
            uint columnIndex14 = 13,
            uint columnIndex15 = 14,
            uint columnIndex16 = 15
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11), GetColumn<T12>(columnIndex12), GetColumn<T13>(columnIndex13), GetColumn<T14>(columnIndex14), GetColumn<T15>(columnIndex15), GetColumn<T16>(columnIndex16));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11, T12 C12, T13 C13, T14 C14, T15 C15, T16 C16, T17 C17)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(
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
            uint columnIndex11 = 10, 
            uint columnIndex12 = 11, 
            uint columnIndex13 = 12, 
            uint columnIndex14 = 13, 
            uint columnIndex15 = 14, 
            uint columnIndex16 = 15, 
            uint columnIndex17 = 16, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull
        {
            const int size = 17;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
            var e12 = GetColumn<T12>(columnIndex12).GetAsyncEnumerator(ct);
            var e13 = GetColumn<T13>(columnIndex13).GetAsyncEnumerator(ct);
            var e14 = GetColumn<T14>(columnIndex14).GetAsyncEnumerator(ct);
            var e15 = GetColumn<T15>(columnIndex15).GetAsyncEnumerator(ct);
            var e16 = GetColumn<T16>(columnIndex16).GetAsyncEnumerator(ct);
            var e17 = GetColumn<T17>(columnIndex17).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                currentTasks[11] = e12.MoveNextAsync();
                currentTasks[12] = e13.MoveNextAsync();
                currentTasks[13] = e14.MoveNextAsync();
                currentTasks[14] = e15.MoveNextAsync();
                currentTasks[15] = e16.MoveNextAsync();
                currentTasks[16] = e17.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current, e17.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;
            readonly IReadOnlyBuffer<T12> _input12;
            readonly IReadOnlyBuffer<T13> _input13;
            readonly IReadOnlyBuffer<T14> _input14;
            readonly IReadOnlyBuffer<T15> _input15;
            readonly IReadOnlyBuffer<T16> _input16;
            readonly IReadOnlyBuffer<T17> _input17;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11, IReadOnlyBuffer<T12> input12, IReadOnlyBuffer<T13> input13, IReadOnlyBuffer<T14> input14, IReadOnlyBuffer<T15> input15, IReadOnlyBuffer<T16> input16, IReadOnlyBuffer<T17> input17)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input12.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input12.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input12.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input13.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input13.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input13.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input14.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input14.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input14.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input15.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input15.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input15.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input16.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input16.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input16.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input17.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input17.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input17.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>);
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
                _input11 = input11;
                _input12 = input12;
                _input13 = input13;
                _input14 = input14;
                _input15 = input15;
                _input16 = input16;
                _input17 = input17;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                var b12 = _input12.GetTypedBlock(blockIndex);
                var b13 = _input13.GetTypedBlock(blockIndex);
                var b14 = _input14.GetTypedBlock(blockIndex);
                var b15 = _input15.GetTypedBlock(blockIndex);
                var b16 = _input16.GetTypedBlock(blockIndex);
                var b17 = _input17.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15, b16, b17);
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
                var block11 = b11.Result;
                var block12 = b12.Result;
                var block13 = b13.Result;
                var block14 = b14.Result;
                var block15 = b15.Result;
                var block16 = b16.Result;
                var block17 = b17.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block12.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block13.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block14.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block15.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block16.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block17.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, block12.Span, block13.Span, block14.Span, block15.Span, block16.Span, block17.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, ReadOnlySpan<T12> span12, ReadOnlySpan<T13> span13, ReadOnlySpan<T14> span14, ReadOnlySpan<T15> span15, ReadOnlySpan<T16> span16, ReadOnlySpan<T17> span17, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i], span12[i], span13[i], span14[i], span15[i], span16[i], span17[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>> EnumerateAllTyped()
            {
                const int size = 17;
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
                var e11 = _input11.GetAsyncEnumerator();
                var e12 = _input12.GetAsyncEnumerator();
                var e13 = _input13.GetAsyncEnumerator();
                var e14 = _input14.GetAsyncEnumerator();
                var e15 = _input15.GetAsyncEnumerator();
                var e16 = _input16.GetAsyncEnumerator();
                var e17 = _input17.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    currentTasks[11] = e12.MoveNextAsync();
                    currentTasks[12] = e13.MoveNextAsync();
                    currentTasks[13] = e14.MoveNextAsync();
                    currentTasks[14] = e15.MoveNextAsync();
                    currentTasks[15] = e16.MoveNextAsync();
                    currentTasks[16] = e17.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current, e17.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(
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
            uint columnIndex11 = 10,
            uint columnIndex12 = 11,
            uint columnIndex13 = 12,
            uint columnIndex14 = 13,
            uint columnIndex15 = 14,
            uint columnIndex16 = 15,
            uint columnIndex17 = 16
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11), GetColumn<T12>(columnIndex12), GetColumn<T13>(columnIndex13), GetColumn<T14>(columnIndex14), GetColumn<T15>(columnIndex15), GetColumn<T16>(columnIndex16), GetColumn<T17>(columnIndex17));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11, T12 C12, T13 C13, T14 C14, T15 C15, T16 C16, T17 C17, T18 C18)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(
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
            uint columnIndex11 = 10, 
            uint columnIndex12 = 11, 
            uint columnIndex13 = 12, 
            uint columnIndex14 = 13, 
            uint columnIndex15 = 14, 
            uint columnIndex16 = 15, 
            uint columnIndex17 = 16, 
            uint columnIndex18 = 17, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull
        {
            const int size = 18;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
            var e12 = GetColumn<T12>(columnIndex12).GetAsyncEnumerator(ct);
            var e13 = GetColumn<T13>(columnIndex13).GetAsyncEnumerator(ct);
            var e14 = GetColumn<T14>(columnIndex14).GetAsyncEnumerator(ct);
            var e15 = GetColumn<T15>(columnIndex15).GetAsyncEnumerator(ct);
            var e16 = GetColumn<T16>(columnIndex16).GetAsyncEnumerator(ct);
            var e17 = GetColumn<T17>(columnIndex17).GetAsyncEnumerator(ct);
            var e18 = GetColumn<T18>(columnIndex18).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                currentTasks[11] = e12.MoveNextAsync();
                currentTasks[12] = e13.MoveNextAsync();
                currentTasks[13] = e14.MoveNextAsync();
                currentTasks[14] = e15.MoveNextAsync();
                currentTasks[15] = e16.MoveNextAsync();
                currentTasks[16] = e17.MoveNextAsync();
                currentTasks[17] = e18.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current, e17.Current, e18.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;
            readonly IReadOnlyBuffer<T12> _input12;
            readonly IReadOnlyBuffer<T13> _input13;
            readonly IReadOnlyBuffer<T14> _input14;
            readonly IReadOnlyBuffer<T15> _input15;
            readonly IReadOnlyBuffer<T16> _input16;
            readonly IReadOnlyBuffer<T17> _input17;
            readonly IReadOnlyBuffer<T18> _input18;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11, IReadOnlyBuffer<T12> input12, IReadOnlyBuffer<T13> input13, IReadOnlyBuffer<T14> input14, IReadOnlyBuffer<T15> input15, IReadOnlyBuffer<T16> input16, IReadOnlyBuffer<T17> input17, IReadOnlyBuffer<T18> input18)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input12.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input12.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input12.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input13.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input13.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input13.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input14.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input14.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input14.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input15.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input15.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input15.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input16.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input16.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input16.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input17.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input17.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input17.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input18.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input18.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input18.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>);
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
                _input11 = input11;
                _input12 = input12;
                _input13 = input13;
                _input14 = input14;
                _input15 = input15;
                _input16 = input16;
                _input17 = input17;
                _input18 = input18;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                var b12 = _input12.GetTypedBlock(blockIndex);
                var b13 = _input13.GetTypedBlock(blockIndex);
                var b14 = _input14.GetTypedBlock(blockIndex);
                var b15 = _input15.GetTypedBlock(blockIndex);
                var b16 = _input16.GetTypedBlock(blockIndex);
                var b17 = _input17.GetTypedBlock(blockIndex);
                var b18 = _input18.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15, b16, b17, b18);
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
                var block11 = b11.Result;
                var block12 = b12.Result;
                var block13 = b13.Result;
                var block14 = b14.Result;
                var block15 = b15.Result;
                var block16 = b16.Result;
                var block17 = b17.Result;
                var block18 = b18.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block12.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block13.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block14.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block15.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block16.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block17.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block18.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, block12.Span, block13.Span, block14.Span, block15.Span, block16.Span, block17.Span, block18.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, ReadOnlySpan<T12> span12, ReadOnlySpan<T13> span13, ReadOnlySpan<T14> span14, ReadOnlySpan<T15> span15, ReadOnlySpan<T16> span16, ReadOnlySpan<T17> span17, ReadOnlySpan<T18> span18, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i], span12[i], span13[i], span14[i], span15[i], span16[i], span17[i], span18[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>> EnumerateAllTyped()
            {
                const int size = 18;
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
                var e11 = _input11.GetAsyncEnumerator();
                var e12 = _input12.GetAsyncEnumerator();
                var e13 = _input13.GetAsyncEnumerator();
                var e14 = _input14.GetAsyncEnumerator();
                var e15 = _input15.GetAsyncEnumerator();
                var e16 = _input16.GetAsyncEnumerator();
                var e17 = _input17.GetAsyncEnumerator();
                var e18 = _input18.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    currentTasks[11] = e12.MoveNextAsync();
                    currentTasks[12] = e13.MoveNextAsync();
                    currentTasks[13] = e14.MoveNextAsync();
                    currentTasks[14] = e15.MoveNextAsync();
                    currentTasks[15] = e16.MoveNextAsync();
                    currentTasks[16] = e17.MoveNextAsync();
                    currentTasks[17] = e18.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current, e17.Current, e18.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(
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
            uint columnIndex11 = 10,
            uint columnIndex12 = 11,
            uint columnIndex13 = 12,
            uint columnIndex14 = 13,
            uint columnIndex15 = 14,
            uint columnIndex16 = 15,
            uint columnIndex17 = 16,
            uint columnIndex18 = 17
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11), GetColumn<T12>(columnIndex12), GetColumn<T13>(columnIndex13), GetColumn<T14>(columnIndex14), GetColumn<T15>(columnIndex15), GetColumn<T16>(columnIndex16), GetColumn<T17>(columnIndex17), GetColumn<T18>(columnIndex18));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11, T12 C12, T13 C13, T14 C14, T15 C15, T16 C16, T17 C17, T18 C18, T19 C19)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull  where T19: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(
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
            uint columnIndex11 = 10, 
            uint columnIndex12 = 11, 
            uint columnIndex13 = 12, 
            uint columnIndex14 = 13, 
            uint columnIndex15 = 14, 
            uint columnIndex16 = 15, 
            uint columnIndex17 = 16, 
            uint columnIndex18 = 17, 
            uint columnIndex19 = 18, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull  where T19: notnull
        {
            const int size = 19;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
            var e12 = GetColumn<T12>(columnIndex12).GetAsyncEnumerator(ct);
            var e13 = GetColumn<T13>(columnIndex13).GetAsyncEnumerator(ct);
            var e14 = GetColumn<T14>(columnIndex14).GetAsyncEnumerator(ct);
            var e15 = GetColumn<T15>(columnIndex15).GetAsyncEnumerator(ct);
            var e16 = GetColumn<T16>(columnIndex16).GetAsyncEnumerator(ct);
            var e17 = GetColumn<T17>(columnIndex17).GetAsyncEnumerator(ct);
            var e18 = GetColumn<T18>(columnIndex18).GetAsyncEnumerator(ct);
            var e19 = GetColumn<T19>(columnIndex19).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                currentTasks[11] = e12.MoveNextAsync();
                currentTasks[12] = e13.MoveNextAsync();
                currentTasks[13] = e14.MoveNextAsync();
                currentTasks[14] = e15.MoveNextAsync();
                currentTasks[15] = e16.MoveNextAsync();
                currentTasks[16] = e17.MoveNextAsync();
                currentTasks[17] = e18.MoveNextAsync();
                currentTasks[18] = e19.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current, e17.Current, e18.Current, e19.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull  where T19: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;
            readonly IReadOnlyBuffer<T12> _input12;
            readonly IReadOnlyBuffer<T13> _input13;
            readonly IReadOnlyBuffer<T14> _input14;
            readonly IReadOnlyBuffer<T15> _input15;
            readonly IReadOnlyBuffer<T16> _input16;
            readonly IReadOnlyBuffer<T17> _input17;
            readonly IReadOnlyBuffer<T18> _input18;
            readonly IReadOnlyBuffer<T19> _input19;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11, IReadOnlyBuffer<T12> input12, IReadOnlyBuffer<T13> input13, IReadOnlyBuffer<T14> input14, IReadOnlyBuffer<T15> input15, IReadOnlyBuffer<T16> input16, IReadOnlyBuffer<T17> input17, IReadOnlyBuffer<T18> input18, IReadOnlyBuffer<T19> input19)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input12.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input12.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input12.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input13.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input13.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input13.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input14.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input14.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input14.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input15.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input15.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input15.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input16.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input16.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input16.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input17.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input17.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input17.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input18.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input18.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input18.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input19.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input19.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input19.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>);
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
                _input11 = input11;
                _input12 = input12;
                _input13 = input13;
                _input14 = input14;
                _input15 = input15;
                _input16 = input16;
                _input17 = input17;
                _input18 = input18;
                _input19 = input19;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                var b12 = _input12.GetTypedBlock(blockIndex);
                var b13 = _input13.GetTypedBlock(blockIndex);
                var b14 = _input14.GetTypedBlock(blockIndex);
                var b15 = _input15.GetTypedBlock(blockIndex);
                var b16 = _input16.GetTypedBlock(blockIndex);
                var b17 = _input17.GetTypedBlock(blockIndex);
                var b18 = _input18.GetTypedBlock(blockIndex);
                var b19 = _input19.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15, b16, b17, b18, b19);
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
                var block11 = b11.Result;
                var block12 = b12.Result;
                var block13 = b13.Result;
                var block14 = b14.Result;
                var block15 = b15.Result;
                var block16 = b16.Result;
                var block17 = b17.Result;
                var block18 = b18.Result;
                var block19 = b19.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block12.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block13.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block14.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block15.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block16.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block17.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block18.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block19.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, block12.Span, block13.Span, block14.Span, block15.Span, block16.Span, block17.Span, block18.Span, block19.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, ReadOnlySpan<T12> span12, ReadOnlySpan<T13> span13, ReadOnlySpan<T14> span14, ReadOnlySpan<T15> span15, ReadOnlySpan<T16> span16, ReadOnlySpan<T17> span17, ReadOnlySpan<T18> span18, ReadOnlySpan<T19> span19, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i], span12[i], span13[i], span14[i], span15[i], span16[i], span17[i], span18[i], span19[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>> EnumerateAllTyped()
            {
                const int size = 19;
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
                var e11 = _input11.GetAsyncEnumerator();
                var e12 = _input12.GetAsyncEnumerator();
                var e13 = _input13.GetAsyncEnumerator();
                var e14 = _input14.GetAsyncEnumerator();
                var e15 = _input15.GetAsyncEnumerator();
                var e16 = _input16.GetAsyncEnumerator();
                var e17 = _input17.GetAsyncEnumerator();
                var e18 = _input18.GetAsyncEnumerator();
                var e19 = _input19.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    currentTasks[11] = e12.MoveNextAsync();
                    currentTasks[12] = e13.MoveNextAsync();
                    currentTasks[13] = e14.MoveNextAsync();
                    currentTasks[14] = e15.MoveNextAsync();
                    currentTasks[15] = e16.MoveNextAsync();
                    currentTasks[16] = e17.MoveNextAsync();
                    currentTasks[17] = e18.MoveNextAsync();
                    currentTasks[18] = e19.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current, e17.Current, e18.Current, e19.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(
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
            uint columnIndex11 = 10,
            uint columnIndex12 = 11,
            uint columnIndex13 = 12,
            uint columnIndex14 = 13,
            uint columnIndex15 = 14,
            uint columnIndex16 = 15,
            uint columnIndex17 = 16,
            uint columnIndex18 = 17,
            uint columnIndex19 = 18
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull  where T19: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11), GetColumn<T12>(columnIndex12), GetColumn<T13>(columnIndex13), GetColumn<T14>(columnIndex14), GetColumn<T15>(columnIndex15), GetColumn<T16>(columnIndex16), GetColumn<T17>(columnIndex17), GetColumn<T18>(columnIndex18), GetColumn<T19>(columnIndex19));
        }
        public readonly record struct Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(T1 C1, T2 C2, T3 C3, T4 C4, T5 C5, T6 C6, T7 C7, T8 C8, T9 C9, T10 C10, T11 C11, T12 C12, T13 C13, T14 C14, T15 C15, T16 C16, T17 C17, T18 C18, T19 C19, T20 C20)  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull  where T19: notnull  where T20: notnull;
        public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>> Enumerate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(
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
            uint columnIndex11 = 10, 
            uint columnIndex12 = 11, 
            uint columnIndex13 = 12, 
            uint columnIndex14 = 13, 
            uint columnIndex15 = 14, 
            uint columnIndex16 = 15, 
            uint columnIndex17 = 16, 
            uint columnIndex18 = 17, 
            uint columnIndex19 = 18, 
            uint columnIndex20 = 19, 
            [EnumeratorCancellation] CancellationToken ct = default) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull  where T19: notnull  where T20: notnull
        {
            const int size = 20;
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
            var e11 = GetColumn<T11>(columnIndex11).GetAsyncEnumerator(ct);
            var e12 = GetColumn<T12>(columnIndex12).GetAsyncEnumerator(ct);
            var e13 = GetColumn<T13>(columnIndex13).GetAsyncEnumerator(ct);
            var e14 = GetColumn<T14>(columnIndex14).GetAsyncEnumerator(ct);
            var e15 = GetColumn<T15>(columnIndex15).GetAsyncEnumerator(ct);
            var e16 = GetColumn<T16>(columnIndex16).GetAsyncEnumerator(ct);
            var e17 = GetColumn<T17>(columnIndex17).GetAsyncEnumerator(ct);
            var e18 = GetColumn<T18>(columnIndex18).GetAsyncEnumerator(ct);
            var e19 = GetColumn<T19>(columnIndex19).GetAsyncEnumerator(ct);
            var e20 = GetColumn<T20>(columnIndex20).GetAsyncEnumerator(ct);
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
                currentTasks[10] = e11.MoveNextAsync();
                currentTasks[11] = e12.MoveNextAsync();
                currentTasks[12] = e13.MoveNextAsync();
                currentTasks[13] = e14.MoveNextAsync();
                currentTasks[14] = e15.MoveNextAsync();
                currentTasks[15] = e16.MoveNextAsync();
                currentTasks[16] = e17.MoveNextAsync();
                currentTasks[17] = e18.MoveNextAsync();
                currentTasks[18] = e19.MoveNextAsync();
                currentTasks[19] = e20.MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }
                var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current, e17.Current, e18.Current, e19.Current, e20.Current);
                yield return row;
            }
        }

        class RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> : IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>>  where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull  where T19: notnull  where T20: notnull
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
            readonly IReadOnlyBuffer<T11> _input11;
            readonly IReadOnlyBuffer<T12> _input12;
            readonly IReadOnlyBuffer<T13> _input13;
            readonly IReadOnlyBuffer<T14> _input14;
            readonly IReadOnlyBuffer<T15> _input15;
            readonly IReadOnlyBuffer<T16> _input16;
            readonly IReadOnlyBuffer<T17> _input17;
            readonly IReadOnlyBuffer<T18> _input18;
            readonly IReadOnlyBuffer<T19> _input19;
            readonly IReadOnlyBuffer<T20> _input20;

            public RowReader(IReadOnlyBuffer<T1> input1, IReadOnlyBuffer<T2> input2, IReadOnlyBuffer<T3> input3, IReadOnlyBuffer<T4> input4, IReadOnlyBuffer<T5> input5, IReadOnlyBuffer<T6> input6, IReadOnlyBuffer<T7> input7, IReadOnlyBuffer<T8> input8, IReadOnlyBuffer<T9> input9, IReadOnlyBuffer<T10> input10, IReadOnlyBuffer<T11> input11, IReadOnlyBuffer<T12> input12, IReadOnlyBuffer<T13> input13, IReadOnlyBuffer<T14> input14, IReadOnlyBuffer<T15> input15, IReadOnlyBuffer<T16> input16, IReadOnlyBuffer<T17> input17, IReadOnlyBuffer<T18> input18, IReadOnlyBuffer<T19> input19, IReadOnlyBuffer<T20> input20)
            {
                if (input1.BlockSize != input2.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input2.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input2.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input3.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input3.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input3.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input4.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input4.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input4.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input5.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input5.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input5.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input6.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input6.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input6.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input7.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input7.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input7.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input8.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input8.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input8.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input9.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input9.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input9.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input10.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input10.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input10.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input11.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input11.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input11.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input12.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input12.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input12.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input13.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input13.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input13.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input14.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input14.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input14.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input15.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input15.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input15.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input16.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input16.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input16.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input17.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input17.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input17.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input18.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input18.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input18.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input19.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input19.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input19.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if (input1.BlockSize != input20.BlockSize)
                    throw new ArgumentException("Expected all buffers to have same block size");
                if(input1.BlockCount != input20.BlockCount)
                    throw new ArgumentException("Expected all buffers to have same block count");
                if(input1.Size != input20.Size)
                    throw new ArgumentException("Expected all buffers to have same block count");

                Size = input1.Size;
                BlockSize = input1.BlockSize;
                BlockCount = input1.BlockCount;
                DataType = typeof(Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>);
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
                _input11 = input11;
                _input12 = input12;
                _input13 = input13;
                _input14 = input14;
                _input15 = input15;
                _input16 = input16;
                _input17 = input17;
                _input18 = input18;
                _input19 = input19;
                _input20 = input20;
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

            public async Task ForEachBlock(BlockCallback<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>> callback, INotifyUser? notify = null, string? message = null, CancellationToken ct = default)
            {
                for (uint i = 0; i < BlockCount && !ct.IsCancellationRequested; i++) {
                    var block = await GetTypedBlock(i);
                    callback(block.Span);
                }
            }

            public async Task<ReadOnlyMemory<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>>> GetTypedBlock(uint blockIndex)
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
                var b11 = _input11.GetTypedBlock(blockIndex);
                var b12 = _input12.GetTypedBlock(blockIndex);
                var b13 = _input13.GetTypedBlock(blockIndex);
                var b14 = _input14.GetTypedBlock(blockIndex);
                var b15 = _input15.GetTypedBlock(blockIndex);
                var b16 = _input16.GetTypedBlock(blockIndex);
                var b17 = _input17.GetTypedBlock(blockIndex);
                var b18 = _input18.GetTypedBlock(blockIndex);
                var b19 = _input19.GetTypedBlock(blockIndex);
                var b20 = _input20.GetTypedBlock(blockIndex);
                await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15, b16, b17, b18, b19, b20);
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
                var block11 = b11.Result;
                var block12 = b12.Result;
                var block13 = b13.Result;
                var block14 = b14.Result;
                var block15 = b15.Result;
                var block16 = b16.Result;
                var block17 = b17.Result;
                var block18 = b18.Result;
                var block19 = b19.Result;
                var block20 = b20.Result;
                if (block1.Length != block2.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block3.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block4.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block5.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block6.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block7.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block8.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block9.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block10.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block11.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block12.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block13.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block14.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block15.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block16.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block17.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block18.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block19.Length)
                    throw new Exception("Expected all blocks to have same size");
                if (block1.Length != block20.Length)
                    throw new Exception("Expected all blocks to have same size");
                var ret = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>[block1.Length];
                Copy(block1.Span, block2.Span, block3.Span, block4.Span, block5.Span, block6.Span, block7.Span, block8.Span, block9.Span, block10.Span, block11.Span, block12.Span, block13.Span, block14.Span, block15.Span, block16.Span, block17.Span, block18.Span, block19.Span, block20.Span, ret);
                return ret;
            }

            static void Copy(ReadOnlySpan<T1> span1, ReadOnlySpan<T2> span2, ReadOnlySpan<T3> span3, ReadOnlySpan<T4> span4, ReadOnlySpan<T5> span5, ReadOnlySpan<T6> span6, ReadOnlySpan<T7> span7, ReadOnlySpan<T8> span8, ReadOnlySpan<T9> span9, ReadOnlySpan<T10> span10, ReadOnlySpan<T11> span11, ReadOnlySpan<T12> span12, ReadOnlySpan<T13> span13, ReadOnlySpan<T14> span14, ReadOnlySpan<T15> span15, ReadOnlySpan<T16> span16, ReadOnlySpan<T17> span17, ReadOnlySpan<T18> span18, ReadOnlySpan<T19> span19, ReadOnlySpan<T20> span20, Span<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>> output)
            {
                for (var i = 0; i < span1.Length; i++)
                    output[i] = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(span1[i], span2[i], span3[i], span4[i], span5[i], span6[i], span7[i], span8[i], span9[i], span10[i], span11[i], span12[i], span13[i], span14[i], span15[i], span16[i], span17[i], span18[i], span19[i], span20[i]);
            }

            public async IAsyncEnumerable<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>> EnumerateAllTyped()
            {
                const int size = 20;
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
                var e11 = _input11.GetAsyncEnumerator();
                var e12 = _input12.GetAsyncEnumerator();
                var e13 = _input13.GetAsyncEnumerator();
                var e14 = _input14.GetAsyncEnumerator();
                var e15 = _input15.GetAsyncEnumerator();
                var e16 = _input16.GetAsyncEnumerator();
                var e17 = _input17.GetAsyncEnumerator();
                var e18 = _input18.GetAsyncEnumerator();
                var e19 = _input19.GetAsyncEnumerator();
                var e20 = _input20.GetAsyncEnumerator();
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
                    currentTasks[10] = e11.MoveNextAsync();
                    currentTasks[11] = e12.MoveNextAsync();
                    currentTasks[12] = e13.MoveNextAsync();
                    currentTasks[13] = e14.MoveNextAsync();
                    currentTasks[14] = e15.MoveNextAsync();
                    currentTasks[15] = e16.MoveNextAsync();
                    currentTasks[16] = e17.MoveNextAsync();
                    currentTasks[17] = e18.MoveNextAsync();
                    currentTasks[18] = e19.MoveNextAsync();
                    currentTasks[19] = e20.MoveNextAsync();
                    for (var i = 0; i < size; i++) {
                        if (await currentTasks[i] != true) {
                            isValid = false;
                            break;
                        }
                    }
                    var row = new Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current, e17.Current, e18.Current, e19.Current, e20.Current);
                    yield return row;
                }
            }

            public IAsyncEnumerator<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>> GetAsyncEnumerator(CancellationToken ct = default) => EnumerateAllTyped().GetAsyncEnumerator(ct);
        }
        public IReadOnlyBuffer<Row<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>> GetRowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(
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
            uint columnIndex11 = 10,
            uint columnIndex12 = 11,
            uint columnIndex13 = 12,
            uint columnIndex14 = 13,
            uint columnIndex15 = 14,
            uint columnIndex16 = 15,
            uint columnIndex17 = 16,
            uint columnIndex18 = 17,
            uint columnIndex19 = 18,
            uint columnIndex20 = 19
        ) 
             where T1: notnull  where T2: notnull  where T3: notnull  where T4: notnull  where T5: notnull  where T6: notnull  where T7: notnull  where T8: notnull  where T9: notnull  where T10: notnull  where T11: notnull  where T12: notnull  where T13: notnull  where T14: notnull  where T15: notnull  where T16: notnull  where T17: notnull  where T18: notnull  where T19: notnull  where T20: notnull
        {

            return new RowReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(GetColumn<T1>(columnIndex1), GetColumn<T2>(columnIndex2), GetColumn<T3>(columnIndex3), GetColumn<T4>(columnIndex4), GetColumn<T5>(columnIndex5), GetColumn<T6>(columnIndex6), GetColumn<T7>(columnIndex7), GetColumn<T8>(columnIndex8), GetColumn<T9>(columnIndex9), GetColumn<T10>(columnIndex10), GetColumn<T11>(columnIndex11), GetColumn<T12>(columnIndex12), GetColumn<T13>(columnIndex13), GetColumn<T14>(columnIndex14), GetColumn<T15>(columnIndex15), GetColumn<T16>(columnIndex16), GetColumn<T17>(columnIndex17), GetColumn<T18>(columnIndex18), GetColumn<T19>(columnIndex19), GetColumn<T20>(columnIndex20));
        }
    }
}