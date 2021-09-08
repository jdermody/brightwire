using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData.DataTable.Builders;

namespace BrightData.DataTable
{
    /// <summary>
    /// Data table base class
    /// </summary>
    internal abstract class DataTableBase : IHaveMetaData, IHaveDataContext
    {
        readonly MetaData _tableMetaData = new();

        protected DataTableBase(IBrightDataContext context)
        {
            Context = context;
        }

        internal static string DefaultColumnName(string? name, int numColumns)
        {
            return String.IsNullOrWhiteSpace(name) ? $"Column {numColumns + 1}" : name;
        }

        public IBrightDataContext Context { get; }
        public uint RowCount { get; protected set; }
        public uint ColumnCount { get; protected set; }
        public IMetaData MetaData => _tableMetaData;

        public abstract void ForEachRow(Action<object[]> callback, uint maxRows = uint.MaxValue);
        public abstract DataTableOrientation Orientation { get; }

        protected abstract IDataTable Table { get; }

        protected IEnumerable<uint> AllOrSpecifiedRows(uint[] indices) => indices.Length == 0
            ? RowCount.AsRange()
            : indices;

        protected void ReadHeader(BinaryReader reader, DataTableOrientation expectedOrientation)
        {
            var version = reader.ReadInt32();
            if (version > Consts.DataTableVersion)
                throw new Exception($"Segment table version {version} exceeds {Consts.DataTableVersion}");
            var orientation = (DataTableOrientation)reader.ReadByte();
            if (orientation != expectedOrientation)
                throw new Exception("Invalid orientation");
        }

        public IRowOrientedDataTable? Project(Func<object[], object[]?> projector, string? filePath = null)
        {
            var mutatedRows = new List<object[]>();
            var columnTypes = new Dictionary<uint, BrightDataType>();

            ForEachRow(row => {
                var projected = projector(row);
                if (projected != null)
                {
                    if (projected.Length > columnTypes.Count)
                    {
                        for (uint i = 0, len = (uint)projected.Length; i < len; i++)
                        {
                            var type = projected.GetType().GetBrightDataType();
                            if (columnTypes.TryGetValue(i, out var existing) && existing != type)
                                throw new Exception($"Column {i} type changed between mutations");
                            columnTypes.Add(i, type);
                        }
                    }
                    mutatedRows.Add(projected);
                }
            });

            if (mutatedRows.Any())
            {
                var newColumnTypes = mutatedRows.First().Select(o => o.GetType().GetBrightDataType());
                using var builder = new RowOrientedTableBuilder(MetaData, (uint)mutatedRows.Count, filePath);
                foreach (var column in newColumnTypes)
                    builder.AddColumn(column);
                foreach (var row in mutatedRows)
                    builder.AddRow(row);
                return builder.Build(Context);
            }

            return null;
        }

        public IDataTable WriteTo(string filePath)
        {
            if (Orientation == DataTableOrientation.RowOriented) {
                var table = (IRowOrientedDataTable) this;
                return table.Clone(filePath);
            }
            
            if (Orientation == DataTableOrientation.ColumnOriented) {
                var table = (IColumnOrientedDataTable) this;
                return table.Clone(filePath);
            }

            throw new NotImplementedException();
        }

        protected string GetGroupLabel(uint[] columnIndices, object[] row) => String.Join('|', 
            columnIndices.Select(ci => row[ci].ToString() ?? throw new Exception("Cannot group by string when value is null"))
        ) ?? throw new Exception("No column indices");

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
