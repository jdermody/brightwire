﻿using BenchmarkDotNet.Attributes;
using System.Reflection;
using BrightData;
using BrightData.Parquet;
using Parquet;
using Parquet.Data;

namespace Benchmarks
{
    public class DataTableBenchmarks
    {
        Stream _housePricesParquet, _housePricesDataTable;
        BrightDataContext _dataContext;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _dataContext = new BrightDataContext();
            _housePricesParquet = Assembly.GetExecutingAssembly().GetManifestResourceStream("Benchmarks.Data.house-price.parquet") ?? throw new InvalidOperationException("Not able to find embedded resource");
            _housePricesDataTable = Assembly.GetExecutingAssembly().GetManifestResourceStream("Benchmarks.Data.house-price.dat") ?? throw new InvalidOperationException("Not able to find embedded resource");
        }

        public async Task SaveDataTable(string outputPath)
        {
            using var dataTable = await _dataContext.LoadParquetDataTableFromStream(_housePricesParquet);
            await dataTable.WriteTo(outputPath);
        }

        [Benchmark]
        public async Task Parquet()
        {
            using var reader = await ParquetReader.CreateAsync(_housePricesParquet);
            var schema = reader.Schema;
            var count = 0U;
            for(var i = 0; i < reader.RowGroupCount; i++) {
                using var rowGroupReader = reader.OpenRowGroupReader(i);
                Count(await rowGroupReader.ReadColumnAsync(schema.DataFields[2]), 3, ref count);
            }
            //Console.WriteLine(count);

            static void Count(DataColumn column, long filter, ref uint count)
            {
                foreach (var bedrooms in column.AsSpan<long?>()) {
                    if (bedrooms == filter)
                        ++count;
                }
            }
        }

        [Benchmark]
        public async Task AdaptedParquet()
        {
            using var dataTable = await _dataContext.LoadParquetDataTableFromStream(_housePricesParquet);
            var count = 0U;
            var bedrooms = dataTable.GetColumn<long>(2);
            await bedrooms.ForEachBlock(x => {
                foreach (var b in x) {
                    if (b == 3)
                        ++count;
                }
            });
            //Console.WriteLine(count);
        }

        [Benchmark]
        public async Task DataTable()
        {
            using var dataTable = await _dataContext.LoadTableFromStream(_housePricesDataTable, false);
            var count = 0U;
            var bedrooms = dataTable.GetColumn<long>(2);
            await bedrooms.ForEachBlock(x => {
                foreach (var b in x) {
                    if (b == 3)
                        ++count;
                }
            });
        }
    }
}
