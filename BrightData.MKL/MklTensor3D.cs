using BrightData.LinearAlgebra;

namespace BrightData.MKL
{
    internal class MklTensor3D : BrightTensor3D<MklLinearAlgebraProvider>
    {
        public MklTensor3D(ITensorSegment data, uint depth, uint rowCount, uint columnCount, MklLinearAlgebraProvider lap) : base(data, depth, rowCount, columnCount, lap)
        {
        }
    }
}
