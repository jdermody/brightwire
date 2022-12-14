using System;
using BrightData.Buffer.Composite;
using BrightData.Helper;

namespace BrightData
{
    /// <summary>
    /// Static methods to create composite buffers
    /// </summary>
    public static class CompositeBuffersExtensionMethods
    {
        /// <summary>
        /// Creates a buffer to store structs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempStream"></param>
        /// <param name="bufferSize"></param>
        /// <param name="maxDistinct"></param>
        /// <returns></returns>
        public static ICompositeBuffer<T> CreateCompositeStructBuffer<T>(this IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024)
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
        public static ICompositeBuffer CreateCompositeStructBuffer(this IProvideTempStreams tempStream, Type type, uint bufferSize = 32768, ushort maxDistinct = 1024)
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
        public static ICompositeBuffer<string> CreateCompositeStringBuffer(this IProvideTempStreams tempStream, uint bufferSize = 32768, ushort maxDistinct = 1024)
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
        public static ICompositeBuffer<T> CreateCompositeObjectBuffer<T>(this IProvideTempStreams tempStream, BrightDataContext context, uint bufferSize = 32768) where T : notnull
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
        public static ICompositeBuffer CreateCompositeObjectBuffer(this IProvideTempStreams tempStream, BrightDataContext context, Type type, uint bufferSize = 32768)
        {
            return GenericActivator.Create<ICompositeBuffer>(typeof(CompositeObjectBuffer<>).MakeGenericType(type),
                context,
                tempStream,
                bufferSize
            );
        }
    }
}
