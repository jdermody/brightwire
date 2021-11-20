using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using BrightData.Converter;
using BrightData.Helper;

namespace BrightData
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Converts a type code to a type
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns></returns>
        public static Type? ToType(this TypeCode code)
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

        /// <summary>
        /// Creates an index list from indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indices">Indices</param>
        /// <returns></returns>
        public static IndexList CreateIndexList(this IBrightDataContext context, params uint[] indices) => IndexList.Create(context, indices);

        /// <summary>
        /// Creates an index list from indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indices">Indices</param>
        /// <returns></returns>
        public static IndexList CreateIndexList(this IBrightDataContext context, IEnumerable<uint> indices) => IndexList.Create(context, indices);

        /// <summary>
        /// Creates an index list from a binary reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader">The binary reader</param>
        public static IndexList CreateIndexList(this IBrightDataContext context, BinaryReader reader)
        {
            var ret = new IndexList(context, Array.Empty<uint>());
            ret.Initialize(context, reader);
            return ret;
        }

        /// <summary>
        /// Creates a weighted index list from weighted indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList CreateWeightedIndexList(this IBrightDataContext context, params (uint Index, float Weight)[] indexList) => WeightedIndexList.Create(context, indexList);

        /// <summary>
        /// Creates a weighted index list from weighted indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList CreateWeightedIndexList(this IBrightDataContext context, IEnumerable<(uint Index, float Weight)> indexList) => WeightedIndexList.Create(context, indexList);

        /// <summary>
        /// Creates a weighted index list from weighted indices
        /// </summary>
        /// <param name="context"></param>
        /// <param name="indexList">Weighted indices</param>
        /// <returns></returns>
        public static WeightedIndexList CreateWeightedIndexList(this IBrightDataContext context, IEnumerable<WeightedIndexList.Item> indexList) => WeightedIndexList.Create(context, indexList);

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

        /// <summary>
        /// Sets a value only if the value is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetIfNotNull<T>(this IMetaData metadata, string name, T? value)
            where T : struct, IConvertible
        {
            if (value.HasValue) {
                metadata.Set(name, value.Value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets a value only if the value is not null
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool SetIfNotNull<T>(this IMetaData metadata, string name, T? value)
            where T : class, IConvertible
        {
            if (value != null) {
                metadata.Set(name, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if one type can be implicitly cast to another
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Randomly shuffles the items in the sequence
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="rnd">Random number generator to use</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq, Random rnd)
        {
            return seq.OrderBy(_ => rnd.Next());
        }

        /// <summary>
        /// Randomly splits the sequence into a two arrays (either "training" or "test")
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="trainPercentage">Percentage of items to add to the training array</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static (T[] Training, T[] Test) Split<T>(this T[] seq, double trainPercentage = 0.8)
        {
            var input = Enumerable.Range(0, seq.Length).ToList();
            int trainingCount = System.Convert.ToInt32(seq.Length * trainPercentage);
            return (
                input.Take(trainingCount).Select(i => seq[i]).ToArray(),
                input.Skip(trainingCount).Select(i => seq[i]).ToArray()
            );
        }

        /// <summary>
        /// Sample with replacement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count">Number of samples</param>
        /// <param name="rnd">Random number generator to use</param>
        /// <returns></returns>
        public static T[] Bag<T>(this T[] list, uint count, Random rnd)
        {
            return count.AsRange()
                .Select(_ => list[rnd.Next(0, list.Length)])
                .ToArray()
            ;
        }

        /// <summary>
        /// Item name
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static string GetName(this IMetaData metadata) => metadata.Get(Consts.Name, "");

        /// <summary>
        /// Item index
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static uint GetIndex(this IMetaData metadata) => metadata.Get(Consts.Index, uint.MaxValue);

        /// <summary>
        /// True if the item is numeric
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsNumeric(this IMetaData metadata) => metadata.Get(Consts.IsNumeric, false);

        /// <summary>
        /// True if the item is a target
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsTarget(this IMetaData metadata) => metadata.Get(Consts.IsTarget, false);

        /// <summary>
        /// True if the item is categorical
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsCategorical(this IMetaData metadata) => metadata.Get(Consts.IsCategorical, false);

        /// <summary>
        /// True if the item is sequential
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsSequential(this IMetaData metadata) => metadata.Get(Consts.IsSequential, false);

        /// <summary>
        /// Writes available meta data to a new meta data store
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public static IMetaData GetMetaData(this IWriteToMetaData writer)
        {
            var ret = new MetaData();
            writer.WriteTo(ret);
            return ret;
        }

        /// <summary>
        /// Lazy create a float converter per context
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ICanConvert<T, float> GetFloatConverter<T>(this IBrightDataContext context) where T: struct
        {
            return context.Get($"float-converter({typeof(T)})", () => new ConvertToFloat<T>());
        }

        /// <summary>
        /// Generates a range of positive integers
        /// </summary>
        /// <param name="count">Upper bound</param>
        /// <returns></returns>
        public static IEnumerable<uint> AsRange(this uint count) => Enumerable.Range(0, (int)count).Select(i => (uint)i);

        /// <summary>
        /// Generates a range of positive integers
        /// </summary>
        /// <param name="count">Upper bound</param>
        /// <returns></returns>
        public static IEnumerable<uint> AsRange(this int count) => Enumerable.Range(0, count).Select(i => (uint)i);

        /// <summary>
        /// Generates a range of positive integers
        /// </summary>
        /// <param name="count"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static IEnumerable<uint> AsRange(this uint count, uint start) => Enumerable.Range((int)start, (int)count).Select(i => (uint)i);

        /// <summary>
        /// Generates a range of positive integers
        /// </summary>
        /// <param name="count"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static IEnumerable<uint> AsRange(this int count, int start) => Enumerable.Range(start, count).Select(i => (uint)i);

        /// <summary>
        /// Aggregates a list of floats
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="data">Data to aggregate</param>
        /// <returns></returns>
        public static float Aggregate(this AggregationType operation, List<float> data)
        {
            return operation switch {
                AggregationType.Sum => data.Sum(),
                AggregationType.Average => data.Average(),
                AggregationType.Max => data.Max(),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Sets this as a target
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="isTarget"></param>
        /// <returns></returns>
        public static IMetaData SetTarget(this IMetaData metaData, bool isTarget)
        {
            metaData.Set(Consts.IsTarget, isTarget);
            return metaData;
        }

        /// <summary>
        /// Sets this as categorical
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="isCategorical"></param>
        /// <returns></returns>
        public static IMetaData SetIsCategorical(this IMetaData metaData, bool isCategorical)
        {
            metaData.Set(Consts.IsCategorical, isCategorical);
            return metaData;
        }

        /// <summary>
        /// Sets the name
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="name">Name</param>
        public static IMetaData SetName(this IMetaData metaData, string name)
        {
            metaData.Set(Consts.Name, name);
            return metaData;
        }

        /// <summary>
        /// Returns the file path associated with the meta data (if any)
        /// </summary>
        /// <param name="metaData"></param>
        /// <returns>File path</returns>
        public static string GetFilePath(this IMetaData metaData) => metaData.Get(Consts.FilePath, "");

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
        public static IReadOnlyList<(T Label, WeightedIndexList Data)> Tfidf<T>(this IReadOnlyList<(T Label, WeightedIndexList Data)> data, IBrightDataContext context) where T: notnull
        {
            var indexOccurence = new Dictionary<uint, uint>();
            var classificationSum = new Dictionary<T, double>();

            // find the overall count of each index
            foreach (var classification in data.GroupBy(c => c.Label))
            {
                double sum = 0;
                foreach (var (_, weightedIndexList) in classification)
                {
                    foreach (var index in weightedIndexList.Indices)
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
            var ret = new List<(T Label, WeightedIndexList Data)>();
            foreach (var (label, weightedIndexList) in data)
            {
                var totalWords = classificationSum[label];
                var classificationIndex = new List<WeightedIndexList.Item>();
                foreach (var item in weightedIndexList.Indices)
                {
                    var index = item.Index;
                    var tf = item.Weight / totalWords;
                    var docsWithTerm = (double)indexOccurence[index];
                    var idf = Math.Log(numDocs / (1.0 + docsWithTerm));
                    var score = tf * idf;
                    classificationIndex.Add(new WeightedIndexList.Item(index, System.Convert.ToSingle(score)));
                }
                ret.Add((label, WeightedIndexList.Create(context, classificationIndex.ToArray())));
            }
            return ret;
        }

        /// <summary>
        /// Normalizes the weighted index classification list to fit between 0 and 1
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IReadOnlyList<(string Label, WeightedIndexList Data)> Normalize(this IReadOnlyList<(string Label, WeightedIndexList Data)> data, IBrightDataContext context)
        {
            var maxWeight = data.GetMaxWeight();
            return data.Select(r => (r.Label, WeightedIndexList.Create(
                context, 
                r.Data.Indices.Select(wi => new WeightedIndexList.Item(wi.Index, wi.Weight / maxWeight)).ToArray()
            ))).ToList();
        }

        /// <summary>
        /// Groups items and counts each group
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<(T Item, uint Count)> GroupAndCount<T>(this IEnumerable<T> items) => items
            .GroupBy(d => d)
            .Select(g => (g.Key, (uint)g.Count()))
        ;

        /// <summary>
        /// Formats groups of items
        /// </summary>
        /// <param name="items"></param>
        /// <param name="separator">Group separator</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string Format<T>(this IEnumerable<(T Item, uint Count)> items, char separator = ';') where T: notnull =>
            String.Join(separator, items.Select(i => $"{i.Item}: {i.Count}"));

        /// <summary>
        /// Enables or disables legacy (version 2) binary serialization - only when reading
        /// </summary>
        /// <param name="context"></param>
        /// <param name="use">True to enable</param>
        public static void UseLegacySerializationInput(this IBrightDataContext context, bool use = true) => context.Set(Consts.LegacyFloatSerialisationInput, use);

        /// <summary>
        /// Creates a data encoder
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DataEncoder GetDataEncoder(this IBrightDataContext context) => new(context);

        /// <summary>
        /// Converts the object to a serialized buffer
        /// </summary>
        /// <param name="writable"></param>
        /// <returns></returns>
        public static byte[] GetData(this ICanWriteToBinaryWriter writable)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writable.WriteTo(writer);
            writer.Flush();
            return stream.ToArray();
        }

        /// <summary>
        /// Notifies about the progress of a multi part operation
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="index">Index of current part</param>
        /// <param name="total">Total number of parts</param>
        /// <param name="progress">Process within the part</param>
        public static void NotifyProgress(this INotifyUser? notify, uint index, uint total, float progress) => notify?.OnOperationProgress((float) index / total + progress / total);

        /// <summary>
        /// Writes a progress bar to the console
        /// </summary>
        /// <param name="progress">New progress (between 0 and 1)</param>
        /// <param name="previousPercentage">Current progress percentage (max 100)</param>
        /// <param name="sw">Stopwatch since start of operation</param>
        /// <returns>True if the progress has increased</returns>
        public static bool WriteProgressPercentage(this float progress, ref int previousPercentage, Stopwatch sw) => ConsoleProgressNotification.WriteProgress(progress, ref previousPercentage, sw);

        /// <summary>
        /// Writes the enumerable to a comma separated string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Items to write</param>
        public static string AsCommaSeparated<T>(this IEnumerable<T> items)
        {
            var sb = new StringBuilder();
            sb.Append('[');
            var isFirst = true;
            foreach (var item in items) {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(", ");
                sb.Append(item);
            }
            sb.Append(']');
            return sb.ToString();
        }
    }
}
