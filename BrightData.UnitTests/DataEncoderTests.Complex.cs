using System;
using BrightData.Helper;
using BrightData.UnitTests.Fixtures;
using Xunit;
using FluentAssertions;
using BrightData.Types;

namespace BrightData.UnitTests
{
    public partial class DataEncoderTests
    {
        readonly SerialisationFixture _context = new();

        void Encode<T>(T input) where T: notnull
        {
            DataEncoder.Write(_context.Writer, input);
            var output = _context.Encoder.Read<T>(_context.ReadFromStart());
            output.Should().BeEquivalentTo(input);
        }

        void EncodeArray<T>(T[] input) where T: notnull
        {
            DataEncoder.Write(_context.Writer, input);
            var output = _context.Encoder.ReadArray<T>(_context.ReadFromStart());
            output.Should().BeEquivalentTo(input);
        }

        [Fact]
        public void EncodeString()
        {
            Encode("test");
        }

        [Fact]
        public void EncodeStringArray()
        {
            EncodeArray(["1", "2", "3"]);
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
            EncodeArray([true, false]);
        }

        [Fact]
        public void EncodeDateTime()
        {
            Encode(DateTime.Now);
        }

        [Fact]
        public void EncodeDateTimeArray()
        {
            EncodeArray([DateTime.Now, DateTime.Now.AddMilliseconds(100)]);
        }

        [Fact]
        public void EncodeIndexList()
        {
            Encode(IndexList.Create(1, 2, 3));
        }

        [Fact]
        public void EncodeIndexListArray()
        {
            EncodeArray([
                IndexList.Create(1, 2, 3),
                IndexList.Create(2, 3, 4)
            ]);
        }

        [Fact]
        public void EncodeWeightedIndexList()
        {
            Encode(WeightedIndexList.Create((1, 1f), (2, 0.5f), (3, 0f)));
        }

        [Fact]
        public void EncodeWeightedIndexListArray()
        {
            EncodeArray([
                WeightedIndexList.Create((1, 1f), (2, 0.5f), (3, 0f)),
                WeightedIndexList.Create((2, 1f), (3, 0.5f), (4, 0f))
            ]);
        }

        [Fact]
        public void EncodeFloatVector()
        {
            Encode(
                _context.Context.CreateReadOnlyVector(8, i => i)
            );
        }

        [Fact]
        public void EncodeFloatVectorArray()
        {
            EncodeArray([
                _context.Context.CreateReadOnlyVector(8, i => i),
                _context.Context.CreateReadOnlyVector(8, i => (float)i*2)
            ]);
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
            Encode(new BinaryData(1, 2, 3));
        }

        [Fact]
        public void EncodeBinaryDataArray()
        {
            EncodeArray([
                new BinaryData(1, 2, 3),
                new BinaryData(2, 3, 4)
            ]);
        }
    }
}
