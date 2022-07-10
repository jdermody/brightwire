using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;

namespace BrightData.Numerics
{
    internal class NumericsVector2 : Vector<NumericsLinearAlgebraProvider>
    {
        public NumericsVector2(ITensorSegment2 data, NumericsLinearAlgebraProvider lap) : base(data, lap)
        {
        }

        public override IVector Create(ITensorSegment2 segment) => new NumericsVector2(segment, _lap);
    }
}
