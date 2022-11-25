using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.DataTable.Operations;

namespace BrightData.DataTable
{
    public partial class BrightDataTable
    {
        /// <summary>
        /// Writes table to a file
        /// </summary>
        /// <param name="path">File path</param>
        public void WriteTo(string path)
        {
            using var file = new FileStream(path, FileMode.Create, FileAccess.Write);
            WriteColumnsTo(file);
        }

        /// <summary>
        /// Writes specified columns to a stream
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="columnIndices">Column indices to write (or all columns if none specified)</param>
        public void WriteColumnsTo(Stream stream, params uint[] columnIndices)
        {
            var columns = GetColumns(AllOrSpecifiedColumnIndices(columnIndices)).ToArray();
            WriteTo(columns, stream);
        }

        /// <summary>
        /// Writes specified columns to a stream
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="columnIndices">Specified column indices</param>
        public void WriteColumnsTo(Stream stream, IEnumerable<uint> columnIndices)
        {
            var columns = GetColumns(columnIndices).ToArray();
            WriteTo(columns, stream);
        }
        void WriteTo(ITypedSegment[] columns, Stream stream) => Context.WriteDataTable(TableMetaData, columns, stream);

        /// <summary>
        /// Horizontally concatenates this data table with other data tables into a stream
        /// (Row counts must agree)
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="tables">Other tables to concatenate </param>
        /// <exception cref="ArgumentException"></exception>
        public void ConcatenateColumns(Stream stream, params BrightDataTable[] tables)
        {
            if (tables.Any(t => t.RowCount != RowCount))
                throw new ArgumentException("Row count across tables must agree");

            var columns = GetAllColumns().Concat(tables.SelectMany(t => t.GetAllColumns())).ToArray();
            WriteTo(columns, stream);
        }

        /// <summary>
        /// Vertically concatenates this data table with other data tables into a stream
        /// (Columns must be the same)
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="tables">Other tables to concatenate</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IOperation<Stream?> ConcatenateRows(Stream stream, params BrightDataTable[] tables)
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
            var buffers = ColumnMetaData
                .Select((m, ci) => ColumnTypes[ci].GetHybridBufferWithMetaData(m, Context, tempStream))
                .Cast<IHybridBuffer>()
                .ToArray()
            ;

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

        /// <summary>
        /// Writes a subset of rows to a stream
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="rowIndices">Row indices to write (or all if none specified)</param>
        /// <returns></returns>
        public IOperation<Stream?> WriteRowsTo(Stream stream, params uint[] rowIndices)
        {
            var tempStream = Context.CreateTempStreamProvider();
            var buffers = ColumnMetaData
                .Select((m, ci) => ColumnTypes[ci].GetHybridBufferWithMetaData(m, Context, tempStream))
                .Cast<IHybridBuffer>()
                .ToArray()
            ;
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

        /// <summary>
        /// Writes rows that satisfy a predicate to a stream
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="predicate">Row filter</param>
        /// <param name="rowIndices">Row indices to write (or all if none specified)</param>
        /// <returns></returns>
        public IOperation<Stream?> WriteRowsTo(Stream stream, Predicate<BrightDataTableRow> predicate, params uint[] rowIndices)
        {
            var tempStream = Context.CreateTempStreamProvider();
            var buffers = ColumnMetaData
                .Select((m, ci) => ColumnTypes[ci].GetHybridBufferWithMetaData(m, Context, tempStream))
                .Cast<IHybridBuffer>()
                .ToArray()
            ;
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
