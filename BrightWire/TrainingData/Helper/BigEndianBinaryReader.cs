using System;
using System.IO;

namespace BrightWire.TrainingData.Helper
{
    /// <summary>
    /// Binary reader for big endian streams
    /// </summary>
    public class BigEndianBinaryReader : BinaryReader
    {
        byte[]? _a16;
        byte[]? _a32;
        byte[]? _a64;

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
            _a32 = ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_a32);
            return BitConverter.ToInt32(_a32, 0);
        }

        /// <summary>
        /// Reads a short
        /// </summary>
        public override Int16 ReadInt16()
        {
            _a16 = ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_a16);
            return BitConverter.ToInt16(_a16, 0);
        }

        /// <summary>
        /// Reads a long
        /// </summary>
        public override Int64 ReadInt64()
        {
            _a64 = ReadBytes(8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_a64);
            return BitConverter.ToInt64(_a64, 0);
        }

        /// <summary>
        /// Reads a ulong
        /// </summary>
        public override UInt32 ReadUInt32()
        {
            _a32 = ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_a32);
            return BitConverter.ToUInt32(_a32, 0);
        }
    }
}
