using System;
using System.Collections.Generic;
using System.Text;
using BrightTable.Segments;

namespace BrightTable.Input
{
    class CsvParser : IDisposable
    {
        class Line
        {
            readonly List<string> _cells = new List<string>();

            public void Add(string cell) => _cells.Add(cell);
            public int CellCount => _cells.Count;

            public string Get(int index)
            {
                if (index < _cells.Count)
                    return _cells[index];
                return string.Empty;
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

        public void Parse(FileReader reader, bool hasHeader, Action<long> progressCallback = null, int lineCount = int.MaxValue)
        {
            var sb = new StringBuilder();
            var inQuote = false;
            Line curr = null;
            long percent = 0;

            while (reader.CanRead && _lineCount < lineCount) {
                var ch = reader.GetNext();
                if (ch == '\0')
                    break;

                if (ch == '"')
                    inQuote = !inQuote;
                else if ((ch == '\r' || ch == '\n') && !inQuote) {
                    _AddLine(ref curr, hasHeader, sb);
                    curr = null;
                    continue;
                } else if (inQuote && ch == _delimiter && char.IsWhiteSpace(_delimiter))
                    inQuote = false;

                if (ch == _delimiter && !inQuote)
                    _AddCell(ref curr, sb);
                else
                    sb.Append(ch);

                if (progressCallback != null) {
                    var newPercent = reader.ProgressPercent;
                    if (newPercent > percent)
                        progressCallback(percent = newPercent);
                }
            }

            _AddLine(ref curr, hasHeader, sb);
        }

        void _AddCell(ref Line line, StringBuilder sb)
        {
            var cell = sb.ToString();
            if (cell.Length > 0) {
                if (line == null)
                    line = new Line();
                line.Add(cell);
                sb.Clear();
            }
        }

        void _AddLine(ref Line line, bool hasHeader, StringBuilder sb)
        {
            _AddCell(ref line, sb);
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
                        column.Header = text;
                    else
                        column.Add(text);
                }

                ++_lineCount;
            }
        }
    }
}
