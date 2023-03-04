using System;
using System.Runtime.Serialization;

namespace BrightData.Cuda.CudaToolkit
{
    internal class CudaBlasException : Exception
    {
        public CublasStatus CudaBlasError { get; set; }

        static string GetErrorMessageFromCuResult(CublasStatus error)
        {
            var message = error switch {
                CublasStatus.Success => "Any CUBLAS operation is successful.",
                CublasStatus.NotInitialized => "The CUBLAS library was not initialized.",
                CublasStatus.AllocFailed => "Resource allocation failed.",
                CublasStatus.InvalidValue => "An invalid numerical value was used as an argument.",
                CublasStatus.ArchMismatch => "An absent device architectural feature is required.",
                CublasStatus.MappingError => "An access to GPU memory space failed.",
                CublasStatus.ExecutionFailed => "An access to GPU memory space failed.",
                CublasStatus.InternalError => "An internal operation failed.",
                CublasStatus.NotSupported => "Error: Not supported.",
                _ => string.Empty
            };

            return error + ": " + message;
        }

        public CudaBlasException(CublasStatus error)
            : base(GetErrorMessageFromCuResult(error))
        {
            CudaBlasError = error;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CudaBlasError", CudaBlasError);
        }
    }
}