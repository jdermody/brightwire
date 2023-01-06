using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.DataTable
{
    /// <summary>
    /// A row in a data table
    /// </summary>
    public class BrightDataTableRow : ICanRandomlyAccessData
    {
        readonly BrightDataTable _dataTable;
        readonly ICanRandomlyAccessData[] _data;

        internal BrightDataTableRow(BrightDataTable dataTable, ICanRandomlyAccessData[] data, uint rowIndex)
        {
            _data = data;
            RowIndex = rowIndex;
            _dataTable = dataTable;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            // nop
        }

        /// <summary>
        /// Returns a value from the row
        /// </summary>
        /// <param name="index">Column index</param>
        /// <returns></returns>
        public object this[int index] => _data[index][RowIndex];

        /// <summary>
        /// Returns a value from the row
        /// </summary>
        /// <param name="index">Column index</param>
        /// <returns></returns>
        public object this[uint index] => _data[index][RowIndex];

        /// <summary>
        /// Data types of the row
        /// </summary>
        public BrightDataType[] Types => _dataTable.ColumnTypes;

        /// <summary>
        /// Number of values within the row
        /// </summary>
        public uint Size => (uint)_data.Length;

        /// <summary>
        /// Enumerates row values
        /// </summary>
        public IEnumerable<object> Data => _data.Select(x => x[RowIndex]);

        /// <inheritdoc />
        public override string ToString() => string.Join(",", Types.Zip(Data, (t, d) => $"{Format(d, t)} [{t}]"));

        static string Format(object obj, BrightDataType type)
        {
            if (type == BrightDataType.String)
                return $"\"{obj}\"";
            return obj.ToString() ?? "???";
        }

        /// <summary>
        /// Returns a value (converted to type T)
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
