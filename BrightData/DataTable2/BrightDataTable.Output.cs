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
        public void WriteColumnsTo(Stream stream, IEnumerable<uint> columnIndices)
        {
            var columns = GetColumns(columnIndices).ToArray();
            WriteTo(columns, stream);
        }
        void WriteTo(ISingleTypeTableSegment[] columns, Stream stream) => Context.WriteDataTable(TableMetaData, columns, stream);

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
            var data = GetAllRowData();
            foreach (var other in tables) {
                if (other.ColumnCount != ColumnCount)
                    throw new ArgumentException("Columns must agree - column count was different");
                if (ColumnTypes.Zip(other.ColumnTypes, (t1, t2) => t1 == t2).Any(v => v == false))
                    throw new ArgumentException("Columns must agree - types were different");

                rowCount += other.RowCount;
                data = data.Concat(other.GetAllRowData(true));
            }

            var tempStream = Context.CreateTempStreamProvider();
            var buffers = ColumnMetaData.Select((m, ci) => ColumnTypes[ci].GetHybridBufferWithMetaData(m, Context, tempStream)).ToArray();

            return new WriteRowsOperation(
                Context,
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

        public IOperation<Stream?> WriteRowsTo(Stream stream, params uint[] rowIndices)
        {
            var tempStream = Context.CreateTempStreamProvider();
            var buffers = ColumnMetaData.Select((m, ci) => ColumnTypes[ci].GetHybridBufferWithMetaData(m, Context, tempStream)).ToArray();

            return new WriteRowsOperation(
                Context,
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

        public IOperation<Stream?> WriteRowsTo(Stream stream, Predicate<object[]> predicate, params uint[] rowIndices)
        {
            var tempStream = Context.CreateTempStreamProvider();
            var buffers = ColumnMetaData.Select((m, ci) => ColumnTypes[ci].GetHybridBufferWithMetaData(m, Context, tempStream)).ToArray();

            return new WriteRowsOperation(
                Context,
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

        public IOperation<Stream?> Bag(uint sampleCount, Stream stream)
        {
            var rowIndices = AllRowIndices.ToArray().Bag(sampleCount, Context.Random);
            return WriteRowsTo(stream, rowIndices);
        }

        public IOperation<Stream?> Shuffle(Stream stream)
        {
            var rowIndices = AllRowIndices.Shuffle(Context.Random).ToArray();
            return WriteRowsTo(stream, rowIndices);
        }
    }
}
