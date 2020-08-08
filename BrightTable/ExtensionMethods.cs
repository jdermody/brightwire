using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Analysis;
using BrightData.Buffers;
using BrightData.Converters;
using BrightData.Helper;
using BrightTable.Buffers;
using BrightTable.Builders;
using BrightTable.Helper;
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

        public static bool IsStructable(this ColumnType columnType) => ColumnTypeClassifier.IsStructable(columnType);
        public static bool IsNumeric(this ColumnType columnType) => ColumnTypeClassifier.IsNumeric(columnType);
        public static bool IsDecimal(this ColumnType columnType) => ColumnTypeClassifier.IsDecimal(columnType);
        public static bool IsContinuous(this ColumnType columnType) => ColumnTypeClassifier.IsContinuous(columnType);
        public static bool IsCategorical(this ColumnType columnType) => ColumnTypeClassifier.IsCategorical(columnType);

        public static bool IsInteger(this ColumnType type) => type switch
        {
            ColumnType.Byte => true,
            ColumnType.Short => true,
            ColumnType.Int => true,
            ColumnType.Long => true,
            _ => false
        };

        public static bool IsDecimal(this ColumnType type) => type switch
        {
            ColumnType.Double => true,
            ColumnType.Decimal => true,
            ColumnType.Float => true,
            _ => false
        };

        public static bool IsIndexed(this ColumnType type) => type switch
        {
            ColumnType.IndexList => true,
            ColumnType.WeightedIndexList => true,
            _ => false
        };

        public static bool IsTensor(this ColumnType type) => type switch
        {
            ColumnType.Vector => true,
            ColumnType.Matrix => true,
            ColumnType.Tensor3D => true,
            ColumnType.Tensor4D => true,
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

        public static IColumnOrientedDataTable ParseCsv(
            this IBrightDataContext context,
            string filePath,
            bool hasHeader,
            char delimiter = ',',
            string fileOutputPath = null,
            bool writeProgress = false,
            string tempBasePath = null
            //uint maxRowsInMemory = 32768 * 32
        )
        {
            using var tempStreams = new TempStreamManager(tempBasePath);
            var columns = new List<GrowableSegment<string>>();
            var isFirst = hasHeader;
            uint rowCount = 0;

            using (var reader = new StreamReader(filePath)) {
                var parser = new CsvParser(reader, delimiter, hasHeader);

                if (writeProgress) {
                    var progress = -1;
                    parser.OnProgress = p => p.WriteProgress(ref progress);
                    Console.WriteLine($"Parsing {filePath}...");
                }

                foreach (var row in parser.Parse()) {
                    var cols = row.Length;

                    for (var i = columns.Count; i < cols; i++) {
                        var buffer = new EncodingBuffer<string>(new HybridStringBuffer(context, (uint)i, tempStreams));
                        columns.Add(new GrowableSegment<string>(ColumnType.String, new MetaData(), buffer));
                    }

                    for (var i = 0; i < cols; i++) {
                        var column = columns[i];
                        var text = row[i];
                        if (isFirst)
                            column.MetaData.Set(Consts.Name, text);
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

            return columns.BuildColumnOrientedTable(context, rowCount, fileOutputPath);
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

        public static uint CopyToFloatSegment<T>(this IDataTableSegment<T> column, ITensorSegment<float> vector)
            where T : struct
        {
            uint index = 0;
            var converter = column.Context.GetFloatConverter<T>();

            foreach (var item in column.EnumerateTyped().Take((int)vector.Size))
                vector[index++] = converter.Convert(item);
            return index;
        }

        public static uint CopyTo(this ISingleTypeTableSegment column, ITensorSegment<float> vector)
        {
            var type = column.SingleType.GetColumnType();
            var copySegment = typeof(ExtensionMethods).GetMethod("CopyToFloatSegment").MakeGenericMethod(type);
            return (uint)copySegment.Invoke(null, new object[] { column, vector });
        }

        public static void SetTargetColumn(this IDataTable table, uint? columnIndex)
        {
            var metaData = table.AllMetaData();
            for (uint i = 0; i < table.ColumnCount; i++) {
                metaData[(int)i].Set(Consts.IsTarget, i == columnIndex);
            }
        }

        public static uint? GetTargetColumn(this IDataTable table)
        {
            var metaData = table.AllMetaData();
            for (uint i = 0; i < table.ColumnCount; i++) {
                if (metaData[(int)i].Get<bool>(Consts.IsTarget))
                    return i;
            }
            return null;
        }

        public static void SetFeatureColumn(this IDataTable table, params uint[] columnIndices)
        {
            var metaData = table.AllMetaData();
            var featureColumns = new HashSet<uint>(columnIndices);

            for (uint i = 0; i < table.ColumnCount; i++) {
                metaData[(int)i].Set(Consts.IsFeature, featureColumns.Contains(i));
            }
        }

        public static void SetSequentialColumn(this IDataTable table, params uint[] columnIndices)
        {
            var metaData = table.AllMetaData();
            var featureColumns = new HashSet<uint>(columnIndices);

            for (uint i = 0; i < table.ColumnCount; i++) {
                metaData[(int)i].Set(Consts.IsSequential, featureColumns.Contains(i));
            }
        }

        public static IAutoGrowBuffer GetGrowableSegment(this IColumnInfo forColumn, IBrightDataContext context, TempStreamManager tempStream, bool tryEncode, uint bufferSize = 32768)
        {
            var type = forColumn.ColumnType;
            var columnType = type.GetColumnType();

            IAutoGrowBuffer buffer;
            if (type.IsStructable()) {
                buffer = (IAutoGrowBuffer)Activator.CreateInstance(typeof(HybridStructBuffer<>).MakeGenericType(type.GetColumnType()),
                    context,
                    forColumn.Index,
                    tempStream,
                    bufferSize
                );
            } else if (type == ColumnType.String) {
                buffer = (IAutoGrowBuffer)Activator.CreateInstance(typeof(HybridStringBuffer),
                    context,
                    forColumn.Index,
                    tempStream,
                    bufferSize
                );
            } else {
                buffer = (IAutoGrowBuffer)Activator.CreateInstance(typeof(HybridObjectBuffer<>).MakeGenericType(type.GetColumnType()),
                    context,
                    forColumn.Index,
                    tempStream,
                    bufferSize
                );
            }

            if (tryEncode) {
                var encoderType = typeof(EncodingBuffer<>).MakeGenericType(columnType);
                buffer = (IAutoGrowBuffer)Activator.CreateInstance(encoderType, new object[] { buffer, bufferSize });
            }

            var segmentType = typeof(GrowableSegment<>).MakeGenericType(columnType);
            var ret = Activator.CreateInstance(segmentType,
                type,
                new MetaData(forColumn.MetaData, Consts.StandardMetaData),
                buffer
            );

            return (IAutoGrowBuffer)ret;
        }

        public static IColumnOrientedDataTable BuildColumnOrientedTable(this IReadOnlyList<ISingleTypeTableSegment> segments, IBrightDataContext context, uint rowCount, string filePath = null)
        {
            var columnCount = (uint)segments.Count;
            var columnOffsets = new List<(long Position, long EndOfColumnOffset)>();
            using var builder = new ColumnOrientedTableBuilder(filePath);

            builder.WriteHeader(columnCount, rowCount);
            foreach (var segment in segments) {
                var position = builder.Write(segment);
                columnOffsets.Add((position, builder.GetCurrentPosition()));
            }
            builder.WriteColumnOffsets(columnOffsets);
            return builder.Build(context);
        }

        public static IColumnOrientedDataTable BuildColumnOrientedTable(this IReadOnlyList<IAutoGrowBuffer> buffers, IBrightDataContext context, uint rowCount, string filePath = null)
        {
            return buffers.Cast<ISingleTypeTableSegment>().ToList().BuildColumnOrientedTable(context, rowCount, filePath);
        }

        public static IColumnOrientedDataTable BuildColumnOrientedTable<T>(this IReadOnlyList<GrowableSegment<T>> buffers, IBrightDataContext context, uint rowCount, string filePath = null)
        {
            return buffers.Cast<ISingleTypeTableSegment>().ToList().BuildColumnOrientedTable(context, rowCount, filePath);
        }

        public static IRowOrientedDataTable BuildRowOrientedTable(this IReadOnlyList<IAutoGrowBuffer> buffers, IBrightDataContext context, uint rowCount, string filePath = null)
        {
            using var builder = new RowOrientedTableBuilder(rowCount, filePath);
            var readers = buffers.Cast<ISingleTypeTableSegment>()
                .Select(b => b.Enumerate().GetEnumerator())
                .ToList();
            while (readers.All(r => r.MoveNext())) {
                var row = readers.Select(r => r.Current).ToArray();
                builder.AddRow(row);
            }
            return builder.Build(context);
        }

        public static IColumnInfo ChangeColumnType(this IColumnInfo column, ColumnType newType)
        {
            if (column.ColumnType == newType)
                return column;
            return new ColumnInfo(column.Index, newType, new MetaData(column.MetaData, Consts.Index, Consts.Name));
        }

        public static IConvertibleTable AsConvertible(this IRowOrientedDataTable dataTable)
        {
            return new DataTableConverter(dataTable);
        }

        public static IEnumerable<(float[] Numeric, string Other)> ForEachAsFloat(
            this IDataTable dataTable, 
            Action<IReadOnlyList<uint>> numericRows = null,
            Action<IReadOnlyList<uint>> otherRows = null)
        {
            var numericColumnIndex = new List<uint>();
            var otherColumnIndex = new List<uint>();
            uint index = 0;

            foreach (var column in dataTable.ColumnTypes) {
                if (ColumnTypeClassifier.IsNumeric(column)) {
                    numericColumnIndex.Add(index);
                } else
                    otherColumnIndex.Add(index);
                ++index;
            }
            if (!numericColumnIndex.Any())
                throw new ArgumentException("No numeric columns");

            numericRows?.Invoke(numericColumnIndex.AsReadOnly());
            otherRows?.Invoke(otherColumnIndex.AsReadOnly());

            var rowCount = dataTable.RowCount;
            var numericColumns = dataTable
                .Columns(numericColumnIndex.ToArray())
                .Select(c => {
                    var vector = dataTable.Context.CreateVector<float>(rowCount);
                    c.CopyTo(vector.Data);
                    return vector;
                })
                .ToList()
            ;
            var otherColumns = new List<object[]>();
            if (otherColumnIndex.Any())
                otherColumns.AddRange(dataTable.Columns(otherColumnIndex.ToArray()).Select(c => c.Enumerate().ToArray()));

            var numericCount = numericColumns.Count;
            var otherCount = otherColumns.Count;
            var row = new float[numericCount];
            var sb = new StringBuilder();
            for (uint i = 0, len = dataTable.RowCount; i < len; i++) {
                for (var j = 0; j < numericCount; j++)
                    row[j] = numericColumns[j][i];
                for (var j = 0; j < otherCount; j++) {
                    if(j > 0)
                        sb.Append(", ");
                    sb.Append(otherColumns[j][i].ToString());
                }
                yield return (row, sb.ToString());
                sb.Clear();
            }
        }

        public static ColumnType GetColumnType(this IMetaData metadata) => metadata.Get<ColumnType>(Consts.Type);
        public static int GetNumDistinct(this IMetaData metadata) => metadata.Get<int>(Consts.NumDistinct, -1);
    }
}
