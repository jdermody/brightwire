using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightWire.Cuda.Helper;

namespace BrightWire.CUDA.Source.Helper
{
    class Tensor3DInput
    {
	    readonly IReadOnlyList<IDeviceMemoryPtr> _matrixPtrList;

	    public Tensor3DInput(int rows, int columns, IReadOnlyList<IDeviceMemoryPtr> data)
	    {
		    _matrixPtrList = data;
		    Rows = rows;
		    Columns = columns;
	    }

	    public int Depth => _matrixPtrList.Count;
	    public int Rows { get; }
	    public int Columns { get; }
	    public int MatrixSize => Rows * Columns;

		public IReadOnlyList<IDeviceMemoryPtr> MatrixPtrList => _matrixPtrList;
	    public DeviceMemoryPtrList GetDeviceMemoryPtr() => new DeviceMemoryPtrList(_matrixPtrList);
	    public Tensor4DInput As4DTensor() => new Tensor4DInput(Rows, Columns, new[] {_matrixPtrList});
    }
}
