using System;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightTable;
using BrightTable.Input;
using BrightTable.Segments;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new BrightDataContext();
            using var table = context.ParseCsvFile(@"C:\data\iris.data", true);

            //using var stream = new MemoryStream();
            //var metaData = new MetaData();
            //using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
            //    StructColumn<float>.WriteHeader(ColumnType.Float, 32768, metaData, writer);
            //    StructColumn<float>.WriteData(Enumerable.Repeat(1f, 32768).ToArray(), writer);
            //}
            //stream.Seek(0, SeekOrigin.Begin);
            //using var inputData = new InputData(stream);
            //var inputReader = new InputBufferReader(inputData, 0, (uint)stream.Length);
            //var column = new StructColumn<float>(inputReader);

            //float total = 0f;
            //foreach(var item in column.EnumerateTyped())
            //    total += item;

            //using var context = new BrightDataContext();
            //using var stream = new MemoryStream();
            //var writer = new BinaryWriter(stream);
            //var encoder = new DataEncoder(context);

            //DataEncoder.Write(writer, new decimal[] {1, 2, 3});
            //stream.Seek(0, SeekOrigin.Begin);
            //var reader = new BinaryReader(stream);
            //var data = encoder.ReadArray<decimal>(reader);

            //var indexList = WeightedIndexList.Create(context, (1, 1f), (2, 2f), (3, 3f));

            //indexList.WriteTo(writer);
            //writer.Flush();
            //stream.Seek(0, SeekOrigin.Begin);
            //var reader = new BinaryReader(stream);
            //var indexList2 = WeightedIndexList.ReadFrom(context, reader);
        }
    }
}
