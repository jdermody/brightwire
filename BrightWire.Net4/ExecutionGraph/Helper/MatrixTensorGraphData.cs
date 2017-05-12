using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Helper
{
    //class MatrixTensorGraphData : IGraphData
    //{
    //    readonly IMatrix _matrix;
    //    readonly ILinearAlgebraProvider _lap;
    //    readonly int _width, _height;

    //    public MatrixTensorGraphData(ILinearAlgebraProvider lap, IMatrix matrix, int width, int height)
    //    {
    //        _lap = lap;
    //        _matrix = matrix;
    //        _width = width;
    //        _height = height;
    //    }

    //    public IMatrix GetAsMatrix()
    //    {
    //        return _matrix;
    //    }

    //    public I3DTensor GetAsTensor()
    //    {
    //        var ret = new List<IMatrix>();
    //        for(var i = 0; i < _matrix.ColumnCount; i++) {
    //            var slice = _matrix.Column(i).ConvertInPlaceToMatrix(_width, _height);
    //            ret.Add(slice);
    //        }
    //        return _lap.CreateTensor(ret);
    //    }
    //}
}
