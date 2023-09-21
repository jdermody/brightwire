using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Analysis;
using BrightData.Buffer.ByteBlockReaders;
using BrightData.Buffer.Composite;
using BrightData.Buffer.ReadOnly.Converter;
using BrightData.Converter;
using BrightData.DataTable;
using BrightData.Helper;
using BrightData.Input;
using BrightData.LinearAlgebra.ReadOnly;
using BrightData.LinearAlgebra.Segments;
using BrightData.Operation;
using BrightData.Operation.Conversion;
using BrightData.Operation.Helper;
using BrightData.Operation.Vectorisation;
using BrightData.Table.Helper;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

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
                BrightDataType.Vector            => typeof(IVectorData),
                BrightDataType.Matrix            => typeof(IMatrixData),
                BrightDataType.Tensor3D          => typeof(ITensor3DData),
                BrightDataType.Tensor4D          => typeof(ITensor4DData),
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

            if (dataType == typeof(IVectorData))
                return BrightDataType.Vector;

            if (dataType == typeof(IMatrixData))
                return BrightDataType.Matrix;

            if (dataType == typeof(ITensor3DData))
                return BrightDataType.Tensor3D;

            if (dataType == typeof(ITensor4DData))
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
        /// <param name="writeCount">Maximum size of sequences to write in final meta data</param>
        /// <returns></returns>
        public static IDataAnalyser GetColumnAnalyser(this BrightDataType type, MetaData metaData, uint writeCount = Consts.MaxWriteCount)
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
                BrightDataType.Decimal    => StaticAnalysers.CreateNumericAnalyser<decimal>(writeCount),
                BrightDataType.SByte      => StaticAnalysers.CreateNumericAnalyser<sbyte>(writeCount),
                BrightDataType.Int        => StaticAnalysers.CreateNumericAnalyser<int>(writeCount),
                BrightDataType.Long       => StaticAnalysers.CreateNumericAnalyser<long>(writeCount),
                BrightDataType.Short      => StaticAnalysers.CreateNumericAnalyser<short>(writeCount),
                BrightDataType.Date       => StaticAnalysers.CreateDateAnalyser(),
                BrightDataType.BinaryData => StaticAnalysers.CreateFrequencyAnalyser<BinaryData>(writeCount),
                BrightDataType.DateOnly => StaticAnalysers.CreateFrequencyAnalyser<DateOnly>(writeCount),
                BrightDataType.TimeOnly => StaticAnalysers.CreateFrequencyAnalyser<TimeOnly>(writeCount),
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
        public static List<object[]> Head(this IDataTable dataTable, uint size = 10) => dataTable
            .EnumerateRows()
            .ToBlockingEnumerable()
            .Select(d => d.Values)
            .Take((int)size)
            .ToList()
        ;

        /// <summary>
        /// Loads a data table from disk
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath">File path on disk</param>
        /// <returns></returns>
        public static IDataTable LoadTable(this BrightDataContext context, string filePath) => new ColumnOrientedDataTable(context, new FileByteBlockReader(filePath));

        /// <summary>
        /// Sets the target column of the data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnIndex">Column index to make target (or null to set no target)</param>
        public static void SetTargetColumn(this IDataTable table, uint? columnIndex)
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
        /// Sets the column type in a meta data store
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
            uint blockIndex = 0, rowSize = vectoriser.OutputSize;
            await foreach (var blockData in vectoriser.Vectorise(featureColumns)) {
                var len = blockData.GetLength(0);
                var block = Unsafe.As<float[]>(blockData).AsMemory();
                var targetBlock = targetColumn is null ? null : await targetColumn.GetTypedBlock(blockIndex++);
                uint offset = 0;
                for (var i = 0; i < len; i++) {
                    yield return (new ReadOnlyMemoryTensorSegment(block.Slice((int)offset, (int)rowSize), offset, rowSize), targetColumn is null ? null : targetBlock.Span[i]);
                    offset += rowSize;
                }
                ++blockIndex;
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
            return (context.LoadTableFromStream(trainingStream), context.LoadTableFromStream(testStream));
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
                writer1.AddColumnsFrom(table);
                foreach (var row in await table.GetRows(trainingRows))
                    writer1.AddRow(row);

                var writer2 = context.CreateTableBuilder();
                writer2.AddColumnsFrom(table);
                foreach (var (_, row) in await table.GetRows(validationRows))
                    writer2.AddRow(row);

                yield return (await writer1.BuildInMemory(), await writer2.BuildInMemory());
            }
        }

        static async Task<IDataTable> BuildInMemory(this IBuildDataTables builder)
        {
            var stream = new MemoryStream();
            await builder.WriteTo(stream);
            return LoadTableFromStream(builder.Context, stream);
        }

        /// <summary>
        /// Converts the data table to feature and target matrices
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static async Task<(ReadOnlyMatrix Features, ReadOnlyMatrix Target)> AsMatrices(this IDataTable dataTable)
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
        public static async Task<ReadOnlyMatrix> AsMatrix(this IDataTable dataTable, params uint[] columnIndices)
        {
            // consider the simple case
            if (columnIndices.Length == 1) {
                var columnIndex = columnIndices[0];
                var columnType = dataTable.ColumnTypes[columnIndex];
                if (columnType.IsTensor()) {
                    var index = 0;
                    var rows = new IReadOnlyVector[dataTable.RowCount];
                    if (columnType == BrightDataType.Vector) {
                        var column = dataTable.GetColumn<ReadOnlyVector>(columnIndex);
                        await foreach (var row in column)
                            rows[index++] = row;
                    }else if (columnType == BrightDataType.Matrix) {
                        var column = dataTable.GetColumn<ReadOnlyMatrix>(columnIndex);
                        await foreach (var row in column)
                            rows[index++] = row.Reshape();
                    }else if (columnType == BrightDataType.Tensor3D) {
                        var column = dataTable.GetColumn<ReadOnlyTensor3D>(columnIndex);
                        await foreach (var row in column)
                            rows[index++] = row.Reshape();
                    }else if (columnType == BrightDataType.Tensor4D) {
                        var column = dataTable.GetColumn<ReadOnlyTensor4D>(columnIndex);
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
            TransposeInPlace(matrix);
            return matrix;
        }

        static unsafe void TransposeInPlace(ReadOnlyMatrix matrix)
        {
            fixed (float* ptr = matrix.ReadOnlySpan) {
                new Span<float>(ptr, (int)matrix.Size).TransposeInPlace(matrix.RowCount, matrix.ColumnCount);
            }
        }

        public static void CopyToReadOnly<T>(this Span<T> from, ReadOnlySpan<T> to) where T : unmanaged => CopyToReadOnly(from.AsReadOnly(), to);
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
            var inputVectoriser = await dataTable.GetVectoriser(oneHotEncodeToMultipleColumns, columnIndexList.ToArray());
            builder.AddFixedSizeVectorColumn(inputVectoriser.OutputSize, "Features");
            if (outputVectoriser != null)
                builder.AddFixedSizeVectorColumn(outputVectoriser.OutputSize, "Target").MetaData.SetTarget(true);

            // vectorise each row
            var context = dataTable.Context;
            foreach(var row in await dataTable.GetAllRows()) {
                var input = inputVectoriser.Vectorise(columnIndexList.Select(x => row.Values[x]).ToArray());
                if (outputVectoriser != null)
                    builder.AddRow(input, outputVectoriser.Vectorise(row.Values[target!.Value]));
                else
                    builder.AddRow(input);
            }

            var stream = GetMemoryOrFileStream(filePath);
            await builder.WriteTo(stream);
            return LoadTableFromStream(dataTable.Context, stream);
        }

        /// <summary>
        /// Converts indexed classifications to a data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        public static Task<IDataTable> ConvertToTable(this Span<IndexListWithLabel<string>> data, BrightDataContext context)
        {
            var builder = new ColumnOrientedDataTableBuilder(context);
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
        public static Task<IDataTable> ConvertToTable(this Span<WeightedIndexListWithLabel<string>> data, BrightDataContext context)
        {
            var builder = new ColumnOrientedDataTableBuilder(context);
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
        public static Task<IDataTable> ConvertToTable(this Span<(string Label, IVector Data)> data, bool preserveVectors, BrightDataContext context)
        {
            var builder = new ColumnOrientedDataTableBuilder(context);
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
        public static (string Classification, IVector Data)[] Vectorise(this Span<WeightedIndexListWithLabel<string>> data, BrightDataContext context)
        {
            var size = data.GetMaxIndex() + 1;
            var lap = context.LinearAlgebraProvider;
            IVector Create(WeightedIndexList weightedIndexList)
            {
                var ret = new float[size];
                foreach (ref readonly var item in weightedIndexList.ReadOnlySpan)
                    ret[item.Index] = item.Weight;
                return lap.CreateVector(ret);
            }

            var index = 0;
            var ret = new (string Classification, IVector Data)[data.Length];
            foreach (ref var item in data)
                ret[index++] = (item.Label, Create(item.Data));
            return ret;
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
        /// Loads a previously created data table vectoriser
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="reader">Reader to load parameters from</param>
        /// <returns></returns>
        public static IDataTableVectoriser LoadVectoriser(this IDataTable dataTable, BinaryReader reader) => new DataTableVectoriser(dataTable, reader);

        /// <summary>
        /// Creates a column conversion parameter
        /// </summary>
        /// <param name="type">Type of column conversion</param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static IColumnTransformationParam ConvertColumn(this ColumnConversionOperation type, uint columnIndex) => new ColumnConversion(columnIndex, type);

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
        public static IEnumerable<BrightDataTableRow> Sample(this IDataTable table, uint sampleSize)
        {
            var rows = table.RowCount.AsRange().Shuffle(table.Context.Random).Take((int)sampleSize).OrderBy(i => i).ToArray();
            return table(rows);
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
        public static IDataTable CreateCustomColumnMutator<TF, TT>(this IDataTable table, uint columnIndex, Func<TF, TT> converter, Action<MetaData>? columnFinaliser = null) where TF : notnull where TT : notnull
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
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="conversion">Column conversion parameters</param>
        /// <returns></returns>
        public static BrightDataTable Convert(this BrightDataTable dataTable, string? filePath, params IColumnTransformationParam[] conversion) => MutateColumns(dataTable, filePath, conversion);

        /// <summary>
        /// Creates a new table with columns that have been converted
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="conversion">Column transformation parameters</param>
        /// <returns></returns>
        public static BrightDataTable Convert(this BrightDataTable dataTable, params IColumnTransformationParam[] conversion) => MutateColumns(dataTable, null, conversion);


        /// <summary>
        /// Creates a new table with columns that have been converted
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="conversionOperations">Array of column conversion operations (one for each column)</param>
        /// <returns></returns>
        public static BrightDataTable Convert(this BrightDataTable dataTable, params ColumnConversionOperation[] conversionOperations) => MutateColumns(dataTable, null, conversionOperations.Select((c, i) => c.ConvertColumn((uint)i)).ToArray());

        /// <summary>
        /// Normalizes the table data per column
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="conversion">Column normalization parameters</param>
        /// <returns></returns>
        public static BrightDataTable Normalize(this BrightDataTable dataTable, params IColumnTransformationParam[] conversion) => MutateColumns(dataTable, null, conversion);


        /// <summary>
        /// Normalizes the table data per column
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="conversion">Column normalization parameters</param>
        /// <returns></returns>
        public static BrightDataTable Normalize(this BrightDataTable dataTable, string? filePath, params IColumnTransformationParam[] conversion) => MutateColumns(dataTable, filePath, conversion);

        /// <summary>
        /// Many to one or one to many style column transformations
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="columns">Parameters to determine which columns are reinterpreted</param>
        /// <param name="tempStreams">Temp stream provider</param>
        /// <returns></returns>
        public static IDataTable ReinterpretColumns(this IDataTable dataTable, IProvideTempStreams tempStreams, string? filePath, params IReinterpretColumns[] columns)
        {
            var ops = dataTable.ReinterpretColumns(tempStreams, columns).ToArray();
            var newColumns = EnsureAllCompleted(CompleteInParallel(ops));
            return BuildDataTable(dataTable.Context, dataTable.TableMetaData, newColumns, GetMemoryOrFileStream(filePath));
        }

        /// <summary>
        /// Horizontally concatenates other data tables with this data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="others">Other data tables</param>
        /// <returns></returns>
        public static IDataTable ConcatenateColumns(this IDataTable table, string? filePath, params IDataTable[] others)
        {
            var stream = GetMemoryOrFileStream(filePath);
            table.ConcatenateColumns(stream, others);
            return table.Context.LoadTableFromStream(stream);
        }

        /// <summary>
        /// Vertically concatenates other data tables with this data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="others">Other data tables</param>
        /// <returns></returns>
        public static IDataTable ConcatenateRows(this IDataTable table, string? filePath, params IDataTable[] others)
        {
            using var operation = table.ConcatenateRows(GetMemoryOrFileStream(filePath), others);
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
        public static IDataTable CopyRowsToNewTable(this IDataTable dataTable, string? filePath, params uint[] rowIndices)
        {
            using var op = dataTable.WriteRowsTo(GetMemoryOrFileStream(filePath), rowIndices);
            var stream = EnsureCompleted(op.Complete(null, CancellationToken.None));
            return dataTable.Context.LoadTableFromStream(stream);
        }

        /// <summary>
        /// Copies all or specified columns to a new data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="columnIndices">Specified column indices (or all columns if none specified)</param>
        /// <returns></returns>
        public static IDataTable CopyColumnsToNewTable(this IDataTable table, string? filePath, params uint[] columnIndices)
        {
            var stream = GetMemoryOrFileStream(filePath);
            table.WriteColumnsTo(stream, columnIndices);
            return table.Context.LoadTableFromStream(stream);
        }

        /// <summary>
        /// Gets column transformers
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="temp">Temp stream provider</param>
        /// <param name="input">Column transformation parameter objects</param>
        /// <returns></returns>
        public static IEnumerable<(uint ColumnIndex, IConvertColumn Transformer)> GetColumnTransformers(this IDataTable dataTable, IProvideTempData? temp, IEnumerable<IColumnTransformationParam> input)
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
        /// Mutates table columns
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="conversionParams">Column transformation parameters</param>
        /// <returns></returns>
        public static IDataTable MutateColumns(this IDataTable dataTable, string? filePath, params IColumnTransformationParam[] conversionParams)
        {
            using var tempStream = dataTable.Context.CreateTempStreamProvider();
            var transformers = dataTable.GetColumnTransformers(tempStream, conversionParams);
            var operations = dataTable.ConvertColumns(tempStream, transformers);
            var results = EnsureAllCompleted(CompleteInParallel(operations.ToArray()));
            return BuildDataTable(dataTable.Context, dataTable.TableMetaData, results, GetMemoryOrFileStream(filePath));
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
        /// Normalizes the data in all columns of the table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to store new table on disk (optional)</param>
        /// <param name="type">Normalization type</param>
        /// <returns></returns>
        public static IDataTable Normalize(this IDataTable dataTable, NormalizationType type, string? filePath = null)
        {
            if (type == NormalizationType.None)
                return dataTable;

            using var tempStream = dataTable.Context.CreateTempFileProvider();
            var transformers = GetColumnTransformers(dataTable, tempStream, dataTable.GetColumnInfo()
                .Where(c => !c.MetaData.IsCategorical() && c.Type.IsNumeric())
                .Select(c => new ColumnNormalization(c.Index, type))
            );
            var operations = dataTable.ConvertColumns(tempStream, transformers);
            var results = EnsureAllCompleted(CompleteInParallel(operations.ToArray()));
            return BuildDataTable(dataTable.Context, dataTable.TableMetaData, results, GetMemoryOrFileStream(filePath));

        }

        /// <summary>
        /// Projects (transforms) table data to a new table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="projector">Projection function</param>
        /// <returns></returns>
        public static IDataTable Project(this IDataTable dataTable, string? filePath, Func<object[], object[]?> projector)
        {
            using var op = dataTable.Project(projector);
            var builder = EnsureCompleted(op.Complete(null, CancellationToken.None));
            return builder.Build(filePath);
        }

        /// <summary>
        /// Bags (random sample with duplication) table data to a new table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <param name="sampleCount">Number of rows to sample</param>
        /// <returns></returns>
        public static IDataTable Bag(this IDataTable dataTable, string? filePath, uint sampleCount)
        {
            using var op = dataTable.BagToStream(sampleCount, GetMemoryOrFileStream(filePath));
            var stream = EnsureCompleted(op.Complete(null, CancellationToken.None));
            return dataTable.Context.LoadTableFromStream(stream);
        }

        /// <summary>
        /// Shuffles all table rows into a new table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="filePath">File path to save new table (optional)</param>
        /// <returns></returns>
        public static IDataTable Shuffle(this IDataTable dataTable, string? filePath)
        {
            using var op = dataTable.ShuffleToStream(GetMemoryOrFileStream(filePath));
            var stream = EnsureCompleted(op.Complete(null, CancellationToken.None));
            return dataTable.Context.LoadTableFromStream(stream);
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
                var newColumns = writer.AddColumnsFrom(dataTable);
                var operations = newColumns.Select((x, i) => GenericActivator.Create<IOperation>(typeof(IndexedCopyOperation<>).MakeGenericType(x.DataType), columns[i], x, columnData));
                await operations.Process();
                var outputStream = GetMemoryOrFileStream(filePathProvider?.Invoke(label));
                await writer.WriteTo(outputStream);
                
                outputStream.Seek(0, SeekOrigin.Begin);
                ret[index++] = (label, LoadTableFromStream(dataTable.Context, outputStream));
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

        public static async Task Process(this IEnumerable<IOperation> operations, INotifyUser? notifyUser = null, CancellationToken ct = default)
        {
            await Task.WhenAll(operations.Select(operation => operation.Process(notifyUser, null, ct)));
        }

        /// <summary>
        /// Returns the results from a collection of operations that might be run in parallel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operations"></param>
        /// <returns></returns>
        public static T[] CompleteInParallel<T>(params IOperation<T>[] operations) => CompleteInParallel(Array.AsReadOnly(operations));

        /// <summary>
        /// Returns the results from a collection of operations that might be run in parallel
        /// </summary>
        /// <param name="operations"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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
            if (columns.Any()) {
                // ensure column indices are correct
                uint columnIndex = 0;
                foreach (var column in columns)
                    column.MetaData.Set(Consts.ColumnIndex, columnIndex++);

                using var tempStream = context.CreateTempFileProvider();
                var writer = new ColumnOrientedDataTableWriter(context, tempStream);
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
            return context.LoadTableFromStream(stream);
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
        /// <returns></returns>
        public static async Task<T[]> MapRows<T>(this IDataTable dataTable, Func<TableRow, T> mapper, CancellationToken ct = default)
        {
            var ret = new T[dataTable.RowCount];
            uint index = 0;
            await foreach (var row in dataTable.EnumerateRows().WithCancellation(ct)) {
                ret[index++] = mapper(row);
            }
            return ret;
        }

        public static async Task<TableRow[]> GetAllRows(this IDataTable dataTable, CancellationToken ct = default)
        {
            var index = 0;
            var ret = new TableRow[dataTable.RowCount];
            await foreach (var row in dataTable.EnumerateRows(ct))
                ret[index++] = row;
            return ret;
        }

        public static async Task<IDataTable> CreateTableInMemory(
            this BrightDataContext context,
            IReadOnlyBufferWithMetaData[] buffers,
            MetaData? tableMetaData = null
        )
        {
            var ret = new MemoryStream();
            var builder = new ColumnOrientedDataTableWriter(context);
            await builder.Write(tableMetaData ?? new(), buffers, ret);
            var memory = new Memory<byte>(ret.GetBuffer(), 0, (int)ret.Length);
            return new ColumnOrientedDataTable(context, new MemoryByteBlockReader(memory, ret));
        }

        public static async Task<IDataTable> CreateTable(
            this BrightDataContext context,
            IReadOnlyBufferWithMetaData[] buffers,
            string filePath,
            MetaData? tableMetaData = null
        )
        {
            {
                var builder = new ColumnOrientedDataTableWriter(context);
                await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await builder.Write(tableMetaData ?? new(), buffers, stream);
            }
            return new ColumnOrientedDataTable(context, new FileByteBlockReader(filePath));
        }

        public static IDataTable LoadTableFromStream(this BrightDataContext context, Stream stream)
        {
            if (stream is FileStream fileStream)
                return new ColumnOrientedDataTable(context, new FileByteBlockReader(fileStream.Name));
            if(stream is MemoryStream memoryStream)
                return new ColumnOrientedDataTable(context, new MemoryByteBlockReader(memoryStream.GetBuffer()));
            return new ColumnOrientedDataTable(context, new StreamByteBlockReader(stream));
        }

        public static ICompositeBuffer<string> CreateCompositeBuffer(
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => new StringCompositeBuffer(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer<T> CreateCompositeBuffer<T>(
            CreateFromReadOnlyByteSpan<T> createItem,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) where T: IHaveDataAsReadOnlyByteSpan => new ManagedCompositeBuffer<T>(createItem, tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer<T> CreateCompositeBuffer<T>(
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) where T: unmanaged => new UnmanagedCompositeBuffer<T>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer CreateCompositeBuffer(
            this BrightDataType dataType,
            IProvideTempData? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null)
        {
            return dataType switch {
                BrightDataType.BinaryData => CreateCompositeBuffer<BinaryData>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Boolean => CreateCompositeBuffer<bool>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Date => CreateCompositeBuffer<DateTime>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.DateOnly => CreateCompositeBuffer<DateOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Decimal => CreateCompositeBuffer<DateOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.SByte => CreateCompositeBuffer<byte>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Short => CreateCompositeBuffer<short>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Int => CreateCompositeBuffer<int>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Long => CreateCompositeBuffer<long>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Float => CreateCompositeBuffer<float>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Double => CreateCompositeBuffer<double>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.String => CreateCompositeBuffer(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.IndexList => CreateCompositeBuffer<IndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.WeightedIndexList => CreateCompositeBuffer<WeightedIndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Vector => CreateCompositeBuffer<ReadOnlyVector>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Matrix => CreateCompositeBuffer<ReadOnlyMatrix>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Tensor3D => CreateCompositeBuffer<ReadOnlyTensor3D>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.Tensor4D => CreateCompositeBuffer<ReadOnlyTensor4D>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                BrightDataType.TimeOnly => CreateCompositeBuffer<TimeOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, $"Not able to create a composite buffer for type: {dataType}")
            };
        }

        public static IBufferWriter<T> AsBufferWriter<T>(this ICompositeBuffer<T> buffer, int bufferSize = 256) where T : notnull => new CompositeBufferWriter<T>(buffer, bufferSize);

        public static bool CanEncode<T>(this ICompositeBuffer<T> buffer) where T : notnull => buffer.DistinctItems.HasValue;

        /// <summary>
        /// Encoding a composite buffer maps each item to an index and returns both the mapping table and a new composite buffer of the indices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="tempStreams"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxInMemoryBlocks"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static (T[] Table, ICompositeBuffer<uint> Data) Encode<T>(
            this ICompositeBuffer<T> buffer, 
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null
        ) where T : notnull {
            if(!buffer.DistinctItems.HasValue)
                throw new ArgumentException("Buffer cannot be encoded as the number of distinct items is not known - create the composite buffer with a high max distinct items", nameof(buffer));
            var table = new Dictionary<T, uint>((int)buffer.DistinctItems.Value);
            var data = new UnmanagedCompositeBuffer<uint>(tempStreams, blockSize, maxInMemoryBlocks);

            buffer.ForEachBlock(block => {
                var len = block.Length;
                if (len == 1) {
                    var item = block[0];
                    if(!table.TryGetValue(item, out var index))
                        table.Add(item, index = (uint)table.Count);
                    data.Add(index);
                }
                else if (len > 1) {
                    var spanOwner = SpanOwner<uint>.Empty;
                    var indices = len <= Consts.MaxStackAllocSize / sizeof(uint)
                        ? stackalloc uint[len]
                        : (spanOwner = SpanOwner<uint>.Allocate(len)).Span
                    ;
                    try {
                        // encode the block
                        for(var i = 0; i < len; i++) {
                            ref readonly var item = ref block[i];
                            if(!table.TryGetValue(item, out var index))
                                table.Add(item, index = (uint)table.Count);
                            indices[i] = index;
                        }
                        data.Add(indices);
                    }
                    finally {
                        if (spanOwner.Length > 0)
                            spanOwner.Dispose();
                    }
                }
            });

            var ret = new T[table.Count];
            foreach (var item in table)
                ret[item.Value] = item.Key;
            return (ret, data);
        }

        public static async Task<T[]> ToArray<T>(this IReadOnlyBuffer<T> buffer) where T : notnull
        {
            var ret = new T[buffer.Size];
            var offset = 0;
            await buffer.ForEachBlock(x => {
                x.CopyTo(new Span<T>(ret, offset, x.Length));
                offset += x.Length;
            });
            return ret;
        }

        public ref struct ReadOnlyBufferIterator<T> where T: notnull
        {
            readonly IReadOnlyBuffer<T> _buffer;
            ReadOnlyMemory<T> _currentBlock = ReadOnlyMemory<T>.Empty;
            uint _blockIndex = 0, _position = 0;

            public ReadOnlyBufferIterator(IReadOnlyBuffer<T> buffer) => _buffer = buffer;

            public bool MoveNext()
            {
                if (++_position < _currentBlock.Length)
                    return true;

                while(_blockIndex < _buffer.BlockCount) {
                    _currentBlock = _buffer.GetTypedBlock(_blockIndex++).Result;
                    if (_currentBlock.Length > 0) {
                        _position = 0;
                        return true;
                    }
                }
                return false;
            }

            public readonly ref readonly T Current => ref _currentBlock.Span[(int)_position];
            public readonly ReadOnlyBufferIterator<T> GetEnumerator() => this;
        }
        public static ReadOnlyBufferIterator<T> GetEnumerator<T>(this IReadOnlyBuffer<T> buffer) where T: notnull => new(buffer);

        static (Type, uint) GetTypeAndSize<T>() => (typeof(T), (uint)Unsafe.SizeOf<T>());

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
        /// Converts from a Type to a ColumnType
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static BrightDataType GetTableDataType(this Type dataType)
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

            if (dataType == typeof(IVectorData))
                return BrightDataType.Vector;

            if (dataType == typeof(IMatrixData))
                return BrightDataType.Matrix;

            if (dataType == typeof(ITensor3DData))
                return BrightDataType.Tensor3D;

            if (dataType == typeof(ITensor4DData))
                return BrightDataType.Tensor4D;

            if (dataType == typeof(BinaryData))
                return BrightDataType.BinaryData;

            throw new ArgumentException($"{dataType} has no corresponding table data type");
        }

        public static ICompositeBuffer GetCompositeBuffer(this Type type,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => GetCompositeBuffer(GetTableDataType(type), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);

        public static ICompositeBuffer GetCompositeBuffer(this BrightDataType type,
            IProvideTempData? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) => type switch 
        {
            BrightDataType.BinaryData        => CreateCompositeBuffer<BinaryData>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Boolean           => CreateCompositeBuffer<bool>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.SByte             => CreateCompositeBuffer<sbyte>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Short             => CreateCompositeBuffer<short>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Int               => CreateCompositeBuffer<int>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Long              => CreateCompositeBuffer<long>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Float             => CreateCompositeBuffer<float>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Double            => CreateCompositeBuffer<double>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Decimal           => CreateCompositeBuffer<decimal>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.String            => CreateCompositeBuffer<uint>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Date              => CreateCompositeBuffer<DateTime>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.IndexList         => CreateCompositeBuffer<IndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.WeightedIndexList => CreateCompositeBuffer<WeightedIndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Vector            => CreateCompositeBuffer<ReadOnlyVector>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Matrix            => CreateCompositeBuffer<ReadOnlyMatrix>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Tensor3D          => CreateCompositeBuffer<ReadOnlyTensor3D>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.Tensor4D          => CreateCompositeBuffer<ReadOnlyTensor4D>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.TimeOnly          => CreateCompositeBuffer<TimeOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            BrightDataType.DateOnly          => CreateCompositeBuffer<DateOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems),
            _                                => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown table data type")
        };

        public static async Task<T> GetItem<T>(this IReadOnlyBuffer<T> buffer, uint index) where T: notnull
        {
            var blockIndex = index / buffer.BlockSize;
            var blockMemory = await buffer.GetTypedBlock(blockIndex);
            var ret = blockMemory.Span[(int)(index % buffer.BlockSize)];
            return ret;
        }

        public static async Task<T[]> GetItems<T>(this IReadOnlyBuffer<T> buffer, uint[] indices) where T: notnull
        {
            var blocks = indices.Select(x => (Index: x, BlockIndex: x / buffer.BlockSize, RelativeIndex: x % buffer.BlockSize))
                .GroupBy(x => x.BlockIndex)
                .OrderBy(x => x.Key)
            ;
            var ret = new T[indices.Length];
            foreach (var block in blocks) {
                var blockMemory = await buffer.GetTypedBlock(block.Key);
                Add(blockMemory, block, ret);
            }
            return ret;

            static void Add(ReadOnlyMemory<T> data, IEnumerable<(uint Index, uint BlockIndex, uint RelativeIndex)> list, T[] output)
            {
                var span = data.Span;
                foreach (var (index, _, relativeIndex) in list)
                    output[relativeIndex] = span[(int)index];
            }
        }

        class MemorySegment<T> : ReadOnlySequenceSegment<T>
        {
            public MemorySegment(ReadOnlyMemory<T> memory) => Memory = memory;
            public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
            {
                var segment = new MemorySegment<T>(memory) {
                    RunningIndex = RunningIndex + Memory.Length
                };
                Next = segment;
                return segment;
            }
        }
        public static async Task<ReadOnlySequence<T>> AsReadOnlySequence<T>(this IReadOnlyBuffer<T> buffer) where T : notnull
        {
            if(buffer.BlockCount == 0)
                return ReadOnlySequence<T>.Empty;

            var first = new MemorySegment<T>(await buffer.GetTypedBlock(0));
            var last = first;
            for(var i = 1; i < buffer.BlockCount; i++)
                last = last.Append(await buffer.GetTypedBlock(1));
            return new ReadOnlySequence<T>(first, 0, last, last.Memory.Length);
        }

        /// <summary>
        /// Creates a column analyser
        /// </summary>
        /// <param name="buffer">Buffer to analyse</param>
        /// <param name="metaData"></param>
        /// <param name="maxMetaDataWriteCount">Maximum count to write to meta data</param>
        /// <returns></returns>
        public static IDataAnalyser GetAnalyser(this IReadOnlyBuffer buffer, MetaData metaData, uint maxMetaDataWriteCount = Consts.MaxMetaDataWriteCount)
        {
            var dataType = buffer.DataType.GetBrightDataType();
            var columnType = ColumnTypeClassifier.GetClass(dataType, metaData);
            if (columnType.HasFlag(ColumnClass.Categorical)) {
                if (dataType == BrightDataType.String)
                    return StaticAnalysers.CreateStringAnalyser(maxMetaDataWriteCount);
                return StaticAnalysers.CreateFrequencyAnalyser(buffer.DataType, maxMetaDataWriteCount);
            }

            if (columnType.HasFlag(ColumnClass.IndexBased))
                return StaticAnalysers.CreateIndexAnalyser(maxMetaDataWriteCount);

            if (columnType.HasFlag(ColumnClass.Tensor))
                return StaticAnalysers.CreateDimensionAnalyser();

            return dataType switch
            {
                BrightDataType.Double     => StaticAnalysers.CreateNumericAnalyser(maxMetaDataWriteCount),
                BrightDataType.Float      => StaticAnalysers.CreateNumericAnalyser<float>(maxMetaDataWriteCount),
                BrightDataType.Decimal    => StaticAnalysers.CreateNumericAnalyser<decimal>(maxMetaDataWriteCount),
                BrightDataType.SByte      => StaticAnalysers.CreateNumericAnalyser<sbyte>(maxMetaDataWriteCount),
                BrightDataType.Int        => StaticAnalysers.CreateNumericAnalyser<int>(maxMetaDataWriteCount),
                BrightDataType.Long       => StaticAnalysers.CreateNumericAnalyser<long>(maxMetaDataWriteCount),
                BrightDataType.Short      => StaticAnalysers.CreateNumericAnalyser<short>(maxMetaDataWriteCount),
                BrightDataType.Date       => StaticAnalysers.CreateDateAnalyser(),
                BrightDataType.BinaryData => StaticAnalysers.CreateFrequencyAnalyser<BinaryData>(maxMetaDataWriteCount),
                BrightDataType.DateOnly   => StaticAnalysers.CreateFrequencyAnalyser<DateOnly>(maxMetaDataWriteCount),
                BrightDataType.TimeOnly   => StaticAnalysers.CreateFrequencyAnalyser<TimeOnly>(maxMetaDataWriteCount),
                _                         => throw new NotImplementedException()
            };
        }
        public static IOperation Analyse(this MetaData metaData, bool force, params IReadOnlyBuffer[] buffers)
        {
            if (force || !metaData.Get(Consts.HasBeenAnalysed, false)) {
                if (buffers.Length == 1) {
                    var buffer = buffers[0];
                    var analyser = buffer.GetAnalyser(metaData);
                    return GenericActivator.Create<IOperation>(typeof(BufferScan<>).MakeGenericType(buffer.DataType), buffer, analyser);
                }
                if (buffers.Length > 1) {
                    var analysers = buffers.Select(x => x.GetAnalyser(metaData)).ToArray();
                    var analyser = analysers[0];
                    if (analysers.Skip(1).Any(x => x.GetType() != analyser.GetType()))
                        throw new InvalidOperationException("Expected all buffers to be in same analysis category");

                    var operations = buffers.Select(x => GenericActivator.Create<IOperation>(typeof(BufferScan<>).MakeGenericType(x.DataType), x, analyser)).ToArray();
                    return new AggregateOperation(operations);
                }
            }
            return new NopOperation();
        }
        
        public static async Task<ICompositeBuffer> ToNumeric(this IReadOnlyBuffer buffer, 
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            if(Type.GetTypeCode(buffer.DataType) is TypeCode.DBNull or TypeCode.Empty or TypeCode.Object)
                throw new NotSupportedException();

            var analysis = GenericActivator.Create<ICastToNumericAnalysis>(typeof(CastToNumericAnalysis<>).MakeGenericType(buffer.DataType), buffer);
            await analysis.Process();

            BrightDataType toType;
            if (analysis.IsInteger) {
                toType = analysis switch 
                {
                    { MinValue: >= sbyte.MinValue, MaxValue: <= sbyte.MaxValue } => BrightDataType.SByte,
                    { MinValue: >= short.MinValue, MaxValue: <= short.MaxValue } => BrightDataType.Short,
                    { MinValue: >= int.MinValue, MaxValue: <= int.MaxValue } => BrightDataType.Int,
                    _ => BrightDataType.Long
                };
            } else {
                toType = analysis is { MinValue: >= float.MinValue, MaxValue: <= float.MaxValue } 
                    ? BrightDataType.Float 
                    : BrightDataType.Double;
            }

            var output = GenericActivator.Create<ICompositeBuffer>(typeof(UnmanagedCompositeBuffer<>).MakeGenericType(toType.GetDataType()), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, toType.GetDataType()), buffer, output);
            await conversion.Process();
            return output;
        }

        static readonly HashSet<string> TrueStrings = new() { "Y", "YES", "TRUE", "T", "1" };
        public static async Task<ICompositeBuffer<bool>> ToBoolean(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
            ) {
            var output = CreateCompositeBuffer<bool>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = (buffer.DataType == typeof(string))
                ? new CustomConversion<string, bool>(StringToBool, (IReadOnlyBuffer<string>)buffer, output)
                : GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, typeof(bool)), buffer, output)
            ;
            await conversion.Process();
            return output;
            static bool StringToBool(string str) => TrueStrings.Contains(str.ToUpperInvariant());
        }

        public static async Task<ICompositeBuffer<string>> ToString(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = GenericActivator.Create<IOperation>(typeof(ToStringConversion<>).MakeGenericType(buffer.DataType), buffer, output);
            await conversion.Process();
            return output;
        }

        public static async Task<ICompositeBuffer<DateTime>> ToDateTime(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<DateTime>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = (buffer.DataType == typeof(string))
                ? new CustomConversion<string, DateTime>(StringToDate, (IReadOnlyBuffer<string>)buffer, output)
                : GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, typeof(bool)), buffer, output)
            ;
            await conversion.Process();
            return output;

            static DateTime StringToDate(string str)
            {
                try {
                    return str.ToDateTime();
                }
                catch {
                    // return placeholder date
                    return DateTime.MinValue;
                }
            }
        }

        public static async Task<ICompositeBuffer<DateOnly>> ToDate(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<DateOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = (buffer.DataType == typeof(string))
                ? new CustomConversion<string, DateOnly>(StringToDate, (IReadOnlyBuffer<string>)buffer, output)
                : GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, typeof(bool)), buffer, output)
            ;
            await conversion.Process();
            return output;

            static DateOnly StringToDate(string str)
            {
                try {
                    return DateOnly.Parse(str);
                }
                catch {
                    // return placeholder date
                    return DateOnly.MinValue;
                }
            }
        }

        public static async Task<ICompositeBuffer<TimeOnly>> ToTime(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<TimeOnly>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = (buffer.DataType == typeof(string))
                ? new CustomConversion<string, TimeOnly>(StringToTime, (IReadOnlyBuffer<string>)buffer, output)
                : GenericActivator.Create<IOperation>(typeof(UnmanagedConversion<,>).MakeGenericType(buffer.DataType, typeof(bool)), buffer, output)
            ;
            await conversion.Process();
            return output;

            static TimeOnly StringToTime(string str)
            {
                try {
                    return TimeOnly.Parse(str);
                }
                catch {
                    // return placeholder date
                    return TimeOnly.MinValue;
                }
            }
        }

        public static async Task<ICompositeBuffer<int>> ToCategoricalIndex(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            
            var output = CreateCompositeBuffer<int>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            var conversion = GenericActivator.Create<IOperation>(typeof(ToCategoricalIndexConversion<>).MakeGenericType(buffer.DataType), buffer, output);
            await conversion.Process();
            return output;
        }

        public static async Task<ICompositeBuffer<IndexList>> ToIndexList(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<IndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            IOperation conversion;
            if (buffer.DataType == typeof(IndexList))
                conversion = new NopConversion<IndexList>((IReadOnlyBuffer<IndexList>)buffer, output);
            else if (buffer.DataType == typeof(WeightedIndexList))
                conversion = new CustomConversion<WeightedIndexList, IndexList>(WeightedIndexListToIndexList, (IReadOnlyBuffer<WeightedIndexList>)buffer, output);
            else if (buffer.DataType == typeof(ReadOnlyVector))
                conversion = new CustomConversion<ReadOnlyVector, IndexList>(VectorToIndexList, (IReadOnlyBuffer<ReadOnlyVector>)buffer, output);
            else
                throw new NotSupportedException("Only weighted index lists and vectors can be converted to index lists");
            await conversion.Process();
            return output;

            static IndexList VectorToIndexList(ReadOnlyVector vector) => vector.ReadOnlySegment.ToSparse().AsIndexList();
            static IndexList WeightedIndexListToIndexList(WeightedIndexList weightedIndexList) => weightedIndexList.AsIndexList();
        }

        public static async Task<ICompositeBuffer<ReadOnlyVector>> ToVector(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<ReadOnlyVector>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            IOperation conversion;
            if (buffer.DataType == typeof(ReadOnlyVector))
                conversion = new NopConversion<ReadOnlyVector>((IReadOnlyBuffer<ReadOnlyVector>)buffer, output);
            else if (buffer.DataType == typeof(WeightedIndexList))
                conversion = new CustomConversion<WeightedIndexList, ReadOnlyVector>(WeightedIndexListToVector, (IReadOnlyBuffer<WeightedIndexList>)buffer, output);
            else if (buffer.DataType == typeof(IndexList))
                conversion = new CustomConversion<IndexList, ReadOnlyVector>(IndexListToVector, (IReadOnlyBuffer<IndexList>)buffer, output);
            else {
                var index = GenericActivator.Create<IOperation>(typeof(TypedIndexer<>).MakeGenericType(buffer.DataType), buffer);
                await index.Process();
                conversion = GenericActivator.Create<IOperation>(typeof(OneHotConversion<>).MakeGenericType(buffer.DataType), buffer, index, output);
            }
            await conversion.Process();
            return output;

            static ReadOnlyVector WeightedIndexListToVector(WeightedIndexList weightedIndexList) => weightedIndexList.AsDense();
            static ReadOnlyVector IndexListToVector(IndexList indexList) => indexList.AsDense();
        }

        public static async Task<ICompositeBuffer<WeightedIndexList>> ToWeightedIndexList(this IReadOnlyBuffer buffer,
            IProvideTempData? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
            var output = CreateCompositeBuffer<WeightedIndexList>(x => new(x), tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
            IOperation conversion;
            if (buffer.DataType == typeof(WeightedIndexList))
                conversion = new NopConversion<WeightedIndexList>((IReadOnlyBuffer<WeightedIndexList>)buffer, output);
            else if (buffer.DataType == typeof(ReadOnlyVector))
                conversion = new CustomConversion<ReadOnlyVector, WeightedIndexList>(VectorToWeightedIndexList, (IReadOnlyBuffer<ReadOnlyVector>)buffer, output);
            else if (buffer.DataType == typeof(IndexList))
                conversion = new CustomConversion<IndexList, WeightedIndexList>(IndexListToWeightedIndexList, (IReadOnlyBuffer<IndexList>)buffer, output);
            else
                throw new NotSupportedException("Only weighted index lists, index lists and vectors can be converted to vectors");
            await conversion.Process();
            return output;

            static WeightedIndexList IndexListToWeightedIndexList(IndexList indexList) => indexList.AsWeightedIndexList();
            static WeightedIndexList VectorToWeightedIndexList(ReadOnlyVector vector) => vector.ToSparse();
        }

        
        public static Task Process(this IOperation[] operations, INotifyUser? notify = null, string? msg = null, CancellationToken ct = default)
        {
            if (operations.Length == 1)
                return operations[0].Process(notify, msg, ct);
            if (operations.Length > 1) {
                if (notify is not null)
                    notify = new AggregateNotification(operations.Length, notify, msg, ct);
                return Task.WhenAll(operations.Select(x => x.Process(notify, null, ct)));
            }
            return Task.CompletedTask;
        }

        public static IReadOnlyBuffer<T> ConvertTo<T>(this IReadOnlyBuffer buffer) where T: unmanaged
        {
            if (buffer.DataType == typeof(T))
                return (IReadOnlyBuffer<T>)buffer;
            var converter = StaticConverters.GetConverterMethodInfo.MakeGenericMethod(buffer.DataType, typeof(T)).Invoke(null, null);
            return GenericActivator.Create<IReadOnlyBuffer<T>>(typeof(TypeConverter<,>).MakeGenericType(buffer.DataType, typeof(T)), buffer, converter);
        }

        public static async Task<NormalisationModel> GetNormalization(this MetaData metaData, NormalizationType type, params IReadOnlyBuffer[] buffers)
        {
            if (metaData.Get(Consts.NormalizationType, NormalizationType.None) == type)
                return metaData.GetNormalization();

            if (!buffers.All(x => x.DataType.GetBrightDataType().IsNumeric()))
                throw new NotSupportedException("Only numeric buffers can be normalized");

            if (!metaData.Get(Consts.HasBeenAnalysed, false)) {
                var analyzer = StaticAnalysers.CreateNumericAnalyser();
                foreach (var buffer in buffers) {
                    var toDouble = ConvertTo<double>(buffer);
                    await toDouble.ForEachBlock(analyzer.Add);
                }
                analyzer.WriteTo(metaData);
            }

            var ret = new NormalisationModel(type, metaData);
            ret.WriteTo(metaData);
            return ret;
        }

        public static IReadOnlyBuffer<T> Normalize<T>(this IReadOnlyBuffer<T> buffer, INormalize normalize) where T : unmanaged, INumber<T>
        {
            return new NormalizationConverter<T>(buffer, normalize);
        }

        public static async Task<ICanVectorise> GetVectoriser(this IReadOnlyBuffer buffer, MetaData metaData, bool oneHotEncodeCategoricalData)
        {
            var dataType = buffer.DataType.GetBrightDataType();
            var ret = ColumnTypeClassifier.GetClass(dataType, metaData) switch {
                ColumnClass.Numeric => GenericActivator.Create<ICanVectorise>(typeof(NumericVectoriser<>).MakeGenericType(buffer.DataType)),
                ColumnClass.Categorical when oneHotEncodeCategoricalData => await GetOneHotEncoder(buffer, metaData),
                ColumnClass.Categorical => GenericActivator.Create<ICanVectorise>(typeof(CategoricalIndexVectorisation<>).MakeGenericType(buffer.DataType)),
                ColumnClass.IndexBased => await GetIndexBasedVectoriser(buffer, metaData),
                ColumnClass.Tensor => await GetTensorVectoriser(buffer, metaData),
                _ => throw new NotSupportedException()
            };
            ret.ReadFrom(metaData);
            return ret;

            static async Task<ICanVectorise> GetIndexBasedVectoriser(IReadOnlyBuffer buffer, MetaData metaData)
            {
                var size = metaData.Get<uint>(Consts.VectorisationSize, 0);
                if (size == 0) {
                    await metaData.Analyse(false, buffer).Process();
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
                    await metaData.Analyse(false, buffer).Process();
                    size = metaData.Get<uint>(Consts.NumDistinct, 0);
                    if (size == 0)
                        throw new Exception("Expected to find a distinct size of items");
                }
                return GenericActivator.Create<ICanVectorise>(typeof(OneHotVectoriser<>).MakeGenericType(buffer.DataType), size);
            }

            static async Task<ICanVectorise> GetTensorVectoriser(IReadOnlyBuffer buffer, MetaData metaData)
            {
                var size = metaData.Get<uint>(Consts.VectorisationSize, 0);
                if (size == 0) {
                    await metaData.Analyse(false, buffer).Process();
                    size = metaData.GetDimensionAnalysis().Size;
                    if (size == 0)
                        throw new Exception("Expected to find non empty tensors");
                }
                return GenericActivator.Create<ICanVectorise>(typeof(TensorVectoriser<>).MakeGenericType(buffer.DataType), size);
            }
        }

        public static Task<VectorisationModel> GetVectoriser(this IDataTable dataTable, bool oneHotEncodeCategoricalData, params uint[] columnIndices)
        {
            var buffers = dataTable.AllOrSpecifiedColumnIndices(false, columnIndices).Select(dataTable.GetColumn).ToArray();
            return buffers.GetVectoriser(oneHotEncodeCategoricalData);
        }

        public static async Task<VectorisationModel> GetVectoriser(this IReadOnlyBufferWithMetaData[] buffers, bool oneHotEncodeCategoricalData)
        {
            var createTasks = buffers.Select((x, i) => GetVectoriser(x, x.MetaData, oneHotEncodeCategoricalData)).ToArray();
            await Task.WhenAll(createTasks);
            var vectorisers = createTasks.Select(x => x.Result).ToArray();
            return new VectorisationModel(vectorisers);
        }

        public static async Task<VectorisationModel> GetVectoriser(this IReadOnlyBuffer[] buffers, bool oneHotEncodeCategoricalData, params MetaData[] metaData)
        {
            var first = buffers[0];
            if (buffers.Skip(1).Any(x => x.Size != first.Size || x.BlockSize != first.BlockSize))
                throw new ArgumentException("Expected all buffers to have the same size and block size", nameof(buffers));

            Task<ICanVectorise>[] createTasks;
            var shouldUpdateMetaData = false;
            if (metaData.Length == buffers.Length) {
                createTasks = buffers.Select((x, i) => GetVectoriser(x, metaData[i], oneHotEncodeCategoricalData)).ToArray();
                shouldUpdateMetaData = true;
            }else switch (metaData.Length) {
                case 0: {
                    var tempMetaData = new MetaData();
                    createTasks = buffers.Select(x => GetVectoriser(x, tempMetaData, oneHotEncodeCategoricalData)).ToArray();
                    break;
                }
                case 1: {
                    var firstMetaData = metaData[0];
                    createTasks = buffers.Select(x => GetVectoriser(x, firstMetaData, oneHotEncodeCategoricalData)).ToArray();
                    break;
                }
                default:
                    throw new ArgumentException("Expected either one, zero or a matching count of meta data", nameof(metaData));
            }

            await Task.WhenAll(createTasks);
            var vectorisers = createTasks.Select(x => x.Result).ToArray();
            return new VectorisationModel(vectorisers);
        }

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

                vectorisers[index++] = type switch {
                    VectorisationType.Tensor => GenericActivator.Create<ICanVectorise>(typeof(TensorVectoriser<>).MakeGenericType(dataType), size),
                    VectorisationType.WeightedIndexList => new WeightedIndexListVectoriser(size),
                    VectorisationType.CategoricalIndex => GenericActivator.Create<ICanVectorise>(typeof(CategoricalIndexVectorisation<>).MakeGenericType(dataType)),
                    VectorisationType.IndexList => new IndexListVectoriser(size),
                    VectorisationType.Numeric => GenericActivator.Create<ICanVectorise>(typeof(NumericVectoriser<>).MakeGenericType(dataType)),
                    VectorisationType.OneHot => GenericActivator.Create<ICanVectorise>(typeof(OneHotVectoriser<>).MakeGenericType(dataType), size),
                    _ => throw new NotImplementedException()
                };
            }

            return new VectorisationModel(vectorisers);
        }

        public static async Task<Dictionary<string, List<uint>>> GetGroups(this IReadOnlyBuffer[] buffers)
        {
            var enumerators = buffers.Select(x => x.EnumerateAll().GetAsyncEnumerator()).ToArray();
            var shouldContinue = true;
            var sb = new StringBuilder();
            Dictionary<string, List<uint>> ret = new();
            uint rowIndex = 0;

            while (shouldContinue) {
                sb.Clear();
                foreach (var enumerator in enumerators) {
                    if (!await enumerator.MoveNextAsync()) {
                        shouldContinue = false; 
                        break;
                    }
                    if (sb.Length > 0)
                        sb.Append('|');
                    sb.Append(enumerator.Current);
                }
                var str = sb.ToString();
                if(!ret.TryGetValue(str, out var list))
                    ret.Add(str, list = new());
                list.Add(rowIndex++);
            }

            return ret;
        }

        public static ReadOnlyMemory<object> AsObjects<T>(this ReadOnlyMemory<T> block) where T: notnull
        {
            var index = 0;
            var ret = new object[block.Length];
            foreach (ref readonly var item in block.Span)
                ret[index++] = item;
            return ret;
        }

        public static IEnumerable<uint> AllOrSpecifiedColumnIndices(this IDataTable dataTable, bool distinct, params uint[] indices) => indices.Length == 0 
            ? dataTable.ColumnCount.AsRange() 
            : distinct 
                ? indices.Order().Distinct()
                : indices
        ;

        /// <summary>
        /// Enumerates specified row indices (or all if none specified)
        /// </summary>
        /// <param name="indices">Row indices (optional)</param>
        /// <returns></returns>
        public static IEnumerable<uint> AllOrSpecifiedRowIndices(this IDataTable dataTable, bool distinct, params uint[] indices) => indices.Length == 0
            ? dataTable.RowCount.AsRange()
            : distinct 
                ? indices.Order().Distinct()
                : indices
        ;

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

        public static IReadOnlyBuffer<string> ToReadOnlyStringBuffer(this IReadOnlyBuffer buffer)
        {
            if (buffer.DataType == typeof(string))
                return (IReadOnlyBuffer<string>)buffer;
            return GenericActivator.Create<IReadOnlyBuffer<string>>(typeof(ToStringConverter<>).MakeGenericType(typeof(string)), buffer);
        }
    }
}
