using System;
using System.ComponentModel;
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
            var table = context.ParseCsv(@"C:\data\malware\train.csv", true, ',', @"c:\temp\table.dat", true);

            //using(var reader = new StreamReader(@"C:\data\iris.data")) {
            //    var parser =new CsvParser2(reader, ',', true);
            //    foreach(var line in parser.Parse()) {
            //        Console.WriteLine(String.Join(", ", line));
            //    }
            //}

            //using var context = new BrightDataContext();
            //using var table = context.ParseCsv(@"C:\data\iris.data", false);
            //var table2 = table.Convert(ColumnConversion.ToNumeric, ColumnConversion.ToNumeric, ColumnConversion.ToNumeric, ColumnConversion.ToNumeric);
            //var mutatedTable = table2.CreateMutateContext()
            //    .Add<float>(0, x => x * 2)
            //    .Add<float>(1, x => x * 3)
            //    .Mutate();

            //var table3 = mutatedTable.AsRowOriented();
            //var baggedTable = table3.Bag(1000);

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
