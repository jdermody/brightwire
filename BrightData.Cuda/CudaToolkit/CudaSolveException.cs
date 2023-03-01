using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Cuda.CudaToolkit
{
    /// <summary>
    /// An CudaSolveException is thrown, if any wrapped call to the CuSolve-library does not return <see cref="cusolverStatus.Success"/>.
    /// </summary>
    public class CudaSolveException : Exception, ISerializable
    {
        private cusolverStatus _solverStatus;

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public CudaSolveException()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serInfo"></param>
        /// <param name="streamingContext"></param>
        protected CudaSolveException(SerializationInfo serInfo, StreamingContext streamingContext)
            : base(serInfo, streamingContext)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        public CudaSolveException(cusolverStatus error)
            : base(GetErrorMessageFromCUResult(error))
        {
            this._solverStatus = error;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public CudaSolveException(string message)
            : base(message)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public CudaSolveException(string message, Exception exception)
            : base(message, exception)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public CudaSolveException(cusolverStatus error, string message, Exception exception)
            : base(message, exception)
        {
            this._solverStatus = error;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.SolverStatus.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SolverStatus", this._solverStatus);
        }
        #endregion

        #region Static methods
        private static string GetErrorMessageFromCUResult(cusolverStatus error)
        {
            string message = string.Empty;
            string correct = string.Empty;

            switch (error)
            {
                case cusolverStatus.Success:
                    message = "No Error.";
                    break;
                case cusolverStatus.NotInititialized:
                    message = "The cuSolver library was not initialized. This is usually caused by the lack of a prior call, an error in the CUDA Runtime API called by the cuSolver routine, or an error in the hardware setup.";
                    correct = "\nTo correct: call cusolverCreate() prior to the function call; and check that the hardware, an appropriate version of the driver, and the cuSolver library are correctly installed.";
                    break;
                case cusolverStatus.AllocFailed:
                    message = "Resource allocation failed inside the cuSolver library. This is usually caused by a cudaMalloc() failure.";
                    correct = "\nTo correct: prior to the function call, deallocate previously allocated memory as much as possible.";
                    break;
                case cusolverStatus.InvalidValue:
                    message = "An unsupported value or parameter was passed to the function (a negative vector size, for example).";
                    correct = "\nTo correct: ensure that all the parameters being passed have valid values.";
                    break;
                case cusolverStatus.ArchMismatch:
                    message = "The function requires a feature absent from the device architecture; usually caused by the lack of support for atomic operations or double precision.";
                    correct = "\nTo correct: compile and run the application on a device with compute capability 2.0 or above.";
                    break;
                case cusolverStatus.MappingError:
                    message = "";
                    correct = "";
                    break;
                case cusolverStatus.ExecutionFailed:
                    message = "The GPU program failed to execute. This is often caused by a launch failure of the kernel on the GPU, which can be caused by multiple reasons.";
                    correct = "\nTo correct: check that the hardware, an appropriate version of the driver, and the cuSolver library are correctly installed.";
                    break;
                case cusolverStatus.InternalError:
                    message = "An internal cuSolver operation failed. This error is usually caused by a cudaMemcpyAsync() failure.";
                    correct = "\nTo correct: check that the hardware, an appropriate version of the driver, and the cuSolver library are correctly installed. Also, check that the memory passed as a parameter to the routine is not being deallocated prior to the routine’s completion.";
                    break;
                case cusolverStatus.MatrixTypeNotSupported:
                    message = "The matrix type is not supported by this function. This is usually caused by passing an invalid matrix descriptor to the function.";
                    correct = "\nTo correct: check that the fields in descrA were set correctly.";
                    break;
                case cusolverStatus.NotSupported:
                    message = "";
                    correct = "";
                    break;
                case cusolverStatus.ZeroPivot:
                    message = "";
                    correct = "";
                    break;
                case cusolverStatus.InvalidLicense:
                    message = "";
                    correct = "";
                    break;
                default:
                    break;
            }

            return error.ToString() + ": " + message + correct;
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public cusolverStatus SolverStatus
        {
            get
            {
                return this._solverStatus;
            }
            set
            {
                this._solverStatus = value;
            }
        }
        #endregion
    }
}
