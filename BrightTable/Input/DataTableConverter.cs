using BrightData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace BrightTable.Input
{
    class DataTableConverter : IConvertibleTable
    {
        class ConvertibleRow : IConvertibleRow
        {
            readonly DataTableConverter _converter;

            public ConvertibleRow(IDataTableSegment row, DataTableConverter converter)
            {
                Row = row;
                _converter = converter;
            }

            public IDataTableSegment Row { get; }
            public IDataTable DataTable => _converter.DataTable;

            public T GetField<T>(uint index) => _converter.GetField<T>(index, Row[index]);
        }
        enum TypeConversion
        {
            Cast,
            ToString,
            FromNumeric,
            DateTicks,
            ChangeType
        }
        readonly Dictionary<(uint Index, Type TargetType), TypeConversion> _typeConversionTable = new Dictionary<(uint Index, Type TargetType), TypeConversion>();

        public DataTableConverter(IRowOrientedDataTable dataTable)
        {
            DataTable = dataTable;
        }

        public IRowOrientedDataTable DataTable { get; }

        public IConvertibleRow GetRow(uint index) => new ConvertibleRow(DataTable.Row(index), this);

        T GetField<T>(uint index, object ret)
        {
            if (ret == null)
                return default(T);

            var targetType = typeof(T);
            var key = (index, targetType);
            if (!_typeConversionTable.TryGetValue(key, out TypeConversion typeConversion)) {
                var retType = ret.GetType();
                if (retType == targetType || targetType.GetTypeInfo().IsAssignableFrom(retType.GetTypeInfo()))
                    typeConversion = TypeConversion.Cast;
                else if (retType == typeof(DateTime)) {
                    if (targetType == typeof(string))
                        typeConversion = TypeConversion.ToString;
                    else
                        typeConversion = TypeConversion.DateTicks;
                } else {
                    typeConversion = TypeConversion.ChangeType;
                }
                _typeConversionTable.Add(key, typeConversion);
            }

            switch (typeConversion) {
                case TypeConversion.Cast:
                    return (T)ret;
                case TypeConversion.ToString:
                    return (T)(object)ret.ToString();
                case TypeConversion.DateTicks:
                    return (T)Convert.ChangeType(((DateTime)ret).Ticks, targetType);
                case TypeConversion.ChangeType:
                    return (T)Convert.ChangeType(ret, targetType);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
