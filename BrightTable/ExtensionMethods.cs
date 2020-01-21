using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Analysis;
using BrightData.Helper;
using BrightTable.Builders;
using BrightTable.Input;
using BrightTable.Segments;

namespace BrightTable
{
    public static class ExtensionMethods
    {
        public static IEnumerable<uint> Range(uint start, uint count)
        {
            for (uint i = 0; i < count; i++)
                yield return start + i;
        }

        public static Type GetColumnType(this ColumnType type)
        {
            return type switch
            {
                ColumnType.Boolean => typeof(bool),
                ColumnType.Byte => typeof(sbyte),
                ColumnType.Date => typeof(DateTime),
                ColumnType.Double => typeof(double),
                ColumnType.Decimal => typeof(decimal),
                ColumnType.Float => typeof(float),
                ColumnType.Short => typeof(short),
                ColumnType.Int => typeof(int),
                ColumnType.Long => typeof(long),
                ColumnType.Unknown => null,
                ColumnType.String => typeof(string),
                ColumnType.IndexList => typeof(IndexList),
                ColumnType.WeightedIndexList => typeof(WeightedIndexList),
                ColumnType.Vector => typeof(Vector<float>),
                ColumnType.Matrix => typeof(Matrix<float>),
                ColumnType.Tensor3D => typeof(Tensor3D<float>),
                ColumnType.Tensor4D => typeof(Tensor4D<float>),
                ColumnType.BinaryData => typeof(BinaryData),
                _ => throw new NotImplementedException()
            };
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

            if (type == typeof(IndexList))
                return ColumnType.IndexList;

            if (type == typeof(WeightedIndexList))
                return ColumnType.WeightedIndexList;

            if (type == typeof(Vector<float>))
                return ColumnType.Vector;

            if (type == typeof(Matrix<float>))
                return ColumnType.Matrix;

            if (type == typeof(Tensor3D<float>))
                return ColumnType.Tensor3D;

            if (type == typeof(Tensor4D<float>))
                return ColumnType.Tensor4D;

            if (type == typeof(BinaryData))
                return ColumnType.BinaryData;

            return ColumnType.Unknown;
        }

        public static bool IsStructable(this ColumnType type) => type switch
        {
            ColumnType.Boolean => true,
            ColumnType.Byte => true,
            ColumnType.Date => true,
            ColumnType.Double => true,
            ColumnType.Decimal => true,
            ColumnType.Float => true,
            ColumnType.Short => true,
            ColumnType.Int => true,
            ColumnType.Long => true,
            _ => false
        };

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
            return Enumerable.Range(0, (int)dataTable.RowCount).Select(i => (uint)i);
        }

