using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2.Bindings
{
    public interface IConvertStructsToObjects<CT, out T> where CT: unmanaged where T: notnull
    {
        T Convert(ref CT item);
    }

    public delegate Type ConvertStructsToObjectsDelegate<CT, out T>(ref CT item) where CT : unmanaged where T : notnull;
    public class ColumnReader<CT, T> : ICanEnumerateDisposable<T>, ICanEnumerateDisposable, IEnumerable<T>
        where CT : unmanaged
        where T : notnull
    {
        class Enumerator : IEnumerator<T>
        {
            readonly IReadOnlyEnumerator<CT> _structEnumerator;
            readonly IConvertStructsToObjects<CT, T> _converter;

            public Enumerator(IReadOnlyEnumerator<CT> structEnumerator, IConvertStructsToObjects<CT, T> converter)
            {
                _structEnumerator = structEnumerator;
                _structEnumerator.Reset();
                _converter = converter;
            }

            public bool MoveNext() => _structEnumerator.MoveNext();
            public void Reset() => _structEnumerator.Reset();
            public T Current => _converter.Convert(ref _structEnumerator.Current);
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                // nop - struct enumerator is disposed in outer scope
            }
        }

        readonly IConvertStructsToObjects<CT, T> _converter;
        readonly IDisposable _stream;
        readonly IReadOnlyEnumerator<CT> _enumerator;

        public ColumnReader(IReadOnlyEnumerator<CT> enumerator, IConvertStructsToObjects<CT, T> converter, IDisposable stream)
        {
            _converter = converter;
            _stream = stream;
            _enumerator = enumerator;
        }

        /// <inheritdoc />
        public IEnumerable<T> EnumerateTyped() => this;

        public IEnumerator<T> GetEnumerator() => new Enumerator(_enumerator, _converter);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            //_enumerator.Dispose();
            _stream.Dispose();
        }

        public IEnumerable<object> Enumerate()
        {
            foreach (var item in EnumerateTyped())
                yield return item;
        }
    }
}
