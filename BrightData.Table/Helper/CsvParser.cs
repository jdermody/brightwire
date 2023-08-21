using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Table.Buffer;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Table.Helper
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

            public void Parse(ReadOnlySpan<char> data)
            {
                var start = 0;
                for (int i = 0, len = data.Length; i < len; i++) {
                    var ch = data[i];
                    if (ch == '\n' && !_inQuote) {
                        if (_columnData != null) {
                            while(_columnData.Count <= _columnIndex)
                                _columnData.Add(new StringBuilder());
                            _columnData[_columnIndex].Append(data[start..(i - 1)]);
                            for (var j = 0; j <= _columnIndex; j++) {
                                var sb = _columnData[j];
                                while ((Columns ??= new()).Count <= j)
                                    Columns.Add(new StringCompositeBuffer(_parser._tempStreams, _parser._blockSize, _parser._maxInMemoryBlocks, _parser._maxDistinctItems));
                                if (_isFirstRow && _parser._firstRowIsHeader)
                                    Columns[j].MetaData.SetName(sb.ToString().Trim());
                                else
                                    Columns[j].Add(sb.ToString());
                                sb.Clear();
                            }

                            // finish any columns that might have been missed
                            if (Columns?.Count > (_columnIndex+1)) {
                                for (var j = _columnIndex+1; j < Columns.Count; j++) {
                                    if (_isFirstRow && _parser._firstRowIsHeader)
                                        Columns[j].MetaData.SetName(string.Empty);
                                    else
                                        Columns[j].Add(String.Empty);
                                }
                            }
                            _isFirstRow = false;
                        }
                        _columnIndex = 0;
                        start = i+1;
                    }else if (ch == _parser._delimiter && !_inQuote) {
                        while((_columnData ??= new()).Count <= _columnIndex)
                            _columnData.Add(new StringBuilder());
                        _columnData[_columnIndex++].Append(data[start..i]);
                        start = i+1;
                    }else if (ch == _parser._quote)
                        _inQuote = !_inQuote;
                }
            }
        }

        readonly bool _firstRowIsHeader;
        readonly char _delimiter, _quote;
        readonly IProvideTempStreams? _tempStreams;
        readonly int _blockSize;
        readonly uint? _maxInMemoryBlocks, _maxDistinctItems;

        public CsvParser(
            bool firstRowIsHeader,
            char delimiter, 
            char quote = '"',
            IProvideTempStreams? tempStreams = null,
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

        public async Task<List<ICompositeBuffer<string>>?> Parse(StreamReader reader, CancellationToken ct = default)
        {
            var parseState = new ParseState(this);
            using var buffer = MemoryOwner<char>.Allocate(_blockSize);
            do {
                var read = await reader.ReadBlockAsync(buffer.Memory, ct);
                parseState.Parse(buffer.Span[..read]);
            } while (!reader.EndOfStream);
            return parseState.Columns;
        }

        public List<ICompositeBuffer<string>>? Parse(string str)
        {
            var parseState = new ParseState(this);
            parseState.Parse(str);
            return parseState.Columns;
        }
    }
}
