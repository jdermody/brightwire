using System;
using System.Collections;
using System.Collections.Generic;

namespace BrightData.DataTable
{
    public class SequentialColumnReader<CT, T> : ICanEnumerateDisposable<T>, ICanEnumerateDisposable, IEnumerable<T>
        where CT : unmanaged
        where T : notnull
    {
        class Enumerator : IEnumerator<T>
        {
            readonly IReadOnlyUnmanagedEnumerator<CT>         _structEnumerator;
            readonly IHaveMutableReference<CT>       _mutableReference;
            readonly IConvertStructsToObjects<CT, T> _converter;

            public Enumerator(IReadOnlyUnmanagedEnumerator<CT> structEnumerator, IConvertStructsToObjects<CT, T> converter)
            {
                _structEnumerator = structEnumerator;
                _mutableReference = (IHaveMutableReference<CT>)_structEnumerator;
                _structEnumerator.Reset();
                _converter = converter;
            }

            public bool MoveNext() => _structEnumerator.MoveNext();
            public void Reset() => _structEnumerator.Reset();
            public T Current => _converter.Convert(in _mutableReference.Current);
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                // nop - struct enumerator is disposed in outer scope
            }
        }

        readonly IConvertStructsToObjects<CT, T> _converter;
        readonly IDisposable                     _stream;
        readonly IReadOnlyUnmanagedEnumerator<CT>         _enumerator;

        public SequentialColumnReader(IReadOnlyUnmanagedEnumerator<CT> enumerator, IConvertStructsToObjects<CT, T> converter, IDisposable stream)
        {
            _converter = converter;
            _stream = stream;
            _enumerator = enumerator;
        }

        /// <inheritdoc />
        public IEnumerable<T> Values => this;

        public IEnumerator<T> GetEnumerator() => new Enumerator(_enumerator, _converter);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            _enumerator.Dispose();
            _stream.Dispose();
        }

        IEnumerable<object> ICanEnumerate.Values
        {
            get
            {
                foreach (var item in Values)
                    yield return item;
            }
        }
    }
}
