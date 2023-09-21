using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Buffer.Composite;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Input
{
    public class CsvParser
    {
        class ParseState
        {
            readonly CsvParser _parser;
            List<StringBuilder>? _columnData = null;
            bool _inQuote = false, _isFirstRow = true;
            int _columnIndex = 0;
            public List<ICompositeBuffer<string>>? Columns = null;

            public ParseState(CsvParser parser) => _parser = parser;

            public void Parse(ReadOnlySpan<char> data, uint maxLines, CancellationToken ct)
            {
                var start = 0;
                uint lineCount = 0;
                for (int i = 0, len = data.Length; i < len && !ct.IsCancellationRequested; i++) {
                    var ch = data[i];

                    // check for new line
                    if (ch == '\n' && !_inQuote) {
                        FinishLine(data[start..i]);
                        if (++lineCount == maxLines)
                            return;
                        _columnIndex = 0;
                        start = i+1;
                    }
                    
                    // check for a delimiter, such as a comma
                    else if (ch == _parser._delimiter && !_inQuote) {
                        while((_columnData ??= new()).Count <= _columnIndex)
                            _columnData.Add(new StringBuilder());
                        _columnData[_columnIndex++].Append(data[start..i]);
                        start = i+1;
                    }
                    
                    // check for quote characters
                    else if (ch == _parser._quote)
                        _inQuote = !_inQuote;
                }

                // handle case where last line might not have a new line character
                if (start < data.Length)
                    FinishLine(data[start..]);
            }
            void FinishLine(ReadOnlySpan<char> data)
            {
                if (_columnData != null) {
                    // add string builders if needed
                    while(_columnData.Count <= _columnIndex)
                        _columnData.Add(new StringBuilder());

                    // add the text to the current string builder
                    _columnData[_columnIndex].Append(data);

                    // flush the string builders to the column buffers
                    for (var j = 0; j <= _columnIndex; j++) {
                        var sb = _columnData[j];
                        while ((Columns ??= new()).Count <= j)
                            Columns.Add(new StringCompositeBuffer(_parser._tempStreams, _parser._blockSize, _parser._maxInMemoryBlocks, _parser._maxDistinctItems));

                        // set the column name if needed
                        if (_isFirstRow && _parser._firstRowIsHeader)
                            Columns[j].MetaData.SetName(sb.ToString().Trim());
                        else
                            Columns[j].Add(sb.ToString());
                        sb.Clear();
                    }

                    // finish any columns that might have been missed
                    if (Columns?.Count > _columnIndex+1) {
                        for (var j = _columnIndex+1; j < Columns.Count; j++) {
                            if (_isFirstRow && _parser._firstRowIsHeader)
                                Columns[j].MetaData.SetName(string.Empty);
                            else
                                Columns[j].Add(String.Empty);
                        }
                    }
                    if(_isFirstRow)
                        _isFirstRow = false;
                }
            }
        }

        readonly bool _firstRowIsHeader;
        readonly char _delimiter, _quote;
        readonly IProvideTempData? _tempStreams;
        readonly int _blockSize;
        readonly uint? _maxInMemoryBlocks, _maxDistinctItems;

        public CsvParser(
            bool firstRowIsHeader,
            char delimiter, 
            char quote = '"',
            IProvideTempData? tempStreams = null,
            int blockSize = Consts.DefaultBlockSize,
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) {
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

        public async Task<List<ICompositeBuffer<string>>?> Parse(StreamReader reader, uint maxLines, CancellationToken ct = default)
        {
            var parseState = new ParseState(this);
            using var buffer = MemoryOwner<char>.Allocate(_blockSize);
            do {
                var read = await reader.ReadBlockAsync(buffer.Memory, ct);
                parseState.Parse(buffer.Span[..read], maxLines, ct);

                // signal progress
                OnProgress?.Invoke((float)reader.BaseStream.Position / reader.BaseStream.Length);
            } while (!reader.EndOfStream);

            // signal complete
            OnComplete?.Invoke();
            return parseState.Columns;
        }

        public List<ICompositeBuffer<string>>? Parse(string str, uint maxLines, CancellationToken ct = default)
        {
            var parseState = new ParseState(this);
            parseState.Parse(str, maxLines, ct);
            return parseState.Columns;
        }
    }
}
