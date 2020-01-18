using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using BrightData;
using BrightTable.Builders;
using BrightTable.Segments;

namespace BrightTable.Transformations
{
    class ProjectColumnsTransformation : TableTransformationBase, IMutateColumns
    {
        readonly IColumnOrientedDataTable _table;

        delegate object Delegate(dynamic target);
        readonly Dictionary<uint, Delegate> _mutations = new Dictionary<uint, Delegate>();

        public ProjectColumnsTransformation(IColumnOrientedDataTable table)
        {
            _table = table;
        }

        public IMutateColumns Add<T>(uint index, Func<T, T> mutator)
        {
            _mutations.Add(index, x => mutator(x));
            return this;
        }

        public IColumnOrientedDataTable Mutate(string filePath = null)
        {
            return Transform(_table, filePath);
        }

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            if (_mutations.TryGetValue(index, out var mutator)) {
                var list = new List<object>();
                foreach (var item in column.Enumerate()) {
                    var item2 = mutator(item);
                    if (item2 != null)
                        list.Add(item2);
                }
                
                var buffer = (IEditableBuffer)Activator.CreateInstance(typeof(DataSegmentBuffer<>).MakeGenericType(column.SingleType.GetColumnType()),
                    dataTable.Context,
                    column.SingleType,
                    new MetaData(column.MetaData, Consts.StandardMetaData),
                    (uint)list.Count
                );
                for(int i = 0, len = list.Count; i < len; i++)
                    buffer.Set((uint)i, list[i]);
                buffer.Finalise();
                return (ISingleTypeTableSegment)buffer;
            }
            else
                return column;
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
