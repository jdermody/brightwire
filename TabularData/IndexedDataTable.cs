using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData
{
    public class IndexedDataTable : DataTable, IIndexableDataTable
    {
        public const int BLOCK_SIZE = 1024;

        readonly IReadOnlyList<long> _index = new List<long>();
        readonly int _rowCount;

        public IndexedDataTable(Stream stream, IReadOnlyList<long> dataIndex, int rowCount) 
            : base(stream)
        {
            _index = dataIndex;
            _rowCount = rowCount;
        }

        //public IReadOnlyList<long> Index { get { return _index; } }

        public override int RowCount
        {
            get
            {
                return _rowCount;
            }
        }

        public IReadOnlyList<IRow> GetSlice(int offset, int count)
        {
            var ret = new List<IRow>();
            lock (_stream) {
                var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                _stream.Seek(_index[offset / BLOCK_SIZE], SeekOrigin.Begin);

                // seek to offset
                for (var i = 0; i < offset % BLOCK_SIZE; i++)
                    _SkipRow(reader);

                // read the data
                for (var i = 0; i < count && _stream.Position < _stream.Length; i++)
                    ret.Add(new DataTableRow(this, _ReadRow(reader)));
            }
            return ret;
        }

        public IReadOnlyList<IRow> GetRows(IEnumerable<int> rowIndex)
        {
            // split the queries into blocks
            HashSet<int> temp;
            var blockMatch = new Dictionary<int, HashSet<int>>();
            foreach (var row in rowIndex.OrderBy(r => r)) {
                var block = row / BLOCK_SIZE;
                if (!blockMatch.TryGetValue(block, out temp))
                    blockMatch.Add(block, temp = new HashSet<int>());
                temp.Add(row);
            }

            var ret = new List<IRow>();
            lock (_stream) {
                var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                foreach (var block in blockMatch.OrderBy(b => b.Key)) {
                    _stream.Seek(_index[block.Key], SeekOrigin.Begin);
                    var match = block.Value;

                    for (int i = block.Key * BLOCK_SIZE, len = i + BLOCK_SIZE; i < len && _stream.Position < _stream.Length; i++) {
                        if (match.Contains(i))
                            ret.Add(new DataTableRow(this, _ReadRow(reader)));
                        else
                            _SkipRow(reader);
                    }
                }
            }
            return ret;
        }

        public Tuple<IDataTable, IDataTable> Split(int? randomSeed = null, double trainPercentage = 0.8, bool shuffle = true)
        {
            var input = Enumerable.Range(0, RowCount);
            if (shuffle)
                input = input.Shuffle(randomSeed);
            var final = input.ToList();
            int trainingCount = Convert.ToInt32(RowCount * trainPercentage);

            return Tuple.Create<IDataTable, IDataTable>(
                new MemoryBasedDataTable(Columns, GetRows(final.Take(trainingCount))), 
                new MemoryBasedDataTable(Columns, GetRows(final.Skip(trainingCount)))
            );
        }
    }
}
