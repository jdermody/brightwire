using System;
using System.Runtime.InteropServices;
using BrightData.Cuda.CudaToolkit.Types;

namespace BrightData.Cuda.CudaToolkit
{
    internal static partial class CudaBlasNativeMethods
    {
        internal const string CublasApiDllName = DriverApiNativeMethods.CublasApiDllName;

        static CudaBlasNativeMethods()
        {
            DriverApiNativeMethods.Init();
        }

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCreate_v2(ref CudaBlasHandle handle);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDestroy_v2(CudaBlasHandle handle);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetVersion_v2(CudaBlasHandle handle, ref int version);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetWorkspace_v2(CudaBlasHandle handle, CuDevicePtr workspace, SizeT workspaceSizeInBytes);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetStream_v2(CudaBlasHandle handle, CuStream streamId);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetStream_v2(CudaBlasHandle handle, ref CuStream streamId);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetPointerMode_v2(CudaBlasHandle handle, ref PointerMode mode);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetPointerMode_v2(CudaBlasHandle handle, PointerMode mode);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetAtomicsMode(CudaBlasHandle handle, ref AtomicsMode mode);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetAtomicsMode(CudaBlasHandle handle, AtomicsMode mode);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetMathMode(CudaBlasHandle handle, ref Math mode);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetMathMode(CudaBlasHandle handle, Math mode);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetSmCountTarget(CudaBlasHandle handle, ref int smCountTarget);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetSmCountTarget(CudaBlasHandle handle, int smCountTarget);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasLoggerConfigure(int logIsOn, int logToStdOut, int logToStdErr, [MarshalAs(UnmanagedType.LPStr)] string logFileName);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetLoggerCallback(CublasLogCallback userCallback);

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetLoggerCallback(ref CublasLogCallback userCallback);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetVector(int n, int elemSize, IntPtr x, int incx, CuDevicePtr devicePtr, int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetVector(int n, int elemSize, CuDevicePtr x, int incx, IntPtr y, int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetMatrix(int rows, int cols, int elemSize, IntPtr a, int lda, CuDevicePtr b, int ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetMatrix(int rows, int cols, int elemSize, CuDevicePtr a, int lda, IntPtr b, int ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetVectorAsync(int n, int elemSize, IntPtr hostPtr, int incx, CuDevicePtr devicePtr, int incy, CuStream stream);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetVectorAsync(int n, int elemSize, CuDevicePtr devicePtr, int incx, IntPtr hostPtr, int incy, CuStream stream);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetMatrixAsync(int rows, int cols, int elemSize, IntPtr a, int lda, CuDevicePtr b, int ldb, CuStream stream);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetMatrixAsync(int rows, int cols, int elemSize, CuDevicePtr a, int lda, IntPtr b, int ldb, CuStream stream);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCopyEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScopy_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDcopy_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCcopy_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZcopy_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSswap_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDswap_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCswap_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZswap_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSwapEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr y,
            CudaDataType yType,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasNrm2Ex(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDotEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDotcEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSnrm2_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDnrm2_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScnrm2_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDznrm2_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSdot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDdot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScalEx(CudaBlasHandle handle,
            int n,
            IntPtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CudaDataType executionType);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSscal_v2(CudaBlasHandle handle,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDscal_v2(CudaBlasHandle handle,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsscal_v2(CudaBlasHandle handle,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdscal_v2(CudaBlasHandle handle,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasAxpyEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSaxpy_v2(CudaBlasHandle handle,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDaxpy_v2(CudaBlasHandle handle,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIsamax_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref int result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIdamax_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref int result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIcamax_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref int result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIzamax_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref int result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIamaxEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x, CudaDataType xType,
            int incx,
            ref int result
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIsamin_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref int result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIdamin_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref int result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIcamin_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref int result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIzamin_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref int result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIaminEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x, CudaDataType xType,
            int incx,
            ref int result
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasAsumEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSasum_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDasum_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScasum_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDzasum_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            ref float c,
            ref float s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            ref double c,
            ref double s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            ref float c,
            ref float s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            ref double c,
            ref double s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrotg_v2(CudaBlasHandle handle,
            ref float a,
            ref float b,
            ref float c,
            ref float s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrotg_v2(CudaBlasHandle handle,
            ref double a,
            ref double b,
            ref double c,
            ref double s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotgEx(CudaBlasHandle handle,
            IntPtr a,
            IntPtr b,
            CudaDataType abType,
            IntPtr c,
            IntPtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrotm_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            float[] param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrotm_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            double[] param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotmEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrotmg_v2(CudaBlasHandle handle,
            ref float d1,
            ref float d2,
            ref float x1,
            ref float y1,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            float[] param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrotmg_v2(CudaBlasHandle handle,
            ref double d1,
            ref double d2,
            ref double x1,
            ref double y1,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            double[] param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotmgEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasNrm2Ex(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDotEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDotcEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSnrm2_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDnrm2_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScnrm2_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDznrm2_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSdot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDdot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCdotu_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCdotc_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdotu_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdotc_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScalEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CudaDataType executionType);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSscal_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDscal_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCscal_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsscal_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZscal_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdscal_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasAxpyEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSaxpy_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDaxpy_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCaxpy_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZaxpy_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIsamax_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIdamax_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIcamax_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIzamax_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIamaxEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x, CudaDataType xType,
            int incx,
            CuDevicePtr result
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIsamin_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIdamin_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIcamin_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIzamin_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIaminEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x, CudaDataType xType,
            int incx,
            CuDevicePtr result
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasAsumEx(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            CudaDataType xType,
            int incx,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSasum_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDasum_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScasum_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDzasum_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdrot_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrotg_v2(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrotg_v2(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCrotg_v2(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZrotg_v2(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotgEx(CudaBlasHandle handle,
            CuDevicePtr a,
            CuDevicePtr b,
            CudaDataType abType,
            CuDevicePtr c,
            CuDevicePtr s,
            CudaDataType csType,
            CudaDataType executiontype);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrotm_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrotm_v2(CudaBlasHandle handle,
            int n,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotmEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrotmg_v2(CudaBlasHandle handle,
            CuDevicePtr d1,
            CuDevicePtr d2,
            CuDevicePtr x1,
            CuDevicePtr y1,
            CuDevicePtr param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrotmg_v2(CudaBlasHandle handle,
            CuDevicePtr d1,
            CuDevicePtr d2,
            CuDevicePtr x1,
            CuDevicePtr y1,
            CuDevicePtr param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotmgEx(CudaBlasHandle handle,
            CuDevicePtr d1,
            CudaDataType d1Type,
            CuDevicePtr d2,
            CudaDataType d2Type,
            CuDevicePtr x1,
            CudaDataType x1Type,
            CuDevicePtr y1,
            CudaDataType y1Type,
            CuDevicePtr param,
            CudaDataType paramType,
            CudaDataType executiontype
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtbmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, CuDevicePtr a, int lda,
            CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr ap, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr ap, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr ap, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtpmv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr ap,
            CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr a, int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr a, int lda,
            CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr ap,
            CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr ap, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr ap, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtpsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, CuDevicePtr ap,
            CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, CuDevicePtr a,
            int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, CuDevicePtr a,
            int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, CuDevicePtr a,
            int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtbsv_v2(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, int n, int k, CuDevicePtr a,
            int lda, CuDevicePtr x, int incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            ref float beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            ref double beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            ref float beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            ref double beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            ref float beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            ref double beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            ref float beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            ref double beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref float alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            int incx,
            ref float beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref double alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            int incx,
            ref double beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSger_v2(CudaBlasHandle handle,
            int m,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDger_v2(CudaBlasHandle handle,
            int m,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref float alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            ref double alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgbmv_v2(CudaBlasHandle handle,
            Operation trans,
            int m,
            int n,
            int kl,
            int ku,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsymv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChemv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhemv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhbmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChpmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhpmv_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            int incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            int incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSger_v2(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDger_v2(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgeru_v2(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgerc_v2(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgeru_v2(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgerc_v2(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZher_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhpr_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCher2_v2(CudaBlasHandle handle,
            FillMode uplo, int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZher2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChpr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhpr2_v2(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            int incx,
            CuDevicePtr y,
            int incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref float beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemm_v2(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            int m,
            int n,
            int k,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref double beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemm3m(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemm3m(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemm(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemm(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemmEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        [Obsolete]
        public static partial CuBlasStatus cublasUint8gemmBias(CudaBlasHandle handle,
            Operation transa, Operation transb, Operation transc,
            int m, int n, int k,
            CuDevicePtr a, int aBias, int lda,
            CuDevicePtr b, int bBias, int ldb,
            CuDevicePtr c, int cBias, int ldc,
            int cMult, int cShift);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            ref float beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            ref double beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyrkEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyrk3mEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            ref float beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            ref double beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref float beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref double beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherkEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherk3mEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherkEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherk3mEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref float beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref double beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            ref float alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref float beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            ref double alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            ref double beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsm_v2(CudaBlasHandle handle,
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
            int ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsm_v2(CudaBlasHandle handle,
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
            int ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrmm_v2(CudaBlasHandle handle,
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
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrmm_v2(CudaBlasHandle handle,
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
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemm_v2(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemm_v2(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemm_v2(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemm_v2(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyrk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZherk_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            int n,
            int k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyr2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCher2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZher2k_v2(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyrkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZherkx(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsymm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChemm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhemm_v2(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            int m,
            int n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            int lda,
            CuDevicePtr b,
            int ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsm_v2(CudaBlasHandle handle,
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
            int ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsm_v2(CudaBlasHandle handle,
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
            int ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrsm_v2(CudaBlasHandle handle,
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
            int ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrsm_v2(CudaBlasHandle handle,
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
            int ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrmm_v2(CudaBlasHandle handle,
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
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrmm_v2(CudaBlasHandle handle,
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
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrmm_v2(CudaBlasHandle handle,
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
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrmm_v2(CudaBlasHandle handle, SideMode side,
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
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgeam(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgeam(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgeam(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgeam(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgeam(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgeam(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSmatinvBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ainv,
            int ldaInv,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDmatinvBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ainv,
            int ldaInv,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCmatinvBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ainv,
            int ldaInv,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZmatinvBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ainv,
            int ldaInv,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdgmm(CudaBlasHandle handle,
            SideMode mode,
            int m,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr x,
            int incx,
            CuDevicePtr c,
            int ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemm3mBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemm3mStridedBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmBatchedEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmStridedBatchedEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmStridedBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemmStridedBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemmStridedBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemmStridedBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemmStridedBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmBatchedEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmStridedBatchedEx(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmStridedBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemmStridedBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemmStridedBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgetrfBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgetrfBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgetrfBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgetrfBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgetriBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr c,
            int ldc,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgetriBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr c,
            int ldc,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgetriBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr c,
            int ldc,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgetriBatched(CudaBlasHandle handle,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr p,
            CuDevicePtr c,
            int ldc,
            CuDevicePtr info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrsmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrsmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsmBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr ap,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr ap,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr ap,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtpttr(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr ap,
            CuDevicePtr a,
            int lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrttp(CudaBlasHandle handle,
            FillMode uplo,
            int n,
            CuDevicePtr a,
            int lda,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr tauArray,
            ref int info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr tauArray,
            ref int info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr tauArray,
            ref int info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgeqrfBatched(CudaBlasHandle handle,
            int m,
            int n,
            CuDevicePtr aarray,
            int lda,
            CuDevicePtr tauArray,
            ref int info,
            int batchSize);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgelsBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgelsBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgelsBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgelsBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgetrsBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgetrsBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgetrsBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgetrsBatched(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetVector_64(long n, long elemSize, IntPtr x, long incx, CuDevicePtr devicePtr, long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetVector_64(long n, long elemSize, CuDevicePtr x, long incx, IntPtr y, long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetMatrix_64(long rows, long cols, long elemSize, IntPtr a, long lda, CuDevicePtr b, long ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetMatrix_64(long rows, long cols, long elemSize, CuDevicePtr a, long lda, IntPtr b, long ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetVectorAsync_64(long n, long elemSize, IntPtr hostPtr, long incx, CuDevicePtr devicePtr, long incy, CuStream stream);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetVectorAsync_64(long n, long elemSize, CuDevicePtr devicePtr, long incx, IntPtr hostPtr, long incy, CuStream stream);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSetMatrixAsync_64(long rows, long cols, long elemSize, IntPtr a, long lda, CuDevicePtr b, long ldb, CuStream stream);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGetMatrixAsync_64(long rows, long cols, long elemSize, CuDevicePtr a, long lda, IntPtr b, long ldb, CuStream stream);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCopyEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScopy_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDcopy_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCcopy_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZcopy_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSswap_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDswap_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCswap_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZswap_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSwapEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr y,
            CudaDataType yType,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasNrm2Ex_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDotEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDotcEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSnrm2_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDnrm2_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScnrm2_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDznrm2_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSdot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDdot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScalEx_64(CudaBlasHandle handle,
            long n,
            IntPtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CudaDataType executionType);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSscal_v2_64(CudaBlasHandle handle,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDscal_v2_64(CudaBlasHandle handle,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsscal_v2_64(CudaBlasHandle handle,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdscal_v2_64(CudaBlasHandle handle,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasAxpyEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSaxpy_v2_64(CudaBlasHandle handle,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDaxpy_v2_64(CudaBlasHandle handle,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIsamax_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref long result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIdamax_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref long result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIcamax_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref long result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIzamax_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref long result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIamaxEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x, CudaDataType xType,
            long incx,
            ref long result
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIsamin_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref long result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIdamin_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref long result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIcamin_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref long result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIzamin_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref long result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIaminEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x, CudaDataType xType,
            long incx,
            ref long result
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasAsumEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            IntPtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSasum_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDasum_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScasum_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref float result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDzasum_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            ref double result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            ref float c,
            ref float s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            ref double c,
            ref double s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            ref float c,
            ref float s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            ref double c,
            ref double s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrotm_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            float[] param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrotm_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
            double[] param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotmEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasNrm2Ex_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executionType);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDotEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDotcEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSnrm2_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDnrm2_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScnrm2_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDznrm2_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSdot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDdot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCdotu_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCdotc_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdotu_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdotc_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScalEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CudaDataType alphaType,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CudaDataType executionType);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSscal_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDscal_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCscal_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsscal_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZscal_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdscal_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasAxpyEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSaxpy_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDaxpy_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCaxpy_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZaxpy_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIsamax_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIdamax_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIcamax_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIzamax_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIamaxEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x, CudaDataType xType,
            long incx,
            CuDevicePtr result
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIsamin_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIdamin_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIcamin_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIzamin_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasIaminEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x, CudaDataType xType,
            long incx,
            CuDevicePtr result
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasAsumEx_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            CudaDataType xType,
            long incx,
            CuDevicePtr result,
            CudaDataType resultType,
            CudaDataType executiontype
        );
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSasum_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDasum_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasScasum_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDzasum_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr result);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdrot_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr c,
            CuDevicePtr s);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSrotm_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDrotm_v2_64(CudaBlasHandle handle,
            long n,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr param);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasRotmEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtbmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, CuDevicePtr a, long lda,
            CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr ap, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr ap, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr ap, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtpmv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr ap,
            CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr a, long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr a, long lda,
            CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr ap,
            CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr ap, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr ap, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtpsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, CuDevicePtr ap,
            CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, CuDevicePtr a,
            long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, CuDevicePtr a,
            long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, CuDevicePtr a,
            long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtbsv_v2_64(CudaBlasHandle handle, FillMode uplo, Operation trans,
            DiagType diag, long n, long k, CuDevicePtr a,
            long lda, CuDevicePtr x, long incx);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            ref float beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            ref double beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            ref float beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            ref double beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            ref float beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            ref double beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            ref float beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            ref double beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref float alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            long incx,
            ref float beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref double alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            long incx,
            ref double beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref float alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            ref double alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgbmv_v2_64(CudaBlasHandle handle,
            Operation trans,
            long m,
            long n,
            long kl,
            long ku,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsymv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChemv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhemv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhbmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChpmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhpmv_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr ap,
            CuDevicePtr x,
            long incx,
            CuDevicePtr beta,
            CuDevicePtr y,
            long incy);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDger_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgeru_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgerc_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgeru_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgerc_v2_64(CudaBlasHandle handle,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZher_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhpr_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCher2_v2_64(CudaBlasHandle handle,
            FillMode uplo, long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZher2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr a,
            long lda);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDspr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChpr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhpr2_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr x,
            long incx,
            CuDevicePtr y,
            long incy,
            CuDevicePtr ap);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref float beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemm_v2_64(CudaBlasHandle handle,
            Operation transa,
            Operation transb,
            long m,
            long n,
            long k,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref double beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemm3m_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemm3m_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemm_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemm_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemmBatched_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemmBatched_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemmEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            ref float beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            ref double beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyrkEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyrk3mEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            ref float beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            ref double beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref float beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref double beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherkEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherk3mEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherkEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherk3mEx_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref float beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref double beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            ref float alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref float beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            ref double alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            ref double beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsm_v2_64(CudaBlasHandle handle,
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
            long ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsm_v2_64(CudaBlasHandle handle,
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
            long ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrmm_v2_64(CudaBlasHandle handle,
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
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrmm_v2_64(CudaBlasHandle handle,
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
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemm_v2_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemm_v2_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemm_v2_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemm_v2_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyrk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZherk_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
            long n,
            long k,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyr2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCher2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZher2k_v2_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsyrkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCherkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZherkx_64(CudaBlasHandle handle,
            FillMode uplo,
            Operation trans,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZsymm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasChemm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZhemm_v2_64(CudaBlasHandle handle,
            SideMode side,
            FillMode uplo,
            long m,
            long n,
            CuDevicePtr alpha,
            CuDevicePtr a,
            long lda,
            CuDevicePtr b,
            long ldb,
            CuDevicePtr beta,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsm_v2_64(CudaBlasHandle handle,
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
            long ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsm_v2_64(CudaBlasHandle handle,
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
            long ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrsm_v2_64(CudaBlasHandle handle,
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
            long ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrsm_v2_64(CudaBlasHandle handle,
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
            long ldb);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrmm_v2_64(CudaBlasHandle handle,
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
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrmm_v2_64(CudaBlasHandle handle,
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
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrmm_v2_64(CudaBlasHandle handle,
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
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrmm_v2_64(CudaBlasHandle handle, SideMode side,
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
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgeam_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgeam_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgeam_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgeam_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgeam_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgeam_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZdgmm_64(CudaBlasHandle handle,
            SideMode mode,
            long m,
            long n,
            CuDevicePtr a,
            long lda,
            CuDevicePtr x,
            long incx,
            CuDevicePtr c,
            long ldc);
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmBatched_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemmBatched_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemmBatched_64(CudaBlasHandle handle,
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
        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemmBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemm3mBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemm3mStridedBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmBatchedEx_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmStridedBatchedEx_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmStridedBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemmStridedBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCgemmStridedBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZgemmStridedBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemmStridedBatched_64(CudaBlasHandle handle,
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


        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmBatched_64(CudaBlasHandle handle,
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


        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemmBatched_64(CudaBlasHandle handle,
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


        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmBatchedEx_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasGemmStridedBatchedEx_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasSgemmStridedBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDgemmStridedBatched_64(CudaBlasHandle handle,
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


        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasHgemmStridedBatched_64(CudaBlasHandle handle,
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


        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsmBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsmBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasCtrsmBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasZtrsmBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasStrsmBatched_64(CudaBlasHandle handle,
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

        [LibraryImport(CublasApiDllName)]
        public static partial CuBlasStatus cublasDtrsmBatched_64(CudaBlasHandle handle,
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