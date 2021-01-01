using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData;

namespace BrightTable
{
    abstract class DataTableBase : IHaveMetaData
    {
        protected readonly Stream _stream;
        readonly MetaData _tableMetaData = new MetaData();
        protected const uint PREVIEW_SIZE = 10;

        protected DataTableBase(IBrightDataContext context, Stream stream)
        {
            Context = context;
            _stream = stream;
        }

        protected BinaryReader Reader => new BinaryReader(_stream, Encoding.UTF8, true);

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

        protected IEnumerable<uint> AllOrSpecifiedColumns(uint[] indices) => (indices?.Length ?? 0) == 0 
            ? ColumnCount.AsRange()
            : indices;

        protected IEnumerable<uint> AllOrSpecifiedRows(uint[] indices) => (indices?.Length ?? 0) == 0
            ? RowCount.AsRange()
            : indices;

        protected void _ReadHeader(BinaryReader reader, DataTableOrientation expectedOrientation)
        {
            var version = reader.ReadInt32();
            if (version > Consts.DataTableVersion)
                throw new Exception($"Segment table version {version} exceeds {Consts.DataTableVersion}");
            var orientation = (DataTableOrientation)reader.ReadByte();
            if (orientation != expectedOrientation)
                throw new Exception("Invalid orientation");
        }

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
