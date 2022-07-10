using System.Collections.Generic;
using System.Linq;

namespace BrightData.DataTable
{
    public partial class BrightDataTable
    {
        public IEnumerable<uint> AllRowIndices => _header.RowCount.AsRange();
        public IEnumerable<uint> AllOrSpecifiedRowIndices(uint[]? indices) => (indices is null || indices.Length == 0)
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

        public T Get<T>(uint rowIndex, uint columnIndex) where T : notnull => (T)_columnReaders.Value[columnIndex][rowIndex];

        public IEnumerable<(uint RowIndex, object[] Data)> GetAllRowData(bool reuseArrayForEachIteration = true, params uint[] rowIndices)
        {
            var rowCount = _header.RowCount;
            var columnCount = _header.ColumnCount;
            var readers = GetColumnReaders(ColumnIndices);
            var enumerators = readers.Select(r => r.Enumerate().GetEnumerator()).ToArray();
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

        public BrightDataTableRow GetRow(uint rowIndex) => GetRows(rowIndex).Single();
        public IEnumerable<BrightDataTableRow> GetRows(params uint[] rowIndices)
        {
            var readers = _columnReaders.Value;
            foreach (var ri in AllOrSpecifiedRowIndices(rowIndices)) {
                yield return new BrightDataTableRow(this, readers, ri);
            }
        }

        public IEnumerable<BrightDataTableRow> AllRows => GetRows();

        public IEnumerable<BrightDataTableRow> GetSlice(uint offset, uint count)
        {
            var readers = _columnReaders.Value;
            for(uint i = offset; i < count; i++) {
                yield return new BrightDataTableRow(this, readers, i);
            }
        }
    }
}
