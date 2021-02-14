using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Helper;

namespace BrightData.DataTable.Builders
{
    /// <summary>
    /// Builds tables dynamically in memory
    /// </summary>
    public class InMemoryTableBuilder : IHaveDataContext
    {
        readonly List<(ColumnType Type, IMetaData MetaData)> _columns = new List<(ColumnType Type, IMetaData MetaData)>();
        readonly List<object[]> _rows = new List<object[]>();

        /// <inheritdoc />
        public IBrightDataContext Context { get; }

        internal InMemoryTableBuilder(IBrightDataContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Copies column definitions from an existing table
        /// </summary>
        /// <param name="table">Table to copy from</param>
        /// <param name="columnIndices">Column indices to copy</param>
        public void CopyColumnsFrom(IDataTable table, params uint[] columnIndices)
        {
            if (columnIndices.Length == 0)
                columnIndices = table.ColumnIndices().ToArray();

            var columnSet = new HashSet<uint>(columnIndices);
            var columnTypes = table.ColumnTypes.Zip(table.ColumnMetaData(), (t, m) => (Type: t, MetaData: m))
                .Select((c, i) => (Column: c, Index: (uint) i));

            var wantedColumnTypes = columnTypes
                .Where(c => columnSet.Contains(c.Index))
                .Select(c => c.Column);

            foreach (var column in wantedColumnTypes)
                _columns.Add(column);
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="type">Type of the column</param>
        /// <param name="name">Name of the column</param>
        /// <returns></returns>
        public IMetaData AddColumn(ColumnType type, string? name = null)
        {
            var ret = new MetaData();
            ret.Set(Consts.Name, DataTableBase.DefaultColumnName(name, _columns.Count));
            _columns.Add((type, ret));
            return ret;
        }

        /// <summary>
        /// Adds a new row
        /// </summary>
        /// <param name="data"></param>
        public void AddRow(params object[] data) => _rows.Add(data);

        /// <summary>
        /// Creates a row oriented table
        /// </summary>
        /// <returns></returns>
        public IRowOrientedDataTable BuildRowOriented()
        {
            using var builder = new RowOrientedTableBuilder((uint)_rows.Count);
            foreach (var column in _columns)
                builder.AddColumn(column.Type, column.MetaData);
            foreach(var row in _rows)
                builder.AddRow(row);
            return builder.Build(Context);
        }

        /// <summary>
        /// Creates a column oriented table
        /// </summary>
        /// <returns></returns>
        public IColumnOrientedDataTable BuildColumnOriented()
        {
            using var builder = new ColumnOrientedTableBuilder();
            builder.WriteHeader((uint)_columns.Count, (uint)_rows.Count);

            var tempStream = new TempStreamManager();
            var columns = _columns.Select(c => c.MetaData.GetGrowableSegment(c.Type, Context, tempStream)).ToList();
            foreach (var row in _rows) {
                for (var i = 0; i < _columns.Count; i++) {
                    var val = i < row.Length 
                        ? row[i] 
                        : _columns[i].Type.GetDefaultValue();
                    if (val == null)
                        throw new Exception("Values cannot be null");
                    columns[i].Add(val);
                }
            }

            var segments = columns.Cast<ISingleTypeTableSegment>().ToArray();
            var columnOffsets = new List<(long Position, long EndOfColumnOffset)>();
            foreach (var segment in segments)
            {
                var position = builder.Write(segment);
                columnOffsets.Add((position, builder.GetCurrentPosition()));
            }
            builder.WriteColumnOffsets(columnOffsets);
            return builder.Build(Context);
        }
    }
}
