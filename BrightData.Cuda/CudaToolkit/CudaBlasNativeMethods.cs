using System;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit
{
    internal static class CudaBlasNativeMethods
    {
        internal const string CublasApiDllName = "cublas64_12";

        static CudaBlasNativeMethods()
        {
            DriverApiNativeMethods.Init();
        }

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCreate_v2(ref CudaBlasHandle handle);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDestroy_v2(CudaBlasHandle handle);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetVersion_v2(CudaBlasHandle handle, ref int version);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetWorkspace_v2(CudaBlasHandle handle, CUdeviceptr workspace, SizeT workspaceSizeInBytes);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetStream_v2(CudaBlasHandle handle, CUstream streamId);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetStream_v2(CudaBlasHandle handle, ref CUstream streamId);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetPointerMode_v2(CudaBlasHandle handle, ref PointerMode mode);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetPointerMode_v2(CudaBlasHandle handle, PointerMode mode);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetAtomicsMode(CudaBlasHandle handle, ref AtomicsMode mode);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetAtomicsMode(CudaBlasHandle handle, AtomicsMode mode);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetMathMode(CudaBlasHandle handle, ref Math mode);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetMathMode(CudaBlasHandle handle, Math mode);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetSmCountTarget(CudaBlasHandle handle, ref int smCountTarget);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetSmCountTarget(CudaBlasHandle handle, int smCountTarget);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasLoggerConfigure(int logIsOn, int logToStdOut, int logToStdErr, [MarshalAs(UnmanagedType.LPStr)] string logFileName);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetLoggerCallback(CublasLogCallback userCallback);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetLoggerCallback(ref CublasLogCallback userCallback);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetVector(int n, int elemSize, [In] IntPtr x, int incx, CUdeviceptr devicePtr, int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetVector(int n, int elemSize, [In] CUdeviceptr x, int incx, IntPtr y, int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetMatrix(int rows, int cols, int elemSize, [In] IntPtr a, int lda, CUdeviceptr b, int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetMatrix(int rows, int cols, int elemSize, [In] CUdeviceptr a, int lda, IntPtr b, int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetVectorAsync(int n, int elemSize, [In] IntPtr hostPtr, int incx, CUdeviceptr devicePtr, int incy, CUstream stream);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetVectorAsync(int n, int elemSize, [In] CUdeviceptr devicePtr, int incx, IntPtr hostPtr, int incy, CUstream stream);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetMatrixAsync(int rows, int cols, int elemSize, [In] IntPtr a, int lda, CUdeviceptr b, int ldb, CUstream stream);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetMatrixAsync(int rows, int cols, int elemSize, [In] CUdeviceptr a, int lda, IntPtr b, int ldb, CUstream stream);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCopyEx(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScopy_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDcopy_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCcopy_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZcopy_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSswap_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDswap_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCswap_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZswap_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSwapEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasNrm2Ex(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDotEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDotcEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDznrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSdot_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDdot_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScalEx(CudaBlasHandle handle,
            int n,
            IntPtr alpha,
            CudaDataType alphaType,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSscal_v2(CudaBlasHandle handle,
            int n,
            [In] ref float alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDscal_v2(CudaBlasHandle handle,
            int n,
            [In] ref double alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsscal_v2(CudaBlasHandle handle,
            int n,
            [In] ref float alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdscal_v2(CudaBlasHandle handle,
            int n,
            [In] ref double alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasAxpyEx(CudaBlasHandle handle,
            int n,
            IntPtr alpha,
            CudaDataType alphaType,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIsamax_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIdamax_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIcamax_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIzamax_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIamaxEx(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x, CudaDataType xType,
            int incx,
            ref int result
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIsamin_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIdamin_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIcamin_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIzamin_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIaminEx(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x, CudaDataType xType,
            int incx,
            ref int result
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasAsumEx(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            CudaDataType xType,
            int incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSasum_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDasum_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScasum_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDzasum_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] ref float c,
            [In] ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] ref double c,
            [In] ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] ref float c,
            [In] ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] ref double c,
            [In] ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            IntPtr c,
            IntPtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrotg_v2(CudaBlasHandle handle,
            ref float a,
            ref float b,
            ref float c,
            ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrotg_v2(CudaBlasHandle handle,
            ref double a,
            ref double b,
            ref double c,
            ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotgEx(CudaBlasHandle handle,
            IntPtr a,
            IntPtr b,
            CudaDataType abType,
            IntPtr c,
            IntPtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrotm_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            float[] param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrotm_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            double[] param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotmEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            IntPtr param,
            CudaDataType paramType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrotmg_v2(CudaBlasHandle handle,
            ref float d1,
            ref float d2,
            ref float x1,
            [In] ref float y1,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            float[] param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrotmg_v2(CudaBlasHandle handle,
            ref double d1,
            ref double d2,
            ref double x1,
            [In] ref double y1,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            double[] param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotmgEx(CudaBlasHandle handle,
            IntPtr d1,
            CudaDataType d1Type,
            IntPtr d2,
            CudaDataType d2Type,
            IntPtr x1,
            CudaDataType x1Type,
            IntPtr y1,
            CudaDataType y1Type,
            IntPtr param,
            CudaDataType paramType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasNrm2Ex(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDotEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            CUdeviceptr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDotcEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            CUdeviceptr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDznrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSdot_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDdot_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCdotu_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCdotc_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdotu_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdotc_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScalEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr alpha,
            CudaDataType alphaType,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSscal_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDscal_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCscal_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsscal_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZscal_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdscal_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasAxpyEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr alpha,
            CudaDataType alphaType,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIsamax_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIdamax_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIcamax_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIzamax_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIamaxEx(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x, CudaDataType xType,
            int incx,
            CUdeviceptr result
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIsamin_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIdamin_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIcamin_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIzamin_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIaminEx(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x, CudaDataType xType,
            int incx,
            CUdeviceptr result
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasAsumEx(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSasum_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDasum_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScasum_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDzasum_v2(CudaBlasHandle handle,
            int n,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdrot_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            CUdeviceptr c,
            CUdeviceptr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrotg_v2(CudaBlasHandle handle,
            CUdeviceptr a,
            CUdeviceptr b,
            CUdeviceptr c,
            CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrotg_v2(CudaBlasHandle handle,
            CUdeviceptr a,
            CUdeviceptr b,
            CUdeviceptr c,
            CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCrotg_v2(CudaBlasHandle handle,
            CUdeviceptr a,
            CUdeviceptr b,
            CUdeviceptr c,
            CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZrotg_v2(CudaBlasHandle handle,
            CUdeviceptr a,
            CUdeviceptr b,
            CUdeviceptr c,
            CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotgEx(CudaBlasHandle handle,
            CUdeviceptr a,
            CUdeviceptr b,
            CudaDataType abType,
            CUdeviceptr c,
            CUdeviceptr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrotm_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] CUdeviceptr param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrotm_v2(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            int incx,
            CUdeviceptr y,
            int incy,
            [In] CUdeviceptr param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotmEx(CudaBlasHandle handle,
            int n,
            CUdeviceptr x,
            CudaDataType xType,
            int incx,
            CUdeviceptr y,
            CudaDataType yType,
            int incy,
            CUdeviceptr param,
            CudaDataType paramType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrotmg_v2(CudaBlasHandle handle,
            CUdeviceptr d1,
            CUdeviceptr d2,
            CUdeviceptr x1,
            [In] CUdeviceptr y1,
            CUdeviceptr param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrotmg_v2(CudaBlasHandle handle,
            CUdeviceptr d1,
            CUdeviceptr d2,
            CUdeviceptr x1,
            [In] CUdeviceptr y1,
            CUdeviceptr param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotmgEx(CudaBlasHandle handle,
            CUdeviceptr d1,
            CudaDataType d1Type,
            CUdeviceptr d2,
            CudaDataType d2Type,
            CUdeviceptr x1,
            CudaDataType x1Type,
            [In] CUdeviceptr y1,
            CudaDataType y1Type,
            CUdeviceptr param,
            CudaDataType paramType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CUdeviceptr a, int lda,
            CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr ap, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr ap, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr ap, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr ap,
            CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr a, int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr a, int lda,
            CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr ap,
            CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr ap, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr ap, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CUdeviceptr ap,
            CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CUdeviceptr a,
            int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CUdeviceptr a,
            int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CUdeviceptr a,
            int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CUdeviceptr a,
            int lda, CUdeviceptr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] ref float beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] ref double beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] ref float beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] ref double beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] ref float beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] ref double beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] ref float beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] ref double beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            int incx,
            [In] ref float beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            int incx,
            [In] ref double beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSger_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDger_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChemv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhemv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChpmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhpmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSger_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDger_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgeru_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgerc_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgeru_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgerc_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCher2_v2(CudaBlasHandle handle,
            FillMode uplo, int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZher2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChpr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhpr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            int incx,
            [In] CUdeviceptr y,
            int incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] ref float beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] ref double beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemm3m(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemm3m(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemm(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref Half alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            ref Half beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemm(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref Half alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            ref Half beta,
            CUdeviceptr c,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref float alpha,
            CUdeviceptr a,
            DataType atype,
            int lda,
            CUdeviceptr b,
            DataType btype,
            int ldb,
            ref float beta,
            CUdeviceptr c,
            DataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            IntPtr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            int lda,
            CUdeviceptr b,
            CudaDataType btype,
            int ldb,
            IntPtr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            int lda,
            CUdeviceptr b,
            CudaDataType btype,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemmEx(CudaBlasHandle handle,
            Operation transa, Operation transb,
            int m, int n, int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            int lda,
            CUdeviceptr b,
            CudaDataType btype,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        [Obsolete]
        public static extern CublasStatus cublasUint8gemmBias(CudaBlasHandle handle,
            Operation transa, Operation transb, Operation transc,
            int m, int n, int k,
            CUdeviceptr a, int aBias, int lda,
            CUdeviceptr b, int bBias, int ldb,
            CUdeviceptr c, int cBias, int ldc,
            int cMult, int cShift);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            DataType atype,
            int lda,
            CUdeviceptr b,
            DataType btype,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            DataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] ref float beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] ref double beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyrkEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            int lda,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyrk3mEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CUdeviceptr lpha,
            CUdeviceptr a,
            CudaDataType atype,
            int lda,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] ref float beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] ref double beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] ref float beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] ref double beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherkEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref float alpha,
            CUdeviceptr a,
            CudaDataType atype,
            int lda,
            ref float beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherk3mEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref float alpha,
            CUdeviceptr a, CudaDataType atype,
            int lda,
            ref float beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherkEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            int lda,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherk3mEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a, CudaDataType atype,
            int lda,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] ref float beta,
            [In] CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] ref double beta,
            [In] CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] ref float beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] ref double beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCher2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZher2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZherkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChemm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhemm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrmm_v2(CudaBlasHandle handle, SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            int lda,
            [In] CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr beta,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr beta,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr beta,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr beta,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            ref float alpha,
            CUdeviceptr a,
            int lda,
            ref float beta,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            ref double alpha,
            CUdeviceptr a,
            int lda,
            ref double beta,
            CUdeviceptr b,
            int ldb,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSmatinvBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr ainv,
            int ldaInv,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDmatinvBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr ainv,
            int ldaInv,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCmatinvBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr ainv,
            int ldaInv,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZmatinvBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr ainv,
            int ldaInv,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr x,
            int incx,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr x,
            int incx,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr x,
            int incx,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr x,
            int incx,
            CUdeviceptr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr barray,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr barray,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr barray,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr barray,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemm3mBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr barray,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemm3mStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            long strideA,
            CUdeviceptr b,
            int ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmBatchedEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            CudaDataType atype,
            int lda,
            CUdeviceptr barray,
            CudaDataType btype,
            int ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            CudaDataType ctype,
            int ldc,
            int batchCount,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmStridedBatchedEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            int lda,
            long strideA,
            CUdeviceptr b,
            CudaDataType btype,
            int ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc,
            long strideC,
            int batchCount,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            long strideA,
            CUdeviceptr b,
            int ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            long strideA,
            CUdeviceptr b,
            int ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            long strideA,
            CUdeviceptr b,
            int ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            long strideA,
            CUdeviceptr b,
            int ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            long strideA,
            CUdeviceptr b,
            int ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref float alpha,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr barray,
            int ldb,
            ref float beta,
            CUdeviceptr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref double alpha,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr barray,
            int ldb,
            ref double beta,
            CUdeviceptr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmBatchedEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            IntPtr alpha,
            CUdeviceptr aarray,
            CudaDataType atype,
            int lda,
            CUdeviceptr barray,
            CudaDataType btype,
            int ldb,
            IntPtr beta,
            CUdeviceptr carray,
            CudaDataType ctype,
            int ldc,
            int batchCount,
            CudaDataType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmStridedBatchedEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            IntPtr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            int lda,
            long strideA,
            CUdeviceptr b,
            CudaDataType btype,
            int ldb,
            long strideB,
            IntPtr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            int ldc,
            long strideC,
            int batchCount,
            CudaDataType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref float alpha,
            CUdeviceptr a,
            int lda,
            long strideA,
            CUdeviceptr b,
            int ldb,
            long strideB,
            ref float beta,
            CUdeviceptr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref double alpha,
            CUdeviceptr a,
            int lda,
            long strideA,
            CUdeviceptr b,
            int ldb,
            long strideB,
            ref double beta,
            CUdeviceptr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref Half alpha,
            CUdeviceptr a,
            int lda,
            long strideA,
            CUdeviceptr b,
            int ldb,
            long strideB,
            ref Half beta,
            CUdeviceptr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgetrfBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr p,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgetrfBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr p,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgetrfBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr p,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgetrfBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr p,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgetriBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr p,
            CUdeviceptr c,
            int ldc,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgetriBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr p,
            CUdeviceptr c,
            int ldc,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgetriBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr p,
            CUdeviceptr c,
            int ldc,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgetriBatched(CudaBlasHandle handle,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr p,
            CUdeviceptr c,
            int ldc,
            CUdeviceptr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            CUdeviceptr salpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            ref float alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            ref double alpha,
            CUdeviceptr a,
            int lda,
            CUdeviceptr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CUdeviceptr ap,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CUdeviceptr ap,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CUdeviceptr ap,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CUdeviceptr ap,
            CUdeviceptr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CUdeviceptr a,
            int lda,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr tauArray,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr tauArray,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr tauArray,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr tauArray,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgelsBatched(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int nrhs,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr carray,
            int ldc,
            ref int info,
            CUdeviceptr devInfoArray,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgelsBatched(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int nrhs,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr carray,
            int ldc,
            ref int info,
            CUdeviceptr devInfoArray,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgelsBatched(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int nrhs,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr carray,
            int ldc,
            ref int info,
            CUdeviceptr devInfoArray,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgelsBatched(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int nrhs,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr carray,
            int ldc,
            ref int info,
            CUdeviceptr devInfoArray,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgetrsBatched(CudaBlasHandle handle,
            Operation trans,
            int n,
            int nrhs,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr devIpiv,
            CUdeviceptr barray,
            int ldb,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgetrsBatched(CudaBlasHandle handle,
            Operation trans,
            int n,
            int nrhs,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr devIpiv,
            CUdeviceptr barray,
            int ldb,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgetrsBatched(CudaBlasHandle handle,
            Operation trans,
            int n,
            int nrhs,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr devIpiv,
            CUdeviceptr barray,
            int ldb,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgetrsBatched(CudaBlasHandle handle,
            Operation trans,
            int n,
            int nrhs,
            CUdeviceptr aarray,
            int lda,
            CUdeviceptr devIpiv,
            CUdeviceptr barray,
            int ldb,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetVector_64(long n, long elemSize, [In] IntPtr x, long incx, CUdeviceptr devicePtr, long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetVector_64(long n, long elemSize, [In] CUdeviceptr x, long incx, IntPtr y, long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetMatrix_64(long rows, long cols, long elemSize, [In] IntPtr a, long lda, CUdeviceptr b, long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetMatrix_64(long rows, long cols, long elemSize, [In] CUdeviceptr a, long lda, IntPtr b, long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetVectorAsync_64(long n, long elemSize, [In] IntPtr hostPtr, long incx, CUdeviceptr devicePtr, long incy, CUstream stream);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetVectorAsync_64(long n, long elemSize, [In] CUdeviceptr devicePtr, long incx, IntPtr hostPtr, long incy, CUstream stream);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSetMatrixAsync_64(long rows, long cols, long elemSize, [In] IntPtr a, long lda, CUdeviceptr b, long ldb, CUstream stream);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGetMatrixAsync_64(long rows, long cols, long elemSize, [In] CUdeviceptr a, long lda, IntPtr b, long ldb, CUstream stream);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCopyEx_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScopy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDcopy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCcopy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZcopy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSswap_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDswap_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCswap_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZswap_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSwapEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasNrm2Ex_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDotEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDotcEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDznrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSdot_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDdot_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScalEx_64(CudaBlasHandle handle,
            long n,
            IntPtr alpha,
            CudaDataType alphaType,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref float alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref double alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref float alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref double alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasAxpyEx_64(CudaBlasHandle handle,
            long n,
            IntPtr alpha,
            CudaDataType alphaType,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIsamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIdamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIcamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIzamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIamaxEx_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x, CudaDataType xType,
            long incx,
            ref long result
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIsamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIdamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIcamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIzamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIaminEx_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x, CudaDataType xType,
            long incx,
            ref long result
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasAsumEx_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            CudaDataType xType,
            long incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDzasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] ref float c,
            [In] ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] ref double c,
            [In] ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] ref float c,
            [In] ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] ref double c,
            [In] ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            IntPtr c,
            IntPtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrotm_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            float[] param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrotm_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            double[] param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotmEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            IntPtr param,
            CudaDataType paramType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasNrm2Ex_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDotEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            CUdeviceptr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDotcEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            CUdeviceptr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDznrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSdot_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDdot_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCdotu_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCdotc_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdotu_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdotc_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScalEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr alpha,
            CudaDataType alphaType,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            CUdeviceptr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasAxpyEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr alpha,
            CudaDataType alphaType,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIsamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIdamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIcamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIzamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIamaxEx_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x, CudaDataType xType,
            long incx,
            CUdeviceptr result
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIsamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIdamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIcamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIzamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasIaminEx_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x, CudaDataType xType,
            long incx,
            CUdeviceptr result
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasAsumEx_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasScasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDzasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr result);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdrot_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] CUdeviceptr c,
            [In] CUdeviceptr s);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            CUdeviceptr c,
            CUdeviceptr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSrotm_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] CUdeviceptr param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDrotm_v2_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            long incx,
            CUdeviceptr y,
            long incy,
            [In] CUdeviceptr param);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasRotmEx_64(CudaBlasHandle handle,
            long n,
            CUdeviceptr x,
            CudaDataType xType,
            long incx,
            CUdeviceptr y,
            CudaDataType yType,
            long incy,
            CUdeviceptr param,
            CudaDataType paramType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CUdeviceptr a, long lda,
            CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr ap, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr ap, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr ap, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr ap,
            CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr a, long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr a, long lda,
            CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr ap,
            CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr ap, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr ap, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CUdeviceptr ap,
            CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CUdeviceptr a,
            long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CUdeviceptr a,
            long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CUdeviceptr a,
            long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CUdeviceptr a,
            long lda, CUdeviceptr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] ref float beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] ref double beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] ref float beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] ref double beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] ref float beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] ref double beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] ref float beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] ref double beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            long incx,
            [In] ref float beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            long incx,
            [In] ref double beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChemv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhemv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChpmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhpmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr ap,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr beta,
            CUdeviceptr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgeru_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgerc_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgeru_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgerc_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCher2_v2_64(CudaBlasHandle handle,
            FillMode uplo, long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZher2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChpr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhpr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr x,
            long incx,
            [In] CUdeviceptr y,
            long incy,
            CUdeviceptr ap);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] ref float beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] ref double beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemm3m_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemm3m_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemm_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref Half alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            ref Half beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemm_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref Half alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            ref Half beta,
            CUdeviceptr c,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref float alpha,
            CUdeviceptr a,
            DataType atype,
            long lda,
            CUdeviceptr b,
            DataType btype,
            long ldb,
            ref float beta,
            CUdeviceptr c,
            DataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            IntPtr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            long lda,
            CUdeviceptr b,
            CudaDataType btype,
            long ldb,
            IntPtr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            long lda,
            CUdeviceptr b,
            CudaDataType btype,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemmEx_64(CudaBlasHandle handle,
            Operation transa, Operation transb,
            long m, long n, long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            long lda,
            CUdeviceptr b,
            CudaDataType btype,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            DataType atype,
            long lda,
            CUdeviceptr b,
            DataType btype,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr c,
            DataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] ref float beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] ref double beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyrkEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            long lda,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyrk3mEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CUdeviceptr lpha,
            CUdeviceptr a,
            CudaDataType atype,
            long lda,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] ref float beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] ref double beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] ref float beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] ref double beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherkEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref float alpha,
            CUdeviceptr a,
            CudaDataType atype,
            long lda,
            ref float beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherk3mEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref float alpha,
            CUdeviceptr a, CudaDataType atype,
            long lda,
            ref float beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherkEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            long lda,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherk3mEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a, CudaDataType atype,
            long lda,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] ref float beta,
            [In] CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] ref double beta,
            [In] CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] ref float beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] ref double beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] ref float alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] ref double alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCher2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZher2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCherkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZherkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            [In] CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasChemm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZhemm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            [In] CUdeviceptr beta,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrmm_v2_64(CudaBlasHandle handle, SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CUdeviceptr alpha,
            [In] CUdeviceptr a,
            long lda,
            [In] CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr beta,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr beta,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr beta,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr beta,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            ref float alpha,
            CUdeviceptr a,
            long lda,
            ref float beta,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            ref double alpha,
            CUdeviceptr a,
            long lda,
            ref double beta,
            CUdeviceptr b,
            long ldb,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CUdeviceptr a,
            long lda,
            CUdeviceptr x,
            long incx,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CUdeviceptr a,
            long lda,
            CUdeviceptr x,
            long incx,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CUdeviceptr a,
            long lda,
            CUdeviceptr x,
            long incx,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CUdeviceptr a,
            long lda,
            CUdeviceptr x,
            long incx,
            CUdeviceptr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            long lda,
            CUdeviceptr barray,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            long lda,
            CUdeviceptr barray,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            long lda,
            CUdeviceptr barray,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            long lda,
            CUdeviceptr barray,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            long ldc,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemm3mBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            long lda,
            CUdeviceptr barray,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            long ldc,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemm3mStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            long strideA,
            CUdeviceptr b,
            long ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmBatchedEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr aarray,
            CudaDataType atype,
            long lda,
            CUdeviceptr barray,
            CudaDataType btype,
            long ldb,
            CUdeviceptr beta,
            CUdeviceptr carray,
            CudaDataType ctype,
            long ldc,
            long batchCount,
            ComputeType computeType,
            GemmAlgo algo);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmStridedBatchedEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            long lda,
            long strideA,
            CUdeviceptr b,
            CudaDataType btype,
            long ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc,
            long strideC,
            long batchCount,
            ComputeType computeType,
            GemmAlgo algo);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            long strideA,
            CUdeviceptr b,
            long ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            long strideA,
            CUdeviceptr b,
            long ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            long strideA,
            CUdeviceptr b,
            long ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            long strideA,
            CUdeviceptr b,
            long ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            long strideA,
            CUdeviceptr b,
            long ldb,
            long strideB,
            CUdeviceptr beta,
            CUdeviceptr c,
            long ldc,
            long strideC,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref float alpha,
            CUdeviceptr aarray,
            long lda,
            CUdeviceptr barray,
            long ldb,
            ref float beta,
            CUdeviceptr carray,
            long ldc,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref double alpha,
            CUdeviceptr aarray,
            long lda,
            CUdeviceptr barray,
            long ldb,
            ref double beta,
            CUdeviceptr carray,
            long ldc,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmBatchedEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            IntPtr alpha,
            CUdeviceptr aarray,
            CudaDataType atype,
            long lda,
            CUdeviceptr barray,
            CudaDataType btype,
            long ldb,
            IntPtr beta,
            CUdeviceptr carray,
            CudaDataType ctype,
            long ldc,
            long batchCount,
            CudaDataType computeType,
            GemmAlgo algo);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasGemmStridedBatchedEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            IntPtr alpha,
            CUdeviceptr a,
            CudaDataType atype,
            long lda,
            long strideA,
            CUdeviceptr b,
            CudaDataType btype,
            long ldb,
            long strideB,
            IntPtr beta,
            CUdeviceptr c,
            CudaDataType ctype,
            long ldc,
            long strideC,
            long batchCount,
            CudaDataType computeType,
            GemmAlgo algo);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasSgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref float alpha,
            CUdeviceptr a,
            long lda,
            long strideA,
            CUdeviceptr b,
            long ldb,
            long strideB,
            ref float beta,
            CUdeviceptr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref double alpha,
            CUdeviceptr a,
            long lda,
            long strideA,
            CUdeviceptr b,
            long ldb,
            long strideB,
            ref double beta,
            CUdeviceptr c,
            long ldc,
            long strideC,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasHgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref Half alpha,
            CUdeviceptr a,
            long lda,
            long strideA,
            CUdeviceptr b,
            long ldb,
            long strideB,
            ref Half beta,
            CUdeviceptr c,
            long ldc,
            long strideC,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasCtrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            CUdeviceptr alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasZtrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            CUdeviceptr salpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasStrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            ref float alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CublasStatus cublasDtrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            ref double alpha,
            CUdeviceptr a,
            long lda,
            CUdeviceptr b,
            long ldb,
            long batchCount);
    }
}