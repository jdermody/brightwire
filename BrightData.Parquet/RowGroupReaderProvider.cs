using Parquet;
using Parquet.Schema;

namespace BrightData.Parquet
{
    internal sealed class RowGroupReaderProvider : IDisposable
    {
        readonly record struct Index(uint RowGroup, uint ColumnIndex);
        readonly ParquetRowGroupReader[]  _rowGroupReaders;
        readonly Dictionary<Index, Array> _columnData = new();
        readonly SemaphoreSlim            _semaphore = new(1);
        bool                              _disposed;

        public RowGroupReaderProvider(ParquetReader reader)
        {
            Reader = reader;
            var rowGroups = reader.RowGroupCount;
            _rowGroupReaders = new ParquetRowGroupReader[rowGroups];
            RowGroupSizes = new uint[rowGroups];
            var totalSize = 0u;
            for (var i = 0; i < rowGroups; i++) {
                var rowGroupReader = _rowGroupReaders[i] = reader.OpenRowGroupReader(i);
                var rowCount = RowGroupSizes[i] = (uint)rowGroupReader.RowCount;
                totalSize += rowCount;
            }
            Size = totalSize;
        }

        public ParquetReader Reader { get; }
        public uint[] RowGroupSizes { get; }
        public uint Size { get; }
        public uint RowGroupCount => (uint)RowGroupSizes.Length;

        public ParquetRowGroupReader GetRowGroupReader(uint rowGroup) => _rowGroupReaders[rowGroup];

        public async ValueTask<T[]> GetColumn<T>(uint rowGroup, int columnIndex, CancellationToken ct)
            where T : struct
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RowGroupReaderProvider));

            if (columnIndex < 0 || columnIndex >= Reader.Schema.DataFields.Length)
                throw new ArgumentOutOfRangeException(nameof(columnIndex));

            var index = new Index(rowGroup, (uint)columnIndex);
            if (_columnData.TryGetValue(index, out var ret))
                return (T[])ret;

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try {
                if (_columnData.TryGetValue(index, out ret))
                    return (T[])ret;
                var rowGroupReader = GetRowGroupReader(rowGroup);
                var data = new T[rowGroupReader.RowCount];
                await rowGroupReader.ReadAsync<T>(Reader.Schema.DataFields[columnIndex], data, null, ct).ConfigureAwait(false);
                _columnData.Add(index, data);
                return data;
            }
            finally {
                _semaphore.Release();
            }
        }

        public async ValueTask<T?[]> GetNullableColumn<T>(uint rowGroup, int columnIndex, CancellationToken ct)
            where T : struct
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RowGroupReaderProvider));

            if (columnIndex < 0 || columnIndex >= Reader.Schema.DataFields.Length)
                throw new ArgumentOutOfRangeException(nameof(columnIndex));

            var index = new Index(rowGroup, (uint)columnIndex);
            if (_columnData.TryGetValue(index, out var ret))
                return (T?[])ret;

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try {
                if (_columnData.TryGetValue(index, out ret))
                    return (T?[])ret;
                var rowGroupReader = GetRowGroupReader(rowGroup);
                var data = new T?[rowGroupReader.RowCount];
                await rowGroupReader.ReadAsync<T>(Reader.Schema.DataFields[columnIndex], data, null, ct).ConfigureAwait(false);
                _columnData.Add(index, data);
                return data;
            }
            finally {
                _semaphore.Release();
            }
        }

        public async ValueTask<Array> GetColumn(Type targetType, uint rowGroup, int columnIndex, CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RowGroupReaderProvider));

            if (columnIndex < 0 || columnIndex >= Reader.Schema.DataFields.Length)
                throw new ArgumentOutOfRangeException(nameof(columnIndex));

            var index = new Index(rowGroup, (uint)columnIndex);
            if (_columnData.TryGetValue(index, out var ret))
                return (Array)ret;

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try {
                if (_columnData.TryGetValue(index, out ret))
                    return (Array)ret;
                var rowGroupReader = GetRowGroupReader(rowGroup);

                if (targetType == typeof(string))
                {
                    var data = new string[rowGroupReader.RowCount];
                    await rowGroupReader.ReadAsync(Reader.Schema.DataFields[columnIndex], data, null, ct).ConfigureAwait(false);
                    _columnData.Add(index, data);
                    return data;
                }
                else
                {
                    var data = Array.CreateInstance(targetType, rowGroupReader.RowCount);
                    // Find ReadAsync<T>(DataField, Memory<T>, Memory<int>?, CancellationToken) by filtering
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
                    // Wrap array in Memory<T> because arrays don't auto-convert via reflection invoke
                    var memoryType = typeof(Memory<>).MakeGenericType(targetType);
                    var memoryInstance = Activator.CreateInstance(memoryType, data);
                    var task = (ValueTask)constructedMethod.Invoke(rowGroupReader,
                        new object[] { Reader.Schema.DataFields[columnIndex], memoryInstance, null, ct })!;
                    await task.ConfigureAwait(false);
                    _columnData.Add(index, data);
                    return data;
                }
            }
            finally {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _disposed = true;
            foreach (var item in _rowGroupReaders) {
                item.Dispose();
            }
        }
    }
}