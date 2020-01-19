namespace BrightData.Analysis
{
    public class StringAnalyser : FrequencyAnalyser<string>
    {
        int _minLength = int.MaxValue, _maxLength = int.MinValue;

        public StringAnalyser(int writeCount = 100) : base(writeCount)
        {
        }

        public override void Add(string str)
        {
            if (str != null) {
                _Add(str);
                var len = str.Length;
                if (len < _minLength)
                    _minLength = len;
                if (len > _maxLength)
                    _maxLength = len;
            }
        }

        public override void WriteTo(IMetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.Set(Consts.MinLength, _minLength);
            metadata.Set(Consts.MaxLength, _maxLength);
            base.WriteTo(metadata);
        }
    }
}
