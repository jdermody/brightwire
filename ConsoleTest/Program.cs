using System;
using System.IO;
using BrightData;
using BrightData.Helper;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new BrightDataContext();
            using var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            var encoder = new DataEncoder(context);

            DataEncoder.Write(writer, new decimal[] {1, 2, 3});
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new BinaryReader(stream);
            var data = encoder.ReadArray<decimal>(reader);
            
            //var indexList = WeightedIndexList.Create(context, (1, 1f), (2, 2f), (3, 3f));

            //indexList.WriteTo(writer);
            //writer.Flush();
            //stream.Seek(0, SeekOrigin.Begin);
            //var reader = new BinaryReader(stream);
            //var indexList2 = WeightedIndexList.ReadFrom(context, reader);
        }
    }
}
