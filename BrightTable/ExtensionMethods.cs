using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BrightData;
using BrightData.Analysis;
using BrightData.Buffers;
using BrightData.Converters;
using BrightData.Helper;
using BrightTable.Builders;
using BrightTable.Helper;
using BrightTable.Input;
using BrightTable.Segments;
using BrightTable.Transformations;

namespace BrightTable
{
    public static class ExtensionMethods
    {
        public static Type GetDataType(this ColumnType type)
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

        public static ColumnType GetColumnType(this Type dataType)
        {
            var typeCode = Type.GetTypeCode(dataType);
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

            if (dataType == typeof(IndexList))
                return ColumnType.IndexList;

            if (dataType == typeof(WeightedIndexList))
                return ColumnType.WeightedIndexList;

            if (dataType == typeof(Vector<float>))
                return ColumnType.Vector;

            if (dataType == typeof(Matrix<float>))
                return ColumnType.Matrix;

            if (dataType == typeof(Tensor3D<float>))
                return ColumnType.Tensor3D;

            if (dataType == typeof(Tensor4D<float>))
                return ColumnType.Tensor4D;

            if (dataType == typeof(BinaryData))
                return ColumnType.BinaryData;

            return ColumnType.Unknown;
        }

        public static bool IsStructable(this ColumnType columnType) => ColumnTypeClassifier.IsBlittable(columnType);
        public static bool IsNumeric(this ColumnType columnType) => ColumnTypeClassifier.IsNumeric(columnType);
        public static bool IsDecimal(this ColumnType columnType) => ColumnTypeClassifier.IsDecimal(columnType);
        public static bool IsContinuous(this ColumnType columnType) => ColumnTypeClassifier.IsContinuous(columnType);

