using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Buffer.Composite;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Helper
{
    public class CsvParser
    {
        class ParseState(CsvParser parser)
        {
            List<StringBuilder>? _columnData = null;
            bool _inQuote = false, _isFirstRow = true, _hasData = false;
            int _columnIndex = 0;
            public List<ICompositeBuffer<string>>? Columns = null;

            public void Parse(ReadOnlySpan<char> data, uint maxLines, CancellationToken ct)
            {
                var start = 0;
                uint lineCount = 0;
                for (int i = 0, len = data.Length; i < len && !ct.IsCancellationRequested; i++)
                {
                    var ch = data[i];

                    // check for new line
                    if (ch == '\n' && !_inQuote)
                    {
                        FinishLine(data[start..i], false);
                        if (++lineCount == maxLines)
                            return;
                        start = i + 1;
                    }

                    // check for a delimiter, such as a comma
                    else if (ch == parser._delimiter && !_inQuote)
                    {
                        while ((_columnData ??= new()).Count <= _columnIndex)
                            _columnData.Add(new StringBuilder());
                        _columnData[_columnIndex++].Append(data[start..i]);
                        start = i + 1;
                        _hasData = true;
                    }

                    // check for quote characters
                    else if (ch == parser._quote)
                        _inQuote = !_inQuote;
                }

                // handle case where last line might not have a new line character
                if (start < data.Length)
                    FinishLine(data[start..], true);
            }

            public void ParseLine(ReadOnlySpan<char> line)
            {
                var start = 0;
                for (int i = 0, len = line.Length; i < len; i++)
                {
                    var ch = line[i];

                    // check for new line
                    if (ch == '\n' && !_inQuote)
                    {
                        FinishLine(line[start..i], false);
                        start = i + 1;
                    }

                    // check for a delimiter, such as a comma
                    else if (ch == parser._delimiter && !_inQuote)
                    {
                        while ((_columnData ??= new()).Count <= _columnIndex)
                            _columnData.Add(new StringBuilder());
                        _columnData[_columnIndex++].Append(line[start..i]);
                        start = i + 1;
                        _hasData = true;
                    }

                    // check for quote characters
                    else if (ch == parser._quote)
                        _inQuote = !_inQuote;
                }
                if (start < line.Length)
                    FinishLine(line[start..], false);
            }

            public void FinishLine(ReadOnlySpan<char> data, bool force)
            {
                if (_columnData != null)
                {
                    // add string builders if needed
                    while (_columnData.Count <= _columnIndex)
                        _columnData.Add(new StringBuilder());

                    // add the text to the current string builder
                    if (data.Length > 0) {
                        _columnData[_columnIndex].Append(data);
                        if(_inQuote)
                            _columnData[_columnIndex].Append('\n');
                        _hasData = true;
                    }

                    if (_hasData && (force || !_inQuote)) {
                        // flush the string builders to the column buffers
                        for (var j = 0; j <= _columnIndex; j++) {
                            var sb = _columnData[j];
                            var text = sb.ToString();
                            if (text.StartsWith(parser._quote) && text.EndsWith(parser._quote))
                                text = text[1..^1];

                            while ((Columns ??= new()).Count <= j)
                                Columns.Add(new StringCompositeBuffer(parser._tempStreams, parser._blockSize, parser._maxInMemoryBlocks, parser._maxDistinctItems));

                            // set the column name if needed
                            if (_isFirstRow && parser._firstRowIsHeader)
                                Columns[j].MetaData.SetName(text.Trim());
                            else
                                Columns[j].Add(text);
                            sb.Clear();
                        }

                        // finish any columns that might have been missed
                        if (Columns?.Count > _columnIndex + 1) {
                            for (var j = _columnIndex + 1; j < Columns.Count; j++) {
                                if (_isFirstRow && parser._firstRowIsHeader)
                                    Columns[j].MetaData.SetName(string.Empty);
                                else
                                    Columns[j].Add(string.Empty);
                            }
                        }
                        if (_isFirstRow)
                            _isFirstRow = false;
                        _columnIndex = 0;
                        _hasData = false;
                    }
                }
            }
        }

        readonly bool _firstRowIsHeader;
        readonly char _delimiter, _quote;
        readonly IProvideDataBlocks? _tempStreams;
        readonly int _blockSize;
        readonly uint? _maxInMemoryBlocks, _maxDistinctItems;

        public CsvParser(
            bool firstRowIsHeader,
            char delimiter,
            char quote = '"',
            IProvideDataBlocks? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        )
        {
            _firstRowIsHeader = firstRowIsHeader;
            _delimiter = delimiter;
            _quote = quote;
            _tempStreams = tempStreams;
            _blockSize = blockSize;
            _maxInMemoryBlocks = maxInMemoryBlocks;
            _maxDistinctItems = maxDistinctItems;
        }

        public Action<float>? OnProgress { get; set; }
        public Action? OnComplete { get; set; }

        public async Task<List<ICompositeBuffer<string>>?> Parse(StreamReader reader, uint maxLines = uint.MaxValue, CancellationToken ct = default)
        {
            var parseState = new ParseState(this);
            using var buffer = MemoryOwner<char>.Allocate(_blockSize);
            var lineCount = 0;
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync(ct);
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                parseState.ParseLine(line);

                // signal progress
                OnProgress?.Invoke((float)reader.BaseStream.Position / reader.BaseStream.Length);

                if (++lineCount == maxLines)
                    break;
            }
            parseState.FinishLine(ReadOnlySpan<char>.Empty, true);

            // signal complete
            OnComplete?.Invoke();
            return parseState.Columns;
        }

        public List<ICompositeBuffer<string>>? Parse(string str, uint maxLines = uint.MaxValue, CancellationToken ct = default)
        {
            var parseState = new ParseState(this);
            parseState.Parse(str, maxLines, ct);
            return parseState.Columns;
        }
    }
}
