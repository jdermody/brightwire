using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace BrightData.Table
{
    public interface IProvideTempStreams
    {
        SafeFileHandle Get(Guid id);
        void Clear();
    }

    public delegate void BlockCallback<T>(ReadOnlySpan<T> block);

    public interface IHaveSize
    {
        uint Size { get; }
    }

    /// <summary>
    /// Composite buffers add data in memory until a pre specified limit and then store the remainder in a temp file
    /// * Not thread safe *
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICompositeBuffer<T> : IHaveSize where T: notnull
    {
        public Guid Id { get; }
        public string? Name { get; set; }
        Task ForEachBlock(BlockCallback<T> callback);
        void Add(in T item);
        void Add(ReadOnlySpan<T> inputBlock);
        uint? DistinctItems { get; }
    }
}
