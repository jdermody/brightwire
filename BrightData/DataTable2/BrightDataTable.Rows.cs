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
        public IEnumerable<uint> AllRowIndices => _header.RowCount.AsRange();

        public IEnumerable<uint> AllOrSpecifiedRowIndices(uint[]? indices) => (indices is null || indices.Length == 0)
            ? AllRowIndices
            : indices
        ;

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
            var columnCount = _header.ColumnCount;
            var readers = GetColumnReaders(ColumnIndices);

            try {
                foreach (var ri in AllOrSpecifiedRowIndices(rowIndices)) {
                    var ret = new object[columnCount];
                    for (uint i = 0; i < columnCount; i++)
                        ret[i] = readers[i].Get(ri);
                    yield return new BrightDataTableRow(this, ret, ri);
                }
            }
            finally {
                foreach(var item in readers)
                    item.Dispose();
            }
        }

        public IEnumerable<BrightDataTableRow> GetSparseRows(params uint[] rowIndices)
        {
            var columnCount = _header.ColumnCount;
            var readers = new ICanEnumerateDisposable[columnCount];
            var enumerators = new IEnumerator<object>[columnCount];

            foreach (var (first, last) in AllOrSpecifiedRowIndices(rowIndices).FindDistinctContiguousRanges()) {
                try {
                    var count = last - first + 1;
                    for (uint i = 0; i < columnCount; i++) {
                        var reader = readers[i] = GetColumnReader(i, count, size => size * first);
                        enumerators[i] = reader.Enumerate().GetEnumerator();
                    }

                    for (uint j = 0; j < count; j++) {
                        var ret = new object[columnCount];
                        for (uint i = 0; i < columnCount; i++) {
                            var enumerator = enumerators[i];
                            enumerator.MoveNext();
                            ret[i] = enumerator.Current;
                        }
                        yield return new BrightDataTableRow(this, ret, first + j);
                    }
                }
                finally {
                    enumerators.DisposeAll();
                    readers.DisposeAll();
                }
            }
        }

        public IEnumerable<BrightDataTableRow> AllRows => GetRows();

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
