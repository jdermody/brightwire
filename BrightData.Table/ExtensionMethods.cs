using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BrightData.Table.Buffer;

namespace BrightData.Table
{
    public static class ExtensionMethods
    {
        public static ICompositeBuffer<T> CreateUnmanagedBuffer<T>(
            IProvideTempStreams? tempStreams = null, 
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = null,
            uint? maxDistinctItems = null
        ) where T: unmanaged => new UnmanagedCompositeBuffer<T>(tempStreams, blockSize, maxInMemoryBlocks, maxDistinctItems);
    }
}
