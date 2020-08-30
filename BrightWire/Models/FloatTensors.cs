using BrightData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrightWire.Models
{
    public static class FloatVector
    {
        public static Vector<float> Create(float[] ret)
        {
            throw new NotImplementedException();
        }

        public static Vector<float> Create(ITensorSegment<float> vector)
        {
            throw new NotImplementedException();
        }

        public static Vector<float> ReadFrom(BinaryReader reader)
        {
            throw new NotImplementedException();
        }
    }

    public static class FloatMatrix
    {
        public static Matrix<float> Create(Vector<float>[] rows)
        {
            throw new NotImplementedException();
        }

        public static Matrix<float> ReadFrom(BinaryReader reader)
        {
            throw new NotImplementedException();
        }
    }

    public static class FloatTensor
    {
        public static Tensor3D<float> Create(Matrix<float>[] matrices)
        {
            throw new NotImplementedException();
        }
    }
}
