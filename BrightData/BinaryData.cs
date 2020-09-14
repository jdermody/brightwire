using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BrightData
{
    public class BinaryData : ICanWriteToBinaryWriter, ICanInitializeFromBinaryReader
    {
        public BinaryData(byte[] data)
        {
            Data = data;
        }

        public BinaryData(BinaryReader reader)
        {
            Initialize(null, reader);
        }

        public void Initialize(IBrightDataContext context, BinaryReader reader)
        {
            var size = reader.ReadInt32();
            Data = reader.ReadBytes(size);
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Data.Length);
            writer.Write(Data);
        }

        public byte[] Data { get; private set; }

        public override string ToString()
        {
            string hash;
            using var hasher = SHA512.Create();
            var binaryHash = hasher.ComputeHash(Data);

            var sb = new StringBuilder();
            for (var i = 0; i < binaryHash.Length; i++) {
                sb.Append($"{binaryHash[i]:X2}");
                if (i % 4 == 3)
                    sb.Append(" ");
            }

            hash = sb.ToString();

            return $"Hash:{hash}, Size:{Data.Length:N0}";
        }

        public override bool Equals(object obj)
        {
            if(obj is BinaryData other)
                return ToString() == other.ToString();
            return false;
        }

        public override int GetHashCode() => ToString().GetHashCode();
    }
}
