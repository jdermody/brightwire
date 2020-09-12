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
            output.Should().BeEquivalentTo(input);
        }

        void _EncodeArray<T>(T[] input)
        {
            DataEncoder.Write(_context.Writer, input);
            var output = _context.Encoder.ReadArray<T>(_context.ReadFromStart());
            output.Should().BeEquivalentTo(input);
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

        [Fact]
        public void EncodeDateTime()
        {
            _Encode(DateTime.Now);
        }

        [Fact]
        public void EncodeDateTimeArray()
        {
            _EncodeArray(new[] { DateTime.Now, DateTime.Now.AddMilliseconds(100) });
        }

        [Fact]
        public void EncodeIndexList()
        {
            _Encode(_context.Context.CreateIndexList(1, 2, 3));
        }

        [Fact]
        public void EncodeIndexListArray()
        {
            _EncodeArray(new [] {
                _context.Context.CreateIndexList(1, 2, 3),
                _context.Context.CreateIndexList(2, 3, 4)
            });
        }

        [Fact]
        public void EncodeWeightedIndexList()
        {
            _Encode(_context.Context.CreateWeightedIndexList((1, 1f), (2, 0.5f), (3, 0f)));
        }

        [Fact]
        public void EncodeWeightedIndexListArray()
        {
            _EncodeArray(new [] {
                _context.Context.CreateWeightedIndexList((1, 1f), (2, 0.5f), (3, 0f)),
                _context.Context.CreateWeightedIndexList((2, 1f), (3, 0.5f), (4, 0f)),
            });
        }

        [Fact]
        public void EncodeFloatVector()
        {
            _Encode(_context.Context.CreateVector(8, i => (float)i));
        }

        [Fact]
        public void EncodeFloatVectorArray()
        {
            _EncodeArray(new [] {
                _context.Context.CreateVector(8, i => (float)i),
                _context.Context.CreateVector(8, i => (float)i*2)
            });
        }

        [Fact]
        public void EncodeFloatMatrix()
        {
            _Encode(_context.Context.CreateMatrix(8, 4, (i, j) => (float)i));
        }

        [Fact]
        public void EncodeFloatMatrixArray()
        {
            _EncodeArray(new[] {
                _context.Context.CreateMatrix(8, 4, (i, j) => (float)i),
                _context.Context.CreateMatrix(4, 8, (i, j) => (float)i * 2)
            });
        }

        [Fact]
        public void EncodeBinaryData()
        {
            _Encode(new BinaryData(new byte[] { 1, 2, 3 }));
        }

        [Fact]
        public void EncodeBinaryDataArray()
        {
            _EncodeArray(new[] {
                new BinaryData(new byte[] { 1, 2, 3 }),
                new BinaryData(new byte[] { 2, 3, 4 })
            });
        }
    }
}
