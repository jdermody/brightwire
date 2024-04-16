using System;

namespace BrightData.DataTable.Rows
{
    /// <summary>
    /// Base class for table rows
    /// </summary>
    /// <param name="Table"></param>
    /// <param name="RowIndex"></param>
    public abstract record TableRowBase(IDataTable Table, uint RowIndex) : IHaveSize, IHaveSingleIndex
    {
        /// <inheritdoc />
        public abstract uint Size { get; }

        uint IHaveSingleIndex.Index => RowIndex;

        /// <summary>
        /// Gets a value from the row
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public T Get<T>(uint columnIndex)
        {
            var ret = Get(columnIndex);
            if (ret.GetType() != typeof(T))
            {
                if (typeof(T) == typeof(string))
                {
                    var str = ret.ToString();
                    return __refvalue(__makeref(str), T);
                }

                // attempt to convert the type
                try
                {
                    return (T)Convert.ChangeType(ret, typeof(T));
                }
                catch
                {
                    throw new InvalidCastException($"Column {columnIndex} is {ret.GetType()} but requested {typeof(T)} and could not be converted");
                }
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
        /// Retrieves a value based on column index
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        protected abstract object Get(uint columnIndex);
    }
}
