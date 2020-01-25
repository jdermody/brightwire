using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData.Helper;
using BrightTable.Segments;

namespace BrightTable.Input
{
    class CsvParser : IDisposable
    {
        class Line
        {
            readonly List<int> _cells = new List<int>();
            string _text;

            public void Add(StringBuilder sb)
            {
                var pos = sb.Length;
                if (_cells.Count == 0 || _cells[^1] != pos)
                    _cells.Add(pos);
            }
            public void Complete(StringBuilder sb)
            {
                Add(sb);
                _text = sb.ToString();
                sb.Clear();
            }
            public int CellCount => _cells.Count;

            public ReadOnlySpan<char> Get(int index)
            {
                if (index >= _cells.Count)
                    return string.Empty;

                var from = index == 0 ? 0 : _cells[index - 1];
                var to = _cells[index];

                ReadOnlySpan<char> ptr = _text;
                return ptr.Slice(from, to - from);
            }
        }

        readonly char _delimiter;
        readonly uint _maxRowsInMemory;
        readonly List<StringColumn> _columns = new List<StringColumn>();
        readonly TempStreamManager _tempStreams;
        uint _columnCount = 0, _lineCount = 0;

        public CsvParser(char delimiter, uint maxRowsInMemory = 1024, string tempPath = null)
        {
            _maxRowsInMemory = maxRowsInMemory;
            _tempStreams = new TempStreamManager(tempPath);
            _delimiter = delimiter;
        }

        public uint LineCount => _lineCount;
        public uint ColumnCount => _columnCount;
        public IReadOnlyList<StringColumn> Columns => _columns;

        public void Dispose()
        {
            foreach(var column in _columns)
                column.Dispose();
            _columnCount = 0;
            _columns.Clear();
            _tempStreams.Dispose();
        }

        public void Parse(IStringIterator reader, bool hasHeader, Action<long> progressCallback = null, int lineCount = int.MaxValue)
        {
            var sb = new StringBuilder();
            var inQuote = false;
            Line curr = null;
            long percent = 0;

            while (_lineCount < lineCount) {
                var ch = reader.Next();
                if (ch == '\0')
                    break;

                if (ch == '"')
                    inQuote = !inQuote;
                else if ((ch == '\r' || ch == '\n') && !inQuote) {
                    _AddLine(ref curr, hasHeader, sb);
                    if (progressCallback != null) {
                        var newPercent = reader.ProgressPercent;
                        if (newPercent > percent)
                            progressCallback(percent = newPercent);
                    }
                    continue;
                } else if (inQuote && ch == _delimiter && char.IsWhiteSpace(_delimiter))
                    inQuote = false;

                if (ch == _delimiter && !inQuote)
                    _AddCell(ref curr, sb);
                else
                    sb.Append(ch);
            }

            _AddLine(ref curr, hasHeader, sb);
        }

        void _AddCell(ref Line line, StringBuilder sb)
        {
            if (line == null)
                line = new Line();
            line.Add(sb);
        }

        void _AddLine(ref Line line, bool hasHeader, StringBuilder sb)
        {
            line.Complete(sb);
            var count = line?.CellCount;
            if (count > 0) {
                if (count > _columnCount)
                    _columnCount = (uint)count.Value;
                for (var i = _columns.Count; i < _columnCount; i++)
                    _columns.Add(new StringColumn((uint)i, _tempStreams, _lineCount, _maxRowsInMemory));

                for (var i = 0; i < _columnCount; i++) {
                    var column = _columns[i];
                    var text = line.Get(i);
                    if (hasHeader && column.Header == null)
                        column.Header = text.ToString();
                    else
                        column.Add(text);
                }

                ++_lineCount;
            }
        }
    }
}
