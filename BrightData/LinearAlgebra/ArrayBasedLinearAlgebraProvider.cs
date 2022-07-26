namespace BrightData.LinearAlgebra
{
    public class ArrayBasedLinearAlgebraProvider : LinearAlgebraProvider
    {
        internal ArrayBasedLinearAlgebraProvider(BrightDataContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public override ITensorSegment CreateSegment(uint size, bool initialiseToZero) => new ArrayBasedTensorSegment(new float[size]);
    }
}
