using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Analysis;
using BrightData.Buffer.Hybrid;
using BrightData.Buffer.InMemory;
using BrightData.Converter;
using BrightData.DataTable;
using BrightData.Helper;
using BrightData.Input;
using BrightData.Transformation;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

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
                BrightDataType.Vector            => typeof(IVectorInfo),
                BrightDataType.Matrix            => typeof(IMatrixInfo),
                BrightDataType.Tensor3D          => typeof(ITensor3DInfo),
                BrightDataType.Tensor4D          => typeof(ITensor4DInfo),
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

            if (dataType == typeof(IVectorInfo))
                return BrightDataType.Vector;

            if (dataType == typeof(IMatrixInfo))
                return BrightDataType.Matrix;

            if (dataType == typeof(ITensor3DInfo))
                return BrightDataType.Tensor3D;

            if (dataType == typeof(ITensor4DInfo))
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
        public static IEnumerable<uint> RowIndices(this BrightDataTable dataTable)
        {
            return dataTable.RowCount.AsRange();
        }

        /// <summary>
        /// Returns all column indices as an enumerable
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<uint> ColumnIndices(this BrightDataTable dataTable)
        {
            return dataTable.ColumnCount.AsRange();
        }

        /// <summary>
        /// Invokes a typed callback on each row of a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <typeparam name="T0"></typeparam>
        public static IEnumerable<T0> ForEachRow<T0>(this BrightDataTable dataTable) => dataTable.GetAllRowData().Select(d => (T0)d.Data[0]);

        /// <summary>
        /// Invokes a typed callback on each row of a data table
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="dataTable"></param>
        public static IEnumerable<(T0, T1)> ForEachRow<T0, T1>(this BrightDataTable dataTable) => dataTable.GetAllRowData().Select(d => ((T0)d.Data[0], (T1)d.Data[1]));

        /// <summary>
        /// Invokes a typed callback on each row of a data table
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dataTable"></param>
        public static IEnumerable<(T0, T1, T2)> ForEachRow<T0, T1, T2>(this BrightDataTable dataTable) => dataTable.GetAllRowData().Select(d => ((T0)d.Data[0], (T1)d.Data[1], (T2)d.Data[2]));

        /// <summary>
        /// Invokes a typed callback on each row of a data table
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="dataTable"></param>
        public static IEnumerable<(T0, T1, T2, T3)> ForEachRow<T0, T1, T2, T3>(this BrightDataTable dataTable) => dataTable.GetAllRowData().Select(d => ((T0)d.Data[0], (T1)d.Data[1], (T2)d.Data[2], (T3)d.Data[3]));

        /// <summary>
        /// Creates a column analyser
        /// </summary>
        /// <param name="type">Column type</param>
        /// <param name="metaData">Column meta data</param>
        /// <param name="writeCount">Maximum size of sequences to write in final meta data</param>
        /// <param name="maxDistinctCount">Maximum number of distinct items to track</param>
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
        public static BrightDataTable ParseCsvIntoMemory(this BrightDataContext context,
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

            var segments = columns.Cast<ISingleTypeTableSegment>().ToArray();
            if (segments.Any(s => s.Size != rowCount))
                throw new Exception("Columns have irregular sizes");

            return context.BuildDataTableInMemory(metaData, segments);
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
        public static BrightDataTable ParseCsv(this BrightDataContext context,
            StreamReader reader,
            bool hasHeader,
            char delimiter = ',',
            string? fileOutputPath = null,
            int maxRows = int.MaxValue,
            uint inMemoryRowCount = 32768,
            ushort maxDistinct = 1024)
        {
            var userNotification = context.UserNotifications;
            var parser = new CsvParser(reader, delimiter);
            using var tempStreams = context.CreateTempStreamProvider();
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
                    Console.WriteLine();
                };
            }

            var ct = context.CancellationToken;
            foreach (var row in parser.Parse().Take(maxRows)) {
                if (ct.IsCancellationRequested)
                    break;

                var cols = row.Count;
                for (var i = columns.Count; i < cols; i++) {
                    var buffer = context.CreateHybridStringBuffer(tempStreams, inMemoryRowCount, maxDistinct);
                    var columnMetaData = new MetaData();
                    columnMetaData.Set(Consts.ColumnIndex, (uint)i);
                    columns.Add(new HybridBufferSegment<string>(BrightDataType.String, columnMetaData, buffer));
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

            var segments = columns.Cast<ISingleTypeTableSegment>().ToArray();
            if (segments.Any(s => s.Size != rowCount))
                throw new Exception("Columns have irregular sizes");

            return context.BuildDataTable(metaData, segments, fileOutputPath);
        }

        /// <summary>
        /// Returns the head (first few rows) of the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="size">Number of rows to return</param>
        /// <returns></returns>
        public static List<object[]> Head(this BrightDataTable dataTable, uint size = 10) => dataTable
            .GetAllRowData(false)
            .Select(d => d.Data)
            .Take((int)size)
            .ToList()
        ;

        /// <summary>
        /// Loads a data table from disk
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath">File path on disk</param>
        /// <returns></returns>
        public static BrightDataTable LoadTable(this BrightDataContext context, string filePath) => new(context, new FileStream(filePath, FileMode.Open, FileAccess.Read));

        /// <summary>
        /// Copies a data table segment to a tensor segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column">Data table segment</param>
        /// <param name="vector">Tensor segment</param>
        /// <returns></returns>
        public static uint CopyToFloatSegment<T>(this IDataTableSegment<T> column, ITensorSegment2 vector)
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
        public static uint CopyTo(this ISingleTypeTableSegment column, ITensorSegment2 vector)
        {
            var type = GetDataType(column.SingleType);
            var copySegment = typeof(ExtensionMethods).GetMethod(nameof(CopyToFloatSegment))!.MakeGenericMethod(type);
            return (uint)copySegment.Invoke(null, new object[] { column, vector })!;
        }

        /// <summary>
        /// Sets the target column of the data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnIndex">Column index to make target (or null to set no target)</param>
        public static void SetTargetColumn(this BrightDataTable table, uint? columnIndex)
        {
            var metaData = table.ColumnMetaData;
            for (int i = 0, len = metaData.Length; i < len; i++)
                metaData[i].Set(Consts.IsTarget, i == columnIndex);
        }

        /// <summary>
        /// Returns the target column of the data table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static uint? GetTargetColumn(this BrightDataTable table)
        {
            var metaData = table.ColumnMetaData;
            for (uint i = 0, len = (uint)metaData.Length; i < len; i++) {
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
        public static uint GetTargetColumnOrThrow(this BrightDataTable table)
        {
            return GetTargetColumn(table) ?? throw new Exception("No target column was set on the table");
        }

        /// <summary>
        /// Returns the feature (non target) columns of the data table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<uint> ColumnIndicesOfFeatures(this BrightDataTable table)
        {
            var targetColumn = table.GetTargetColumn();
            var ret = table.ColumnIndices;
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
        public static IHybridBufferWithMetaData GetHybridBufferWithMetaData(this BrightDataType type, IMetaData metaData, BrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024)
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
            return GenericActivator.Create<IHybridBufferWithMetaData>(segmentType,
                type,
                new MetaData(metaData, Consts.StandardMetaData),
                buffer
            );
        }

        /// <summary>
        /// Converts the data table to a sequence of labeled vectors (feature columns are vectorised, target column is converted to a string)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<(IVector Numeric, string? Label)> GetVectorisedFeatures(this BrightDataTable dataTable)
        {
            var target = dataTable.GetTargetColumn();
            using var vectoriser = new DataTableVectoriser(dataTable, true, dataTable.ColumnIndicesOfFeatures().ToArray());
            if (target.HasValue) {
                using var column = dataTable.GetColumn(target.Value);
                var targetColumn = column.Enumerate().Select(o => o.ToString());
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
        /// Splits a data table into training and test tables (rows are randomly selected for either)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="trainingFraction">Fraction (0..1) of rows to add to the training table</param>
        /// <param name="trainingFilePath">Path to write training table to disk (optional)</param>
        /// <param name="testFilePath">Path to write test table to disk (optional)</param>
        /// <returns></returns>
        public static (BrightDataTable Training, BrightDataTable Test) Split(this BrightDataTable table, double trainingFraction = 0.8, string? trainingFilePath = null, string? testFilePath = null)
        {
            var context = table.Context;
            var (training, test) = table.AllRowIndices.Shuffle(table.Context.Random).ToArray().Split(trainingFraction);
            var trainingOutput = table.WriteRowsTo(GetMemoryOrFileStream(trainingFilePath), training);
            var testOutput = table.WriteRowsTo(GetMemoryOrFileStream(testFilePath), test);
            var results = CompleteInParallel(trainingOutput, testOutput);
            return (context.LoadTableFromStream(results[0]!), context.LoadTableFromStream(results[1]!));
        }

        /// <summary>
        /// Folds the data table into k buckets (for k fold cross validation)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="k">Number of buckets to create</param>
        /// <param name="shuffle">True to shuffle the table before folding</param>
        /// <returns></returns>
        public static IEnumerable<(BrightDataTable Training, BrightDataTable Validation)> Fold(this BrightDataTable table, int k, bool shuffle = true)
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
                foreach (var (_, row) in table.GetAllRowData(true, trainingRows))
                    writer1.AddRow(row);

                var writer2 = context.BuildTable();
                writer2.CopyColumnsFrom(table);
                foreach (var (_, row) in table.GetAllRowData(true, validationRows))
                    writer2.AddRow(row);

                yield return (writer1.BuildInMemory(), writer2.BuildInMemory());
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
        public static (IMatrix Features, IMatrix Target) AsMatrices(this BrightDataTable dataTable)
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
        public static IMatrix AsMatrix(this BrightDataTable dataTable, params uint[] columnIndices)
        {
            // consider the simple case
            if (columnIndices.Length == 1) {
                var columnType = dataTable.ColumnTypes[columnIndices[0]];
                if (columnType == BrightDataType.Vector) {
                    var index = 0;
                    var rows = new IVector[dataTable.RowCount];
                    var vectorSegment = (IDataTableSegment<IVector>)dataTable.GetColumn(columnIndices[0]);
                    foreach (var row in vectorSegment.EnumerateTyped())
                        rows[index++] = row;
                    return dataTable.Context.LinearAlgebraProvider2.CreateMatrixFromRowsAndThenDisposeInput(rows);
                }

                if (columnType.IsNumeric()) {
                    var ret = dataTable.Context.LinearAlgebraProvider2.CreateMatrix(dataTable.RowCount, 1);
                    dataTable.GetColumn(columnIndices[0]).CopyTo(ret.Segment);
                    return ret;
                }
            }

            var vectoriser = new DataTableVectoriser(dataTable, false, columnIndices);
            return dataTable.Context.LinearAlgebraProvider2.CreateMatrixFromRowsAndThenDisposeInput(vectoriser.Enumerate().ToArray());
        }

        /// <summary>
        /// Creates a new data table that has two vector columns, one for the features and the other for the target
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="oneHotEncodeToMultipleColumns"></param>
        /// <param name="filePath">Optional path to save data table to disk</param>
        /// <returns></returns>
        public static BrightDataTable Vectorise(this BrightDataTable dataTable, bool oneHotEncodeToMultipleColumns, string? filePath = null)
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
        public static BrightDataTable Vectorise(this BrightDataTable dataTable, bool oneHotEncodeToMultipleColumns, IEnumerable<uint> columnIndices, string? filePath = null)
        {
            var target = dataTable.GetTargetColumn();
            var columnIndexList = columnIndices.ToList();
            if(columnIndexList.Count == 0)
                columnIndexList.AddRange(dataTable.ColumnIndices());
            var builder = new BrightDataTableBuilder(dataTable.Context);

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
                builder.AddFixedSizeVectorColumn(outputVectoriser.OutputSize, "Target").MetaData.SetTarget(true);

            // vectorise each row
            var context = dataTable.Context;
            var lap = context.LinearAlgebraProvider2;
            foreach(var (_, row) in dataTable.GetAllRowData()) {
                var input = lap.CreateVector(inputVectoriser.Vectorise(row));
                if (outputVectoriser != null)
                    builder.AddRow(input, lap.CreateVector(outputVectoriser.Vectorise(row)));
                else
                    builder.AddRow(input);
            }


            return builder.Build(filePath);
        }

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
        public static BrightDataTable ConvertToTable(this IReadOnlyList<(string Label, IndexList Data)> data, BrightDataContext context)
        {
            var builder = new BrightDataTableBuilder(context);
            builder.AddColumn(BrightDataType.IndexList, "Index");
            builder.AddColumn(BrightDataType.String, "Label").MetaData.SetTarget(true);

            foreach (var (label, indexList) in data)
                builder.AddRow(indexList, label);

            return builder.BuildInMemory();
        }

        /// <summary>
        /// Converts weighted index classifications to a data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        public static BrightDataTable ConvertToTable(this IReadOnlyList<(string Label, WeightedIndexList Data)> data, BrightDataContext context)
        {
            var builder = new BrightDataTableBuilder(context);
            builder.AddColumn(BrightDataType.WeightedIndexList, "Weighted Index");
            builder.AddColumn(BrightDataType.String, "Label").MetaData.SetTarget(true);

            foreach (var (label, weightedIndexList) in data)
                builder.AddRow(weightedIndexList, label);

            return builder.BuildInMemory();
        }

        /// <summary>
        /// Converts the vector classifications into a data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="preserveVectors">True to create a data table with a vector column type, false to to convert to columns of floats</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static BrightDataTable ConvertToTable(this IReadOnlyList<(string Label, IVector Data)> data, bool preserveVectors, BrightDataContext context)
        {
            var builder = new BrightDataTableBuilder(context);
            if (preserveVectors) {
                var first = data[0].Data;
                builder.AddFixedSizeVectorColumn(first.Size, "Vector");
                builder.AddColumn(BrightDataType.String, "Label").MetaData.SetTarget(true);

                foreach (var (label, vector) in data)
                    builder.AddRow(vector, label);
            }

            else {
                var size = data[0].Data.Size;
                for (var i = 1; i <= size; i++)
                    builder.AddColumn(BrightDataType.Float, "Value " + i);
                builder.AddColumn(BrightDataType.String, "Label").MetaData.SetTarget(true);

                foreach (var (label, vector) in data) {
                    var row = new List<object>();
                    for (var i = 0; i < size; i++)
                        row.Add(vector[i]);
                    row.Add(label);
                    builder.AddRow(row);
                }
            }

            return builder.BuildInMemory();
        }

        /// <summary>
        /// Converts the weighted index classification list to a list of dense vectors
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        public static IReadOnlyList<(string Classification, IVector Data)> Vectorise(this IReadOnlyList<(string Label, WeightedIndexList Data)> data, BrightDataContext context)
        {
            var size = data.GetMaxIndex() + 1;
            var lap = context.LinearAlgebraProvider2;
            IVector Create(WeightedIndexList weightedIndexList)
            {
                var ret = new float[size];
                foreach (var item in weightedIndexList.Indices)
                    ret[item.Index] = item.Weight;
                return lap.CreateVector(ret);
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
        public static IDataTableVectoriser GetVectoriser(this BrightDataTable table, bool oneHotEncodeToMultipleColumns = true, params uint[] columnIndices) => new DataTableVectoriser(table, oneHotEncodeToMultipleColumns, columnIndices);

        /// <summary>
        /// Loads a previously created data table vectoriser
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="reader">Reader to load parameters from</param>
        /// <returns></returns>
        public static IDataTableVectoriser LoadVectoriser(this BrightDataTable dataTable, BinaryReader reader) => new DataTableVectoriser(dataTable, reader);

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
        /// <param name="outputColumnIndex"></param>
        /// <returns></returns>
        public static IReinterpretColumns ReinterpretColumns(this uint[] sourceColumnIndices, BrightDataType newColumnType, string newColumnName, uint outputColumnIndex)
        {
            return new ManyToOneColumn(newColumnType, newColumnName, outputColumnIndex, sourceColumnIndices);
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
        public static IEnumerable<IDataTableSegment> Sample(this BrightDataTable table, uint sampleSize)
        {
            var rows = table.RowCount.AsRange().Shuffle(table.Context.Random).Take((int)sampleSize).OrderBy(i => i).ToArray();
            return table.GetRows(rows);
        }

        /// <summary>
        /// Creates a custom column mutator
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnIndex">Column index to convert</param>
        /// <param name="converter">Column converter</param>
        /// <param name="columnFinaliser">Called after each row </param>
        /// <typeparam name="TF"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IColumnTransformationParam CreateCustomColumnMutator<TF, TT>(this BrightDataTable table, uint columnIndex, Func<TF, TT> converter, Action<IMetaData>? columnFinaliser = null) where TF : notnull where TT : notnull
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
        public static BrightDataTable Convert(this BrightDataTable dataTable, params IColumnTransformationParam[] conversion) => MutateColumns(dataTable, null, conversion);

        public static BrightDataTable Convert(this BrightDataTable dataTable, params ColumnConversionType[] conversionTypes) => MutateColumns(dataTable, null, conversionTypes.Select((c, i) => c.ConvertColumn((uint)i)).ToArray());

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="conversion">Column normalization parameters</param>
        /// <returns></returns>
        public static BrightDataTable Normalize(this BrightDataTable dataTable, params IColumnTransformationParam[] conversion) => MutateColumns(dataTable, null, conversion);

        /// <summary>
        /// Creates a new data table with this concatenated with other column oriented data tables
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="others">Other tables to concatenate</param>
        public static BrightDataTable ConcatenateColumns(this BrightDataTable dataTable, params BrightDataTable[] others) => ConcatenateColumns(dataTable, null, others);

        /// <summary>
        /// Creates a new data table with this concatenated with other column oriented data tables
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="others">Other tables to concatenate</param>
        public static BrightDataTable ConcatenateRows(this BrightDataTable dataTable, params BrightDataTable[] others) => ConcatenateRows(dataTable, null, others);

        /// <summary>
        /// Many to one or one to many style column transformations
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columns">Parameters to determine which columns are reinterpreted</param>
        /// <returns></returns>
        public static BrightDataTable ReinterpretColumns(this BrightDataTable dataTable, IProvideTempStreams tempStreams, string? filePath, params IReinterpretColumns[] columns)
        {
            var ops = dataTable.ReinterpretColumns(tempStreams, columns).ToArray();
            var newColumns = EnsureAllCompleted(CompleteInParallel(ops));
            return BuildDataTable(dataTable.Context, dataTable.TableMetaData, newColumns, GetMemoryOrFileStream(filePath));
        }

        public static BrightDataTable ConcatenateColumns(this BrightDataTable table, string? filePath, BrightDataTable[] others)
        {
            var stream = GetMemoryOrFileStream(filePath);
            table.ConcatenateColumns(others, stream);
            return table.Context.LoadTableFromStream(stream);
        }

        public static BrightDataTable ConcatenateRows(this BrightDataTable table, string? filePath, BrightDataTable[] others)
        {
            using var operation = table.ConcatenateRows(others, GetMemoryOrFileStream(filePath));
            var stream = EnsureCompleted(operation.Complete(null, CancellationToken.None));
            return table.Context.LoadTableFromStream(stream);
        }

        /// <summary>
        /// Copy specified rows from this to a new data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath"></param>
        /// <param name="rowIndices">Row indices to copy</param>
        /// <returns></returns>
        public static BrightDataTable CopyRows(this BrightDataTable dataTable, string? filePath, params uint[] rowIndices)
        {
            using var op = dataTable.WriteRowsTo(GetMemoryOrFileStream(filePath), rowIndices);
            var stream = EnsureCompleted(op.Complete(null, CancellationToken.None));
            return dataTable.Context.LoadTableFromStream(stream);
        }

        public static BrightDataTable CopyColumnsToNewTable(this BrightDataTable table, string? filePath, params uint[] columnIndices)
        {
            var stream = GetMemoryOrFileStream(filePath);
            table.WriteColumnsTo(stream, columnIndices);
            return table.Context.LoadTableFromStream(stream);
        }

        /// <summary>
        /// Gets column transformers
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="input">Column transformation parameter objects</param>
        /// <returns></returns>
        public static IEnumerable<(uint ColumnIndex, IConvertColumn Transformer)> GetColumnTransformers(this BrightDataTable dataTable, IProvideTempStreams temp, IEnumerable<IColumnTransformationParam> input)
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

            foreach (var ci in dataTable.ColumnIndices) {
                if (columnConversionTable.TryGetValue(ci, out var conversion)) {
                    var columnIndex = ci;
                    var converter = conversion.GetTransformer(dataTable.Context, dataTable.ColumnTypes[ci], dataTable.GetColumn(ci), () => dataTable.GetColumnAnalysis(columnIndex), temp);
                    if (converter is not null)
                        yield return (ci, converter);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <param name="conversionParams">Column normalization parameters</param>
        /// <returns></returns>
        public static BrightDataTable MutateColumns(this BrightDataTable dataTable, string? filePath, params IColumnTransformationParam[] conversionParams)
        {
            using var tempStream = dataTable.Context.CreateTempStreamProvider();
            var transformers = dataTable.GetColumnTransformers(tempStream, conversionParams);
            var operations = dataTable.MutateColumns(tempStream, transformers);
            var results = EnsureAllCompleted(CompleteInParallel(operations.ToArray()));
            return BuildDataTable(dataTable.Context, dataTable.TableMetaData, results, GetMemoryOrFileStream(filePath));
        }

        /// <summary>
        /// Returns column information
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<(uint Index, BrightDataType Type, MetaData MetaData)> GetColumnInfo(this BrightDataTable dataTable)
        {
            return dataTable.ColumnTypes.Zip(dataTable.ColumnMetaData)
                .Select((z, i) => ((uint) i, z.First, z.Second));
        }

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="type">Normalization type</param>
        /// <returns></returns>
        public static BrightDataTable Normalize(this BrightDataTable dataTable, NormalizationType type) => Normalize(dataTable, null, type);

        /// <summary>
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <param name="type">Normalization type</param>
        /// <returns></returns>
        public static BrightDataTable Normalize(this BrightDataTable dataTable, string? filePath, NormalizationType type)
        {
            if (type == NormalizationType.None)
                return dataTable;

            using var tempStream = dataTable.Context.CreateTempStreamProvider();
            var transformers = GetColumnTransformers(dataTable, tempStream, dataTable.GetColumnInfo()
                .Where(c => !c.MetaData.IsCategorical() && c.Type.IsNumeric())
                .Select(c => new ColumnNormalization(c.Index, type))
            );
            var operations = dataTable.MutateColumns(tempStream, transformers);
            var results = EnsureAllCompleted(CompleteInParallel(operations.ToArray()));
            return BuildDataTable(dataTable.Context, dataTable.TableMetaData, results, GetMemoryOrFileStream(filePath));

        }

        public static BrightDataTable Project(this BrightDataTable dataTable, string? filePath, Func<object[], object[]?> projector)
        {
            using var op = dataTable.Project(projector);
            var builder = EnsureCompleted(op.Complete(null, CancellationToken.None));
            return builder.Build(filePath);
        }

        public static BrightDataTable Bag(this BrightDataTable dataTable, string? filePath, uint sampleCount)
        {
            using var op = dataTable.BagToStream(sampleCount, GetMemoryOrFileStream(filePath));
            var stream = EnsureCompleted(op.Complete(null, CancellationToken.None));
            return dataTable.Context.LoadTableFromStream(stream);
        }

        public static BrightDataTable Shuffle(this BrightDataTable dataTable, string? filePath)
        {
            using var op = dataTable.ShuffleToStream(GetMemoryOrFileStream(filePath));
            var stream = EnsureCompleted(op.Complete(null, CancellationToken.None));
            return dataTable.Context.LoadTableFromStream(stream);
        }

        public static BrightDataTable Clone(this BrightDataTable dataTable, string? filePath)
        {
            var allColumns = dataTable.GetAllColumns().ToArray();
            return BuildDataTable(dataTable.Context, dataTable.TableMetaData, allColumns, filePath);
        }

        public static (string Label, BrightDataTable Table)[] GroupBy(this BrightDataTable dataTable, params uint[] groupByColumnIndices) => GroupBy(dataTable, null, groupByColumnIndices);
        public static (string Label, BrightDataTable Table)[] GroupBy(this BrightDataTable dataTable, Func<string, string?>? filePathProvider, params uint[] groupByColumnIndices)
        {
            var context = dataTable.Context;
            using var tempStreams = context.CreateTempStreamProvider();
            using var op = dataTable.GroupBy(tempStreams, groupByColumnIndices);
            var groups = EnsureCompleted(op.Complete(null, CancellationToken.None));
            var ret = new (string Label, BrightDataTable Table)[groups.Length];

            foreach (var (label, columnData) in groups) {
                var filePath = filePathProvider?.Invoke(label);
                var writer = new BrightDataTableWriter(context, tempStreams, GetMemoryOrFileStream(filePath));
                writer.Write(
                    dataTable.TableMetaData,
                    columnData.Cast<ISingleTypeTableSegment>().ToArray()
                );
            }

            return ret;
        }

        /// <summary>
        /// Gets the table signature based on column types
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string GetTableSignature(this BrightDataTable dataTable)
        {
            var sb = new StringBuilder();
            foreach (var (_, type, metaData) in dataTable.GetColumnInfo()) {
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

        public static BrightDataTable BuildInMemory(this BrightDataTableBuilder builder)
        {
            var stream = new MemoryStream();
            builder.WriteTo(stream);
            return builder.Context.LoadTableFromStream(stream);
        }

        public static BrightDataTable BuildToStream(this BrightDataTableBuilder builder, Stream stream)
        {
            builder.WriteTo(stream);
            return builder.Context.LoadTableFromStream(stream);
        }

        public static BrightDataTable Build(this BrightDataTableBuilder builder, string? filePath) => builder.BuildToStream(GetMemoryOrFileStream(filePath));

        public static T[] CompleteInParallel<T>(params IOperation<T>[] operations) => CompleteInParallel(Array.AsReadOnly(operations));
        public static T[] CompleteInParallel<T>(this IReadOnlyList<IOperation<T>> operations)
        {
            var ret = new T[operations.Count];
            #if DEBUG
            if (Debugger.IsAttached) {
                var index = 0;
                foreach(var op in operations) using (op) {
                    ret[index++] = op.Complete(null, CancellationToken.None);
                }

                return ret;
            }
            #endif
            Parallel.ForEach(operations, (op, _, i) => {
                using(op)
                    ret[i] = op.Complete(null, CancellationToken.None);
            });
            return ret;
        }

        public static T EnsureCompleted<T>(T? result) => result ?? throw new Exception("Operation failed");
        public static T[] EnsureAllCompleted<T>(this IReadOnlyList<T?> results)
        {
            var ret = new T[results.Count];
            for (int i = 0, len = results.Count; i < len; i++)
                ret[i] = EnsureCompleted(results[i]);
            return ret;
        }

        static Stream GetMemoryOrFileStream(string? filePath) => String.IsNullOrWhiteSpace(filePath)
            ? new MemoryStream()
            : new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite)
        ;

        static BrightDataTable LoadTableFromStream(this BrightDataContext context, Stream stream, uint bufferSize = 32768)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return new BrightDataTable(context, stream, bufferSize);
        }

        public static void WriteDataTable(this BrightDataContext context, MetaData tableMetaData, ISingleTypeTableSegment[] columns, Stream stream)
        {
            if (columns.Any()) {
                try {
                    using var tempStream = context.CreateTempStreamProvider();
                    var writer = new BrightDataTableWriter(context, tempStream, stream);
                    writer.Write(tableMetaData, columns);
                }
                finally {
                    foreach (var item in columns)
                        item.Dispose();
                }
            }
        }

        public static BrightDataTable BuildDataTable(this BrightDataContext context, MetaData tableMetaData, ISingleTypeTableSegment[] columns, Stream stream)
        {
            context.WriteDataTable(tableMetaData, columns, stream);
            return context.LoadTableFromStream(stream);
        }

        public static BrightDataTable BuildDataTableInMemory(this BrightDataContext context, MetaData tableMetaData, ISingleTypeTableSegment[] columns) => context
            .BuildDataTable(tableMetaData, columns, new MemoryStream())
        ;

        public static BrightDataTable BuildDataTable(this BrightDataContext context, MetaData tableMetaData, ISingleTypeTableSegment[] columns, string? filePath) => context
            .BuildDataTable(tableMetaData, columns, GetMemoryOrFileStream(filePath))
        ;

        public static IEnumerable<T> MapRows<T>(this BrightDataTable dataTable, Func<BrightDataTableRow, T> mapper)
        {
            foreach (var row in dataTable.GetRows()) {
                yield return mapper(row);
            }
        }

        /// <summary>
        /// Creates a table segment with an associated hybrid buffer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="columnIndex">Column index</param>
        /// <param name="columnType">Column data type</param>
        /// <param name="metaData">Column meta data (optional)</param>
        /// <returns></returns>
        //public static (ISingleTypeTableSegment Segment, IHybridBuffer Buffer) GetSegmentWithHybridBuffer(
        //    this BrightDataContext context, 
        //    uint columnIndex, 
        //    BrightDataType columnType, 
        //    IMetaData? metaData = null
        //)
        //{
        //    var type = typeof(GrowableDataTableSegment<>).MakeGenericType(columnType.GetDataType());
        //    var columnInfo = CreateColumnInfo(columnIndex, columnType, metaData);
        //    return GenericActivator.Create<ISingleTypeTableSegment, IHybridBuffer>(type, context, columnInfo, context.TempStreamProvider);
        //}
    }
}
