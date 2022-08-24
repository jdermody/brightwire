using System;
using System.IO;
using BrightData.Buffer.EncodedStream;
using BrightData.Buffer.Hybrid;
using BrightData.Helper;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a struct buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <param name="maxDistinct">Maximum number of distinct items (to encode)</param>
        /// <returns></returns>
        public static IHybridBuffer<T> CreateHybridStructBuffer<T>(this BrightDataContext _, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) where T : struct =>
            tempStream.CreateHybridStructBuffer<T>(bufferSize, maxDistinct);

        /// <summary>
        /// Creates a struct buffer
        /// </summary>
        /// <param name="_"></param>
        /// <param name="type">Type of structs</param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <param name="maxDistinct">Maximum number of distinct items (to encode)</param>
        /// <returns></returns>
        public static IHybridBuffer CreateHybridStructBuffer(this BrightDataContext _, Type type, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) =>
            tempStream.CreateHybridStructBuffer(type, bufferSize, maxDistinct);

        /// <summary>
        /// Creates a string buffer
        /// </summary>
        /// <param name="_"></param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <param name="maxDistinct">Maximum number of distinct items (to encode)</param>
        /// <returns></returns>
        public static IHybridBuffer<string> CreateHybridStringBuffer(this BrightDataContext _, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) =>
            tempStream.CreateHybridStringBuffer(bufferSize, maxDistinct);

        /// <summary>
        /// Creates an object buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <returns></returns>
        public static IHybridBuffer<T> CreateHybridObjectBuffer<T>(this BrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768) where T : notnull =>
            tempStream.CreateHybridObjectBuffer<T>(context, bufferSize);

        /// <summary>
        /// Creates an object buffer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type">Type of objects</param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <returns></returns>
        public static IHybridBuffer CreateHybridObjectBuffer(this BrightDataContext context, Type type, IProvideTempStreams tempStream, uint bufferSize = 32768) =>
            tempStream.CreateHybridObjectBuffer(context, type, bufferSize);

        /// <summary>
        /// Returns a reader that buffers items in memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">Bright data context</param>
        /// <param name="reader">Binary reader</param>
        /// <param name="inMemorySize">Number of bytes to use as an in memory buffer</param>
        /// <returns></returns>
        public static ICanEnumerateWithSize<T> GetBufferReader<T>(this BrightDataContext context, BinaryReader reader, uint inMemorySize) where T : notnull
        {
            var stream = reader.BaseStream;
            var type = (HybridBufferType)reader.ReadByte();

            return type switch {
                HybridBufferType.EncodedString => (ICanEnumerateWithSize<T>)new EncodedStreamReader.StringDecoder(reader, stream, inMemorySize),
                HybridBufferType.EncodedStruct => GenericActivator.Create<ICanEnumerateWithSize<T>>(typeof(EncodedStreamReader.StructDecoder<>).MakeGenericType(typeof(T)), reader, stream, inMemorySize),
                HybridBufferType.Object => GenericActivator.Create<ICanEnumerateWithSize<T>>(typeof(EncodedStreamReader.ObjectReader<>).MakeGenericType(typeof(T)), context, reader, stream, inMemorySize),
                HybridBufferType.String => (ICanEnumerateWithSize<T>)new EncodedStreamReader.StringReader(reader, stream, inMemorySize),
                HybridBufferType.Struct => GenericActivator.Create<ICanEnumerateWithSize<T>>(typeof(EncodedStreamReader.StructReader<>).MakeGenericType(typeof(T)), reader, stream, inMemorySize),
                _ => throw new NotImplementedException()
            };
        }

        public static void CopyFrom(this IHybridBuffer<float> buffer, ITensorSegment segment)
        {
            for(uint i = 0, len = segment.Size; i < len; i++)
                buffer.Add(segment[i]);
        }

        public static void CopyFrom(this IHybridBuffer<float> buffer, ReadOnlySpan<float> span)
        {
            for(int i = 0, len = span.Length; i < len; i++)
                buffer.Add(span[i]);
        }

        public static (ITypedSegment<T> Segment, IHybridBufferWithMetaData<T> Buffer) GetSegmentWithHybridBuffer<T>(this BrightDataContext context, MetaData metaData, IHybridBuffer<T> buffer) where T : notnull
        {
            var ret = new HybridBufferSegment<T>(context, typeof(T).GetBrightDataType(), metaData, buffer);
            return (ret, ret);
        }
    }
}
