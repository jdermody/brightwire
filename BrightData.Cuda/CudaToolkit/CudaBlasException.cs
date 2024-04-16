using System;
using System.Runtime.Serialization;

namespace BrightData.Cuda.CudaToolkit
{
    internal class CudaBlasException(CuBlasStatus error) : Exception(GetErrorMessageFromCuResult(error))
    {
        public CuBlasStatus CudaBlasError { get; set; } = error;

        static string GetErrorMessageFromCuResult(CuBlasStatus error)
        {
            var message = error switch {
                CuBlasStatus.Success => "Any CUBLAS operation is successful.",
                CuBlasStatus.NotInitialized => "The CUBLAS library was not initialized.",
                CuBlasStatus.AllocFailed => "Resource allocation failed.",
                CuBlasStatus.InvalidValue => "An invalid numerical value was used as an argument.",
                CuBlasStatus.ArchMismatch => "An absent device architectural feature is required.",
                CuBlasStatus.MappingError => "An access to GPU memory space failed.",
                CuBlasStatus.ExecutionFailed => "An access to GPU memory space failed.",
                CuBlasStatus.InternalError => "An internal operation failed.",
                CuBlasStatus.NotSupported => "Error: Not supported.",
                _ => string.Empty
            };

            return error + ": " + message;
        }

        [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CudaBlasError", CudaBlasError);
        }
    }
}