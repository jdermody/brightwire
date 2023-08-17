﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BrightData.Buffer.Composite
{
    /// <summary>
    /// A composite buffer of structs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CompositeStructBuffer<T> : CompositeBufferBase<T>
        where T : struct
    {
        public CompositeStructBuffer(IProvideTempStreams tempStream, uint blockSize, ushort maxDistinct) : base(tempStream, blockSize, maxDistinct)
        {
        }

        protected override void WriteTo(ReadOnlySpan<T> ptr, Stream stream)
        {
            var bytes = MemoryMarshal.Cast<T, byte>(ptr);
            stream.Write(bytes);
        }

        protected override uint ReadTo(Stream stream, uint count, T[] buffer)
        {
            var ptr = MemoryMarshal.Cast<T, byte>(buffer);
            var len = stream.Read(ptr);
            var sizeOfT = ptr.Length / buffer.Length;
            return (uint)len / (uint)sizeOfT;
        }
    }
}
