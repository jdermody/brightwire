using System;
using BrightData.Buffer.ReadOnly.Converter;
using BrightData.Types;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Analysis;
using BrightData.Buffer.Operations;
using System.Collections.Generic;
using BrightData.Buffer.Operations.Conversion;
using BrightData.Converter;
using BrightData.Buffer.Vectorisation;
using BrightData.Buffer.Composite;
using BrightData.DataTable.Meta;

namespace BrightData.Helper
{
    /// <summary>
	/// Contains auto generated type mapping functions
	/// </summary>
    internal class GenericTypeMapping
    {
        internal static IReadOnlyBuffer<string> ToStringBuffer(IReadOnlyBuffer buffer)
        {
            var type = buffer.DataType;
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ToStringConverter<bool>((IReadOnlyBuffer<bool>)buffer);
			if(typeCode == TypeCode.SByte)
				return new ToStringConverter<sbyte>((IReadOnlyBuffer<sbyte>)buffer);
			if(typeCode == TypeCode.Single)
				return new ToStringConverter<float>((IReadOnlyBuffer<float>)buffer);
			if(typeCode == TypeCode.Double)
				return new ToStringConverter<double>((IReadOnlyBuffer<double>)buffer);
			if(typeCode == TypeCode.Decimal)
				return new ToStringConverter<decimal>((IReadOnlyBuffer<decimal>)buffer);
			if(typeCode == TypeCode.String)
				return (IReadOnlyBuffer<string>)buffer;
			if(typeCode == TypeCode.Int16)
				return new ToStringConverter<short>((IReadOnlyBuffer<short>)buffer);
			if(typeCode == TypeCode.Int32)
				return new ToStringConverter<int>((IReadOnlyBuffer<int>)buffer);
			if(typeCode == TypeCode.Int64)
				return new ToStringConverter<long>((IReadOnlyBuffer<long>)buffer);
			if(typeCode == TypeCode.DateTime)
				return new ToStringConverter<DateTime>((IReadOnlyBuffer<DateTime>)buffer);
			if(type == typeof(IndexList))
				return new ToStringConverter<IndexList>((IReadOnlyBuffer<IndexList>)buffer);
			if(type == typeof(WeightedIndexList))
				return new ToStringConverter<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)buffer);
			if(type == typeof(BinaryData))
				return new ToStringConverter<BinaryData>((IReadOnlyBuffer<BinaryData>)buffer);
			if(type == typeof(ReadOnlyVector<float>))
				return new ToStringConverter<ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)buffer);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new ToStringConverter<ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)buffer);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new ToStringConverter<ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)buffer);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new ToStringConverter<ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)buffer);
			if(type == typeof(TimeOnly))
				return new ToStringConverter<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer);
			if(type == typeof(DateOnly))
				return new ToStringConverter<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer);
			if(type == typeof(DateTime))
				return new ToStringConverter<DateTime>((IReadOnlyBuffer<DateTime>)buffer);
			throw new NotImplementedException($"Could not create ToStringConverter for type {type}");
        }

        internal static IDataAnalyser ConvertToStringFrequencyAnalysis(Type type, uint writeCount)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ConvertToStringFrequencyAnalysis<bool>(writeCount);
			if(typeCode == TypeCode.SByte)
				return new ConvertToStringFrequencyAnalysis<sbyte>(writeCount);
			if(typeCode == TypeCode.Single)
				return new ConvertToStringFrequencyAnalysis<float>(writeCount);
			if(typeCode == TypeCode.Double)
				return new ConvertToStringFrequencyAnalysis<double>(writeCount);
			if(typeCode == TypeCode.Decimal)
				return new ConvertToStringFrequencyAnalysis<decimal>(writeCount);
			if(typeCode == TypeCode.String)
				return new ConvertToStringFrequencyAnalysis<string>(writeCount);
			if(typeCode == TypeCode.Int16)
				return new ConvertToStringFrequencyAnalysis<short>(writeCount);
			if(typeCode == TypeCode.Int32)
				return new ConvertToStringFrequencyAnalysis<int>(writeCount);
			if(typeCode == TypeCode.Int64)
				return new ConvertToStringFrequencyAnalysis<long>(writeCount);
			if(typeCode == TypeCode.DateTime)
				return new ConvertToStringFrequencyAnalysis<DateTime>(writeCount);
			if(type == typeof(IndexList))
				return new ConvertToStringFrequencyAnalysis<IndexList>(writeCount);
			if(type == typeof(WeightedIndexList))
				return new ConvertToStringFrequencyAnalysis<WeightedIndexList>(writeCount);
			if(type == typeof(BinaryData))
				return new ConvertToStringFrequencyAnalysis<BinaryData>(writeCount);
			if(type == typeof(ReadOnlyVector<float>))
				return new ConvertToStringFrequencyAnalysis<ReadOnlyVector<float>>(writeCount);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new ConvertToStringFrequencyAnalysis<ReadOnlyMatrix<float>>(writeCount);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new ConvertToStringFrequencyAnalysis<ReadOnlyTensor3D<float>>(writeCount);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new ConvertToStringFrequencyAnalysis<ReadOnlyTensor4D<float>>(writeCount);
			if(type == typeof(TimeOnly))
				return new ConvertToStringFrequencyAnalysis<TimeOnly>(writeCount);
			if(type == typeof(DateOnly))
				return new ConvertToStringFrequencyAnalysis<DateOnly>(writeCount);
			if(type == typeof(DateTime))
				return new ConvertToStringFrequencyAnalysis<DateTime>(writeCount);
			throw new NotImplementedException($"Could not create ConvertToStringFrequencyAnalysis for type {type}");
        }

        internal static IReadOnlyBuffer<object> ToObjectBuffer(IReadOnlyBuffer from)
        {
            var type = from.DataType;
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ToObjectConverter<bool>((IReadOnlyBuffer<bool>)from);
			if(typeCode == TypeCode.SByte)
				return new ToObjectConverter<sbyte>((IReadOnlyBuffer<sbyte>)from);
			if(typeCode == TypeCode.Single)
				return new ToObjectConverter<float>((IReadOnlyBuffer<float>)from);
			if(typeCode == TypeCode.Double)
				return new ToObjectConverter<double>((IReadOnlyBuffer<double>)from);
			if(typeCode == TypeCode.Decimal)
				return new ToObjectConverter<decimal>((IReadOnlyBuffer<decimal>)from);
			if(typeCode == TypeCode.String)
				return new ToObjectConverter<string>((IReadOnlyBuffer<string>)from);
			if(typeCode == TypeCode.Int16)
				return new ToObjectConverter<short>((IReadOnlyBuffer<short>)from);
			if(typeCode == TypeCode.Int32)
				return new ToObjectConverter<int>((IReadOnlyBuffer<int>)from);
			if(typeCode == TypeCode.Int64)
				return new ToObjectConverter<long>((IReadOnlyBuffer<long>)from);
			if(typeCode == TypeCode.DateTime)
				return new ToObjectConverter<DateTime>((IReadOnlyBuffer<DateTime>)from);
			if(type == typeof(IndexList))
				return new ToObjectConverter<IndexList>((IReadOnlyBuffer<IndexList>)from);
			if(type == typeof(WeightedIndexList))
				return new ToObjectConverter<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type == typeof(BinaryData))
				return new ToObjectConverter<BinaryData>((IReadOnlyBuffer<BinaryData>)from);
			if(type == typeof(ReadOnlyVector<float>))
				return new ToObjectConverter<ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)from);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new ToObjectConverter<ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new ToObjectConverter<ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new ToObjectConverter<ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from);
			if(type == typeof(TimeOnly))
				return new ToObjectConverter<TimeOnly>((IReadOnlyBuffer<TimeOnly>)from);
			if(type == typeof(DateOnly))
				return new ToObjectConverter<DateOnly>((IReadOnlyBuffer<DateOnly>)from);
			if(type == typeof(DateTime))
				return new ToObjectConverter<DateTime>((IReadOnlyBuffer<DateTime>)from);
			throw new NotImplementedException($"Could not create ToObjectConverter for type {type}");
        }

        internal static IReadOnlyBuffer TypeConverter(Type type2, IReadOnlyBuffer from, ICanConvert converter) 
        {
            var type1 = from.DataType;
			if(type1 == typeof(bool) && type2 == typeof(bool))
				return new TypeConverter<bool, bool>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, bool>)converter);
			if(type1 == typeof(bool) && type2 == typeof(sbyte))
				return new TypeConverter<bool, sbyte>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, sbyte>)converter);
			if(type1 == typeof(bool) && type2 == typeof(float))
				return new TypeConverter<bool, float>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, float>)converter);
			if(type1 == typeof(bool) && type2 == typeof(double))
				return new TypeConverter<bool, double>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, double>)converter);
			if(type1 == typeof(bool) && type2 == typeof(decimal))
				return new TypeConverter<bool, decimal>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, decimal>)converter);
			if(type1 == typeof(bool) && type2 == typeof(string))
				return new TypeConverter<bool, string>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, string>)converter);
			if(type1 == typeof(bool) && type2 == typeof(short))
				return new TypeConverter<bool, short>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, short>)converter);
			if(type1 == typeof(bool) && type2 == typeof(int))
				return new TypeConverter<bool, int>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, int>)converter);
			if(type1 == typeof(bool) && type2 == typeof(long))
				return new TypeConverter<bool, long>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, long>)converter);
			if(type1 == typeof(bool) && type2 == typeof(IndexList))
				return new TypeConverter<bool, IndexList>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, IndexList>)converter);
			if(type1 == typeof(bool) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<bool, WeightedIndexList>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, WeightedIndexList>)converter);
			if(type1 == typeof(bool) && type2 == typeof(BinaryData))
				return new TypeConverter<bool, BinaryData>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, BinaryData>)converter);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<bool, ReadOnlyVector<float>>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<bool, ReadOnlyMatrix<float>>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<bool, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<bool, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(bool) && type2 == typeof(TimeOnly))
				return new TypeConverter<bool, TimeOnly>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, TimeOnly>)converter);
			if(type1 == typeof(bool) && type2 == typeof(DateOnly))
				return new TypeConverter<bool, DateOnly>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, DateOnly>)converter);
			if(type1 == typeof(bool) && type2 == typeof(DateTime))
				return new TypeConverter<bool, DateTime>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, DateTime>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(bool))
				return new TypeConverter<sbyte, bool>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, bool>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(sbyte))
				return new TypeConverter<sbyte, sbyte>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, sbyte>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(float))
				return new TypeConverter<sbyte, float>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, float>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(double))
				return new TypeConverter<sbyte, double>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, double>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(decimal))
				return new TypeConverter<sbyte, decimal>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, decimal>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(string))
				return new TypeConverter<sbyte, string>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, string>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(short))
				return new TypeConverter<sbyte, short>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, short>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(int))
				return new TypeConverter<sbyte, int>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, int>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(long))
				return new TypeConverter<sbyte, long>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, long>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(IndexList))
				return new TypeConverter<sbyte, IndexList>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, IndexList>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<sbyte, WeightedIndexList>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, WeightedIndexList>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(BinaryData))
				return new TypeConverter<sbyte, BinaryData>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, BinaryData>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<sbyte, ReadOnlyVector<float>>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<sbyte, ReadOnlyMatrix<float>>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<sbyte, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<sbyte, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(TimeOnly))
				return new TypeConverter<sbyte, TimeOnly>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, TimeOnly>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(DateOnly))
				return new TypeConverter<sbyte, DateOnly>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, DateOnly>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(DateTime))
				return new TypeConverter<sbyte, DateTime>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, DateTime>)converter);
			if(type1 == typeof(float) && type2 == typeof(bool))
				return new TypeConverter<float, bool>((IReadOnlyBuffer<float>)from, (ICanConvert<float, bool>)converter);
			if(type1 == typeof(float) && type2 == typeof(sbyte))
				return new TypeConverter<float, sbyte>((IReadOnlyBuffer<float>)from, (ICanConvert<float, sbyte>)converter);
			if(type1 == typeof(float) && type2 == typeof(float))
				return new TypeConverter<float, float>((IReadOnlyBuffer<float>)from, (ICanConvert<float, float>)converter);
			if(type1 == typeof(float) && type2 == typeof(double))
				return new TypeConverter<float, double>((IReadOnlyBuffer<float>)from, (ICanConvert<float, double>)converter);
			if(type1 == typeof(float) && type2 == typeof(decimal))
				return new TypeConverter<float, decimal>((IReadOnlyBuffer<float>)from, (ICanConvert<float, decimal>)converter);
			if(type1 == typeof(float) && type2 == typeof(string))
				return new TypeConverter<float, string>((IReadOnlyBuffer<float>)from, (ICanConvert<float, string>)converter);
			if(type1 == typeof(float) && type2 == typeof(short))
				return new TypeConverter<float, short>((IReadOnlyBuffer<float>)from, (ICanConvert<float, short>)converter);
			if(type1 == typeof(float) && type2 == typeof(int))
				return new TypeConverter<float, int>((IReadOnlyBuffer<float>)from, (ICanConvert<float, int>)converter);
			if(type1 == typeof(float) && type2 == typeof(long))
				return new TypeConverter<float, long>((IReadOnlyBuffer<float>)from, (ICanConvert<float, long>)converter);
			if(type1 == typeof(float) && type2 == typeof(IndexList))
				return new TypeConverter<float, IndexList>((IReadOnlyBuffer<float>)from, (ICanConvert<float, IndexList>)converter);
			if(type1 == typeof(float) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<float, WeightedIndexList>((IReadOnlyBuffer<float>)from, (ICanConvert<float, WeightedIndexList>)converter);
			if(type1 == typeof(float) && type2 == typeof(BinaryData))
				return new TypeConverter<float, BinaryData>((IReadOnlyBuffer<float>)from, (ICanConvert<float, BinaryData>)converter);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<float, ReadOnlyVector<float>>((IReadOnlyBuffer<float>)from, (ICanConvert<float, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<float, ReadOnlyMatrix<float>>((IReadOnlyBuffer<float>)from, (ICanConvert<float, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<float, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<float>)from, (ICanConvert<float, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<float, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<float>)from, (ICanConvert<float, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(float) && type2 == typeof(TimeOnly))
				return new TypeConverter<float, TimeOnly>((IReadOnlyBuffer<float>)from, (ICanConvert<float, TimeOnly>)converter);
			if(type1 == typeof(float) && type2 == typeof(DateOnly))
				return new TypeConverter<float, DateOnly>((IReadOnlyBuffer<float>)from, (ICanConvert<float, DateOnly>)converter);
			if(type1 == typeof(float) && type2 == typeof(DateTime))
				return new TypeConverter<float, DateTime>((IReadOnlyBuffer<float>)from, (ICanConvert<float, DateTime>)converter);
			if(type1 == typeof(double) && type2 == typeof(bool))
				return new TypeConverter<double, bool>((IReadOnlyBuffer<double>)from, (ICanConvert<double, bool>)converter);
			if(type1 == typeof(double) && type2 == typeof(sbyte))
				return new TypeConverter<double, sbyte>((IReadOnlyBuffer<double>)from, (ICanConvert<double, sbyte>)converter);
			if(type1 == typeof(double) && type2 == typeof(float))
				return new TypeConverter<double, float>((IReadOnlyBuffer<double>)from, (ICanConvert<double, float>)converter);
			if(type1 == typeof(double) && type2 == typeof(double))
				return new TypeConverter<double, double>((IReadOnlyBuffer<double>)from, (ICanConvert<double, double>)converter);
			if(type1 == typeof(double) && type2 == typeof(decimal))
				return new TypeConverter<double, decimal>((IReadOnlyBuffer<double>)from, (ICanConvert<double, decimal>)converter);
			if(type1 == typeof(double) && type2 == typeof(string))
				return new TypeConverter<double, string>((IReadOnlyBuffer<double>)from, (ICanConvert<double, string>)converter);
			if(type1 == typeof(double) && type2 == typeof(short))
				return new TypeConverter<double, short>((IReadOnlyBuffer<double>)from, (ICanConvert<double, short>)converter);
			if(type1 == typeof(double) && type2 == typeof(int))
				return new TypeConverter<double, int>((IReadOnlyBuffer<double>)from, (ICanConvert<double, int>)converter);
			if(type1 == typeof(double) && type2 == typeof(long))
				return new TypeConverter<double, long>((IReadOnlyBuffer<double>)from, (ICanConvert<double, long>)converter);
			if(type1 == typeof(double) && type2 == typeof(IndexList))
				return new TypeConverter<double, IndexList>((IReadOnlyBuffer<double>)from, (ICanConvert<double, IndexList>)converter);
			if(type1 == typeof(double) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<double, WeightedIndexList>((IReadOnlyBuffer<double>)from, (ICanConvert<double, WeightedIndexList>)converter);
			if(type1 == typeof(double) && type2 == typeof(BinaryData))
				return new TypeConverter<double, BinaryData>((IReadOnlyBuffer<double>)from, (ICanConvert<double, BinaryData>)converter);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<double, ReadOnlyVector<float>>((IReadOnlyBuffer<double>)from, (ICanConvert<double, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<double, ReadOnlyMatrix<float>>((IReadOnlyBuffer<double>)from, (ICanConvert<double, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<double, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<double>)from, (ICanConvert<double, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<double, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<double>)from, (ICanConvert<double, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(double) && type2 == typeof(TimeOnly))
				return new TypeConverter<double, TimeOnly>((IReadOnlyBuffer<double>)from, (ICanConvert<double, TimeOnly>)converter);
			if(type1 == typeof(double) && type2 == typeof(DateOnly))
				return new TypeConverter<double, DateOnly>((IReadOnlyBuffer<double>)from, (ICanConvert<double, DateOnly>)converter);
			if(type1 == typeof(double) && type2 == typeof(DateTime))
				return new TypeConverter<double, DateTime>((IReadOnlyBuffer<double>)from, (ICanConvert<double, DateTime>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(bool))
				return new TypeConverter<decimal, bool>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, bool>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(sbyte))
				return new TypeConverter<decimal, sbyte>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, sbyte>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(float))
				return new TypeConverter<decimal, float>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, float>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(double))
				return new TypeConverter<decimal, double>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, double>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(decimal))
				return new TypeConverter<decimal, decimal>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, decimal>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(string))
				return new TypeConverter<decimal, string>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, string>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(short))
				return new TypeConverter<decimal, short>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, short>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(int))
				return new TypeConverter<decimal, int>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, int>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(long))
				return new TypeConverter<decimal, long>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, long>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(IndexList))
				return new TypeConverter<decimal, IndexList>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, IndexList>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<decimal, WeightedIndexList>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, WeightedIndexList>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(BinaryData))
				return new TypeConverter<decimal, BinaryData>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, BinaryData>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<decimal, ReadOnlyVector<float>>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<decimal, ReadOnlyMatrix<float>>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<decimal, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<decimal, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(TimeOnly))
				return new TypeConverter<decimal, TimeOnly>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, TimeOnly>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(DateOnly))
				return new TypeConverter<decimal, DateOnly>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, DateOnly>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(DateTime))
				return new TypeConverter<decimal, DateTime>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, DateTime>)converter);
			if(type1 == typeof(string) && type2 == typeof(bool))
				return new TypeConverter<string, bool>((IReadOnlyBuffer<string>)from, (ICanConvert<string, bool>)converter);
			if(type1 == typeof(string) && type2 == typeof(sbyte))
				return new TypeConverter<string, sbyte>((IReadOnlyBuffer<string>)from, (ICanConvert<string, sbyte>)converter);
			if(type1 == typeof(string) && type2 == typeof(float))
				return new TypeConverter<string, float>((IReadOnlyBuffer<string>)from, (ICanConvert<string, float>)converter);
			if(type1 == typeof(string) && type2 == typeof(double))
				return new TypeConverter<string, double>((IReadOnlyBuffer<string>)from, (ICanConvert<string, double>)converter);
			if(type1 == typeof(string) && type2 == typeof(decimal))
				return new TypeConverter<string, decimal>((IReadOnlyBuffer<string>)from, (ICanConvert<string, decimal>)converter);
			if(type1 == typeof(string) && type2 == typeof(string))
				return new TypeConverter<string, string>((IReadOnlyBuffer<string>)from, (ICanConvert<string, string>)converter);
			if(type1 == typeof(string) && type2 == typeof(short))
				return new TypeConverter<string, short>((IReadOnlyBuffer<string>)from, (ICanConvert<string, short>)converter);
			if(type1 == typeof(string) && type2 == typeof(int))
				return new TypeConverter<string, int>((IReadOnlyBuffer<string>)from, (ICanConvert<string, int>)converter);
			if(type1 == typeof(string) && type2 == typeof(long))
				return new TypeConverter<string, long>((IReadOnlyBuffer<string>)from, (ICanConvert<string, long>)converter);
			if(type1 == typeof(string) && type2 == typeof(IndexList))
				return new TypeConverter<string, IndexList>((IReadOnlyBuffer<string>)from, (ICanConvert<string, IndexList>)converter);
			if(type1 == typeof(string) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<string, WeightedIndexList>((IReadOnlyBuffer<string>)from, (ICanConvert<string, WeightedIndexList>)converter);
			if(type1 == typeof(string) && type2 == typeof(BinaryData))
				return new TypeConverter<string, BinaryData>((IReadOnlyBuffer<string>)from, (ICanConvert<string, BinaryData>)converter);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<string, ReadOnlyVector<float>>((IReadOnlyBuffer<string>)from, (ICanConvert<string, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<string, ReadOnlyMatrix<float>>((IReadOnlyBuffer<string>)from, (ICanConvert<string, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<string, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<string>)from, (ICanConvert<string, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<string, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<string>)from, (ICanConvert<string, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(string) && type2 == typeof(TimeOnly))
				return new TypeConverter<string, TimeOnly>((IReadOnlyBuffer<string>)from, (ICanConvert<string, TimeOnly>)converter);
			if(type1 == typeof(string) && type2 == typeof(DateOnly))
				return new TypeConverter<string, DateOnly>((IReadOnlyBuffer<string>)from, (ICanConvert<string, DateOnly>)converter);
			if(type1 == typeof(string) && type2 == typeof(DateTime))
				return new TypeConverter<string, DateTime>((IReadOnlyBuffer<string>)from, (ICanConvert<string, DateTime>)converter);
			if(type1 == typeof(short) && type2 == typeof(bool))
				return new TypeConverter<short, bool>((IReadOnlyBuffer<short>)from, (ICanConvert<short, bool>)converter);
			if(type1 == typeof(short) && type2 == typeof(sbyte))
				return new TypeConverter<short, sbyte>((IReadOnlyBuffer<short>)from, (ICanConvert<short, sbyte>)converter);
			if(type1 == typeof(short) && type2 == typeof(float))
				return new TypeConverter<short, float>((IReadOnlyBuffer<short>)from, (ICanConvert<short, float>)converter);
			if(type1 == typeof(short) && type2 == typeof(double))
				return new TypeConverter<short, double>((IReadOnlyBuffer<short>)from, (ICanConvert<short, double>)converter);
			if(type1 == typeof(short) && type2 == typeof(decimal))
				return new TypeConverter<short, decimal>((IReadOnlyBuffer<short>)from, (ICanConvert<short, decimal>)converter);
			if(type1 == typeof(short) && type2 == typeof(string))
				return new TypeConverter<short, string>((IReadOnlyBuffer<short>)from, (ICanConvert<short, string>)converter);
			if(type1 == typeof(short) && type2 == typeof(short))
				return new TypeConverter<short, short>((IReadOnlyBuffer<short>)from, (ICanConvert<short, short>)converter);
			if(type1 == typeof(short) && type2 == typeof(int))
				return new TypeConverter<short, int>((IReadOnlyBuffer<short>)from, (ICanConvert<short, int>)converter);
			if(type1 == typeof(short) && type2 == typeof(long))
				return new TypeConverter<short, long>((IReadOnlyBuffer<short>)from, (ICanConvert<short, long>)converter);
			if(type1 == typeof(short) && type2 == typeof(IndexList))
				return new TypeConverter<short, IndexList>((IReadOnlyBuffer<short>)from, (ICanConvert<short, IndexList>)converter);
			if(type1 == typeof(short) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<short, WeightedIndexList>((IReadOnlyBuffer<short>)from, (ICanConvert<short, WeightedIndexList>)converter);
			if(type1 == typeof(short) && type2 == typeof(BinaryData))
				return new TypeConverter<short, BinaryData>((IReadOnlyBuffer<short>)from, (ICanConvert<short, BinaryData>)converter);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<short, ReadOnlyVector<float>>((IReadOnlyBuffer<short>)from, (ICanConvert<short, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<short, ReadOnlyMatrix<float>>((IReadOnlyBuffer<short>)from, (ICanConvert<short, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<short, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<short>)from, (ICanConvert<short, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<short, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<short>)from, (ICanConvert<short, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(short) && type2 == typeof(TimeOnly))
				return new TypeConverter<short, TimeOnly>((IReadOnlyBuffer<short>)from, (ICanConvert<short, TimeOnly>)converter);
			if(type1 == typeof(short) && type2 == typeof(DateOnly))
				return new TypeConverter<short, DateOnly>((IReadOnlyBuffer<short>)from, (ICanConvert<short, DateOnly>)converter);
			if(type1 == typeof(short) && type2 == typeof(DateTime))
				return new TypeConverter<short, DateTime>((IReadOnlyBuffer<short>)from, (ICanConvert<short, DateTime>)converter);
			if(type1 == typeof(int) && type2 == typeof(bool))
				return new TypeConverter<int, bool>((IReadOnlyBuffer<int>)from, (ICanConvert<int, bool>)converter);
			if(type1 == typeof(int) && type2 == typeof(sbyte))
				return new TypeConverter<int, sbyte>((IReadOnlyBuffer<int>)from, (ICanConvert<int, sbyte>)converter);
			if(type1 == typeof(int) && type2 == typeof(float))
				return new TypeConverter<int, float>((IReadOnlyBuffer<int>)from, (ICanConvert<int, float>)converter);
			if(type1 == typeof(int) && type2 == typeof(double))
				return new TypeConverter<int, double>((IReadOnlyBuffer<int>)from, (ICanConvert<int, double>)converter);
			if(type1 == typeof(int) && type2 == typeof(decimal))
				return new TypeConverter<int, decimal>((IReadOnlyBuffer<int>)from, (ICanConvert<int, decimal>)converter);
			if(type1 == typeof(int) && type2 == typeof(string))
				return new TypeConverter<int, string>((IReadOnlyBuffer<int>)from, (ICanConvert<int, string>)converter);
			if(type1 == typeof(int) && type2 == typeof(short))
				return new TypeConverter<int, short>((IReadOnlyBuffer<int>)from, (ICanConvert<int, short>)converter);
			if(type1 == typeof(int) && type2 == typeof(int))
				return new TypeConverter<int, int>((IReadOnlyBuffer<int>)from, (ICanConvert<int, int>)converter);
			if(type1 == typeof(int) && type2 == typeof(long))
				return new TypeConverter<int, long>((IReadOnlyBuffer<int>)from, (ICanConvert<int, long>)converter);
			if(type1 == typeof(int) && type2 == typeof(IndexList))
				return new TypeConverter<int, IndexList>((IReadOnlyBuffer<int>)from, (ICanConvert<int, IndexList>)converter);
			if(type1 == typeof(int) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<int, WeightedIndexList>((IReadOnlyBuffer<int>)from, (ICanConvert<int, WeightedIndexList>)converter);
			if(type1 == typeof(int) && type2 == typeof(BinaryData))
				return new TypeConverter<int, BinaryData>((IReadOnlyBuffer<int>)from, (ICanConvert<int, BinaryData>)converter);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<int, ReadOnlyVector<float>>((IReadOnlyBuffer<int>)from, (ICanConvert<int, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<int, ReadOnlyMatrix<float>>((IReadOnlyBuffer<int>)from, (ICanConvert<int, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<int, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<int>)from, (ICanConvert<int, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<int, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<int>)from, (ICanConvert<int, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(int) && type2 == typeof(TimeOnly))
				return new TypeConverter<int, TimeOnly>((IReadOnlyBuffer<int>)from, (ICanConvert<int, TimeOnly>)converter);
			if(type1 == typeof(int) && type2 == typeof(DateOnly))
				return new TypeConverter<int, DateOnly>((IReadOnlyBuffer<int>)from, (ICanConvert<int, DateOnly>)converter);
			if(type1 == typeof(int) && type2 == typeof(DateTime))
				return new TypeConverter<int, DateTime>((IReadOnlyBuffer<int>)from, (ICanConvert<int, DateTime>)converter);
			if(type1 == typeof(long) && type2 == typeof(bool))
				return new TypeConverter<long, bool>((IReadOnlyBuffer<long>)from, (ICanConvert<long, bool>)converter);
			if(type1 == typeof(long) && type2 == typeof(sbyte))
				return new TypeConverter<long, sbyte>((IReadOnlyBuffer<long>)from, (ICanConvert<long, sbyte>)converter);
			if(type1 == typeof(long) && type2 == typeof(float))
				return new TypeConverter<long, float>((IReadOnlyBuffer<long>)from, (ICanConvert<long, float>)converter);
			if(type1 == typeof(long) && type2 == typeof(double))
				return new TypeConverter<long, double>((IReadOnlyBuffer<long>)from, (ICanConvert<long, double>)converter);
			if(type1 == typeof(long) && type2 == typeof(decimal))
				return new TypeConverter<long, decimal>((IReadOnlyBuffer<long>)from, (ICanConvert<long, decimal>)converter);
			if(type1 == typeof(long) && type2 == typeof(string))
				return new TypeConverter<long, string>((IReadOnlyBuffer<long>)from, (ICanConvert<long, string>)converter);
			if(type1 == typeof(long) && type2 == typeof(short))
				return new TypeConverter<long, short>((IReadOnlyBuffer<long>)from, (ICanConvert<long, short>)converter);
			if(type1 == typeof(long) && type2 == typeof(int))
				return new TypeConverter<long, int>((IReadOnlyBuffer<long>)from, (ICanConvert<long, int>)converter);
			if(type1 == typeof(long) && type2 == typeof(long))
				return new TypeConverter<long, long>((IReadOnlyBuffer<long>)from, (ICanConvert<long, long>)converter);
			if(type1 == typeof(long) && type2 == typeof(IndexList))
				return new TypeConverter<long, IndexList>((IReadOnlyBuffer<long>)from, (ICanConvert<long, IndexList>)converter);
			if(type1 == typeof(long) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<long, WeightedIndexList>((IReadOnlyBuffer<long>)from, (ICanConvert<long, WeightedIndexList>)converter);
			if(type1 == typeof(long) && type2 == typeof(BinaryData))
				return new TypeConverter<long, BinaryData>((IReadOnlyBuffer<long>)from, (ICanConvert<long, BinaryData>)converter);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<long, ReadOnlyVector<float>>((IReadOnlyBuffer<long>)from, (ICanConvert<long, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<long, ReadOnlyMatrix<float>>((IReadOnlyBuffer<long>)from, (ICanConvert<long, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<long, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<long>)from, (ICanConvert<long, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<long, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<long>)from, (ICanConvert<long, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(long) && type2 == typeof(TimeOnly))
				return new TypeConverter<long, TimeOnly>((IReadOnlyBuffer<long>)from, (ICanConvert<long, TimeOnly>)converter);
			if(type1 == typeof(long) && type2 == typeof(DateOnly))
				return new TypeConverter<long, DateOnly>((IReadOnlyBuffer<long>)from, (ICanConvert<long, DateOnly>)converter);
			if(type1 == typeof(long) && type2 == typeof(DateTime))
				return new TypeConverter<long, DateTime>((IReadOnlyBuffer<long>)from, (ICanConvert<long, DateTime>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(bool))
				return new TypeConverter<IndexList, bool>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, bool>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(sbyte))
				return new TypeConverter<IndexList, sbyte>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, sbyte>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(float))
				return new TypeConverter<IndexList, float>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, float>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(double))
				return new TypeConverter<IndexList, double>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, double>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(decimal))
				return new TypeConverter<IndexList, decimal>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, decimal>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(string))
				return new TypeConverter<IndexList, string>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, string>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(short))
				return new TypeConverter<IndexList, short>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, short>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(int))
				return new TypeConverter<IndexList, int>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, int>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(long))
				return new TypeConverter<IndexList, long>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, long>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(IndexList))
				return new TypeConverter<IndexList, IndexList>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, IndexList>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<IndexList, WeightedIndexList>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, WeightedIndexList>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(BinaryData))
				return new TypeConverter<IndexList, BinaryData>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, BinaryData>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<IndexList, ReadOnlyVector<float>>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<IndexList, ReadOnlyMatrix<float>>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<IndexList, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<IndexList, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(TimeOnly))
				return new TypeConverter<IndexList, TimeOnly>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, TimeOnly>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(DateOnly))
				return new TypeConverter<IndexList, DateOnly>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, DateOnly>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(DateTime))
				return new TypeConverter<IndexList, DateTime>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, DateTime>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(bool))
				return new TypeConverter<WeightedIndexList, bool>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, bool>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(sbyte))
				return new TypeConverter<WeightedIndexList, sbyte>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, sbyte>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(float))
				return new TypeConverter<WeightedIndexList, float>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, float>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(double))
				return new TypeConverter<WeightedIndexList, double>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, double>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(decimal))
				return new TypeConverter<WeightedIndexList, decimal>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, decimal>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(string))
				return new TypeConverter<WeightedIndexList, string>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, string>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(short))
				return new TypeConverter<WeightedIndexList, short>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, short>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(int))
				return new TypeConverter<WeightedIndexList, int>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, int>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(long))
				return new TypeConverter<WeightedIndexList, long>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, long>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(IndexList))
				return new TypeConverter<WeightedIndexList, IndexList>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, IndexList>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<WeightedIndexList, WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, WeightedIndexList>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(BinaryData))
				return new TypeConverter<WeightedIndexList, BinaryData>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, BinaryData>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<WeightedIndexList, ReadOnlyVector<float>>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<WeightedIndexList, ReadOnlyMatrix<float>>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<WeightedIndexList, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<WeightedIndexList, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(TimeOnly))
				return new TypeConverter<WeightedIndexList, TimeOnly>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, TimeOnly>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(DateOnly))
				return new TypeConverter<WeightedIndexList, DateOnly>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, DateOnly>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(DateTime))
				return new TypeConverter<WeightedIndexList, DateTime>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, DateTime>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(bool))
				return new TypeConverter<BinaryData, bool>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, bool>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(sbyte))
				return new TypeConverter<BinaryData, sbyte>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, sbyte>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(float))
				return new TypeConverter<BinaryData, float>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, float>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(double))
				return new TypeConverter<BinaryData, double>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, double>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(decimal))
				return new TypeConverter<BinaryData, decimal>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, decimal>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(string))
				return new TypeConverter<BinaryData, string>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, string>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(short))
				return new TypeConverter<BinaryData, short>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, short>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(int))
				return new TypeConverter<BinaryData, int>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, int>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(long))
				return new TypeConverter<BinaryData, long>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, long>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(IndexList))
				return new TypeConverter<BinaryData, IndexList>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, IndexList>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<BinaryData, WeightedIndexList>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, WeightedIndexList>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(BinaryData))
				return new TypeConverter<BinaryData, BinaryData>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, BinaryData>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<BinaryData, ReadOnlyVector<float>>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<BinaryData, ReadOnlyMatrix<float>>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<BinaryData, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<BinaryData, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(TimeOnly))
				return new TypeConverter<BinaryData, TimeOnly>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, TimeOnly>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(DateOnly))
				return new TypeConverter<BinaryData, DateOnly>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, DateOnly>)converter);
			if(type1 == typeof(BinaryData) && type2 == typeof(DateTime))
				return new TypeConverter<BinaryData, DateTime>((IReadOnlyBuffer<BinaryData>)from, (ICanConvert<BinaryData, DateTime>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(bool))
				return new TypeConverter<ReadOnlyVector<float>, bool>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, bool>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(sbyte))
				return new TypeConverter<ReadOnlyVector<float>, sbyte>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, sbyte>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(float))
				return new TypeConverter<ReadOnlyVector<float>, float>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, float>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(double))
				return new TypeConverter<ReadOnlyVector<float>, double>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, double>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(decimal))
				return new TypeConverter<ReadOnlyVector<float>, decimal>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, decimal>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(string))
				return new TypeConverter<ReadOnlyVector<float>, string>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, string>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(short))
				return new TypeConverter<ReadOnlyVector<float>, short>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, short>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(int))
				return new TypeConverter<ReadOnlyVector<float>, int>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, int>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(long))
				return new TypeConverter<ReadOnlyVector<float>, long>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, long>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(IndexList))
				return new TypeConverter<ReadOnlyVector<float>, IndexList>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, IndexList>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<ReadOnlyVector<float>, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, WeightedIndexList>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(BinaryData))
				return new TypeConverter<ReadOnlyVector<float>, BinaryData>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, BinaryData>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<ReadOnlyVector<float>, ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<ReadOnlyVector<float>, ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<ReadOnlyVector<float>, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<ReadOnlyVector<float>, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(TimeOnly))
				return new TypeConverter<ReadOnlyVector<float>, TimeOnly>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, TimeOnly>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(DateOnly))
				return new TypeConverter<ReadOnlyVector<float>, DateOnly>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, DateOnly>)converter);
			if(type1 == typeof(ReadOnlyVector<float>) && type2 == typeof(DateTime))
				return new TypeConverter<ReadOnlyVector<float>, DateTime>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (ICanConvert<ReadOnlyVector<float>, DateTime>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(bool))
				return new TypeConverter<ReadOnlyMatrix<float>, bool>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, bool>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(sbyte))
				return new TypeConverter<ReadOnlyMatrix<float>, sbyte>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, sbyte>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(float))
				return new TypeConverter<ReadOnlyMatrix<float>, float>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, float>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(double))
				return new TypeConverter<ReadOnlyMatrix<float>, double>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, double>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(decimal))
				return new TypeConverter<ReadOnlyMatrix<float>, decimal>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, decimal>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(string))
				return new TypeConverter<ReadOnlyMatrix<float>, string>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, string>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(short))
				return new TypeConverter<ReadOnlyMatrix<float>, short>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, short>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(int))
				return new TypeConverter<ReadOnlyMatrix<float>, int>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, int>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(long))
				return new TypeConverter<ReadOnlyMatrix<float>, long>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, long>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(IndexList))
				return new TypeConverter<ReadOnlyMatrix<float>, IndexList>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, IndexList>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<ReadOnlyMatrix<float>, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, WeightedIndexList>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(BinaryData))
				return new TypeConverter<ReadOnlyMatrix<float>, BinaryData>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, BinaryData>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<ReadOnlyMatrix<float>, ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<ReadOnlyMatrix<float>, ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<ReadOnlyMatrix<float>, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<ReadOnlyMatrix<float>, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(TimeOnly))
				return new TypeConverter<ReadOnlyMatrix<float>, TimeOnly>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, TimeOnly>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(DateOnly))
				return new TypeConverter<ReadOnlyMatrix<float>, DateOnly>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, DateOnly>)converter);
			if(type1 == typeof(ReadOnlyMatrix<float>) && type2 == typeof(DateTime))
				return new TypeConverter<ReadOnlyMatrix<float>, DateTime>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (ICanConvert<ReadOnlyMatrix<float>, DateTime>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(bool))
				return new TypeConverter<ReadOnlyTensor3D<float>, bool>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, bool>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(sbyte))
				return new TypeConverter<ReadOnlyTensor3D<float>, sbyte>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, sbyte>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(float))
				return new TypeConverter<ReadOnlyTensor3D<float>, float>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, float>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(double))
				return new TypeConverter<ReadOnlyTensor3D<float>, double>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, double>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(decimal))
				return new TypeConverter<ReadOnlyTensor3D<float>, decimal>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, decimal>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(string))
				return new TypeConverter<ReadOnlyTensor3D<float>, string>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, string>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(short))
				return new TypeConverter<ReadOnlyTensor3D<float>, short>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, short>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(int))
				return new TypeConverter<ReadOnlyTensor3D<float>, int>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, int>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(long))
				return new TypeConverter<ReadOnlyTensor3D<float>, long>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, long>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(IndexList))
				return new TypeConverter<ReadOnlyTensor3D<float>, IndexList>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, IndexList>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<ReadOnlyTensor3D<float>, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, WeightedIndexList>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(BinaryData))
				return new TypeConverter<ReadOnlyTensor3D<float>, BinaryData>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, BinaryData>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<ReadOnlyTensor3D<float>, ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<ReadOnlyTensor3D<float>, ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<ReadOnlyTensor3D<float>, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<ReadOnlyTensor3D<float>, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(TimeOnly))
				return new TypeConverter<ReadOnlyTensor3D<float>, TimeOnly>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, TimeOnly>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(DateOnly))
				return new TypeConverter<ReadOnlyTensor3D<float>, DateOnly>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, DateOnly>)converter);
			if(type1 == typeof(ReadOnlyTensor3D<float>) && type2 == typeof(DateTime))
				return new TypeConverter<ReadOnlyTensor3D<float>, DateTime>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (ICanConvert<ReadOnlyTensor3D<float>, DateTime>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(bool))
				return new TypeConverter<ReadOnlyTensor4D<float>, bool>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, bool>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(sbyte))
				return new TypeConverter<ReadOnlyTensor4D<float>, sbyte>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, sbyte>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(float))
				return new TypeConverter<ReadOnlyTensor4D<float>, float>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, float>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(double))
				return new TypeConverter<ReadOnlyTensor4D<float>, double>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, double>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(decimal))
				return new TypeConverter<ReadOnlyTensor4D<float>, decimal>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, decimal>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(string))
				return new TypeConverter<ReadOnlyTensor4D<float>, string>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, string>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(short))
				return new TypeConverter<ReadOnlyTensor4D<float>, short>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, short>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(int))
				return new TypeConverter<ReadOnlyTensor4D<float>, int>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, int>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(long))
				return new TypeConverter<ReadOnlyTensor4D<float>, long>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, long>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(IndexList))
				return new TypeConverter<ReadOnlyTensor4D<float>, IndexList>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, IndexList>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<ReadOnlyTensor4D<float>, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, WeightedIndexList>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(BinaryData))
				return new TypeConverter<ReadOnlyTensor4D<float>, BinaryData>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, BinaryData>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<ReadOnlyTensor4D<float>, ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<ReadOnlyTensor4D<float>, ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<ReadOnlyTensor4D<float>, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<ReadOnlyTensor4D<float>, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(TimeOnly))
				return new TypeConverter<ReadOnlyTensor4D<float>, TimeOnly>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, TimeOnly>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(DateOnly))
				return new TypeConverter<ReadOnlyTensor4D<float>, DateOnly>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, DateOnly>)converter);
			if(type1 == typeof(ReadOnlyTensor4D<float>) && type2 == typeof(DateTime))
				return new TypeConverter<ReadOnlyTensor4D<float>, DateTime>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (ICanConvert<ReadOnlyTensor4D<float>, DateTime>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(bool))
				return new TypeConverter<TimeOnly, bool>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, bool>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(sbyte))
				return new TypeConverter<TimeOnly, sbyte>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, sbyte>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(float))
				return new TypeConverter<TimeOnly, float>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, float>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(double))
				return new TypeConverter<TimeOnly, double>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, double>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(decimal))
				return new TypeConverter<TimeOnly, decimal>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, decimal>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(string))
				return new TypeConverter<TimeOnly, string>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, string>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(short))
				return new TypeConverter<TimeOnly, short>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, short>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(int))
				return new TypeConverter<TimeOnly, int>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, int>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(long))
				return new TypeConverter<TimeOnly, long>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, long>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(IndexList))
				return new TypeConverter<TimeOnly, IndexList>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, IndexList>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<TimeOnly, WeightedIndexList>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, WeightedIndexList>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(BinaryData))
				return new TypeConverter<TimeOnly, BinaryData>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, BinaryData>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<TimeOnly, ReadOnlyVector<float>>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<TimeOnly, ReadOnlyMatrix<float>>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<TimeOnly, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<TimeOnly, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(TimeOnly))
				return new TypeConverter<TimeOnly, TimeOnly>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, TimeOnly>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(DateOnly))
				return new TypeConverter<TimeOnly, DateOnly>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, DateOnly>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(DateTime))
				return new TypeConverter<TimeOnly, DateTime>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, DateTime>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(bool))
				return new TypeConverter<DateOnly, bool>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, bool>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(sbyte))
				return new TypeConverter<DateOnly, sbyte>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, sbyte>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(float))
				return new TypeConverter<DateOnly, float>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, float>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(double))
				return new TypeConverter<DateOnly, double>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, double>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(decimal))
				return new TypeConverter<DateOnly, decimal>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, decimal>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(string))
				return new TypeConverter<DateOnly, string>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, string>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(short))
				return new TypeConverter<DateOnly, short>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, short>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(int))
				return new TypeConverter<DateOnly, int>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, int>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(long))
				return new TypeConverter<DateOnly, long>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, long>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(IndexList))
				return new TypeConverter<DateOnly, IndexList>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, IndexList>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<DateOnly, WeightedIndexList>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, WeightedIndexList>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(BinaryData))
				return new TypeConverter<DateOnly, BinaryData>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, BinaryData>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<DateOnly, ReadOnlyVector<float>>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<DateOnly, ReadOnlyMatrix<float>>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<DateOnly, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<DateOnly, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(TimeOnly))
				return new TypeConverter<DateOnly, TimeOnly>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, TimeOnly>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(DateOnly))
				return new TypeConverter<DateOnly, DateOnly>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, DateOnly>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(DateTime))
				return new TypeConverter<DateOnly, DateTime>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, DateTime>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(bool))
				return new TypeConverter<DateTime, bool>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, bool>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(sbyte))
				return new TypeConverter<DateTime, sbyte>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, sbyte>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(float))
				return new TypeConverter<DateTime, float>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, float>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(double))
				return new TypeConverter<DateTime, double>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, double>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(decimal))
				return new TypeConverter<DateTime, decimal>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, decimal>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(string))
				return new TypeConverter<DateTime, string>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, string>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(short))
				return new TypeConverter<DateTime, short>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, short>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(int))
				return new TypeConverter<DateTime, int>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, int>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(long))
				return new TypeConverter<DateTime, long>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, long>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(IndexList))
				return new TypeConverter<DateTime, IndexList>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, IndexList>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<DateTime, WeightedIndexList>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, WeightedIndexList>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(BinaryData))
				return new TypeConverter<DateTime, BinaryData>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, BinaryData>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(ReadOnlyVector<float>))
				return new TypeConverter<DateTime, ReadOnlyVector<float>>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, ReadOnlyVector<float>>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(ReadOnlyMatrix<float>))
				return new TypeConverter<DateTime, ReadOnlyMatrix<float>>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, ReadOnlyMatrix<float>>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(ReadOnlyTensor3D<float>))
				return new TypeConverter<DateTime, ReadOnlyTensor3D<float>>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, ReadOnlyTensor3D<float>>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(ReadOnlyTensor4D<float>))
				return new TypeConverter<DateTime, ReadOnlyTensor4D<float>>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, ReadOnlyTensor4D<float>>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(TimeOnly))
				return new TypeConverter<DateTime, TimeOnly>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, TimeOnly>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(DateOnly))
				return new TypeConverter<DateTime, DateOnly>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, DateOnly>)converter);
			if(type1 == typeof(DateTime) && type2 == typeof(DateTime))
				return new TypeConverter<DateTime, DateTime>((IReadOnlyBuffer<DateTime>)from, (ICanConvert<DateTime, DateTime>)converter);
			throw new NotImplementedException($"Could not create TypeConverter for types {type1} and {type2}");
        }

        internal static IOperation IndexedCopyOperation(IReadOnlyBuffer from, IAppendToBuffer to, IEnumerable<uint> indices)
        {
            var type = from.DataType;
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new IndexedCopyOperation<bool>((IReadOnlyBuffer<bool>)from, (IAppendToBuffer<bool>)to, indices);
			if(typeCode == TypeCode.SByte)
				return new IndexedCopyOperation<sbyte>((IReadOnlyBuffer<sbyte>)from, (IAppendToBuffer<sbyte>)to, indices);
			if(typeCode == TypeCode.Single)
				return new IndexedCopyOperation<float>((IReadOnlyBuffer<float>)from, (IAppendToBuffer<float>)to, indices);
			if(typeCode == TypeCode.Double)
				return new IndexedCopyOperation<double>((IReadOnlyBuffer<double>)from, (IAppendToBuffer<double>)to, indices);
			if(typeCode == TypeCode.Decimal)
				return new IndexedCopyOperation<decimal>((IReadOnlyBuffer<decimal>)from, (IAppendToBuffer<decimal>)to, indices);
			if(typeCode == TypeCode.String)
				return new IndexedCopyOperation<string>((IReadOnlyBuffer<string>)from, (IAppendToBuffer<string>)to, indices);
			if(typeCode == TypeCode.Int16)
				return new IndexedCopyOperation<short>((IReadOnlyBuffer<short>)from, (IAppendToBuffer<short>)to, indices);
			if(typeCode == TypeCode.Int32)
				return new IndexedCopyOperation<int>((IReadOnlyBuffer<int>)from, (IAppendToBuffer<int>)to, indices);
			if(typeCode == TypeCode.Int64)
				return new IndexedCopyOperation<long>((IReadOnlyBuffer<long>)from, (IAppendToBuffer<long>)to, indices);
			if(typeCode == TypeCode.DateTime)
				return new IndexedCopyOperation<DateTime>((IReadOnlyBuffer<DateTime>)from, (IAppendToBuffer<DateTime>)to, indices);
			if(type == typeof(IndexList))
				return new IndexedCopyOperation<IndexList>((IReadOnlyBuffer<IndexList>)from, (IAppendToBuffer<IndexList>)to, indices);
			if(type == typeof(WeightedIndexList))
				return new IndexedCopyOperation<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)from, (IAppendToBuffer<WeightedIndexList>)to, indices);
			if(type == typeof(BinaryData))
				return new IndexedCopyOperation<BinaryData>((IReadOnlyBuffer<BinaryData>)from, (IAppendToBuffer<BinaryData>)to, indices);
			if(type == typeof(ReadOnlyVector<float>))
				return new IndexedCopyOperation<ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (IAppendToBuffer<ReadOnlyVector<float>>)to, indices);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new IndexedCopyOperation<ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (IAppendToBuffer<ReadOnlyMatrix<float>>)to, indices);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new IndexedCopyOperation<ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (IAppendToBuffer<ReadOnlyTensor3D<float>>)to, indices);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new IndexedCopyOperation<ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (IAppendToBuffer<ReadOnlyTensor4D<float>>)to, indices);
			if(type == typeof(TimeOnly))
				return new IndexedCopyOperation<TimeOnly>((IReadOnlyBuffer<TimeOnly>)from, (IAppendToBuffer<TimeOnly>)to, indices);
			if(type == typeof(DateOnly))
				return new IndexedCopyOperation<DateOnly>((IReadOnlyBuffer<DateOnly>)from, (IAppendToBuffer<DateOnly>)to, indices);
			if(type == typeof(DateTime))
				return new IndexedCopyOperation<DateTime>((IReadOnlyBuffer<DateTime>)from, (IAppendToBuffer<DateTime>)to, indices);
			throw new NotImplementedException($"Could not create IndexedCopyOperation for type {type}");
        }

        internal static IOperation BufferCopyOperation(IReadOnlyBuffer from, IAppendBlocks to, Action? onComplete = null)
        {
            var type = from.DataType;
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new BufferCopyOperation<bool>((IReadOnlyBuffer<bool>)from, (IAppendBlocks<bool>)to, onComplete);
			if(typeCode == TypeCode.SByte)
				return new BufferCopyOperation<sbyte>((IReadOnlyBuffer<sbyte>)from, (IAppendBlocks<sbyte>)to, onComplete);
			if(typeCode == TypeCode.Single)
				return new BufferCopyOperation<float>((IReadOnlyBuffer<float>)from, (IAppendBlocks<float>)to, onComplete);
			if(typeCode == TypeCode.Double)
				return new BufferCopyOperation<double>((IReadOnlyBuffer<double>)from, (IAppendBlocks<double>)to, onComplete);
			if(typeCode == TypeCode.Decimal)
				return new BufferCopyOperation<decimal>((IReadOnlyBuffer<decimal>)from, (IAppendBlocks<decimal>)to, onComplete);
			if(typeCode == TypeCode.String)
				return new BufferCopyOperation<string>((IReadOnlyBuffer<string>)from, (IAppendBlocks<string>)to, onComplete);
			if(typeCode == TypeCode.Int16)
				return new BufferCopyOperation<short>((IReadOnlyBuffer<short>)from, (IAppendBlocks<short>)to, onComplete);
			if(typeCode == TypeCode.Int32)
				return new BufferCopyOperation<int>((IReadOnlyBuffer<int>)from, (IAppendBlocks<int>)to, onComplete);
			if(typeCode == TypeCode.Int64)
				return new BufferCopyOperation<long>((IReadOnlyBuffer<long>)from, (IAppendBlocks<long>)to, onComplete);
			if(typeCode == TypeCode.DateTime)
				return new BufferCopyOperation<DateTime>((IReadOnlyBuffer<DateTime>)from, (IAppendBlocks<DateTime>)to, onComplete);
			if(type == typeof(IndexList))
				return new BufferCopyOperation<IndexList>((IReadOnlyBuffer<IndexList>)from, (IAppendBlocks<IndexList>)to, onComplete);
			if(type == typeof(WeightedIndexList))
				return new BufferCopyOperation<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)from, (IAppendBlocks<WeightedIndexList>)to, onComplete);
			if(type == typeof(BinaryData))
				return new BufferCopyOperation<BinaryData>((IReadOnlyBuffer<BinaryData>)from, (IAppendBlocks<BinaryData>)to, onComplete);
			if(type == typeof(ReadOnlyVector<float>))
				return new BufferCopyOperation<ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)from, (IAppendBlocks<ReadOnlyVector<float>>)to, onComplete);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new BufferCopyOperation<ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)from, (IAppendBlocks<ReadOnlyMatrix<float>>)to, onComplete);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new BufferCopyOperation<ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)from, (IAppendBlocks<ReadOnlyTensor3D<float>>)to, onComplete);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new BufferCopyOperation<ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)from, (IAppendBlocks<ReadOnlyTensor4D<float>>)to, onComplete);
			if(type == typeof(TimeOnly))
				return new BufferCopyOperation<TimeOnly>((IReadOnlyBuffer<TimeOnly>)from, (IAppendBlocks<TimeOnly>)to, onComplete);
			if(type == typeof(DateOnly))
				return new BufferCopyOperation<DateOnly>((IReadOnlyBuffer<DateOnly>)from, (IAppendBlocks<DateOnly>)to, onComplete);
			if(type == typeof(DateTime))
				return new BufferCopyOperation<DateTime>((IReadOnlyBuffer<DateTime>)from, (IAppendBlocks<DateTime>)to, onComplete);
			throw new NotImplementedException($"Could not create BufferCopyOperation for type {type}");
        }

        internal static ISimpleNumericAnalysis SimpleNumericAnalysis(IReadOnlyBuffer buffer)
        {
            var type = buffer.DataType;
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new SimpleNumericAnalysis<bool>((IReadOnlyBuffer<bool>)buffer);
			if(typeCode == TypeCode.SByte)
				return new SimpleNumericAnalysis<sbyte>((IReadOnlyBuffer<sbyte>)buffer);
			if(typeCode == TypeCode.Single)
				return new SimpleNumericAnalysis<float>((IReadOnlyBuffer<float>)buffer);
			if(typeCode == TypeCode.Double)
				return new SimpleNumericAnalysis<double>((IReadOnlyBuffer<double>)buffer);
			if(typeCode == TypeCode.Decimal)
				return new SimpleNumericAnalysis<decimal>((IReadOnlyBuffer<decimal>)buffer);
			if(typeCode == TypeCode.String)
				return new SimpleNumericAnalysis<string>((IReadOnlyBuffer<string>)buffer);
			if(typeCode == TypeCode.Int16)
				return new SimpleNumericAnalysis<short>((IReadOnlyBuffer<short>)buffer);
			if(typeCode == TypeCode.Int32)
				return new SimpleNumericAnalysis<int>((IReadOnlyBuffer<int>)buffer);
			if(typeCode == TypeCode.Int64)
				return new SimpleNumericAnalysis<long>((IReadOnlyBuffer<long>)buffer);
			if(typeCode == TypeCode.DateTime)
				return new SimpleNumericAnalysis<DateTime>((IReadOnlyBuffer<DateTime>)buffer);
			if(type == typeof(IndexList))
				return new SimpleNumericAnalysis<IndexList>((IReadOnlyBuffer<IndexList>)buffer);
			if(type == typeof(WeightedIndexList))
				return new SimpleNumericAnalysis<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)buffer);
			if(type == typeof(BinaryData))
				return new SimpleNumericAnalysis<BinaryData>((IReadOnlyBuffer<BinaryData>)buffer);
			if(type == typeof(ReadOnlyVector<float>))
				return new SimpleNumericAnalysis<ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)buffer);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new SimpleNumericAnalysis<ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)buffer);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new SimpleNumericAnalysis<ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)buffer);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new SimpleNumericAnalysis<ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)buffer);
			if(type == typeof(TimeOnly))
				return new SimpleNumericAnalysis<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer);
			if(type == typeof(DateOnly))
				return new SimpleNumericAnalysis<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer);
			if(type == typeof(DateTime))
				return new SimpleNumericAnalysis<DateTime>((IReadOnlyBuffer<DateTime>)buffer);
			throw new NotImplementedException($"Could not create SimpleNumericAnalysis for type {type}");
        }

        internal static ICanConvert ConvertToDecimal(Type type, bool throwOnFailure = false)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ConvertToDecimal<bool>(throwOnFailure);
			if(typeCode == TypeCode.SByte)
				return new ConvertToDecimal<sbyte>(throwOnFailure);
			if(typeCode == TypeCode.Single)
				return new ConvertToDecimal<float>(throwOnFailure);
			if(typeCode == TypeCode.Double)
				return new ConvertToDecimal<double>(throwOnFailure);
			if(typeCode == TypeCode.Decimal)
				return new ConvertToDecimal<decimal>(throwOnFailure);
			if(typeCode == TypeCode.Int16)
				return new ConvertToDecimal<short>(throwOnFailure);
			if(typeCode == TypeCode.Int32)
				return new ConvertToDecimal<int>(throwOnFailure);
			if(typeCode == TypeCode.Int64)
				return new ConvertToDecimal<long>(throwOnFailure);
			if(typeCode == TypeCode.String)
				return new ConvertToDecimal<string>(throwOnFailure);
			throw new NotImplementedException($"Could not create ConvertToDecimal for type {type}");
        }

        internal static ICanConvert ConvertToDouble(Type type, bool throwOnFailure = false)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ConvertToDouble<bool>(throwOnFailure);
			if(typeCode == TypeCode.SByte)
				return new ConvertToDouble<sbyte>(throwOnFailure);
			if(typeCode == TypeCode.Single)
				return new ConvertToDouble<float>(throwOnFailure);
			if(typeCode == TypeCode.Double)
				return new ConvertToDouble<double>(throwOnFailure);
			if(typeCode == TypeCode.Decimal)
				return new ConvertToDouble<decimal>(throwOnFailure);
			if(typeCode == TypeCode.Int16)
				return new ConvertToDouble<short>(throwOnFailure);
			if(typeCode == TypeCode.Int32)
				return new ConvertToDouble<int>(throwOnFailure);
			if(typeCode == TypeCode.Int64)
				return new ConvertToDouble<long>(throwOnFailure);
			if(typeCode == TypeCode.String)
				return new ConvertToDouble<string>(throwOnFailure);
			throw new NotImplementedException($"Could not create ConvertToDouble for type {type}");
        }

        internal static ICanConvert ConvertToFloat(Type type, bool throwOnFailure = false)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ConvertToFloat<bool>(throwOnFailure);
			if(typeCode == TypeCode.SByte)
				return new ConvertToFloat<sbyte>(throwOnFailure);
			if(typeCode == TypeCode.Single)
				return new ConvertToFloat<float>(throwOnFailure);
			if(typeCode == TypeCode.Double)
				return new ConvertToFloat<double>(throwOnFailure);
			if(typeCode == TypeCode.Decimal)
				return new ConvertToFloat<decimal>(throwOnFailure);
			if(typeCode == TypeCode.Int16)
				return new ConvertToFloat<short>(throwOnFailure);
			if(typeCode == TypeCode.Int32)
				return new ConvertToFloat<int>(throwOnFailure);
			if(typeCode == TypeCode.Int64)
				return new ConvertToFloat<long>(throwOnFailure);
			if(typeCode == TypeCode.String)
				return new ConvertToFloat<string>(throwOnFailure);
			throw new NotImplementedException($"Could not create ConvertToFloat for type {type}");
        }

        internal static ICanConvert ConvertToInt(Type type, bool throwOnFailure = false)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ConvertToInt<bool>(throwOnFailure);
			if(typeCode == TypeCode.SByte)
				return new ConvertToInt<sbyte>(throwOnFailure);
			if(typeCode == TypeCode.Single)
				return new ConvertToInt<float>(throwOnFailure);
			if(typeCode == TypeCode.Double)
				return new ConvertToInt<double>(throwOnFailure);
			if(typeCode == TypeCode.Decimal)
				return new ConvertToInt<decimal>(throwOnFailure);
			if(typeCode == TypeCode.Int16)
				return new ConvertToInt<short>(throwOnFailure);
			if(typeCode == TypeCode.Int32)
				return new ConvertToInt<int>(throwOnFailure);
			if(typeCode == TypeCode.Int64)
				return new ConvertToInt<long>(throwOnFailure);
			if(typeCode == TypeCode.String)
				return new ConvertToInt<string>(throwOnFailure);
			throw new NotImplementedException($"Could not create ConvertToInt for type {type}");
        }

        internal static ICanConvert ConvertToLong(Type type, bool throwOnFailure = false)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ConvertToLong<bool>(throwOnFailure);
			if(typeCode == TypeCode.SByte)
				return new ConvertToLong<sbyte>(throwOnFailure);
			if(typeCode == TypeCode.Single)
				return new ConvertToLong<float>(throwOnFailure);
			if(typeCode == TypeCode.Double)
				return new ConvertToLong<double>(throwOnFailure);
			if(typeCode == TypeCode.Decimal)
				return new ConvertToLong<decimal>(throwOnFailure);
			if(typeCode == TypeCode.Int16)
				return new ConvertToLong<short>(throwOnFailure);
			if(typeCode == TypeCode.Int32)
				return new ConvertToLong<int>(throwOnFailure);
			if(typeCode == TypeCode.Int64)
				return new ConvertToLong<long>(throwOnFailure);
			if(typeCode == TypeCode.String)
				return new ConvertToLong<string>(throwOnFailure);
			throw new NotImplementedException($"Could not create ConvertToLong for type {type}");
        }

         internal static ICanConvert ConvertToShort(Type type, bool throwOnFailure = false)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ConvertToShort<bool>(throwOnFailure);
			if(typeCode == TypeCode.SByte)
				return new ConvertToShort<sbyte>(throwOnFailure);
			if(typeCode == TypeCode.Single)
				return new ConvertToShort<float>(throwOnFailure);
			if(typeCode == TypeCode.Double)
				return new ConvertToShort<double>(throwOnFailure);
			if(typeCode == TypeCode.Decimal)
				return new ConvertToShort<decimal>(throwOnFailure);
			if(typeCode == TypeCode.Int16)
				return new ConvertToShort<short>(throwOnFailure);
			if(typeCode == TypeCode.Int32)
				return new ConvertToShort<int>(throwOnFailure);
			if(typeCode == TypeCode.Int64)
				return new ConvertToShort<long>(throwOnFailure);
			if(typeCode == TypeCode.String)
				return new ConvertToShort<string>(throwOnFailure);
			throw new NotImplementedException($"Could not create ConvertToShort for type {type}");
        }

        internal static ICanConvert ConvertToSignedByte(Type type, bool throwOnFailure = false)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ConvertToSignedByte<bool>(throwOnFailure);
			if(typeCode == TypeCode.SByte)
				return new ConvertToSignedByte<sbyte>(throwOnFailure);
			if(typeCode == TypeCode.Single)
				return new ConvertToSignedByte<float>(throwOnFailure);
			if(typeCode == TypeCode.Double)
				return new ConvertToSignedByte<double>(throwOnFailure);
			if(typeCode == TypeCode.Decimal)
				return new ConvertToSignedByte<decimal>(throwOnFailure);
			if(typeCode == TypeCode.Int16)
				return new ConvertToSignedByte<short>(throwOnFailure);
			if(typeCode == TypeCode.Int32)
				return new ConvertToSignedByte<int>(throwOnFailure);
			if(typeCode == TypeCode.Int64)
				return new ConvertToSignedByte<long>(throwOnFailure);
			if(typeCode == TypeCode.String)
				return new ConvertToSignedByte<string>(throwOnFailure);
			throw new NotImplementedException($"Could not create ConvertToSignedByte for type {type}");
        }

        internal static IAppendBlocks ColumnFilter(Type type, uint columnIndex, BrightDataType columnType, HashSet<uint> nonConformingRowIndices, IDataTypeSpecification typeSpecification)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new ColumnFilter<bool>(columnIndex, columnType, (IDataTypeSpecification<bool>)typeSpecification, nonConformingRowIndices);
			if(typeCode == TypeCode.SByte)
				return new ColumnFilter<sbyte>(columnIndex, columnType, (IDataTypeSpecification<sbyte>)typeSpecification, nonConformingRowIndices);
			if(typeCode == TypeCode.Single)
				return new ColumnFilter<float>(columnIndex, columnType, (IDataTypeSpecification<float>)typeSpecification, nonConformingRowIndices);
			if(typeCode == TypeCode.Double)
				return new ColumnFilter<double>(columnIndex, columnType, (IDataTypeSpecification<double>)typeSpecification, nonConformingRowIndices);
			if(typeCode == TypeCode.Decimal)
				return new ColumnFilter<decimal>(columnIndex, columnType, (IDataTypeSpecification<decimal>)typeSpecification, nonConformingRowIndices);
			if(typeCode == TypeCode.String)
				return new ColumnFilter<string>(columnIndex, columnType, (IDataTypeSpecification<string>)typeSpecification, nonConformingRowIndices);
			if(typeCode == TypeCode.Int16)
				return new ColumnFilter<short>(columnIndex, columnType, (IDataTypeSpecification<short>)typeSpecification, nonConformingRowIndices);
			if(typeCode == TypeCode.Int32)
				return new ColumnFilter<int>(columnIndex, columnType, (IDataTypeSpecification<int>)typeSpecification, nonConformingRowIndices);
			if(typeCode == TypeCode.Int64)
				return new ColumnFilter<long>(columnIndex, columnType, (IDataTypeSpecification<long>)typeSpecification, nonConformingRowIndices);
			if(typeCode == TypeCode.DateTime)
				return new ColumnFilter<DateTime>(columnIndex, columnType, (IDataTypeSpecification<DateTime>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(IndexList))
				return new ColumnFilter<IndexList>(columnIndex, columnType, (IDataTypeSpecification<IndexList>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(WeightedIndexList))
				return new ColumnFilter<WeightedIndexList>(columnIndex, columnType, (IDataTypeSpecification<WeightedIndexList>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(BinaryData))
				return new ColumnFilter<BinaryData>(columnIndex, columnType, (IDataTypeSpecification<BinaryData>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(ReadOnlyVector<float>))
				return new ColumnFilter<ReadOnlyVector<float>>(columnIndex, columnType, (IDataTypeSpecification<ReadOnlyVector<float>>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new ColumnFilter<ReadOnlyMatrix<float>>(columnIndex, columnType, (IDataTypeSpecification<ReadOnlyMatrix<float>>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new ColumnFilter<ReadOnlyTensor3D<float>>(columnIndex, columnType, (IDataTypeSpecification<ReadOnlyTensor3D<float>>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new ColumnFilter<ReadOnlyTensor4D<float>>(columnIndex, columnType, (IDataTypeSpecification<ReadOnlyTensor4D<float>>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(TimeOnly))
				return new ColumnFilter<TimeOnly>(columnIndex, columnType, (IDataTypeSpecification<TimeOnly>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(DateOnly))
				return new ColumnFilter<DateOnly>(columnIndex, columnType, (IDataTypeSpecification<DateOnly>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(DateTime))
				return new ColumnFilter<DateTime>(columnIndex, columnType, (IDataTypeSpecification<DateTime>)typeSpecification, nonConformingRowIndices);
			throw new NotImplementedException($"Could not create ColumnFilter for type {type}");
        }

        internal static IOperation TypedIndexer(IReadOnlyBuffer buffer)
        {
            var type = buffer.DataType;
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new TypedIndexer<bool>((IReadOnlyBuffer<bool>)buffer);
			if(typeCode == TypeCode.SByte)
				return new TypedIndexer<sbyte>((IReadOnlyBuffer<sbyte>)buffer);
			if(typeCode == TypeCode.Single)
				return new TypedIndexer<float>((IReadOnlyBuffer<float>)buffer);
			if(typeCode == TypeCode.Double)
				return new TypedIndexer<double>((IReadOnlyBuffer<double>)buffer);
			if(typeCode == TypeCode.Decimal)
				return new TypedIndexer<decimal>((IReadOnlyBuffer<decimal>)buffer);
			if(typeCode == TypeCode.String)
				return new TypedIndexer<string>((IReadOnlyBuffer<string>)buffer);
			if(typeCode == TypeCode.Int16)
				return new TypedIndexer<short>((IReadOnlyBuffer<short>)buffer);
			if(typeCode == TypeCode.Int32)
				return new TypedIndexer<int>((IReadOnlyBuffer<int>)buffer);
			if(typeCode == TypeCode.Int64)
				return new TypedIndexer<long>((IReadOnlyBuffer<long>)buffer);
			if(typeCode == TypeCode.DateTime)
				return new TypedIndexer<DateTime>((IReadOnlyBuffer<DateTime>)buffer);
			if(type == typeof(IndexList))
				return new TypedIndexer<IndexList>((IReadOnlyBuffer<IndexList>)buffer);
			if(type == typeof(WeightedIndexList))
				return new TypedIndexer<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)buffer);
			if(type == typeof(BinaryData))
				return new TypedIndexer<BinaryData>((IReadOnlyBuffer<BinaryData>)buffer);
			if(type == typeof(ReadOnlyVector<float>))
				return new TypedIndexer<ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)buffer);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new TypedIndexer<ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)buffer);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new TypedIndexer<ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)buffer);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new TypedIndexer<ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)buffer);
			if(type == typeof(TimeOnly))
				return new TypedIndexer<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer);
			if(type == typeof(DateOnly))
				return new TypedIndexer<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer);
			if(type == typeof(DateTime))
				return new TypedIndexer<DateTime>((IReadOnlyBuffer<DateTime>)buffer);
			throw new NotImplementedException($"Could not create TypedIndexer for type {type}");
        }

        internal static IReadOnlyBuffer<ReadOnlyVector<float>> OneHotConverter(IReadOnlyBuffer buffer, ICanIndex indexer)
        {
            var type = buffer.DataType;
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new OneHotConverter<bool>((IReadOnlyBuffer<bool>)buffer, (ICanIndex<bool>)indexer);
			if(typeCode == TypeCode.SByte)
				return new OneHotConverter<sbyte>((IReadOnlyBuffer<sbyte>)buffer, (ICanIndex<sbyte>)indexer);
			if(typeCode == TypeCode.Single)
				return new OneHotConverter<float>((IReadOnlyBuffer<float>)buffer, (ICanIndex<float>)indexer);
			if(typeCode == TypeCode.Double)
				return new OneHotConverter<double>((IReadOnlyBuffer<double>)buffer, (ICanIndex<double>)indexer);
			if(typeCode == TypeCode.Decimal)
				return new OneHotConverter<decimal>((IReadOnlyBuffer<decimal>)buffer, (ICanIndex<decimal>)indexer);
			if(typeCode == TypeCode.String)
				return new OneHotConverter<string>((IReadOnlyBuffer<string>)buffer, (ICanIndex<string>)indexer);
			if(typeCode == TypeCode.Int16)
				return new OneHotConverter<short>((IReadOnlyBuffer<short>)buffer, (ICanIndex<short>)indexer);
			if(typeCode == TypeCode.Int32)
				return new OneHotConverter<int>((IReadOnlyBuffer<int>)buffer, (ICanIndex<int>)indexer);
			if(typeCode == TypeCode.Int64)
				return new OneHotConverter<long>((IReadOnlyBuffer<long>)buffer, (ICanIndex<long>)indexer);
			if(typeCode == TypeCode.DateTime)
				return new OneHotConverter<DateTime>((IReadOnlyBuffer<DateTime>)buffer, (ICanIndex<DateTime>)indexer);
			if(type == typeof(IndexList))
				return new OneHotConverter<IndexList>((IReadOnlyBuffer<IndexList>)buffer, (ICanIndex<IndexList>)indexer);
			if(type == typeof(WeightedIndexList))
				return new OneHotConverter<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)buffer, (ICanIndex<WeightedIndexList>)indexer);
			if(type == typeof(BinaryData))
				return new OneHotConverter<BinaryData>((IReadOnlyBuffer<BinaryData>)buffer, (ICanIndex<BinaryData>)indexer);
			if(type == typeof(ReadOnlyVector<float>))
				return new OneHotConverter<ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)buffer, (ICanIndex<ReadOnlyVector<float>>)indexer);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new OneHotConverter<ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)buffer, (ICanIndex<ReadOnlyMatrix<float>>)indexer);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new OneHotConverter<ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)buffer, (ICanIndex<ReadOnlyTensor3D<float>>)indexer);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new OneHotConverter<ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)buffer, (ICanIndex<ReadOnlyTensor4D<float>>)indexer);
			if(type == typeof(TimeOnly))
				return new OneHotConverter<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer, (ICanIndex<TimeOnly>)indexer);
			if(type == typeof(DateOnly))
				return new OneHotConverter<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer, (ICanIndex<DateOnly>)indexer);
			if(type == typeof(DateTime))
				return new OneHotConverter<DateTime>((IReadOnlyBuffer<DateTime>)buffer, (ICanIndex<DateTime>)indexer);
			throw new NotImplementedException($"Could not create OneHotConverter for type {type}");
        }

        internal static IReadOnlyBuffer<int> CategoricalIndexConverter(IReadOnlyBuffer buffer, ICanIndex indexer)
        {
            var type = buffer.DataType;
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new CategoricalIndexConverter<bool>((IReadOnlyBuffer<bool>)buffer, (ICanIndex<bool>)indexer);
			if(typeCode == TypeCode.SByte)
				return new CategoricalIndexConverter<sbyte>((IReadOnlyBuffer<sbyte>)buffer, (ICanIndex<sbyte>)indexer);
			if(typeCode == TypeCode.Single)
				return new CategoricalIndexConverter<float>((IReadOnlyBuffer<float>)buffer, (ICanIndex<float>)indexer);
			if(typeCode == TypeCode.Double)
				return new CategoricalIndexConverter<double>((IReadOnlyBuffer<double>)buffer, (ICanIndex<double>)indexer);
			if(typeCode == TypeCode.Decimal)
				return new CategoricalIndexConverter<decimal>((IReadOnlyBuffer<decimal>)buffer, (ICanIndex<decimal>)indexer);
			if(typeCode == TypeCode.String)
				return new CategoricalIndexConverter<string>((IReadOnlyBuffer<string>)buffer, (ICanIndex<string>)indexer);
			if(typeCode == TypeCode.Int16)
				return new CategoricalIndexConverter<short>((IReadOnlyBuffer<short>)buffer, (ICanIndex<short>)indexer);
			if(typeCode == TypeCode.Int32)
				return new CategoricalIndexConverter<int>((IReadOnlyBuffer<int>)buffer, (ICanIndex<int>)indexer);
			if(typeCode == TypeCode.Int64)
				return new CategoricalIndexConverter<long>((IReadOnlyBuffer<long>)buffer, (ICanIndex<long>)indexer);
			if(typeCode == TypeCode.DateTime)
				return new CategoricalIndexConverter<DateTime>((IReadOnlyBuffer<DateTime>)buffer, (ICanIndex<DateTime>)indexer);
			if(type == typeof(IndexList))
				return new CategoricalIndexConverter<IndexList>((IReadOnlyBuffer<IndexList>)buffer, (ICanIndex<IndexList>)indexer);
			if(type == typeof(WeightedIndexList))
				return new CategoricalIndexConverter<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)buffer, (ICanIndex<WeightedIndexList>)indexer);
			if(type == typeof(BinaryData))
				return new CategoricalIndexConverter<BinaryData>((IReadOnlyBuffer<BinaryData>)buffer, (ICanIndex<BinaryData>)indexer);
			if(type == typeof(ReadOnlyVector<float>))
				return new CategoricalIndexConverter<ReadOnlyVector<float>>((IReadOnlyBuffer<ReadOnlyVector<float>>)buffer, (ICanIndex<ReadOnlyVector<float>>)indexer);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new CategoricalIndexConverter<ReadOnlyMatrix<float>>((IReadOnlyBuffer<ReadOnlyMatrix<float>>)buffer, (ICanIndex<ReadOnlyMatrix<float>>)indexer);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new CategoricalIndexConverter<ReadOnlyTensor3D<float>>((IReadOnlyBuffer<ReadOnlyTensor3D<float>>)buffer, (ICanIndex<ReadOnlyTensor3D<float>>)indexer);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new CategoricalIndexConverter<ReadOnlyTensor4D<float>>((IReadOnlyBuffer<ReadOnlyTensor4D<float>>)buffer, (ICanIndex<ReadOnlyTensor4D<float>>)indexer);
			if(type == typeof(TimeOnly))
				return new CategoricalIndexConverter<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer, (ICanIndex<TimeOnly>)indexer);
			if(type == typeof(DateOnly))
				return new CategoricalIndexConverter<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer, (ICanIndex<DateOnly>)indexer);
			if(type == typeof(DateTime))
				return new CategoricalIndexConverter<DateTime>((IReadOnlyBuffer<DateTime>)buffer, (ICanIndex<DateTime>)indexer);
			throw new NotImplementedException($"Could not create CategoricalIndexConverter for type {type}");
        }

        internal static IReadOnlyBuffer NormalizationConverter(IReadOnlyBuffer buffer, INormalize normalization)
        {
            var type = buffer.DataType;
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.SByte)
				return new NormalizationConverter<sbyte>((IReadOnlyBuffer<sbyte>)buffer, normalization);
			if(typeCode == TypeCode.Single)
				return new NormalizationConverter<float>((IReadOnlyBuffer<float>)buffer, normalization);
			if(typeCode == TypeCode.Double)
				return new NormalizationConverter<double>((IReadOnlyBuffer<double>)buffer, normalization);
			if(typeCode == TypeCode.Decimal)
				return new NormalizationConverter<decimal>((IReadOnlyBuffer<decimal>)buffer, normalization);
			if(typeCode == TypeCode.Int16)
				return new NormalizationConverter<short>((IReadOnlyBuffer<short>)buffer, normalization);
			if(typeCode == TypeCode.Int32)
				return new NormalizationConverter<int>((IReadOnlyBuffer<int>)buffer, normalization);
			if(typeCode == TypeCode.Int64)
				return new NormalizationConverter<long>((IReadOnlyBuffer<long>)buffer, normalization);
			throw new NotImplementedException($"Could not create NormalizationConverter for type {type}");
        }

        internal static ICanVectorise NumericVectoriser(Type type)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new NumericVectoriser<bool>();
			if(typeCode == TypeCode.SByte)
				return new NumericVectoriser<sbyte>();
			if(typeCode == TypeCode.Single)
				return new NumericVectoriser<float>();
			if(typeCode == TypeCode.Double)
				return new NumericVectoriser<double>();
			if(typeCode == TypeCode.Decimal)
				return new NumericVectoriser<decimal>();
			if(typeCode == TypeCode.Int16)
				return new NumericVectoriser<short>();
			if(typeCode == TypeCode.Int32)
				return new NumericVectoriser<int>();
			if(typeCode == TypeCode.Int64)
				return new NumericVectoriser<long>();
			if(typeCode == TypeCode.String)
				return new NumericVectoriser<string>();
			throw new NotImplementedException($"Could not create NumericVectoriser for type {type}");
        }

        internal static ICanVectorise CategoricalIndexVectoriser(Type type)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new CategoricalIndexVectoriser<bool>();
			if(typeCode == TypeCode.SByte)
				return new CategoricalIndexVectoriser<sbyte>();
			if(typeCode == TypeCode.Single)
				return new CategoricalIndexVectoriser<float>();
			if(typeCode == TypeCode.Double)
				return new CategoricalIndexVectoriser<double>();
			if(typeCode == TypeCode.Decimal)
				return new CategoricalIndexVectoriser<decimal>();
			if(typeCode == TypeCode.String)
				return new CategoricalIndexVectoriser<string>();
			if(typeCode == TypeCode.Int16)
				return new CategoricalIndexVectoriser<short>();
			if(typeCode == TypeCode.Int32)
				return new CategoricalIndexVectoriser<int>();
			if(typeCode == TypeCode.Int64)
				return new CategoricalIndexVectoriser<long>();
			if(typeCode == TypeCode.DateTime)
				return new CategoricalIndexVectoriser<DateTime>();
			if(type == typeof(IndexList))
				return new CategoricalIndexVectoriser<IndexList>();
			if(type == typeof(WeightedIndexList))
				return new CategoricalIndexVectoriser<WeightedIndexList>();
			if(type == typeof(BinaryData))
				return new CategoricalIndexVectoriser<BinaryData>();
			if(type == typeof(ReadOnlyVector<float>))
				return new CategoricalIndexVectoriser<ReadOnlyVector<float>>();
			if(type == typeof(ReadOnlyMatrix<float>))
				return new CategoricalIndexVectoriser<ReadOnlyMatrix<float>>();
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new CategoricalIndexVectoriser<ReadOnlyTensor3D<float>>();
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new CategoricalIndexVectoriser<ReadOnlyTensor4D<float>>();
			if(type == typeof(TimeOnly))
				return new CategoricalIndexVectoriser<TimeOnly>();
			if(type == typeof(DateOnly))
				return new CategoricalIndexVectoriser<DateOnly>();
			if(type == typeof(DateTime))
				return new CategoricalIndexVectoriser<DateTime>();
			throw new NotImplementedException($"Could not create CategoricalIndexVectoriser for type {type}");
        }

        internal static ICanVectorise OneHotVectoriser(Type type, uint maxSize)
        {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.Boolean)
				return new OneHotVectoriser<bool>(maxSize);
			if(typeCode == TypeCode.SByte)
				return new OneHotVectoriser<sbyte>(maxSize);
			if(typeCode == TypeCode.Single)
				return new OneHotVectoriser<float>(maxSize);
			if(typeCode == TypeCode.Double)
				return new OneHotVectoriser<double>(maxSize);
			if(typeCode == TypeCode.Decimal)
				return new OneHotVectoriser<decimal>(maxSize);
			if(typeCode == TypeCode.String)
				return new OneHotVectoriser<string>(maxSize);
			if(typeCode == TypeCode.Int16)
				return new OneHotVectoriser<short>(maxSize);
			if(typeCode == TypeCode.Int32)
				return new OneHotVectoriser<int>(maxSize);
			if(typeCode == TypeCode.Int64)
				return new OneHotVectoriser<long>(maxSize);
			if(typeCode == TypeCode.DateTime)
				return new OneHotVectoriser<DateTime>(maxSize);
			if(type == typeof(IndexList))
				return new OneHotVectoriser<IndexList>(maxSize);
			if(type == typeof(WeightedIndexList))
				return new OneHotVectoriser<WeightedIndexList>(maxSize);
			if(type == typeof(BinaryData))
				return new OneHotVectoriser<BinaryData>(maxSize);
			if(type == typeof(ReadOnlyVector<float>))
				return new OneHotVectoriser<ReadOnlyVector<float>>(maxSize);
			if(type == typeof(ReadOnlyMatrix<float>))
				return new OneHotVectoriser<ReadOnlyMatrix<float>>(maxSize);
			if(type == typeof(ReadOnlyTensor3D<float>))
				return new OneHotVectoriser<ReadOnlyTensor3D<float>>(maxSize);
			if(type == typeof(ReadOnlyTensor4D<float>))
				return new OneHotVectoriser<ReadOnlyTensor4D<float>>(maxSize);
			if(type == typeof(TimeOnly))
				return new OneHotVectoriser<TimeOnly>(maxSize);
			if(type == typeof(DateOnly))
				return new OneHotVectoriser<DateOnly>(maxSize);
			if(type == typeof(DateTime))
				return new OneHotVectoriser<DateTime>(maxSize);
			throw new NotImplementedException($"Could not create OneHotVectoriser for type {type}");
        }

        internal static ICompositeBuffer CreateNumericCompositeBuffer(
            Type type,
            IProvideByteBlocks? tempStreams, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
			var typeCode = Type.GetTypeCode(type);
			if(typeCode == TypeCode.SByte)
				return new UnmanagedCompositeBuffer<sbyte>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
			if(typeCode == TypeCode.Single)
				return new UnmanagedCompositeBuffer<float>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
			if(typeCode == TypeCode.Double)
				return new UnmanagedCompositeBuffer<double>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
			if(typeCode == TypeCode.Decimal)
				return new UnmanagedCompositeBuffer<decimal>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
			if(typeCode == TypeCode.Int16)
				return new UnmanagedCompositeBuffer<short>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
			if(typeCode == TypeCode.Int32)
				return new UnmanagedCompositeBuffer<int>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
			if(typeCode == TypeCode.Int64)
				return new UnmanagedCompositeBuffer<long>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems);
			throw new NotImplementedException($"Could not create UnmanagedCompositeBuffer for type {type}");
        }
    }
}

