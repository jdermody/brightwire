using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Builders;

namespace BrightTable
{
    abstract class DataTableBase : IHaveMetaData
    {
        readonly MetaData _tableMetaData = new MetaData();
        protected const uint PREVIEW_SIZE = 10;

        protected DataTableBase(IBrightDataContext context)
        {
            Context = context;
        }

        internal static string DefaultColumnName(string name, int numColumns)
        {
            return name ?? $"Column {numColumns + 1}";
        }

        public IBrightDataContext Context { get; }
        public uint RowCount { get; protected set; }
        public uint ColumnCount { get; protected set; }
        public IMetaData MetaData => _tableMetaData;

        public abstract void ForEachRow(Action<object[]> callback);

        protected abstract IDataTable Table { get; }

        //public IRowOrientedDataTable Vectorise(string columnName, params uint[] vectorColumnIndices) => Vectorise(null, columnName, vectorColumnIndices);
        //public IRowOrientedDataTable Vectorise(string filePath, string columnName, params uint[] vectorColumnIndices)
        //{
        //    var numericIndices = vectorColumnIndices.Length > 0
        //        ? vectorColumnIndices
        //        : Table.AllMetaData().Where(md => md.IsNumeric()).Select(md => md.Index()).ToArray();
        //    var uniqueIndexSet = new HashSet<uint>(numericIndices);
        //    var otherColumns = Table.AllMetaData().Where(md => !uniqueIndexSet.Contains(md.Index())).ToList();
        //    var uniqueIndices = uniqueIndexSet.OrderBy(d => d).ToArray();

        //    using var builder = new RowOrientedTableBuilder(RowCount, filePath);
        //    builder.AddColumn(ColumnType.Vector, columnName);
        //    var otherColumnIndices = new List<uint>();
        //    var hasOtherColumns = false;
        //    foreach (var column in otherColumns) {
        //        var columnIndex = column.Index();
        //        otherColumnIndices.Add(columnIndex);
        //        hasOtherColumns = true;
        //        var columnType = Table.ColumnTypes[(int)columnIndex];
        //        var metadata = builder.AddColumn(columnType, column);
        //        column.CopyAllExcept(metadata, Consts.Index, Consts.Type);
        //    }

        //    ForEachRow(row => {
        //        var vector = Context.CreateVector((uint)uniqueIndices.Length, i => Convert.ToSingle(row[uniqueIndices[i]]));
        //        if (hasOtherColumns)
        //            builder.AddRow(new object[] { vector }.Concat(otherColumnIndices.Select(i => row[i])).ToArray());
        //        else
        //            builder.AddRow(vector);
        //    });

        //    return builder.Build(Context);
        //}
    }
}
