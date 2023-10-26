using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Operations.Vectorisation
{
    internal class BooleanVectoriser : VectorisationBase<bool>
    {
        public BooleanVectoriser(bool isOutput) : base(isOutput, 1)
        {
        }

        protected override void Vectorise(in bool item, Span<float> buffer)
        {
            buffer[0] = item ? 1f : 0f;
        }

        public override VectorisationType Type => VectorisationType.Boolean;
    }
}
