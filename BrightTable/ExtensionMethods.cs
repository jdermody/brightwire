using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;

namespace BrightTable
{
    static class ExtensionMethods
    {
        public static IEnumerable<uint> Range(uint start, uint count)
        {
            for(uint i = 0; i < count; i++)
                yield return start + i;
        }

        public static Type GetColumnType(this ColumnType type)
        {
            switch (type) {
                case ColumnType.Boolean:
                    return typeof(bool);

                case ColumnType.Byte:
                    return typeof(sbyte);

                case ColumnType.Date:
                    return typeof(DateTime);

                case ColumnType.Double:
                    return typeof(double);

                case ColumnType.Decimal:
                    return typeof(decimal);

                case ColumnType.Float:
                    return typeof(float);

                case ColumnType.Short:
                    return typeof(short);

                case ColumnType.Int:
                    return typeof(int);

                case ColumnType.Long:
                    return typeof(long);

                case ColumnType.Unknown:
                    return null;

                case ColumnType.String:
                    return typeof(string);

                case ColumnType.IndexList:
                    return typeof(IndexList);

                case ColumnType.WeightedIndexList:
                    return typeof(WeightedIndexList);

                case ColumnType.Vector:
                    return typeof(Vector<float>);

                case ColumnType.Matrix:
                    return typeof(Matrix<float>);

                case ColumnType.Tensor3D:
                    return typeof(Tensor3D<float>);

                case ColumnType.Tensor4D:
                    return typeof(Tensor4D<float>);

                case ColumnType.BinaryData:
                    return typeof(BinaryData);

                default:
                    throw new NotImplementedException();
            }
        }

        public static ColumnType GetColumnType(this Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode) {
                case TypeCode.Boolean:
                    return ColumnType.Boolean;

                case TypeCode.SByte:
                    return ColumnType.Byte;

                case TypeCode.DateTime:
                    return ColumnType.Date;

                case TypeCode.Double:
                    return ColumnType.Double;

                case TypeCode.Decimal:
                    return ColumnType.Decimal;

                case TypeCode.Single:
                    return ColumnType.Float;

                case TypeCode.Int16:
                    return ColumnType.Short;

                case TypeCode.Int32:
                    return ColumnType.Int;

                case TypeCode.Int64:
                    return ColumnType.Long;

                case TypeCode.String:
                    return ColumnType.String;
            }

            if(type == typeof(IndexList))
                return ColumnType.IndexList;

            if(type == typeof(WeightedIndexList))
                return ColumnType.WeightedIndexList;

            if(type == typeof(Vector<float>))
                return ColumnType.Vector;

            if(type == typeof(Matrix<float>))
                return ColumnType.Matrix;

            if(type == typeof(Tensor3D<float>))
                return ColumnType.Tensor3D;

            if(type == typeof(Tensor4D<float>))
                return ColumnType.Tensor4D;

            if(type == typeof(BinaryData))
                return ColumnType.BinaryData;

            return ColumnType.Unknown;
        }

        public static Type DataType(this IDataTableSegment segment)
        {
            Type ret = null;
            for (uint i = 0; i < segment.Size; i++) {
                var type = segment[i]?.GetType();
                if (ret == null)
                    ret = type;
                else if (type != null && type != ret)
                    return null;
            }
            return ret;
        }

        public static Type DataType<T>(this IDataTableSegment<T> segment) => typeof(T);

        public static IEnumerable<uint> RowIndices(this IDataTable dataTable)
        {
            return Enumerable.Range(0, (int) dataTable.RowCount).Select(i => (uint) i);
        }

        public static IEnumerable<uint> ColumnIndices(this IDataTable dataTable)
        {
            return Enumerable.Range(0, (int) dataTable.ColumnCount).Select(i => (uint) i);
        }

        public static IReadOnlyList<IMetaData> AllMetaData(this IDataTable dataTable)
        {
            return dataTable.ColumnMetaData(dataTable.ColumnIndices().ToArray());
        }

        public static void ForEachRow(this IDataTable dataTable, Action<object[]> callback)
        {
            dataTable.ForEachRow((row, index) => callback(row));
        }

        public static void ForEachRow<T0>(this IDataTable dataTable, Action<T0> callback) => dataTable.ForEachRow((row, index) => callback((T0)row[0]));
        public static void ForEachRow<T0, T1>(this IDataTable dataTable, Action<T0, T1> callback) => dataTable.ForEachRow((row, index) => callback((T0)row[0], (T1)row[1]));
        public static void ForEachRow<T0, T1, T2>(this IDataTable dataTable, Action<T0, T1, T2> callback) => dataTable.ForEachRow((row, index) => callback((T0)row[0], (T1)row[1], (T2)row[2]));
        public static void ForEachRow<T0, T1, T2, T3>(this IDataTable dataTable, Action<T0, T1, T2, T3> callback) => dataTable.ForEachRow((row, index) => callback((T0)row[0], (T1)row[1], (T2)row[2], (T3)row[3]));

        public static IReadOnlyList<T> MapRows<T>(this IDataTable dataTable, Func<object[], uint, T> callback)
        {
            var ret = new List<T>();
            dataTable.ForEachRow((row, index) => ret.Add(callback(row, index)));
            return ret.AsReadOnly();
        }

        public static IReadOnlyList<T> MapRows<T0, T>(this IDataTable dataTable, Func<T0, T> callback) => MapRows(dataTable, (rows, index) => callback((T0) rows[0]));
        public static IReadOnlyList<T> MapRows<T0, T1, T>(this IDataTable dataTable, Func<T0, T1, T> callback) => MapRows(dataTable, (rows, index) => callback((T0) rows[0], (T1) rows[1]));

        //public static IReadOnlyList<IDataTableSegment> Rows(this IRowOrientedDataTable dataTable, IEnumerable<uint> rowIndices) => dataTable.Rows(rowIndices.ToArray());
        //public static IReadOnlyList<IDataTableSegment> AllRows(this IRowOrientedDataTable dataTable) => dataTable.Rows(Range(0, dataTable.RowCount));
        ///public static IReadOnlyList<ISingleTypeTableSegment> Columns(this IDataTable dataTable, IEnumerable<uint> columnIndices) => dataTable.Columns(columnIndices.ToArray());
        //public static IReadOnlyList<ISingleTypeTableSegment> AllColumns(this IDataTable dataTable) => dataTable.Columns(Range(0, dataTable.ColumnCount));
    }
}
