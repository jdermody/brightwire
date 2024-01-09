using System;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// Vectorisation of booleans
    /// </summary>
    /// <param name="isOutput"></param>
    internal class BooleanVectoriser(bool isOutput) : VectorisationBase<bool>(isOutput, 1)
    {
        protected override void Vectorise(in bool item, Span<float> buffer)
        {
            buffer[0] = item ? 1f : 0f;
        }

        public override VectorisationType Type => VectorisationType.Boolean;
    }
}
