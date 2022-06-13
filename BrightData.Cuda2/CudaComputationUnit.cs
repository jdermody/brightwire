using BrightData.Cuda.Helper;
using BrightData2;
using BrightData.Cuda;
using ManagedCuda;

namespace BrightData.Cuda2
{
    public class CudaComputationUnit : ComputationUnit
    {
        readonly CudaProvider _cuda;

        public CudaComputationUnit(BrightDataContext2 context, CudaProvider cuda) : base(context)
        {
            _cuda = cuda;
        }

        public override IDisposableTensorSegment CreateSegment(uint size) => new CudaTensorSegment(_cuda.Memory.GetMemory(size));
        public override IVector CreateVector(ITensorSegment2 data) => new CudaVector2(OptionallyCopyToDevice(data), this);

        ITensorSegment2 OptionallyCopyToDevice(ITensorSegment2 segment)
        {
            if (segment.SegmentType == "cuda")
                return segment;

            var deviceMemory = _cuda.Memory.GetMemory(segment.Size);
            deviceMemory.CopyToDevice(segment.GetSpan());
            return new CudaTensorSegment(deviceMemory);
        }

        public override float DotProduct(ITensorSegment2 tensor, ITensorSegment2 tensor2)
        {
            return _cuda.Blas.Dot(
                GetDeviceVariable(tensor), 
                1, 
                GetDeviceVariable(tensor2), 
                1
            );
        }

        static CudaDeviceVariable<float> GetDeviceVariable(ITensorSegment2 segment)
        {
            if(segment is CudaTensorSegment cudaSegment)
                return cudaSegment.DeviceMemory.DeviceVariable;

            throw new Exception("CUDA tensors can only be used with other CUDA tensors");
        }
    }
}