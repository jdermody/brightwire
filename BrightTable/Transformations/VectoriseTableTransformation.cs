using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Builders;

namespace BrightTable.Transformations
{
    public class VectoriseTableTransformation : TableTransformationBase
    {
        readonly uint[] _vectorColumnIndices;
        readonly string _columnName;

        public VectoriseTableTransformation(string columnName, params uint[] vectorColumnIndices)
        {
            _vectorColumnIndices = vectorColumnIndices;
            _columnName = columnName;
        }

        protected override (uint ColumnCount, uint RowCount) CalculateNewSize(IDataTable dataTable) => (1, dataTable.RowCount);

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            throw new NotImplementedException();
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            var vectorColumnIndices = _vectorColumnIndices.Length > 0
                ? _vectorColumnIndices
                : dataTable.AllMetaData().Where(md => md.IsNumeric()).Select(md => md.Index()).ToArray();
            var vectorColumnIndexSet = new HashSet<uint>(vectorColumnIndices);
            var otherColumns = dataTable.AllMetaData().Where(md => !vectorColumnIndexSet.Contains(md.Index())).ToList();

            builder.AddColumn(ColumnType.Vector, _columnName);
            var otherColumnIndices = new List<uint>();
            var hasOtherColumns = false;
            foreach(var column in otherColumns) {
                var columnIndex = column.Index();
                otherColumnIndices.Add(columnIndex);
                hasOtherColumns = true;
                var columnType = dataTable.ColumnTypes[(int)columnIndex];
                var metadata = builder.AddColumn(columnType, column);
                column.CopyAllExcept(metadata, Consts.Index, Consts.Type);
            }

            var context = dataTable.Context;
            dataTable.ForEachRow(row => {
                var vector = context.CreateVector((uint)_vectorColumnIndices.Length, i => Convert.ToSingle(row[i]));
                if (hasOtherColumns)
                    builder.AddRow(new object[] {vector}.Concat(otherColumnIndices.Select(i => row[i])).ToArray());
                else
                    builder.AddRow(vector);
            });
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
