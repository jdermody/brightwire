using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightTable.Buffers;
using BrightTable.Builders;
using BrightTable.Input;

namespace BrightTable.Segments
{
    class StringColumn : IDisposable, ICanWriteToBinaryWriter
    {
        readonly StringBuffer _buffer;
        readonly TempStreamManager _tempStreamManager;
        readonly uint _maxRowsInMemory;
        readonly Dictionary<string, uint> _unique = new Dictionary<string, uint>();
        Stream _stream = null;
        BinaryWriter _writer = null;
        bool _hasReadFromStream = false;
        bool _hasExceededUniqueLimit = false;

        public StringColumn(uint columnIndex, TempStreamManager tempStreamManager, uint initialEmptyLines, uint maxRowsInMemory)
        {
            _maxRowsInMemory = maxRowsInMemory;
            ColumnIndex = columnIndex;
            _buffer = new StringBuffer(initialEmptyLines);
            if (initialEmptyLines > 0)
                _unique.Add(String.Empty, initialEmptyLines);
            _tempStreamManager = tempStreamManager;
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }

        public uint ColumnIndex { get; }
        public string Header { get; set; } = null;
        public bool HasUnique => !_hasExceededUniqueLimit && _unique.Any(d => d.Value > 1);
        public MetaData MetaData { get; } = new MetaData();

        public void Add(ReadOnlySpan<char> buffer)
        {
            if (_hasReadFromStream)
                throw new Exception("Cannot write after enumeration");

            if (_writer == null) {
                if (_buffer.Count >= _maxRowsInMemory) {
                    _stream = _tempStreamManager.Get(ColumnIndex);
                    _writer = new BinaryWriter(_stream);
                } else
                    _buffer.Add(buffer);
            }

            string str = null;
            if(_writer != null)
                _writer.Write(str = buffer.ToString());

            if (!_hasExceededUniqueLimit && _unique.Count < Consts.MaxDistinct) {
                if (str == null)
                    str = buffer.ToString();
                if (_unique.TryGetValue(str, out var count))
                    _unique[str] = count + 1;
                else
                    _unique.Add(str, 1);
            } else
                _hasExceededUniqueLimit = true;
        }

        public IEnumerable<string> EnumerateAll()
        {
            foreach (var item in _buffer.All)
                yield return item;

            if (_stream != null) {
                _hasReadFromStream = true;
                _writer.Flush();
                _stream.Flush();

                _stream.Seek(0, SeekOrigin.Begin);
                var reader = new BinaryReader(_stream);
                while (_stream.Position < _stream.Length)
                    yield return reader.ReadString();
            }
        }

        public void WriteTo(BinaryWriter writer)
        {
            // write the column data
            if(HasUnique) {
                var stringTable = new Dictionary<string, uint>();
                var indices = new List<uint>();
                foreach (var str in EnumerateAll()) {
                    if (!stringTable.TryGetValue(str, out var index))
                        stringTable.Add(str, index = (uint)stringTable.Count);
                    indices.Add(index);
                }

                // write the strings
                writer.Write(stringTable.Count);
                foreach(var item in stringTable.OrderBy(kv => kv.Value).Select(kv => kv.Key))
                    writer.Write(item);

                // write the column string indices
                foreach(var item in indices)
                    writer.Write(item);
            }else {
                // write all strings
                foreach(var str in EnumerateAll())
                    writer.Write(str);
            }
        }
    }
}
