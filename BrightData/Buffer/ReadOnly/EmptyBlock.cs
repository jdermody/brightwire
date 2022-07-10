﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Buffer.ReadOnly
{
    public class EmptyBlock<T> : ICanRandomlyAccessUnmanagedData<T> where T : unmanaged
    {
        public void Dispose()
        {
            // nop
        }

        public void Get(int index, out T value) => value = default;
        public void Get(uint index, out T value) => value = default;
        public ReadOnlySpan<T> GetSpan(uint startIndex, uint count) => new(null);
    }
}
