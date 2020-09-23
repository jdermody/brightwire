using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightTable.Input
{
    //public class InputBufferReader : IDisposable
    //{
    //    readonly InputData _data;
    //    long _startOffset;

    //    public InputBufferReader(InputData data, long startOffset, uint size)
    //    {
    //        _data = data;
    //        _startOffset = startOffset;
    //        Length = size;
    //    }

    //    public void Dispose()
    //    {
    //        _data.Dispose();
    //    }

    //    public BinaryReader Reader => _data.Reader;
    //    public uint Length { get; }

    //    public void Reset()
    //    {
    //        _data.MoveTo(_startOffset);
    //    }

    //    public void ResetStartPosition() => _startOffset = _data.Position;
    //}
}
