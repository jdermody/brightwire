using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightWire.TrainingData.Helper
{
    /// <summary>
    /// Binary reader for big endian streams
    /// </summary>
    public class BigEndianBinaryReader : BinaryReader
    {
        byte[] a16;
        byte[] a32;
        byte[] a64;

        /// <summary>
        /// Creates a new big endian binary reader
        /// </summary>
        /// <param name="stream"></param>
        public BigEndianBinaryReader(Stream stream) : base(stream)
        {
        }

        /// <summary>
        /// Reads an int
        /// </summary>
        public override int ReadInt32()
        {
            a32 = base.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a32);
            return BitConverter.ToInt32(a32, 0);
        }

        /// <summary>
        /// Reads a short
        /// </summary>
        public override Int16 ReadInt16()
        {
            a16 = base.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a16);
            return BitConverter.ToInt16(a16, 0);
        }

        /// <summary>
        /// Reads a long
        /// </summary>
        public override Int64 ReadInt64()
        {
            a64 = base.ReadBytes(8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a64);
            return BitConverter.ToInt64(a64, 0);
        }

        /// <summary>
        /// Reads a ulong
        /// </summary>
        public override UInt32 ReadUInt32()
        {
            a32 = base.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(a32);
            return BitConverter.ToUInt32(a32, 0);
        }
    }
}
