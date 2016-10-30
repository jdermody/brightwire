using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.TabularData.Helper
{
    internal class RowConverter
    {
        enum TypeConversion
        {
            Cast,
            ToString,
            DateTicks,
            ChangeType
        }
        readonly Dictionary<Tuple<int, Type>, TypeConversion> _typeConversionTable = new Dictionary<Tuple<int, Type>, TypeConversion>();

        public T GetField<T>(IReadOnlyList<object> data, int index)
        {
#if DEBUG
            if (index < 0 || index > data.Count)
                throw new IndexOutOfRangeException();
#endif

            var ret = data[index];
            if (ret == null)
                return default(T);

            var targetType = typeof(T);
            TypeConversion typeConversion;
            var key = Tuple.Create(index, targetType);
            if (!_typeConversionTable.TryGetValue(key, out typeConversion)) {
                var retType = ret.GetType();
                if (retType == targetType || targetType.IsAssignableFrom(retType))
                    typeConversion = TypeConversion.Cast;
                else if (retType == typeof(DateTime)) {
                    if (targetType == typeof(string))
                        typeConversion = TypeConversion.ToString;
                    else
                        typeConversion = TypeConversion.DateTicks;
                }
                else
                    typeConversion = TypeConversion.ChangeType;
                _typeConversionTable.Add(key, typeConversion);
            }
            
            switch(typeConversion) {
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
