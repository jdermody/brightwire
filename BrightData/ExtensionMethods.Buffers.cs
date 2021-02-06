using System;
using System.IO;
using System.Text;
using BrightData.Buffer;
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
        public static IHybridBuffer<T> CreateHybridStructBuffer<T>(this IBrightDataContext _, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) where T : struct =>
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
        public static IHybridBuffer CreateHybridStructBuffer(this IBrightDataContext _, Type type, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) =>
            tempStream.CreateHybridStructBuffer(type, bufferSize, maxDistinct);

        /// <summary>
        /// Creates a string buffer
        /// </summary>
        /// <param name="_"></param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <param name="maxDistinct">Maximum number of distinct items (to encode)</param>
        /// <returns></returns>
        public static IHybridBuffer<string> CreateHybridStringBuffer(this IBrightDataContext _, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) =>
            tempStream.CreateHybridStringBuffer(bufferSize, maxDistinct);

        /// <summary>
        /// Creates an object buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <returns></returns>
        public static IHybridBuffer<T> CreateHybridObjectBuffer<T>(this IBrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768) where T : notnull =>
            tempStream.CreateHybridObjectBuffer<T>(context, bufferSize);

        /// <summary>
        /// Creates an object buffer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type">Type of objects</param>
        /// <param name="tempStream">Temp stream provider</param>
        /// <param name="bufferSize">Max items to cache in memory</param>
        /// <returns></returns>
        public static IHybridBuffer CreateHybridObjectBuffer(this IBrightDataContext context, Type type, IProvideTempStreams tempStream, uint bufferSize = 32768) =>
            tempStream.CreateHybridObjectBuffer(context, type, bufferSize);

        /// <summary>
        /// Returns a reader that buffers items in memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">Bright data context</param>
        /// <param name="stream">Stream to read from</param>
        /// <param name="inMemorySize">Number of bytes to use as an in memory buffer</param>
        /// <returns></returns>
        public static ICanEnumerate<T> GetReader<T>(this IBrightDataContext context, Stream stream, uint inMemorySize) where T : notnull
        {
            var reader = new BinaryReader(stream, Encoding.UTF8);
            var type = (HybridBufferType)reader.ReadByte();

            if (type == HybridBufferType.EncodedString)
                return (ICanEnumerate<T>)new BufferedStreamReader.StringDecoder(reader, stream, inMemorySize);

            if (type == HybridBufferType.EncodedStruct)
                return GenericActivator.Create<ICanEnumerate<T>>(typeof(BufferedStreamReader.StructDecoder<>).MakeGenericType(typeof(T)), reader, stream, inMemorySize);

            if (type == HybridBufferType.Object)
                return GenericActivator.Create<ICanEnumerate<T>>(typeof(BufferedStreamReader.ObjectReader<>).MakeGenericType(typeof(T)), context, reader, stream, inMemorySize);

            if (type == HybridBufferType.String)
                return (ICanEnumerate<T>)new BufferedStreamReader.StringReader(reader, stream, inMemorySize);

            if (type == HybridBufferType.Struct)
                return GenericActivator.Create<ICanEnumerate<T>>(typeof(BufferedStreamReader.StructReader<>).MakeGenericType(typeof(T)), reader, stream, inMemorySize);

            throw new NotImplementedException();
        }
    }
}
