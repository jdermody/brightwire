using BrightData.Types;

namespace BrightData.Analysis
{
    /// <summary>
    /// String analysis
    /// </summary>
    internal class StringAnalyser(uint writeCount = Consts.MaxWriteCount) : FrequencyAnalyser<string>(writeCount)
    {
        uint _minLength = uint.MaxValue, _maxLength = uint.MinValue;

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
