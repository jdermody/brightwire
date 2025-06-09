using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Buffer.Composite;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Helper
{
    /// <summary>
    /// Simple CSV parser
    /// </summary>
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
                        while ((_columnData ??= []).Count <= _columnIndex)
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
                        while ((_columnData ??= []).Count <= _columnIndex)
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

                            while ((Columns ??= []).Count <= j)
                                Columns.Add(new StringCompositeBuffer(parser._tempStreams, parser._blockSize, parser._maxBlockSize, parser._maxInMemoryBlocks, parser._maxDistinctItems));

                            // set the column name if needed
                            if (_isFirstRow && parser._firstRowIsHeader)
                                Columns[j].MetaData.SetName(text.Trim());
                            else
                                Columns[j].Append(text);
                            sb.Clear();
                        }

                        // finish any columns that might have been missed
                        if (Columns?.Count > _columnIndex + 1) {
                            for (var j = _columnIndex + 1; j < Columns.Count; j++) {
                                if (_isFirstRow && parser._firstRowIsHeader)
                                    Columns[j].MetaData.SetName(string.Empty);
                                else
                                    Columns[j].Append(string.Empty);
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
        readonly IProvideByteBlocks? _tempStreams;
        readonly int _blockSize, _maxBlockSize;
        readonly uint? _maxInMemoryBlocks, _maxDistinctItems;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="firstRowIsHeader">True if the first row is a header</param>
        /// <param name="delimiter">Column delimiter character</param>
        /// <param name="quote">Quote character</param>
        /// <param name="tempStreams">Temp stream provider (optional)</param>
        /// <param name="blockSize">Block size in bytes</param>
        /// <param name="maxBlockSize"></param>
        /// <param name="maxInMemoryBlocks">Max number of blocks to keep in memory</param>
        /// <param name="maxDistinctItems">Max number of distinct items to track</param>
        public CsvParser(
            bool firstRowIsHeader,
            char delimiter,
            char quote = '"',
            IProvideByteBlocks? tempStreams = null,
            int blockSize = Consts.DefaultInitialBlockSize,
            int maxBlockSize = Consts.DefaultMaxBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        )
        {
            _firstRowIsHeader  = firstRowIsHeader;
            _delimiter         = delimiter;
            _quote             = quote;
            _tempStreams       = tempStreams;
            _blockSize         = blockSize;
            _maxBlockSize      = maxBlockSize;
            _maxInMemoryBlocks = maxInMemoryBlocks;
            _maxDistinctItems  = maxDistinctItems;
        }

        /// <summary>
        /// Progress notification
        /// </summary>
        public Action<float>? OnProgress { get; set; }

        /// <summary>
        /// Completion notification
        /// </summary>
        public Action? OnComplete { get; set; }

        /// <summary>
        /// Parses CSV from a stream reader
        /// </summary>
        /// <param name="reader">Stream reader</param>
        /// <param name="maxLines">Max number of lines to read</param>
        /// <param name="ct"></param>
        /// <returns></returns>
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
            parseState.FinishLine([], true);

            // signal complete
            OnComplete?.Invoke();
            return parseState.Columns;
        }

        /// <summary>
        /// Parses CSV from a string
        /// </summary>
        /// <param name="str">String to parse</param>
        /// <param name="maxLines">Max number of lines to read</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public List<ICompositeBuffer<string>>? Parse(string str, uint maxLines = uint.MaxValue, CancellationToken ct = default)
        {
            var parseState = new ParseState(this);
            parseState.Parse(str, maxLines, ct);
            return parseState.Columns;
        }
    }
}