        public static bool IsInteger(this ColumnType type) => type switch
        {
            ColumnType.Byte => true,
            ColumnType.Short => true,
            ColumnType.Int => true,
            ColumnType.Long => true,
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

        public static Type GetDataType<T>(this IDataTableSegment<T> segment) => typeof(T);

        public static IEnumerable<uint> RowIndices(this IDataTable dataTable)
        {
            return dataTable.RowCount.AsRange();
        }

        public static IEnumerable<uint> ColumnIndices(this IDataTable dataTable)
        {
            return dataTable.ColumnCount.AsRange();
        }

        public static IEnumerable<IMetaData> AllMetaData(this IDataTable dataTable)
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

        public static List<T> MapRows<T>(this IDataTable dataTable, Func<object[], uint, T> callback)
        {
            var ret = new List<T>();
            dataTable.ForEachRow((row, index) => ret.Add(callback(row, index)));
            return ret;
        }

        public static List<T> MapRows<T0, T>(this IDataTable dataTable, Func<T0, T> callback) => MapRows(dataTable, (rows, index) => callback((T0)rows[0]));
        public static List<T> MapRows<T0, T1, T>(this IDataTable dataTable, Func<T0, T1, T> callback) => MapRows(dataTable, (rows, index) => callback((T0)rows[0], (T1)rows[1]));

        public static IMetaData[] AllColumnsMetaData(this IDataTable dataTable) => dataTable.ColumnMetaData(dataTable.ColumnCount.AsRange().ToArray()).ToArray();

        interface IAnalyserBinding
        {
            void Analyse();
        }
        class AnalyserBinding<T> : IAnalyserBinding
        {
            readonly IDataAnalyser<T> _analyser;
            readonly IDataTableSegment<T> _segment;

            public AnalyserBinding(ISingleTypeTableSegment segment, IDataAnalyser analyser)
            {
                _analyser = (IDataAnalyser<T>)analyser;
                _segment = (IDataTableSegment<T>)segment;
            }

            public void Analyse()
            {
                foreach (var item in _segment.EnumerateTyped())
                    _analyser.AddObject(item);
            }
        }

        public static IDataAnalyser GetColumnAnalyser(this ColumnType type, IMetaData metaData, uint writeCount = 100, uint maxCount = Consts.MaxDistinct)
        {
            var dataType = ColumnTypeClassifier.GetClass(type, metaData);
            if ((dataType & BrightTable.ColumnClass.Categorical) != 0) {
                if (type == ColumnType.String)
                    return StaticAnalysers.CreateStringAnalyser(maxCount, writeCount);
                return StaticAnalysers.CreateFrequencyAnalyser(type.GetDataType(), maxCount, writeCount);
            }
            if ((dataType & BrightTable.ColumnClass.IndexBased) != 0)
                return StaticAnalysers.CreateIndexAnalyser(maxCount, writeCount);
            if ((dataType & BrightTable.ColumnClass.Tensor) != 0)
                return StaticAnalysers.CreateDimensionAnalyser(maxCount);

            switch (type) {
                case ColumnType.Double:
                    return StaticAnalysers.CreateNumericAnalyser(maxCount, writeCount);
                case ColumnType.Float:
                    return StaticAnalysers.CreateNumericAnalyser<float>(maxCount, writeCount);
                case ColumnType.Decimal:
                    return StaticAnalysers.CreateNumericAnalyser<decimal>(maxCount, writeCount);
                case ColumnType.Byte:
                    return StaticAnalysers.CreateNumericAnalyser<sbyte>(maxCount, writeCount);
                case ColumnType.Int:
                    return StaticAnalysers.CreateNumericAnalyser<int>(maxCount, writeCount);
                case ColumnType.Long:
                    return StaticAnalysers.CreateNumericAnalyser<long>(maxCount, writeCount);
                case ColumnType.Short:
                    return StaticAnalysers.CreateNumericAnalyser<short>(maxCount, writeCount);
            }
            if (type == ColumnType.Date)
                return StaticAnalysers.CreateDateAnalyser();
            if (type == ColumnType.BinaryData)
                return StaticAnalysers.CreateFrequencyAnalyser<BinaryData>(maxCount, writeCount);

            throw new NotImplementedException();
        }

        public static IMetaData Analyse(this ISingleTypeTableSegment segment, bool force = false, uint distinctValueCount = 100)
        {
            var ret = segment.MetaData;
            if (force || !ret.Get(Consts.HasBeenAnalysed, false)) {
                var type = segment.SingleType;
                var analyser = type.GetColumnAnalyser(segment.MetaData, distinctValueCount);
                var binding = GenericActivator.Create<IAnalyserBinding>(typeof(AnalyserBinding<>).MakeGenericType(type.GetDataType()),
                    segment,
                    analyser
                );
                binding.Analyse();
                analyser.WriteTo(ret);
                ret.Set(Consts.HasBeenAnalysed, true);
            }

            return ret;
        }

        public static IMetaData[] GetColumnAnalysis(this IDataTable table, bool force = false, uint distinctValueCount = 100)
        {
            var count = table.ColumnCount;
            var columnMetaData = table.AllColumnsMetaData();
            var ret = new IMetaData[count];
            for (uint i = 0; i < count; i++) {
                if (columnMetaData[i].Get(Consts.HasBeenAnalysed, false))
                    ret[i] = columnMetaData[i];
                else {
                    var column = table.Column(i);
                    ret[i] = column.Analyse(force, distinctValueCount);
                }
            }
            return ret;
        }

        public static IColumnOrientedDataTable ParseCsv(
            this IBrightDataContext context,
            StreamReader reader,
            bool hasHeader,
            char delimiter = ',',
            string fileOutputPath = null,
            uint inMemoryRowCount = 32768,
            ushort maxDistinct = 1024,
            bool writeProgress = false,
            string tempBasePath = null
        )
        {
            var parser = new CsvParser(reader, delimiter);
            using var tempStreams = new TempStreamManager(tempBasePath);
            var columns = new List<GrowableSegment<string>>();
            var isFirst = hasHeader;
            uint rowCount = 0;

            if (writeProgress) {
                var progress = -1;
                parser.OnProgress = p => p.WriteProgress(ref progress);
            }

            foreach (var row in parser.Parse()) {
                var cols = row.Length;

                for (var i = columns.Count; i < cols; i++) {
                    var buffer = context.CreateHybridStringBuffer(tempStreams, inMemoryRowCount, maxDistinct);
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

        public static List<object[]> Head(this IDataTable dataTable, uint size = 10)
        {
            var ret = new List<object[]>();
            dataTable.ForEachRow((row, index) => ret.Add(row), size);
            return ret;
        }

        public static IDataTable LoadTable(this IBrightDataContext context, string filePath)
        {
            var input = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(input, Encoding.UTF8, true);
            var version = reader.ReadInt32();

            if (version > Consts.DataTableVersion)
                throw new Exception($"Segment table version {version} exceeds {Consts.DataTableVersion}");
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
            var type = GetDataType(column.SingleType);
            var copySegment = typeof(ExtensionMethods).GetMethod("CopyToFloatSegment").MakeGenericMethod(type);
            return (uint)copySegment.Invoke(null, new object[] { column, vector });
        }

        public static void SetTargetColumn(this IDataTable table, uint? columnIndex)
        {
            var metaData = table.AllMetaData().ToList();
            for (uint i = 0; i < table.ColumnCount; i++) {
                metaData[(int)i].Set(Consts.IsTarget, i == columnIndex);
            }
        }

        public static uint? GetTargetColumn(this IDataTable table)
        {
            var metaData = table.AllMetaData().ToList();
            for (uint i = 0; i < table.ColumnCount; i++) {
                if (metaData[(int)i].IsTarget())
                    return i;
            }
            return null;
        }

        public static uint GetTargetColumnOrThrow(this IDataTable table)
        {
            return GetTargetColumn(table) ?? throw new Exception("No target column was set on the table");
        }

        public static IEnumerable<uint> ColumnIndicesOfFeatures(this IDataTable table)
        {
            var targetColumn = table.GetTargetColumn();
            var ret = table.ColumnIndices();
            if (targetColumn.HasValue)
                ret = ret.Where(i => i != targetColumn.Value);
            return ret;
        }

        public static IMetaData SetType(this IMetaData metaData, ColumnType type)
        {
            metaData.Set(Consts.Type, (byte)type);
            return metaData;
        }

        //public static void SetFeatureColumn(this IDataTable table, params uint[] columnIndices)
        //{
        //    var metaData = table.AllMetaData();
        //    var featureColumns = new HashSet<uint>(columnIndices);

        //    for (uint i = 0; i < table.ColumnCount; i++) {
        //        metaData[(int)i].Set(Consts.IsFeature, featureColumns.Contains(i));
        //    }
        //}

        public static void SetSequentialColumn(this IDataTable table, params uint[] columnIndices)
        {
            var metaData = table.AllMetaData().ToList();
            var featureColumns = new HashSet<uint>(columnIndices);

            for (uint i = 0; i < table.ColumnCount; i++) {
                metaData[(int)i].Set(Consts.IsSequential, featureColumns.Contains(i));
            }
        }

        public static IHybridBuffer GetGrowableSegment(this IMetaData metaData, ColumnType type, IBrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024)
        {
            var columnType = GetDataType(type);

            IHybridBuffer buffer;
            if (type.IsStructable())
                buffer = context.CreateHybridStructBuffer(GetDataType(type), tempStream, bufferSize, maxDistinct);
            else if (type == ColumnType.String)
                buffer = context.CreateHybridStringBuffer(tempStream, bufferSize, maxDistinct);
            else
                buffer = context.CreateHybridObjectBuffer(GetDataType(type), tempStream, bufferSize);

            var segmentType = typeof(GrowableSegment<>).MakeGenericType(columnType);
            return GenericActivator.Create<IHybridBuffer>(segmentType,
                type,
                new MetaData(metaData, Consts.StandardMetaData),
                buffer
            );
        }

        public static IColumnOrientedDataTable BuildColumnOrientedTable(this List<ISingleTypeTableSegment> segments, IBrightDataContext context, uint rowCount, string? filePath = null)
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

        public static IColumnOrientedDataTable BuildColumnOrientedTable(this List<IHybridBuffer> buffers, IBrightDataContext context, uint rowCount, string? filePath = null)
        {
            return buffers.Cast<ISingleTypeTableSegment>().ToList().BuildColumnOrientedTable(context, rowCount, filePath);
        }

        public static IColumnOrientedDataTable BuildColumnOrientedTable<T>(this List<GrowableSegment<T>> buffers, IBrightDataContext context, uint rowCount, string? filePath = null)
        {
            return buffers.Cast<ISingleTypeTableSegment>().ToList().BuildColumnOrientedTable(context, rowCount, filePath);
        }

        public static IRowOrientedDataTable BuildRowOrientedTable(this List<IHybridBuffer> buffers, IBrightDataContext context, uint rowCount, string? filePath = null)
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

        public static IEnumerable<(Vector<float> Numeric, string? Label)> GetVectorisedFeatures(this IDataTable dataTable)
        {
            var target = dataTable.GetTargetColumn();
            var vectoriser = new DataTableVectoriser(dataTable, dataTable.ColumnIndicesOfFeatures().ToArray());
            if (target.HasValue) {
                var targetColumn = dataTable.Column(target.Value).Enumerate().Select(o => o.ToString());
                return vectoriser.Enumerate().Zip(targetColumn);
            }

            return vectoriser.Enumerate().Select(v => (v, (string?) null));
        }

        public static ColumnType GetColumnType(this IMetaData metadata) => metadata.Get<ColumnType>(Consts.Type, ColumnType.Unknown);
        public static uint GetNumDistinct(this IMetaData metadata) => metadata.Get<uint>(Consts.NumDistinct, 0);

        public static TableBuilder BuildTable(this IBrightDataContext context) => new TableBuilder(context);

        public static IRowOrientedDataTable ToRowOriented(this IDataTable table, string? filePath = null)
        {
            if (table.Orientation == DataTableOrientation.RowOriented)
                return (IRowOrientedDataTable)table;
            var columnOriented = (IColumnOrientedDataTable)table;
            return columnOriented.AsRowOriented(filePath);
        }

        public static IColumnOrientedDataTable ToColumnOriented(this IDataTable table, string? filePath = null)
        {
            if (table.Orientation == DataTableOrientation.ColumnOriented)
                return (IColumnOrientedDataTable)table;
            var rowOriented = (IRowOrientedDataTable)table;
            return rowOriented.AsColumnOriented(filePath);
        }

        public static (IRowOrientedDataTable Training, IRowOrientedDataTable Test) Split(this IRowOrientedDataTable table, double trainingPercentage = 0.8, string? trainingFilePath = null, string? testFilePath = null)
        {
            var (training, test) = table.RowIndices().Shuffle(table.Context.Random).ToArray().Split(trainingPercentage);
            return (table.CopyRows(trainingFilePath, training), table.CopyRows(testFilePath, test));
        }

        interface IHaveFloatArray
        {
            float[] Data { get; }
        }
        class ColumnReader<T> : ITypedRowConsumer<T>, IHaveFloatArray
            where T : struct
        {
            readonly List<float> _data = new List<float>();
            readonly ICanConvert<T, float> _converter = StaticConverters.ConvertToFloat<T>();

            public ColumnReader(uint columnIndex, ColumnType type)
            {
                ColumnIndex = columnIndex;
                ColumnType = type;
            }

            public uint ColumnIndex { get; }
            public ColumnType ColumnType { get; }
            public void Add(T value)
            {
                _data.Add(_converter.Convert(value));
            }

            public float[] Data => _data.ToArray();
        }

        static (ITypedRowConsumer Consumer, IHaveFloatArray Array) _GetColumnReader(uint columnIndex, ColumnType columnType)
        {
            if (!columnType.IsNumeric())
                throw new ArgumentException("Column is not numeric");
            var dataType = GetDataType(columnType);
            return GenericActivator.Create<ITypedRowConsumer, IHaveFloatArray>(typeof(ColumnReader<>).MakeGenericType(dataType), columnIndex, columnType);
        }

        public static IEnumerable<Vector<float>> GetColumnsAsVectors(this IDataTable dataTable, params uint[] columnIndices)
        {
            var readers = columnIndices.Select(i => _GetColumnReader(i, dataTable.ColumnTypes[i])).ToList();
            var consumers = readers.Select(r => r.Consumer).ToArray();
            dataTable.ReadTyped(consumers);
            var context = dataTable.Context;
            return readers.Select(r => context.CreateVector(r.Array.Data));
        }

        public static IEnumerable<T> EnumerateTyped<T>(this ISingleTypeTableSegment segment) => ((IDataTableSegment<T>)segment).EnumerateTyped();

        public static T[] ToArray<T>(this ISingleTypeTableSegment segment) => EnumerateTyped<T>(segment).ToArray();

        public static (Matrix<float> Features, Matrix<float> Target) AsMatrices(this IDataTable dataTable)
        {
            var targetColumn = dataTable.GetTargetColumnOrThrow();
            var featureColumns = dataTable.ColumnIndices().Where(i => i != targetColumn).ToArray();
            return (AsMatrix(dataTable, featureColumns), AsMatrix(dataTable, targetColumn));
        }

        public static Matrix<float> AsMatrix(this IDataTable dataTable, params uint[] columnIndices)
        {
            // consider the simple case
            if (columnIndices.Length == 1 && dataTable.ColumnTypes[columnIndices[0]] == ColumnType.Vector) {
                var index = 0;
                var rows = new Vector<float>[dataTable.RowCount];
                foreach(var row in dataTable.Column(columnIndices[0]).Enumerate())
                    rows[index++] = (Vector<float>)row;
                return dataTable.Context.CreateMatrixFromRows(rows);
            }

            var vectoriser = new DataTableVectoriser(dataTable, columnIndices);
            return dataTable.Context.CreateMatrixFromRows(vectoriser.Enumerate().ToArray());
        }

        public static IRowOrientedDataTable Vectorise(this IDataTable dataTable, string? filePath = null)
        {
            var target = dataTable.GetTargetColumn();
            var columnIndices = dataTable.ColumnIndices().ToList();

            var builder = new RowOrientedTableBuilder(dataTable.RowCount, filePath);
            builder.AddColumn(ColumnType.Vector, "Input");

            DataTableVectoriser outputVectoriser = null;
            if (target.HasValue) {
                builder.AddColumn(ColumnType.Vector, "Target").SetTarget(true);
                outputVectoriser = new DataTableVectoriser(dataTable, target.Value);
                columnIndices.Remove(target.Value);
            }
            var inputVectoriser = new DataTableVectoriser(dataTable, columnIndices.ToArray());

            var context = dataTable.Context;
            dataTable.ForEachRow(row => {
                var input = inputVectoriser.Vectorise(row);
                if (outputVectoriser != null)
                    builder.AddRow(context.CreateVector(input), context.CreateVector(outputVectoriser.Vectorise(row)));
                else
                    builder.AddRow(input);
            });

            return builder.Build(context);
        }

        public static IDataTableSegment<T> Column<T>(this IDataTable dataTable, uint index) => (IDataTableSegment<T>) dataTable.Column(index);

        public static T[] ToArray<T>(this IDataTableSegment<T> segment) => segment.EnumerateTyped().ToArray();

        /// <summary>
        /// Converts indexed classifications to a data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        public static IDataTable ConvertToTable(this IReadOnlyList<(string Label, IndexList Data)> data, IBrightDataContext context)
        {
            var builder = context.BuildTable();
            builder.AddColumn(ColumnType.IndexList, "Index");
            builder.AddColumn(ColumnType.String, "Label").SetTarget(true);

            foreach (var item in data)
                builder.AddRow(item.Data, item.Label);

            return builder.BuildRowOriented();
        }

        /// <summary>
        /// Converts weighted index classifications to a data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        public static IRowOrientedDataTable ConvertToTable(this IReadOnlyList<(string Label, WeightedIndexList Data)> data, IBrightDataContext context)
        {
            var builder = context.BuildTable();
            builder.AddColumn(ColumnType.WeightedIndexList, "Weighted Index");
            builder.AddColumn(ColumnType.String, "Label").SetTarget(true);

            foreach (var item in data)
                builder.AddRow(item.Data, item.Label);

            return builder.BuildRowOriented();
        }

        /// <summary>
        /// Converts the vector classifications into a data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="preserveVectors">True to create a data table with a vector column type, false to to convert to columns of floats</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IDataTable ConvertToTable(this IReadOnlyList<(string Label, Vector<float> Data)> data, bool preserveVectors, IBrightDataContext context)
        {
            var builder = context.BuildTable();
            if (preserveVectors)
            {
                builder.AddColumn(ColumnType.Vector, "Vector");
                builder.AddColumn(ColumnType.String, "Label").SetTarget(true);

                foreach (var item in data)
                    builder.AddRow(item.Data, item.Label);
            }
            else
            {
                var size = data.First().Data.Size;
                for (var i = 1; i <= size; i++)
                    builder.AddColumn(ColumnType.Float, "Value " + i);
                builder.AddColumn(ColumnType.String, "Label").SetTarget(true);

                foreach (var item in data)
                {
                    var vector = item.Data;
                    var row = new List<object>();
                    for (var i = 0; i < size; i++)
                        row.Add(vector[i]);
                    row.Add(item.Label);
                    builder.AddRow(row);
                }
            }

            return builder.BuildRowOriented();
        }

        /// <summary>
        /// Converts the weighted index classification list to a list of dense vectors
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        public static IReadOnlyList<(string Classification, Vector<float> Data)> Vectorise(this IReadOnlyList<(string Label, WeightedIndexList Data)> data, IBrightDataContext context)
        {
            var size = data.GetMaxIndex() + 1;
            Vector<float> _Create(WeightedIndexList weightedIndexList)
            {
                var ret = new float[size];
                foreach (var item in weightedIndexList.Indices)
                    ret[item.Index] = item.Weight;
                return context.CreateVector(ret);
            }
            return data.Select(r => (r.Label, _Create(r.Data))).ToList();
        }

        public static object? GetDefaultValue(this ColumnType columnType)
        {
            if (columnType == ColumnType.String)
                return "";
            if (columnType == ColumnType.Date)
                return DateTime.MinValue;
            if (columnType != ColumnType.Unknown) {
                var dataType = columnType.GetDataType();
                if (dataType.GetTypeInfo().IsValueType)
                    return Activator.CreateInstance(dataType);
            }

            return null;
        }
    }
}
