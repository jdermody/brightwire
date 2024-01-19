using System;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// Vectorisation of booleans
    /// </summary>
    internal class BooleanVectoriser() : VectorisationBase<bool>(1)
    {
        protected override void Vectorise(in bool item, Span<float> buffer)
        {
            buffer[0] = item ? 1f : 0f;
        }

        public override VectorisationType Type => VectorisationType.Boolean;
    }
}
