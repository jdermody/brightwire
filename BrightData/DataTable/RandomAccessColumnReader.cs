namespace BrightData.DataTable
{
    internal class RandomAccessColumnReader<CT, T> : ICanRandomlyAccessData<T>
        where CT : unmanaged
        where T : notnull
    {
        readonly IConvertStructsToObjects<CT, T>     _converter;
        readonly ICanRandomlyAccessUnmanagedData<CT> _randomAccess;

        public RandomAccessColumnReader(ICanRandomlyAccessUnmanagedData<CT> randomAccess, IConvertStructsToObjects<CT, T> converter)
        {
            _converter    = converter;
            _randomAccess = randomAccess;
        }

        public void Dispose()
        {
            _randomAccess.Dispose();
        }

        public T this[int index]
        {
            get
            {
                _randomAccess.Get(index, out var val);
                return _converter.Convert(in val);
            }
        }
        public T this[uint index]
        {
            get
            {
                _randomAccess.Get(index, out var val);
                return _converter.Convert(in val);
            }
        }
        object ICanRandomlyAccessData.this[uint index]
        {
            get
            {
                _randomAccess.Get(index, out var val);
                return _converter.Convert(in val);
            }
        }
        object ICanRandomlyAccessData.this[int index]
        {
            get
            {
                _randomAccess.Get(index, out var val);
                return _converter.Convert(in val);
            }
        }

        public uint Size => _randomAccess.Size;
    }
}
