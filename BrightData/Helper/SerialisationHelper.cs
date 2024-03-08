using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace BrightData.Helper
{
    /// <summary>
    /// Serialisation helpers
    /// </summary>
    public static class SerialisationHelper
    {
        /// <summary>
        /// Writes the string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="writer"></param>
        public static void WriteTo(this string? str, BinaryWriter writer) => writer.Write(str ?? "");

        /// <summary>
        /// Writes the integer
        /// </summary>
        /// <param name="val"></param>
        /// <param name="writer"></param>
        public static void WriteTo(this int val, BinaryWriter writer) => writer.Write(val);

        /// <summary>
        /// Writes the uint
        /// </summary>
        /// <param name="val"></param>
        /// <param name="writer"></param>
        public static void WriteTo(this uint val, BinaryWriter writer) => writer.Write((int)val);

        /// <summary>
        /// Writes the double
        /// </summary>
        /// <param name="val"></param>
        /// <param name="writer"></param>
        public static void WriteTo(this double val, BinaryWriter writer) => writer.Write(val);

        /// <summary>
        /// Writes the float
        /// </summary>
        /// <param name="val"></param>
        /// <param name="writer"></param>
        public static void WriteTo(this float val, BinaryWriter writer) => writer.Write(val);

        /// <summary>
        /// Conditionally writes a nullable value
        /// </summary>
        /// <param name="val"></param>
        /// <param name="writer"></param>
        /// <param name="onWrite"></param>
        /// <typeparam name="T"></typeparam>
        public static void WriteTo<T>(this T? val, BinaryWriter writer, Action<T> onWrite) where T : struct
        {
            writer.Write(val.HasValue);
            if (val.HasValue)
                onWrite(val.Value);
        }

        /// <summary>
        /// Conditionally reads a nullable value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="onRead"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? ReadNullable<T>(this BinaryReader reader, Func<T> onRead) where T : struct
        {
            if (reader.ReadBoolean())
                return onRead();
            return null;
        }

        /// <summary>
        /// Writes the collection of items
        /// </summary>
        /// <param name="list"></param>
        /// <param name="writer"></param>
        public static void WriteTo(this IReadOnlyCollection<ICanWriteToBinaryWriter>? list, BinaryWriter writer)
        {
            writer.Write(list?.Count ?? 0);
            if (list?.Count > 0)
            {
                foreach (var item in list)
                    item.WriteTo(writer);
            }
        }

        /// <summary>
        /// Writes the array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="writer"></param>
        /// <typeparam name="T"></typeparam>
        public static void WriteTo<T>(this T[]? array, BinaryWriter writer) where T : struct
        {
            writer.Write(array?.Length ?? 0);
            if (array?.Length > 0)
                writer.Write(MemoryMarshal.AsBytes(array.AsSpan()));
        }

        /// <summary>
        /// Writes an array of arrays to a binary writer
        /// </summary>
        /// <param name="arrayOfArrays"></param>
        /// <param name="writer"></param>
        /// <typeparam name="T"></typeparam>
        public static void WriteTo<T>(this T[][]? arrayOfArrays, BinaryWriter writer) where T : struct
        {
            writer.Write(arrayOfArrays?.Length ?? 0);
            writer.Write(arrayOfArrays?.Length > 0 ? arrayOfArrays[0].Length : 0);

            if (arrayOfArrays?.Length > 0 && arrayOfArrays[0].Length > 0)
            {
                foreach (var array in arrayOfArrays)
                    writer.Write(MemoryMarshal.AsBytes(array.AsSpan()));
            }
        }

        /// <summary>
        /// Reads an array of arrays from a binary reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T[][] ReadArrayOfArrays<T>(this BinaryReader reader) where T : struct
        {
            var count = reader.ReadInt32();
            var size = reader.ReadInt32();
            var ret = new T[count][];
            for (var i = 0; i < count; i++)
                ret[i] = reader.BaseStream.ReadArray<T>(size);
            return ret;
        }

        /// <summary>
        /// Writes the array of strings
        /// </summary>
        /// <param name="array"></param>
        /// <param name="writer"></param>
        public static void WriteTo(this string[]? array, BinaryWriter writer)
        {
            writer.Write(array?.Length ?? 0);
            if (array?.Length > 0)
            {
                foreach (var str in array)
                    writer.Write(str);
            }
        }

        /// <summary>
        /// Creates a new object after reading its serialized data from the reader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]T>(
            this BrightDataContext context, 
            BinaryReader reader)
            where T : ICanInitializeFromBinaryReader
        {
            T ret;
            if (typeof(T) == typeof(IVector<float>))
                ret = GenericActivator.CreateUninitialized<T>(context.LinearAlgebraProvider.VectorType);
            else if (typeof(T) == typeof(IMatrix<float>))
                ret = GenericActivator.CreateUninitialized<T>(context.LinearAlgebraProvider.MatrixType);
            else if (typeof(T) == typeof(ITensor3D<float>))
                ret = GenericActivator.CreateUninitialized<T>(context.LinearAlgebraProvider.Tensor3DType);
            else if (typeof(T) == typeof(ITensor4D<float>))
                ret = GenericActivator.CreateUninitialized<T>(context.LinearAlgebraProvider.Tensor4DType);
            else
                ret = GenericActivator.CreateUninitialized<T>();
            ret.Initialize(context, reader);
            return ret;
        }

        /// <summary>
        /// Reads an array of objects from the reader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="context"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ReadObjectArray<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]T>(
            this BinaryReader reader, 
            BrightDataContext context)
            where T : ICanInitializeFromBinaryReader
        {
            var len = reader.ReadInt32();
            var ret = new T[len];
            for (var i = 0; i < len; i++)
                ret[i] = context.Create<T>(reader);
            return ret;
        }

        /// <summary>
        /// Reads an array of structs from the reader
        /// </summary>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ReadStructArray<T>(this BinaryReader reader)
            where T : struct
        {
            var len = reader.ReadInt32();
            return reader.BaseStream.ReadArray<T>(len);
        }

        /// <summary>
        /// Reads an array of strings from the reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string[] ReadStringArray(this BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new string[len];
            for (var i = 0; i < len; i++)
                ret[i] = reader.ReadString();
            return ret;
        }
    }
}
