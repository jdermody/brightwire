using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2
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
                return _converter.Convert(ref val);
            }
        }
        public T this[uint index]
        {
            get
            {
                _randomAccess.Get(index, out var val);
                return _converter.Convert(ref val);
            }
        }
        object ICanRandomlyAccessData.this[uint index]
        {
            get
            {
                _randomAccess.Get(index, out var val);
                return _converter.Convert(ref val);
            }
        }
        object ICanRandomlyAccessData.this[int index]
        {
            get
            {
                _randomAccess.Get(index, out var val);
                return _converter.Convert(ref val);
            }
        }
    }
}