        public static IEnumerable<uint> ColumnIndices(this IDataTable dataTable)
        {
            return Enumerable.Range(0, (int)dataTable.ColumnCount).Select(i => (uint)i);
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

        public static IReadOnlyList<T> MapRows<T0, T>(this IDataTable dataTable, Func<T0, T> callback) => MapRows(dataTable, (rows, index) => callback((T0)rows[0]));
        public static IReadOnlyList<T> MapRows<T0, T1, T>(this IDataTable dataTable, Func<T0, T1, T> callback) => MapRows(dataTable, (rows, index) => callback((T0)rows[0], (T1)rows[1]));

        public static IDataAnalyser GetColumnAnalyser(this ColumnType type, int distinctValueCount = 100)
        {
            switch (type) {
                case ColumnType.Double:
                    return new NumericAnalyser(distinctValueCount);
                case ColumnType.Float:
                    return new CastToDoubleNumericAnalysis<float>(distinctValueCount);
                case ColumnType.Decimal:
                    return new CastToDoubleNumericAnalysis<decimal>(distinctValueCount);
                case ColumnType.Byte:
                    return new CastToDoubleNumericAnalysis<sbyte>(distinctValueCount);
                case ColumnType.Int:
                    return new CastToDoubleNumericAnalysis<int>(distinctValueCount);
                case ColumnType.Long:
                    return new CastToDoubleNumericAnalysis<long>(distinctValueCount);
                case ColumnType.Short:
                    return new CastToDoubleNumericAnalysis<short>(distinctValueCount);
            }
            if (type == ColumnType.String)
                return new StringAnalyser(distinctValueCount);
            if (type == ColumnType.IndexList || type == ColumnType.WeightedIndexList)
                return new IndexAnalyser(distinctValueCount);
            if (type == ColumnType.Date)
                return new DateAnalyser();
            if (type == ColumnType.Vector || type == ColumnType.Matrix || type == ColumnType.Tensor3D || type == ColumnType.Tensor4D)
                return new DimensionAnalyser();
            if (type == ColumnType.BinaryData)
                return new FrequencyAnalyser<BinaryData>(distinctValueCount);

            throw new NotImplementedException();
        }

        public static IMetaData Analyse(this ISingleTypeTableSegment segment, bool force = false, int distinctValueCount = 100)
        {
            var ret = segment.MetaData;
            if (force || !ret.Get<bool>(Consts.HasBeenAnalysed)) {
                var type = segment.SingleType;
                var analyser = type.GetColumnAnalyser(distinctValueCount);
                foreach (var item in segment.Enumerate())
                    analyser.AddObject(item);
                analyser.WriteTo(ret);
                ret.Set(Consts.HasBeenAnalysed, true);
            }

            return ret;
        }

        public static IMetaData[] Analyse(this IColumnOrientedDataTable table, bool force = false, int distinctValueCount = 100)
        {
            var count = table.ColumnCount;
            var ret = new IMetaData[count];
            for (uint i = 0; i < count; i++) {
                var column = table.Column(i);
                ret[i] = column.Analyse(force, distinctValueCount);
            }
            return ret;
        }

        public static IReadOnlyList<ISingleTypeTableSegment> AllColumns(this IDataTable dataTable) => dataTable.Columns(Range(0, dataTable.ColumnCount).ToArray());
        public static IReadOnlyList<IDataTableSegment> AllRows(this IRowOrientedDataTable dataTable) => dataTable.Rows(Range(0, dataTable.RowCount).ToArray());

        //public static IColumnOrientedDataTable ParseCsvText(this IBrightDataContext context, string csv, bool hasHeader, char delimiter = ',', string filePath = null, string tempPath = null)
        //{
        //    var reader = new SimpleStringReader(csv);
        //    return _ParseCsv(context, reader, hasHeader, delimiter, filePath, tempPath);
        //}

        //public static IColumnOrientedDataTable ParseCsvFile(this IBrightDataContext context, string csvFilePath, bool hasHeader, char delimiter = ',', string filePath = null, string tempPath = null)
        //{
        //    using var reader = new FileReader(csvFilePath);
        //    return _ParseCsv(context, reader, hasHeader, delimiter, filePath, tempPath);
        //}

        //static IColumnOrientedDataTable _ParseCsv(IBrightDataContext context, IStringIterator reader, bool hasHeader, char delimiter = ',', string filePath = null, string tempPath = null)
        //{
        //    using var parser = new CsvParser(delimiter, 32768 * 32, tempPath);
        //    parser.Parse(reader, hasHeader);
        //    var builder = new ColumnOrientedTableBuilder(filePath);
        //    builder.Write(parser.LineCount, parser.Columns);
        //    return builder.Build(context);
        //}

        public static IColumnOrientedDataTable ParseCsv(
            this IBrightDataContext context,
            string filePath,
            bool hasHeader,
            char delimiter = ',',
            string fileOutputPath = null,
            bool writeProgress = false,
            string tempBasePath = null,
            uint maxRowsInMemory = 32768 * 32
        )
        {
            using var tempStreams = new TempStreamManager(tempBasePath);
            var columns = new List<StringColumn>();
            var isFirst = hasHeader;
            uint rowCount = 0;

            using (var reader = new StreamReader(filePath)) {
                var parser = new CsvParser2(reader, delimiter, hasHeader);

                if (writeProgress) {
                    var progress = -1;
                    parser.OnProgress = p => p.WriteProgress(ref progress);
                    Console.WriteLine($"Parsing {filePath}...");
                }

                foreach (var row in parser.Parse()) {
                    var cols = row.Length;

                    for (var i = columns.Count; i < cols; i++)
                        columns.Add(new StringColumn((uint)i, tempStreams, rowCount, maxRowsInMemory));

                    for (var i = 0; i < cols; i++) {
                        var column = columns[i];
                        var text = row[i];
                        if (isFirst)
                            column.Header = text;
                        else
                            column.Add(text);
                    }

                    if (isFirst)
                        isFirst = false;
                    else
                        ++rowCount;
                }
            }

            if (writeProgress) {
                Console.WriteLine();
                Console.WriteLine($"Read {rowCount:N0} lines into {columns.Count:N0} columns");
            }

            var builder = new ColumnOrientedTableBuilder(fileOutputPath);
            builder.Write(rowCount, columns, writeProgress);
            columns.ForEach(c => c.Dispose());
            return builder.Build(context);
        }

        public static void WriteProgress(this int newProgress, ref int oldProgress, int max = 100)
        {
            if (newProgress > oldProgress) {
                var sb = new StringBuilder();
                sb.Append('\r');
                for (var i = 0; i < max; i++)
                    sb.Append(i < newProgress ? '█' : '_');
                sb.Append($" ({oldProgress = newProgress}%)");
                Console.Write(sb.ToString());
            }
        }

        public static IReadOnlyList<object[]> Head(this IDataTable dataTable, uint size = 10)
        {
            var ret = new List<object[]>();
            dataTable.ForEachRow((row, index) => ret.Add(row), size);
            return ret;
        }

        public static IDataTable LoadTable(this IBrightDataContext context, string filePath)
        {
            var input = new InputData(filePath);
            var reader = input.Reader;
            var version = reader.ReadInt32();

            if (version > Consts.DataTableVersion)
                throw new Exception($"Data table version {version} exceeds {Consts.DataTableVersion}");
            var orientation = (DataTableOrientation)reader.ReadByte();
            if (orientation == DataTableOrientation.ColumnOriented)
                return new ColumnOrientedDataTable(context, input, false);
            else if (orientation == DataTableOrientation.RowOriented)
                return new RowOrientedDataTable(context, input, false);
            throw new Exception($"Found unknown data table orientation: {orientation}");
        }

        public static uint CopySegment<T>(this IDataTableSegment<T> column, ITensorSegment<float> vector)
            where T : struct
        {
            uint index = 0;
            var converter = new ConvertToFloat<T>();

            foreach (var item in column.EnumerateTyped().Take((int)vector.Size))
                vector[index++] = converter.Convert(item);
            return index;
        }

        public static uint CopyTo(this ISingleTypeTableSegment column, ITensorSegment<float> vector)
        {
            var type = column.SingleType.GetColumnType();
            var copySegment = typeof(ExtensionMethods).GetMethod("CopySegment").MakeGenericMethod(type);
            return (uint)copySegment.Invoke(null, new object[] { column, vector });
        }
    }
}
