namespace BrightData.Analysis
{
    class StringAnalyser : FrequencyAnalyser<string>
    {
        uint _minLength = uint.MaxValue, _maxLength = uint.MinValue;

        public StringAnalyser(uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct) : base(writeCount, maxCount)
        {
        }

        public override void Add(string str)
        {
            if (str != null) {
                _Add(str);
                var len = (uint)str.Length;
                if (len < _minLength)
                    _minLength = len;
                if (len > _maxLength)
                    _maxLength = len;
            }
        }

        public override void WriteTo(IMetaData metadata)
        {
            base.WriteTo(metadata);
            metadata.Set(Consts.MinLength, _minLength);
            metadata.Set(Consts.MaxLength, _maxLength);
        }
    }
}
