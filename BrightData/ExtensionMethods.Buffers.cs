﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrightData.Buffers;
using BrightData.Helper;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        public static IHybridBuffer<T> CreateHybridStructBuffer<T>(this IBrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) where T : struct =>
            StaticBuffers.CreateHybridStructBuffer<T>(tempStream, bufferSize, maxDistinct);
        public static IHybridBuffer CreateHybridStructBuffer(this IBrightDataContext context, Type type, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) =>
            StaticBuffers.CreateHybridStructBuffer(tempStream, type, bufferSize, maxDistinct);
        public static IHybridBuffer<string> CreateHybridStringBuffer(this IBrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024) =>
            StaticBuffers.CreateHybridStringBuffer(tempStream, bufferSize, maxDistinct);
        public static IHybridBuffer<T> CreateHybridObjectBuffer<T>(this IBrightDataContext context, IProvideTempStreams tempStream, uint bufferSize = 32768) =>
            StaticBuffers.CreateHybridObjectBuffer<T>(tempStream, context, bufferSize);
        public static IHybridBuffer CreateHybridObjectBuffer(this IBrightDataContext context, Type type, IProvideTempStreams tempStream, uint bufferSize = 32768) =>
            StaticBuffers.CreateHybridObjectBuffer(tempStream, context, type, bufferSize);

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
