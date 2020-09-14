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
using BrightWire;
using BrightData.Numerics;
using BrightTable.Transformations;
using BrightData.Cuda;
using ExampleCode;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new BrightDataContext();
            Simple.Xor(context);
            
            var lap = context.UseCudaLinearAlgebra();

            //var table = context.ParseCsv(@"C:\data\plotly\processed_data.csv", true, ',', @"c:\temp\table.dat", true);
            //using var table = (IColumnOrientedDataTable)context.LoadTable(@"c:\temp\table.dat");
            //using var table2 = table.Convert(@"c:\temp\table2.dat", Enumerable.Range(0, (int)table.ColumnCount).Select(i => ColumnConversion.ToNumeric).ToArray());
            using var table = context.ParseCsv(@"c:\data\iris.data", true);
            table.SetTargetColumn(4);
            using var numericTable = table.Convert(
                ColumnConversionType.ToNumeric, 
                ColumnConversionType.ToNumeric, 
                ColumnConversionType.ToNumeric, 
                ColumnConversionType.ToNumeric, 
                ColumnConversionType.ToCategoricalIndex);
            var head = numericTable.Head(60);
            using var normalized = numericTable.Normalize(NormalizationType.Standard);
            head = normalized.Head(60);

            var rowOriented = numericTable.ToRowOriented();
            var head2 = rowOriented.Head();
            var columnOrieted = rowOriented.AsColumnOriented();

            var vectorised = normalized.Vectorise();
            head = vectorised.Head(60);


            var naiveBayes = normalized.TrainNaiveBayes().CreateClassifier();
            var normalisedConvertible = normalized.AsRowOriented().AsConvertible();
            foreach (var result in normalisedConvertible.Classify(naiveBayes)) {
                var (row, classification) = result;
            }

            // train model
            //var costFunction = new BinaryClassification();
            //var trainer = trainingTable.GetLogisticRegressionTrainer();
            //var trainingContext = trainer.CreateContext(0.1f, 0.1f);
            //for (var i = 0; i < 50000; i++) {
            //    trainingContext.Iterate();
            //    if (i % 100 == 0) {
            //        var finalModel = trainer.Evaluate();
            //        var error = finalModel.Average(costFunction.Compute);
            //        Console.WriteLine(trainer.Model.Theta);
            //        Console.WriteLine($"{i}) MSE: {error}");
            //    }
            //}



            //numericTable.SetTargetColumn(4);
            //using var trainer = numericTable.GetLogisticRegressionTrainer();
            //var trainingContext = trainer.CreateContext(0.1f, 0.1f);
            //for (var i = 0; i < 30; i++) {
            //    Console.WriteLine(trainingContext.Iterate());
            //}

            //using var table2 = table.
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
