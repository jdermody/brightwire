using System;
using BrightData.Helper;

namespace BrightData.Buffer
{
    /// <summary>
    /// Static methods to create hybrid buffers
    /// </summary>
    public static class StaticBuffers
    {
        /// <summary>
        /// Creates a buffer to store structs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempStream"></param>
        /// <param name="bufferSize"></param>
        /// <param name="maxDistinct"></param>
        /// <returns></returns>
        public static IHybridBuffer<T> CreateHybridStructBuffer<T>(this IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024)
            where T: struct
        {
            return GenericActivator.Create<IHybridBuffer<T>>(typeof(StructHybridBuffer<>).MakeGenericType(typeof(T)),
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
        public static IHybridBuffer CreateHybridStructBuffer(this IProvideTempStreams tempStream, Type type, uint bufferSize = 32768, ushort maxDistinct = 1024)
        {
            return GenericActivator.Create<IHybridBuffer>(typeof(StructHybridBuffer<>).MakeGenericType(type),
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
        public static IHybridBuffer<string> CreateHybridStringBuffer(this IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024)
        {
            return GenericActivator.Create<IHybridBuffer<string>>(typeof(StringHybridBuffer),
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
        public static IHybridBuffer<T> CreateHybridObjectBuffer<T>(this IProvideTempStreams tempStream, IBrightDataContext context, uint bufferSize = 32768) where T : notnull
        {
            return GenericActivator.Create<IHybridBuffer<T>>(typeof(ObjectHybridBuffer<>).MakeGenericType(typeof(T)),
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
        public static IHybridBuffer CreateHybridObjectBuffer(this IProvideTempStreams tempStream, IBrightDataContext context, Type type, uint bufferSize = 32768)
        {
            return GenericActivator.Create<IHybridBuffer>(typeof(ObjectHybridBuffer<>).MakeGenericType(type),
                context,
                tempStream,
                bufferSize
            );
        }
    }
}
