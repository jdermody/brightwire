using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BrightData.Helper
{
    internal class DataTableConverter : IConvertibleTable
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

            object IConvertibleRow.Get(uint index)
            {
                return Get(index);
            }

            public IDataTableSegment Segment { get; }
            public IDataTable DataTable => _converter.DataTable;

            public T GetTyped<T>(uint index) where T: notnull => _converter.GetField<T>(index, Segment[index]);
            public uint RowIndex { get; }
            public uint NumColumns => Segment.Size;
            object Get(uint index) => Segment[index];
        }
        enum TypeConversion
        {
            Cast,
            ToString,
            DateTicks,
            ChangeType
        }
        readonly Dictionary<(uint Index, Type TargetType), TypeConversion> _typeConversionTable = new();

        public DataTableConverter(IRowOrientedDataTable dataTable)
        {
            DataTable = dataTable;
        }

        public IRowOrientedDataTable DataTable { get; }
        public IEnumerable<T> Map<T>(Func<IConvertibleRow, T> rowMapper) where T: notnull
        {
            for (uint i = 0, len = DataTable.RowCount; i < len; i++)
                yield return rowMapper(Row(i));
        }

        public void ForEachRow(Action<IConvertibleRow> action)
        {
            for (uint i = 0, len = DataTable.RowCount; i < len; i++)
                action(Row(i));
        }

        public IConvertibleRow Row(uint index) => new ConvertibleRow(index, DataTable.Row(index), this);

        public IEnumerable<IConvertibleRow> Rows(params uint[] indices) => (indices.Length == 0 ? DataTable.RowCount.AsRange() : indices).Select(Row);

        T GetField<T>(uint index, object ret) where T: notnull
        {
            var targetType = typeof(T);
            var key = (index, targetType);
            if (!_typeConversionTable.TryGetValue(key, out var typeConversion)) {
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
                TypeConversion.ToString => (T) (object) (ret.ToString() ?? ""),
                TypeConversion.DateTicks => (T) Convert.ChangeType(((DateTime) ret).Ticks, targetType),
                TypeConversion.ChangeType => (T) Convert.ChangeType(ret, targetType),
                _ => throw new NotImplementedException()
            };
        }
    }
}
