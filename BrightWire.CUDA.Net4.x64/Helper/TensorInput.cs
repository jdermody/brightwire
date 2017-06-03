using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Cuda.Helper
{
    /// <summary>
    /// Helper class that represents a 4D tensor input to a cuda kernel
    /// </summary>
    class TensorInput
    {
        readonly IReadOnlyList<IReadOnlyList<IDeviceMemoryPtr>> _data;
        readonly int _depth, _rows, _columns;

        public TensorInput(int rows, int columns, IReadOnlyList<IReadOnlyList<IDeviceMemoryPtr>> data)
        {
            _data = data;
            var first = data.First();
            _depth = first.Count;
            _rows = rows;
            _columns = columns;
        }

        public int Count => _data.Count;
        public int Depth => _depth;
        public int Rows => _rows;
        public int Columns => _columns;
        public int MatrixSize => _rows * _columns;

        internal DeviceMemoryPtrList GetDeviceMemoryPtr() => new DeviceMemoryPtrList(_data);
        internal IReadOnlyList<IDeviceMemoryPtr> Single() => _data.Single();
    }
}
