using System;
using System.Collections.Generic;
using System.Text;
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
