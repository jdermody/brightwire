using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using BrightData.Buffer.Operations;
using BrightData.Converter;
using BrightData.DataTable;
using BrightData.Helper;
using BrightData.Helper.Vectors;
using BrightData.LinearAlgebra.Clustering;
using BrightData.LinearAlgebra.ReadOnly;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using BrightData.Types;

namespace BrightData
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Converts a type code to a type
        /// </summary>
        /// <param name="code">Type code</param>
        /// <returns></returns>
        public static Type ToType(this TypeCode code)
        {
            return code switch
            {
                TypeCode.Boolean  => typeof(bool),
                TypeCode.Byte     => typeof(byte),
                TypeCode.Char     => typeof(char),
                TypeCode.DateTime => typeof(DateTime),
                TypeCode.Decimal  => typeof(decimal),
                TypeCode.Double   => typeof(double),
                TypeCode.Int16    => typeof(short),
                TypeCode.Int32    => typeof(int),
                TypeCode.Int64    => typeof(long),
                TypeCode.Object   => typeof(object),
                TypeCode.SByte    => typeof(sbyte),
                TypeCode.Single   => typeof(float),
                TypeCode.String   => typeof(string),
                TypeCode.UInt16   => typeof(ushort),
                TypeCode.UInt32   => typeof(uint),
                TypeCode.UInt64   => typeof(ulong),
                _                 => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Sets a value only if the value is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetIfNotNull<T>(this MetaData metadata, string name, T? value)
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
        public static bool SetIfNotNull<T>(this MetaData metadata, string name, T? value)
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
        /// Randomly splits the sequence into two arrays (either "training" or "test")
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
        /// <param name="defaultName">Name to use if no name was set</param>
        /// <returns></returns>
        public static string GetName(this MetaData metadata, string defaultName = "") => metadata.Get(Consts.Name, defaultName);

        /// <summary>
        /// Item index
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static uint GetColumnIndex(this MetaData metadata) => metadata.Get(Consts.ColumnIndex, uint.MaxValue);

        /// <summary>
        /// True if the item is numeric
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsNumeric(this MetaData metadata) => metadata.Get(Consts.IsNumeric, false);

        /// <summary>
        /// True if the item is a target
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsTarget(this MetaData metadata) => metadata.Get(Consts.IsTarget, false);

        /// <summary>
        /// True if the item is categorical
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsCategorical(this MetaData metadata) => metadata.Get(Consts.IsCategorical, false);

        /// <summary>
        /// True if the item is sequential
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsSequential(this MetaData metadata) => metadata.Get(Consts.IsSequential, false);

        /// <summary>
        /// Writes available meta data to a new meta data store
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public static MetaData GetMetaData(this IWriteToMetaData writer)
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
        public static ICanConvert<T, float> GetFloatConverter<T>(this BrightDataContext context) where T: struct
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
        /// <param name="range"></param>
        /// <returns></returns>
        public static IEnumerable<uint> AsRange(this Range range) => Enumerable.Range(range.Start.Value, (range.End.Value - range.Start.Value)).Select(i => (uint)i);

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
        public static float Aggregate(this AggregationType operation, IEnumerable<float> data)
        {
            return operation switch {
                AggregationType.Sum => data.Sum(),
                AggregationType.Average => data.Average(),
                AggregationType.Max => data.Max(),
                AggregationType.Min => data.Min(),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Sets this as a target
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="isTarget"></param>
        /// <returns></returns>
        public static MetaData SetTarget(this MetaData metaData, bool isTarget)
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
        public static MetaData SetIsCategorical(this MetaData metaData, bool isCategorical)
        {
            metaData.Set(Consts.IsCategorical, isCategorical);
            return metaData;
        }

        /// <summary>
        /// Sets this as one hot encoded
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="isOneHotEncoded"></param>
        /// <returns></returns>
        public static MetaData SetIsOneHot(this MetaData metaData, bool isOneHotEncoded)
        {
            metaData.Set(Consts.IsCategorical, isOneHotEncoded);
            return metaData;
        }

        /// <summary>
        /// Sets the name
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="name">Name</param>
        public static MetaData SetName(this MetaData metaData, string name)
        {
            metaData.Set(Consts.Name, name);
            return metaData;
        }

        /// <summary>
        /// Returns the file path associated with the metadata (if any)
        /// </summary>
        /// <param name="metaData"></param>
        /// <returns>File path</returns>
        public static string GetFilePath(this MetaData metaData) => metaData.Get(Consts.FilePath, "");

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
        public static string Format<T>(this IEnumerable<(T Item, uint Count)> items, char separator = ';') =>
            String.Join(separator, items.Select(i => $"{i.Item}: {i.Count}"));

        /// <summary>
        /// Enables or disables legacy (version 2) binary serialization - only when reading
        /// </summary>
        /// <param name="context"></param>
        /// <param name="use">True to enable</param>
        public static void UseLegacySerializationInput(this BrightDataContext context, bool use = true) => context.Set(Consts.LegacyFloatSerialisationInput, use);

        /// <summary>
        /// Creates a data encoder
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DataEncoder GetDataEncoder(this BrightDataContext context) => new(context);

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

        /// <summary>
        /// Reads an array of T from the stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="size">Number of items to read</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T[] ReadArray<T>(this Stream stream, uint size) where T: struct
        {
            var ret       = new T[size];
            var bytesRead = stream.Read(MemoryMarshal.AsBytes(ret.AsSpan()));
#if DEBUG
            
            if (bytesRead != Unsafe.SizeOf<T>() * size)
                throw new Exception("Unexpected end of file");
#endif
            return ret;
        }

        /// <summary>
        /// Reads an array of T from the stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="size">Number of items to read</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T[] ReadArray<T>(this Stream stream, int size) where T: struct
        {
            return ReadArray<T>(stream, (uint)size);
        }

        /// <summary>
        /// Finds all possible permutations of sub items from the array, including the array itself
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Array to permute</param>
        public static IEnumerable<ImmutableList<T>> FindPermutations<T>(this T[] items)
        {
            if (items.Length == 1)
                yield return ImmutableList<T>.Empty.Add(items[0]);
            else {
                for (var pos = 0; pos < items.Length - 1; pos++) {
                    for (var size = 1; size < items.Length; size++) {
                        var prefixIndices = size.AsRange(pos).Where(i => i < items.Length);
                        var prefix = new Lazy<ImmutableList<T>>(() => [.. prefixIndices.Select(i => items[i])]);
                        for (var i = pos + size; i < items.Length; i++) {
                            yield return prefix.Value.Add(items[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates all unique pairs of items within the array
        /// [1, 2, 3] => [1,2], [1,3], [2,3]
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<(T First, T Second)> FindAllPairs<T>(this T[] items)
        {
            var len = items.Length;
            var state = new HashSet<(int, int)>();

            for (var i = 0; i < len; i++) {
                for (var j = 0; j < len; j++) {
                    if (i != j && state.Add((i, j)) && state.Add((j, i)))
                        yield return (items[i], items[j]);
                }
            }
        }

        /// <summary>
        /// Enumerates a stream as a series of structs. This is best for small structs such as int32 etc. as the values are not passed by reference.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="count">Total count to return</param>
        /// <param name="tempBufferSize">Size of temp buffer to use</param>
        /// <returns></returns>
        public static IEnumerable<T> Enumerate<T>(this Stream stream, uint count, int tempBufferSize = 8192)
            where T : struct
        {
            if (count < tempBufferSize) {
                // simple case
                var buffer = stream.ReadArray<T>(count);
                for(uint i = 0; i < count; i++)
                    yield return buffer[i];
            }
            else {
                // buffered read
                var sizeofT = Unsafe.SizeOf<T>();
                var temp = ArrayPool<T>.Shared.Rent(tempBufferSize);
                try {
                    var totalRead = 0;
                    while (totalRead < count) {
                        var remaining = count - totalRead;
                        var ptr = remaining >= tempBufferSize
                            ? temp
                            : ((Span<T>)temp)[..(int)remaining];
                        var readCount = stream.Read(MemoryMarshal.Cast<T, byte>(ptr)) / sizeofT;
                        for (var i = 0; i < readCount; i++)
                            yield return temp[i];
                        totalRead += readCount;
                    }
                }
                finally {
                    ArrayPool<T>.Shared.Return(temp);
                }
            }
        }

        /// <summary>
        /// Writes unmanaged items to a stream as a byte array
        /// </summary>
        /// <param name="items">Unmanaged items to write</param>
        /// <param name="stream">Destination stream</param>
        /// <param name="tempBufferSize">Size of buffer</param>
        /// <typeparam name="T"></typeparam>
        public static void WriteTo<T>(this IEnumerable<T> items, Stream stream, int tempBufferSize = 8192) where T: unmanaged
        {
            using var buffer = SpanOwner<T>.Allocate(tempBufferSize);
            var span = buffer.Span;
            var index = 0;

            foreach (var item in items) {
                span[index++] = item;
                if (index == buffer.Length) {
                    stream.Write(span.AsBytes());
                    index = 0;
                }
            }
            if(index > 0)
                stream.Write(span[..index].AsBytes());
        }

        /// <summary>
        /// Finds the unique ranges of indices within a sequence
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static IEnumerable<(uint First, uint Last)> FindDistinctContiguousRanges(this IEnumerable<uint> indices)
        {
            uint? prev = null, startRange = null, nextInRange = null;

            foreach (var item in indices.OrderBy(d => d)) {
                if(prev == item)
                    continue;

                if (item == nextInRange) {
                    prev = item;
                    nextInRange = item + 1;
                    continue;
                }

                if (startRange.HasValue)
                    yield return (startRange.Value, prev!.Value);

                startRange = item;
                nextInRange = item + 1;
                prev = item;
            }
            if (startRange.HasValue)
                yield return (startRange.Value, prev!.Value);
        }

        /// <summary>
        /// Creates a new table builder
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IBuildDataTables CreateTableBuilder(this BrightDataContext context) => new ColumnOrientedDataTableBuilder(context);

        /// <summary>
        /// Disposes a collection of disposables
        /// </summary>
        /// <param name="disposables"></param>
        public static void DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach(var item in disposables)
                item.Dispose();
        }

        /// <summary>
        /// Converts a single object into an enumerator that will enumerate that object once
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Item to enumerate (once)</param>
        /// <returns></returns>
        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        /// <summary>
        /// Extracts an array of floats
        /// </summary>
        /// <param name="spanOwner"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(this IHaveSpanOf<T> spanOwner)
        {
            var temp = SpanOwner<T>.Empty;
            var span = spanOwner.GetSpan(ref temp, out var wasTempUsed);
            try {
                return span.ToArray();
            }
            finally {
                if(wasTempUsed)
                    temp.Dispose();
            }
        }

        /// <summary>
        /// Copies tensor values to another tensor
        /// </summary>
        /// <param name="source">Copy from</param>
        /// <param name="target">Copy to</param>
        public static void CopyTo(this IHaveReadOnlyTensorSegment<float> source, IHaveTensorSegment<float> target)
        {
            source.ReadOnlySegment.CopyTo(target.Segment);
        }

        /// <summary>
        /// Attempts to parse a string into a date
        /// </summary>
        /// <param name="str">String that contains a valid date</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DateTime ToDateTime(this string str)
        {
            if (DateTime.TryParse(str, out var ret))
                return ret;

            var formatInfo = new DateTimeFormatInfo();
            var styles = DateTimeStyles.AllowWhiteSpaces;
            if (DateTime.TryParse(str, formatInfo, styles, out ret))
                return ret;

            // 20150225T000000
            if (DateTime.TryParseExact(str, "yyyyMMddTHHmmss", formatInfo, styles, out ret))
                return ret;

            // 20070405143000
            if (DateTime.TryParseExact(str, "yyyyMMddHHmmss", formatInfo, styles, out ret))
                return ret;

            // 200704051430
            if (DateTime.TryParseExact(str, "yyyyMMddHHmm", formatInfo, styles, out ret))
                return ret;

            // 19950204
            if (DateTime.TryParseExact(str, "yyyyMMdd", formatInfo, styles, out ret))
                return ret;

            throw new Exception($"{str} was not recognised as a valid date");
        }

        /// <summary>
        /// Re references each item in the memory as an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="block"></param>
        /// <returns></returns>
        public static ReadOnlyMemory<object> AsObjects<T>(this ReadOnlyMemory<T> block) where T: notnull
        {
            var index = 0;
            var ret = new object[block.Length];
            foreach (ref readonly var item in block.Span)
                ret[index++] = item;
            return ret;
        }

        static (Type, uint) GetTypeAndSize<T>() => (typeof(T), (uint)Unsafe.SizeOf<T>());

        /// <summary>
        /// Executes multiple operations as one
        /// </summary>
        /// <param name="operations"></param>
        /// <param name="notify"></param>
        /// <param name="msg"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task ExecuteAllAsOne(this IOperation[] operations, INotifyOperationProgress? notify = null, string? msg = null, CancellationToken ct = default)
        {
            if (operations.Length == 1)
                return operations[0].Execute(notify, msg, ct);
            if (operations.Length > 1)
                return new AggregateOperation(operations).Execute(notify, msg, ct);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes the async enumerable to a memory block
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static async Task<ReadOnlyMemory<T>> ToMemory<T>(this IAsyncEnumerable<T> enumerable)
        {
            using var buffer = new ArrayPoolBufferWriter<T>();
            await foreach (var item in enumerable) {
                buffer.GetSpan(1)[0] = item;
                buffer.Advance(1);
            }
            return buffer.WrittenMemory;
        }

        /// <summary>
        /// Writes items from an async enumerable to an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> enumerable, uint size)
        {
            var ret = new T[size];
            var index = 0;
            await foreach (var item in enumerable) {
                if (index >= ret.Length)
                    break;
                ret[index++] = item;
            }
            return ret;
        }

        /// <summary>
        /// Writes items from an async enumerable to a list
        /// </summary>
        /// <param name="items"></param>
        /// <param name="ct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items, CancellationToken ct = default)
        {
            var results = new List<T>();
            await foreach (var item in items.WithCancellation(ct))
                results.Add(item);
            return results;
        }

        /// <summary>
        /// Returns the first item from the enumerator (or null if no item is available)
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T?> FirstOrDefault<T>(this IAsyncEnumerable<T> items)
        {
            await using var enumerator = items.GetAsyncEnumerator();
            if (await enumerator.MoveNextAsync()) {
                return enumerator.Current;
            }

            return default;
        }

        /// <summary>
        /// Returns the first item from the enumerator
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> First<T>(this IAsyncEnumerable<T> items)
        {
            await using var enumerator = items.GetAsyncEnumerator();
            if (await enumerator.MoveNextAsync())
                return enumerator.Current;
            throw new Exception("Enumerator contained no elements");
        }

        /// <summary>
        /// Converts an async enumerable of multidimensional arrays to a list of float arrays
        /// </summary>
        /// <param name="vectorData"></param>
        /// <returns></returns>
        public static async Task<List<float[]>> ToFloatVectors(this IAsyncEnumerable<float[,]> vectorData)
        {
            var ret = new List<float[]>();
            await foreach (var vector in vectorData)
                AddRows(vector, ret);
            return ret;

            static void AddRows(Span2D<float> data, List<float[]> output)
            {
                for(var i = 0; i < data.Height; i++)
                    output.Add(data.GetRowSpan(i).ToArray());
            }
        }

        /// <summary>
        /// Converts an async enumerable of multidimensional arrays to a buffer of vectors
        /// </summary>
        /// <param name="vectorData"></param>
        /// <returns></returns>
        public static async Task<IReadOnlyBufferWithMetaData<ReadOnlyVector<float>>> ToVectors(this IAsyncEnumerable<float[,]> vectorData)
        {
            var ret = (ICompositeBuffer<ReadOnlyVector<float>>)BrightDataType.Vector.CreateCompositeBuffer();
            await foreach (var vector in vectorData)
                AddRows(vector, ret);
            return ret;

            static void AddRows(Span2D<float> data, ICompositeBuffer<ReadOnlyVector<float>> output)
            {
                for(var i = 0; i < data.Height; i++)
                    output.Append(new ReadOnlyVector<float>(data.GetRowSpan(i).ToArray()));
            }
        }

        /// <summary>
        /// Converts an async enumerable of multidimensional arrays to a buffer of index lists
        /// </summary>
        /// <param name="vectorData"></param>
        /// <returns></returns>
        public static async Task<IReadOnlyBufferWithMetaData<IndexList>> ToIndexLists(this IAsyncEnumerable<float[,]> vectorData)
        {
            var ret = (ICompositeBuffer<IndexList>)BrightDataType.IndexList.CreateCompositeBuffer();
            await foreach (var vector in vectorData)
                AddRows(vector, ret);
            return ret;

            static void AddRows(Span2D<float> data, ICompositeBuffer<IndexList> output)
            {
                for(var i = 0; i < data.Height; i++)
                    output.Append(new IndexList(data.GetRowSpan(i)));
            }
        }

        /// <summary>
        /// Converts an async enumerable of multidimensional arrays to a buffer of weighted index lists
        /// </summary>
        /// <param name="vectorData"></param>
        /// <returns></returns>
        public static async Task<IReadOnlyBufferWithMetaData<WeightedIndexList>> ToWeightedIndexLists(this IAsyncEnumerable<float[,]> vectorData)
        {
            var ret = (ICompositeBuffer<WeightedIndexList>)BrightDataType.IndexList.CreateCompositeBuffer();
            await foreach (var vector in vectorData)
                AddRows(vector, ret);
            return ret;

            static void AddRows(Span2D<float> data, ICompositeBuffer<WeightedIndexList> output)
            {
                for(var i = 0; i < data.Height; i++)
                    output.Append(new WeightedIndexList(data.GetRowSpan(i)));
            }
        }

        /// <summary>
        /// Creates a byte block source from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IByteBlockSource AsDataBlock(this Stream stream, Guid? id = null) => new StreamDataBlock(id ?? Guid.NewGuid(), stream);

        /// <summary>
        /// Creates a hierarchical clustering strategy
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        public static IClusteringStrategy NewHierarchicalClustering(this BrightDataContext _) => new Hierarchical();

        /// <summary>
        /// Creates a k means clustering strategy
        /// </summary>
        /// <param name="context"></param>
        /// <param name="maxIterations"></param>
        /// <returns></returns>
        public static IClusteringStrategy NewKMeansClustering(this BrightDataContext context, uint maxIterations = 1000) => new KMeans(context, maxIterations);

        /// <summary>
        /// Creates a new Non-Negative Matrix Factorisation clustering strategy
        /// </summary>
        /// <param name="context"></param>
        /// <param name="numIterations"></param>
        /// <param name="errorThreshold"></param>
        /// <param name="costFunction"></param>
        /// <returns></returns>
        public static IClusteringStrategy NewNNMFClustering(this BrightDataContext context, uint numIterations, float errorThreshold = 0.001f, ICostFunction<float>? costFunction = null) =>
            new NonNegativeMatrixFactorisation(context.LinearAlgebraProvider, numIterations, errorThreshold, costFunction);

        /// <summary>
        /// Hierarchical clustering successively finds the closest distance between pairs of centroids until k is reached
        /// </summary>
        /// <param name="data">The list of vectors to cluster</param>
        /// <param name="k">The number of clusters to find</param>
        /// <param name="metric"></param>
        /// <returns>A list of k clusters</returns>
        public static uint[][] HierarchicalCluster(this IReadOnlyVector<float>[] data, uint k, DistanceMetric metric = DistanceMetric.Euclidean)
        {
            var clusterer = new Hierarchical();
            return clusterer.Cluster(data, k, metric);
        }

        /// <summary>
        /// K Means uses coordinate descent and a distance metric between randomly selected centroids to cluster the data
        /// </summary>
        /// <param name="data">The list of vectors to cluster</param>
        /// <param name="context">Bright data context</param>
        /// <param name="k">The number of clusters to find</param>
        /// <param name="maxIterations">The maximum number of iterations</param>
        /// <param name="distanceMetric">Distance metric to use to compare centroids</param>
        /// <returns>A list of k clusters</returns>
        public static uint[][] KMeansCluster(this IReadOnlyVector<float>[] data, BrightDataContext context, uint k, uint maxIterations = 1000, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            var kmeans = new KMeans(context, maxIterations);
            return kmeans.Cluster(data, k, distanceMetric);
        }

        /// <summary>
        /// Returns the distance between a query vector and the selected vectors in the vector store
        /// </summary>
        /// <param name="vectors">Vectors</param>
        /// <param name="vectorIndices">Vector indices within the store to use</param>
        /// <param name="query">Query vector</param>
        /// <param name="distanceMetric">Distance metric</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static unsafe SpanOwner<T> FindDistancesFromVectorsToQuery<T>(this IReadOnlyVectorStore<T> vectors, ReadOnlySpan<uint> vectorIndices, ReadOnlySpan<T> query, DistanceMetric distanceMetric)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            var len = query.Length;
            var ret = SpanOwner<T>.Allocate(vectorIndices.Length);
            fixed(T* qPtr = query)
            fixed (T* retPtr = ret.Span) {
                var r = retPtr;
                var q = qPtr;
                vectors.ForEach(vectorIndices, (x, vi, i) => {
                    r[i] = x.FindDistance(new ReadOnlySpan<T>(q, len), distanceMetric);
                });
            }
            return ret;
        }

        /// <summary>
        /// Invokes a 
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="callback"></param>
        /// <param name="ct"></param>
        /// <typeparam name="T"></typeparam>
        public static void ForEach<T>(this IReadOnlyVectorStore<T> vectors, IndexedSpanCallback<T> callback, CancellationToken ct = default)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            vectors.ForEach((x, _) => callback(x), ct);
        }

        /// <summary>
        /// Passes each vector to the callback, possible in parallel
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="indices"></param>
        /// <param name="callback"></param>
        public static void ForEach<T>(this IReadOnlyVectorStore<T> vectors, ReadOnlySpan<uint> indices, IndexedSpanCallbackWithVectorIndex<T> callback)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            vectors.ForEach(indices, (x, i, _) => callback(x, i));
        }

        /// <summary>
        /// Passes each vector to the callback, possible in parallel
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="indices"></param>
        /// <param name="callback"></param>
        public static void ForEach<T>(this IReadOnlyVectorStore<T> vectors, ReadOnlySpan<uint> indices, IndexedSpanCallback<T> callback)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            vectors.ForEach(indices, (x, _, _) => callback(x));
        }

        /// <summary>
        /// Enumerates all values in the array, in sorted order
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="AT"></typeparam>
        /// <typeparam name="WT"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> EnumerateValues<T, AT, WT>(this AT array)
            where AT : IFixedSizeSortedArray<T, WT>
        {
            for (uint i = 0, len = array.Size; i < len; i++)
                yield return array[i].Value;
        }

        /// <summary>
        /// Creates a KD search tree from the vectors in the vector store
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static ISupportKnnSearch<T> KDTreeSearch<T>(this IReadOnlyVectorStore<T> vectors)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            return new VectorKDTree<T>(vectors);
        }

        /// <summary>
        /// Creates a ball search tree from the vectors in the vector store
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vectors"></param>
        /// <param name="distanceMetric"></param>
        /// <returns></returns>
        public static ISupportKnnSearch<T> BallTreeSearch<T>(this IReadOnlyVectorStore<T> vectors, DistanceMetric distanceMetric = DistanceMetric.Cosine)
            where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            return new VectorBallTree<T>(vectors, distanceMetric);
        }

        /// <summary>
        /// Binary search for a sorted list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <param name="comparer"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static int BinarySearch<K, V>(this SortedList<K, V> list, K value, Func<K, K, int> comparer)
            where K : notnull
        {
            var lower = 0;
            var upper = list.Count - 1;
            var keys = list.Keys;

            while (lower <= upper)
            {
                var middle = lower + (upper - lower) / 2;
                var comparisonResult = comparer(value, keys[middle]);
                switch (comparisonResult) {
                    case < 0:
                        upper = middle - 1;
                        break;
                    case > 0:
                        lower = middle + 1;
                        break;
                    default:
                        return middle;
                }
            }

            return ~lower;
        }

        /// <summary>
        /// Binary search for a sorted list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static int BinarySearch<K, V>(this SortedList<K, V> list, K value)
            where K : notnull
        {
            return BinarySearch(list, value, Comparer<K>.Default);
        }

        /// <summary>
        /// Binary search for a sorted list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <param name="comparer"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static int BinarySearch<K, V>(this SortedList<K, V> list, K value, IComparer<K> comparer)
            where K : notnull
        {
            return list.BinarySearch(value, comparer.Compare);
        }
    }
}
