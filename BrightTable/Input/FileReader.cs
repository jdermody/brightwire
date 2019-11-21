using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightTable.Input
{
    class FileReader : IDisposable
    {
        const int BUFFER_SIZE = 4096 * 2;

        readonly StreamReader _streamReader;
        readonly long _size;
        readonly char[] _forwardBuffer = new char[BUFFER_SIZE];

        long _forwardPosition = 0;
        int _forwardBufferSize = 0;
        int _bufferOffset = 0;
        bool _atEnd = false;

        public FileReader(string path)
        {
            _size = new FileInfo(path).Length;
            _streamReader = new StreamReader(path);
        }

        public long Position => _forwardPosition + _bufferOffset;
        public bool CanRead => !_atEnd && Position < _size;
        public long ProgressPercent => Position * 100 / _size;

        public void Reset()
        {
            _forwardPosition = 0;
            _forwardBufferSize = 0;
            _bufferOffset = 0;
            _atEnd = false;
        }

        public char GetNext()
        {
            var delta = _size - Position;
            if (delta > 0 && !_atEnd) {
                // see if we need to load the next page
                if (_bufferOffset >= _forwardBufferSize) {
                    _bufferOffset = 0;
                    _forwardPosition += _forwardBufferSize;
                    if (delta >= BUFFER_SIZE)
                        _forwardBufferSize = _streamReader.Read(_forwardBuffer, 0, BUFFER_SIZE);
                    else {
                        _forwardBufferSize = _streamReader.Read(_forwardBuffer, 0, (int)delta);
                        _atEnd = _forwardBufferSize == 0;
                    }
                }

                if (_bufferOffset < _forwardBufferSize)
                    return _forwardBuffer[_bufferOffset++];
            }
            return '\0';
        }

        public void Dispose()
        {
            _streamReader?.Dispose();
        }
    }
}
