using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.Helper;

namespace BrightTable.Builders
{
    public class TableBuilder : IHaveBrightDataContext
    {
        readonly List<(ColumnType Type, IMetaData MetaData)> _columns = new List<(ColumnType Type, IMetaData MetaData)>();
        readonly List<object[]> _rows = new List<object[]>();

        public IBrightDataContext Context { get; }

        public TableBuilder(IBrightDataContext context)
        {
            Context = context;
        }

        public void CopyColumnsFrom(IDataTable table, params uint[] columnIndices)
        {
            if (columnIndices == null || columnIndices.Length == 0)
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

        public IMetaData AddColumn(ColumnType type, string name)
        {
            var ret = new MetaData();
            ret.Set(Consts.Name, DataTableBase.DefaultColumnName(name, _columns.Count));
            _columns.Add((type, ret));
            return ret;
        }

        public void AddRow(params object[] data) => _rows.Add(data);

        public IRowOrientedDataTable BuildRowOriented()
        {
            using var builder = new RowOrientedTableBuilder((uint)_rows.Count);
            foreach (var column in _columns)
                builder.AddColumn(column.Type, column.MetaData);
            foreach(var row in _rows)
                builder.AddRow(row);
            return builder.Build(Context);
        }

        public IColumnOrientedDataTable BuildColumnOriented()
        {
            using var builder = new ColumnOrientedTableBuilder();
            builder.WriteHeader((uint)_columns.Count, (uint)_rows.Count);

            var tempStream = new TempStreamManager();
            var columns = _columns.Select(c => c.MetaData.GetGrowableSegment(c.Type, Context, tempStream)).ToList();
            foreach (var row in _rows) {
                for (var i = 0; i < _columns.Count; i++) {
                    var val = i < row.Length ? row[i] : _columns[i].Type.GetDefaultValue();
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
