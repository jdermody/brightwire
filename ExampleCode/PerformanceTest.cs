﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BetterConsoleTables;
using BrightData;
using BrightData.LinearAlgebra;

namespace ExampleCode
{
    internal static class PerformanceTest
    {
        public static void Run(params LinearAlgebraProvider<float>[] laps)
        {
            var headers = new ColumnHeader("Operation").ToEnumerable()
                .Concat(laps.Select(lap => new ColumnHeader(lap.ProviderName + " (μs)", Alignment.Right)))
                .ToArray();

            var table = new Table(new TableConfiguration(Style.Unicode), headers);
            AddLine("Matrix Multiply 100", table, laps.Select(lap => MatrixMultiply(lap, 100)));
            AddLine("Matrix Multiply 500", table, laps.Select(lap => MatrixMultiply(lap, 500)));
            AddLine("Matrix Multiply 1000", table, laps.Select(lap => MatrixMultiply(lap, 1000)));
            AddLine("Matrix Multiply 2000", table, laps.Select(lap => MatrixMultiply(lap, 2000)));

            AddLine("Matrix Transpose 100", table, laps.Select(lap => MatrixTranspose(lap, 100)));
            AddLine("Matrix Transpose 500", table, laps.Select(lap => MatrixTranspose(lap, 500)));
            AddLine("Matrix Transpose 1000", table, laps.Select(lap => MatrixTranspose(lap, 1000)));
            AddLine("Matrix Transpose 2000", table, laps.Select(lap => MatrixTranspose(lap, 2000)));

            AddLine("Matrix Transpose First and Multiply 100", table, laps.Select(lap => MatrixTransposeThisAndMultiply(lap, 100)));
            AddLine("Matrix Transpose First and Multiply 500", table, laps.Select(lap => MatrixTransposeThisAndMultiply(lap, 500)));
            AddLine("Matrix Transpose First and Multiply 1000", table, laps.Select(lap => MatrixTransposeThisAndMultiply(lap, 1000)));
            AddLine("Matrix Transpose First and Multiply 2000", table, laps.Select(lap => MatrixTransposeThisAndMultiply(lap, 2000)));

            AddLine("Matrix Transpose Second and Multiply 100", table, laps.Select(lap => MatrixTransposeAndMultiply(lap, 100)));
            AddLine("Matrix Transpose Second and Multiply 500", table, laps.Select(lap => MatrixTransposeAndMultiply(lap, 500)));
            AddLine("Matrix Transpose Second and Multiply 1000", table, laps.Select(lap => MatrixTransposeAndMultiply(lap, 1000)));
            AddLine("Matrix Transpose Second and Multiply 2000", table, laps.Select(lap => MatrixTransposeAndMultiply(lap, 2000)));

            AddLine("Matrix Pointwise Multiply 100", table, laps.Select(lap => MatrixPointwiseMultiply(lap, 100)));
            AddLine("Matrix Pointwise Multiply 500", table, laps.Select(lap => MatrixPointwiseMultiply(lap, 500)));
            AddLine("Matrix Pointwise Multiply 1000", table, laps.Select(lap => MatrixPointwiseMultiply(lap, 1000)));
            AddLine("Matrix Pointwise Multiply 2000", table, laps.Select(lap => MatrixPointwiseMultiply(lap, 2000)));
        }

        static void AddLine(string name, Table table, IEnumerable<TimeSpan> results)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
            table.AddRow(name.ToEnumerable()
                .Concat(results.Select(r => $"{r.TotalMicroseconds:N0}"))
                .Cast<object>()
                .ToArray()
            );
            Console.Write(table.ToString());
        }

        static TimeSpan MatrixMultiply(LinearAlgebraProvider<float> lap, uint size)
        {
            using var matrix = lap.CreateMatrix(size, size, (i, j) => (i+1) * (j+1));
            using var matrix2 = matrix.Clone();

            var sw = Stopwatch.StartNew();
            using var result = matrix.Multiply(matrix2);
            sw.Stop();
            return sw.Elapsed;
        }

        static TimeSpan MatrixTranspose(LinearAlgebraProvider<float> lap, uint size)
        {
            using var matrix = lap.CreateMatrix(size, size, (i, j) => (i+1) * (j+1));

            var sw = Stopwatch.StartNew();
            using var result = matrix.Transpose();
            sw.Stop();
            return sw.Elapsed;
        }

        static TimeSpan MatrixTransposeThisAndMultiply(LinearAlgebraProvider<float> lap, uint size)
        {
            using var matrix = lap.CreateMatrix(size, size, (i, j) => (i+1) * (j+1));
            using var matrix2 = matrix.Clone();

            var sw = Stopwatch.StartNew();
            using var result = matrix.TransposeThisAndMultiply(matrix2);
            sw.Stop();
            return sw.Elapsed;
        }

        static TimeSpan MatrixTransposeAndMultiply(LinearAlgebraProvider<float> lap, uint size)
        {
            using var matrix = lap.CreateMatrix(size, size, (i, j) => (i+1) * (j+1));
            using var matrix2 = matrix.Clone();

            var sw = Stopwatch.StartNew();
            using var result = matrix.TransposeAndMultiply(matrix2);
            sw.Stop();
            return sw.Elapsed;
        }

        static TimeSpan MatrixPointwiseMultiply(LinearAlgebraProvider<float> lap, uint size)
        {
            using var matrix = lap.CreateMatrix(size, size, (i, j) => (i+1) * (j+1));
            using var matrix2 = matrix.Clone();

            var sw = Stopwatch.StartNew();
            using var result = matrix.PointwiseMultiply(matrix2);
            sw.Stop();
            return sw.Elapsed;
        }
    }
}
