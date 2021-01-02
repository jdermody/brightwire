using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BrightData.Helper
{
    public class ShapedBase
    {
        public uint[] Shape { get; private set; }

        public ShapedBase(uint[] shape)
        {
            Shape = shape;
        }

        public uint Size
        {
            get
            {
                uint ret = 1;
                foreach (var item in Shape)
                    ret *= item;
                return ret;
            }
        }
        public uint Rank => (uint)Shape.Length;

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Shape.Length);
            foreach (var item in Shape)
                writer.Write(item);
        }

        public uint ReadFrom(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            Shape = new uint[len];
            uint size = 0;
            for (var i = 0; i < len; i++)
            {
                var item = reader.ReadUInt32();
                size = i == 0 ? item : size * item;
                Shape[i] = item;
            }

            return size;
        }

        protected static uint[] _ResolveShape(uint total, uint?[] shape)
        {
            uint nonNullTotal = 0;
            bool hasFoundNull = false;
            foreach (var item in shape)
            {
                if (item.HasValue)
                    nonNullTotal += item.Value;
                else if (!hasFoundNull)
                    hasFoundNull = true;
                else
                    throw new ArgumentException("Only one parameter can be null");
            }

            if (hasFoundNull && nonNullTotal == 0)
                throw new ArgumentException("Cannot resolve null parameter");

            return shape.Select(v => v ?? total / nonNullTotal).ToArray();
        }
    }
}
