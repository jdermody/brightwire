using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BrightData
{
    /// <summary>
    /// Blob of binary data
    /// </summary>
    public class BinaryData : ICanWriteToBinaryWriter, ICanInitializeFromBinaryReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Binary data</param>
        public BinaryData(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reader">Binary reader to read binary data from</param>
        public BinaryData(BinaryReader reader)
        {
            Initialize(null, reader);
        }


        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            var size = reader.ReadInt32();
            Data = reader.ReadBytes(size);
        }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Data.Length);
            writer.Write(Data);
        }

        /// <summary>
        /// Byte array of binary data
        /// </summary>
        public byte[] Data { get; private set; }

        /// <inheritdoc />
        public override string ToString()
        {
            using var hasher = SHA512.Create();
            var binaryHash = hasher.ComputeHash(Data);

            var sb = new StringBuilder();
            for (var i = 0; i < binaryHash.Length; i++) {
                sb.Append($"{binaryHash[i]:X2}");
                if (i % 4 == 3)
                    sb.Append(" ");
            }

            var hash = sb.ToString();
            return $"Hash:{hash}, Size:{Data.Length:N0}";
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if(obj is BinaryData other)
                return ToString() == other.ToString();
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode() => ToString().GetHashCode();
    }
}
