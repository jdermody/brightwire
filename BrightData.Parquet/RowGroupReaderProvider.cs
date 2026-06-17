using System.Linq.Expressions;
using Parquet;
using Parquet.Schema;

namespace BrightData.Parquet
{
    internal sealed class RowGroupReaderProvider : IDisposable
    {
        readonly record struct Index(uint RowGroup, uint ColumnIndex);

        // Fix #3: Cache reflection delegates for ReadAsync<T>
        static readonly Dictionary<Type, Func<ParquetRowGroupReader, DataField, Array, CancellationToken, ValueTask>> _readAsyncCache = new();

        readonly ParquetRowGroupReader[]  _rowGroupReaders;
        readonly Dictionary<Index, Array> _columnData = new();
        readonly SemaphoreSlim            _semaphore;          // Fix #6: disposed in Dispose()
        volatile bool                     _disposed;           // Fix #7: volatile for visibility

        public RowGroupReaderProvider(ParquetReader reader)
        {
            Reader = reader;
            var rowGroups = reader.RowGroupCount;
            _rowGroupReaders = new ParquetRowGroupReader[rowGroups];
            RowGroupSizes = new uint[rowGroups];
            var totalSize = 0u;
            for (var i = 0; i < rowGroups; i++)
            {
                var rowGroupReader = _rowGroupReaders[i] = reader.OpenRowGroupReader(i);
                var rowCount = RowGroupSizes[i] = (uint)rowGroupReader.RowCount;
                totalSize += rowCount;
            }
            Size = totalSize;
            _semaphore = new SemaphoreSlim(1);
        }

        public ParquetReader Reader { get; }
        public uint[] RowGroupSizes { get; }
        public uint Size { get; }
        public uint RowGroupCount => (uint)RowGroupSizes.Length;

        public ParquetRowGroupReader GetRowGroupReader(uint rowGroup) => _rowGroupReaders[rowGroup];

        // Fix #2: Consistent disposed check + Fix #1: Extracted shared validation
        void ThrowIfDisposedOrInvalidIndex(int columnIndex)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            CheckColumnIndex(columnIndex);
        }

        void CheckColumnIndex(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= Reader.Schema.DataFields.Length)
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
        }

        public async ValueTask<T[]> GetColumn<T>(uint rowGroup, int columnIndex, CancellationToken ct)
            where T : struct
        {
            ThrowIfDisposedOrInvalidIndex(columnIndex);

            var index = new Index(rowGroup, (uint)columnIndex);
            if (_columnData.TryGetValue(index, out var ret))
                return (T[])ret;

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_columnData.TryGetValue(index, out ret))
                    return (T[])ret;

                var rowGroupReader = GetRowGroupReader(rowGroup);
                // Fix #4: Overflow check
                var rowCount = (int)rowGroupReader.RowCount;
                var data = new T[rowCount];
                await rowGroupReader.ReadAsync<T>(Reader.Schema.DataFields[columnIndex], data, null, ct).ConfigureAwait(false);
                _columnData.Add(index, data);
                return data;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask<T?[]> GetNullableColumn<T>(uint rowGroup, int columnIndex, CancellationToken ct)
            where T : struct
        {
            ThrowIfDisposedOrInvalidIndex(columnIndex);

            var index = new Index(rowGroup, (uint)columnIndex);
            if (_columnData.TryGetValue(index, out var ret))
                return (T?[])ret;

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_columnData.TryGetValue(index, out ret))
                    return (T?[])ret;

                var rowGroupReader = GetRowGroupReader(rowGroup);
                var rowCount = (int)rowGroupReader.RowCount;
                var data = new T?[rowCount];
                await rowGroupReader.ReadAsync<T>(Reader.Schema.DataFields[columnIndex], data, null, ct).ConfigureAwait(false);
                _columnData.Add(index, data);
                return data;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask<Array> GetColumn(Type targetType, uint rowGroup, int columnIndex, CancellationToken ct)
        {
            ThrowIfDisposedOrInvalidIndex(columnIndex);

            var index = new Index(rowGroup, (uint)columnIndex);
            if (_columnData.TryGetValue(index, out var ret))
                return ret;

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_columnData.TryGetValue(index, out ret))
                    return ret;

                var rowGroupReader = GetRowGroupReader(rowGroup);

                if (targetType == typeof(string))
                {
                    var rowCount = (int)rowGroupReader.RowCount;
                    var data = new string[rowCount];
                    await rowGroupReader.ReadAsync(Reader.Schema.DataFields[columnIndex], data, null, ct).ConfigureAwait(false);
                    _columnData.Add(index, data);
                    return data;
                }
                else
                {
                    // Fix #3: Use cached reflection delegate
                    var invokeRead = GetOrCreateReadAsync(targetType);
                    var rowCount = (int)rowGroupReader.RowCount;
                    var data = Array.CreateInstance(targetType, rowCount);
                    await invokeRead(rowGroupReader, Reader.Schema.DataFields[columnIndex], data, ct).ConfigureAwait(false);
                    _columnData.Add(index, data);
                    return data;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Fix #3: Cached reflection for ReadAsync<T> via Expression tree
        static Func<ParquetRowGroupReader, DataField, Array, CancellationToken, ValueTask> GetOrCreateReadAsync(Type targetType)
        {
            lock (_readAsyncCache)
            {
                if (_readAsyncCache.TryGetValue(targetType, out var cached))
                    return cached;

                cached = BuildReadAsyncDelegate(targetType);
                _readAsyncCache[targetType] = cached;
                return cached;
            }
        }

        static Func<ParquetRowGroupReader, DataField, Array, CancellationToken, ValueTask> BuildReadAsyncDelegate(Type targetType)
        {
            var readMethod = typeof(ParquetRowGroupReader).GetMethods()
                .Where(m => m.Name == "ReadAsync" && m.IsGenericMethodDefinition && m.GetParameters().Length == 4
                           && m.GetParameters()[0].ParameterType == typeof(DataField)
                           && m.GetParameters()[3].ParameterType == typeof(CancellationToken))
                .FirstOrDefault(m =>
                {
                    var p1 = m.GetParameters()[1].ParameterType;
                    return p1.IsGenericType && p1.GetGenericTypeDefinition() == typeof(Memory<>);
                });

            if (readMethod == null)
                throw new InvalidOperationException($"Could not find ReadAsync method for type {targetType}");

            var constructedMethod = readMethod.MakeGenericMethod(targetType);
            var memoryType = typeof(Memory<>).MakeGenericType(targetType);
            var memoryCtor = memoryType.GetConstructor(new[] { typeof(Array) })
                ?? throw new InvalidOperationException($"Could not find Memory<{targetType.Name}> constructor");

            var readerParam = Expression.Parameter(typeof(ParquetRowGroupReader), "reader");
            var fieldParam = Expression.Parameter(typeof(DataField), "field");
            var dataParam = Expression.Parameter(typeof(Array), "data");
            var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

            var memoryInstance = Expression.New(memoryCtor, dataParam);
            var methodCall = Expression.Call(
                readerParam, constructedMethod,
                fieldParam,
                memoryInstance,
                Expression.Default(typeof(Memory<int>)),
                ctParam);

            var lambda = Expression.Lambda<Func<ParquetRowGroupReader, DataField, Array, CancellationToken, ValueTask>>(
                methodCall, readerParam, fieldParam, dataParam, ctParam);

            return lambda.Compile();
        }

        public void Dispose()
        {
            _disposed = true;
            foreach (var item in _rowGroupReaders)
                item.Dispose();
            // Fix #6: Dispose semaphore
            _semaphore.Dispose();
        }
    }
}
