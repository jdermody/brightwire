using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2.Bindings
{
    public interface IConvertStructsToObjects<CT, out T> where CT: struct where T: notnull
    {
        T Convert(ref CT item);
    }
    public class ColumnReaderBinding<CT, T> : ICanEnumerateDisposable<T>, IEnumerable<T>
        where CT : struct
        where T : notnull
    {
        class Enumerator : IEnumerator<T>
        {
            readonly IStructEnumerator<CT> _structEnumerator;
            readonly IConvertStructsToObjects<CT, T> _converter;

            public Enumerator(IStructEnumerator<CT> structEnumerator, IConvertStructsToObjects<CT, T> converter)
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

        readonly BrightDataTable _dataTable;
        readonly IConvertStructsToObjects<CT, T> _converter;
        readonly IStructEnumerator<CT> _enumerator;

        public ColumnReaderBinding(IStructEnumerator<CT> enumerator, IConvertStructsToObjects<CT, T> converter)
        {
            _converter = converter;
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
            _enumerator.Dispose();
        }
    }
}
