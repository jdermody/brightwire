using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace BrightData
{
    /// <summary>
    /// Blob of binary data
    /// </summary>
    public struct BinaryData : ICanWriteToBinaryWriter, ICanInitializeFromBinaryReader, IEquatable<BinaryData>
    {
        readonly byte[] _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Binary data blob</param>
        public BinaryData(byte[] data)
        {
            _data = data;
        }

        public byte[] Data => _data;

        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var size = reader.ReadInt32();
            ref var array = ref Unsafe.AsRef(_data);
            array = reader.ReadBytes(size);
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Data.Length);
            writer.Write(Data);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            using var hasher = SHA512.Create();
            var binaryHash = hasher.ComputeHash(Data);

            var sb = new StringBuilder();
            for (var i = 0; i < binaryHash.Length; i++) {
                sb.Append($"{binaryHash[i]:X2}");
                if (i % 4 == 3)
                    sb.Append(' ');
            }

            var hash = sb.ToString();
            return $"Hash:{hash}, Size:{Data.Length:N0}";
        }

        /// <inheritdoc />
        public bool Equals(BinaryData other) => StructuralComparisons.StructuralEqualityComparer.Equals(Data, other.Data);

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is BinaryData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
    }
}
