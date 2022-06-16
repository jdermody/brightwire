using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.DataTable2.Bindings;

namespace BrightData.DataTable2
{
    public partial class BrightDataTable
    {
        ICanEnumerate<T> GetReader<CT, T>(uint offset, uint sizeInBytes)
            where T: notnull
            where CT: unmanaged
        {
            var block = _buffer.GetIterator<CT>(offset, sizeInBytes);
            var converter = GetConverter<CT, T>();
            return new ColumnReader<CT, T>(block.GetEnumerator(), converter, block);
        }

        IConvertStructsToObjects<CT, T> GetConverter<CT, T>()
            where CT : unmanaged
            where T: notnull
        {
            var dataType = typeof(T);
            if (dataType == typeof(string)) {
                var ret = new StringColumnConverter(_stringTable.Value);
                return (IConvertStructsToObjects<CT, T>)ret;
            }
            return new NopColumnConverter<CT, T>();
        }

        class NopColumnConverter<CT, T> : IConvertStructsToObjects<CT, T>
            where CT : unmanaged
            where T: notnull
        {
            public T Convert(ref CT item)
            {
                return __refvalue(__makeref(item), T);
            }
        }

        class StringColumnConverter : IConvertStructsToObjects<uint, string>
        {
            readonly string[] _stringTable;

            public StringColumnConverter(string[] stringTable)
            {
                _stringTable = stringTable;
            }

            public string Convert(ref uint item) => _stringTable[item];
        }
    }
}
