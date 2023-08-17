using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.Segments
{
    /// <summary>
    /// A tensor segment that temporarily owns a buffer from an array pool
    /// </summary>
    public class ArrayPoolTensorSegment : ArrayBasedTensorSegment
    {
        readonly MemoryOwner<float> _memoryOwner;
        int _refCount = 0;
        bool _isValid = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Rented buffer from pool</param>
        public ArrayPoolTensorSegment(MemoryOwner<float> data) : base(data.DangerousGetArray().Array!)
        {
            _memoryOwner = data;
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
        public override string SegmentType => "memory owner";

        /// <inheritdoc />
        public override IEnumerable<float> Values
        {
            get
            {
                for (int i = 0, len = (int)Size; i < len; i++)
                    yield return _data[i];
            }
        }

        /// <inheritdoc />
        public override float[] ToNewArray() => _memoryOwner.Span.ToArray();

        /// <inheritdoc />
        public override void CopyFrom(ReadOnlySpan<float> span, uint targetOffset) => span.CopyTo(targetOffset == 0 ? _memoryOwner.Span : _memoryOwner.Span[(int)targetOffset..]);

        /// <inheritdoc />
        public override void CopyTo(ITensorSegment segment, uint sourceOffset, uint targetOffset)
        {
            var span = GetSpan(sourceOffset);
            var destination = segment.GetArrayIfEasilyAvailable();
            if (destination is not null)
                span.CopyTo(destination.AsSpan((int)targetOffset, (int)(segment.Size - targetOffset)));
            else
                segment.CopyFrom(span, targetOffset);
        }

        /// <inheritdoc />
        public override ReadOnlySpan<float> GetSpan(uint offset = 0) => _memoryOwner.Span[(int)offset..];
    }
}
