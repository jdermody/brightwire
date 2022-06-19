using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.DataTable2.Operations;
using BrightData.Helper;

namespace BrightData.DataTable2
{
    public partial class BrightDataTable
    {
        public void WriteColumnsTo(Stream stream, params uint[] columnIndices)
        {
            var columns = GetColumns(AllOrSpecifiedColumnIndices(columnIndices)).ToArray();
            WriteTo(columns, stream);
        }

        IEnumerable<ISingleTypeTableSegment> GetAllColumns() => GetColumns(ColumnIndices);
        IEnumerable<ISingleTypeTableSegment> GetColumns(IEnumerable<uint> columnIndices)
        {
            foreach (var ci in columnIndices) {
                var brightDataType = ColumnTypes[ci];
                var columnDataType = brightDataType.GetColumnType().Type;
                var dataType = brightDataType.GetDataType();
                yield return GenericActivator.Create<ISingleTypeTableSegment>(typeof(ColumnSegment<,>).MakeGenericType(columnDataType, dataType),
                    _context,
                    dataType,
                    _header.RowCount,
                    GetColumnReader(ci, _header.RowCount),
                    GetColumnMetaData(ci)
                );
            }
        }

        void WriteTo(ISingleTypeTableSegment[] columns, Stream stream)
        {
            if (columns.Any()) {
                try {
                    using var tempStream = _context.CreateTempStreamProvider();
                    var writer = new BrightDataTableWriter(_context, tempStream, stream);
                    writer.Write(TableMetaData, columns);
                }
                finally {
                    foreach (var item in columns)
                        item.Dispose();
                }
            }
        }

        public void ConcatColumns(BrightDataTable[] tables, Stream stream)
        {
            if (tables.Any(t => t.RowCount != RowCount))
                throw new ArgumentException("Row count across tables must agree");

            var columns = GetAllColumns().Concat(tables.SelectMany(t => t.GetAllColumns())).ToArray();
            WriteTo(columns, stream);
        }

        public IOperation<bool> ConcatRows(BrightDataTable[] tables, Stream stream)
        {
            var rowCount = RowCount;
            var data = GetAllRowData(true);
            foreach (var other in tables) {
                if (other.ColumnCount != ColumnCount)
                    throw new ArgumentException("Columns must agree - column count was different");
                if (ColumnTypes.Zip(other.ColumnTypes, (t1, t2) => t1 == t2).Any(v => v == false))
                    throw new ArgumentException("Columns must agree - types were different");

                rowCount += other.RowCount;
                data = data.Concat(other.GetAllRowData(true));
            }

            var tempStream = _context.CreateTempStreamProvider();
            var buffers = ColumnMetaData.Select((m, ci) => m.GetGrowableSegment(ColumnTypes[ci], _context, tempStream)).ToArray();

            return new WriteRowsOperation(
                _context,
                data.GetEnumerator(),
                null,
                null,
                buffers,
                RowCount,
                tempStream,
                stream,
                TableMetaData
            );
        }

        public IOperation<bool> WriteRowsTo(Stream stream, params uint[] rowIndices)
        {
            var tempStream = _context.CreateTempStreamProvider();
            var buffers = ColumnMetaData.Select((m, ci) => m.GetGrowableSegment(ColumnTypes[ci], _context, tempStream)).ToArray();

            return new WriteRowsOperation(
                _context,
                GetAllRowData(true).GetEnumerator(),
                rowIndices.Length > 0 ? new HashSet<uint>(rowIndices) : null,
                null,
                buffers,
                RowCount,
                tempStream,
                stream,
                TableMetaData
            );
        }

        public IOperation<bool> WriteRowsTo(Stream stream, Predicate<object[]> predicate, params uint[] rowIndices)
        {
            var tempStream = _context.CreateTempStreamProvider();
            var buffers = ColumnMetaData.Select((m, ci) => m.GetGrowableSegment(ColumnTypes[ci], _context, tempStream)).ToArray();

            return new WriteRowsOperation(
                _context,
                GetAllRowData(true).GetEnumerator(),
                rowIndices.Length > 0 ? new HashSet<uint>(rowIndices) : null,
                predicate,
                buffers,
                RowCount,
                tempStream,
                stream,
                TableMetaData
            );
        }

        public IOperation<bool> Bag(uint sampleCount, Stream stream)
        {
            var rowIndices = AllRowIndices.ToArray().Bag(sampleCount, _context.Random);
            return WriteRowsTo(stream, rowIndices);
        }

        public IOperation<bool> Shuffle(Stream stream)
        {
            var rowIndices = AllRowIndices.Shuffle(_context.Random).ToArray();
            return WriteRowsTo(stream, rowIndices);
        }
    }
}
