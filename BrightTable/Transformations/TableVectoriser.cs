using BrightData;
using BrightData.Helper;
using BrightTable.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightTable.Transformations
{
    static class TableVectoriser
    {
        interface IColumnVectoriser : IDisposable
        {
            IEnumerable<float> GetNext();
        }
        class NumericVectoriser<T> : IColumnVectoriser
            where T: struct
        {
            readonly IEnumerator<T> _enumerator;
            readonly ConvertToFloat<T> _converter = new ConvertToFloat<T>();

            public NumericVectoriser(ISingleTypeTableSegment column)
            {
                _enumerator = ((IDataTableSegment<T>)column).EnumerateTyped().GetEnumerator();
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public IEnumerable<float> GetNext()
            {
                if(_enumerator.MoveNext())
                    yield return _converter.Convert(_enumerator.Current);
            }
        }

        public static IRowOrientedDataTable Vectorise(IColumnOrientedDataTable dataTable, string filePath = null)
        {
            var targetColumn = dataTable.GetTargetColumn();
            var builder = new RowOrientedTableBuilder(dataTable.RowCount, filePath);
            builder.AddColumn(ColumnType.Vector, "Input");
            var inputColumnIndices = dataTable.ColumnIndices().ToList();
            var targetColumnIndices = new List<uint>();
            if(targetColumn.HasValue) {
                builder.AddColumn(ColumnType.Vector, "Target");
                inputColumnIndices.Remove(targetColumn.Value);
            }

            var inputVectorisers = inputColumnIndices
                .Select(i => _GetColumnVectoriser(dataTable.Column(i)))
                .ToList()
            ;



            return null;
        }

        static IColumnVectoriser _GetColumnVectoriser(ISingleTypeTableSegment column)
        {
            var type = column.SingleType;
            if(type.IsNumeric()) {
                return (IColumnVectoriser)Activator.CreateInstance(
                    typeof(NumericVectoriser<>).MakeGenericType(type.GetColumnType()),
                    column
                );
            }

            return null;
        }
    }
}
