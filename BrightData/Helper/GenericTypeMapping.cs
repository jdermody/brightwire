using System;
using BrightData.Buffer.ReadOnly.Converter;
using BrightData.Types;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.Analysis;
using BrightData.Buffer.Operations;
using System.Collections.Generic;
using BrightData.Buffer.Operations.Helper;
using BrightData.Converter;
using BrightData.Buffer.Operations.Vectorisation;
using BrightData.DataTable.ConstraintValidation;

namespace BrightData.Helper
{
    internal class GenericTypeMapping
    {
        internal static IReadOnlyBuffer<string> ToStringConverter(IReadOnlyBuffer buffer)
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
				return new ToStringConverter<string>((IReadOnlyBuffer<string>)buffer);
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
			if(type == typeof(ReadOnlyVector))
				return new ToStringConverter<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)buffer);
			if(type == typeof(ReadOnlyMatrix))
				return new ToStringConverter<ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)buffer);
			if(type == typeof(ReadOnlyTensor3D))
				return new ToStringConverter<ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)buffer);
			if(type == typeof(ReadOnlyTensor4D))
				return new ToStringConverter<ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)buffer);
			if(type == typeof(TimeOnly))
				return new ToStringConverter<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer);
			if(type == typeof(DateOnly))
				return new ToStringConverter<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer);
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
			if(type == typeof(ReadOnlyVector))
				return new ConvertToStringFrequencyAnalysis<ReadOnlyVector>(writeCount);
			if(type == typeof(ReadOnlyMatrix))
				return new ConvertToStringFrequencyAnalysis<ReadOnlyMatrix>(writeCount);
			if(type == typeof(ReadOnlyTensor3D))
				return new ConvertToStringFrequencyAnalysis<ReadOnlyTensor3D>(writeCount);
			if(type == typeof(ReadOnlyTensor4D))
				return new ConvertToStringFrequencyAnalysis<ReadOnlyTensor4D>(writeCount);
			if(type == typeof(TimeOnly))
				return new ConvertToStringFrequencyAnalysis<TimeOnly>(writeCount);
			if(type == typeof(DateOnly))
				return new ConvertToStringFrequencyAnalysis<DateOnly>(writeCount);
			throw new NotImplementedException($"Could not create ConvertToStringFrequencyAnalysis for type {type}");
        }

        internal static IReadOnlyBuffer<object> ToObjectConverter(IReadOnlyBuffer from)
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
			if(type == typeof(ReadOnlyVector))
				return new ToObjectConverter<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type == typeof(ReadOnlyMatrix))
				return new ToObjectConverter<ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type == typeof(ReadOnlyTensor3D))
				return new ToObjectConverter<ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type == typeof(ReadOnlyTensor4D))
				return new ToObjectConverter<ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type == typeof(TimeOnly))
				return new ToObjectConverter<TimeOnly>((IReadOnlyBuffer<TimeOnly>)from);
			if(type == typeof(DateOnly))
				return new ToObjectConverter<DateOnly>((IReadOnlyBuffer<DateOnly>)from);
			throw new NotImplementedException($"Could not create ToObjectConverter for type {type}");
        }

        internal static IReadOnlyBuffer CastConverter(Type type2, IReadOnlyBuffer from)
        {
            var type1 = from.DataType;
			if(type1 == typeof(bool) && type2 == typeof(bool))
				return new CastConverter<bool, bool>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(sbyte))
				return new CastConverter<bool, sbyte>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(float))
				return new CastConverter<bool, float>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(double))
				return new CastConverter<bool, double>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(decimal))
				return new CastConverter<bool, decimal>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(string))
				return new CastConverter<bool, string>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(short))
				return new CastConverter<bool, short>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(int))
				return new CastConverter<bool, int>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(long))
				return new CastConverter<bool, long>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(IndexList))
				return new CastConverter<bool, IndexList>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(WeightedIndexList))
				return new CastConverter<bool, WeightedIndexList>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<bool, ReadOnlyVector>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<bool, ReadOnlyMatrix>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<bool, ReadOnlyTensor3D>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<bool, ReadOnlyTensor4D>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(TimeOnly))
				return new CastConverter<bool, TimeOnly>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(bool) && type2 == typeof(DateOnly))
				return new CastConverter<bool, DateOnly>((IReadOnlyBuffer<bool>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(bool))
				return new CastConverter<sbyte, bool>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(sbyte))
				return new CastConverter<sbyte, sbyte>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(float))
				return new CastConverter<sbyte, float>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(double))
				return new CastConverter<sbyte, double>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(decimal))
				return new CastConverter<sbyte, decimal>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(string))
				return new CastConverter<sbyte, string>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(short))
				return new CastConverter<sbyte, short>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(int))
				return new CastConverter<sbyte, int>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(long))
				return new CastConverter<sbyte, long>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(IndexList))
				return new CastConverter<sbyte, IndexList>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(WeightedIndexList))
				return new CastConverter<sbyte, WeightedIndexList>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<sbyte, ReadOnlyVector>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<sbyte, ReadOnlyMatrix>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<sbyte, ReadOnlyTensor3D>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<sbyte, ReadOnlyTensor4D>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(TimeOnly))
				return new CastConverter<sbyte, TimeOnly>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(sbyte) && type2 == typeof(DateOnly))
				return new CastConverter<sbyte, DateOnly>((IReadOnlyBuffer<sbyte>)from);
			if(type1 == typeof(float) && type2 == typeof(bool))
				return new CastConverter<float, bool>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(sbyte))
				return new CastConverter<float, sbyte>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(float))
				return new CastConverter<float, float>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(double))
				return new CastConverter<float, double>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(decimal))
				return new CastConverter<float, decimal>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(string))
				return new CastConverter<float, string>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(short))
				return new CastConverter<float, short>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(int))
				return new CastConverter<float, int>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(long))
				return new CastConverter<float, long>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(IndexList))
				return new CastConverter<float, IndexList>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(WeightedIndexList))
				return new CastConverter<float, WeightedIndexList>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<float, ReadOnlyVector>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<float, ReadOnlyMatrix>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<float, ReadOnlyTensor3D>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<float, ReadOnlyTensor4D>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(TimeOnly))
				return new CastConverter<float, TimeOnly>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(float) && type2 == typeof(DateOnly))
				return new CastConverter<float, DateOnly>((IReadOnlyBuffer<float>)from);
			if(type1 == typeof(double) && type2 == typeof(bool))
				return new CastConverter<double, bool>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(sbyte))
				return new CastConverter<double, sbyte>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(float))
				return new CastConverter<double, float>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(double))
				return new CastConverter<double, double>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(decimal))
				return new CastConverter<double, decimal>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(string))
				return new CastConverter<double, string>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(short))
				return new CastConverter<double, short>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(int))
				return new CastConverter<double, int>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(long))
				return new CastConverter<double, long>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(IndexList))
				return new CastConverter<double, IndexList>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(WeightedIndexList))
				return new CastConverter<double, WeightedIndexList>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<double, ReadOnlyVector>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<double, ReadOnlyMatrix>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<double, ReadOnlyTensor3D>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<double, ReadOnlyTensor4D>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(TimeOnly))
				return new CastConverter<double, TimeOnly>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(double) && type2 == typeof(DateOnly))
				return new CastConverter<double, DateOnly>((IReadOnlyBuffer<double>)from);
			if(type1 == typeof(decimal) && type2 == typeof(bool))
				return new CastConverter<decimal, bool>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(sbyte))
				return new CastConverter<decimal, sbyte>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(float))
				return new CastConverter<decimal, float>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(double))
				return new CastConverter<decimal, double>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(decimal))
				return new CastConverter<decimal, decimal>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(string))
				return new CastConverter<decimal, string>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(short))
				return new CastConverter<decimal, short>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(int))
				return new CastConverter<decimal, int>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(long))
				return new CastConverter<decimal, long>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(IndexList))
				return new CastConverter<decimal, IndexList>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(WeightedIndexList))
				return new CastConverter<decimal, WeightedIndexList>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<decimal, ReadOnlyVector>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<decimal, ReadOnlyMatrix>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<decimal, ReadOnlyTensor3D>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<decimal, ReadOnlyTensor4D>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(TimeOnly))
				return new CastConverter<decimal, TimeOnly>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(decimal) && type2 == typeof(DateOnly))
				return new CastConverter<decimal, DateOnly>((IReadOnlyBuffer<decimal>)from);
			if(type1 == typeof(string) && type2 == typeof(bool))
				return new CastConverter<string, bool>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(sbyte))
				return new CastConverter<string, sbyte>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(float))
				return new CastConverter<string, float>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(double))
				return new CastConverter<string, double>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(decimal))
				return new CastConverter<string, decimal>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(string))
				return new CastConverter<string, string>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(short))
				return new CastConverter<string, short>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(int))
				return new CastConverter<string, int>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(long))
				return new CastConverter<string, long>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(IndexList))
				return new CastConverter<string, IndexList>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(WeightedIndexList))
				return new CastConverter<string, WeightedIndexList>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<string, ReadOnlyVector>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<string, ReadOnlyMatrix>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<string, ReadOnlyTensor3D>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<string, ReadOnlyTensor4D>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(TimeOnly))
				return new CastConverter<string, TimeOnly>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(string) && type2 == typeof(DateOnly))
				return new CastConverter<string, DateOnly>((IReadOnlyBuffer<string>)from);
			if(type1 == typeof(short) && type2 == typeof(bool))
				return new CastConverter<short, bool>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(sbyte))
				return new CastConverter<short, sbyte>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(float))
				return new CastConverter<short, float>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(double))
				return new CastConverter<short, double>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(decimal))
				return new CastConverter<short, decimal>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(string))
				return new CastConverter<short, string>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(short))
				return new CastConverter<short, short>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(int))
				return new CastConverter<short, int>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(long))
				return new CastConverter<short, long>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(IndexList))
				return new CastConverter<short, IndexList>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(WeightedIndexList))
				return new CastConverter<short, WeightedIndexList>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<short, ReadOnlyVector>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<short, ReadOnlyMatrix>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<short, ReadOnlyTensor3D>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<short, ReadOnlyTensor4D>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(TimeOnly))
				return new CastConverter<short, TimeOnly>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(short) && type2 == typeof(DateOnly))
				return new CastConverter<short, DateOnly>((IReadOnlyBuffer<short>)from);
			if(type1 == typeof(int) && type2 == typeof(bool))
				return new CastConverter<int, bool>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(sbyte))
				return new CastConverter<int, sbyte>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(float))
				return new CastConverter<int, float>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(double))
				return new CastConverter<int, double>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(decimal))
				return new CastConverter<int, decimal>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(string))
				return new CastConverter<int, string>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(short))
				return new CastConverter<int, short>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(int))
				return new CastConverter<int, int>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(long))
				return new CastConverter<int, long>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(IndexList))
				return new CastConverter<int, IndexList>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(WeightedIndexList))
				return new CastConverter<int, WeightedIndexList>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<int, ReadOnlyVector>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<int, ReadOnlyMatrix>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<int, ReadOnlyTensor3D>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<int, ReadOnlyTensor4D>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(TimeOnly))
				return new CastConverter<int, TimeOnly>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(int) && type2 == typeof(DateOnly))
				return new CastConverter<int, DateOnly>((IReadOnlyBuffer<int>)from);
			if(type1 == typeof(long) && type2 == typeof(bool))
				return new CastConverter<long, bool>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(sbyte))
				return new CastConverter<long, sbyte>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(float))
				return new CastConverter<long, float>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(double))
				return new CastConverter<long, double>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(decimal))
				return new CastConverter<long, decimal>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(string))
				return new CastConverter<long, string>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(short))
				return new CastConverter<long, short>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(int))
				return new CastConverter<long, int>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(long))
				return new CastConverter<long, long>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(IndexList))
				return new CastConverter<long, IndexList>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(WeightedIndexList))
				return new CastConverter<long, WeightedIndexList>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<long, ReadOnlyVector>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<long, ReadOnlyMatrix>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<long, ReadOnlyTensor3D>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<long, ReadOnlyTensor4D>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(TimeOnly))
				return new CastConverter<long, TimeOnly>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(long) && type2 == typeof(DateOnly))
				return new CastConverter<long, DateOnly>((IReadOnlyBuffer<long>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(bool))
				return new CastConverter<IndexList, bool>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(sbyte))
				return new CastConverter<IndexList, sbyte>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(float))
				return new CastConverter<IndexList, float>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(double))
				return new CastConverter<IndexList, double>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(decimal))
				return new CastConverter<IndexList, decimal>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(string))
				return new CastConverter<IndexList, string>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(short))
				return new CastConverter<IndexList, short>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(int))
				return new CastConverter<IndexList, int>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(long))
				return new CastConverter<IndexList, long>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(IndexList))
				return new CastConverter<IndexList, IndexList>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(WeightedIndexList))
				return new CastConverter<IndexList, WeightedIndexList>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<IndexList, ReadOnlyVector>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<IndexList, ReadOnlyMatrix>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<IndexList, ReadOnlyTensor3D>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<IndexList, ReadOnlyTensor4D>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(TimeOnly))
				return new CastConverter<IndexList, TimeOnly>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(IndexList) && type2 == typeof(DateOnly))
				return new CastConverter<IndexList, DateOnly>((IReadOnlyBuffer<IndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(bool))
				return new CastConverter<WeightedIndexList, bool>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(sbyte))
				return new CastConverter<WeightedIndexList, sbyte>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(float))
				return new CastConverter<WeightedIndexList, float>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(double))
				return new CastConverter<WeightedIndexList, double>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(decimal))
				return new CastConverter<WeightedIndexList, decimal>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(string))
				return new CastConverter<WeightedIndexList, string>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(short))
				return new CastConverter<WeightedIndexList, short>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(int))
				return new CastConverter<WeightedIndexList, int>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(long))
				return new CastConverter<WeightedIndexList, long>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(IndexList))
				return new CastConverter<WeightedIndexList, IndexList>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(WeightedIndexList))
				return new CastConverter<WeightedIndexList, WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<WeightedIndexList, ReadOnlyVector>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<WeightedIndexList, ReadOnlyMatrix>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<WeightedIndexList, ReadOnlyTensor3D>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<WeightedIndexList, ReadOnlyTensor4D>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(TimeOnly))
				return new CastConverter<WeightedIndexList, TimeOnly>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(DateOnly))
				return new CastConverter<WeightedIndexList, DateOnly>((IReadOnlyBuffer<WeightedIndexList>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(bool))
				return new CastConverter<ReadOnlyVector, bool>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(sbyte))
				return new CastConverter<ReadOnlyVector, sbyte>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(float))
				return new CastConverter<ReadOnlyVector, float>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(double))
				return new CastConverter<ReadOnlyVector, double>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(decimal))
				return new CastConverter<ReadOnlyVector, decimal>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(string))
				return new CastConverter<ReadOnlyVector, string>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(short))
				return new CastConverter<ReadOnlyVector, short>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(int))
				return new CastConverter<ReadOnlyVector, int>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(long))
				return new CastConverter<ReadOnlyVector, long>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(IndexList))
				return new CastConverter<ReadOnlyVector, IndexList>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(WeightedIndexList))
				return new CastConverter<ReadOnlyVector, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<ReadOnlyVector, ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<ReadOnlyVector, ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<ReadOnlyVector, ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<ReadOnlyVector, ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(TimeOnly))
				return new CastConverter<ReadOnlyVector, TimeOnly>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(DateOnly))
				return new CastConverter<ReadOnlyVector, DateOnly>((IReadOnlyBuffer<ReadOnlyVector>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(bool))
				return new CastConverter<ReadOnlyMatrix, bool>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(sbyte))
				return new CastConverter<ReadOnlyMatrix, sbyte>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(float))
				return new CastConverter<ReadOnlyMatrix, float>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(double))
				return new CastConverter<ReadOnlyMatrix, double>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(decimal))
				return new CastConverter<ReadOnlyMatrix, decimal>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(string))
				return new CastConverter<ReadOnlyMatrix, string>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(short))
				return new CastConverter<ReadOnlyMatrix, short>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(int))
				return new CastConverter<ReadOnlyMatrix, int>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(long))
				return new CastConverter<ReadOnlyMatrix, long>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(IndexList))
				return new CastConverter<ReadOnlyMatrix, IndexList>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(WeightedIndexList))
				return new CastConverter<ReadOnlyMatrix, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<ReadOnlyMatrix, ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<ReadOnlyMatrix, ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<ReadOnlyMatrix, ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<ReadOnlyMatrix, ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(TimeOnly))
				return new CastConverter<ReadOnlyMatrix, TimeOnly>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(DateOnly))
				return new CastConverter<ReadOnlyMatrix, DateOnly>((IReadOnlyBuffer<ReadOnlyMatrix>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(bool))
				return new CastConverter<ReadOnlyTensor3D, bool>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(sbyte))
				return new CastConverter<ReadOnlyTensor3D, sbyte>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(float))
				return new CastConverter<ReadOnlyTensor3D, float>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(double))
				return new CastConverter<ReadOnlyTensor3D, double>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(decimal))
				return new CastConverter<ReadOnlyTensor3D, decimal>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(string))
				return new CastConverter<ReadOnlyTensor3D, string>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(short))
				return new CastConverter<ReadOnlyTensor3D, short>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(int))
				return new CastConverter<ReadOnlyTensor3D, int>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(long))
				return new CastConverter<ReadOnlyTensor3D, long>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(IndexList))
				return new CastConverter<ReadOnlyTensor3D, IndexList>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(WeightedIndexList))
				return new CastConverter<ReadOnlyTensor3D, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<ReadOnlyTensor3D, ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<ReadOnlyTensor3D, ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<ReadOnlyTensor3D, ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<ReadOnlyTensor3D, ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(TimeOnly))
				return new CastConverter<ReadOnlyTensor3D, TimeOnly>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(DateOnly))
				return new CastConverter<ReadOnlyTensor3D, DateOnly>((IReadOnlyBuffer<ReadOnlyTensor3D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(bool))
				return new CastConverter<ReadOnlyTensor4D, bool>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(sbyte))
				return new CastConverter<ReadOnlyTensor4D, sbyte>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(float))
				return new CastConverter<ReadOnlyTensor4D, float>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(double))
				return new CastConverter<ReadOnlyTensor4D, double>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(decimal))
				return new CastConverter<ReadOnlyTensor4D, decimal>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(string))
				return new CastConverter<ReadOnlyTensor4D, string>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(short))
				return new CastConverter<ReadOnlyTensor4D, short>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(int))
				return new CastConverter<ReadOnlyTensor4D, int>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(long))
				return new CastConverter<ReadOnlyTensor4D, long>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(IndexList))
				return new CastConverter<ReadOnlyTensor4D, IndexList>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(WeightedIndexList))
				return new CastConverter<ReadOnlyTensor4D, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<ReadOnlyTensor4D, ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<ReadOnlyTensor4D, ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<ReadOnlyTensor4D, ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<ReadOnlyTensor4D, ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(TimeOnly))
				return new CastConverter<ReadOnlyTensor4D, TimeOnly>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(DateOnly))
				return new CastConverter<ReadOnlyTensor4D, DateOnly>((IReadOnlyBuffer<ReadOnlyTensor4D>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(bool))
				return new CastConverter<TimeOnly, bool>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(sbyte))
				return new CastConverter<TimeOnly, sbyte>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(float))
				return new CastConverter<TimeOnly, float>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(double))
				return new CastConverter<TimeOnly, double>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(decimal))
				return new CastConverter<TimeOnly, decimal>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(string))
				return new CastConverter<TimeOnly, string>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(short))
				return new CastConverter<TimeOnly, short>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(int))
				return new CastConverter<TimeOnly, int>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(long))
				return new CastConverter<TimeOnly, long>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(IndexList))
				return new CastConverter<TimeOnly, IndexList>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(WeightedIndexList))
				return new CastConverter<TimeOnly, WeightedIndexList>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<TimeOnly, ReadOnlyVector>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<TimeOnly, ReadOnlyMatrix>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<TimeOnly, ReadOnlyTensor3D>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<TimeOnly, ReadOnlyTensor4D>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(TimeOnly))
				return new CastConverter<TimeOnly, TimeOnly>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(TimeOnly) && type2 == typeof(DateOnly))
				return new CastConverter<TimeOnly, DateOnly>((IReadOnlyBuffer<TimeOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(bool))
				return new CastConverter<DateOnly, bool>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(sbyte))
				return new CastConverter<DateOnly, sbyte>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(float))
				return new CastConverter<DateOnly, float>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(double))
				return new CastConverter<DateOnly, double>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(decimal))
				return new CastConverter<DateOnly, decimal>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(string))
				return new CastConverter<DateOnly, string>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(short))
				return new CastConverter<DateOnly, short>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(int))
				return new CastConverter<DateOnly, int>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(long))
				return new CastConverter<DateOnly, long>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(IndexList))
				return new CastConverter<DateOnly, IndexList>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(WeightedIndexList))
				return new CastConverter<DateOnly, WeightedIndexList>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyVector))
				return new CastConverter<DateOnly, ReadOnlyVector>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyMatrix))
				return new CastConverter<DateOnly, ReadOnlyMatrix>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyTensor3D))
				return new CastConverter<DateOnly, ReadOnlyTensor3D>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyTensor4D))
				return new CastConverter<DateOnly, ReadOnlyTensor4D>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(TimeOnly))
				return new CastConverter<DateOnly, TimeOnly>((IReadOnlyBuffer<DateOnly>)from);
			if(type1 == typeof(DateOnly) && type2 == typeof(DateOnly))
				return new CastConverter<DateOnly, DateOnly>((IReadOnlyBuffer<DateOnly>)from);
			throw new NotImplementedException($"Could not create CastConverter for types {type1} and {type2}");
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
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<bool, ReadOnlyVector>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, ReadOnlyVector>)converter);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<bool, ReadOnlyMatrix>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, ReadOnlyMatrix>)converter);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<bool, ReadOnlyTensor3D>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(bool) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<bool, ReadOnlyTensor4D>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(bool) && type2 == typeof(TimeOnly))
				return new TypeConverter<bool, TimeOnly>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, TimeOnly>)converter);
			if(type1 == typeof(bool) && type2 == typeof(DateOnly))
				return new TypeConverter<bool, DateOnly>((IReadOnlyBuffer<bool>)from, (ICanConvert<bool, DateOnly>)converter);
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
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<sbyte, ReadOnlyVector>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, ReadOnlyVector>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<sbyte, ReadOnlyMatrix>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, ReadOnlyMatrix>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<sbyte, ReadOnlyTensor3D>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<sbyte, ReadOnlyTensor4D>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(TimeOnly))
				return new TypeConverter<sbyte, TimeOnly>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, TimeOnly>)converter);
			if(type1 == typeof(sbyte) && type2 == typeof(DateOnly))
				return new TypeConverter<sbyte, DateOnly>((IReadOnlyBuffer<sbyte>)from, (ICanConvert<sbyte, DateOnly>)converter);
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
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<float, ReadOnlyVector>((IReadOnlyBuffer<float>)from, (ICanConvert<float, ReadOnlyVector>)converter);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<float, ReadOnlyMatrix>((IReadOnlyBuffer<float>)from, (ICanConvert<float, ReadOnlyMatrix>)converter);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<float, ReadOnlyTensor3D>((IReadOnlyBuffer<float>)from, (ICanConvert<float, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(float) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<float, ReadOnlyTensor4D>((IReadOnlyBuffer<float>)from, (ICanConvert<float, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(float) && type2 == typeof(TimeOnly))
				return new TypeConverter<float, TimeOnly>((IReadOnlyBuffer<float>)from, (ICanConvert<float, TimeOnly>)converter);
			if(type1 == typeof(float) && type2 == typeof(DateOnly))
				return new TypeConverter<float, DateOnly>((IReadOnlyBuffer<float>)from, (ICanConvert<float, DateOnly>)converter);
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
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<double, ReadOnlyVector>((IReadOnlyBuffer<double>)from, (ICanConvert<double, ReadOnlyVector>)converter);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<double, ReadOnlyMatrix>((IReadOnlyBuffer<double>)from, (ICanConvert<double, ReadOnlyMatrix>)converter);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<double, ReadOnlyTensor3D>((IReadOnlyBuffer<double>)from, (ICanConvert<double, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(double) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<double, ReadOnlyTensor4D>((IReadOnlyBuffer<double>)from, (ICanConvert<double, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(double) && type2 == typeof(TimeOnly))
				return new TypeConverter<double, TimeOnly>((IReadOnlyBuffer<double>)from, (ICanConvert<double, TimeOnly>)converter);
			if(type1 == typeof(double) && type2 == typeof(DateOnly))
				return new TypeConverter<double, DateOnly>((IReadOnlyBuffer<double>)from, (ICanConvert<double, DateOnly>)converter);
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
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<decimal, ReadOnlyVector>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, ReadOnlyVector>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<decimal, ReadOnlyMatrix>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, ReadOnlyMatrix>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<decimal, ReadOnlyTensor3D>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<decimal, ReadOnlyTensor4D>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(TimeOnly))
				return new TypeConverter<decimal, TimeOnly>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, TimeOnly>)converter);
			if(type1 == typeof(decimal) && type2 == typeof(DateOnly))
				return new TypeConverter<decimal, DateOnly>((IReadOnlyBuffer<decimal>)from, (ICanConvert<decimal, DateOnly>)converter);
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
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<string, ReadOnlyVector>((IReadOnlyBuffer<string>)from, (ICanConvert<string, ReadOnlyVector>)converter);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<string, ReadOnlyMatrix>((IReadOnlyBuffer<string>)from, (ICanConvert<string, ReadOnlyMatrix>)converter);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<string, ReadOnlyTensor3D>((IReadOnlyBuffer<string>)from, (ICanConvert<string, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(string) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<string, ReadOnlyTensor4D>((IReadOnlyBuffer<string>)from, (ICanConvert<string, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(string) && type2 == typeof(TimeOnly))
				return new TypeConverter<string, TimeOnly>((IReadOnlyBuffer<string>)from, (ICanConvert<string, TimeOnly>)converter);
			if(type1 == typeof(string) && type2 == typeof(DateOnly))
				return new TypeConverter<string, DateOnly>((IReadOnlyBuffer<string>)from, (ICanConvert<string, DateOnly>)converter);
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
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<short, ReadOnlyVector>((IReadOnlyBuffer<short>)from, (ICanConvert<short, ReadOnlyVector>)converter);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<short, ReadOnlyMatrix>((IReadOnlyBuffer<short>)from, (ICanConvert<short, ReadOnlyMatrix>)converter);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<short, ReadOnlyTensor3D>((IReadOnlyBuffer<short>)from, (ICanConvert<short, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(short) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<short, ReadOnlyTensor4D>((IReadOnlyBuffer<short>)from, (ICanConvert<short, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(short) && type2 == typeof(TimeOnly))
				return new TypeConverter<short, TimeOnly>((IReadOnlyBuffer<short>)from, (ICanConvert<short, TimeOnly>)converter);
			if(type1 == typeof(short) && type2 == typeof(DateOnly))
				return new TypeConverter<short, DateOnly>((IReadOnlyBuffer<short>)from, (ICanConvert<short, DateOnly>)converter);
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
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<int, ReadOnlyVector>((IReadOnlyBuffer<int>)from, (ICanConvert<int, ReadOnlyVector>)converter);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<int, ReadOnlyMatrix>((IReadOnlyBuffer<int>)from, (ICanConvert<int, ReadOnlyMatrix>)converter);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<int, ReadOnlyTensor3D>((IReadOnlyBuffer<int>)from, (ICanConvert<int, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(int) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<int, ReadOnlyTensor4D>((IReadOnlyBuffer<int>)from, (ICanConvert<int, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(int) && type2 == typeof(TimeOnly))
				return new TypeConverter<int, TimeOnly>((IReadOnlyBuffer<int>)from, (ICanConvert<int, TimeOnly>)converter);
			if(type1 == typeof(int) && type2 == typeof(DateOnly))
				return new TypeConverter<int, DateOnly>((IReadOnlyBuffer<int>)from, (ICanConvert<int, DateOnly>)converter);
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
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<long, ReadOnlyVector>((IReadOnlyBuffer<long>)from, (ICanConvert<long, ReadOnlyVector>)converter);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<long, ReadOnlyMatrix>((IReadOnlyBuffer<long>)from, (ICanConvert<long, ReadOnlyMatrix>)converter);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<long, ReadOnlyTensor3D>((IReadOnlyBuffer<long>)from, (ICanConvert<long, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(long) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<long, ReadOnlyTensor4D>((IReadOnlyBuffer<long>)from, (ICanConvert<long, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(long) && type2 == typeof(TimeOnly))
				return new TypeConverter<long, TimeOnly>((IReadOnlyBuffer<long>)from, (ICanConvert<long, TimeOnly>)converter);
			if(type1 == typeof(long) && type2 == typeof(DateOnly))
				return new TypeConverter<long, DateOnly>((IReadOnlyBuffer<long>)from, (ICanConvert<long, DateOnly>)converter);
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
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<IndexList, ReadOnlyVector>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, ReadOnlyVector>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<IndexList, ReadOnlyMatrix>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, ReadOnlyMatrix>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<IndexList, ReadOnlyTensor3D>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<IndexList, ReadOnlyTensor4D>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(TimeOnly))
				return new TypeConverter<IndexList, TimeOnly>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, TimeOnly>)converter);
			if(type1 == typeof(IndexList) && type2 == typeof(DateOnly))
				return new TypeConverter<IndexList, DateOnly>((IReadOnlyBuffer<IndexList>)from, (ICanConvert<IndexList, DateOnly>)converter);
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
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<WeightedIndexList, ReadOnlyVector>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, ReadOnlyVector>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<WeightedIndexList, ReadOnlyMatrix>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, ReadOnlyMatrix>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<WeightedIndexList, ReadOnlyTensor3D>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<WeightedIndexList, ReadOnlyTensor4D>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(TimeOnly))
				return new TypeConverter<WeightedIndexList, TimeOnly>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, TimeOnly>)converter);
			if(type1 == typeof(WeightedIndexList) && type2 == typeof(DateOnly))
				return new TypeConverter<WeightedIndexList, DateOnly>((IReadOnlyBuffer<WeightedIndexList>)from, (ICanConvert<WeightedIndexList, DateOnly>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(bool))
				return new TypeConverter<ReadOnlyVector, bool>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, bool>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(sbyte))
				return new TypeConverter<ReadOnlyVector, sbyte>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, sbyte>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(float))
				return new TypeConverter<ReadOnlyVector, float>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, float>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(double))
				return new TypeConverter<ReadOnlyVector, double>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, double>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(decimal))
				return new TypeConverter<ReadOnlyVector, decimal>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, decimal>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(string))
				return new TypeConverter<ReadOnlyVector, string>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, string>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(short))
				return new TypeConverter<ReadOnlyVector, short>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, short>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(int))
				return new TypeConverter<ReadOnlyVector, int>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, int>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(long))
				return new TypeConverter<ReadOnlyVector, long>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, long>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(IndexList))
				return new TypeConverter<ReadOnlyVector, IndexList>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, IndexList>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<ReadOnlyVector, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, WeightedIndexList>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<ReadOnlyVector, ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, ReadOnlyVector>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<ReadOnlyVector, ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, ReadOnlyMatrix>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<ReadOnlyVector, ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<ReadOnlyVector, ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(TimeOnly))
				return new TypeConverter<ReadOnlyVector, TimeOnly>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, TimeOnly>)converter);
			if(type1 == typeof(ReadOnlyVector) && type2 == typeof(DateOnly))
				return new TypeConverter<ReadOnlyVector, DateOnly>((IReadOnlyBuffer<ReadOnlyVector>)from, (ICanConvert<ReadOnlyVector, DateOnly>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(bool))
				return new TypeConverter<ReadOnlyMatrix, bool>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, bool>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(sbyte))
				return new TypeConverter<ReadOnlyMatrix, sbyte>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, sbyte>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(float))
				return new TypeConverter<ReadOnlyMatrix, float>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, float>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(double))
				return new TypeConverter<ReadOnlyMatrix, double>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, double>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(decimal))
				return new TypeConverter<ReadOnlyMatrix, decimal>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, decimal>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(string))
				return new TypeConverter<ReadOnlyMatrix, string>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, string>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(short))
				return new TypeConverter<ReadOnlyMatrix, short>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, short>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(int))
				return new TypeConverter<ReadOnlyMatrix, int>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, int>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(long))
				return new TypeConverter<ReadOnlyMatrix, long>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, long>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(IndexList))
				return new TypeConverter<ReadOnlyMatrix, IndexList>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, IndexList>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<ReadOnlyMatrix, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, WeightedIndexList>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<ReadOnlyMatrix, ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, ReadOnlyVector>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<ReadOnlyMatrix, ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, ReadOnlyMatrix>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<ReadOnlyMatrix, ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<ReadOnlyMatrix, ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(TimeOnly))
				return new TypeConverter<ReadOnlyMatrix, TimeOnly>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, TimeOnly>)converter);
			if(type1 == typeof(ReadOnlyMatrix) && type2 == typeof(DateOnly))
				return new TypeConverter<ReadOnlyMatrix, DateOnly>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (ICanConvert<ReadOnlyMatrix, DateOnly>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(bool))
				return new TypeConverter<ReadOnlyTensor3D, bool>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, bool>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(sbyte))
				return new TypeConverter<ReadOnlyTensor3D, sbyte>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, sbyte>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(float))
				return new TypeConverter<ReadOnlyTensor3D, float>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, float>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(double))
				return new TypeConverter<ReadOnlyTensor3D, double>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, double>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(decimal))
				return new TypeConverter<ReadOnlyTensor3D, decimal>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, decimal>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(string))
				return new TypeConverter<ReadOnlyTensor3D, string>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, string>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(short))
				return new TypeConverter<ReadOnlyTensor3D, short>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, short>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(int))
				return new TypeConverter<ReadOnlyTensor3D, int>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, int>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(long))
				return new TypeConverter<ReadOnlyTensor3D, long>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, long>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(IndexList))
				return new TypeConverter<ReadOnlyTensor3D, IndexList>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, IndexList>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<ReadOnlyTensor3D, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, WeightedIndexList>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<ReadOnlyTensor3D, ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, ReadOnlyVector>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<ReadOnlyTensor3D, ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, ReadOnlyMatrix>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<ReadOnlyTensor3D, ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<ReadOnlyTensor3D, ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(TimeOnly))
				return new TypeConverter<ReadOnlyTensor3D, TimeOnly>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, TimeOnly>)converter);
			if(type1 == typeof(ReadOnlyTensor3D) && type2 == typeof(DateOnly))
				return new TypeConverter<ReadOnlyTensor3D, DateOnly>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (ICanConvert<ReadOnlyTensor3D, DateOnly>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(bool))
				return new TypeConverter<ReadOnlyTensor4D, bool>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, bool>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(sbyte))
				return new TypeConverter<ReadOnlyTensor4D, sbyte>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, sbyte>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(float))
				return new TypeConverter<ReadOnlyTensor4D, float>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, float>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(double))
				return new TypeConverter<ReadOnlyTensor4D, double>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, double>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(decimal))
				return new TypeConverter<ReadOnlyTensor4D, decimal>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, decimal>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(string))
				return new TypeConverter<ReadOnlyTensor4D, string>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, string>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(short))
				return new TypeConverter<ReadOnlyTensor4D, short>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, short>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(int))
				return new TypeConverter<ReadOnlyTensor4D, int>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, int>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(long))
				return new TypeConverter<ReadOnlyTensor4D, long>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, long>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(IndexList))
				return new TypeConverter<ReadOnlyTensor4D, IndexList>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, IndexList>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(WeightedIndexList))
				return new TypeConverter<ReadOnlyTensor4D, WeightedIndexList>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, WeightedIndexList>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<ReadOnlyTensor4D, ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, ReadOnlyVector>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<ReadOnlyTensor4D, ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, ReadOnlyMatrix>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<ReadOnlyTensor4D, ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<ReadOnlyTensor4D, ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(TimeOnly))
				return new TypeConverter<ReadOnlyTensor4D, TimeOnly>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, TimeOnly>)converter);
			if(type1 == typeof(ReadOnlyTensor4D) && type2 == typeof(DateOnly))
				return new TypeConverter<ReadOnlyTensor4D, DateOnly>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (ICanConvert<ReadOnlyTensor4D, DateOnly>)converter);
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
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<TimeOnly, ReadOnlyVector>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, ReadOnlyVector>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<TimeOnly, ReadOnlyMatrix>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, ReadOnlyMatrix>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<TimeOnly, ReadOnlyTensor3D>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<TimeOnly, ReadOnlyTensor4D>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(TimeOnly))
				return new TypeConverter<TimeOnly, TimeOnly>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, TimeOnly>)converter);
			if(type1 == typeof(TimeOnly) && type2 == typeof(DateOnly))
				return new TypeConverter<TimeOnly, DateOnly>((IReadOnlyBuffer<TimeOnly>)from, (ICanConvert<TimeOnly, DateOnly>)converter);
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
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyVector))
				return new TypeConverter<DateOnly, ReadOnlyVector>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, ReadOnlyVector>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyMatrix))
				return new TypeConverter<DateOnly, ReadOnlyMatrix>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, ReadOnlyMatrix>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyTensor3D))
				return new TypeConverter<DateOnly, ReadOnlyTensor3D>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, ReadOnlyTensor3D>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(ReadOnlyTensor4D))
				return new TypeConverter<DateOnly, ReadOnlyTensor4D>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, ReadOnlyTensor4D>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(TimeOnly))
				return new TypeConverter<DateOnly, TimeOnly>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, TimeOnly>)converter);
			if(type1 == typeof(DateOnly) && type2 == typeof(DateOnly))
				return new TypeConverter<DateOnly, DateOnly>((IReadOnlyBuffer<DateOnly>)from, (ICanConvert<DateOnly, DateOnly>)converter);
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
			if(type == typeof(ReadOnlyVector))
				return new IndexedCopyOperation<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)from, (IAppendToBuffer<ReadOnlyVector>)to, indices);
			if(type == typeof(ReadOnlyMatrix))
				return new IndexedCopyOperation<ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (IAppendToBuffer<ReadOnlyMatrix>)to, indices);
			if(type == typeof(ReadOnlyTensor3D))
				return new IndexedCopyOperation<ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (IAppendToBuffer<ReadOnlyTensor3D>)to, indices);
			if(type == typeof(ReadOnlyTensor4D))
				return new IndexedCopyOperation<ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (IAppendToBuffer<ReadOnlyTensor4D>)to, indices);
			if(type == typeof(TimeOnly))
				return new IndexedCopyOperation<TimeOnly>((IReadOnlyBuffer<TimeOnly>)from, (IAppendToBuffer<TimeOnly>)to, indices);
			if(type == typeof(DateOnly))
				return new IndexedCopyOperation<DateOnly>((IReadOnlyBuffer<DateOnly>)from, (IAppendToBuffer<DateOnly>)to, indices);
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
			if(type == typeof(ReadOnlyVector))
				return new BufferCopyOperation<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)from, (IAppendBlocks<ReadOnlyVector>)to, onComplete);
			if(type == typeof(ReadOnlyMatrix))
				return new BufferCopyOperation<ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)from, (IAppendBlocks<ReadOnlyMatrix>)to, onComplete);
			if(type == typeof(ReadOnlyTensor3D))
				return new BufferCopyOperation<ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)from, (IAppendBlocks<ReadOnlyTensor3D>)to, onComplete);
			if(type == typeof(ReadOnlyTensor4D))
				return new BufferCopyOperation<ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)from, (IAppendBlocks<ReadOnlyTensor4D>)to, onComplete);
			if(type == typeof(TimeOnly))
				return new BufferCopyOperation<TimeOnly>((IReadOnlyBuffer<TimeOnly>)from, (IAppendBlocks<TimeOnly>)to, onComplete);
			if(type == typeof(DateOnly))
				return new BufferCopyOperation<DateOnly>((IReadOnlyBuffer<DateOnly>)from, (IAppendBlocks<DateOnly>)to, onComplete);
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
			if(type == typeof(ReadOnlyVector))
				return new SimpleNumericAnalysis<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)buffer);
			if(type == typeof(ReadOnlyMatrix))
				return new SimpleNumericAnalysis<ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)buffer);
			if(type == typeof(ReadOnlyTensor3D))
				return new SimpleNumericAnalysis<ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)buffer);
			if(type == typeof(ReadOnlyTensor4D))
				return new SimpleNumericAnalysis<ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)buffer);
			if(type == typeof(TimeOnly))
				return new SimpleNumericAnalysis<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer);
			if(type == typeof(DateOnly))
				return new SimpleNumericAnalysis<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer);
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
			if(type == typeof(ReadOnlyVector))
				return new ColumnFilter<ReadOnlyVector>(columnIndex, columnType, (IDataTypeSpecification<ReadOnlyVector>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(ReadOnlyMatrix))
				return new ColumnFilter<ReadOnlyMatrix>(columnIndex, columnType, (IDataTypeSpecification<ReadOnlyMatrix>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(ReadOnlyTensor3D))
				return new ColumnFilter<ReadOnlyTensor3D>(columnIndex, columnType, (IDataTypeSpecification<ReadOnlyTensor3D>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(ReadOnlyTensor4D))
				return new ColumnFilter<ReadOnlyTensor4D>(columnIndex, columnType, (IDataTypeSpecification<ReadOnlyTensor4D>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(TimeOnly))
				return new ColumnFilter<TimeOnly>(columnIndex, columnType, (IDataTypeSpecification<TimeOnly>)typeSpecification, nonConformingRowIndices);
			if(type == typeof(DateOnly))
				return new ColumnFilter<DateOnly>(columnIndex, columnType, (IDataTypeSpecification<DateOnly>)typeSpecification, nonConformingRowIndices);
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
			if(type == typeof(ReadOnlyVector))
				return new TypedIndexer<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)buffer);
			if(type == typeof(ReadOnlyMatrix))
				return new TypedIndexer<ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)buffer);
			if(type == typeof(ReadOnlyTensor3D))
				return new TypedIndexer<ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)buffer);
			if(type == typeof(ReadOnlyTensor4D))
				return new TypedIndexer<ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)buffer);
			if(type == typeof(TimeOnly))
				return new TypedIndexer<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer);
			if(type == typeof(DateOnly))
				return new TypedIndexer<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer);
			throw new NotImplementedException($"Could not create TypedIndexer for type {type}");
        }

        internal static IReadOnlyBuffer<ReadOnlyVector> OneHotConverter(IReadOnlyBuffer buffer, ICanIndex indexer)
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
			if(type == typeof(ReadOnlyVector))
				return new OneHotConverter<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)buffer, (ICanIndex<ReadOnlyVector>)indexer);
			if(type == typeof(ReadOnlyMatrix))
				return new OneHotConverter<ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)buffer, (ICanIndex<ReadOnlyMatrix>)indexer);
			if(type == typeof(ReadOnlyTensor3D))
				return new OneHotConverter<ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)buffer, (ICanIndex<ReadOnlyTensor3D>)indexer);
			if(type == typeof(ReadOnlyTensor4D))
				return new OneHotConverter<ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)buffer, (ICanIndex<ReadOnlyTensor4D>)indexer);
			if(type == typeof(TimeOnly))
				return new OneHotConverter<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer, (ICanIndex<TimeOnly>)indexer);
			if(type == typeof(DateOnly))
				return new OneHotConverter<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer, (ICanIndex<DateOnly>)indexer);
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
			if(type == typeof(ReadOnlyVector))
				return new CategoricalIndexConverter<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)buffer, (ICanIndex<ReadOnlyVector>)indexer);
			if(type == typeof(ReadOnlyMatrix))
				return new CategoricalIndexConverter<ReadOnlyMatrix>((IReadOnlyBuffer<ReadOnlyMatrix>)buffer, (ICanIndex<ReadOnlyMatrix>)indexer);
			if(type == typeof(ReadOnlyTensor3D))
				return new CategoricalIndexConverter<ReadOnlyTensor3D>((IReadOnlyBuffer<ReadOnlyTensor3D>)buffer, (ICanIndex<ReadOnlyTensor3D>)indexer);
			if(type == typeof(ReadOnlyTensor4D))
				return new CategoricalIndexConverter<ReadOnlyTensor4D>((IReadOnlyBuffer<ReadOnlyTensor4D>)buffer, (ICanIndex<ReadOnlyTensor4D>)indexer);
			if(type == typeof(TimeOnly))
				return new CategoricalIndexConverter<TimeOnly>((IReadOnlyBuffer<TimeOnly>)buffer, (ICanIndex<TimeOnly>)indexer);
			if(type == typeof(DateOnly))
				return new CategoricalIndexConverter<DateOnly>((IReadOnlyBuffer<DateOnly>)buffer, (ICanIndex<DateOnly>)indexer);
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
			if(type == typeof(ReadOnlyVector))
				return new CategoricalIndexVectoriser<ReadOnlyVector>();
			if(type == typeof(ReadOnlyMatrix))
				return new CategoricalIndexVectoriser<ReadOnlyMatrix>();
			if(type == typeof(ReadOnlyTensor3D))
				return new CategoricalIndexVectoriser<ReadOnlyTensor3D>();
			if(type == typeof(ReadOnlyTensor4D))
				return new CategoricalIndexVectoriser<ReadOnlyTensor4D>();
			if(type == typeof(TimeOnly))
				return new CategoricalIndexVectoriser<TimeOnly>();
			if(type == typeof(DateOnly))
				return new CategoricalIndexVectoriser<DateOnly>();
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
			if(type == typeof(ReadOnlyVector))
				return new OneHotVectoriser<ReadOnlyVector>(maxSize);
			if(type == typeof(ReadOnlyMatrix))
				return new OneHotVectoriser<ReadOnlyMatrix>(maxSize);
			if(type == typeof(ReadOnlyTensor3D))
				return new OneHotVectoriser<ReadOnlyTensor3D>(maxSize);
			if(type == typeof(ReadOnlyTensor4D))
				return new OneHotVectoriser<ReadOnlyTensor4D>(maxSize);
			if(type == typeof(TimeOnly))
				return new OneHotVectoriser<TimeOnly>(maxSize);
			if(type == typeof(DateOnly))
				return new OneHotVectoriser<DateOnly>(maxSize);
			throw new NotImplementedException($"Could not create OneHotVectoriser for type {type}");
        }
    }
}

