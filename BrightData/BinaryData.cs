using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace BrightData
{
    /// <summary>
    /// Blob of binary data
    /// </summary>
    public readonly struct BinaryData : 
        ICanWriteToBinaryWriter, 
        ICanInitializeFromBinaryReader, 
        IEquatable<BinaryData>, 
        IHaveDataAsReadOnlyByteSpan, 
        IHaveSize, 
        IHaveReadOnlyContiguousSpan<byte>,
        IHaveMemory<byte>
    {
        readonly byte[] _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Binary data blob</param>
        public BinaryData(params byte[] data) => _data = data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Binary data blob</param>
        public BinaryData(ReadOnlySpan<byte> data) => _data = data.ToArray();

        /// <summary>
        /// Returns the data as a span
        /// </summary>
        public ReadOnlySpan<byte> Data => _data.AsSpan();

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
            writer.Write(_data.Length);
            writer.Write(Data);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var binaryHash = SHA512.HashData(Data);

            var sb = new StringBuilder();
            for (var i = 0; i < binaryHash.Length; i++) {
                sb.Append($"{binaryHash[i]:X2}");
                if (i % 4 == 3)
                    sb.Append(' ');
            }

            var hash = sb.ToString();
            return $"Hash:{hash}, Size:{_data.Length:N0}";
        }

        /// <inheritdoc />
        public bool Equals(BinaryData other) => StructuralComparisons.StructuralEqualityComparer.Equals(_data, other._data);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is BinaryData other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            foreach (var item in Data)
                hashCode.Add(item);
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Binary data equality
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(BinaryData obj1, BinaryData obj2) => obj1.Equals(obj2);

        /// <summary>
        /// Binary data non equality
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(BinaryData obj1, BinaryData obj2) => !obj1.Equals(obj2);

        /// <inheritdoc />
        public ReadOnlySpan<byte> DataAsBytes => Data;

        public uint Size => (uint)_data.Length;
        public ReadOnlySpan<byte> ReadOnlySpan => Data;
        public ReadOnlyMemory<byte> ReadOnlyMemory => _data;
    }
}
