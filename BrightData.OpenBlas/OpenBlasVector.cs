using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlegbra2;

namespace BrightData.OpenBlas
{
    internal class OpenBlasVector : Vector2<OpenBlasLinearAlgebraProvider>
    {
        public OpenBlasVector(ITensorSegment2 data, OpenBlasLinearAlgebraProvider lap) : base(data, lap)
        {
        }

        public override IVector Create(ITensorSegment2 segment) => new OpenBlasVector(segment, _lap);
    }
}
