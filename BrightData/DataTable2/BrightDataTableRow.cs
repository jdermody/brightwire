using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.DataTable2
{
    public class BrightDataTableRow : IDataTableSegment
    {
        readonly BrightDataTable _dataTable;
        readonly ICanRandomlyAccessData[] _data;

        public BrightDataTableRow(BrightDataTable dataTable, ICanRandomlyAccessData[] data, uint rowIndex)
        {
            _data = data;
            RowIndex = rowIndex;
            _dataTable = dataTable;
        }

        public object this[uint index] => _data[index][RowIndex];
        public BrightDataType[] Types => _dataTable.ColumnTypes;
        public uint Size => (uint)_data.Length;
        public IEnumerable<object> Data => _data.Select(x => x[RowIndex]);

        public override string ToString()
        {
            return string.Join(",", Types.Zip(Data, (t, d) => $"{Format(d, t)} [{t}]"));
        }

        static string Format(object obj, BrightDataType type)
        {
            if (type == BrightDataType.String)
                return $"\"{obj}\"";
            return obj.ToString() ?? "???";
        }

        /// <summary>
        /// Returns a value (dynamic conversion to type T)
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="index">Column index</param>
        /// <returns></returns>
        public T Get<T>(uint index) where T : notnull
        {
            return _dataTable.ConvertObjectTo<T>(index, this[index]);
        }

        /// <summary>
        /// Row index
        /// </summary>
        public uint RowIndex { get; }
    }
}
