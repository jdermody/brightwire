using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BrightTable.Helper
{
    class DataTableConverter : IConvertibleTable
    {
        class ConvertibleRow : IConvertibleRow
        {
            readonly DataTableConverter _converter;

            public ConvertibleRow(uint rowIndex, IDataTableSegment row, DataTableConverter converter)
            {
                Segment = row;
                RowIndex = rowIndex;
                _converter = converter;
            }

            public IDataTableSegment Segment { get; }
            public IDataTable DataTable => _converter.DataTable;

            public T GetField<T>(uint index) => _converter.GetField<T>(index, Segment[index]);
            public uint RowIndex { get; }
        }
        enum TypeConversion
        {
            Cast,
            ToString,
            DateTicks,
            ChangeType
        }
        readonly Dictionary<(uint Index, Type TargetType), TypeConversion> _typeConversionTable = new Dictionary<(uint Index, Type TargetType), TypeConversion>();

        public DataTableConverter(IRowOrientedDataTable dataTable)
        {
            DataTable = dataTable;
        }

        public IEnumerable<IConvertibleRow> Rows(params uint[] rowIndices) => rowIndices.Select(GetRow);

        public IRowOrientedDataTable DataTable { get; }
        public IEnumerable<T> Map<T>(Func<IConvertibleRow, T> rowMapper)
        {
            for (uint i = 0, len = DataTable.RowCount; i < len; i++)
                yield return rowMapper(GetRow(i));
        }

        public void ForEachRow(Action<IConvertibleRow> action)
        {
            for (uint i = 0, len = DataTable.RowCount; i < len; i++)
                action(GetRow(i));
        }

        public IConvertibleRow GetRow(uint index) => new ConvertibleRow(index, DataTable.Row(index), this);

        T GetField<T>(uint index, object ret)
        {
            if (ret == null)
                return default;

            var targetType = typeof(T);
            var key = (index, targetType);
            if (!_typeConversionTable.TryGetValue(key, out TypeConversion typeConversion)) {
                var retType = ret.GetType();
                if (retType == targetType || targetType.GetTypeInfo().IsAssignableFrom(retType.GetTypeInfo()))
                    typeConversion = TypeConversion.Cast;
                else if (retType == typeof(DateTime))
                    typeConversion = targetType == typeof(string) ? TypeConversion.ToString : TypeConversion.DateTicks;
                else
                    typeConversion = TypeConversion.ChangeType;
                _typeConversionTable.Add(key, typeConversion);
            }

            return typeConversion switch {
                TypeConversion.Cast => (T) ret,
                TypeConversion.ToString => (T) (object) ret.ToString(),
                TypeConversion.DateTicks => (T) Convert.ChangeType(((DateTime) ret).Ticks, targetType),
                TypeConversion.ChangeType => (T) Convert.ChangeType(ret, targetType),
                _ => throw new NotImplementedException()
            };
        }
    }
}
