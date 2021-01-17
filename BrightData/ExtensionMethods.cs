using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using BrightData.Analysis;
using BrightData.Analysis.Readers;
using BrightData.Buffers;
using BrightData.Converters;
using BrightData.Distributions;
using BrightData.Helper;
using BrightData.Memory;

namespace BrightData
{
    public static partial class ExtensionMethods
    {
        public static Type ToType(this TypeCode code)
        {
            return code switch
            {
                TypeCode.Boolean => typeof(bool),
                TypeCode.Byte => typeof(byte),
                TypeCode.Char => typeof(char),
                TypeCode.DateTime => typeof(DateTime),
                TypeCode.DBNull => typeof(DBNull),
                TypeCode.Decimal => typeof(decimal),
                TypeCode.Double => typeof(double),
                TypeCode.Empty => null,
                TypeCode.Int16 => typeof(short),
                TypeCode.Int32 => typeof(int),
                TypeCode.Int64 => typeof(long),
                TypeCode.Object => typeof(object),
                TypeCode.SByte => typeof(sbyte),
                TypeCode.Single => typeof(Single),
                TypeCode.String => typeof(string),
                TypeCode.UInt16 => typeof(UInt16),
                TypeCode.UInt32 => typeof(UInt32),
                TypeCode.UInt64 => typeof(UInt64),
                _ => null,
            };
        }

        public static IndexList CreateIndexList(this IBrightDataContext context, params uint[] indices) => IndexList.Create(context, indices);
        public static IndexList CreateIndexList(this IBrightDataContext context, IEnumerable<uint> indices) => IndexList.Create(context, indices);

        /// <summary>
        /// Creates an index list from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader">The binary reader</param>
        public static IndexList CreateIndexList(this IBrightDataContext context, BinaryReader reader)
        {
            var ret = new IndexList(context); ;
            ret.Initialize(context, reader);
            return ret;
        }

        public static WeightedIndexList CreateWeightedIndexList(this IBrightDataContext context, params (uint Index, float Weight)[] indexList) => WeightedIndexList.Create(context, indexList);
        public static WeightedIndexList CreateWeightedIndexList(this IBrightDataContext context, IEnumerable<(uint Index, float Weight)> indexList) => WeightedIndexList.Create(context, indexList);

        /// <summary>
        /// Creates a weighted index list from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader">The binary reader</param>
        public static WeightedIndexList CreateWeightedIndexList(this IBrightDataContext context, BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new WeightedIndexList.Item[len];
            var span = MemoryMarshal.Cast<WeightedIndexList.Item, byte>(ret);
            reader.BaseStream.Read(span);

            return WeightedIndexList.Create(context, ret);
        }

        public static Vector<T> CreateVector<T>(this IBrightDataContext context, uint size, Func<uint, T> initializer) where T: struct
        {
            var segment = context.CreateSegment<T>(size);
            if (initializer != null)
                segment.Initialize(initializer);
            return new Vector<T>(segment);
        }

        public static Vector<T> CreateVector<T>(this IBrightDataContext context, uint size, T initializer = default) where T: struct
        {
            var segment = context.CreateSegment<T>(size);
            segment.Initialize(initializer);
            return new Vector<T>(segment);
        }

        public static Vector<T> CreateVector<T>(this IBrightDataContext context, params T[] initialData) where T: struct
        {
            var segment = context.CreateSegment<T>((uint)initialData.Length);
            if (initialData.Any())
                segment.Initialize(initialData);
            return new Vector<T>(segment);
        }

        public static Vector<T> CreateVector<T>(this IBrightDataContext context, BinaryReader reader) where T : struct
        {
            return new Vector<T>(context, reader);
        }

        public static Matrix<T> CreateMatrix<T>(this IBrightDataContext context, uint rows, uint columns, Func<uint, uint, T> initializer = null) where T: struct
        {
            var segment = context.CreateSegment<T>(rows * columns);
            if (initializer != null)
                segment.Initialize(i => initializer(i / columns, i % columns));
            return new Matrix<T>(segment, rows, columns);
        }
        public static Matrix<T> CreateMatrix<T>(this IBrightDataContext context, uint rows, uint columns, T initializer) where T: struct => CreateMatrix(context, rows, columns, (i, j) => initializer);

