using System;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// Vectorisation to single index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CategoricalIndexVectorisation<T>() : OneHotVectoriser<T>(1)
        where T : notnull
    {
        protected override void Vectorise(in T item, Span<float> buffer)
        {
            var str = item.ToString() ?? string.Empty;
            if (!_table.TryGetValue(str, out var index))
                _table.Add(str, index = (uint)_table.Count);
            buffer[0] = index;
        }

        public override VectorisationType Type => VectorisationType.CategoricalIndex;
    }
}
