using System;
using System.Linq;

namespace BrightData.DataTable.Rows
{
    /// <summary>
    /// Generic table row - column types are not specified
    /// </summary>
    /// <param name="Table"></param>
    /// <param name="RowIndex"></param>
    /// <param name="Values"></param>
    public record GenericTableRow(IDataTable Table, uint RowIndex, object[] Values) : TableRowBase(Table, RowIndex), ICanRandomlyAccessData
    {
        /// <summary>
        /// Number of columns in row
        /// </summary>
        public override uint Size => (uint)Values.Length;

        /// <inheritdoc />
        protected override object Get(uint columnIndex) => Values[columnIndex];

        /// <summary>
        /// Returns an indexed value from the row
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object this[int index] => Values[index];

        /// <summary>
        /// Returns an indexed value from the row
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object this[uint index] => Values[index];

        /// <inheritdoc />
        public void Dispose()
        {
            // nop
        }

        /// <inheritdoc />
        public override string ToString() => string.Join('|', Values.Select(x => x.ToString()));
    }
}
