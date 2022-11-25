using System.Collections.Generic;
using System.Linq;

namespace BrightData.DataTable
{
    public partial class BrightDataTable
    {
        /// <summary>
        /// Enumerates row indices
        /// </summary>
        public IEnumerable<uint> AllRowIndices => _header.RowCount.AsRange();
        
        /// <summary>
        /// Enumerates specified row indices (or all if none specified)
        /// </summary>
        /// <param name="indices">Row indices (optional)</param>
        /// <returns></returns>
        public IEnumerable<uint> AllOrSpecifiedRowIndices(params uint[]? indices) => (indices is null || indices.Length == 0)
            ? AllRowIndices
            : indices
        ;

        RandomAccessColumnReader<CT, T> GetRandomAccessColumnReader<CT, T>(uint offset, uint sizeInBytes, IConvertStructsToObjects<CT, T> converter)
            where CT : unmanaged
            where T : notnull
        {
            var block = _buffer.GetBlock<CT>(offset, sizeInBytes);
            return new RandomAccessColumnReader<CT, T>(block, converter);
        }

        ICanRandomlyAccessData[] GetColumnReaders()
        {
            var mappingGuid = TableMetaData.GetNullable<string>(Consts.CustomColumnReaders);
            if (mappingGuid is not null && Context.TryGet<ICanRandomlyAccessData[]>($"{Consts.CustomColumnReaders}:{mappingGuid}", out var readers))
                return readers;
            return DefaultColumnReaders;
        }

        /// <summary>
        /// Returns a typed value from the table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnIndex">Column index</param>
        /// <returns></returns>
        public T Get<T>(uint rowIndex, uint columnIndex) where T : notnull => (T)_columnReaders.Value[columnIndex][rowIndex];

        /// <summary>
        /// Enumerates the data from specified rows
        /// </summary>
        /// <param name="reuseArrayForEachIteration">True to reuse the same array for each iteration (allocates less memory)</param>
        /// <param name="rowIndices">Row indices to enumerate</param>
        /// <returns></returns>
        public IEnumerable<(uint RowIndex, object[] Data)> GetAllRowData(bool reuseArrayForEachIteration = true, params uint[] rowIndices)
        {
            var rowCount = _header.RowCount;
            var columnCount = _header.ColumnCount;
            var readers = GetColumnReaders(ColumnIndices);
            var enumerators = readers.Select(r => r.Values.GetEnumerator()).ToArray();
            var selectedRowIndices = rowIndices.Length > 0 ? new HashSet<uint>(rowIndices) : null;
            
            try {
                var ret = new object[columnCount];
                for (uint j = 0; j < rowCount; j++) {
                    if (selectedRowIndices?.Contains(j) == false) {
                        foreach (var item in enumerators)
                            item.MoveNext();
                    }
                    else {
                        if (!reuseArrayForEachIteration && j > 0)
                            ret = new object[columnCount];

                        for (uint i = 0; i < columnCount; i++) {
                            var enumerator = enumerators[i];
                            enumerator.MoveNext();
                            ret[i] = enumerator.Current;
                        }

                        yield return (j, ret);
                    }
                }
            }
            finally {
                foreach(var item in enumerators)
                    item.Dispose();
                foreach(var item in readers)
                    item.Dispose();
            }
        }

        /// <summary>
        /// Returns a row
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <returns></returns>
        public BrightDataTableRow GetRow(uint rowIndex) => GetRows(rowIndex).Single();

        /// <summary>
        /// Enumerates specified rows
        /// </summary>
        /// <param name="rowIndices">Row indices to return</param>
        /// <returns></returns>
        public IEnumerable<BrightDataTableRow> GetRows(IEnumerable<uint> rowIndices)
        {
            var readers = GetColumnReaders();
            foreach (var ri in rowIndices) {
                yield return new BrightDataTableRow(this, readers, ri);
            }
        }

        /// <summary>
        /// Enumerates all or specified row indices
        /// </summary>
        /// <param name="rowIndices">Row indices (optional)</param>
        /// <returns></returns>
        public IEnumerable<BrightDataTableRow> GetRows(params uint[] rowIndices)
        {
            var readers = GetColumnReaders();
            foreach (var ri in AllOrSpecifiedRowIndices(rowIndices)) {
                yield return new BrightDataTableRow(this, readers, ri);
            }
        }

        /// <summary>
        /// Enumerates all rows
        /// </summary>
        public IEnumerable<BrightDataTableRow> AllRows => GetRows();

        /// <summary>
        /// Enumerates a range of row indices
        /// </summary>
        /// <param name="offset">First inclusive row index</param>
        /// <param name="count">Number of rows to enumerate</param>
        /// <returns></returns>
        public IEnumerable<BrightDataTableRow> GetSlice(uint offset, uint count)
        {
            var readers = GetColumnReaders();
            for(uint i = 0; i < count && offset + i < RowCount; i++) {
                yield return new BrightDataTableRow(this, readers, offset + i);
            }
        }
    }
}
