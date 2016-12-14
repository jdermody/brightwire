using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.LinearAlgebra
{
    public class Cpu3DTensor : IIndexable3DTensor
    {
        readonly CpuMatrix[] _data;
        readonly int _rows, _columns, _depth;

        public Cpu3DTensor(int rows, int columns, int depth)
        {
            _rows = rows;
            _columns = columns;
            _depth = depth;
            _data = Enumerable.Range(0, depth)
                .Select(i => new CpuMatrix(DenseMatrix.Create(_rows, _columns, 0f)))
                .ToArray()
            ;
        }

        public Cpu3DTensor(IReadOnlyList<IMatrix> matrixList)
        {
            var first = matrixList.First();
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = matrixList.Count;
            _data = matrixList.Cast<CpuMatrix>().ToArray();
        }

        public float this[int row, int column, int depth]
        {
            get
            {
                return _data[depth][row, column];
            }

            set
            {
                _data[depth][row, column] = value;
            }
        }

        public int Depth
        {
            get
            {
                return _depth;
            }
        }

        public int RowCount
        {
            get
            {
                return _rows;
            }
        }

        public int ColumnCount
        {
            get
            {
                return _columns;
            }
        }

        public IMatrix GetDepthSlice(int depth)
        {
            return _data[depth];
        }

        public IIndexable3DTensor AsIndexable()
        {
            return this;
        }

        public I3DTensor AddPadding(int padding)
        {
            var newRows = _rows + padding * 2;
            var newColumns = _columns + padding * 2;
            var ret = new Cpu3DTensor(newRows, newColumns, _depth);
            for(var k = 0; k < _depth; k++) {
                for (var j = 0; j < newRows; j++) {
                    for (var i = 0; i < newColumns; i++) {
                        if (i < padding || j < padding)
                            continue;
                        else if (i >= newRows - padding || j >= newColumns - padding)
                            continue;
                        ret[i, j, k] = this[i - padding, j - padding, k];
                    }
                }
            }
            return ret;
        }

        public I3DTensor RemovePadding(int padding)
        {
            var newRows = _rows - padding * 2;
            var newColumns = _columns - padding * 2;
            var ret = new Cpu3DTensor(newRows, newColumns, _depth);
            for (var k = 0; k < _depth; k++) {
                for (var j = 0; j < newRows; j++) {
                    for (var i = 0; i < newColumns; i++) {
                        ret[i, j, k] = this[i + padding, j + padding, k];
                    }
                }
            }
            return ret;
        }

        public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
        {
            int xOffset = 0, yOffset = 0;
            var data = new List<List<float>>();

            while (yOffset <= _rows - filterHeight) {
                var column = new List<float>();
                for (var k = 0; k < _depth; k++) {
                    for (var j = 0; j < filterHeight; j++) {
                        for (var i = 0; i < filterWidth; i++) {
                            column.Add(this[xOffset + i, yOffset + j, k]);
                        }
                    }
                }
                data.Add(column);

                // move the window
                xOffset += stride;
                if (xOffset > _columns - filterWidth) {
                    xOffset = 0;
                    yOffset += stride;
                }
            }
            var firstOutput = data.First();
            return new CpuMatrix(DenseMatrix.Create(data.Count, firstOutput.Count, (i, j) => data[i][j]));
        }

        public IVector ConvertInPlaceToVector()
        {
            var vectorList = _data.Select(m => m.ConvertInPlaceToVector().AsIndexable()).ToArray();
            var size = _rows * _columns;
            var ret = DenseVector.Create(_depth * size, i => {
                var offset = i / size;
                var index = i % size;
                return vectorList[offset][index];
            });
            return new CpuVector(ret);
        }
    }
}
