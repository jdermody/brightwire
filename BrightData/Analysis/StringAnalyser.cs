using System;

namespace BrightData.Analysis
{
    /// <summary>
    /// String analysis
    /// </summary>
    internal class StringAnalyser : FrequencyAnalyser<string>
    {
        uint _minLength = uint.MaxValue, _maxLength = uint.MinValue;

        public StringAnalyser(uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct) : base(writeCount, maxCount)
        {
        }

        public override void Add(string? str)
        {
            if (str != null) {
                base.Add(str);
                var len = (uint)str.Length;
                if (len < _minLength)
                    _minLength = len;
                if (len > _maxLength)
                    _maxLength = len;
            }
        }

        public void Add(ReadOnlySpan<string> span)
        {
            foreach(var item in span)
                Add(item);
        }

        public override void WriteTo(MetaData metadata)
        {
            base.WriteTo(metadata);
            if(_minLength != uint.MaxValue)
                metadata.Set(Consts.MinLength, _minLength);
            if(_minLength != uint.MinValue)
                metadata.Set(Consts.MaxLength, _maxLength);
        }
    }
}
