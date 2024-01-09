using System;
using System.Linq;

namespace BrightData.DataTable
{
    /// <summary>
    /// Generic table row
    /// </summary>
    /// <param name="Table"></param>
    /// <param name="RowIndex"></param>
    /// <param name="Values"></param>
    public readonly record struct TableRow(IDataTable Table, uint RowIndex, object[] Values) : ICanRandomlyAccessData
    {
        /// <summary>
        /// Number of columns in row
        /// </summary>
        public uint Size => (uint)Values.Length;

        /// <summary>
        /// Gets a value from the row
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public T Get<T>(uint columnIndex)
        {
            var ret = Values[columnIndex];
            if (ret.GetType() != typeof(T)) {
                if (typeof(T) == typeof(string)) {
                    var str = ret.ToString();
                    return __refvalue(__makeref(str), T);
                }

                throw new InvalidCastException($"Column {columnIndex} is {ret.GetType()} but requested {typeof(T)}");
            }

            return (T)ret;
        }

        /// <summary>
        /// Gets many values from the row
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public T[] GetMany<T>(params uint[] columnIndices)
        {
            var ret = new T[columnIndices.Length];
            var index = 0;
            foreach (var columnIndex in columnIndices)
                ret[index++] = Get<T>(columnIndex);
            return ret;
        }

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
        public override string ToString() => String.Join('|', Values.Select(x => x.ToString()));
    }
}
