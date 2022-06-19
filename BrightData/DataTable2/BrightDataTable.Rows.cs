using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.DataTable;

namespace BrightData.DataTable2
{
    public partial class BrightDataTable
    {
        IEnumerable<uint> AllRowIndices => _header.RowCount.AsRange();

        IEnumerable<uint> AllOrSpecifiedRowIndices(uint[]? indices) => (indices is null || indices.Length == 0)
            ? AllRowIndices
            : indices
        ;

        public IEnumerable<(uint RowIndex, object[] Data)> GetAllRowData(bool reuseArrayForEachIteration = true)
        {
            var rowCount = _header.RowCount;
            var columnCount = _header.ColumnCount;
            var readers = GetColumnReaders(ColumnIndices);
            var enumerators = readers.Select(r => r.Enumerate().GetEnumerator()).ToArray();
            
            try {
                var ret = new object[columnCount];
                for (uint j = 0; j < rowCount; j++) {
                    if(!reuseArrayForEachIteration && j > 0)
                        ret = new object[columnCount];

                    for (uint i = 0; i < columnCount; i++) {
                        var enumerator = enumerators[i];
                        enumerator.MoveNext();
                        ret[i] = enumerator.Current;
                    }

                    yield return (j, ret);
                }
            }
            finally {
                foreach(var item in enumerators)
                    item.Dispose();
                foreach(var item in readers)
                    item.Dispose();
            }
        }

        public IDataTableRow GetRow(uint rowIndex) => GetRows(rowIndex).Single();

        public IEnumerable<IDataTableRow> GetRows(params uint[] rowIndices)
        {
            var columnCount = _header.ColumnCount;
            var readers = new ICanEnumerateDisposable[columnCount];
            var enumerators = new IEnumerator<object>[columnCount];

            foreach (var rowIndexRange in AllOrSpecifiedRowIndices(rowIndices).FindDistinctContiguousRanges()) {
                try {
                    var firstRowIndex = rowIndexRange.First;
                    var rowIndexCount = rowIndexRange.Last - firstRowIndex;
                    for (uint i = 0; i < columnCount; i++) {
                        var reader = readers[i] = GetColumnReader(i, rowIndexCount, size => size * firstRowIndex);
                        enumerators[i] = reader.Enumerate().GetEnumerator();
                    }

                    var ret = ArrayPool<object>.Shared.Rent((int)columnCount);
                    for (var j = firstRowIndex; j < rowIndexCount; j++) {
                        for (uint i = 0; i < columnCount; i++) {
                            var enumerator = enumerators[i];
                            enumerator.MoveNext();
                            ret[i] = enumerator.Current;
                        }

                        yield return new Row2(this, ret, j);
                    }
                }
                finally {
                    foreach(var item in enumerators)
                        item.Dispose();
                    foreach(var item in readers)
                        item.Dispose();
                }
            }
        }

        //public object[] GetRowData(uint rowIndex)
        //{
        //    var columnCount = _header.ColumnCount;
        //    var readers = new ICanEnumerateDisposable[columnCount];
        //    try {
        //        for (uint i = 0; i < columnCount; i++)
        //            readers[i] = GetColumnReader(i, 1, size => size * rowIndex);
        //        return readers.Select(r => r.Enumerate().First()).ToArray();
        //    }
        //    finally {
        //        foreach(var item in readers)
        //            item.Dispose();
        //    }
        //}
    }
}
