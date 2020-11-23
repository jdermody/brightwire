using System;
using BrightData.Helper;
using BrightData.UnitTests.Fixtures;
using Xunit;
using FluentAssertions;

namespace BrightData.UnitTests
{
    public partial class DataEncoderTests
    {
		[Fact]
        public void EncodeDecimal()
        {
            _Encode((decimal)123);
        }

        [Fact]
        public void EncodeDecimalArray()
        {
            _EncodeArray(new decimal[] {1, 2, 3});
        }
		[Fact]
        public void EncodeDouble()
        {
            _Encode((double)123);
        }

        [Fact]
        public void EncodeDoubleArray()
        {
            _EncodeArray(new double[] {1, 2, 3});
        }
		[Fact]
        public void EncodeFloat()
        {
            _Encode((float)123);
        }

        [Fact]
        public void EncodeFloatArray()
        {
            _EncodeArray(new float[] {1, 2, 3});
        }
		[Fact]
        public void EncodeLong()
        {
            _Encode((long)123);
        }

        [Fact]
        public void EncodeLongArray()
        {
            _EncodeArray(new long[] {1, 2, 3});
        }
		[Fact]
        public void EncodeInt()
        {
            _Encode((int)123);
        }

        [Fact]
        public void EncodeIntArray()
        {
            _EncodeArray(new int[] {1, 2, 3});
        }
		[Fact]
        public void EncodeShort()
        {
            _Encode((short)123);
        }

        [Fact]
        public void EncodeShortArray()
        {
            _EncodeArray(new short[] {1, 2, 3});
        }
		[Fact]
        public void EncodeSbyte()
        {
            _Encode((sbyte)123);
        }

        [Fact]
        public void EncodeSbyteArray()
        {
            _EncodeArray(new sbyte[] {1, 2, 3});
        }
    }
}