using System;
using System.Runtime.InteropServices;
using BrightData.Cuda.CudaToolkit.Types;

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
        public static extern CuBlasStatus cublasCreate_v2(ref CudaBlasHandle handle);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDestroy_v2(CudaBlasHandle handle);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetVersion_v2(CudaBlasHandle handle, ref int version);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetWorkspace_v2(CudaBlasHandle handle, CuDevicePtr workspace, SizeT workspaceSizeInBytes);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetStream_v2(CudaBlasHandle handle, CuStream streamId);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetStream_v2(CudaBlasHandle handle, ref CuStream streamId);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetPointerMode_v2(CudaBlasHandle handle, ref PointerMode mode);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetPointerMode_v2(CudaBlasHandle handle, PointerMode mode);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetAtomicsMode(CudaBlasHandle handle, ref AtomicsMode mode);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetAtomicsMode(CudaBlasHandle handle, AtomicsMode mode);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetMathMode(CudaBlasHandle handle, ref Math mode);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetMathMode(CudaBlasHandle handle, Math mode);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetSmCountTarget(CudaBlasHandle handle, ref int smCountTarget);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetSmCountTarget(CudaBlasHandle handle, int smCountTarget);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasLoggerConfigure(int logIsOn, int logToStdOut, int logToStdErr, [MarshalAs(UnmanagedType.LPStr)] string logFileName);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetLoggerCallback(CublasLogCallback userCallback);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetLoggerCallback(ref CublasLogCallback userCallback);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetVector(int n, int elemSize, [In] IntPtr x, int incx, CuDevicePtr devicePtr, int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetVector(int n, int elemSize, [In] CuDevicePtr x, int incx, IntPtr y, int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetMatrix(int rows, int cols, int elemSize, [In] IntPtr a, int lda, CuDevicePtr b, int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetMatrix(int rows, int cols, int elemSize, [In] CuDevicePtr a, int lda, IntPtr b, int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetVectorAsync(int n, int elemSize, [In] IntPtr hostPtr, int incx, CuDevicePtr devicePtr, int incy, CuStream stream);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetVectorAsync(int n, int elemSize, [In] CuDevicePtr devicePtr, int incx, IntPtr hostPtr, int incy, CuStream stream);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetMatrixAsync(int rows, int cols, int elemSize, [In] IntPtr a, int lda, CuDevicePtr b, int ldb, CuStream stream);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetMatrixAsync(int rows, int cols, int elemSize, [In] CuDevicePtr a, int lda, IntPtr b, int ldb, CuStream stream);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCopyEx(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScopy_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDcopy_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCcopy_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZcopy_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSswap_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDswap_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCswap_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZswap_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSwapEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasNrm2Ex(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDotEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDotcEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDznrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSdot_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDdot_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScalEx(CudaBlasHandle handle,
            int n,
            IntPtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSscal_v2(CudaBlasHandle handle,
            int n,
            [In] ref float alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDscal_v2(CudaBlasHandle handle,
            int n,
            [In] ref double alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsscal_v2(CudaBlasHandle handle,
            int n,
            [In] ref float alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdscal_v2(CudaBlasHandle handle,
            int n,
            [In] ref double alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasAxpyEx(CudaBlasHandle handle,
            int n,
            IntPtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIsamax_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIdamax_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIcamax_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIzamax_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIamaxEx(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x, CudaDataType xType,
            int incx,
            ref int result
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIsamin_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIdamin_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIcamin_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIzamin_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref int result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIaminEx(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x, CudaDataType xType,
            int incx,
            ref int result
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasAsumEx(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            CudaDataType xType,
            int incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSasum_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDasum_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScasum_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDzasum_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] ref float c,
            [In] ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] ref double c,
            [In] ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] ref float c,
            [In] ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] ref double c,
            [In] ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            IntPtr c,
            IntPtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrotg_v2(CudaBlasHandle handle,
            ref float a,
            ref float b,
            ref float c,
            ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrotg_v2(CudaBlasHandle handle,
            ref double a,
            ref double b,
            ref double c,
            ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotgEx(CudaBlasHandle handle,
            IntPtr a,
            IntPtr b,
            CudaDataType abType,
            IntPtr c,
            IntPtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrotm_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            float[] param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrotm_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            double[] param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotmEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            IntPtr param,
            CudaDataType paramType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrotmg_v2(CudaBlasHandle handle,
            ref float d1,
            ref float d2,
            ref float x1,
            [In] ref float y1,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            float[] param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrotmg_v2(CudaBlasHandle handle,
            ref double d1,
            ref double d2,
            ref double x1,
            [In] ref double y1,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            double[] param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotmgEx(CudaBlasHandle handle,
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
        public static extern CuBlasStatus cublasNrm2Ex(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDotEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDotcEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScnrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDznrm2_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSdot_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDdot_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCdotu_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCdotc_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdotu_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdotc_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScalEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSscal_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDscal_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCscal_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsscal_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZscal_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdscal_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasAxpyEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZaxpy_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIsamax_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIdamax_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIcamax_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIzamax_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIamaxEx(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x, CudaDataType xType,
            int incx,
            CuDevicePtr result
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIsamin_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIdamin_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIcamin_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIzamin_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIaminEx(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x, CudaDataType xType,
            int incx,
            CuDevicePtr result
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasAsumEx(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSasum_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDasum_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScasum_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDzasum_v2(CudaBlasHandle handle,
            int n,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            CuDevicePtr c,
            CuDevicePtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrotg_v2(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CuDevicePtr c,
            CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrotg_v2(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CuDevicePtr c,
            CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCrotg_v2(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CuDevicePtr c,
            CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZrotg_v2(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CuDevicePtr c,
            CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotgEx(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CudaDataType abType,
            CuDevicePtr c,
            CuDevicePtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrotm_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] CuDevicePtr param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrotm_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [In] CuDevicePtr param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotmEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy,
            CuDevicePtr param,
            CudaDataType paramType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrotmg_v2(CudaBlasHandle handle,
            CuDevicePtr d1,
            CuDevicePtr d2,
            CuDevicePtr x1,
            [In] CuDevicePtr y1,
            CuDevicePtr param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrotmg_v2(CudaBlasHandle handle,
            CuDevicePtr d1,
            CuDevicePtr d2,
            CuDevicePtr x1,
            [In] CuDevicePtr y1,
            CuDevicePtr param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotmgEx(CudaBlasHandle handle,
            CuDevicePtr d1,
            CudaDataType d1Type,
            CuDevicePtr d2,
            CudaDataType d2Type,
            CuDevicePtr x1,
            CudaDataType x1Type,
            [In] CuDevicePtr y1,
            CudaDataType y1Type,
            CuDevicePtr param,
            CudaDataType paramType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CuDevicePtr a, int lda,
            CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr ap, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr ap, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr ap, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr ap,
            CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr a, int lda,
            CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr ap,
            CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr ap, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr ap, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, [In] CuDevicePtr ap,
            CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CuDevicePtr a,
            int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CuDevicePtr a,
            int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CuDevicePtr a,
            int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, [In] CuDevicePtr a,
            int lda, CuDevicePtr x, int incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] ref float beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] ref double beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] ref float beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] ref double beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] ref float beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] ref double beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] ref float beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] ref double beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            int incx,
            [In] ref float beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            int incx,
            [In] ref double beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSger_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDger_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChemv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhemv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChpmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhpmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSger_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDger_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgeru_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgerc_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgeru_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgerc_v2(CudaBlasHandle handle,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCher2_v2(CudaBlasHandle handle,
            FillMode uplo, int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZher2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChpr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhpr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            int incx,
            [In] CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] ref float beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] ref double beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemm3m(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemm3m(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemm(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref CudaHalf alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref CudaHalf beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemm(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref CudaHalf alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref CudaHalf beta,
            CuDevicePtr c,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a,
            DataType atype,
            int lda,
            CuDevicePtr b,
            DataType btype,
            int ldb,
            ref float beta,
            CuDevicePtr c,
            DataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            IntPtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            int lda,
            CuDevicePtr b,
            CudaDataType btype,
            int ldb,
            IntPtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            int lda,
            CuDevicePtr b,
            CudaDataType btype,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemmEx(CudaBlasHandle handle,
            Operation transa, Operation transb,
            int m, int n, int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            int lda,
            CuDevicePtr b,
            CudaDataType btype,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        [Obsolete]
        public static extern CuBlasStatus cublasUint8gemmBias(CudaBlasHandle handle,
            Operation transa, Operation transb, Operation transc,
            int m, int n, int k,
            CuDevicePtr a, int aBias, int lda,
            CuDevicePtr b, int bBias, int ldb,
            CuDevicePtr c, int cBias, int ldc,
            int cMult, int cShift);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            DataType atype,
            int lda,
            CuDevicePtr b,
            DataType btype,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            DataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] ref float beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] ref double beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyrkEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyrk3mEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr lpha,
            CuDevicePtr a,
            CudaDataType atype,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] ref float beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] ref double beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] ref float beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] ref double beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherkEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a,
            CudaDataType atype,
            int lda,
            ref float beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherk3mEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a, CudaDataType atype,
            int lda,
            ref float beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherkEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherk3mEx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a, CudaDataType atype,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] ref float beta,
            [In] CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] ref double beta,
            [In] CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] ref float beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] ref double beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCher2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZher2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZherkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChemm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhemm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrsm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrmm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrmm_v2(CudaBlasHandle handle, SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            int lda,
            [In] CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            ref float beta,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgeam(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            ref double beta,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSmatinvBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ainv,
            int ldaInv,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDmatinvBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ainv,
            int ldaInv,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCmatinvBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ainv,
            int ldaInv,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZmatinvBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ainv,
            int ldaInv,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr c,
            int ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr barray,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr barray,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr barray,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr barray,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemm3mBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr barray,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemm3mStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            long strideA,
            CuDevicePtr b,
            int ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmBatchedEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            CudaDataType atype,
            int lda,
            CuDevicePtr barray,
            CudaDataType btype,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            CudaDataType ctype,
            int ldc,
            int batchCount,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmStridedBatchedEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            int lda,
            long strideA,
            CuDevicePtr b,
            CudaDataType btype,
            int ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc,
            long strideC,
            int batchCount,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            long strideA,
            CuDevicePtr b,
            int ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            long strideA,
            CuDevicePtr b,
            int ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            long strideA,
            CuDevicePtr b,
            int ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            long strideA,
            CuDevicePtr b,
            int ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            long strideA,
            CuDevicePtr b,
            int ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr barray,
            int ldb,
            ref float beta,
            CuDevicePtr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemmBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref double alpha,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr barray,
            int ldb,
            ref double beta,
            CuDevicePtr carray,
            int ldc,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmBatchedEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            IntPtr alpha,
            CuDevicePtr aarray,
            CudaDataType atype,
            int lda,
            CuDevicePtr barray,
            CudaDataType btype,
            int ldb,
            IntPtr beta,
            CuDevicePtr carray,
            CudaDataType ctype,
            int ldc,
            int batchCount,
            CudaDataType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmStridedBatchedEx(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            IntPtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            int lda,
            long strideA,
            CuDevicePtr b,
            CudaDataType btype,
            int ldb,
            long strideB,
            IntPtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            int ldc,
            long strideC,
            int batchCount,
            CudaDataType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            long strideA,
            CuDevicePtr b,
            int ldb,
            long strideB,
            ref float beta,
            CuDevicePtr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            long strideA,
            CuDevicePtr b,
            int ldb,
            long strideB,
            ref double beta,
            CuDevicePtr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemmStridedBatched(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref CudaHalf alpha,
            CuDevicePtr a,
            int lda,
            long strideA,
            CuDevicePtr b,
            int ldb,
            long strideB,
            ref CudaHalf beta,
            CuDevicePtr c,
            int ldc,
            long strideC,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgetrfBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgetrfBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgetrfBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgetrfBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgetriBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr c,
            int ldc,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgetriBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr c,
            int ldc,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgetriBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr c,
            int ldc,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgetriBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr c,
            int ldc,
            CuDevicePtr info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            CuDevicePtr salpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsmBatched(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            int m,
            int n,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            int batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr ap,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr ap,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr ap,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr ap,
            CuDevicePtr a,
            int lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr tauArray,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr tauArray,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr tauArray,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr tauArray,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgelsBatched(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int nrhs,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr carray,
            int ldc,
            ref int info,
            CuDevicePtr devInfoArray,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgelsBatched(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int nrhs,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr carray,
            int ldc,
            ref int info,
            CuDevicePtr devInfoArray,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgelsBatched(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int nrhs,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr carray,
            int ldc,
            ref int info,
            CuDevicePtr devInfoArray,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgelsBatched(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int nrhs,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr carray,
            int ldc,
            ref int info,
            CuDevicePtr devInfoArray,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgetrsBatched(CudaBlasHandle handle,
            Operation trans,
            int n,
            int nrhs,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr devIpiv,
            CuDevicePtr barray,
            int ldb,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgetrsBatched(CudaBlasHandle handle,
            Operation trans,
            int n,
            int nrhs,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr devIpiv,
            CuDevicePtr barray,
            int ldb,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgetrsBatched(CudaBlasHandle handle,
            Operation trans,
            int n,
            int nrhs,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr devIpiv,
            CuDevicePtr barray,
            int ldb,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgetrsBatched(CudaBlasHandle handle,
            Operation trans,
            int n,
            int nrhs,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr devIpiv,
            CuDevicePtr barray,
            int ldb,
            ref int info,
            int batchSize);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetVector_64(long n, long elemSize, [In] IntPtr x, long incx, CuDevicePtr devicePtr, long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetVector_64(long n, long elemSize, [In] CuDevicePtr x, long incx, IntPtr y, long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetMatrix_64(long rows, long cols, long elemSize, [In] IntPtr a, long lda, CuDevicePtr b, long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetMatrix_64(long rows, long cols, long elemSize, [In] CuDevicePtr a, long lda, IntPtr b, long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetVectorAsync_64(long n, long elemSize, [In] IntPtr hostPtr, long incx, CuDevicePtr devicePtr, long incy, CuStream stream);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetVectorAsync_64(long n, long elemSize, [In] CuDevicePtr devicePtr, long incx, IntPtr hostPtr, long incy, CuStream stream);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSetMatrixAsync_64(long rows, long cols, long elemSize, [In] IntPtr a, long lda, CuDevicePtr b, long ldb, CuStream stream);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGetMatrixAsync_64(long rows, long cols, long elemSize, [In] CuDevicePtr a, long lda, IntPtr b, long ldb, CuStream stream);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCopyEx_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScopy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDcopy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCcopy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZcopy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSswap_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDswap_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCswap_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZswap_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSwapEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasNrm2Ex_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDotEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDotcEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDznrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSdot_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDdot_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScalEx_64(CudaBlasHandle handle,
            long n,
            IntPtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref float alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref double alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref float alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref double alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasAxpyEx_64(CudaBlasHandle handle,
            long n,
            IntPtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIsamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIdamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIcamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIzamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIamaxEx_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x, CudaDataType xType,
            long incx,
            ref long result
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIsamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIdamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIcamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIzamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref long result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIaminEx_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x, CudaDataType xType,
            long incx,
            ref long result
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasAsumEx_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            CudaDataType xType,
            long incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref float result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDzasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            ref double result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] ref float c,
            [In] ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] ref double c,
            [In] ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] ref float c,
            [In] ref float s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] ref double c,
            [In] ref double s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            IntPtr c,
            IntPtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrotm_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            float[] param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrotm_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            double[] param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotmEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            IntPtr param,
            CudaDataType paramType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasNrm2Ex_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDotEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDotcEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScnrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDznrm2_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSdot_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDdot_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCdotu_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCdotc_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdotu_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdotc_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScalEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CudaDataType executionType);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdscal_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasAxpyEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZaxpy_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIsamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIdamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIcamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIzamax_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIamaxEx_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x, CudaDataType xType,
            long incx,
            CuDevicePtr result
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIsamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIdamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIcamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIzamin_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasIaminEx_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x, CudaDataType xType,
            long incx,
            CuDevicePtr result
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasAsumEx_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasScasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDzasum_v2_64(CudaBlasHandle handle,
            long n,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] CuDevicePtr c,
            [In] CuDevicePtr s);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            CuDevicePtr c,
            CuDevicePtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSrotm_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] CuDevicePtr param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDrotm_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [In] CuDevicePtr param);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasRotmEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy,
            CuDevicePtr param,
            CudaDataType paramType,
            CudaDataType executiontype);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CuDevicePtr a, long lda,
            CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr ap, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr ap, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr ap, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr ap,
            CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr a, long lda,
            CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr ap,
            CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr ap, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr ap, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, [In] CuDevicePtr ap,
            CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CuDevicePtr a,
            long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CuDevicePtr a,
            long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CuDevicePtr a,
            long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, [In] CuDevicePtr a,
            long lda, CuDevicePtr x, long incx);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] ref float beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] ref double beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] ref float beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] ref double beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] ref float beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] ref double beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] ref float beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] ref double beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            long incx,
            [In] ref float beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            long incx,
            [In] ref double beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChemv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhemv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChpmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhpmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr ap,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgeru_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgerc_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgeru_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgerc_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCher2_v2_64(CudaBlasHandle handle,
            FillMode uplo, long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZher2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChpr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhpr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr x,
            long incx,
            [In] CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] ref float beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] ref double beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemm3m_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemm3m_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemm_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref CudaHalf alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref CudaHalf beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemm_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref CudaHalf alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref CudaHalf beta,
            CuDevicePtr c,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a,
            DataType atype,
            long lda,
            CuDevicePtr b,
            DataType btype,
            long ldb,
            ref float beta,
            CuDevicePtr c,
            DataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            IntPtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            long lda,
            CuDevicePtr b,
            CudaDataType btype,
            long ldb,
            IntPtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            long lda,
            CuDevicePtr b,
            CudaDataType btype,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc,
            ComputeType computeType,
            GemmAlgo algo);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemmEx_64(CudaBlasHandle handle,
            Operation transa, Operation transb,
            long m, long n, long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            long lda,
            CuDevicePtr b,
            CudaDataType btype,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            DataType atype,
            long lda,
            CuDevicePtr b,
            DataType btype,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            DataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] ref float beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] ref double beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyrkEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyrk3mEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr lpha,
            CuDevicePtr a,
            CudaDataType atype,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] ref float beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] ref double beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] ref float beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] ref double beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherkEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a,
            CudaDataType atype,
            long lda,
            ref float beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherk3mEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a, CudaDataType atype,
            long lda,
            ref float beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherkEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherk3mEx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a, CudaDataType atype,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] ref float beta,
            [In] CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] ref double beta,
            [In] CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] ref float beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] ref double beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] ref float alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] ref double alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCher2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZher2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCherkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZherkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            [In] CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasChemm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZhemm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            [In] CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrsm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrmm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrmm_v2_64(CudaBlasHandle handle, SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            [In] CuDevicePtr alpha,
            [In] CuDevicePtr a,
            long lda,
            [In] CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            ref float beta,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgeam_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            ref double beta,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr c,
            long ldc);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            long lda,
            CuDevicePtr barray,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            long lda,
            CuDevicePtr barray,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            long lda,
            CuDevicePtr barray,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            long ldc,
            long batchCount);
        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            long lda,
            CuDevicePtr barray,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            long ldc,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemm3mBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            long lda,
            CuDevicePtr barray,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            long ldc,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemm3mStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            long strideA,
            CuDevicePtr b,
            long ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmBatchedEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr aarray,
            CudaDataType atype,
            long lda,
            CuDevicePtr barray,
            CudaDataType btype,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr carray,
            CudaDataType ctype,
            long ldc,
            long batchCount,
            ComputeType computeType,
            GemmAlgo algo);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmStridedBatchedEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            long lda,
            long strideA,
            CuDevicePtr b,
            CudaDataType btype,
            long ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc,
            long strideC,
            long batchCount,
            ComputeType computeType,
            GemmAlgo algo);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            long strideA,
            CuDevicePtr b,
            long ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            long strideA,
            CuDevicePtr b,
            long ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            long strideA,
            CuDevicePtr b,
            long ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            long strideA,
            CuDevicePtr b,
            long ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            long strideA,
            CuDevicePtr b,
            long ldb,
            long strideB,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc,
            long strideC,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr aarray,
            long lda,
            CuDevicePtr barray,
            long ldb,
            ref float beta,
            CuDevicePtr carray,
            long ldc,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemmBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref double alpha,
            CuDevicePtr aarray,
            long lda,
            CuDevicePtr barray,
            long ldb,
            ref double beta,
            CuDevicePtr carray,
            long ldc,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmBatchedEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            IntPtr alpha,
            CuDevicePtr aarray,
            CudaDataType atype,
            long lda,
            CuDevicePtr barray,
            CudaDataType btype,
            long ldb,
            IntPtr beta,
            CuDevicePtr carray,
            CudaDataType ctype,
            long ldc,
            long batchCount,
            CudaDataType computeType,
            GemmAlgo algo);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasGemmStridedBatchedEx_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            IntPtr alpha,
            CuDevicePtr a,
            CudaDataType atype,
            long lda,
            long strideA,
            CuDevicePtr b,
            CudaDataType btype,
            long ldb,
            long strideB,
            IntPtr beta,
            CuDevicePtr c,
            CudaDataType ctype,
            long ldc,
            long strideC,
            long batchCount,
            CudaDataType computeType,
            GemmAlgo algo);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasSgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            long strideA,
            CuDevicePtr b,
            long ldb,
            long strideB,
            ref float beta,
            CuDevicePtr c,
            long ldc,
            long strideC,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            long strideA,
            CuDevicePtr b,
            long ldb,
            long strideB,
            ref double beta,
            CuDevicePtr c,
            long ldc,
            long strideC,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasHgemmStridedBatched_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref CudaHalf alpha,
            CuDevicePtr a,
            long lda,
            long strideA,
            CuDevicePtr b,
            long ldb,
            long strideB,
            ref CudaHalf beta,
            CuDevicePtr c,
            long ldc,
            long strideC,
            long batchCount);


        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasCtrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasZtrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            CuDevicePtr salpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasStrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            long batchCount);

        [DllImport(CublasApiDllName)]
        public static extern CuBlasStatus cublasDtrsmBatched_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            Operation trans,
            DiagType diag,
            long m,
            long n,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            long batchCount);
    }
}