        public static Matrix<T> CreateMatrix<T>(this IBrightDataContext context, BinaryReader reader) where T : struct
        {
            return new Matrix<T>(context, reader);
        }

        public static Matrix<T> CreateMatrixFromRows<T>(this IBrightDataContext context, params Vector<T>[] rows) where T: struct
        {
            var columns = rows.First().Size;
            return CreateMatrix(context, (uint) rows.Length, columns, (j, i) => rows[j][i]);
        }

        public static Matrix<T> CreateMatrixFromRows<T>(this IBrightDataContext context, params T[][] rows) where T : struct
        {
            var columns = (uint)rows.First().Length;
            return CreateMatrix(context, (uint)rows.Length, columns, (j, i) => rows[j][i]);
        }

        public static Matrix<T> CreateMatrixFromColumns<T>(this IBrightDataContext context, params Vector<T>[] columns) where T: struct
        {
            var rows = columns.First().Size;
            return CreateMatrix(context, rows, (uint) columns.Length, (j, i) => columns[i][j]);
        }

        public static Matrix<T> CreateMatrixFromColumns<T>(this IBrightDataContext context, params T[][] columns) where T : struct
        {
            var rows = (uint)columns.First().Length;
            return CreateMatrix(context, rows, (uint)columns.Length, (j, i) => columns[i][j]);
        }

        public static Tensor3D<T> CreateTensor3D<T>(this IBrightDataContext context, uint depth, uint rows, uint columns) where T : struct
        {
            var segment = context.CreateSegment<T>(depth * rows * columns);
            return new Tensor3D<T>(segment, depth, rows, columns);
        }

        public static Tensor3D<T> CreateTensor3D<T>(this IBrightDataContext context, params Matrix<T>[] slices) where T : struct
        {
            var first = slices.First();
            var depth = (uint) slices.Length;
            var rows = first.RowCount;
            var columns = first.ColumnCount;

            var data = context.CreateSegment<T>(depth * rows * columns);
            var ret = new Tensor3D<T>(data, depth, rows, columns);
            var allSame = ret.Matrices.Zip(slices, (t, s) => {
                if (s.RowCount == t.RowCount && s.ColumnCount == t.ColumnCount) {
                    s.Segment.CopyTo(t.Segment);
                    return true;
                }
                return false;
            }).All(v => v);
            if(!allSame)
                throw new ArgumentException("Input matrices had different sizes");
            return ret;
        }

        public static Tensor3D<T> CreateTensor3D<T>(this IBrightDataContext context, BinaryReader reader) where T : struct
        {
            return new Tensor3D<T>(context, reader);
        }

        public static Tensor4D<T> CreateTensor4D<T>(this IBrightDataContext context, uint count, uint depth, uint rows, uint columns) where T : struct
        {
            var segment = context.CreateSegment<T>(count * depth * rows * columns);
            return new Tensor4D<T>(segment, count, depth, rows, columns);
        }

        public static Tensor4D<T> CreateTensor4D<T>(this IBrightDataContext context, BinaryReader reader) where T : struct
        {
            return new Tensor4D<T>(context, reader);
        }

        public static uint GetColumnCount<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 1 ? tensor.Shape[^1] : 0;
        }

