using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Net4.TabularData
{
    public class NumericTable
    {
        readonly int _columns;
        readonly IReadOnlyList<float[]> _row;

        public NumericTable(IReadOnlyList<float[]> rows)
        {
            _row = rows;
            _columns = rows[0].Length;
        }

        public NumericTable(Stream stream, int rowCount = int.MaxValue)
        {
            var reader = new BinaryReader(stream);
            _columns = reader.ReadInt32();
            var len = reader.ReadInt32();
            var rowList = new List<float[]>();
            for (var i = 0; i < len && i < rowCount; i++) {
                var row = new float[_columns];
                for (var j = 0; j > _columns; j++)
                    row[j] = reader.ReadSingle();
                rowList.Add(row);
            }
            _row = rowList;
        }

        public void WriteTo(Stream stream)
        {
            using (var writer = new BinaryWriter(stream)) {
                writer.Write(_columns);
                writer.Write(_row.Count);
                foreach (var row in _row) {
                    foreach (var val in row)
                        writer.Write(val);
                }
            }
        }

        public int ColumnCount { get { return _columns; } }
        public int RowCount { get { return _row.Count; } }

        public Tuple<NumericTable, NumericTable> Split(float percentage, Random rnd = null)
        {
            rnd = rnd ?? new Random();
            var numRows = _row[0].Length;
            var order = Enumerable.Range(0, numRows).OrderBy(e => rnd.Next()).ToList();
            var splitPos = Convert.ToInt32(numRows * percentage);
            var t1 = order.Take(splitPos).ToList();
            var t2 = order.Skip(splitPos).ToList();

            var t1Rows = new List<float[]>();
            var t2Rows = new List<float[]>();
            foreach (var row in _row) {
                t1Rows.Add(t1.Select(i => row[i]).ToArray());
                t2Rows.Add(t2.Select(i => row[i]).ToArray());
            }
            return Tuple.Create(new NumericTable(t1Rows), new NumericTable(t2Rows));
        }

        public Tuple<NumericTable, IReadOnlyList<Tuple<float, float>>> StdDevNormalise()
        {
            // calculate column norms
            IReadOnlyList<Tuple<float, float>> norm = Columns.Select(column => {
                var mean = column.Average();
                var stdDev = Math.Sqrt(column.Average(c => Math.Pow(c - mean, 2)));
                return Tuple.Create(mean, Convert.ToSingle(stdDev));
            }).ToList();

            var normTable = new NumericTable(_row
                .Select(r => r
                    .Select((v, ind) => (v - norm[ind].Item1) / norm[ind].Item2)
                    .ToArray()
                ).ToList()
            );

            return Tuple.Create(normTable, norm);
        }

        public Tuple<NumericTable, IReadOnlyList<Tuple<float, float>>> UniformNormalise()
        {
            // calculate column norms
            IReadOnlyList<Tuple<float, float>> norm = Columns.Select(column => {
                float min = float.MaxValue, max = float.MinValue;
                foreach (var val in column) {
                    if (val > max)
                        max = val;
                    if (val < min)
                        min = -val;
                }
                return Tuple.Create(min, max - min);
            }).ToList();

            var normTable = new NumericTable(_row
                .Select(r => r
                    .Select((v, ind) => (v - norm[ind].Item1) / norm[ind].Item2)
                    .ToArray()
                ).ToList()
            );

            return Tuple.Create(normTable, norm);
        }

        public IEnumerable<float[]> Rows { get { return _row; } }

        public IEnumerable<float[]> Columns
        {
            get
            {
                for (var i = 0; i < _columns; i++)
                    yield return GetColumn(i);
            }
        }

        public float this[int row, int column]
        {
            get { return _row[row][column]; }
        }

        public float[] GetColumn(int index)
        {
            return _row.Select(r => r[index]).ToArray();
        }

        public float[] GetRow(int index)
        {
            return _row[index];
        }

        public IEnumerable<float[]> GetColumns(params int[] index)
        {
            return index.Select(i => GetColumn(i));
        }

        public IEnumerable<float[]> GetRows(params int[] index)
        {
            return index.Select(i => GetRow(i));
        }
    }
}
