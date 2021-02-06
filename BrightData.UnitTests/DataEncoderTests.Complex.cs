using System;
using BrightData.Helper;
using BrightData.UnitTests.Fixtures;
using Xunit;
using FluentAssertions;
using FluentAssertions.Equivalency;

namespace BrightData.UnitTests
{
    public partial class DataEncoderTests
    {
        readonly SerialisationFixture _context;

        public DataEncoderTests()
        {
            _context = new SerialisationFixture();
        }

        void Encode<T>(T input, Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> optionsFunc = null)
        {
            DataEncoder.Write(_context.Writer, input);
            var output = _context.Encoder.Read<T>(_context.ReadFromStart());
            output.Should().BeEquivalentTo(input, options => optionsFunc?.Invoke(options) ?? options);
        }

        void EncodeArray<T>(T[] input, Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> optionsFunc = null)
        {
            DataEncoder.Write(_context.Writer, input);
            var output = _context.Encoder.ReadArray<T>(_context.ReadFromStart());
            output.Should().BeEquivalentTo(input, options => optionsFunc?.Invoke(options) ?? options);
        }

        [Fact]
        public void EncodeString()
        {
            Encode("test");
        }

        [Fact]
        public void EncodeStringArray()
        {
            EncodeArray(new [] {"1", "2", "3"});
        }

        [Fact]
        public void EncodeBoolTrue()
        {
            Encode(true);
        }

        [Fact] public void EncodeBoolFalse()
        {
            Encode(false);
        }

        [Fact]
        public void EncodeBoolArray()
        {
            EncodeArray(new[] { true, false });
        }

        [Fact]
        public void EncodeDateTime()
        {
            Encode(DateTime.Now);
        }

        [Fact]
        public void EncodeDateTimeArray()
        {
            EncodeArray(new[] { DateTime.Now, DateTime.Now.AddMilliseconds(100) });
        }

        [Fact]
        public void EncodeIndexList()
        {
            Encode(_context.Context.CreateIndexList(1, 2, 3));
        }

        [Fact]
        public void EncodeIndexListArray()
        {
            EncodeArray(new [] {
                _context.Context.CreateIndexList(1, 2, 3),
                _context.Context.CreateIndexList(2, 3, 4)
            });
        }

        [Fact]
        public void EncodeWeightedIndexList()
        {
            Encode(_context.Context.CreateWeightedIndexList((1, 1f), (2, 0.5f), (3, 0f)));
        }

        [Fact]
        public void EncodeWeightedIndexListArray()
        {
            EncodeArray(new [] {
                _context.Context.CreateWeightedIndexList((1, 1f), (2, 0.5f), (3, 0f)),
                _context.Context.CreateWeightedIndexList((2, 1f), (3, 0.5f), (4, 0f)),
            });
        }

        [Fact]
        public void EncodeFloatVector()
        {
            Encode(
                _context.Context.CreateVector(8, i => (float)i), 
                options => options.Excluding(v => v.Segment.AllocationIndex)
            );
        }

        [Fact]
        public void EncodeFloatVectorArray()
        {
            EncodeArray(new [] {
                _context.Context.CreateVector(8, i => (float)i),
                _context.Context.CreateVector(8, i => (float)i*2)
            }, options => options.Excluding(v => v.Segment.AllocationIndex));
        }

        //[Fact]
        //public void EncodeFloatMatrix()
        //{
        //    Encode(
        //        _context.Context.CreateMatrix(8, 4, (i, j) => (float)i), 
        //        options => options.Excluding(v => v.Segment.AllocationIndex).Excluding(v => v.Rows.First().Segment.AllocationIndex)
        //    );
        //}

        //[Fact]
        //public void EncodeFloatMatrixArray()
        //{
        //    EncodeArray(new[] {
        //        _context.Context.CreateMatrix(8, 4, (i, j) => (float)i),
        //        _context.Context.CreateMatrix(4, 8, (i, j) => (float)i * 2)
        //    }, options => options.Excluding(v => v.Segment.AllocationIndex));
        //}

        [Fact]
        public void EncodeBinaryData()
        {
            Encode(new BinaryData(new byte[] { 1, 2, 3 }));
        }

        [Fact]
        public void EncodeBinaryDataArray()
        {
            EncodeArray(new[] {
                new BinaryData(new byte[] { 1, 2, 3 }),
                new BinaryData(new byte[] { 2, 3, 4 })
            });
        }
    }
}