        public static uint GetRowCount<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 1 ? tensor.Shape[^2] : 0;
        }

        public static uint GetDepth<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 2 ? tensor.Shape[^3] : 0;
        }

        public static uint GetCount<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 3 ? tensor.Shape[^4] : 0;
        }

        public static WeightedIndexList ToSparse(this ITensorSegment<float> segment)
        {
            return WeightedIndexList.Create(segment.Context, segment.Values
                .Select((v, i) => new WeightedIndexList.Item((uint)i, v))
                .Where(d => FloatMath.IsNotZero(d.Weight))
            );
        }

        public static bool SetIfNotNull<T>(this IMetaData metadata, string name, T? value)
            where T : struct, IConvertible
        {
            if (value.HasValue) {
                metadata.Set(name, value.Value);
                return true;
            }
            return false;
        }

        public static bool SetIfNotNull<T>(this IMetaData metadata, string name, T value)
            where T : class, IConvertible
        {
            if (value != null) {
                metadata.Set(name, value);
                return true;
            }
            return false;
        }

        public static bool HasConversionOperator(this Type from, Type to)
        {
            UnaryExpression BodyFunction(Expression body) => Expression.Convert(body, to);
            var inp = Expression.Parameter(from, "inp");
            try {
                // If this succeeds then we can cast 'from' type to 'to' type using implicit coercion
                Expression.Lambda(BodyFunction(inp), inp).Compile();
                return true;
            }
            catch (InvalidOperationException) {
                return false;
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq, Random rnd)
        {
            return seq.OrderBy(e => rnd.Next()).ToList();
        }

        public static (T[] Training, T[] Test) Split<T>(this T[] seq, double trainPercentage = 0.8)
        {
            var input = Enumerable.Range(0, seq.Length).ToList();
            int trainingCount = Convert.ToInt32(seq.Length * trainPercentage);
            return (
                input.Take(trainingCount).Select(i => seq[i]).ToArray(),
                input.Skip(trainingCount).Select(i => seq[i]).ToArray()
            );
        }

        public static T[] Bag<T>(this T[] list, uint count, Random rnd)
        {
            return count.AsRange()
                .Select(i => list[rnd.Next(0, list.Length)])
                .ToArray()
            ;
        }

        public static string Name(this IMetaData metadata) => metadata.Get<string>(Consts.Name);
        public static uint Index(this IMetaData metadata) => metadata.Get<uint>(Consts.Index);
        public static bool IsNumeric(this IMetaData metadata) => metadata.Get<bool>(Consts.IsNumeric);
        public static bool IsTarget(this IMetaData metadata) => metadata.Get<bool>(Consts.IsTarget);
        public static bool IsCategorical(this IMetaData metadata) => metadata.Get<bool>(Consts.IsCategorical);
        public static bool IsSequential(this IMetaData metadata) => metadata.Get<bool>(Consts.IsSequential);

        public static string Name(this IHaveMetaData metadataProvider) => metadataProvider.MetaData.Name();
        public static uint Index(this IHaveMetaData metadataProvider) => metadataProvider.MetaData.Index();
        public static bool IsNumeric(this IHaveMetaData metadataProvider) => metadataProvider.MetaData.IsNumeric();
        public static bool IsTarget(this IHaveMetaData metadataProvider) => metadataProvider.MetaData.IsTarget();
        public static bool IsCategorical(this IHaveMetaData metadataProvider) => metadataProvider.MetaData.IsCategorical();
        public static bool IsSequential(this IHaveMetaData metadataProvider) => metadataProvider.MetaData.IsSequential();

        public static float CosineDistance(this float[] vector, float[] other)
        {
            return Distance.CosineDistance.Calculate(vector, other);
        }

        public static float EuclideanDistance(this float[] vector, float[] other)
        {
            return Distance.EuclideanDistance.Calculate(vector, other);
        }

        public static float ManhattanDistance(this float[] vector, float[] other)
        {
            return Distance.ManhattanDistance.Calculate(vector, other);
        }

        public static Vector<float> Mutate(this Vector<float> vector, Func<float, float> mutator)
        {
            var context = vector.Context;
            var segment = context.CreateSegment<float>(vector.Size);
            segment.Initialize(i => mutator(vector[i]));
            return new Vector<float>(segment);
        }

        public static Vector<float> MutateWith(this Vector<float> vector, Vector<float> other, Func<float, float, float> mutator)
        {
            var context = vector.Context;
            var segment = context.CreateSegment<float>(vector.Size);
            segment.Initialize(i => mutator(vector[i], other[i]));
            return new Vector<float>(segment);
        }

        public static IMetaData GetMetaData(this IWriteToMetaData writer)
        {
            var ret = new MetaData();
            writer.WriteTo(ret);
            return ret;
        }

        public static DateAnalysis GetDateAnalysis(this IMetaData metaData) => new DateAnalysis(metaData);
        public static DimensionAnalysis GetDimensionAnalysis(this IMetaData metaData) => new DimensionAnalysis(metaData);
        public static FrequencyAnalysis GetFrequencyAnalysis(this IMetaData metaData) => new FrequencyAnalysis(metaData);
        public static IndexAnalysis GetIndexAnalysis(this IMetaData metaData) => new IndexAnalysis(metaData);
        public static NumericAnalysis GetNumericAnalysis(this IMetaData metaData) => new NumericAnalysis(metaData);
        public static StringAnalysis GetStringAnalysis(this IMetaData metaData) => new StringAnalysis(metaData);

        public static NumericAnalysis Analyze<T>(this IEnumerable<T> data)
            where T: struct
        {
            var analysis = new CastToDoubleNumericAnalysis<T>();
            foreach(var item in data)
                analysis.Add(item);
            return analysis.GetMetaData().GetNumericAnalysis();
        }

        public static DateAnalysis Analyze(this IEnumerable<DateTime> dates)
        {
            var analysis = new DateAnalyser();
            foreach(var item in dates)
                analysis.Add(item);
            return analysis.GetMetaData().GetDateAnalysis();
        }

        public static DimensionAnalysis Analyze(this IEnumerable<ITensor<float>> tensors)
        {
            var analysis = new DimensionAnalyser();
            foreach (var item in tensors)
                analysis.Add(item);
            return analysis.GetMetaData().GetDimensionAnalysis();
        }

        public static IndexAnalysis Analyze<T>(this IEnumerable<IHaveIndices> items)
        {
            var analysis = new IndexAnalyser();
            foreach (var item in items)
                analysis.Add(item);
            return analysis.GetMetaData().GetIndexAnalysis();
        }

        public static StringAnalysis Analyze<T>(this IEnumerable<string> items)
        {
            var analysis = new StringAnalyser();
            foreach (var item in items)
                analysis.Add(item);
            return analysis.GetMetaData().GetStringAnalysis();
        }

        public static FrequencyAnalysis AnalyzeFrequency<T>(this IEnumerable<T> items)
        {
            var analysis = new FrequencyAnalyser<T>();
            foreach (var item in items)
                analysis.Add(item);
            return analysis.GetMetaData().GetFrequencyAnalysis();
        }

        public static void InitializeRandomly<T>(this ITensor<T> tensor) where T : struct
        {
            var computation = tensor.Computation;
            tensor.Segment.Initialize(i => computation.NextRandom());
        }

        public static void Initialize<T>(this ITensor<T> tensor, T value) where T : struct
        {
            tensor.Segment.Initialize(value);
        }

        public static void Initialize<T>(this ITensor<T> tensor, Func<uint, T> initializer) where T : struct
        {
            tensor.Segment.Initialize(initializer);
        }

        public static ICanConvert<T, float> GetFloatConverter<T>(this IBrightDataContext context) where T: struct
        {
            return context.Set($"float-converter({typeof(T)})", () => new ConvertToFloat<T>());
        }

        public static void Set<T>(this ITensorSegment<T> vector, Func<uint, T> getValue)
            where T : struct
        {
            for (uint i = 0, len = vector.Size; i < len; i++)
                vector[i] = getValue(i);
        }

        public static ICanConvert GetConverter<T>(this Type toType) where T : struct
        {
            var typeCode = Type.GetTypeCode(toType);
            switch (typeCode) {
                case TypeCode.Single:
                    return new ConvertToFloat<T>();
                case TypeCode.Double:
                    return new ConvertToDouble<T>();
                case TypeCode.SByte:
                    return new ConvertToSignedByte<T>();
                case TypeCode.Int16:
                    return new ConvertToShort<T>();
                case TypeCode.Int32:
                    return new ConvertToInt<T>();
                case TypeCode.Int64:
                    return new ConvertToLong<T>();
                case TypeCode.Decimal:
                    return new ConvertToDecimal<T>();
                default:
                    throw new NotImplementedException();
            }
        }

        public static IEnumerable<uint> AsRange(this uint count) => Enumerable.Range(0, (int)count).Select(i => (uint)i);
        public static IEnumerable<uint> AsRange(this int count) => Enumerable.Range(0, count).Select(i => (uint)i);
        public static IEnumerable<uint> AsRange(this uint count, uint start) => Enumerable.Range((int)start, (int)count).Select(i => (uint)i);
        public static IEnumerable<uint> AsRange(this int count, int start) => Enumerable.Range(start, count).Select(i => (uint)i);

        public static IEnumerable<IFloatVector> AsFloatVectors(this IEnumerable<Vector<float>> vectors)
        {
            IBrightDataContext context = null;
            foreach (var vector in vectors) {
                context ??= vector.Context;
                yield return context.LinearAlgebraProvider.CreateVector(vector);
            }
        }

        public static float Execute(this OperationType operation, List<float> data)
        {
            if (operation == OperationType.Add)
                return data.Sum();
            else if (operation == OperationType.Average)
                return data.Average();
            else if (operation == OperationType.Max)
                return data.Max();

            throw new NotImplementedException();
        }

        public static ITensorSegment<T> CreateSegment<T>(this IBrightDataContext context, T[] block) where T : struct => new TensorSegment<T>(context, block);
        public static ITensorSegment<T> CreateSegment<T>(this IBrightDataContext context, uint size) where T : struct => new TensorSegment<T>(context, context.TensorPool.Get<T>(size));

        public static IMetaData SetTargetColumn(this IMetaData metaData, bool isTarget)
        {
            metaData.Set(Consts.IsTarget, isTarget);
            return metaData;
        }

        public static IMetaData SetCategorical(this IMetaData metaData, bool isCategorical)
        {
            metaData.Set(Consts.IsCategorical, isCategorical);
            return metaData;
        }

        /// <summary>
        /// Converts the indexed classifications to weighted indexed classifications
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        /// <param name="groupByClassification">True to group by classification (i.e convert the bag to a set)</param>
        public static IReadOnlyList<(string Label, WeightedIndexList Data)> ConvertToWeightedIndexList(
            this IReadOnlyList<(string Label, IndexList Data)> data,
            IBrightDataContext context,
            bool groupByClassification
        )
        {
            if (groupByClassification)
            {
                return data.GroupBy(c => c.Label)
                    .Select(g => (g.Key, WeightedIndexList.Create(context, g.SelectMany(d => d.Data.Indices)
                        .GroupBy(d => d)
                        .Select(g2 => new WeightedIndexList.Item(g2.Key, g2.Count()))
                        .ToArray()
                    )))
                    .ToArray()
                ;
            }
            return data
                .Select(d => (d.Label, WeightedIndexList.Create(context, d.Data.Indices
                    .GroupBy(i => i)
                    .Select(g2 => new WeightedIndexList.Item(g2.Key, g2.Count()))
                    .ToArray()
                )))
                .ToArray()
            ;
        }

        /// <summary>
        /// Finds the greatest weight within the weighted index classification list
        /// </summary>
        /// <param name="data"></param>
        public static float GetMaxWeight(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            return data.SelectMany(r => r.Data.Indices).Max(wi => wi.Weight);
        }

        /// <summary>
        /// Find the greatest index within the weighted index classification list
        /// </summary>
        /// <param name="data"></param>
        public static uint GetMaxIndex(this IReadOnlyList<(string Label, WeightedIndexList Data)> data)
        {
            return data.SelectMany(r => r.Data.Indices).Max(wi => wi.Index);
        }

        /// <summary>
        /// Find the greatest index within the index classification list
        /// </summary>
        /// <param name="data"></param>
        public static uint GetMaxIndex(this IReadOnlyList<(string Label, IndexList Data)> data)
        {
            return data.SelectMany(r => r.Data.Indices).Max();
        }

        /// <summary>
        /// Modifies the weights in the classification set based on relative corpus statistics to increase the weight of important words relative to each document
        /// https://en.wikipedia.org/wiki/Tf%E2%80%93idf
        /// </summary>
        /// <returns>A new weighted classification set</returns>
        public static IReadOnlyList<(string Label, WeightedIndexList Data)> TFIDF(this IReadOnlyList<(string Label, WeightedIndexList Data)> data, IBrightDataContext context)
        {
            var indexOccurence = new Dictionary<uint, uint>();
            var classificationSum = new Dictionary<string, double>();

            // find the overall count of each index
            foreach (var classification in data.GroupBy(c => c.Label))
            {
                double sum = 0;
                foreach (var item in classification)
                {
                    foreach (var index in item.Data.Indices)
                    {
                        var key = index.Index;
                        if (indexOccurence.TryGetValue(key, out uint temp))
                            indexOccurence[key] = temp + 1;
                        else
                            indexOccurence.Add(key, 1);
                        sum += index.Weight;
                    }
                }
                classificationSum.Add(classification.Key, sum);
            }

            // calculate tf-idf for each document
            var numDocs = (double)data.Count;
            var ret = new List<(string Label, WeightedIndexList Data)>();
            foreach (var classification in data)
            {
                var totalWords = classificationSum[classification.Label];
                var classificationIndex = new List<WeightedIndexList.Item>();
                foreach (var item in classification.Data.Indices)
                {
                    var index = item.Index;
                    var tf = item.Weight / totalWords;
                    var docsWithTerm = (double)indexOccurence[index];
                    var idf = Math.Log(numDocs / (1.0 + docsWithTerm));
                    var score = tf * idf;
                    classificationIndex.Add(new WeightedIndexList.Item(index, Convert.ToSingle(score)));
                }
                ret.Add((classification.Label, WeightedIndexList.Create(context, classificationIndex.ToArray())));
            }
            return ret;
        }

        /// <summary>
        /// Normalises the weighted index classification list to fit between 0 and 1
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IReadOnlyList<(string Label, WeightedIndexList Data)> Normalise(this IReadOnlyList<(string Label, WeightedIndexList Data)> data, IBrightDataContext context)
        {
            var maxWeight = data.GetMaxWeight();
            return data.Select(r => (r.Label, WeightedIndexList.Create(
                context, 
                r.Data.Indices.Select(wi => new WeightedIndexList.Item(wi.Index, wi.Weight / maxWeight)).ToArray()
            ))).ToList();
        }


        public static IEnumerable<(T Item, uint Count)> GroupAndCount<T>(this IEnumerable<T> items) => items
            .GroupBy(d => d)
            .Select(g => (g.Key, (uint)g.Count()))
        ;

        public static string Format<T>(this IEnumerable<(T Item, uint Count)> items) =>
            String.Join(';', items.Select(i => $"{i.Item.ToString()}: {i.Count}"));

        public static void UseLegacySerialisationInput(this IBrightDataContext context, bool use = true) => context.Set(Consts.LegacyFloatSerialisationInput, use);

        public static IHybridBuffer<T> CreateHybridStructBuffer<T>(this IBrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) where T: struct =>
            StaticBuffers.CreateHybridStructBuffer<T>(tempStream, bufferSize, maxDistinct);
        public static IHybridBuffer CreateHybridStructBuffer(this IBrightDataContext context, Type type, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) =>
            StaticBuffers.CreateHybridStructBuffer(tempStream, type, bufferSize, maxDistinct);
        public static IHybridBuffer<string> CreateHybridStringBuffer(this IBrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) =>
            StaticBuffers.CreateHybridStringBuffer(tempStream, bufferSize, maxDistinct);
        public static IHybridBuffer<T> CreateHybridObjectBuffer<T>(this IBrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768) =>
            StaticBuffers.CreateHybridObjectBuffer<T>(tempStream, context, bufferSize);
        public static IHybridBuffer CreateHybridObjectBuffer(this IBrightDataContext context, Type type, IProvideTempStreams tempStream, uint bufferSize = 32768) =>
            StaticBuffers.CreateHybridObjectBuffer(tempStream, context, type, bufferSize);

        public static INonNegativeDiscreteDistribution CreateBernoulliDistribution(this IBrightDataContext context, float probability) => new BernoulliDistribution(context, probability);
        public static INonNegativeDiscreteDistribution CreateBinomialDistribution(this IBrightDataContext context, float probability, uint numTrials) => new BinomialDistribution(context, probability, numTrials);
        public static INonNegativeDiscreteDistribution CreateCategoricalDistribution(this IBrightDataContext context, IEnumerable<float> categoricalValues) => new CategoricalDistribution(context, categoricalValues);
        public static IContinuousDistribution CreateContinuousDistribution(this IBrightDataContext context, float inclusiveLowerBound = 0f, float exclusiveUpperBound = 1f) => new ContinuousDistribution(context, inclusiveLowerBound, exclusiveUpperBound);
        public static IDiscreteDistribution CreateDiscreteUniformDistribution(this IBrightDataContext context, int inclusiveLowerBound, int exclusiveUpperBound) => new DiscreteUniformDistribution(context, inclusiveLowerBound, exclusiveUpperBound);
        public static IContinuousDistribution CreateNormalDistribution(this IBrightDataContext context, float mean = 0f, float stdDev = 1f) => new NormalDistribution(context, mean, stdDev);
    }
}
