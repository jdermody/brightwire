using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BrightData.Analysis;
using BrightData.Buffer.Hybrid;
using BrightData.Buffer.InMemory;
using BrightData.Converter;
using BrightData.DataTable;
using BrightData.DataTable.Builders;
using BrightData.DataTable.Consumers;
using BrightData.Helper;
using BrightData.Input;
using BrightData.LinearAlegbra2;
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
        public static Type GetDataType(this BrightDataType type)
        {
            return type switch
            {
                BrightDataType.Boolean           => typeof(bool),
                BrightDataType.SByte             => typeof(sbyte),
                BrightDataType.Date              => typeof(DateTime),
                BrightDataType.Double            => typeof(double),
                BrightDataType.Decimal           => typeof(decimal),
                BrightDataType.Float             => typeof(float),
                BrightDataType.Short             => typeof(short),
                BrightDataType.Int               => typeof(int),
                BrightDataType.Long              => typeof(long),
                BrightDataType.Unknown           => null,
                BrightDataType.String            => typeof(string),
                BrightDataType.IndexList         => typeof(IndexList),
                BrightDataType.WeightedIndexList => typeof(WeightedIndexList),
                BrightDataType.Vector            => typeof(IVector),
                BrightDataType.Matrix            => typeof(IMatrix),
                BrightDataType.Tensor3D          => typeof(ITensor3D),
                BrightDataType.Tensor4D          => typeof(ITensor4D),
                BrightDataType.BinaryData        => typeof(BinaryData),
                _                                => throw new NotImplementedException()
            } ?? throw new NotImplementedException();
        }

        /// <summary>
        /// Converts from a Type to a ColumnType
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static BrightDataType GetBrightDataType(this Type dataType)
        {
            var typeCode = Type.GetTypeCode(dataType);
            if (typeCode == TypeCode.Boolean)
                return BrightDataType.Boolean;

            if (typeCode == TypeCode.SByte)
                return BrightDataType.SByte;

            if (typeCode == TypeCode.DateTime)
                return BrightDataType.Date;

            if (typeCode == TypeCode.Double)
                return BrightDataType.Double;

            if (typeCode == TypeCode.Decimal)
                return BrightDataType.Decimal;

            if (typeCode == TypeCode.Single)
                return BrightDataType.Float;

            if (typeCode == TypeCode.Int16)
                return BrightDataType.Short;

            if (typeCode == TypeCode.Int32)
                return BrightDataType.Int;

            if (typeCode == TypeCode.Int64)
                return BrightDataType.Long;

            if (typeCode == TypeCode.String)
                return BrightDataType.String;

            if (dataType == typeof(IndexList))
                return BrightDataType.IndexList;

            if (dataType == typeof(WeightedIndexList))
                return BrightDataType.WeightedIndexList;

            if (dataType == typeof(IVector) || dataType.IsAssignableTo(typeof(IVector)))
                return BrightDataType.Vector;

            if (dataType == typeof(IMatrix) || dataType.IsAssignableTo(typeof(IMatrix)))
                return BrightDataType.Matrix;

            if (dataType == typeof(ITensor3D) || dataType.IsAssignableTo(typeof(ITensor3D)))
                return BrightDataType.Tensor3D;

            if (dataType == typeof(ITensor4D) || dataType.IsAssignableTo(typeof(ITensor4D)))
                return BrightDataType.Tensor4D;

            if (dataType == typeof(BinaryData))
                return BrightDataType.BinaryData;

            throw new ArgumentException($"{dataType} has no corresponding bright data type");
        }

        /// <summary>
        /// Checks if the column type is blittable
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static bool IsBlittable(this BrightDataType columnType) => ColumnTypeClassifier.IsBlittable(columnType);

        /// <summary>
        /// Checks if the column type is numeric
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static bool IsNumeric(this BrightDataType columnType) => ColumnTypeClassifier.IsNumeric(columnType);

        /// <summary>
        /// Checks if the column type is decimal
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static bool IsDecimal(this BrightDataType columnType) => ColumnTypeClassifier.IsDecimal(columnType);

        /// <summary>
        /// Checks if the column type is continuous
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static bool IsContinuous(this BrightDataType columnType) => ColumnTypeClassifier.IsContinuous(columnType);

        /// <summary>
        /// Checks if the column type is an integer
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsInteger(this BrightDataType type) => type switch
        {
            BrightDataType.SByte => true,
            BrightDataType.Short => true,
            BrightDataType.Int   => true,
            BrightDataType.Long  => true,
            _                    => false
        };

        /// <summary>
        /// Checks if the column type is an indexed list (or weighted index list)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIndexedList(this BrightDataType type) => type switch
        {
            BrightDataType.IndexList         => true,
            BrightDataType.WeightedIndexList => true,
            _                                => false
        };

        /// <summary>
        /// Checks if the column type is a tensor
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTensor(this BrightDataType type) => type switch
        {
            BrightDataType.Vector   => true,
            BrightDataType.Matrix   => true,
            BrightDataType.Tensor3D => true,
            BrightDataType.Tensor4D => true,
            _                       => false
        };

        /// <summary>
        /// Returns the underlying Type for a data table segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static Type? GetDataType(this IDataTableSegment segment)
        {
            Type? ret = null;
            for (uint i = 0; i < segment.Size; i++) {
                var type = segment[i].GetType();
                if (ret == null)
                    ret = type;
                else if (type != ret)
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
            dataTable.ForEachRow((row, _) => callback(row));
        }

        /// <summary>
        /// Invokes a typed callback on each row of a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T0"></typeparam>
        public static void ForEachRow<T0>(this IDataTable dataTable, Action<T0> callback) => dataTable.ForEachRow((row, _) => callback((T0)row[0]));

        /// <summary>
        /// Invokes a typed callback on each row of a data table
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="dataTable"></param>
        /// <param name="callback"></param>
        public static void ForEachRow<T0, T1>(this IDataTable dataTable, Action<T0, T1> callback) => dataTable.ForEachRow((row, _) => callback((T0)row[0], (T1)row[1]));

        /// <summary>
        /// Invokes a typed callback on each row of a data table
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dataTable"></param>
        /// <param name="callback"></param>
        public static void ForEachRow<T0, T1, T2>(this IDataTable dataTable, Action<T0, T1, T2> callback) => dataTable.ForEachRow((row, _) => callback((T0)row[0], (T1)row[1], (T2)row[2]));

        /// <summary>
        /// Invokes a typed callback on each row of a data table
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="dataTable"></param>
        /// <param name="callback"></param>
        public static void ForEachRow<T0, T1, T2, T3>(this IDataTable dataTable, Action<T0, T1, T2, T3> callback) => dataTable.ForEachRow((row, _) => callback((T0)row[0], (T1)row[1], (T2)row[2], (T3)row[3]));

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
        public static IEnumerable<IMetaData> ColumnMetaData(this IDataTable dataTable, params uint[] columnIndices) => dataTable.AllOrSelectedColumnIndices(columnIndices).Select(dataTable.ColumnMetaData);

        /// <summary>
        /// Returns selected column indices or all if nothing selected
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices">Column indices to retrieve</param>
        /// <returns></returns>
        public static IEnumerable<uint> AllOrSelectedColumnIndices(this IDataTable dataTable, uint[] columnIndices) => columnIndices.Length > 0
            ? columnIndices
            : dataTable.ColumnIndices();

        /// <summary>
        /// Creates a column analyser
        /// </summary>
        /// <param name="type">Column type</param>
        /// <param name="metaData">Column meta data</param>
        /// <param name="writeCount">Maximum size of sequences to write in final meta data</param>
        /// <param name="maxCount">Maximum number of distinct items to track</param>
        /// <returns></returns>
        public static IDataAnalyser GetColumnAnalyser(this BrightDataType type, IMetaData metaData, uint writeCount = Consts.MaxWriteCount, uint maxDistinctCount = Consts.MaxDistinct)
        {
            var dataType = ColumnTypeClassifier.GetClass(type, metaData);
            if (dataType.HasFlag(ColumnClass.Categorical)) {
                if (type == BrightDataType.String)
                    return StaticAnalysers.CreateStringAnalyser(maxDistinctCount, writeCount);
                return StaticAnalysers.CreateFrequencyAnalyser(type.GetDataType(), maxDistinctCount, writeCount);
            }
            if (dataType.HasFlag(ColumnClass.IndexBased))
                return StaticAnalysers.CreateIndexAnalyser(maxDistinctCount, writeCount);
            if (dataType.HasFlag(ColumnClass.Tensor))
                return StaticAnalysers.CreateDimensionAnalyser(maxDistinctCount);

            return type switch
            {
                BrightDataType.Double     => StaticAnalysers.CreateNumericAnalyser(maxDistinctCount, writeCount),
                BrightDataType.Float      => StaticAnalysers.CreateNumericAnalyser<float>(maxDistinctCount, writeCount),
                BrightDataType.Decimal    => StaticAnalysers.CreateNumericAnalyser<decimal>(maxDistinctCount, writeCount),
                BrightDataType.SByte      => StaticAnalysers.CreateNumericAnalyser<sbyte>(maxDistinctCount, writeCount),
                BrightDataType.Int        => StaticAnalysers.CreateNumericAnalyser<int>(maxDistinctCount, writeCount),
                BrightDataType.Long       => StaticAnalysers.CreateNumericAnalyser<long>(maxDistinctCount, writeCount),
                BrightDataType.Short      => StaticAnalysers.CreateNumericAnalyser<short>(maxDistinctCount, writeCount),
                BrightDataType.Date       => StaticAnalysers.CreateDateAnalyser(),
                BrightDataType.BinaryData => StaticAnalysers.CreateFrequencyAnalyser<BinaryData>(maxDistinctCount, writeCount),
                _                         => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Parse CSV in memory without writing to disk (for small data sets)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader">Stream reader that contains CSV data</param>
        /// <param name="hasHeader">True if the data contains a header</param>
        /// <param name="delimiter">CSV delimiter character</param>
        /// <param name="maxRows">Maximum number of rows to read</param>
        /// <param name="maxDistinct">Maximum number of distinct items to track</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IColumnOrientedDataTable ParseCsvIntoMemory(this IBrightDataContext context,
            StreamReader reader,
            bool hasHeader,
            char delimiter = ',',
            int maxRows = int.MaxValue,
            ushort maxDistinct = 1024
        )
        {
            var userNotification = context.UserNotifications;
            var parser = new CsvParser(reader, delimiter);
            var columns = new List<InMemorySegment<string>>();
            var isFirst = hasHeader;
            uint rowCount = 0;

            // write file metadata
            var metaData = new MetaData();
            if (reader.BaseStream is FileStream fs) {
                metaData.Set(Consts.Name, Path.GetFileName(fs.Name));
                metaData.Set(Consts.Source, fs.Name);
            }

            if (userNotification is not null) {
                var operationId = Guid.NewGuid().ToString("n");
                userNotification.OnStartOperation(operationId, "Parsing CSV into memory...");
                parser.OnProgress = p => userNotification.OnOperationProgress(operationId, p);
                parser.OnComplete = () => {
                    userNotification.OnCompleteOperation(operationId, false);
                };
            }

            var ct = context.CancellationToken;
            foreach (var row in parser.Parse().Take(maxRows)) {
                if (ct.IsCancellationRequested)
                    break;
                var cols = row.Count;

                for (var i = columns.Count; i < cols; i++)
                    columns.Add(new InMemorySegment<string>(context, BrightDataType.String, new MetaData(), maxDistinct));

                for (var i = 0; i < cols; i++) {
                    var column = columns[i];
                    var text = row[i].ToString();
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
            if (segments.Any(s => s.Size != rowCount))
                throw new Exception("Columns have irregular sizes");

            return segments.BuildColumnOrientedTable(metaData, context, rowCount, null);
        }

        /// <summary>
        /// Parse CSV into a column oriented data table using hybrid buffers
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="reader">CSV</param>
        /// <param name="hasHeader">True if the CSV has a text based header</param>
        /// <param name="delimiter">CSV delimiter</param>
        /// <param name="fileOutputPath">Optional path to save final table</param>
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
            int maxRows = int.MaxValue,
            uint inMemoryRowCount = 32768,
            ushort maxDistinct = 1024,
            string? tempBasePath = null)
        {
            var userNotification = context.UserNotifications;
            var parser = new CsvParser(reader, delimiter);
            using var tempStreams = new TempStreamManager(tempBasePath);
            var columns = new List<HybridBufferSegment<string>>();
            var isFirst = hasHeader;
            uint rowCount = 0;

            // write file metadata
            var metaData = new MetaData();
            if (reader.BaseStream is FileStream fs) {
                metaData.Set(Consts.Name, Path.GetFileName(fs.Name));
                metaData.Set(Consts.Source, fs.Name);
            }

            // set up notifications
            if (userNotification is not null) {
                var operationId = Guid.NewGuid().ToString("n");
                userNotification.OnStartOperation(operationId, "Parsing CSV...");
                parser.OnProgress = p => userNotification.OnOperationProgress(operationId, p);
                parser.OnComplete = () => {
                    userNotification.OnCompleteOperation(operationId, false);
                };
            }

            var ct = context.CancellationToken;
            foreach (var row in parser.Parse().Take(maxRows)) {
                if (ct.IsCancellationRequested)
                    break;

                var cols = row.Count;
                for (var i = columns.Count; i < cols; i++) {
                    var buffer = context.CreateHybridStringBuffer(tempStreams, inMemoryRowCount, maxDistinct);
                    columns.Add(new HybridBufferSegment<string>(BrightDataType.String, new MetaData(), buffer));
                }

                for (var i = 0; i < cols; i++) {
                    var column = columns[i];
                    var text = row[i].ToString();
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
            if (segments.Any(s => s.Size != rowCount))
                throw new Exception("Columns have irregular sizes");

            return segments.BuildColumnOrientedTable(metaData, context, rowCount, fileOutputPath);
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
            dataTable.ForEachRow((row, _) => ret.Add(row), size);
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
            var input = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var streamCloner = new StreamCloner(input);
            using var reader = new BinaryReader(input, Encoding.UTF8, true);
            var version = reader.ReadInt32();

            if (version > Consts.DataTableVersion)
                throw new Exception($"Segment table version {version} exceeds {Consts.DataTableVersion}");
            var orientation = (DataTableOrientation)reader.ReadByte();
            if (orientation == DataTableOrientation.ColumnOriented) {
                var ret = new ColumnOrientedDataTable(context, input, false, streamCloner);
                input.Dispose();
                return ret;
            }

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
        /// Returns columns as vectors
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices">Column indices to return as vectors</param>
        /// <returns></returns>
        public static IEnumerable<Vector<float>> GetColumnsAsVectors(this IDataTable dataTable, params uint[] columnIndices)
        {
            var readers = dataTable.AllOrSelectedColumnIndices(columnIndices).Select(i => GetColumnReader(i, dataTable.ColumnTypes[i])).ToList();
            dataTable.ReadTyped(readers.Select(r => r.Consumer));
            var context = dataTable.Context;
            return readers.Select(r => context.CreateVector(r.Array.Data));
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
        public static IMetaData SetType(this IMetaData metaData, BrightDataType type)
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
        public static IHybridBuffer GetGrowableSegment(this IMetaData metaData, BrightDataType type, IBrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024)
        {
            var columnType = GetDataType(type);

            IHybridBuffer buffer;
            if (type.IsBlittable())
                buffer = context.CreateHybridStructBuffer(GetDataType(type), tempStream, bufferSize, maxDistinct);
            else if (type == BrightDataType.String)
                buffer = context.CreateHybridStringBuffer(tempStream, bufferSize, maxDistinct);
            else
                buffer = context.CreateHybridObjectBuffer(GetDataType(type), tempStream, bufferSize);

            var segmentType = typeof(HybridBufferSegment<>).MakeGenericType(columnType);
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
        /// <param name="metaData">Table meta data</param>
        /// <param name="context"></param>
        /// <param name="rowCount">Number of rows</param>
        /// <param name="filePath">File path to save on disk (optional)</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable BuildColumnOrientedTable(
            this List<ISingleTypeTableSegment> segments,
            IMetaData metaData,
            IBrightDataContext context,
            uint rowCount,
            string? filePath = null
        ) {
            var columnCount = (uint)segments.Count;
            var columnOffsets = new List<(long Position, long EndOfColumnOffset)>();
            using var builder = new ColumnOrientedTableBuilder(filePath);

            builder.WriteHeader(columnCount, rowCount, metaData);
            foreach (var segment in segments) {
                var position = builder.Write(segment);
                var endPosition = builder.GetCurrentPosition();
                columnOffsets.Add((position, endPosition));
            }
            builder.WriteColumnOffsets(columnOffsets);
            return builder.Build(context);
        }

        /// <summary>
        /// Creates a row oriented data table from a list of segments
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="metaData">Table meta data</param>
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
        public static IColumnInfo ChangeColumnType(this IColumnInfo column, BrightDataType newType)
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
        public static BrightDataType GetColumnType(this IMetaData metadata) => metadata.Get(Consts.Type, BrightDataType.Unknown);

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
        public static InMemoryTableBuilder BuildTable(this IBrightDataContext context) => new(context);

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

        /// <summary>
        /// Folds the data table into k buckets (for k fold cross validation)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="k">Number of buckets to create</param>
        /// <param name="shuffle">True to shuffle the table before folding</param>
        /// <returns></returns>
        public static IEnumerable<(IRowOrientedDataTable Training, IRowOrientedDataTable Validation)> Fold(this IRowOrientedDataTable table, int k, bool shuffle = true)
        {
            var context = table.Context;
            var input = table.RowCount.AsRange();
            if (shuffle)
                input = input.Shuffle(table.Context.Random);
            var rowIndices = input.ToList();
            var foldSize = rowIndices.Count / k;
            var folds = Enumerable.Range(0, k).Select(i => rowIndices.Skip(i * foldSize).Take(foldSize).ToArray()).ToList();

            for (var i = 0; i < k; i++) {
                var i1 = i;
                var trainingRows = Enumerable.Range(0, k).Where(n => n != i1).SelectMany(n => folds[n]).ToArray();
                var validationRows = folds[i1];

                var writer1 = context.BuildTable();
                writer1.CopyColumnsFrom(table);
                foreach (var row in table.Rows(trainingRows))
                    writer1.AddRow(row.ToArray());

                var writer2 = context.BuildTable();
                writer2.CopyColumnsFrom(table);
                foreach (var row in table.Rows(validationRows))
                    writer2.AddRow(row.ToArray());

                yield return (writer1.BuildRowOriented(), writer2.BuildRowOriented());
            }
        }

        interface IHaveFloatArray
        {
            float[] Data { get; }
        }
        class ConvertToFloatColumnReader<T> : IConsumeColumnData<T>, IHaveFloatArray
            where T : struct
        {
            readonly List<float> _data = new();
            readonly ICanConvert<T, float> _converter = StaticConverters.GetConverterToFloat<T>();

            public ConvertToFloatColumnReader(uint columnIndex, BrightDataType type)
            {
                ColumnIndex = columnIndex;
                ColumnType = type;
            }

            public uint ColumnIndex { get; }
            public BrightDataType ColumnType { get; }
            public void Add(T value)
            {
                _data.Add(_converter.Convert(value));
            }

            public float[] Data => _data.ToArray();
        }

        static (IConsumeColumnData Consumer, IHaveFloatArray Array) GetColumnReader(uint columnIndex, BrightDataType columnType)
        {
            if (!columnType.IsNumeric())
                throw new ArgumentException("Column is not numeric");
            var dataType = GetDataType(columnType);
            return GenericActivator.Create<IConsumeColumnData, IHaveFloatArray>(typeof(ConvertToFloatColumnReader<>).MakeGenericType(dataType), columnIndex, columnType);
        }

        /// <summary>
        /// Returns analysed column meta data
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices">Column indices</param>
        /// <returns></returns>
        public static IEnumerable<(uint ColumnIndex, IMetaData MetaData)> ColumnAnalysis(this IDataTable dataTable, params uint[] columnIndices) => dataTable.ColumnAnalysis(AllOrSelectedColumnIndices(dataTable, columnIndices));

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
            if (columnIndices.Length == 1) {
                var columnType = dataTable.ColumnTypes[columnIndices[0]];
                if (columnType == BrightDataType.Vector) {
                    var index = 0;
                    var rows = new Vector<float>[dataTable.RowCount];
                    var vectorSegment = (IDataTableSegment<Vector<float>>)dataTable.Column(columnIndices[0]);
                    foreach (var row in vectorSegment.EnumerateTyped())
                        rows[index++] = row;
                    return dataTable.Context.CreateMatrixFromRows(rows);
                }

                if (columnType.IsNumeric()) {
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
            return Vectorise(dataTable, oneHotEncodeToMultipleColumns, dataTable.ColumnIndices(), filePath);
        }

        /// <summary>
        /// Creates a new data table that has two vector columns, one for the features and the other for the target
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="oneHotEncodeToMultipleColumns"></param>
        /// <param name="columnIndices">Columns to use</param>
        /// <param name="filePath">Optional path to save data table to disk</param>
        /// <returns></returns>
        public static IRowOrientedDataTable Vectorise(this IDataTable dataTable, bool oneHotEncodeToMultipleColumns, IEnumerable<uint> columnIndices, string? filePath = null)
        {
            var target = dataTable.GetTargetColumn();
            var columnIndexList = columnIndices.ToList();
            if(columnIndexList.Count == 0)
                columnIndexList.AddRange(dataTable.ColumnIndices());
            var builder = new RowOrientedTableBuilder(dataTable.MetaData, dataTable.RowCount, filePath);

            // create an optional output vectoriser
            DataTableVectoriser? outputVectoriser = null;
            if (target.HasValue && columnIndexList.Contains((target.Value))) {
                outputVectoriser = new DataTableVectoriser(dataTable, oneHotEncodeToMultipleColumns, target.Value);
                columnIndexList.Remove(target.Value);
            }

            // create the input vectoriser
            var inputVectoriser = new DataTableVectoriser(dataTable, oneHotEncodeToMultipleColumns, columnIndexList.ToArray());
            builder.AddFixedSizeVectorColumn(inputVectoriser.OutputSize, "Features");
            if (outputVectoriser != null)
                builder.AddFixedSizeVectorColumn(outputVectoriser.OutputSize, "Target").SetTarget(true);

            // vectorise each row
            var context = dataTable.Context;
            dataTable.ForEachRow(row => {
                var input = context.CreateVector(inputVectoriser.Vectorise(row));
                if (outputVectoriser != null)
                    builder.AddRow(input, context.CreateVector(outputVectoriser.Vectorise(row)));
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
            builder.AddColumn(BrightDataType.IndexList, "Index");
            builder.AddColumn(BrightDataType.String, "Label").SetTarget(true);

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
            builder.AddColumn(BrightDataType.WeightedIndexList, "Weighted Index");
            builder.AddColumn(BrightDataType.String, "Label").SetTarget(true);

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
            if (preserveVectors) {
                var first = data[0].Data;
                builder.AddFixedSizeVectorColumn(first.Size, "Vector");
                builder.AddColumn(BrightDataType.String, "Label").SetTarget(true);

                foreach (var (label, vector) in data)
                    builder.AddRow(vector, label);
            }

            else {
                var size = data[0].Data.Size;
                for (var i = 1; i <= size; i++)
                    builder.AddColumn(BrightDataType.Float, "Value " + i);
                builder.AddColumn(BrightDataType.String, "Label").SetTarget(true);

                foreach (var (label, vector) in data) {
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
        public static object? GetDefaultValue(this BrightDataType columnType)
        {
            if (columnType == BrightDataType.String)
                return "";
            if (columnType == BrightDataType.Date)
                return DateTime.MinValue;
            if (columnType != BrightDataType.Unknown) {
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
        public static IDataTableVectoriser GetVectoriser(this IDataTable table, bool oneHotEncodeToMultipleColumns = true, params uint[] columnIndices) => new DataTableVectoriser(table, oneHotEncodeToMultipleColumns, columnIndices);

        /// <summary>
        /// Loads a previously created data table vectoriser
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="reader">Reader to load parameters from</param>
        /// <returns></returns>
        public static IDataTableVectoriser LoadVectoriser(this IDataTable dataTable, BinaryReader reader) => new DataTableVectoriser(dataTable, reader);

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
        public static IReinterpretColumns ReinterpretColumns(this uint[] sourceColumnIndices, BrightDataType newColumnType, string newColumnName)
        {
            return new ManyToOneColumn(newColumnType, newColumnName, sourceColumnIndices);
        }

        /// <summary>
        /// Converts the segment to an array
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static object[] ToArray(this IDataTableSegment row)
        {
            var len = row.Size;
            var ret = new object[len];
            for (uint i = 0; i < len; i++)
                ret[i] = row[i];
            return ret;
        }

        /// <summary>
        /// Returns a strongly typed column from the data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnIndex">Column index to retrieve</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDataTableSegment<T> GetColumn<T>(this IColumnOrientedDataTable table, uint columnIndex) where T : notnull
        {
            return (IDataTableSegment<T>)table.Column(columnIndex);
        }

        /// <summary>
        /// Casts the value at column index to type T
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="columnIndex"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>(this IDataTableSegment segment, uint columnIndex) where T : notnull => (T)segment[columnIndex];

        /// <summary>
        /// Samples rows from the data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="sampleSize">Number of rows to sample</param>
        /// <returns></returns>
        public static IEnumerable<IDataTableSegment> Sample(this IRowOrientedDataTable table, uint sampleSize)
        {
            var rows = table.RowCount.AsRange().Shuffle(table.Context.Random).Take((int)sampleSize).OrderBy(i => i).ToArray();
            return table.Rows(rows);
        }

        /// <summary>
        /// Creates a custom column converter
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnIndex">Column index to convert</param>
        /// <param name="converter">Column converter</param>
        /// <param name="columnFinaliser">Called after each row </param>
        /// <typeparam name="TF"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IColumnTransformationParam CreateCustomColumnConverter<TF, TT>(this IColumnOrientedDataTable table, uint columnIndex, Func<TF, TT> converter, Action<IMetaData>? columnFinaliser = null) where TF : notnull where TT : notnull
        {
            var type = table.ColumnTypes[columnIndex].GetDataType();
            if (type != typeof(TF))
                throw new ArgumentException("Invalid from data type");
            if (typeof(TT).GetBrightDataType() == BrightDataType.Unknown)
                throw new ArgumentException("Invalid to data type");

            var transformer = new ColumnConversion.CustomConverter<TF, TT>(converter, columnFinaliser);
            return new ColumnConversion(columnIndex, transformer);
        }

        /// <summary>
        /// Creates a new table with columns that have been converted
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="conversion">Column conversion parameters</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable Convert(this IColumnOrientedDataTable dataTable, params IColumnTransformationParam[] conversion) => Convert(dataTable, null, conversion);

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="conversion">Column normalization parameters</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable Normalize(this IColumnOrientedDataTable dataTable, params IColumnTransformationParam[] conversion) => Normalize(dataTable, null, conversion);

        /// <summary>
        /// Creates a new data table with this concatenated with other column oriented data tables
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="others">Other tables to concatenate</param>
        public static IColumnOrientedDataTable ConcatColumns(this IColumnOrientedDataTable dataTable, params IColumnOrientedDataTable[] others) => dataTable.ConcatColumns(null, others);

        /// <summary>
        /// Copies the selected columns to a new data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices">Column indices to copy</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable CopyColumns(this IColumnOrientedDataTable dataTable, params uint[] columnIndices) => dataTable.CopyColumns(null, columnIndices);

        /// <summary>
        /// Many to one or one to many style column transformations
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columns">Parameters to determine which columns are reinterpreted</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable ReinterpretColumns(this IColumnOrientedDataTable dataTable, params IReinterpretColumns[] columns) => dataTable.ReinterpretColumns(null, columns);

        /// <summary>
        /// Creates a new table of this concatenated with other row oriented data tables
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="others">Other row oriented data tables to concatenate</param>
        /// <returns></returns>
        public static IRowOrientedDataTable Concat(this IRowOrientedDataTable dataTable, params IRowOrientedDataTable[] others) => dataTable.ConcatRows(null, others);

        /// <summary>
        /// Copy specified rows from this to a new data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="rowIndices">Row indices to copy</param>
        /// <returns></returns>
        public static IRowOrientedDataTable CopyRows(this IRowOrientedDataTable dataTable, params uint[] rowIndices) => dataTable.CopyRows(null, rowIndices);

        /// <summary>
        /// Gets column transformers
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="input">Column transformation parameter objects</param>
        /// <returns></returns>
        public static IEnumerable<(uint ColumnIndex, ITransformColumn Transformer)> GetColumnTransformers(this IColumnOrientedDataTable dataTable, IEnumerable<IColumnTransformationParam> input)
        {
            var columnConversionTable = new Dictionary<uint, IColumnTransformationParam>();

            // build the map of columns to transform
            uint nextIndex = 0;
            foreach (var item in input) {
                if (item.ColumnIndex.HasValue && item.ColumnIndex.Value < dataTable.ColumnCount) {
                    columnConversionTable[item.ColumnIndex.Value] = item;
                    nextIndex = item.ColumnIndex.Value + 1;
                }
                else if (nextIndex < dataTable.ColumnCount)
                    columnConversionTable[nextIndex++] = item;
            }

            uint index = 0;
            foreach (var (segment, columnType) in dataTable.Columns().Zip(dataTable.ColumnTypes)) {
                if (columnConversionTable.TryGetValue(index, out var conversion)) {
                    var index1 = index;
                    var converter = conversion.GetTransformer(columnType, segment, () => dataTable.ColumnAnalysis(index1), dataTable.Context.TempStreamProvider);
                    if (converter is not null)
                        yield return (index, converter);
                }
                ++index;
            }
        }

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <param name="conversionParams">Column normalization parameters</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable Normalize(this IColumnOrientedDataTable dataTable, string? filePath, params IColumnTransformationParam[] conversionParams)
        {
            return dataTable.Transform(dataTable.GetColumnTransformers(conversionParams), filePath);
        }

        /// <summary>
        /// Creates a new table with columns that have been converted
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to store new table on disk</param>
        /// <param name="conversionParams">Column conversion parameters</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable Convert(this IColumnOrientedDataTable dataTable, string? filePath, params IColumnTransformationParam[] conversionParams)
        {
            return dataTable.Transform(dataTable.GetColumnTransformers(conversionParams), filePath);
        }

        /// <summary>
        /// Returns column information
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<(uint Index, BrightDataType Type, IMetaData MetaData)> GetColumnInfo(this IDataTable dataTable)
        {
            return dataTable.ColumnTypes.Zip(dataTable.ColumnMetaData())
                .Select((z, i) => ((uint) i, z.First, z.Second));
        }

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="type">Normalization type</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable Normalize(this IColumnOrientedDataTable dataTable, NormalizationType type) => Normalize(dataTable, null, type);

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <param name="type">Normalization type</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable Normalize(this IColumnOrientedDataTable dataTable, string? filePath, NormalizationType type)
        {
            if (type == NormalizationType.None)
                return dataTable;
            var transformers = dataTable.GetColumnTransformers(dataTable.GetColumnInfo()
                .Where(c => !c.MetaData.IsCategorical() && c.Type.IsNumeric())
                .Select(c => new ColumnNormalization(c.Index, type))
            );
            return dataTable.Transform(transformers, filePath);
        }

        /// <summary>
        /// Gets the table signature based on column types
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string GetTableSignature(this IDataTable dataTable)
        {
            var sb = new StringBuilder();
            foreach (var (type, metaData) in dataTable.ColumnTypes.Zip(dataTable.ColumnMetaData())) {
                if (sb.Length > 0)
                    sb.Append('|');
                sb.Append(type);

                if (metaData.Has(Consts.XDimension)) {
                    sb.Append('(');
                    if (metaData.Has(Consts.YDimension)) {
                        if (metaData.Has(Consts.ZDimension)) {
                            sb.Append(metaData.Get<uint>(Consts.ZDimension));
                            sb.Append(", ");
                        }
                        sb.Append(metaData.Get<uint>(Consts.YDimension));
                        sb.Append(", ");
                    }
                    sb.Append(metaData.Get<uint>(Consts.XDimension));
                    sb.Append(')');
                }

                if (metaData.IsTarget())
                    sb.Append('*');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a new table using the same column information as this data table but with modified column data
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="buffers">New column buffers (correctly typed and one for each column)</param>
        /// <param name="filePath">File path to save on disk (optional)</param>
        /// <returns></returns>
        public static IColumnOrientedDataTable CreateWithNewColumnData(this IDataTable dataTable, IHybridBuffer[] buffers, string? filePath = null)
        {
            var segments = new List<ISingleTypeTableSegment>();
            uint rowCount = 0;
            foreach (var (metaData, buffer) in dataTable.ColumnMetaData().Zip(buffers)) {
                var dataType = buffer.DataType;
                var brightDataType = dataType.GetBrightDataType();
                var columnMetaData = new MetaData(metaData, Consts.StandardMetaData);
                columnMetaData.SetType(brightDataType);

                var segmentType = typeof(HybridBufferSegment<>).MakeGenericType(dataType);
                segments.Add(GenericActivator.Create<ISingleTypeTableSegment>(segmentType,
                    brightDataType,
                    columnMetaData,
                    buffer
                ));
                rowCount = buffer.Size;
            }

            return segments.BuildColumnOrientedTable(dataTable.MetaData, dataTable.Context, rowCount, filePath);
        }

        /// <summary>
        /// Creates a column info
        /// </summary>
        /// <param name="columnIndex">Column index</param>
        /// <param name="columnType">Column data type</param>
        /// <param name="metaData">Column meta data (optional)</param>
        /// <returns></returns>
        public static IColumnInfo CreateColumnInfo(uint columnIndex, BrightDataType columnType, IMetaData? metaData = null)
        {
            if (metaData is null) {
                metaData = new MetaData();
                metaData.Set(Consts.Index, columnIndex);
                metaData.Set(Consts.Type, columnType);
            }
            return new ColumnInfo(columnIndex, columnType, metaData);
        }

        /// <summary>
        /// Creates a table segment with an associated hybrid buffer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="columnIndex">Column index</param>
        /// <param name="columnType">Column data type</param>
        /// <param name="metaData">Column meta data (optional)</param>
        /// <returns></returns>
        public static (ISingleTypeTableSegment Segment, IHybridBuffer Buffer) GetSegmentWithHybridBuffer(
            this IBrightDataContext context, 
            uint columnIndex, 
            BrightDataType columnType, 
            IMetaData? metaData = null
        )
        {
            var type = typeof(GrowableDataTableSegment<>).MakeGenericType(columnType.GetDataType());
            var columnInfo = CreateColumnInfo(columnIndex, columnType, metaData);
            return GenericActivator.Create<ISingleTypeTableSegment, IHybridBuffer>(type, context, columnInfo, context.TempStreamProvider);
        }
    }
}
