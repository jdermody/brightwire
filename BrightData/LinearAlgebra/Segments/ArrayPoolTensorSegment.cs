using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Segments
{
    /// <summary>
    /// A tensor segment that temporarily owns a buffer from an array pool
    /// </summary>
    public class ArrayPoolTensorSegment<T> : MutableTensorSegment<T> where T: unmanaged, INumber<T>
    {
        readonly MemoryOwner<T> _memoryOwner;
        int _refCount = 0;
        bool _isValid = true;

        #if DEBUG
        static uint NextId = 0;
        static readonly uint BreakOnCreate = uint.MaxValue;
        static readonly uint BreakOnRelease = uint.MaxValue;
        readonly uint _id = Interlocked.Increment(ref NextId);
        #endif

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Rented buffer from pool</param>
        public ArrayPoolTensorSegment(MemoryOwner<T> data) : base(data.DangerousGetArray().Array!)
        {
            _memoryOwner = data;
            #if DEBUG
            if(_id == BreakOnCreate)
                Debugger.Break();
            #endif
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Release();
        }

        /// <inheritdoc />
        public override int AddRef() => Interlocked.Increment(ref _refCount);

        /// <inheritdoc />
        public override int Release()
        {
            var ret = Interlocked.Decrement(ref _refCount);
            if (_isValid && ret <= 0)
            {
#if DEBUG
                if(_id == BreakOnRelease)
                    Debugger.Break();
#endif
                _memoryOwner.Dispose();
                _isValid = false;
            }

            return ret;
        }

        /// <inheritdoc />
        public override uint Size => (uint)_memoryOwner.Length;

        /// <inheritdoc />
        public override bool IsValid => _isValid;

        /// <inheritdoc />
        public override string SegmentType => Consts.MemoryOwnerBased;

        /// <inheritdoc />
        public override IEnumerable<T> Values
        {
            get
            {
                for (int i = 0, len = (int)Size; i < len; i++)
                    yield return _data[i];
            }
        }

        /// <inheritdoc />
        public override T[] ToNewArray() => _memoryOwner.Span.ToArray();

        /// <inheritdoc />
        public override void CopyFrom(ReadOnlySpan<T> span, uint targetOffset) => span.CopyTo(targetOffset == 0 ? _memoryOwner.Span : _memoryOwner.Span[(int)targetOffset..]);

        /// <inheritdoc />
        public override void CopyTo(INumericSegment<T> segment, uint sourceOffset, uint targetOffset)
        {
            var span = ReadOnlySpan[(int)sourceOffset..];
            var (destinationArray, offset, stride) = segment.GetUnderlyingArray();
            if (destinationArray is not null && stride == 1)
                span.CopyTo(destinationArray.AsSpan((int)(targetOffset + offset), (int)(segment.Size - targetOffset)));
            else
                segment.CopyFrom(span, targetOffset);
        }

        /// <inheritdoc />
        public override ReadOnlySpan<T> ReadOnlySpan => _memoryOwner.Span;
    }
}
