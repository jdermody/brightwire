using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BrightData.Input
{
    internal class CsvParser
    {
        readonly StreamReader _stream;
        readonly char _delimiter, _quote;
        readonly char[] _buffer;
        readonly List<StringBuilder> _columns = new();
        int _currIndex;
        bool _inQuote;

        public CsvParser(StreamReader stream, char delimiter, char quote = '"', int bufferSize = 32768)
        {
            _stream = stream;
            _delimiter = delimiter;
            _quote = quote;
            _buffer = new char[bufferSize];
        }

        public Action<float>? OnProgress { get; set; }
        public Action? OnComplete { get; set; }

        public IEnumerable<List<StringBuilder>> Parse(CancellationToken ct = default)
        {
            while(!_stream.EndOfStream) {
                if (ct.IsCancellationRequested) 
                    yield break;

                foreach(var line in ReadPage())
                    yield return line;

                // signal progress
                OnProgress?.Invoke((float)_stream.BaseStream.Position / _stream.BaseStream.Length);
            }
            // check for the last line that may not have an end of line character
            if (_columns.Any(c => c.Length > 0))
                yield return _columns;

            // signal complete
            OnComplete?.Invoke();
        }

        StringBuilder GetBuilder(bool increment)
        {
            if(increment)
                _currIndex++;
            if(_columns.Count == _currIndex)
                _columns.Add(new StringBuilder());
            return _columns[_currIndex];
        }

        IEnumerable<List<StringBuilder>> ReadPage()
        {
            var len = _stream.Read(_buffer);
            var curr = GetBuilder(false);

            for (var i = 0; i < len; i++) {
                var ch = _buffer[i];
                switch (ch) {
                    case '\r':
                        continue;

                    case '\n' when !_inQuote:
                        if (_columns.Any(c => c.Length > 0)) {
                            yield return _columns;
                            foreach (var c in _columns)
                                c.Clear();
                        }
                        _currIndex = 0;
                        curr = GetBuilder(false);
                        break;

                    default:
                        if(ch == _delimiter && !_inQuote)
                            curr = GetBuilder(true);
                        else if(ch == _quote)
                            _inQuote = !_inQuote; // TODO: look for escaped quotes?
                        else
                            curr.Append(ch);
                        break;
                }
            }
        }
    }
}
