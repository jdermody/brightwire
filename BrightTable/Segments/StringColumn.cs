using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
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
        public (string[] StringTable, uint[] Indices) Encode()
        {
            if (_hasExceededUniqueLimit)
                throw new Exception("Table has too many unique values to encode");

            var stringTable = new Dictionary<string, uint>();
            var ret = new List<uint>();
            foreach (var str in EnumerateAll()) {
                if (!stringTable.TryGetValue(str, out var index))
                    stringTable.Add(str, index = (uint)stringTable.Count);
                ret.Add(index);
            }
            return (stringTable.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray(), ret.ToArray());
        }

        public void Add(string str)
        {
            if (_hasReadFromStream)
                throw new Exception("Cannot write after enumeration");

            if (_writer == null) {
                if (_buffer.Count >= _maxRowsInMemory) {
                    _stream = _tempStreamManager.Get(ColumnIndex);
                    _writer = new BinaryWriter(_stream);
                } else
                    _buffer.Add(str);
            }

            _writer?.Write(str);

            if (_unique.Count < Consts.MaxDistinct) {
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
            var isEncoded = HasUnique;

            // write the column data
            if(isEncoded) {
                var data = Encode();

                // write the strings
                writer.Write(data.StringTable.Length);
                foreach(var item in data.StringTable)
                    writer.Write(item);

                // write the column string indices
                foreach(var item in data.Indices)
                    writer.Write(item);
            }else {
                foreach(var str in EnumerateAll())
                    writer.Write(str);
            }
        }
    }
}
