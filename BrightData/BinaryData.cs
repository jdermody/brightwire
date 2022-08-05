using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BrightData
{
    /// <summary>
    /// Blob of binary data
    /// </summary>
    public record struct BinaryData(byte[] Data) : ICanWriteToBinaryWriter, ICanInitializeFromBinaryReader
    {
        /// <inheritdoc />
        public void Initialize(BrightDataContext context, BinaryReader reader)
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
    }
}
