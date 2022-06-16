using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.Buffer2;
using FluentAssertions;
using Microsoft.Toolkit.HighPerformance;
using Xunit;

namespace BrightData.UnitTests
{
    public class BufferTests2 : IDisposable
    {
        const int SIZE = 1024;
        const int UINT_BUFFER_SIZE = SIZE * sizeof(uint);
        static readonly uint[] TestData = SIZE.AsRange().Select(i => i).ToArray();
        readonly IntPtr _memoryBlock;

        public unsafe BufferTests2()
        {
            _memoryBlock = Marshal.AllocCoTaskMem(UINT_BUFFER_SIZE);
            var dest = (uint*)_memoryBlock;
            fixed (uint* ptr = TestData) {
                var ptr2 = ptr;
                for (var i = 0; i < SIZE; i++)
                    *dest++ = *ptr2++;
            }
        }

        public void Dispose()
        {
            Marshal.FreeCoTaskMem(_memoryBlock);
        }

        static ReadOnlyFileBasedBuffer GetIntegersFileBuffer()
        {
            var buffer = MemoryMappedFile.CreateNew(null, UINT_BUFFER_SIZE);
            using var stream = buffer.CreateViewStream(0, UINT_BUFFER_SIZE);
            stream.Write(MemoryMarshal.Cast<uint, byte>(TestData.AsSpan()));
            return new ReadOnlyFileBasedBuffer(buffer);
        }

        static ReadOnlyMemory<uint> GetIntegersMemory() => TestData;

        [Fact]
        public void FileBasedIterableBuffer()
        {
            using var buffer = GetIntegersFileBuffer();
            using var iterator = buffer.GetIterator<uint>(0, UINT_BUFFER_SIZE);
            var fromBuffer = iterator.Enumerate().ToArray();
            fromBuffer.Should().BeEquivalentTo(TestData);
        }

        [Fact]
        public void FileBasedIterableReferencesBuffer()
        {
            using var buffer = GetIntegersFileBuffer();
            using var iterator = buffer.GetIterator<uint>(0, UINT_BUFFER_SIZE);
            var fromBuffer = new uint[SIZE];
            uint index = 0;

            foreach(ref readonly var item in iterator)
                fromBuffer[index++] = item;
            fromBuffer.Should().BeEquivalentTo(TestData);
        }

        [Fact]
        public void MemoryBasedIterableBuffer()
        {
            var buffer = new ReadOnlyMemoryBasedBuffer(_memoryBlock);
            using var iterator = buffer.GetIterator<uint>(0, UINT_BUFFER_SIZE);
            var fromBuffer = iterator.Enumerate().ToArray();
            fromBuffer.Should().BeEquivalentTo(TestData);
        }

        [Fact]
        public void MemoryBasedIterableReferencesBuffer()
        {
            var buffer = new ReadOnlyMemoryBasedBuffer(_memoryBlock);
            using var iterator = buffer.GetIterator<uint>(0, UINT_BUFFER_SIZE);
            var fromBuffer = new uint[SIZE];
            uint index = 0;

            foreach(ref readonly var item in iterator)
                fromBuffer[index++] = item;
            fromBuffer.Should().BeEquivalentTo(TestData);
        }

        [Fact]
        public void FileBasedRandomAccess()
        {
            using var buffer = GetIntegersFileBuffer();
            using var block = buffer.GetBlock<uint>(0, UINT_BUFFER_SIZE);

            for (var i = 0; i < SIZE; i++) {
                var val = block[i];
                val.Should().Be(TestData[i]);
            }
        }

        [Fact]
        public void FileBaseRandomAccessRange()
        {
            using var buffer = GetIntegersFileBuffer();
            using var block = buffer.GetBlock<uint>(0, UINT_BUFFER_SIZE);

            var fromBuffer = block.GetSpan(0, SIZE).ToArray();
            fromBuffer.Should().BeEquivalentTo(TestData);
        }

        [Fact]
        public void MemoryBasedRandomAccess()
        {
            var buffer = new ReadOnlyMemoryBasedBuffer(_memoryBlock);
            using var block = buffer.GetBlock<uint>(0, UINT_BUFFER_SIZE);

            for (var i = 0; i < SIZE; i++) {
                var val = block[i];
                val.Should().Be(TestData[i]);
            }
        }

        [Fact]
        public void MemoryBaseRandomAccessRange()
        {
            var buffer = new ReadOnlyMemoryBasedBuffer(_memoryBlock);
            using var block = buffer.GetBlock<uint>(0, UINT_BUFFER_SIZE);

            var fromBuffer = block.GetSpan(0, SIZE).ToArray();
            fromBuffer.Should().BeEquivalentTo(TestData);
        }
    }
}
