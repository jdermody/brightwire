using System;
using System.IO;
using BrightData.Buffer.Composite;
using BrightData.Buffer.EncodedStream;
using BrightData.Helper;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a buffer to store structs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempStream"></param>
        /// <param name="bufferSize"></param>
        /// <param name="maxDistinct"></param>
        /// <returns></returns>
        public static ICompositeBuffer<T> CreateCompositeStructBuffer<T>(this IProvideTempStreams tempStream, uint bufferSize = Consts.DefaultInMemoryBufferSize, ushort maxDistinct = Consts.DefaultMaxDistinctCount)
            where T : struct
        {
            return GenericActivator.Create<ICompositeBuffer<T>>(typeof(CompositeStructBuffer<>).MakeGenericType(typeof(T)),
                tempStream,
                bufferSize,
                maxDistinct
            );
        }

        /// <summary>
        /// Creates a buffer to store structs
        /// </summary>
        /// <param name="tempStream"></param>
        /// <param name="type"></param>
        /// <param name="bufferSize"></param>
        /// <param name="maxDistinct"></param>
        /// <returns></returns>
        public static ICompositeBuffer CreateCompositeStructBuffer(this IProvideTempStreams tempStream, Type type, uint bufferSize = Consts.DefaultInMemoryBufferSize, ushort maxDistinct = Consts.DefaultMaxDistinctCount)
        {
            return GenericActivator.Create<ICompositeBuffer>(typeof(CompositeStructBuffer<>).MakeGenericType(type),
                tempStream,
                bufferSize,
                maxDistinct
            );
        }

        /// <summary>
        /// Creates a buffer to store strings
        /// </summary>
        /// <param name="tempStream"></param>
        /// <param name="bufferSize"></param>
        /// <param name="maxDistinct"></param>
        /// <returns></returns>
        public static ICompositeBuffer<string> CreateCompositeStringBuffer(this IProvideTempStreams tempStream, uint bufferSize = Consts.DefaultInMemoryBufferSize, ushort maxDistinct = Consts.DefaultMaxDistinctCount)
        {
            return GenericActivator.Create<ICompositeBuffer<string>>(typeof(CompositeStringBuffer),
                tempStream,
                bufferSize,
                maxDistinct
            );
        }

        /// <summary>
        /// Creates a buffer to store objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempStream"></param>
        /// <param name="context"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static ICompositeBuffer<T> CreateCompositeObjectBuffer<T>(this IProvideTempStreams tempStream, BrightDataContext context, uint bufferSize = Consts.DefaultInMemoryBufferSize) where T : notnull
        {
            return GenericActivator.Create<ICompositeBuffer<T>>(typeof(CompositeObjectBuffer<>).MakeGenericType(typeof(T)),
                context,
                tempStream,
                bufferSize
            ) ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Creates a buffer to store objects
        /// </summary>
        /// <param name="tempStream"></param>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static ICompositeBuffer CreateCompositeObjectBuffer(this IProvideTempStreams tempStream, BrightDataContext context, Type type, uint bufferSize = Consts.DefaultInMemoryBufferSize)
        {
            return GenericActivator.Create<ICompositeBuffer>(typeof(CompositeObjectBuffer<>).MakeGenericType(type),
                context,
                tempStream,
                bufferSize
            );
        }

        /// <summary>
        /// Creates a struct buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <param name="maxDistinct">Maximum number of distinct items (to encode)</param>
        /// <returns></returns>
        public static ICompositeBuffer<T> CreateCompositeStructBuffer<T>(this BrightDataContext _, IProvideTempStreams tempStream, uint bufferSize = Consts.DefaultInMemoryBufferSize, ushort maxDistinct = Consts.DefaultMaxDistinctCount) where T : struct =>
            tempStream.CreateCompositeStructBuffer<T>(bufferSize, maxDistinct);

        /// <summary>
        /// Creates a struct buffer
        /// </summary>
        /// <param name="_"></param>
        /// <param name="type">Type of structs</param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <param name="maxDistinct">Maximum number of distinct items (to encode)</param>
        /// <returns></returns>
        public static ICompositeBuffer CreateCompositeStructBuffer(this BrightDataContext _, Type type, IProvideTempStreams tempStream, uint bufferSize = Consts.DefaultInMemoryBufferSize, ushort maxDistinct = Consts.DefaultMaxDistinctCount) =>
            tempStream.CreateCompositeStructBuffer(type, bufferSize, maxDistinct);

        /// <summary>
        /// Creates a string buffer
        /// </summary>
        /// <param name="_"></param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <param name="maxDistinct">Maximum number of distinct items (to encode)</param>
        /// <returns></returns>
        public static ICompositeBuffer<string> CreateCompositeStringBuffer(this BrightDataContext _, IProvideTempStreams tempStream, uint bufferSize = Consts.DefaultInMemoryBufferSize, ushort maxDistinct = Consts.DefaultMaxDistinctCount) =>
            tempStream.CreateCompositeStringBuffer(bufferSize, maxDistinct);

        /// <summary>
        /// Creates an object buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <returns></returns>
        public static ICompositeBuffer<T> CreateCompositeObjectBuffer<T>(this BrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = Consts.DefaultInMemoryBufferSize) where T : notnull =>
            tempStream.CreateCompositeObjectBuffer<T>(context, bufferSize);

        /// <summary>
        /// Creates an object buffer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type">Type of objects</param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <returns></returns>
        public static ICompositeBuffer CreateCompositeObjectBuffer(this BrightDataContext context, Type type, IProvideTempStreams tempStream, uint bufferSize = Consts.DefaultInMemoryBufferSize) =>
            tempStream.CreateCompositeObjectBuffer(context, type, bufferSize);

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
            var type = (CompositeBufferType)reader.ReadByte();

            return type switch {
                CompositeBufferType.EncodedString => (ICanEnumerateWithSize<T>)new EncodedStreamReader.StringDecoder(reader, stream, inMemorySize),
                CompositeBufferType.EncodedStruct => GenericActivator.Create<ICanEnumerateWithSize<T>>(typeof(EncodedStreamReader.StructDecoder<>).MakeGenericType(typeof(T)), reader, stream, inMemorySize),
                CompositeBufferType.Object        => GenericActivator.Create<ICanEnumerateWithSize<T>>(typeof(EncodedStreamReader.ObjectReader<>).MakeGenericType(typeof(T)), context, reader, stream, inMemorySize),
                CompositeBufferType.String        => (ICanEnumerateWithSize<T>)new EncodedStreamReader.StringReader(reader, stream, inMemorySize),
                CompositeBufferType.Struct        => GenericActivator.Create<ICanEnumerateWithSize<T>>(typeof(EncodedStreamReader.StructReader<>).MakeGenericType(typeof(T)), reader, stream, inMemorySize),
                _                                 => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Copies all values from a tensor segment into a float buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="segment"></param>
        public static void CopyFrom(this ICompositeBuffer<float> buffer, INumericSegment<float> segment)
        {
            for(uint i = 0, len = segment.Size; i < len; i++)
                buffer.Add(segment[i]);
        }

        /// <summary>
        /// Copies all values from a span into a float buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="span"></param>
        public static void CopyFrom(this ICompositeBuffer<float> buffer, ReadOnlySpan<float> span)
        {
            for(int i = 0, len = span.Length; i < len; i++)
                buffer.Add(span[i]);
        }

        /// <summary>
        /// Creates a mutable typed segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">Bright data context</param>
        /// <param name="metaData">Segment meta data</param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static (ITableSegment<T> Segment, ICompositeBufferWithMetaData<T> Buffer) GetSegmentWithCompositeBuffer<T>(this BrightDataContext context, MetaData metaData, ICompositeBuffer<T> buffer) where T : notnull
        {
            var ret = new CompositeBufferSegment<T>(context, typeof(T).GetBrightDataType(), metaData, buffer);
            return (ret, ret);
        }
    }
}
