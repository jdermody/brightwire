using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.LinearAlegbra2;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace BrightData.Numerics
{
    public class NumericsLinearAlgebraProvider : LinearAlgebraProvider
    {
        public NumericsLinearAlgebraProvider(BrightDataContext context, bool useNumericsMkl = false) : base(context)
        {
            if(useNumericsMkl)
                Control.UseNativeMKL();
        }

        public override IVector CreateVector(ITensorSegment2 data)
        {
            return base.CreateVector(data);
        }
    }
}
