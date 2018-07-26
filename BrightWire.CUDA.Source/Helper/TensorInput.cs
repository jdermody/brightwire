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

	    public TensorInput(int rows, int columns, IReadOnlyList<IReadOnlyList<IDeviceMemoryPtr>> data)
        {
            _data = data;
            var first = data.First();
            Depth = first.Count;
            Rows = rows;
            Columns = columns;
        }

        public int Count => _data.Count;
        public int Depth { get; }
	    public int Rows { get; }
	    public int Columns { get; }
	    public int MatrixSize => Rows * Columns;

        internal DeviceMemoryPtrList GetDeviceMemoryPtr() => new DeviceMemoryPtrList(_data);
        internal IReadOnlyList<IDeviceMemoryPtr> Single() => _data.Single();
    }
}
