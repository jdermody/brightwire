using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterConsoleTables;
using BrightData.LinearAlegbra2;
using BrightData;

namespace ExampleCode
{
    internal static class PerformanceTest
    {
        public static void Run(params LinearAlgebraProvider[] laps)
        {
            var headers = new ColumnHeader("Operation").ToEnumerable()
                .Concat(laps.Select(lap => new ColumnHeader(lap.Name + " (ms)", Alignment.Right)))
                .ToArray();

            var table = new Table(new TableConfiguration(Style.Unicode), headers);
            AddLine("Matrix Multiply 10", table, laps.Select(lap => MatrixMultiply(lap, 10)));
            AddLine("Matrix Multiply 100", table, laps.Select(lap => MatrixMultiply(lap, 100)));
            AddLine("Matrix Multiply 300", table, laps.Select(lap => MatrixMultiply(lap, 300)));
            AddLine("Matrix Multiply 600", table, laps.Select(lap => MatrixMultiply(lap, 600)));

            Console.Write(table.ToString());
        }

        static void AddLine(string name, Table table, IEnumerable<TimeSpan> results)
        {
            table.AddRow(name.ToEnumerable()
                .Concat(results.Select(r => $"{r.TotalMilliseconds:N0}"))
                .Cast<object>()
                .ToArray()
            );
        }

        static TimeSpan MatrixMultiply(LinearAlgebraProvider lap, uint size)
        {
            using var matrix = lap.CreateMatrix(size, size);
            matrix.MapIndexedInPlace((i, j, v) => (i+1) * (j+1));
            using var matrix2 = matrix.Clone();

            var sw = Stopwatch.StartNew();
            using var result = matrix.Multiply(matrix2);
            sw.Stop();
            return sw.Elapsed;
        }
    }
}
