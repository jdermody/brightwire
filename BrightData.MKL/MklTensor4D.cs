using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    internal class MklTensor4D : BrightTensor4D<MklLinearAlgebraProvider>
    {
        public MklTensor4D(ITensorSegment data, uint count, uint depth, uint rows, uint columns, MklLinearAlgebraProvider lap) : base(data, count, depth, rows, columns, lap)
        {
        }
    }
}
