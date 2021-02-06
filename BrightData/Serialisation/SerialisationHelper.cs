using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using BrightData.Helper;

namespace BrightData.Serialisation
{
    internal static class SerialisationHelper
    {
        public static void WriteTo(this String? str, BinaryWriter writer) => writer.Write(str ?? "");
        public static void WriteTo(this int val, BinaryWriter writer) => writer.Write(val);
        public static void WriteTo(this uint val, BinaryWriter writer) => writer.Write((int)val);
        public static void WriteTo(this double val, BinaryWriter writer) => writer.Write(val);

        public static void WriteTo<T>(this T? val, BinaryWriter writer, Action<T> onWrite) where T : struct
        {
            writer.Write(val.HasValue);
            if (val.HasValue)
                onWrite(val.Value);
        }

        public static T? ReadNullable<T>(this BinaryReader reader, Func<T> onRead) where T : struct
        {
            if (reader.ReadBoolean())
                return onRead();
            return null;
        }

        public static void WriteTo(this IReadOnlyCollection<ICanWriteToBinaryWriter>? list, BinaryWriter writer)
        {
            writer.Write(list?.Count ?? 0);
            if (list?.Count > 0)
            {
                foreach (var item in list)
                    item.WriteTo(writer);
            }
        }

        public static void WriteTo<T>(this T[]? array, BinaryWriter writer) where T : struct
        {
            writer.Write(array?.Length ?? 0);
            if (array?.Length > 0)
            {
                var bytes = MemoryMarshal.Cast<T, byte>(array);
                writer.Flush();
                writer.BaseStream.Write(bytes);
            }
        }

        public static void WriteTo(this string[]? array, BinaryWriter writer)
        {
            writer.Write(array?.Length ?? 0);
            if (array?.Length > 0)
            {
                foreach (var str in array)
                    writer.Write(str);
            }
        }

        public static T Create<T>(this IBrightDataContext context, BinaryReader reader)
            where T : ICanInitializeFromBinaryReader
        {
            var ret = GenericActivator.CreateUninitialized<T>();
            ret.Initialize(context, reader);
            return ret;
        }

        public static T[] ReadArray<T>(this BinaryReader reader, IBrightDataContext context)
            where T : ICanInitializeFromBinaryReader
        {
            var len = reader.ReadInt32();
            var ret = new T[len];
            for (var i = 0; i < len; i++)
                ret[i] = Create<T>(context, reader);
            return ret;
        }

        public static T[] ReadStructArray<T>(this BinaryReader reader)
            where T : struct
        {
            var len = reader.ReadInt32();
            var ret = new T[len];
            var bytes = MemoryMarshal.Cast<T, byte>(ret);
            reader.BaseStream.Read(bytes);
            return ret;
        }

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
