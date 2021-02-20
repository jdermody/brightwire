using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BrightData.Analysis;
using BrightData.Buffer;
using BrightData.Converter;
using BrightData.DataTable;
using BrightData.DataTable.Builders;
using BrightData.Helper;
using BrightData.Input;
using BrightData.LinearAlgebra;
using BrightData.Transformation;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Converts from a column type to a Type
        /// </summary>
        /// <param name="type">Column type</param>
        /// <returns></returns>
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
            } ?? throw new NotImplementedException();
        }

        /// <summary>
        /// Converts from a Type to a ColumnType
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static ColumnType GetColumnType(this Type dataType)
        {
            var typeCode = Type.GetTypeCode(dataType);
            switch (typeCode)
            {
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

        /// <summary>
        /// Checks if the column type is blittable
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static bool IsStructable(this ColumnType columnType) => ColumnTypeClassifier.IsBlittable(columnType);

        /// <summary>
        /// Checks if the column type is numeric
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static bool IsNumeric(this ColumnType columnType) => ColumnTypeClassifier.IsNumeric(columnType);

        /// <summary>
        /// Checks if the column type is decimal
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static bool IsDecimal(this ColumnType columnType) => ColumnTypeClassifier.IsDecimal(columnType);

        /// <summary>
        /// Checks if the column type is continuous
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static bool IsContinuous(this ColumnType columnType) => ColumnTypeClassifier.IsContinuous(columnType);

        /// <summary>
        /// Checks if the column type is an integer
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsInteger(this ColumnType type) => type switch
        {
            ColumnType.Byte => true,
            ColumnType.Short => true,
            ColumnType.Int => true,
            ColumnType.Long => true,
            _ => false
        };

        /// <summary>
        /// Checks if the column type is an indexed list (or weighted index list)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIndexedList(this ColumnType type) => type switch
        {
            ColumnType.IndexList => true,
            ColumnType.WeightedIndexList => true,
            _ => false
        };

        /// <summary>
        /// Checks if the column type is a tensor
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTensor(this ColumnType type) => type switch
        {
            ColumnType.Vector => true,
            ColumnType.Matrix => true,
            ColumnType.Tensor3D => true,
            ColumnType.Tensor4D => true,
            _ => false
        };

        /// <summary>
        /// Returns the underlying Type for a data table segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static Type? DataType(this IDataTableSegment segment)
        {
            Type? ret = null;
            for (uint i = 0; i < segment.Size; i++)
            {
                var type = segment[i].GetType();
                if (ret == null)
                    ret = type;
                else if (type != null && type != ret)
                    return null;
            }
            return ret;
        }

        /// <summary>
        /// Returns the underlying Type for a data table segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Type GetDataType<T>(this IDataTableSegment<T> _) where T : notnull => typeof(T);

        /// <summary>
        /// Returns all row indices as an enumerable
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<uint> RowIndices(this IDataTable dataTable)
        {
            return dataTable.RowCount.AsRange();
        }

        /// <summary>
        /// Returns all column indices as an enumerable
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<uint> ColumnIndices(this IDataTable dataTable)
        {
            return dataTable.ColumnCount.AsRange();
        }

        /// <summary>
        /// Invokes a callback on each row of a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="callback"></param>
        public static void ForEachRow(this IDataTable dataTable, Action<object[]> callback)
        {
            dataTable.ForEachRow((row, index) => callback(row));
        }

        //public static void ForEachRow<T0>(this IDataTable dataTable, Action<T0> callback) => dataTable.ForEachRow((row, index) => callback((T0)row[0]));
        //public static void ForEachRow<T0, T1>(this IDataTable dataTable, Action<T0, T1> callback) => dataTable.ForEachRow((row, index) => callback((T0)row[0], (T1)row[1]));
        //public static void ForEachRow<T0, T1, T2>(this IDataTable dataTable, Action<T0, T1, T2> callback) => dataTable.ForEachRow((row, index) => callback((T0)row[0], (T1)row[1], (T2)row[2]));
        //public static void ForEachRow<T0, T1, T2, T3>(this IDataTable dataTable, Action<T0, T1, T2, T3> callback) => dataTable.ForEachRow((row, index) => callback((T0)row[0], (T1)row[1], (T2)row[2], (T3)row[3]));

        //public static List<T> MapRows<T>(this IDataTable dataTable, Func<object[], uint, T> callback)
        //{
        //    var ret = new List<T>();
        //    dataTable.ForEachRow((row, index) => ret.Add(callback(row, index)));
        //    return ret;
        //}

        //public static List<T> MapRows<T0, T>(this IDataTable dataTable, Func<T0, T> callback) => MapRows(dataTable, (rows, index) => callback((T0)rows[0]));
        //public static List<T> MapRows<T0, T1, T>(this IDataTable dataTable, Func<T0, T1, T> callback) => MapRows(dataTable, (rows, index) => callback((T0)rows[0], (T1)rows[1]));

        /// <summary>
        /// Returns meta data for all columns
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IMetaData[] AllColumnsMetaData(this IDataTable dataTable)
        {
            var ret = new IMetaData[dataTable.ColumnCount];
            for (uint i = 0, len = dataTable.ColumnCount; i < len; i++)
                ret[i] = dataTable.ColumnMetaData(i);
            return ret;
        }

        /// <summary>
        /// Enumerates metadata for each specified column
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices">Column indices to retrieve</param>
        public static IEnumerable<IMetaData> ColumnMetaData(this IDataTable dataTable, params uint[] columnIndices)
        {
            foreach (var index in columnIndices)
                yield return dataTable.ColumnMetaData(index);
        }

        /// <summary>
        /// Creates a column analyser
        /// </summary>
        /// <param name="type">Column type</param>
        /// <param name="metaData">Column meta data</param>
        /// <param name="writeCount">Maximum size of sequences to write in final meta data</param>
        /// <param name="maxCount">Maximum number of distinct items to track</param>
        /// <returns></returns>
        public static IDataAnalyser GetColumnAnalyser(this ColumnType type, IMetaData metaData, uint writeCount = Consts.MaxWriteCount, uint maxCount = Consts.MaxDistinct)
        {
            var dataType = ColumnTypeClassifier.GetClass(type, metaData);
            if ((dataType & ColumnClass.Categorical) != 0)
            {
                if (type == ColumnType.String)
                    return StaticAnalysers.CreateStringAnalyser(maxCount, writeCount);
                return StaticAnalysers.CreateFrequencyAnalyser(type.GetDataType(), maxCount, writeCount);
            }
            if ((dataType & ColumnClass.IndexBased) != 0)
                return StaticAnalysers.CreateIndexAnalyser(maxCount, writeCount);
            if ((dataType & ColumnClass.Tensor) != 0)
                return StaticAnalysers.CreateDimensionAnalyser(maxCount);

            switch (type)
            {
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

        /// <summary>
        /// Parse CSV into a column oriented data table
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="reader">CSV</param>
        /// <param name="hasHeader">True if the CSV has a text based header</param>
        /// <param name="delimiter">CSV delimiter</param>
        /// <param name="fileOutputPath">Optional path to save final table</param>
        /// <param name="writeProgress"></param>
        /// <param name="maxRows">Maximum number of rows of CSV to read</param>
        /// <param name="inMemoryRowCount">Number of rows to cache in memory</param>
        /// <param name="maxDistinct">Maximum number of distinct items to track</param>
        /// <param name="tempBasePath"></param>
        /// <returns></returns>
        public static IColumnOrientedDataTable ParseCsv(this IBrightDataContext context,
            StreamReader reader,
            bool hasHeader,
            char delimiter = ',',
            string? fileOutputPath = null,
            bool writeProgress = false,
            int maxRows = int.MaxValue,
            uint inMemoryRowCount = 32768,
            ushort maxDistinct = 1024,
            string? tempBasePath = null)
        {
            var parser = new CsvParser(reader, delimiter);
            using var tempStreams = new TempStreamManager(tempBasePath);
            var columns = new List<GrowableSegment<string>>();
            var isFirst = hasHeader;
            uint rowCount = 0;

            // write file metadata
            var metaData = new MetaData();
            if (reader.BaseStream is FileStream fs) {
                metaData.Set(Consts.Name, Path.GetFileName(fs.Name));
                metaData.Set(Consts.Source, fs.Name);
            }

            if (writeProgress)
            {
                var progress = -1;
                Console.WriteLine($"Parsing CSV...");
                parser.OnProgress = p => p.WriteProgress(ref progress);
                parser.OnComplete = Console.WriteLine;
            }

            foreach (var row in parser.Parse().Take(maxRows))
            {
                var cols = row.Length;

                for (var i = columns.Count; i < cols; i++)
                {
                    var buffer = context.CreateHybridStringBuffer(tempStreams, inMemoryRowCount, maxDistinct);
                    columns.Add(new GrowableSegment<string>(ColumnType.String, new MetaData(), buffer));
                }

                for (var i = 0; i < cols; i++)
                {
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

            var segments = columns.Cast<ISingleTypeTableSegment>().ToList();
            if (writeProgress)
            {
                Console.WriteLine($"Read {rowCount:N0} lines into {columns.Count:N0} columns");
                int index = 1;
                return segments.BuildColumnOrientedTable(metaData, context, rowCount, fileOutputPath, segment => Console.Write($"{index++}/{columns.Count}) {segment.MetaData.GetName()}..."), size => Console.WriteLine($"done ({size:N0} bytes)"));
            }
            return segments.BuildColumnOrientedTable(metaData, context, rowCount, fileOutputPath);
        }

        /// <summary>
        /// Writes a progress bar to the console
        /// </summary>
        /// <param name="newProgress">Current progress</param>
        /// <param name="oldProgress">Previous progress</param>
        /// <param name="max">Maximum progress</param>
        public static void WriteProgress(this int newProgress, ref int oldProgress, int max = 100)
        {
            if (newProgress > oldProgress)
            {
                var sb = new StringBuilder();
                sb.Append('\r');
                for (var i = 0; i < max; i++)
                    sb.Append(i < newProgress ? '█' : '_');
                sb.Append($" ({oldProgress = newProgress}%)");
                Console.Write(sb.ToString());
            }
        }

        /// <summary>
        /// Returns the head (first few rows) of the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="size">Number of rows to return</param>
        /// <returns></returns>
        public static List<object[]> Head(this IDataTable dataTable, uint size = 10)
        {
            var ret = new List<object[]>();
            dataTable.ForEachRow((row, index) => ret.Add(row), size);
            return ret;
        }

        /// <summary>
        /// Loads a data table from disk
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath">File path on disk</param>
        /// <returns></returns>
        public static IDataTable LoadTable(this IBrightDataContext context, string filePath)
        {
            using var input = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var streamCloner = new StreamCloner(input);
            using var reader = new BinaryReader(input, Encoding.UTF8, true);
            var version = reader.ReadInt32();

            if (version > Consts.DataTableVersion)
                throw new Exception($"Segment table version {version} exceeds {Consts.DataTableVersion}");
            var orientation = (DataTableOrientation)reader.ReadByte();
            if (orientation == DataTableOrientation.ColumnOriented)
                return new ColumnOrientedDataTable(context, input, false, streamCloner);
            if (orientation == DataTableOrientation.RowOriented)
                return new RowOrientedDataTable(context, input, false);
            throw new Exception($"Found unknown data table orientation: {orientation}");
        }

        /// <summary>
        /// Copies a data table segment to a tensor segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column">Data table segment</param>
        /// <param name="vector">Tensor segment</param>
        /// <returns></returns>
        public static uint CopyToFloatSegment<T>(this IDataTableSegment<T> column, ITensorSegment<float> vector)
            where T : struct
        {
            uint index = 0;
            var converter = column.Context.GetFloatConverter<T>();

            foreach (var item in column.EnumerateTyped().Take((int)vector.Size))
                vector[index++] = converter.Convert(item);
            return index;
        }

        /// <summary>
        /// Copies a data table segment to tensor segment
        /// </summary>
        /// <param name="column">Data table segment</param>
        /// <param name="vector">Tensor segment</param>
        /// <returns></returns>
        public static uint CopyTo(this ISingleTypeTableSegment column, ITensorSegment<float> vector)
        {
            var type = GetDataType(column.SingleType);
            var copySegment = typeof(ExtensionMethods).GetMethod("CopyToFloatSegment")!.MakeGenericMethod(type);
            return (uint)copySegment.Invoke(null, new object[] { column, vector })!;
        }

        /// <summary>
        /// Sets the target column of the data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnIndex">Column index to make target (or null to set no target)</param>
        public static void SetTargetColumn(this IDataTable table, uint? columnIndex)
        {
            var metaData = table.AllColumnsMetaData();
            for (uint i = 0; i < table.ColumnCount; i++)
                metaData[i].Set(Consts.IsTarget, i == columnIndex);
        }

        /// <summary>
        /// Returns the target column of the data table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static uint? GetTargetColumn(this IDataTable table)
        {
            var metaData = table.AllColumnsMetaData();
            for (uint i = 0; i < table.ColumnCount; i++) {
                if (metaData[i].IsTarget())
                    return i;
            }
            return null;
        }

        /// <summary>
        /// Returns the target column or throws an exception if none set
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static uint GetTargetColumnOrThrow(this IDataTable table)
        {
            return GetTargetColumn(table) ?? throw new Exception("No target column was set on the table");
        }

        /// <summary>
        /// Returns the feature (non target) columns of the data table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<uint> ColumnIndicesOfFeatures(this IDataTable table)
        {
            var targetColumn = table.GetTargetColumn();
            var ret = table.ColumnIndices();
            if (targetColumn.HasValue)
                ret = ret.Where(i => i != targetColumn.Value);
            return ret;
        }

        /// <summary>
        /// Sets the column type in a meta data store
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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

        //public static void SetSequentialColumn(this IDataTable table, params uint[] columnIndices)
        //{
        //    var metaData = table.AllMetaData().ToList();
        //    var featureColumns = new HashSet<uint>(columnIndices);

        //    for (uint i = 0; i < table.ColumnCount; i++)
        //    {
        //        metaData[(int)i].Set(Consts.IsSequential, featureColumns.Contains(i));
        //    }
        //}

        /// <summary>
        /// Creates an appendable buffer for a column type
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="type">Column type</param>
        /// <param name="context"></param>
        /// <param name="tempStream"></param>
        /// <param name="bufferSize">In memory cache size</param>
        /// <param name="maxDistinct">Maximum number of distinct items to track</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a column oriented data table from a list of segments
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="context"></param>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="filePath">File path to save on disk (optional)</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable BuildColumnOrientedTable(this List<ISingleTypeTableSegment> segments, IMetaData metaData, IBrightDataContext context, uint rowCount, string? filePath = null, Action<ISingleTypeTableSegment>? onBeforeWrite = null, Action<long>? onAfterWrite = null)
        {
            var columnCount = (uint)segments.Count;
            var columnOffsets = new List<(long Position, long EndOfColumnOffset)>();
            using var builder = new ColumnOrientedTableBuilder(filePath);

            builder.WriteHeader(columnCount, rowCount, metaData);
            foreach (var segment in segments) {
                onBeforeWrite?.Invoke(segment);
                var position = builder.Write(segment);
                var endPosition = builder.GetCurrentPosition();
                onAfterWrite?.Invoke(endPosition-position);
                columnOffsets.Add((position, endPosition));
            }
            builder.WriteColumnOffsets(columnOffsets);
            return builder.Build(context);
        }

        /// <summary>
        /// Creates a row oriented data table from a list of segments
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="context"></param>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="filePath">File path to save on disk (optional)</param>
        /// <returns></returns>
        public static IRowOrientedDataTable BuildRowOrientedTable(this List<ISingleTypeTableSegment> segments, IMetaData metaData, IBrightDataContext context, uint rowCount, string? filePath = null)
        {
            using var builder = new RowOrientedTableBuilder(metaData, rowCount, filePath);
            var readers = segments
                .Select(b => b.Enumerate().GetEnumerator())
                .ToList();
            while (readers.All(r => r.MoveNext())) {
                var row = readers.Select(r => r.Current).ToArray();
                builder.AddRow(row);
            }
            return builder.Build(context);
        }

        /// <summary>
        /// Creates a column info with a new column type
        /// </summary>
        /// <param name="column">Column info source</param>
        /// <param name="newType">New column type</param>
        /// <returns></returns>
        public static IColumnInfo ChangeColumnType(this IColumnInfo column, ColumnType newType)
        {
            if (column.ColumnType == newType)
                return column;
            return new ColumnInfo(column.Index, newType, new MetaData(column.MetaData, Consts.Index, Consts.Name));
        }

        /// <summary>
        /// Creates a convertible data table
        /// </summary>
        /// <param name="dataTable">Data table to use as basis</param>
        /// <returns></returns>
        public static IConvertibleTable AsConvertible(this IRowOrientedDataTable dataTable)
        {
            return new DataTableConverter(dataTable);
        }

        /// <summary>
        /// Converts the data table to a sequence of labeled vectors (feature columns are vectorised, target column is converted to a string)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<(Vector<float> Numeric, string? Label)> GetVectorisedFeatures(this IDataTable dataTable)
        {
            var target = dataTable.GetTargetColumn();
            var vectoriser = new DataTableVectoriser(dataTable, true, dataTable.ColumnIndicesOfFeatures().ToArray());
            if (target.HasValue) {
                var targetColumn = dataTable.Column(target.Value).Enumerate().Select(o => o.ToString());
                return vectoriser.Enumerate().Zip(targetColumn);
            }

            return vectoriser.Enumerate().Select(v => (v, (string?)null));
        }

        /// <summary>
        /// Gets the column type
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static ColumnType GetColumnType(this IMetaData metadata) => metadata.Get(Consts.Type, ColumnType.Unknown);

        /// <summary>
        /// Gets the number of distinct items
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static uint GetNumDistinct(this IMetaData metadata) => metadata.Get<uint>(Consts.NumDistinct, 0);

        /// <summary>
        /// Creates a table builder
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static InMemoryTableBuilder BuildTable(this IBrightDataContext context) => new InMemoryTableBuilder(context);

        /// <summary>
        /// Splits a data table into training and test tables (rows are randomly selected for either)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="trainingFraction">Fraction (0..1) of rows to add to the training table</param>
        /// <param name="trainingFilePath">Path to write training table to disk (optional)</param>
        /// <param name="testFilePath">Path to write test table to disk (optional)</param>
        /// <returns></returns>
        public static (IRowOrientedDataTable Training, IRowOrientedDataTable Test) Split(this IRowOrientedDataTable table, double trainingFraction = 0.8, string? trainingFilePath = null, string? testFilePath = null)
        {
            var (training, test) = table.RowIndices().Shuffle(table.Context.Random).ToArray().Split(trainingFraction);
            return (table.CopyRows(trainingFilePath, training), table.CopyRows(testFilePath, test));
        }

        interface IHaveFloatArray
        {
            float[] Data { get; }
        }
        class ColumnReader<T> : IConsumeColumnData<T>, IHaveFloatArray
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

        static (IConsumeColumnData Consumer, IHaveFloatArray Array) GetColumnReader(uint columnIndex, ColumnType columnType)
        {
            if (!columnType.IsNumeric())
                throw new ArgumentException("Column is not numeric");
            var dataType = GetDataType(columnType);
            return GenericActivator.Create<IConsumeColumnData, IHaveFloatArray>(typeof(ColumnReader<>).MakeGenericType(dataType), columnIndex, columnType);
        }

        /// <summary>
        /// Returns columns as vectors
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices">Column indices to return as vectors</param>
        /// <returns></returns>
        public static IEnumerable<Vector<float>> GetColumnsAsVectors(this IDataTable dataTable, params uint[] columnIndices)
        {
            var readers = columnIndices.Select(i => GetColumnReader(i, dataTable.ColumnTypes[i])).ToList();
            dataTable.ReadTyped(readers.Select(r => r.Consumer));
            var context = dataTable.Context;
            return readers.Select(r => context.CreateVector(r.Array.Data));
        }

        /// <summary>
        /// Returns analysed column meta data
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices">Column indices</param>
        /// <returns></returns>
        public static IEnumerable<(uint ColumnIndex, IMetaData MetaData)> ColumnAnalysis(this IDataTable dataTable, params uint[] columnIndices) => dataTable.ColumnAnalysis(columnIndices);

        /// <summary>
        /// Returns analysed column meta data for all columns in the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IMetaData[] AllColumnAnalysis(this IDataTable dataTable) => dataTable.ColumnAnalysis(dataTable.ColumnCount.AsRange()).Select(d => d.MetaData).ToArray();

        /// <summary>
        /// Strongly typed enumeration of items in segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static IEnumerable<T> EnumerateTyped<T>(this ISingleTypeTableSegment segment) where T : notnull => ((IDataTableSegment<T>)segment).EnumerateTyped();

        /// <summary>
        /// Reads the segment as a strongly typed array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(this ISingleTypeTableSegment segment) where T : notnull => EnumerateTyped<T>(segment).ToArray();

        /// <summary>
        /// Converts the data table to feature and target matrices
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static (Matrix<float> Features, Matrix<float> Target) AsMatrices(this IDataTable dataTable)
        {
            var targetColumn = dataTable.GetTargetColumnOrThrow();
            var featureColumns = dataTable.ColumnIndices().Where(i => i != targetColumn).ToArray();
            return (AsMatrix(dataTable, featureColumns), AsMatrix(dataTable, targetColumn));
        }

        /// <summary>
        /// Converts data table columns to a matrix
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices">Column indices to include in the matrix</param>
        /// <returns></returns>
        public static Matrix<float> AsMatrix(this IDataTable dataTable, params uint[] columnIndices)
        {
            // consider the simple case
            if (columnIndices.Length == 1)
            {
                var columnType = dataTable.ColumnTypes[columnIndices[0]];
                if (columnType == ColumnType.Vector)
                {
                    var index = 0;
                    var rows = new Vector<float>[dataTable.RowCount];
                    foreach (var row in dataTable.Column(columnIndices[0]).Enumerate())
                        rows[index++] = (Vector<float>)row;
                    return dataTable.Context.CreateMatrixFromRows(rows);
                }

                if (columnType.IsNumeric())
                {
                    var ret = dataTable.Context.CreateMatrix<float>(dataTable.RowCount, 1);
                    dataTable.Column(columnIndices[0]).CopyTo(ret.Segment);
                    return ret;
                }
            }

            var vectoriser = new DataTableVectoriser(dataTable, false, columnIndices);
            return dataTable.Context.CreateMatrixFromRows(vectoriser.Enumerate().ToArray());
        }

        /// <summary>
        /// Creates a new data table that has two vector columns, one for the features and the other for the target
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="oneHotEncodeToMultipleColumns"></param>
        /// <param name="filePath">Optional path to save data table to disk</param>
        /// <returns></returns>
        public static IRowOrientedDataTable Vectorise(this IDataTable dataTable, bool oneHotEncodeToMultipleColumns, string? filePath = null)
        {
            var target = dataTable.GetTargetColumn();
            var columnIndices = dataTable.ColumnIndices().ToList();

            var builder = new RowOrientedTableBuilder(dataTable.MetaData, dataTable.RowCount, filePath);
            builder.AddColumn(ColumnType.Vector, "Features");

            DataTableVectoriser? outputVectoriser = null;
            if (target.HasValue)
            {
                builder.AddColumn(ColumnType.Vector, "Target").SetTarget(true);
                outputVectoriser = new DataTableVectoriser(dataTable, oneHotEncodeToMultipleColumns, target.Value);
                columnIndices.Remove(target.Value);
            }
            var inputVectoriser = new DataTableVectoriser(dataTable, oneHotEncodeToMultipleColumns, columnIndices.ToArray());

            var context = dataTable.Context;
            dataTable.ForEachRow(row =>
            {
                var input = inputVectoriser.Vectorise(row);
                if (outputVectoriser != null)
                    builder.AddRow(context.CreateVector(input), context.CreateVector(outputVectoriser.Vectorise(row)));
                else
                    builder.AddRow(input);
            });

            return builder.Build(context);
        }

        /// <summary>
        /// Returns a data table segment from the data table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static IDataTableSegment<T> Column<T>(this IDataTable dataTable, uint columnIndex) where T : notnull => (IDataTableSegment<T>)dataTable.Column(columnIndex);

        /// <summary>
        /// Converts a data table segment to an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(this IDataTableSegment<T> segment) where T : notnull => segment.EnumerateTyped().ToArray();

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

            foreach (var (label, indexList) in data)
                builder.AddRow(indexList, label);

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

            foreach (var (label, weightedIndexList) in data)
                builder.AddRow(weightedIndexList, label);

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

                foreach (var (label, vector) in data)
                    builder.AddRow(vector, label);
            }
            else
            {
                var size = data.First().Data.Size;
                for (var i = 1; i <= size; i++)
                    builder.AddColumn(ColumnType.Float, "Value " + i);
                builder.AddColumn(ColumnType.String, "Label").SetTarget(true);

                foreach (var (label, vector) in data)
                {
                    var row = new List<object>();
                    for (var i = 0; i < size; i++)
                        row.Add(vector[i]);
                    row.Add(label);
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
            Vector<float> Create(WeightedIndexList weightedIndexList)
            {
                var ret = new float[size];
                foreach (var item in weightedIndexList.Indices)
                    ret[item.Index] = item.Weight;
                return context.CreateVector(ret);
            }
            return data.Select(r => (r.Label, Create(r.Data))).ToList();
        }

        /// <summary>
        /// Returns a default value for a column type
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static object? GetDefaultValue(this ColumnType columnType)
        {
            if (columnType == ColumnType.String)
                return "";
            if (columnType == ColumnType.Date)
                return DateTime.MinValue;
            if (columnType != ColumnType.Unknown)
            {
                var dataType = columnType.GetDataType();
                if (dataType.GetTypeInfo().IsValueType)
                    return Activator.CreateInstance(dataType);
            }

            return null;
        }

        /// <summary>
        /// Returns a vectoriser
        /// </summary>
        /// <param name="table"></param>
        /// <param name="oneHotEncodeToMultipleColumns"></param>
        /// <param name="columnIndices">Column indices to vectorise</param>
        /// <returns></returns>
        public static IDataTableVectoriser GetVectoriser(this IDataTable table, bool oneHotEncodeToMultipleColumns, params uint[] columnIndices) => new DataTableVectoriser(table, oneHotEncodeToMultipleColumns, columnIndices);

        /// <summary>
        /// Converts columns
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="conversions"></param>
        /// <returns></returns>
        public static IColumnOrientedDataTable ConvertTable(this IColumnOrientedDataTable dataTable, params ColumnConversionType[] conversions)
        {
            return dataTable.Convert(conversions.Select((c, i) => (IColumnTransformationParam)new ColumnConversion((uint)i, c)).ToArray());
        }

        /// <summary>
        /// Creates a column conversion parameter
        /// </summary>
        /// <param name="type">Type of column conversion</param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static IColumnTransformationParam ConvertColumn(this ColumnConversionType type, uint columnIndex) => new ColumnConversion(columnIndex, type);

        /// <summary>
        /// Creates a column normalization parameter
        /// </summary>
        /// <param name="type"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static IColumnTransformationParam ConvertColumn(this NormalizationType type, uint columnIndex) => new ColumnNormalization(columnIndex, type);

        /// <summary>
        /// Creates a reinterpret columns parameter
        /// </summary>
        /// <param name="sourceColumnIndices"></param>
        /// <param name="newColumnType"></param>
        /// <param name="newColumnName"></param>
        /// <returns></returns>
        public static IReinterpretColumnsParam ReinterpretColumns(this uint[] sourceColumnIndices, ColumnType newColumnType, string newColumnName)
        {
            return new ReinterpretColumns(newColumnType, newColumnName, sourceColumnIndices);
        }
    }
}
