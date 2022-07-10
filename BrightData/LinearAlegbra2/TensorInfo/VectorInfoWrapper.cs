﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2.TensorInfo
{
    internal class VectorInfoWrapper : IVectorInfo
    {
        readonly TensorSegmentWrapper2 _segment;

        public VectorInfoWrapper(TensorSegmentWrapper2 segment)
        {
            _segment = segment;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(Size);
            var temp = SpanOwner<float>.Empty;
            _segment.GetSpan(ref temp, out var wasTempUsed);
            try {
                writer.Write(temp.Span.AsBytes());
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        public void Initialize(BrightDataContext context, BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => _segment.GetSpan(ref temp, out wasTempUsed);
        public uint Size => _segment.Size;
        public float this[int index] => _segment[index];
        public float this[uint index] => _segment[index];
        public float[] ToArray() => _segment.ToNewArray();
        public IVector Create(LinearAlgebraProvider lap) => lap.CreateVector(_segment);
        public ITensorSegment2 Segment => _segment;
        public override string ToString()
        {
            var preview = String.Join("|", _segment.Values.Take(Consts.PreviewSize));
            if (Size > Consts.PreviewSize)
                preview += "|...";
            return $"Vector Info ({Size}): {preview}";
        }
    }
}
