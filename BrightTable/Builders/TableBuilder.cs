using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightTable.Builders
{
    public class TableBuilder
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

        public IRowOrientedDataTable Build()
        {
            using var builder = new RowOrientedTableBuilder((uint)_rows.Count);
            foreach (var column in _columns)
                builder.AddColumn(column.Type, column.MetaData);
            foreach(var row in _rows)
                builder.AddRow(row);
            return builder.Build(Context);
        }
    }
}
