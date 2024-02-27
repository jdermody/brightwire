using Xunit;

namespace BrightData.UnitTests
{
    public partial class DataEncoderTests
    {
		[Fact]
        public void EncodeDecimal()
        {
            Encode((decimal)123);
        }

        [Fact]
        public void EncodeDecimalArray()
        {
            EncodeArray(new decimal[] {1, 2, 3});
        }

		[Fact]
        public void EncodeDouble()
        {
            Encode((double)123);
        }

        [Fact]
        public void EncodeDoubleArray()
        {
            EncodeArray(new double[] {1, 2, 3});
        }

		[Fact]
        public void EncodeFloat()
        {
            Encode((float)123);
        }

        [Fact]
        public void EncodeFloatArray()
        {
            EncodeArray(new float[] {1, 2, 3});
        }

		[Fact]
        public void EncodeLong()
        {
            Encode((long)123);
        }

        [Fact]
        public void EncodeLongArray()
        {
            EncodeArray(new long[] {1, 2, 3});
        }

		[Fact]
        public void EncodeInt()
        {
            Encode(123);
        }

        [Fact]
        public void EncodeIntArray()
        {
            EncodeArray([1, 2, 3]);
        }

		[Fact]
        public void EncodeShort()
        {
            Encode((short)123);
        }

        [Fact]
        public void EncodeShortArray()
        {
            EncodeArray(new short[] {1, 2, 3});
        }

		[Fact]
        public void EncodeSbyte()
        {
            Encode((sbyte)123);
        }

        [Fact]
        public void EncodeSbyteArray()
        {
            EncodeArray(new sbyte[] {1, 2, 3});
        }

    }
}