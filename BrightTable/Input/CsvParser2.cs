using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BrightTable.Input
{
    public class CsvParser2
    {
        readonly StreamReader _stream;
        readonly char _delimiter;
        readonly bool _hasHeader;
        readonly char[] _buffer;
        readonly char _quote;
        readonly List<StringBuilder> _columns = new List<StringBuilder>();
        int _currIndex;
        bool _inQuote;

        public CsvParser2(StreamReader stream, char delimiter, bool hasHeader, char quote = '"')
        {
            _stream = stream;
            _delimiter = delimiter;
            _hasHeader = hasHeader;
            _quote = quote;
            _buffer = new char[32768];
        }

        public IEnumerable<string[]> Parse()
        {
            while(!_stream.EndOfStream) {
                foreach(var line in _ReadPage())
                    yield return line;
            }
            var lastLine = _GetLine();
            if(lastLine.Any(s => s.Length > 0))
                yield return lastLine;
        }

        StringBuilder _GetBuilder(bool increment)
        {
            if(increment)
                _currIndex++;
            if(_columns.Count == _currIndex)
                _columns.Add(new StringBuilder());
            return _columns[_currIndex];
        }

        IEnumerable<string[]> _ReadPage()
        {
            var len = _stream.Read(_buffer);
            var curr = _GetBuilder(false);

            for (var i = 0; i < len; i++)
            {
                var ch = _buffer[i];
                if(ch == '\r')
                    continue;
                else if(ch == '\n' && !_inQuote) {
                    var line = _GetLine();
                    if(line.Any(s => s.Length > 0))
                        yield return line;
                    curr = _GetBuilder(false);
                } else if(ch == _delimiter && !_inQuote)
                    curr = _GetBuilder(true);
                else if(ch == _quote) {
                    // TODO: look for escaped quotes
                    _inQuote = !_inQuote;
                }else
                    curr.Append(ch);
            }
        }

        string[] _GetLine()
        {
            var ret = new string[_columns.Count];
            for (int i = 0, len = _columns.Count; i < len; i++)
            {
                var sb = _columns[i];
                ret[i] = sb.ToString();
                sb.Clear();
            }
            _currIndex = 0;
            return ret;
        }
    }
}
