using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BrightData.Helper
{
    /// <summary>
    /// Tensor shape
    /// </summary>
    public class ShapedBase : ICanWriteToBinaryWriter
    {
        /// <summary>
        /// Array of sizes
        /// </summary>
        public uint[] Shape { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shape">Array of sizes</param>
        protected ShapedBase(uint[] shape)
        {
            Shape = shape;
        }

        /// <summary>
        /// Total size of the shape
        /// </summary>
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

        /// <summary>
        /// Number of sizes within the shape
        /// </summary>
        public uint Rank => (uint)Shape.Length;

        /// <inheritdoc />
        public virtual void WriteTo(BinaryWriter writer)
        {
            writer.Write(Shape.Length);
            foreach (var item in Shape)
                writer.Write(item);
        }

        /// <summary>
        /// Reads the shape from a binary reader
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <returns>Size of tensor</returns>
        protected uint ReadFrom(BinaryReader reader)
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

        /// <summary>
        /// Works out the shape from a possibly incomplete list of sizes
        /// </summary>
        /// <param name="total">Total size</param>
        /// <param name="shape">List of sizes that form the shape (one can be null)</param>
        /// <returns></returns>
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
