using System;
using BrightData.Helper;
using BrightData.UnitTests.Fixtures;
using Xunit;
using FluentAssertions;

namespace BrightData.UnitTests
{
    public partial class DataEncoderTests
    {
        private readonly SerialisationFixture _context;

        public DataEncoderTests()
        {
            _context = new SerialisationFixture();
        }

        void _Encode<T>(T input)
        {
            DataEncoder.Write(_context.Writer, input);
            var output = _context.Encoder.Read<T>(_context.ReadFromStart());
            output.ShouldBeEquivalentTo(input);
        }

        void _EncodeArray<T>(T[] input)
        {
            DataEncoder.Write(_context.Writer, input);
            var output = _context.Encoder.ReadArray<T>(_context.ReadFromStart());
            output.ShouldBeEquivalentTo(input);
        }

        [Fact]
        public void EncodeString()
        {
            _Encode("test");
        }

        [Fact]
        public void EncodeStringArray()
        {
            _EncodeArray(new [] {"1", "2", "3"});
        }

        [Fact]
        public void EncodeBoolTrue()
        {
            _Encode(true);
        }

        [Fact] public void EncodeBoolFalse()
        {
            _Encode(false);
        }

        [Fact]
        public void EncodeBoolArray()
        {
            _EncodeArray(new[] { true, false });
        }
    }
}
