using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Buffer.Operations.Helper
{
    /// <summary>
    /// Maintains a mapping of types to indices
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TypedIndexer<T> : IOperation, ICanIndex<T>, IAppendBlocks<T> where T : notnull
    {
        readonly BufferScan<T> _scan;
        readonly Dictionary<T, uint> _index = [];

        public TypedIndexer(IReadOnlyBuffer<T> buffer)
        {
            _scan = new(buffer, this, null);
        }

        public void Append(ReadOnlySpan<T> block)
        {
            foreach (var item in block)
                _index.TryAdd(item, (uint)_index.Count);
        }

        public Task Execute(INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default) => _scan.Execute(notify, msg, ct);
        public uint GetIndex(in T item) => _index[item];
        public uint Size => (uint)_index.Count;
    }
}
