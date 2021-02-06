﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BrightData.Input
{
    internal class CsvParser
    {
        readonly StreamReader _stream;
        readonly char _delimiter;
        readonly char[] _buffer;
        readonly char _quote;
        readonly List<StringBuilder> _columns = new List<StringBuilder>();
        int _currIndex;
        bool _inQuote;

        public CsvParser(StreamReader stream, char delimiter, char quote = '"')
        {
            _stream = stream;
            _delimiter = delimiter;
            _quote = quote;
            _buffer = new char[32768];
        }

        public Action<int>? OnProgress { get; set; }

        public IEnumerable<string[]> Parse()
        {
            while(!_stream.EndOfStream) {
                foreach(var line in ReadPage())
                    yield return line;
                OnProgress?.Invoke((int) (_stream.BaseStream.Position * 100 / _stream.BaseStream.Length));
            }
            var lastLine = GetLine();
            if(lastLine.Any(s => s.Length > 0))
                yield return lastLine;
        }

        StringBuilder GetBuilder(bool increment)
        {
            if(increment)
                _currIndex++;
            if(_columns.Count == _currIndex)
                _columns.Add(new StringBuilder());
            return _columns[_currIndex];
        }

        IEnumerable<string[]> ReadPage()
        {
            var len = _stream.Read(_buffer);
            var curr = GetBuilder(false);

            for (var i = 0; i < len; i++)
            {
                var ch = _buffer[i];
                if (ch == '\r') {
                    // ReSharper disable once RedundantJumpStatement
                    continue;
                }
                if(ch == '\n' && !_inQuote) {
                    var line = GetLine();
                    if(line.Any(s => s.Length > 0))
                        yield return line;
                    curr = GetBuilder(false);
                } else if(ch == _delimiter && !_inQuote)
                    curr = GetBuilder(true);
                else if(ch == _quote) {
                    // TODO: look for escaped quotes?
                    _inQuote = !_inQuote;
                }
                else {
                    curr.Append(ch);
                }
            }
        }

        string[] GetLine()
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