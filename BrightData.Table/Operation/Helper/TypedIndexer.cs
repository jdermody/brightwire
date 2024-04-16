using BrightData.Converter;
using BrightData.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table.Operation.Helper
{
    internal class TypedIndexer<T> : IOperation, ICanIndex<T>, IAcceptBlock<T> where T : notnull
    {
        readonly BufferScan<T> _scan;
        readonly Dictionary<T, uint> _index = new();

        public TypedIndexer(IReadOnlyBuffer<T> buffer)
        {
            _scan = new(buffer, this, null);
        }

        public void Add(ReadOnlySpan<T> block)
        {
            foreach (var item in block)
                _index.TryAdd(item, (uint)_index.Count);
        }

        public Task Process(INotifyUser? notify = null, string? msg = null, CancellationToken ct = default) => _scan.Process(notify, msg, ct);
        public uint GetIndex(in T item) => _index[item];
        public uint Size => (uint)_index.Count;
    }
}
