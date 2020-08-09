using BrightData;
using BrightData.Converters;
using BrightData.Helper;
using BrightTable.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightTable.Transformations
{
    public class DataTableVectoriser
    {
        interface IColumnVectoriser : IDisposable
        {
            IEnumerable<float> GetNext();
            int Size { get; }
        }
        class NumericVectoriser<T> : IColumnVectoriser
            where T : struct
        {
            readonly IEnumerator<T> _enumerator;
            readonly ConvertToFloat<T> _converter = new ConvertToFloat<T>();

            public NumericVectoriser(ISingleTypeTableSegment column)
            {
                _enumerator = ((IDataTableSegment<T>)column).EnumerateTyped().GetEnumerator();
            }

            public int Size => 1;

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public IEnumerable<float> GetNext()
            {
                if (_enumerator.MoveNext())
                    yield return _converter.Convert(_enumerator.Current);
            }
        }

        public float[] GetInput(object[] r)
        {
            throw new NotImplementedException();
        }

        public float[] GetOutput(object[] r)
        {
            throw new NotImplementedException();
        }

        readonly RowOrientedTableBuilder _builder;
        readonly List<IColumnVectoriser> _input = new List<IColumnVectoriser>();
        readonly List<IColumnVectoriser> _target = new List<IColumnVectoriser>();

        public int InputSize => _input.Sum(c => c.Size);
        public int OutputSize => _target.Sum(c => c.Size);

        public DataTableVectoriser(IDataTable dataTable, string filePath = null)
        {
            var targetColumn = dataTable.GetTargetColumn();
            var builder = new RowOrientedTableBuilder(dataTable.RowCount, filePath);
            builder.AddColumn(ColumnType.Vector, "Input");
            var inputColumnIndices = dataTable.ColumnIndices().ToList();
            var targetColumnIndices = new List<uint>();
            if (targetColumn.HasValue) {
                builder.AddColumn(ColumnType.Vector, "Target");
                inputColumnIndices.Remove(targetColumn.Value);
            }

            _input.AddRange(dataTable.Columns(inputColumnIndices.ToArray())
                .Select(_GetColumnVectoriser)
            );
        }

        static IColumnVectoriser _GetColumnVectoriser(ISingleTypeTableSegment column)
        {
            var type = column.SingleType;
            if (type.IsNumeric()) {
                return (IColumnVectoriser)Activator.CreateInstance(
                    typeof(NumericVectoriser<>).MakeGenericType(type.GetColumnType()),
                    column
                );
            }

            return null;
        }
    }
}
