using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.Buffer.Operations.Conversion
{
    /// <summary>
    /// Maps each item to a consistent index (category)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ToCategoricalIndexConversion<T> : ConversionBase<T, int> where T : notnull
    {
        readonly Dictionary<string, int> _categoryIndex = [];
        readonly IHaveMetaData _metaData;

        public ToCategoricalIndexConversion(IReadOnlyBuffer<T> input, IAppendToBuffer<int> output) : base(input, output)
        {
            _metaData = output as IHaveMetaData ?? throw new ArgumentNullException(nameof(output), "Expected to be able to write to meta data");
            _onComplete = WriteMetadata;
        }

        void WriteMetadata()
        {
            var metaData = _metaData.MetaData;
            metaData.Set(Consts.IsNumeric, true);
            metaData.SetType(BrightDataType.Int);
            metaData.SetIsCategorical(true);

            foreach (var category in _categoryIndex.OrderBy(d => d.Value))
                metaData.Set(Consts.CategoryPrefix + category.Value, category.Key);
        }

        protected override int Convert(T from)
        {
            var str = from.ToString() ?? string.Empty;
            if(!_categoryIndex.TryGetValue(str, out var ret))
                _categoryIndex.Add(str, ret = _categoryIndex.Count);
            return ret;
        }
    }
}
