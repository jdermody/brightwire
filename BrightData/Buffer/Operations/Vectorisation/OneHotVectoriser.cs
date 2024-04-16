using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Types;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// One hot encoding vectorisation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maxSize"></param>
    internal class OneHotVectoriser<T>(uint maxSize) : VectorisationBase<T>(maxSize)
        where T : notnull
    {
        protected readonly Dictionary<string, uint> _table = [];

        protected override void Vectorise(in T item, Span<float> buffer)
        {
            var str = item.ToString() ?? string.Empty;
            if (!_table.TryGetValue(str, out var index)) {
                _table.Add(str, index = (uint)_table.Count);
                if (index >= buffer.Length)
                    throw new Exception("Expected max size of vectorisation to be able to handle all possible values");
            }
            buffer[(int)index] = 1f;
        }

        public override string? ReverseVectorise(uint index) => _table.Where(x => x.Value == index).Select(x => x.Key).FirstOrDefault();

        public override VectorisationType Type => VectorisationType.OneHot;

        public override void WriteTo(MetaData metaData)
        {
            metaData.SetIsOneHot(true);
            foreach (var item in _table.OrderBy(d => d.Value))
                metaData.Set(Consts.CategoryPrefix + item.Value, item.Key);
        }

        public override void ReadFrom(MetaData metaData)
        {
            uint index = 0;
            foreach (var key in metaData.GetStringsWithPrefix(Consts.CategoryPrefix)) {
                var item = key[Consts.CategoryPrefix.Length..];
                _table.Add(item, index++);
            }
        }
    }
}
