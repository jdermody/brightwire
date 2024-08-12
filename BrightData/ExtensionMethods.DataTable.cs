using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Analysis;
using BrightData.Buffer.ByteBlockReaders;
using BrightData.Buffer.Operations;
using BrightData.Buffer.Operations.Vectorisation;
using BrightData.Buffer.ReadOnly.Helper;
using BrightData.Converter;
using BrightData.DataTable;
using BrightData.DataTable.Columns;
using BrightData.DataTable.Helper;
using BrightData.DataTable.Rows;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using BrightData.Types;
using CommunityToolkit.HighPerformance;
using static BrightData.ExtensionMethods;

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
                BrightDataType.Vector            => typeof(ReadOnlyVector<float>),
                BrightDataType.Matrix            => typeof(ReadOnlyMatrix<float>),
                BrightDataType.Tensor3D          => typeof(ReadOnlyTensor3D<float>),
                BrightDataType.Tensor4D          => typeof(ReadOnlyTensor4D<float>),
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

            if (dataType == typeof(DateOnly))
                return BrightDataType.DateOnly;

            if (dataType == typeof(TimeOnly))
                return BrightDataType.TimeOnly;

            if (dataType == typeof(IndexList))
                return BrightDataType.IndexList;

            if (dataType == typeof(WeightedIndexList))
                return BrightDataType.WeightedIndexList;

            if (dataType == typeof(ReadOnlyVector<float>))
                return BrightDataType.Vector;

            if (dataType == typeof(ReadOnlyMatrix<float>))
                return BrightDataType.Matrix;

            if (dataType == typeof(ReadOnlyTensor3D<float>))
                return BrightDataType.Tensor3D;

            if (dataType == typeof(ReadOnlyTensor4D<float>))
                return BrightDataType.Tensor4D;

            if (dataType == typeof(BinaryData))
                return BrightDataType.BinaryData;

            return BrightDataType.Unknown;
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
        /// Creates a column analyser
        /// </summary>
        /// <param name="type">Column type</param>
        /// <param name="metaData">Column meta data</param>
        /// <param name="writeCount">Maximum size of sequences to write in final metadata</param>
        /// <returns></returns>
        public static IDataAnalyser GetAnalyser(this BrightDataType type, MetaData metaData, uint writeCount = Consts.MaxWriteCount)
        {
            var dataType = ColumnTypeClassifier.GetClass(type, metaData);
            if (dataType.HasFlag(ColumnClass.Categorical)) {
                if (type == BrightDataType.String)
                    return StaticAnalysers.CreateStringAnalyser(writeCount);
                return StaticAnalysers.CreateFrequencyAnalyser(type.GetDataType(), writeCount);
            }
            if (dataType.HasFlag(ColumnClass.IndexBased))
                return StaticAnalysers.CreateIndexAnalyser(writeCount);
            if (dataType.HasFlag(ColumnClass.Tensor))
                return StaticAnalysers.CreateDimensionAnalyser();

            return type switch
            {
                BrightDataType.Double     => StaticAnalysers.CreateNumericAnalyser(writeCount),
                BrightDataType.Float      => StaticAnalysers.CreateNumericAnalyser<float>(writeCount),
                BrightDataType.Decimal    => StaticAnalysers.CreateNumericAnalyserCastToDouble<decimal>(writeCount),
                BrightDataType.SByte      => StaticAnalysers.CreateNumericAnalyserCastToDouble<sbyte>(writeCount),
                BrightDataType.Int        => StaticAnalysers.CreateNumericAnalyserCastToDouble<int>(writeCount),
                BrightDataType.Long       => StaticAnalysers.CreateNumericAnalyserCastToDouble<long>(writeCount),
                BrightDataType.Short      => StaticAnalysers.CreateNumericAnalyserCastToDouble<short>(writeCount),
                BrightDataType.Date       => StaticAnalysers.CreateDateAnalyser(),
                BrightDataType.BinaryData => StaticAnalysers.CreateFrequencyAnalyser<BinaryData>(writeCount),
                BrightDataType.DateOnly   => StaticAnalysers.CreateFrequencyAnalyser<DateOnly>(writeCount),
                BrightDataType.TimeOnly   => StaticAnalysers.CreateFrequencyAnalyser<TimeOnly>(writeCount),
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
        /// <param name="fileOutputPath">Path to write data table on disk (optional)</param>
        /// <param name="maxRows">Maximum number of rows to read</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<IDataTable> ParseCsv(this BrightDataContext context,
            StreamReader reader,
            bool hasHeader,
            char delimiter = ',',
            string? fileOutputPath = null,
            uint maxRows = uint.MaxValue,
            CancellationToken ct = default
        )
        {
            var userNotification = context.UserNotifications;
            var parser = new CsvParser(hasHeader, delimiter);

            // write file metadata
            var metaData = new MetaData();
            if (reader.BaseStream is FileStream fs) {
                metaData.Set(Consts.Name, Path.GetFileName(fs.Name));
                metaData.Set(Consts.Source, fs.Name);
            }

            if (userNotification is not null) {
                var operationId = Guid.NewGuid();
                userNotification.OnStartOperation(operationId, "Parsing CSV into memory...");
                parser.OnProgress = p => userNotification.OnOperationProgress(operationId, p);
                parser.OnComplete = () => {
                    userNotification.OnCompleteOperation(operationId, false);
                };
            }

            var buffers = (await parser.Parse(reader, maxRows, ct))?.Cast<IReadOnlyBufferWithMetaData>().ToArray();
            if (buffers is not null) {
                if(fileOutputPath is null)
                    return await context.BuildDataTableInMemory(metaData, buffers);
                return await context.BuildDataTable(metaData, buffers, fileOutputPath);
            }

            throw new Exception("Invalid CSV");
        }

        /// <summary>
        /// Returns the head (first few rows) of the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="size">Number of rows to return</param>
        /// <returns></returns>
        public static Task<GenericTableRow[]> Head(this IDataTable dataTable, uint size = 10) => dataTable
            .EnumerateRows()
            .ToArrayAsync(size)
        ;

        /// <summary>
        /// Loads a data table from disk
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath">File path on disk</param>
        /// <returns></returns>
        public static Task<IDataTable> LoadTableFromFile(this BrightDataContext context, string filePath) => ColumnOrientedDataTable.Load(context, new FileByteBlockReader(filePath));

        /// <summary>
        /// Sets the target column across an array of metadata
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="columnIndex">Column index to make target (or null to set no target)</param>
        public static void SetTargetColumn(this MetaData[] metaData, uint? columnIndex)
        {
            for (int i = 0, len = metaData.Length; i < len; i++)
                metaData[i].Set(Consts.IsTarget, i == columnIndex);
        }

        /// <summary>
        /// Sets the target column of the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndex">Column index to make target (or null to set no target)</param>
        public static void SetTargetColumn(this IDataTable dataTable, uint? columnIndex) => dataTable.ColumnMetaData.SetTargetColumn(columnIndex);

        /// <summary>
        /// Returns the target column of the data table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static uint? GetTargetColumn(this IDataTable table)
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
        /// Sets the column type in a metadata store
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MetaData SetType(this MetaData metaData, BrightDataType type)
        {
            metaData.Set(Consts.Type, (byte)type);
            return metaData;
        }

        /// <summary>
        /// Converts the data table to a sequence of labeled vectors (feature columns are vectorised, target column is converted to a string)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="oneHotEncode"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<(IReadOnlyNumericSegment<float> Numeric, string? Label)> GetVectorisedFeatures(this IDataTable dataTable, bool oneHotEncode)
        {
            var target = dataTable.GetTargetColumn();
            var featureColumnIndices = dataTable.ColumnCount.AsRange();
            if (target.HasValue)
                featureColumnIndices = featureColumnIndices.Where(x => x != target.Value);
            var featureColumns = dataTable.GetColumns(featureColumnIndices);

            var vectoriser = await GetVectoriser(featureColumns, oneHotEncode);
            var targetColumn = target.HasValue ? dataTable.GetColumn(target.Value).ToReadOnlyStringBuffer() : null;
            uint blockIndex = 0;
            await foreach (var blockData in vectoriser.Vectorise(featureColumns)) {
                var blockMemory = new Memory2D<float>(blockData);
                var len = blockMemory.Height;
                var targetBlock = targetColumn is null 
                    ? null 
                    : await targetColumn.GetTypedBlock(blockIndex++);
                for (var i = 0; i < len; i++) {
                    var memory = blockMemory.Span.GetRowSpan(i).ToArray();
                    yield return (new ReadOnlyTensorSegment<float>(memory), targetColumn is null ? null : targetBlock.Span[i]);
                }
            }
        }

        /// <summary>
        /// Gets the column type
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static BrightDataType GetColumnType(this MetaData metadata) => metadata.Get(Consts.Type, BrightDataType.Unknown);

        /// <summary>
        /// Gets the number of distinct items
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static uint GetNumDistinct(this MetaData metadata) => metadata.Get<uint>(Consts.NumDistinct, 0);

        /// <summary>
        /// Splits a data table into training and test tables (rows are randomly selected for either)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="trainingFraction">Fraction (0..1) of rows to add to the training table</param>
        /// <param name="trainingFilePath">Path to write training table to disk (optional)</param>
        /// <param name="testFilePath">Path to write test table to disk (optional)</param>
        /// <returns></returns>
        public static async Task<(IDataTable Training, IDataTable Test)> Split(this IDataTable table, double trainingFraction = 0.8, string? trainingFilePath = null, string? testFilePath = null)
        {
            var context = table.Context;
            var (training, test) = table.AllOrSpecifiedRowIndices(false).Shuffle(table.Context.Random).ToArray().Split(trainingFraction);
            var (trainingStream, testStream) = (GetMemoryOrFileStream(trainingFilePath), GetMemoryOrFileStream(testFilePath));
            await Task.WhenAll(
                table.WriteRowsTo(trainingStream, training), 
                table.WriteRowsTo(testStream, test)
            );
            return (await context.LoadTableFromStream(trainingStream), await context.LoadTableFromStream(testStream));
        }

        /// <summary>
        /// Folds the data table into k buckets (for k fold cross validation)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="k">Number of buckets to create</param>
        /// <param name="shuffle">True to shuffle the table before folding</param>
        /// <returns></returns>
        public static async IAsyncEnumerable<(IDataTable Training, IDataTable Validation)> Fold(this IDataTable table, int k, bool shuffle = true)
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

                var writer1 = context.CreateTableBuilder();
                writer1.CreateColumnsFrom(table);
                foreach (var (_, _, row) in await table.GetRows(trainingRows))
                    writer1.AddRow(row);

                var writer2 = context.CreateTableBuilder();
                writer2.CreateColumnsFrom(table);
                foreach (var (_, _, row) in await table.GetRows(validationRows))
                    writer2.AddRow(row);

                yield return (await writer1.BuildInMemory(), await writer2.BuildInMemory());
            }
        }

        /// <summary>
        /// Converts the data table to feature and target matrices
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static async Task<(ReadOnlyMatrix<float> Features, ReadOnlyMatrix<float> Target)> AsMatrices(this IDataTable dataTable)
        {
            var targetColumn = dataTable.GetTargetColumnOrThrow();
            var featureColumns = dataTable.ColumnIndices().Where(i => i != targetColumn).ToArray();
            return (await AsMatrix(dataTable, featureColumns), await AsMatrix(dataTable, targetColumn));
        }

        /// <summary>
        /// Converts data table columns to a matrix
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices">Column indices to include in the matrix</param>
        /// <returns></returns>
        public static async Task<ReadOnlyMatrix<float>> AsMatrix(this IDataTable dataTable, params uint[] columnIndices)
        {
            if (columnIndices.Length == 0)
                columnIndices = dataTable.ColumnCount.AsRange().ToArray();

            // consider the simple case
            if (columnIndices.Length == 1) {
                var columnIndex = columnIndices[0];
                var columnType = dataTable.ColumnTypes[columnIndex];
                if (columnType.IsTensor()) {
                    var index = 0;
                    var rows = new IReadOnlyVector<float>[dataTable.RowCount];
                    if (columnType == BrightDataType.Vector) {
                        var column = dataTable.GetColumn<ReadOnlyVector<float>>(columnIndex);
                        await foreach (var row in column)
                            rows[index++] = row;
                    }else if (columnType == BrightDataType.Matrix) {
                        var column = dataTable.GetColumn<ReadOnlyMatrix<float>>(columnIndex);
                        await foreach (var row in column)
                            rows[index++] = row.Reshape();
                    }else if (columnType == BrightDataType.Tensor3D) {
                        var column = dataTable.GetColumn<ReadOnlyTensor3D<float>>(columnIndex);
                        await foreach (var row in column)
                            rows[index++] = row.Reshape();
                    }else if (columnType == BrightDataType.Tensor4D) {
                        var column = dataTable.GetColumn<ReadOnlyTensor4D<float>>(columnIndex);
                        await foreach (var row in column)
                            rows[index++] = row.Reshape();
                    }
                    return dataTable.Context.CreateReadOnlyMatrixFromRows(rows);
                }

                if (columnType.IsNumeric()) {
                    var ret = dataTable.Context.CreateReadOnlyMatrix(dataTable.RowCount, 1, 0f);
                    var data = await dataTable.GetColumn(columnIndices[0]).ConvertTo<float>().ToArray();
                    CopyToReadOnly(data, ret.ReadOnlySpan);
                    return ret;
                }
            }

            var columns = columnIndices.Select(dataTable.GetColumn).ToArray();
            var vectoriser = await GetVectoriser(columns, false);
            var size = (int)vectoriser.OutputSize;
            var matrix = dataTable.Context.CreateReadOnlyMatrix(vectoriser.OutputSize, dataTable.RowCount, 0f);
            var rowIndex = 0;
            await vectoriser.ForEachVector(columns, x => {
                CopyToReadOnly(x, matrix.ReadOnlySpan.Slice(rowIndex, size));
                rowIndex += size;
            });
            //TransposeInPlace(matrix);
            return matrix;
        }

        //static unsafe void TransposeInPlace(ReadOnlyMatrix matrix)
        //{
        //    fixed (float* ptr = matrix.ReadOnlySpan) {
        //        new Span<float>(ptr, (int)matrix.Size).TransposeInPlace(matrix.RowCount, matrix.ColumnCount);
        //    }
        //}

        /// <summary>
        /// Copies from a span to a read only span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void CopyToReadOnly<T>(this Span<T> from, ReadOnlySpan<T> to) where T : unmanaged => CopyToReadOnly(from.AsReadOnly(), to);

        /// <summary>
        /// Copies from a span to a read only span
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static unsafe void CopyToReadOnly<T>(this ReadOnlySpan<T> from, ReadOnlySpan<T> to) where T: unmanaged
        {
            fixed (T* ptr = to) {
                from.CopyTo(new Span<T>(ptr, to.Length));
            }
        }

        /// <summary>
        /// Creates a new data table that has two vector columns, one for the features and the other for the target
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="oneHotEncodeToMultipleColumns"></param>
        /// <param name="filePath">Optional path to save data table to disk</param>
        /// <returns></returns>
        public static Task<IDataTable> Vectorise(this IDataTable dataTable, bool oneHotEncodeToMultipleColumns, string? filePath = null)
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
        public static async Task<IDataTable> Vectorise(this IDataTable dataTable, bool oneHotEncodeToMultipleColumns, IEnumerable<uint> columnIndices, string? filePath = null)
        {
            var target = dataTable.GetTargetColumn();
            var columnIndexList = columnIndices.ToList();
            if(columnIndexList.Count == 0)
                columnIndexList.AddRange(dataTable.ColumnIndices());
            var builder = new ColumnOrientedDataTableBuilder(dataTable.Context);

            // create an optional output vectoriser
            VectorisationModel? outputVectoriser = null;
            if (target.HasValue && columnIndexList.Contains((target.Value))) {
                outputVectoriser = await dataTable.GetVectoriser(oneHotEncodeToMultipleColumns, target.Value);
                columnIndexList.Remove(target.Value);
            }

            // create the input vectoriser
            var inputVectoriser = await dataTable.GetVectoriser(oneHotEncodeToMultipleColumns, [.. columnIndexList]);
            builder.CreateFixedSizeVectorColumn(inputVectoriser.OutputSize, "Features");
            if (outputVectoriser != null)
                builder.CreateFixedSizeVectorColumn(outputVectoriser.OutputSize, "Target").MetaData.SetTarget(true);

            // vectorise each row
            foreach(var row in await GetAllRows(dataTable)) {
                var input = inputVectoriser.Vectorise(columnIndexList.Select(x => row.Values[x]).ToArray());
                if (outputVectoriser != null)
                    builder.AddRow(input, outputVectoriser.Vectorise(row.Values[target!.Value]));
                else
                    builder.AddRow(input);
            }

            var stream = GetMemoryOrFileStream(filePath);
            await builder.WriteTo(stream);
            return await LoadTableFromStream(dataTable.Context, stream);
        }

        /// <summary>
        /// Converts indexed classifications to a data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        public static Task<IDataTable> ConvertToTable(this Span<IndexListWithLabel<string>> data, BrightDataContext context)
        {
            var builder = new ColumnOrientedDataTableBuilder(context);
            builder.CreateColumn(BrightDataType.IndexList, "Index");
            builder.CreateColumn(BrightDataType.String, "Label").MetaData.SetTarget(true);

            foreach (var (label, indexList) in data)
                builder.AddRow(indexList, label);

            return builder.BuildInMemory();
        }

        /// <summary>
        /// Converts weighted index classifications to a data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        public static Task<IDataTable> ConvertToTable(this Span<WeightedIndexListWithLabel<string>> data, BrightDataContext context)
        {
            var builder = new ColumnOrientedDataTableBuilder(context);
            builder.CreateColumn(BrightDataType.WeightedIndexList, "Weighted Index");
            builder.CreateColumn(BrightDataType.String, "Label").MetaData.SetTarget(true);

            foreach (var (label, weightedIndexList) in data)
                builder.AddRow(weightedIndexList, label);

            return builder.BuildInMemory();
        }

        /// <summary>
        /// Converts the vector classifications into a data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="preserveVectors">True to create a data table with a vector column type, false to convert to columns of floats</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task<IDataTable> ConvertToTable(this Span<(string Label, IVector<float> Data)> data, bool preserveVectors, BrightDataContext context)
        {
            var builder = new ColumnOrientedDataTableBuilder(context);
            if (preserveVectors) {
                var first = data[0].Data;
                builder.CreateFixedSizeVectorColumn(first.Size, "Vector");
                builder.CreateColumn(BrightDataType.String, "Label").MetaData.SetTarget(true);

                foreach (var (label, vector) in data)
                    builder.AddRow(vector, label);
            }

            else {
                var size = data[0].Data.Size;
                for (var i = 1; i <= size; i++)
                    builder.CreateColumn(BrightDataType.Float, "Value " + i);
                builder.CreateColumn(BrightDataType.String, "Label").MetaData.SetTarget(true);

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
        public static (string Classification, IReadOnlyVector<float> Data, uint Index)[] Vectorise(this Span<WeightedIndexListWithLabel<string>> data, BrightDataContext context)
        {
            var size = data.GetMaxIndex() + 1;
            uint index = 0;
            var ret = new (string Classification, IReadOnlyVector<float> Data, uint Index)[data.Length];

            foreach (ref var item in data)
                ret[index] = (item.Label, Create(item.Data), index++);
            return ret;

            IReadOnlyVector<float> Create(WeightedIndexList weightedIndexList)
            {
                var localRet = new float[size];
                foreach (ref readonly var item in weightedIndexList.ReadOnlySpan)
                    localRet[item.Index] = item.Weight;
                return context.CreateReadOnlyVector(localRet);
            }
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
        /// Converts the segment to an array
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static object[] ToArray(this ICanRandomlyAccessData row)
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
        public static T Get<T>(this ICanRandomlyAccessData segment, uint columnIndex) where T : notnull => (T)segment[columnIndex];

        /// <summary>
        /// Samples rows from the data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="sampleSize">Number of rows to sample</param>
        /// <returns></returns>
        public static Task<GenericTableRow[]> Sample(this IDataTable table, uint sampleSize)
        {
            var rows = table.RowCount.AsRange().Shuffle(table.Context.Random).Take((int)sampleSize).OrderBy(i => i).ToArray();
            return table.GetRows(rows);
        }

        /// <summary>
        /// Horizontally concatenates other data tables with this data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="others">Other data tables</param>
        /// <returns></returns>
        public static async Task<IDataTable> ConcatenateColumns(this IDataTable table, string? filePath, params IDataTable[] others)
        {
            if(others.Any(x => x.RowCount != table.RowCount))
                throw new ArgumentException("Expected all tables to have same row count", nameof(others));

            var builder = new ColumnOrientedDataTableBuilder(table.Context);
            var input = table.GetColumns().ToList();
            builder.CreateColumnsFrom(table);
            foreach (var other in others) {
                input.AddRange(other.GetColumns());
                builder.CreateColumnsFrom(other);
            }
            await builder.AddRows(input);

            await using var stream = GetMemoryOrFileStream(filePath);
            await builder.WriteTo(stream);
            return await LoadTableFromStream(table.Context, stream);
        }

        /// <summary>
        /// Vertically concatenates other data tables with this data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="others">Other data tables</param>
        /// <returns></returns>
        public static async Task<IDataTable> ConcatenateRows(this IDataTable table, string? filePath, params IDataTable[] others)
        {
            var signature = table.GetTableSignature();
            if (others.Any(other => other.GetTableSignature() != signature))
                throw new ArgumentException("Expected all tables to have same signature", nameof(others));

            var builder = new ColumnOrientedDataTableBuilder(table.Context);
            var columns = builder.CreateColumnsFrom(table);
            var operations = CopyToBuffers(table, columns).ToEnumerable().Concat(others.Select(x => CopyToBuffers(x, columns))).ToArray();
            await operations.ExecuteAllAsOne();

            await using var stream = GetMemoryOrFileStream(filePath);
            await builder.WriteTo(stream);
            return await LoadTableFromStream(table.Context, stream);

            static IOperation CopyToBuffers(IDataTable dataTable, ICompositeBuffer[] buffers)
            {
                return new ManyToManyCopy(dataTable.GetColumns(), buffers);
            }
        }

        /// <summary>
        /// Copy specified rows from this to a new data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath"></param>
        /// <param name="rowIndices">Row indices to copy</param>
        /// <returns></returns>
        public static async Task<IDataTable> CopyRowsToNewTable(this IDataTable dataTable, string? filePath, params uint[] rowIndices)
        {
            await using var stream = GetMemoryOrFileStream(filePath);
            await dataTable.WriteRowsTo(stream, rowIndices);
            return await dataTable.Context.LoadTableFromStream(stream);
        }

        /// <summary>
        /// Copies all or specified columns to a new data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="columnIndices">Specified column indices (or all columns if none specified)</param>
        /// <returns></returns>
        public static async Task<IDataTable> CopyColumnsToNewTable(this IDataTable table, string? filePath, params uint[] columnIndices)
        {
            var stream = GetMemoryOrFileStream(filePath);
            await table.WriteColumnsTo(stream, columnIndices);
            return await table.Context.LoadTableFromStream(stream);
        }

        /// <summary>
        /// Returns column information
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<(uint Index, BrightDataType Type, MetaData MetaData)> GetColumnInfo(this IDataTable dataTable)
        {
            return dataTable.ColumnTypes.Zip(dataTable.ColumnMetaData)
                .Select((z, i) => ((uint) i, z.First, z.Second));
        }

        /// <summary>
        /// Normalizes the data in specified columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <param name="type">Normalization type</param>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public static async Task<IDataTable> Normalize(this IDataTable dataTable, NormalizationType type, string? filePath = null, params uint[] columnIndices)
        {
            if (type == NormalizationType.None)
                return dataTable;

            var columnMutationTasks = await Task.WhenAll(dataTable.AllOrSpecifiedColumnIndices(true, columnIndices)
                .Where(x => dataTable.ColumnTypes[x].IsNumeric())
                .Select(async i => (Index: i, Task: await GetNormalization(dataTable.GetColumn(i), type))));
            var columnMutationTable = columnMutationTasks.ToDictionary(x => x.Index, x => x.Task);

            using var tempStream = dataTable.Context.CreateTempDataBlockProvider();
            var writer = new ColumnOrientedDataTableBuilder(dataTable.Context, tempStream);
            var columns = new IReadOnlyBuffer[dataTable.ColumnCount];
            for (uint i = 0; i < dataTable.ColumnCount; i++) {
                var column = dataTable.GetColumn(i);
                var buffer = (IReadOnlyBuffer)column;
                var dataType = column.DataType.GetBrightDataType();

                if (columnMutationTable.TryGetValue(i, out var model)) {
                    // convert integer numeric types to floating point
                    if (dataType.IsNumeric() && dataType.IsInteger()) {
                        buffer = buffer.ConvertTo<double>();
                        dataType = BrightDataType.Double;
                    }
                    columns[i] = GenericTypeMapping.NormalizationConverter(buffer, model);
                }
                else
                    columns[i] = column;
                writer.CreateColumn(dataType, column.MetaData);
            }
            await writer.AddRows(columns);

            // copy normalisation data to new columns
            foreach(var (index, model) in columnMutationTable)
                model.WriteTo(writer.ColumnMetaData[index]);

            await using var stream = GetMemoryOrFileStream(filePath);
            await writer.WriteTo(stream);
            return await LoadTableFromStream(dataTable.Context, stream);
        }

        /// <summary>
        /// Normalizes the data in specified columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnNormalizationTypes">Normalization types per column</param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <returns></returns>
        public static async Task<IDataTable> Normalize(this IDataTable dataTable, string? filePath = null, params NormalizationType[] columnNormalizationTypes)
        {
            var columnMutationTasks = await Task.WhenAll(columnNormalizationTypes
                .Select(async (x, i) => (Index: (uint)i, Task: await GetNormalization(dataTable.GetColumn((uint)i), x))));
            var columnMutationTable = columnMutationTasks.ToDictionary(x => x.Index, x => x.Task);

            using var tempStream = dataTable.Context.CreateTempDataBlockProvider();
            var writer = new ColumnOrientedDataTableBuilder(dataTable.Context, tempStream);
            writer.CreateColumnsFrom(dataTable);
            var columns = new IReadOnlyBuffer[dataTable.ColumnCount];
            for (uint i = 0; i < dataTable.ColumnCount; i++) {
                var column = dataTable.GetColumn(i);
                if (columnMutationTable.TryGetValue(i, out var model))
                    columns[i] = GenericTypeMapping.NormalizationConverter(column, model);
                else
                    columns[i] = column;
            }
            await writer.AddRows(columns);

            await using var stream = GetMemoryOrFileStream(filePath);
            await writer.WriteTo(stream);
            return await LoadTableFromStream(dataTable.Context, stream);
        }

        /// <summary>
        /// Converts the buffer with a column conversion
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="conversion"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <param name="maxDistinctItems"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static async Task<IReadOnlyBufferWithMetaData> Convert(
            this IReadOnlyBufferWithMetaData buffer, 
            ColumnConversion conversion,
            IProvideByteBlocks? tempStreams = null, 
            int blockSize = Consts.DefaultInitialBlockSize, 
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        )
        {
            return conversion switch {
                ColumnConversion.Unchanged           => buffer,
                ColumnConversion.ToBoolean           => await buffer.ToBoolean(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToByte              => await buffer.To<byte>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToCategoricalIndex  => await buffer.ToCategoricalIndex(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToDateTime          => await buffer.ToDateTime(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToDate              => await buffer.ToDate(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToTime              => await buffer.ToTime(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToDecimal           => await buffer.To<decimal>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToDouble            => await buffer.To<double>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToFloat             => await buffer.To<float>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToIndexList         => await buffer.ToIndexList(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToInt               => await buffer.To<int>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToLong              => await buffer.To<long>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToNumeric           => await buffer.ToNumeric(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToString            => await buffer.ToString(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToWeightedIndexList => await buffer.ToWeightedIndexList(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToVector            => await buffer.ToVector(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                ColumnConversion.ToShort             => await buffer.To<short>(tempStreams, blockSize, maxBlockSize, maxInMemoryBlocks, maxDistinctItems),
                _                                    => throw new ArgumentOutOfRangeException(nameof(conversion), conversion, null)
            };
        }

        /// <summary>
        /// Creates a column conversion
        /// </summary>
        /// <param name="conversion"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static ColumnConversionInfo ConvertColumn(this ColumnConversion conversion, uint columnIndex) => new(columnIndex, conversion);

        /// <summary>
        /// Creates a custom column conversion
        /// </summary>
        /// <typeparam name="FT"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <param name="conversion"></param>
        /// <param name="columnIndex"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ColumnConversionInfo ConvertColumn<FT, TT>(this ColumnConversion conversion, uint columnIndex, Func<FT, TT> converter) where FT : notnull where TT : notnull
        {
            if (conversion != ColumnConversion.Custom)
                throw new ArgumentException("Expected custom column conversion");
            return new CustomColumnConversionInfo<FT, TT>(columnIndex, converter);
        }

        /// <summary>
        /// Applies column conversions to the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath"></param>
        /// <param name="conversions"></param>
        /// <returns></returns>
        public static Task<IDataTable> Convert(this IDataTable dataTable, string? filePath, params ColumnConversion[] conversions)
        {
            return dataTable.Convert(filePath, conversions
                .Select((x, i) => x.ConvertColumn((uint)i))
                .ToArray()
            );
        }

        /// <summary>
        /// Applies column conversions to the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath"></param>
        /// <param name="conversions"></param>
        /// <returns></returns>
        public static async Task<IDataTable> Convert(this IDataTable dataTable, string? filePath, params ColumnConversionInfo[] conversions)
        {
            var newColumnTable = conversions
                .Select(x => (Index: x.ColumnIndex, Task: x.Convert(dataTable)))
                .ToDictionary(x => x.Index, x => x.Task);

            using var tempStream = dataTable.Context.CreateTempDataBlockProvider();
            var writer = new ColumnOrientedDataTableBuilder(dataTable.Context, tempStream);
            var columns = new IReadOnlyBufferWithMetaData[dataTable.ColumnCount];
            for (uint i = 0; i < dataTable.ColumnCount; i++) {
                if (newColumnTable.TryGetValue(i, out var newColumn))
                    writer.CreateColumn(columns[i] = await newColumn);
                else {
                    writer.CreateColumn(columns[i] = dataTable.GetColumn(i));
                }
            }
            await writer.AddRows(columns);

            await using var stream = GetMemoryOrFileStream(filePath);
            await writer.WriteTo(stream);
            return await LoadTableFromStream(dataTable.Context, stream);
        }

        /// <summary>
        /// Bags (random sample with duplication) table data to a new table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="sampleCount">Number of rows to sample</param>
        /// <returns></returns>
        public static Task<IDataTable> Bag(this IDataTable dataTable, string? filePath, uint sampleCount)
        {
            var rowIndices = dataTable.RowCount.AsRange().ToArray().Bag(sampleCount, dataTable.Context.Random).ToArray();
            return dataTable.CopyRowsToNewTable(filePath, rowIndices);
        }

        /// <summary>
        /// Shuffles all table rows into a new table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <returns></returns>
        public static Task<IDataTable> Shuffle(this IDataTable dataTable, string? filePath)
        {
            var rowIndices = dataTable.RowCount.AsRange().Shuffle(dataTable.Context.Random).ToArray();
            return dataTable.CopyRowsToNewTable(filePath, rowIndices);
        }

        /// <summary>
        /// Clones the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <returns></returns>
        public static Task<IDataTable> Clone(this IDataTable dataTable, string? filePath)
        {
            var allColumns = dataTable.GetColumns();
            return BuildDataTable(dataTable.Context, dataTable.MetaData, allColumns, filePath);
        }

        /// <summary>
        /// Groups table data into multiple new tables based on the value(s) from columns
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="groupByColumnIndices">Column indices that form the group</param>
        /// <returns></returns>
        public static Task<(string Label, IDataTable Table)[]> GroupBy(this IDataTable dataTable, params uint[] groupByColumnIndices) => GroupBy(dataTable, null, groupByColumnIndices);

        /// <summary>
        /// Groups table data into multiple new tables based on the value(s) from columns
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePathProvider">Provides file paths to save new table</param>
        /// <param name="groupByColumnIndices">Column indices that form the group</param>
        /// <returns></returns>
        public static async Task<(string Label, IDataTable Table)[]> GroupBy(this IDataTable dataTable, Func<string, string?>? filePathProvider, params uint[] groupByColumnIndices)
        {
            var context = dataTable.Context;
            var columns = dataTable.GetColumnsAsBuffers(groupByColumnIndices);
            var groups = await columns.GetGroups();
            var ret = new (string Label, IDataTable Table)[groups.Count];

            uint index = 0;
            foreach (var (label, columnData) in groups) {
                var writer = new ColumnOrientedDataTableBuilder(context);
                var newColumns = writer.CreateColumnsFrom(dataTable);
                var operations = newColumns.Select((x, i) => GenericTypeMapping.IndexedCopyOperation(dataTable.GetColumn((uint)i), x, columnData)).ToArray();
                await operations.ExecuteAllAsOne();
                var outputStream = GetMemoryOrFileStream(filePathProvider?.Invoke(label));
                await writer.WriteTo(outputStream);
                
                outputStream.Seek(0, SeekOrigin.Begin);
                ret[index++] = (label, await LoadTableFromStream(dataTable.Context, outputStream));
            }

            return ret;
        }

        /// <summary>
        /// Gets the table signature based on column types
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string GetTableSignature(this IDataTable dataTable)
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
                            sb.Append(metaData.GetOrThrow<uint>(Consts.ZDimension));
                            sb.Append(", ");
                        }
                        sb.Append(metaData.GetOrThrow<uint>(Consts.YDimension));
                        sb.Append(", ");
                    }
                    sb.Append(metaData.GetOrThrow<uint>(Consts.XDimension));
                    sb.Append(')');
                }

                if (metaData.IsTarget())
                    sb.Append('*');
            }

            return sb.ToString();
        }

        static Stream GetMemoryOrFileStream(string? filePath) => String.IsNullOrWhiteSpace(filePath)
            ? new MemoryStream()
            : new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite)
        ;

        /// <summary>
        /// Writes a data table to a stream
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="tableMetaData">Table meta data</param>
        /// <param name="columns">Data table columns</param>
        /// <param name="stream">Output stream</param>
        public static Task WriteDataTable(this BrightDataContext context, MetaData tableMetaData, IReadOnlyBufferWithMetaData[] columns, Stream stream)
        {
            if (columns.Length > 0) {
                // ensure column indices are correct
                uint columnIndex = 0;
                foreach (var column in columns)
                    column.MetaData.Set(Consts.ColumnIndex, columnIndex++);

                using var tempStream = context.CreateTempDataBlockProvider();
                var writer = new ColumnOrientedDataTableWriter(tempStream);
                return writer.Write(tableMetaData, columns, stream);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Builds a data table from an array of typed segments
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="tableMetaData">Table level meta data</param>
        /// <param name="columns">Typed segments (table columns)</param>
        /// <param name="stream">Output stream</param>
        /// <returns></returns>
        public static async Task<IDataTable> BuildDataTable(this BrightDataContext context, MetaData tableMetaData, IReadOnlyBufferWithMetaData[] columns, Stream stream)
        {
            await context.WriteDataTable(tableMetaData, columns, stream);
            return await context.LoadTableFromStream(stream);
        }

        /// <summary>
        /// Builds a data table in memory
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="tableMetaData">Table level meta data</param>
        /// <param name="columns">Typed segments (table columns)</param>
        /// <returns></returns>
        public static Task<IDataTable> BuildDataTableInMemory(this BrightDataContext context, MetaData tableMetaData, IReadOnlyBufferWithMetaData[] columns) => context
            .BuildDataTable(tableMetaData, columns, new MemoryStream())
        ;

        /// <summary>
        /// Builds a data table (and writes table data to a file)
        /// </summary>
        /// <param name="context">Bright data context</param>
        /// <param name="tableMetaData">Table level meta data</param>
        /// <param name="columns">Typed segments (table columns)</param>
        /// <param name="filePath">File path</param>
        /// <returns></returns>
        public static Task<IDataTable> BuildDataTable(this BrightDataContext context, MetaData tableMetaData, IReadOnlyBufferWithMetaData[] columns, string? filePath) => context
            .BuildDataTable(tableMetaData, columns, GetMemoryOrFileStream(filePath))
        ;

        /// <summary>
        /// Maps table rows to another type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable"></param>
        /// <param name="mapper"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<T[]> MapRows<T>(this IDataTable dataTable, Func<GenericTableRow, T> mapper, CancellationToken ct = default)
        {
            var ret = new T[dataTable.RowCount];
            uint index = 0;
            await foreach (var row in dataTable.EnumerateRows(ct)) {
                ret[index++] = mapper(row);
            }
            return ret;
        }

        /// <summary>
        /// Returns all rows in the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<GenericTableRow[]> GetAllRows(this IDataTable dataTable, CancellationToken ct = default)
        {
            var index = 0;
            var ret = new GenericTableRow[dataTable.RowCount];
            await foreach (var row in dataTable.EnumerateRows(ct))
                ret[index++] = row;
            return ret;
        }

        /// <summary>
        /// Creates a data table in memory
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tableMetaData"></param>
        /// <param name="buffers"></param>
        /// <returns></returns>
        public static async Task<IDataTable> CreateTableInMemory(
            this BrightDataContext context,
            MetaData? tableMetaData = null,
            params IReadOnlyBufferWithMetaData[] buffers
        )
        {
            var ret = new MemoryStream();
            var builder = new ColumnOrientedDataTableWriter();
            await builder.Write(tableMetaData ?? new(), buffers, ret);
            var memory = new Memory<byte>(ret.GetBuffer(), 0, (int)ret.Length);
            return await ColumnOrientedDataTable.Load(context, new MemoryByteBlockReader(memory, ret));
        }

        /// <summary>
        /// Creates a data table saved to disk
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath"></param>
        /// <param name="tableMetaData"></param>
        /// <param name="buffers"></param>
        /// <returns></returns>
        public static async Task<IDataTable> CreateTable(
            this BrightDataContext context,
            string filePath,
            MetaData? tableMetaData = null,
            params IReadOnlyBufferWithMetaData[] buffers
        )
        {
            var builder = new ColumnOrientedDataTableWriter();
            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await builder.Write(tableMetaData ?? new(), buffers, stream);
            return await ColumnOrientedDataTable.Load(context, new FileByteBlockReader(filePath));
        }

        /// <summary>
        /// Loads a data table from the stream
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Task<IDataTable> LoadTableFromStream(this BrightDataContext context, Stream stream)
        {
            if (stream is FileStream fileStream) {
                var path = fileStream.Name;
                fileStream.Close();
                return ColumnOrientedDataTable.Load(context, new FileByteBlockReader(path));
            }

            if(stream is MemoryStream memoryStream)
                return ColumnOrientedDataTable.Load(context, new MemoryByteBlockReader(memoryStream.GetBuffer()));
            return ColumnOrientedDataTable.Load(context, new StreamByteBlockReader(stream));
        }

        /// <summary>
        /// Returns the .net type and its size to represent a bright data type within a column
        /// </summary>
        /// <param name="dataType">Bright data type</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static (Type Type, uint Size) GetColumnType(this BrightDataType dataType) => dataType switch 
        {
            BrightDataType.BinaryData        => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Boolean           => GetTypeAndSize<bool>(),
            BrightDataType.SByte             => GetTypeAndSize<sbyte>(),
            BrightDataType.Short             => GetTypeAndSize<short>(),
            BrightDataType.Int               => GetTypeAndSize<int>(),
            BrightDataType.Long              => GetTypeAndSize<long>(),
            BrightDataType.Float             => GetTypeAndSize<float>(),
            BrightDataType.Double            => GetTypeAndSize<double>(),
            BrightDataType.Decimal           => GetTypeAndSize<decimal>(),
            BrightDataType.String            => GetTypeAndSize<uint>(),
            BrightDataType.Date              => GetTypeAndSize<DateTime>(),
            BrightDataType.IndexList         => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.WeightedIndexList => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Vector            => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Matrix            => GetTypeAndSize<MatrixColumnType>(),
            BrightDataType.Tensor3D          => GetTypeAndSize<Tensor3DColumnType>(),
            BrightDataType.Tensor4D          => GetTypeAndSize<Tensor4DColumnType>(),
            BrightDataType.TimeOnly          => GetTypeAndSize<TimeOnly>(),
            BrightDataType.DateOnly          => GetTypeAndSize<DateOnly>(),
            _                                => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };
        
        /// <summary>
        /// Converts the buffer to a typed buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer<T> ConvertTo<T>(this IReadOnlyBuffer buffer) where T: unmanaged
        {
            if (buffer.DataType == typeof(T))
                return (IReadOnlyBuffer<T>)buffer;
            var converter = StaticConverters.GetConverter(buffer.DataType, typeof(T));
            return (IReadOnlyBuffer<T>)GenericTypeMapping.TypeConverter(typeof(T), buffer, converter);
        }

        /// <summary>
        /// Creates or loads an existing normalisation model from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task<NormalisationModel> GetNormalization(this IReadOnlyBufferWithMetaData buffer, NormalizationType type)
        {
            var metaData = buffer.MetaData;
            if (metaData.Get(Consts.NormalizationType, NormalizationType.None) == type)
                return metaData.GetNormalization();

            if (!buffer.DataType.GetBrightDataType().IsNumeric())
                throw new NotSupportedException("Only numeric buffers can be normalized");

            if (!metaData.Get(Consts.HasBeenAnalysed, false)) {
                var analyzer = StaticAnalysers.CreateNumericAnalyser();
                var toDouble = ConvertTo<double>(buffer);
                await toDouble.ForEachBlock(analyzer.Append);
                analyzer.WriteTo(metaData);
            }

            var ret = new NormalisationModel(type, metaData);
            ret.WriteTo(metaData);
            return ret;
        }

        /// <summary>
        /// Creates a vectoriser for the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="metaData"></param>
        /// <param name="oneHotEncodeCategoricalData"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task<ICanVectorise> GetVectoriser(this IReadOnlyBuffer buffer, MetaData metaData, bool oneHotEncodeCategoricalData)
        {
            var dataType = buffer.DataType.GetBrightDataType();
            var cls = ColumnTypeClassifier.GetClass(dataType, metaData);
            ICanVectorise? ret;

            if (dataType == BrightDataType.Boolean)
                ret = new BooleanVectoriser();
            else if (cls.HasFlag(ColumnClass.Numeric))
                ret = GenericTypeMapping.NumericVectoriser(buffer.DataType);
            else if (cls.HasFlag(ColumnClass.Categorical)) {
                if (oneHotEncodeCategoricalData)
                    ret = await GetOneHotEncoder(buffer, metaData);
                else
                    ret = GenericTypeMapping.CategoricalIndexVectoriser(buffer.DataType);
            }
            else if (cls.HasFlag(ColumnClass.IndexBased))
                ret = await GetIndexBasedVectoriser(buffer, metaData);
            else if (cls.HasFlag(ColumnClass.Tensor))
                ret = await GetTensorVectoriser(buffer, metaData);
            else
                throw new NotImplementedException();
            ret.ReadFrom(metaData);
            return ret;

            static async Task<ICanVectorise> GetIndexBasedVectoriser(IReadOnlyBuffer buffer, MetaData metaData)
            {
                var size = metaData.Get<uint>(Consts.VectorisationSize, 0);
                if (size == 0) {
                    await metaData.Analyse(false, buffer).Execute();
                    size = metaData.GetIndexAnalysis().MaxIndex ?? 0;
                    if (size == 0)
                        throw new Exception("Expected to find a max index size");
                }

                if(buffer.DataType == typeof(IndexList))
                    return GenericActivator.Create<ICanVectorise>(typeof(IndexListVectoriser), size);
                if(buffer.DataType == typeof(WeightedIndexList))
                    return GenericActivator.Create<ICanVectorise>(typeof(WeightedIndexListVectoriser), size);
                throw new NotSupportedException();
            }

            static async Task<ICanVectorise> GetOneHotEncoder(IReadOnlyBuffer buffer, MetaData metaData)
            {
                var size = metaData.Get<uint>(Consts.VectorisationSize, 0);
                if (size == 0) {
                    await metaData.Analyse(false, buffer).Execute();
                    size = metaData.Get<uint>(Consts.NumDistinct, 0);
                    if (size == 0)
                        throw new Exception("Expected to find a distinct size of items");
                }

                return GenericTypeMapping.OneHotVectoriser(buffer.DataType, size);
            }

            static async Task<ICanVectorise> GetTensorVectoriser(IReadOnlyBuffer buffer, MetaData metaData)
            {
                var size = metaData.Get<uint>(Consts.VectorisationSize, 0);
                if (size == 0) {
                    await metaData.Analyse(false, buffer).Execute();
                    size = metaData.GetDimensionAnalysis().Size;
                    if (size == 0)
                        throw new Exception("Expected to find non empty tensors");
                }

                return CreateTensorVectoriser(buffer.DataType, size);
            }
        }

        /// <summary>
        /// Creates a vectoriser for the specified columns in a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="oneHotEncodeCategoricalData"></param>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public static async Task<VectorisationModel> GetVectoriser(this IDataTable dataTable, bool oneHotEncodeCategoricalData, params uint[] columnIndices)
        {
            var actualColumnIndices = dataTable.AllOrSpecifiedColumnIndices(false, columnIndices).ToArray();
            var buffers = actualColumnIndices.Select(dataTable.GetColumn).ToArray();
            var ret = await buffers.GetVectoriser(oneHotEncodeCategoricalData);
            ret.SourceColumnIndices = actualColumnIndices;
            return ret;
        }

        /// <summary>
        /// Creates a vectoriser from multiple buffers
        /// </summary>
        /// <param name="buffers"></param>
        /// <param name="oneHotEncodeCategoricalData"></param>
        /// <returns></returns>
        public static async Task<VectorisationModel> GetVectoriser(this IReadOnlyBufferWithMetaData[] buffers, bool oneHotEncodeCategoricalData)
        {
            var createTasks = buffers.Select(x => GetVectoriser(x, x.MetaData, oneHotEncodeCategoricalData)).ToArray();
            await Task.WhenAll(createTasks);
            var vectorisers = createTasks.Select(x => x.Result).ToArray();
            return new VectorisationModel(vectorisers);
        }

        /// <summary>
        /// Creates a vectoriser from saved metadata
        /// </summary>
        /// <param name="metaData"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static VectorisationModel GetVectoriser(this MetaData[] metaData)
        {
            var index = 0;
            var vectorisers = new ICanVectorise[metaData.Length];

            foreach (var item in metaData) {
                var type = (VectorisationType)item.Get(Consts.VectorisationType, (byte)VectorisationType.Unknown);
                if (type == VectorisationType.Unknown)
                    throw new ArgumentException("Expected meta data to contain a vectorisation type");
                var size = item.Get<uint>(Consts.VectorisationSize, 0);
                if(size == 0)
                    throw new ArgumentException("Expected meta data to contain a vectorisation size");
                var dataType = item.GetColumnType().GetDataType();

                var vectoriser = vectorisers[index++] = type switch {
                    VectorisationType.Tensor            => CreateTensorVectoriser(dataType, size),
                    VectorisationType.WeightedIndexList => new WeightedIndexListVectoriser(size-1),
                    VectorisationType.CategoricalIndex  => GenericTypeMapping.CategoricalIndexVectoriser(dataType),
                    VectorisationType.IndexList         => new IndexListVectoriser(size-1),
                    VectorisationType.Numeric           => GenericTypeMapping.NumericVectoriser(dataType),
                    VectorisationType.OneHot            => GenericTypeMapping.OneHotVectoriser(dataType, size),
                    VectorisationType.Boolean           => new BooleanVectoriser(),
                    _                                   => throw new NotImplementedException()
                };
                vectoriser.ReadFrom(item);
            }

            return new VectorisationModel(vectorisers);
        }

        static ICanVectorise CreateTensorVectoriser(Type type, uint outputSize)
        {
            if (type == typeof(ReadOnlyVector<float>))
                return new TensorVectoriser<ReadOnlyVector<float>>(outputSize);
            if (type == typeof(ReadOnlyMatrix<float>))
                return new TensorVectoriser<ReadOnlyMatrix<float>>(outputSize);
            if (type == typeof(ReadOnlyTensor3D<float>))
                return new TensorVectoriser<ReadOnlyTensor3D<float>>(outputSize);
            if (type == typeof(ReadOnlyTensor4D<float>))
                return new TensorVectoriser<ReadOnlyTensor4D<float>>(outputSize);
            throw new NotImplementedException($"Could not convert to tensor: {type}");
        }

        /// <summary>
        /// Returns all or the specified column indices for the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="distinct"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static IEnumerable<uint> AllOrSpecifiedColumnIndices(this IDataTable dataTable, bool distinct, params uint[] indices) => indices.Length == 0 
            ? dataTable.ColumnCount.AsRange() 
            : distinct 
                ? indices.Order().Distinct()
                : indices
        ;

        /// <summary>
        /// Enumerates specified row indices (or all if none specified)
        /// </summary>
        /// <param name="distinct"></param>
        /// <param name="indices">Row indices (optional)</param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IEnumerable<uint> AllOrSpecifiedRowIndices(this IDataTable dataTable, bool distinct, params uint[] indices) => indices.Length == 0
            ? dataTable.RowCount.AsRange()
            : distinct 
                ? indices.Order().Distinct()
                : indices
        ;

        /// <summary>
        /// Returns the specified columns from the data table (or all if none specified) as buffers
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public static IReadOnlyBuffer[] GetColumnsAsBuffers(this IDataTable dataTable, params uint[] columnIndices)
        {
            if (columnIndices.Length == 0) {
                var ret = new IReadOnlyBuffer[dataTable.ColumnCount];
                for (uint i = 0; i < dataTable.ColumnCount; i++)
                    ret[i] = dataTable.GetColumn(i);
                return ret;
            }
            else {
                var ret = new IReadOnlyBuffer[columnIndices.Length];
                var index = 0;
                foreach (var columnIndex in columnIndices)
                    ret[index++] = dataTable.GetColumn(columnIndex);
                return ret;
            }
        }

        /// <summary>
        /// Returns the type of column from a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static Type GetColumnType(this IDataTable dataTable, uint columnIndex) => dataTable.ColumnTypes[columnIndex].GetColumnType().Type;

        /// <summary>
        /// Creates operations to copy all columns in the table to a destination
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="buffers"></param>
        /// <returns></returns>
        public static IOperation[] CopyTo(this IDataTable dataTable, params IAppendBlocks[] buffers)
        {
            return dataTable.ColumnCount.AsRange()
                .Select(i => dataTable.GetColumn(i).CreateBufferCopyOperation(buffers[i]))
                .ToArray()
            ;
        }

        /// <summary>
        /// Writes the data table to a stream
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task WriteTo(this IDataTable dataTable, Stream stream)
        {
            var builder = new ColumnOrientedDataTableBuilder(dataTable.Context);
            builder.CreateColumnsFrom(dataTable);
            await builder.AddRows(dataTable.GetColumns());
            await builder.WriteTo(stream);
        }

        /// <summary>
        /// Writes the data table to disk
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task WriteTo(this IDataTable dataTable, string path)
        {
            await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            await WriteTo(dataTable, stream);
        }

        /// <summary>
        /// Returns a slice of rows from the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Task<GenericTableRow[]> GetSlice(this IDataTable dataTable, uint start, uint count)
        {
            return dataTable.GetRows(count.AsRange(start).Where(x => x < dataTable.RowCount).ToArray());
        }

        /// <summary>
        /// Creates a new data table from the existing and a projection function that will be applied to each row
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="projection"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<IBuildDataTables> Project(this IDataTable dataTable, Func<GenericTableRow, object[]?> projection, CancellationToken ct = default)
        {
            var builder = dataTable.Context.CreateTableBuilder();
            var isFirst = true;

            await foreach (var row in dataTable.EnumerateRows(ct)) {
                var result = projection(row);
                if(result is null)
                    continue;

                if (isFirst) {
                    foreach (var item in result) {
                        var type = item.GetType().GetBrightDataType();
                        if (type == BrightDataType.Unknown)
                            throw new Exception($"{item.GetType()} cannot be stored in a data table");
                        builder.CreateColumn(type);
                    }
                    isFirst = false;
                }
                builder.AddRow(result);
            }
            return builder;
        }

        /// <summary>
        /// Creates a data table from an array of buffers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="buffers"></param>
        /// <param name="tableMetaData"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<IDataTable> CreateTable<T>(this BrightDataContext context, IReadOnlyBufferWithMetaData<T>[] buffers, MetaData? tableMetaData = null, string? filePath = null) where T : notnull
        {
            var builder = context.CreateTableBuilder();
            builder.CreateColumnsFrom(buffers);
            tableMetaData?.CopyTo(builder.TableMetaData);
            await builder.AddRows(buffers);
            return await builder.Build(filePath);
        }

        /// <summary>
        /// Builds the data table, either writing it to disk if a file path was specified otherwise in memory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<IDataTable> Build(this IBuildDataTables builder, string? filePath)
        {
            await using Stream stream = (filePath is null)
                ? new MemoryStream()
                : new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await builder.WriteTo(stream);
            return await LoadTableFromStream(builder.Context, stream);
        }

        /// <summary>
        /// Builds a data table in memory
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static Task<IDataTable> BuildInMemory(this IBuildDataTables builder) => Build(builder, null);

        /// <summary>
        /// Returns an array of columns (all if none specified)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public static IReadOnlyBufferWithMetaData[] GetColumns(this IDataTable dataTable, params uint[] columnIndices)
        {
            return dataTable.GetColumns(dataTable.AllOrSpecifiedColumnIndices(true, columnIndices));
        }

        /// <summary>
        /// Returns a typed column after applying a conversion function to each value
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="index"></param>
        /// <param name="converter"></param>
        /// <typeparam name="FT"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <returns></returns>
        public static IReadOnlyBufferWithMetaData<TT> GetColumn<FT, TT>(this IDataTable dataTable, uint index, Func<FT, TT> converter) 
            where FT: notnull 
            where TT : notnull
        {
            var from = dataTable.GetColumn<FT>(index);
            var ret = (IReadOnlyBuffer<TT>)GenericTypeMapping.TypeConverter(typeof(TT), from, new CustomConversionFunction<FT, TT>(converter));
            return new ReadOnlyBufferMetaDataWrapper<TT>(ret, dataTable.ColumnMetaData[index]);
        }

        /// <summary>
        /// Returns column analysis of the specified columns (or all if none specified)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public static async Task<MetaData[]> GetColumnAnalysis(this IDataTable dataTable, params uint[] columnIndices)
        {
            var columnMetaData = dataTable.ColumnMetaData;
            if (!AllOrSpecifiedColumnIndices(dataTable, true, columnIndices).All(i => columnMetaData[i].Get(Consts.HasBeenAnalysed, false))) {
                var operations = AllOrSpecifiedColumnIndices(dataTable, true, columnIndices).Select(i => columnMetaData[i].Analyse(false, dataTable.GetColumn(i))).ToArray();
                await operations.ExecuteAllAsOne();
            }
            return AllOrSpecifiedColumnIndices(dataTable, false, columnIndices).Select(x => columnMetaData[x]).ToArray();
        }

        /// <summary>
        /// Enumerates all rows of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<GenericTableRow> EnumerateRows(this IDataTable dataTable, [EnumeratorCancellation] CancellationToken ct)
        {
            var size = dataTable.ColumnCount;
            var enumerators = new IAsyncEnumerator<object>[size];
            var currentTasks = new ValueTask<bool>[size];
            var isValid = true;

            for (uint i = 0; i < size; i++)
                enumerators[i] = dataTable.GetColumn(i).EnumerateAll().GetAsyncEnumerator(ct);

            uint rowIndex = 0;
            while (!ct.IsCancellationRequested && isValid) {
                for (var i = 0; i < size; i++)
                    currentTasks[i] = enumerators[i].MoveNextAsync();
                for (var i = 0; i < size; i++) {
                    if (await currentTasks[i] != true) {
                        isValid = false;
                        break;
                    }
                }

                if (isValid) {
                    var curr = new object[size];
                    for (var i = 0; i < size; i++)
                        curr[i] = enumerators[i].Current;
                    yield return new GenericTableRow(dataTable, rowIndex++, curr);
                }
            }
        }

        /// <summary>
        /// Returns an array of columns
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public static IReadOnlyBufferWithMetaData[] GetColumns(this IDataTable dataTable, IEnumerable<uint> columnIndices)
        {
            return columnIndices.Select(dataTable.GetColumn).ToArray();
        }

        /// <summary>
        /// Writes the specified columns to a stream
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="stream"></param>
        /// <param name="columnIndices"></param>
        /// <returns></returns>
        public static Task WriteColumnsTo(this IDataTable dataTable, Stream stream, params uint[] columnIndices)
        {
            var writer = new ColumnOrientedDataTableWriter();
            return writer.Write(dataTable.MetaData, dataTable.GetColumns(columnIndices), stream);
        }

        /// <summary>
        /// Writes the specified rows to a stream
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="stream"></param>
        /// <param name="rowIndices"></param>
        /// <returns></returns>
        public static async Task WriteRowsTo(this IDataTable dataTable, Stream stream, params uint[] rowIndices)
        {
            var writer = new ColumnOrientedDataTableBuilder(dataTable.Context);
            var newColumns = writer.CreateColumnsFrom(dataTable);
            var wantedRowIndices = rowIndices.Length > 0 ? rowIndices : dataTable.RowCount.AsRange().ToArray();
            var operations = newColumns
                .Select((x, i) => GenericTypeMapping.IndexedCopyOperation(dataTable.GetColumn((uint)i), x, wantedRowIndices))
                .ToArray();
            await operations.ExecuteAllAsOne();
            await writer.WriteTo(stream);
        }

        /// <summary>
        /// Returns the first few rows in the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="rowCount">Number of rows to return</param>
        /// <returns></returns>
        public static GenericTableRow[] GetHead(this IDataTable dataTable, int rowCount = 5) => dataTable.EnumerateRows().ToBlockingEnumerable().Take(rowCount).ToArray();

        /// <summary>
        /// Enumerates the rows in a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<GenericTableRow> EnumerateRows(this IDataTable dataTable)
        {
            var columns = dataTable.GenericColumns;
            var blockSizes = columns[0].BlockSizes;
            var numColumns = columns.Length;
            var tasks = new Task<ReadOnlyMemory<object>>[numColumns];
            var rowIndex = 0U;

            for(uint i = 0, numBlocks = (uint)blockSizes.Length; i < numBlocks; i++) {
                for(var j = 0; j < numColumns; j++)
                    tasks[j] = columns[j].GetTypedBlock(i);
                await Task.WhenAll(tasks);

                for (int k = 0, numRowsInBlock = tasks[0].Result.Length; k < numRowsInBlock; k++) {
                    var rowValues = tasks.Select(x => x.Result.Span[k]).ToArray();
                    yield return new GenericTableRow(dataTable, rowIndex++, rowValues);
                }
            }
        }

        /// <summary>
        /// Returns a generic row from the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="index">Row index</param>
        /// <returns></returns>
        public static Task<GenericTableRow> GetRow(this IDataTable dataTable, uint index)
        {
            var columns = dataTable.GenericColumns;
            var fetchTasks = columns.Select(x => x.GetItem(index)).ToArray();
            return Task.WhenAll(fetchTasks)
                .ContinueWith(_ => new GenericTableRow(dataTable, index, fetchTasks.Select(x => x.Result).ToArray()));
        }

        /// <summary>
        /// Returns an array of rows from the data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="rowIndices">Row indices to return or all if non specified</param>
        /// <returns></returns>
        public static Task<GenericTableRow[]> GetRows(this IDataTable dataTable, params uint[] rowIndices)
        {
            var columns = dataTable.GenericColumns;
            var numColumns = columns.Length;

            return CopyRows(dataTable, columns[0], rowIndices, x => x.Select(y => new GenericTableRow(dataTable, y, new object[numColumns])).ToArray(), async (blockIndex, rowCallback) => {
                var tasks = new Task<ReadOnlyMemory<object>>[numColumns];
                for(var i = 0; i < numColumns; i++)
                    tasks[i] = columns[i].GetTypedBlock(blockIndex);
                await Task.WhenAll(tasks);
                rowCallback((uint _, uint relativeBlockIndex, ref GenericTableRow row) => {
                    var rowValues = row.Values;
                    for (var i = 0; i < numColumns; i++)
                        rowValues[i] = tasks[i].Result.Span[(int)relativeBlockIndex];
                });
            });
        }

        internal delegate void RowCallback<T>(uint absoluteIndex, uint relativeBlockIndex, ref T row);
        static async Task<T[]> CopyRows<T>(this IDataTable dataTable, IReadOnlyBuffer representativeColumn, uint[] rowIndices, Func<uint[], T[]> factory, Func<uint, Action<RowCallback<T>>, Task> copyBlock)
            where T: IHaveSingleIndex
        {
            // take all rows if none specified
            if (rowIndices.Length == 0) {
                Array.Resize(ref rowIndices, (int)dataTable.RowCount);
                for (uint i = 0; i < dataTable.RowCount; i++)
                    rowIndices[i] = i;
            }

            // create output
            var ret = factory(rowIndices);

            // group by block indices
            var blocks = representativeColumn.GetIndices(rowIndices)
                .GroupBy(x => x.BlockIndex)
                .OrderBy(x => x.Key)
                .ToArray()
            ;
            
            var index = 0;
            var blockTasks = new Task[blocks.Length];
            foreach (var block in blocks) {
                blockTasks[index++] = copyBlock(block.Key, cb => {
                    foreach (var item in block) {
                        cb(item.SourceIndex, item.RelativeBlockIndex, ref ret[item.SourceIndex]);
                    }
                });
            }
            await Task.WhenAll(blockTasks);
            return ret;
        }
    }
}
