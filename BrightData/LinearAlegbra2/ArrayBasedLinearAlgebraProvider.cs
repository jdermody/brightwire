using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.LinearAlegbra2
{
    public class ArrayBasedLinearAlgebraProvider : LinearAlgebraProvider
    {
        internal ArrayBasedLinearAlgebraProvider(BrightDataContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override ITensorSegment2 CreateSegment(uint size) => new ArrayBasedTensorSegment(new float[size]);
    }
}
