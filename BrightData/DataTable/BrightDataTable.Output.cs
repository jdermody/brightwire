using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.DataTable.Operations;

namespace BrightData.DataTable
{
    public partial class BrightDataTable
    {
        public void WriteColumnsTo(Stream stream, params uint[] columnIndices)
        {
            var columns = GetColumns(AllOrSpecifiedColumnIndices(columnIndices)).ToArray();
            WriteTo(columns, stream);
        }
        public void WriteColumnsTo(Stream stream, IEnumerable<uint> columnIndices)
        {
            var columns = GetColumns(columnIndices).ToArray();
            WriteTo(columns, stream);
        }
        void WriteTo(ITypedSegment[] columns, Stream stream) => Context.WriteDataTable(TableMetaData, columns, stream);

        public void ConcatenateColumns(BrightDataTable[] tables, Stream stream)
        {
            if (tables.Any(t => t.RowCount != RowCount))
                throw new ArgumentException("Row count across tables must agree");

            var columns = GetAllColumns().Concat(tables.SelectMany(t => t.GetAllColumns())).ToArray();
            WriteTo(columns, stream);
        }

        public IOperation<Stream?> ConcatenateRows(BrightDataTable[] tables, Stream stream)
        {
            var rowCount = RowCount;
            var data = AllRows;
            foreach (var other in tables) {
                if (other.ColumnCount != ColumnCount)
                    throw new ArgumentException("Columns must agree - column count was different");
                if (ColumnTypes.Zip(other.ColumnTypes, (t1, t2) => t1 == t2).Any(v => v == false))
                    throw new ArgumentException("Columns must agree - types were different");

                rowCount += other.RowCount;
                data = data.Concat(other.AllRows);
            }

            var tempStream = Context.CreateTempStreamProvider();
            var buffers = ColumnMetaData.Select((m, ci) => ColumnTypes[ci].GetHybridBufferWithMetaData(m, Context, tempStream)).ToArray();

            return new WriteRowsOperation(
                Context,
                data.GetEnumerator(),
                null,
                buffers,
                rowCount,
                tempStream,
                stream,
                TableMetaData
            );
        }

        public IOperation<Stream?> WriteRowsTo(Stream stream, params uint[] rowIndices)
        {
            var tempStream = Context.CreateTempStreamProvider();
            var buffers = ColumnMetaData.Select((m, ci) => ColumnTypes[ci].GetHybridBufferWithMetaData(m, Context, tempStream)).ToArray();
            var rowCount = rowIndices.Length > 0 ? (uint)rowIndices.Length : RowCount;

            return new WriteRowsOperation(
                Context,
                GetRows(rowIndices).GetEnumerator(),
                null,
                buffers,
                rowCount,
                tempStream,
                stream,
                TableMetaData
            );
        }

        public IOperation<Stream?> WriteRowsTo(Stream stream, Predicate<BrightDataTableRow> predicate, params uint[] rowIndices)
        {
            var tempStream = Context.CreateTempStreamProvider();
            var buffers = ColumnMetaData.Select((m, ci) => ColumnTypes[ci].GetHybridBufferWithMetaData(m, Context, tempStream)).ToArray();
            var rowCount = rowIndices.Length > 0 ? (uint)rowIndices.Length : RowCount;

            return new WriteRowsOperation(
                Context,
                GetRows(rowIndices).GetEnumerator(),
                predicate,
                buffers,
                rowCount,
                tempStream,
                stream,
                TableMetaData
            );
        }
    }
}
