//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace BrightTable.Transformations.Conversions
//{
//    class DataConverter : ICanConvert
//    {
//        class StringToTypedConverter<T> : IConvert<string, T>
//        {
//            readonly Func<string, T> _converter;

//            public StringToTypedConverter(Func<string, T> converter)
//            {
//                _converter = converter;
//            }

//            public T Convert(string data) => _converter(data);
//        }

//        class AnyToStringConverter<T> : IConvert<T, string>
//        {
//            public string Convert(T data) => data.ToString();
//        }

//        private readonly ICanConvert _converter;
//        public DataConverter(ICanConvert converter)
//        {
//            _converter = converter;
//        }

//        //public T Convert<T>(string data) => ((IConvertFromString<T>) _converter).Convert(data);

//        static HashSet<string> TrueSet = new HashSet<string> {"Y", "YES", "TRUE", "T"};

//        public static implicit operator DataConverter(ColumnConversionType type)
//        {
//            if (type == ColumnConversionType.ToBoolean)
//                return new DataConverter(new StringToTypedConverter<bool>(str => TrueSet.Contains(str)));
//            else if (type == ColumnConversionType.ToDate)
//                return new DataConverter(new TypedConverter<DateTime>(DateTime.Parse));
//            else if (type == ColumnConversionType.ToNumeric)
//                Add(index, new ConvertColumnToNumeric());
//            else if (type == ColumnConversionType.ToString)
//                return new DataConverter(new TypedConverter<string>(str => str));
//            else if (type == ColumnConversionType.ToIndexList)
//                Add(index, new ConvertColumnToIndexList());
//            else if (type == ColumnConversionType.ToWeightedIndexList)
//                Add(index, new ConvertColumnToWeightedIndexList());
//            else if (type == ColumnConversionType.ToVector)
//                Add(index, new ConvertColumnToVector());
//            else if (type == ColumnConversionType.ToCategoricalIndex)
//                Add(index, new ConvertColumnToCategoricalIndex());
//            else
//                throw new NotImplementedException();
//        }
//    }
//}
