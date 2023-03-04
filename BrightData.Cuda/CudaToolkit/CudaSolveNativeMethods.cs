using System;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit
{
    internal static class CudaSolveNativeMethods
    {
        internal const string CuSolveApiDllName = "cusolver64_11.dll";
        public static class Dense
        {
            static Dense()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCreate(ref CusolverDnHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDestroy(CusolverDnHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSetStream(CusolverDnHandle handle, CUstream streamId);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnGetStream(CusolverDnHandle handle, ref CUstream sreamId);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSpotrf_bufferSize(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDpotrf_bufferSize(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCpotrf_bufferSize(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZpotrf_bufferSize(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSpotrf(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, CUdeviceptr workspace, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDpotrf(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, CUdeviceptr workspace, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCpotrf(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, CUdeviceptr workspace, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZpotrf(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, CUdeviceptr workspace, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSpotrs(CusolverDnHandle handle, FillMode uplo, int n, int nrhs, CUdeviceptr a, int lda, CUdeviceptr b, int ldb, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDpotrs(CusolverDnHandle handle, FillMode uplo, int n, int nrhs, CUdeviceptr a, int lda, CUdeviceptr b, int ldb, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCpotrs(CusolverDnHandle handle, FillMode uplo, int n, int nrhs, CUdeviceptr a, int lda, CUdeviceptr b, int ldb, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZpotrs(CusolverDnHandle handle, FillMode uplo, int n, int nrhs, CUdeviceptr a, int lda, CUdeviceptr b, int ldb, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgetrf_bufferSize(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgetrf_bufferSize(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgetrf_bufferSize(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgetrf_bufferSize(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgetrf(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr workspace, CUdeviceptr devIpiv, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgetrf(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr workspace, CUdeviceptr devIpiv, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgetrf(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr workspace, CUdeviceptr devIpiv, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgetrf(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr workspace, CUdeviceptr devIpiv, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSlaswp(CusolverDnHandle handle, int n, CUdeviceptr a, int lda, int k1, int k2, CUdeviceptr devIpiv, int incx);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDlaswp(CusolverDnHandle handle, int n, CUdeviceptr a, int lda, int k1, int k2, CUdeviceptr devIpiv, int incx);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnClaswp(CusolverDnHandle handle, int n, CUdeviceptr a, int lda, int k1, int k2, CUdeviceptr devIpiv, int incx);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZlaswp(CusolverDnHandle handle, int n, CUdeviceptr a, int lda, int k1, int k2, CUdeviceptr devIpiv, int incx);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgetrs(CusolverDnHandle handle, Operation trans, int n, int nrhs, CUdeviceptr a, int lda, CUdeviceptr devIpiv, CUdeviceptr b, int ldb, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgetrs(CusolverDnHandle handle, Operation trans, int n, int nrhs, CUdeviceptr a, int lda, CUdeviceptr devIpiv, CUdeviceptr b, int ldb, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgetrs(CusolverDnHandle handle, Operation trans, int n, int nrhs, CUdeviceptr a, int lda, CUdeviceptr devIpiv, CUdeviceptr b, int ldb, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgetrs(CusolverDnHandle handle, Operation trans, int n, int nrhs, CUdeviceptr a, int lda, CUdeviceptr devIpiv, CUdeviceptr b, int ldb, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgeqrf(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr workspace, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgeqrf(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr workspace, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgeqrf(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr workspace, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgeqrf(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr workspace, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSormqr(CusolverDnHandle handle, SideMode side, Operation trans, int m, int n, int k, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr c, int ldc, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDormqr(CusolverDnHandle handle, SideMode side, Operation trans, int m, int n, int k, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr c, int ldc, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCunmqr(CusolverDnHandle handle, SideMode side, Operation trans, int m, int n, int k, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr c, int ldc, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZunmqr(CusolverDnHandle handle, SideMode side, Operation trans, int m, int n, int k, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr c, int ldc, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgeqrf_bufferSize(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgeqrf_bufferSize(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgeqrf_bufferSize(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgeqrf_bufferSize(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSormqr_bufferSize(CusolverDnHandle handle, SideMode side, Operation trans,
                int m, int n, int k, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr c, int ldc, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDormqr_bufferSize(CusolverDnHandle handle, SideMode side, Operation trans,
                int m, int n, int k, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr c, int ldc, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCunmqr_bufferSize(CusolverDnHandle handle, SideMode side, Operation trans,
                int m, int n, int k, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr c, int ldc, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZunmqr_bufferSize(CusolverDnHandle handle, SideMode side, Operation trans,
                int m, int n, int k, CUdeviceptr a, int lda, CUdeviceptr tau, CUdeviceptr c, int ldc, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSorgqr_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDorgqr_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCungqr_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZungqr_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSorgqr(
                CusolverDnHandle handle,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDorgqr(
                CusolverDnHandle handle,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCungqr(
                CusolverDnHandle handle,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZungqr(
                CusolverDnHandle handle,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgebrd(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr d, CUdeviceptr e, CUdeviceptr tauq, CUdeviceptr taup, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgebrd(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr d, CUdeviceptr e, CUdeviceptr tauq, CUdeviceptr taup, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgebrd(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr d, CUdeviceptr e, CUdeviceptr tauq, CUdeviceptr taup, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgebrd(CusolverDnHandle handle, int m, int n, CUdeviceptr a, int lda, CUdeviceptr d, CUdeviceptr e, CUdeviceptr tauq, CUdeviceptr taup, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsytrd_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr d,
                CUdeviceptr e,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsytrd_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr d,
                CUdeviceptr e,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsytrd(CusolverDnHandle handle, char uplo, int n, CUdeviceptr a, int lda, CUdeviceptr d, CUdeviceptr e, CUdeviceptr tau, CUdeviceptr work, int lwork, CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsytrd(CusolverDnHandle handle, char uplo, int n, CUdeviceptr a, int lda, CUdeviceptr d, CUdeviceptr e, CUdeviceptr tau, CUdeviceptr work, int lwork, CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnChetrd_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr d,
                CUdeviceptr e,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZhetrd_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr d,
                CUdeviceptr e,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnChetrd(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr d,
                CUdeviceptr e,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZhetrd(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr d,
                CUdeviceptr e,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgebrd_bufferSize(CusolverDnHandle handle, int m, int n, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgebrd_bufferSize(CusolverDnHandle handle, int m, int n, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgebrd_bufferSize(CusolverDnHandle handle, int m, int n, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgebrd_bufferSize(CusolverDnHandle handle, int m, int n, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgesvd_bufferSize(CusolverDnHandle handle, int m, int n, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgesvd_bufferSize(CusolverDnHandle handle, int m, int n, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgesvd_bufferSize(CusolverDnHandle handle, int m, int n, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgesvd_bufferSize(CusolverDnHandle handle, int m, int n, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgesvd(CusolverDnHandle handle, [MarshalAs(UnmanagedType.I1)] char jobu, [MarshalAs(UnmanagedType.I1)] char jobvt, int m, int n, CUdeviceptr a, int lda, CUdeviceptr s, CUdeviceptr u, int ldu, CUdeviceptr vt, int ldvt, CUdeviceptr work, int lwork, CUdeviceptr rwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgesvd(CusolverDnHandle handle, [MarshalAs(UnmanagedType.I1)] char jobu, [MarshalAs(UnmanagedType.I1)] char jobvt, int m, int n, CUdeviceptr a, int lda, CUdeviceptr s, CUdeviceptr u, int ldu, CUdeviceptr vt, int ldvt, CUdeviceptr work, int lwork, CUdeviceptr rwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgesvd(CusolverDnHandle handle, [MarshalAs(UnmanagedType.I1)] char jobu, [MarshalAs(UnmanagedType.I1)] char jobvt, int m, int n, CUdeviceptr a, int lda, CUdeviceptr s, CUdeviceptr u, int ldu, CUdeviceptr vt, int ldvt, CUdeviceptr work, int lwork, CUdeviceptr rwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgesvd(CusolverDnHandle handle, [MarshalAs(UnmanagedType.I1)] char jobu, [MarshalAs(UnmanagedType.I1)] char jobvt, int m, int n, CUdeviceptr a, int lda, CUdeviceptr s, CUdeviceptr u, int ldu, CUdeviceptr vt, int ldvt, CUdeviceptr work, int lwork, CUdeviceptr rwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsytrf(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, CUdeviceptr ipiv, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsytrf(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, CUdeviceptr ipiv, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCsytrf(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, CUdeviceptr ipiv, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZsytrf(CusolverDnHandle handle, FillMode uplo, int n, CUdeviceptr a, int lda, CUdeviceptr ipiv, CUdeviceptr work, int lwork, CUdeviceptr devInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsytrf_bufferSize(CusolverDnHandle handle, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsytrf_bufferSize(CusolverDnHandle handle, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCsytrf_bufferSize(CusolverDnHandle handle, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZsytrf_bufferSize(CusolverDnHandle handle, int n, CUdeviceptr a, int lda, ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSorgbr_bufferSize(
                CusolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDorgbr_bufferSize(
                CusolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCungbr_bufferSize(
                CusolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZungbr_bufferSize(
                CusolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSorgbr(
                CusolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDorgbr(
                CusolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCungbr(
                CusolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZungbr(
                CusolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSorgtr_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDorgtr_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCungtr_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZungtr_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSorgtr(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDorgtr(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCungtr(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZungtr(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSormtr_bufferSize(
                CusolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr c,
                int ldc,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDormtr_bufferSize(
                CusolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr c,
                int ldc,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCunmtr_bufferSize(
                CusolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr c,
                int ldc,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZunmtr_bufferSize(
                CusolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr c,
                int ldc,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSormtr(
                CusolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr c,
                int ldc,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDormtr(
                CusolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr c,
                int ldc,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCunmtr(
                CusolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr c,
                int ldc,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZunmtr(
                CusolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr tau,
                CUdeviceptr c,
                int ldc,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsyevd_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsyevd_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCheevd_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZheevd_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsyevd(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsyevd(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCheevd(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZheevd(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsygvd_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsygvd_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnChegvd_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZhegvd_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                ref int lwork);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsygvd(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsygvd(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnChegvd(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZhegvd(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsCreate(ref CusolverDnIrsParams paramsPtr);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsDestroy(CusolverDnIrsParams parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsSetRefinementSolver(
                CusolverDnIrsParams parameters, CusolverIrsRefinement refinementSolver);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsSetSolverMainPrecision(
                CusolverDnIrsParams parameters, CusolverPrecType solverMainPrecision);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsSetSolverLowestPrecision(
                CusolverDnIrsParams parameters, CusolverPrecType solverLowestPrecision);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsSetSolverPrecisions(
                CusolverDnIrsParams parameters, CusolverPrecType solverMainPrecision, CusolverPrecType solverLowestPrecision);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsSetTol(CusolverDnIrsParams parameters, double val);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsSetTolInner(CusolverDnIrsParams parameters, double val);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsSetMaxIters(CusolverDnIrsParams parameters, int maxiters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsSetMaxItersInner(CusolverDnIrsParams parameters, int maxitersInner);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsGetMaxIters(CusolverDnIrsParams parameters, ref int maxiters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsEnableFallback(CusolverDnIrsParams parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSParamsDisableFallback(CusolverDnIrsParams parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSInfosDestroy(
                CusolverDnIrsInfos infos);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSInfosCreate(
                ref CusolverDnIrsInfos infosPtr);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSInfosGetNiters(
                CusolverDnIrsInfos infos,
                ref int niters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSInfosGetOuterNiters(
                CusolverDnIrsInfos infos,
                ref int outerNiters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSInfosGetMaxIters(
                CusolverDnIrsInfos infos,
                ref int maxiters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSInfosRequestResidual(
                CusolverDnIrsInfos infos);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSInfosGetResidualHistory(
                CusolverDnIrsInfos infos,
                ref IntPtr residualHistory);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZZgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZCgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZKgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZEgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZYgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCCgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCEgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCKgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCYgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDDgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDSgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDHgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDBgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDXgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSSgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSHgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSBgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSXgesv(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZZgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZCgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZKgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZEgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZYgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCCgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCKgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCEgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCYgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDDgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDSgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDHgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDBgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDXgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSSgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSHgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSBgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSXgesv_bufferSize(
                CusolverDnHandle handle,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dipiv,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZZgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZCgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZKgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZEgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZYgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCCgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCKgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCEgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCYgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDDgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDSgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDHgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDBgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDXgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSSgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSHgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSBgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSXgels(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CUdeviceptr dInfo);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZZgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZCgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZKgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZEgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZYgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCCgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCKgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCEgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCYgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDDgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDSgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDHgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDBgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDXgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSSgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSHgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSBgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSXgels_bufferSize(
                CusolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, ref SizeT lworkBytes);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSXgesv(
                CusolverDnHandle handle,
                CusolverDnIrsParams gesvIrsParameters,
                CusolverDnIrsInfos gesvIrsInfos,
                int n, int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int niters,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSXgesv_bufferSize(
                CusolverDnHandle handle,
                CusolverDnIrsParams parameters,
                int n, int nrhs,
                ref SizeT lworkBytes);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSXgels(
                CusolverDnHandle handle,
                CusolverDnIrsParams gelsIrsParams,
                CusolverDnIrsInfos gelsIrsInfos,
                int m,
                int n,
                int nrhs,
                CUdeviceptr dA, int ldda,
                CUdeviceptr dB, int lddb,
                CUdeviceptr dX, int lddx,
                CUdeviceptr dWorkspace, SizeT lworkBytes,
                ref int niters,
                CUdeviceptr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnIRSXgels_bufferSize(
                CusolverDnHandle handle,
                CusolverDnIrsParams parameters,
                int m,
                int n,
                int nrhs,
                ref SizeT lworkBytes);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSpotrfBatched(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr aarray,
                int lda,
                CUdeviceptr infoArray,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDpotrfBatched(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr aarray,
                int lda,
                CUdeviceptr infoArray,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCpotrfBatched(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr aarray,
                int lda,
                CUdeviceptr infoArray,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZpotrfBatched(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr aarray,
                int lda,
                CUdeviceptr infoArray,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSpotrsBatched(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                int nrhs,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr dInfo,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDpotrsBatched(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                int nrhs,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr dInfo,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCpotrsBatched(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                int nrhs,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr dInfo,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZpotrsBatched(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                int nrhs,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr dInfo,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSpotri_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDpotri_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCpotri_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZpotri_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSpotri(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDpotri(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCpotri(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZpotri(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXtrtri_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                DiagType diag,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXtrtri(
                CusolverDnHandle handle,
                FillMode uplo,
                DiagType diag,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr devInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSlauum_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDlauum_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnClauum_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZlauum_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSlauum(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDlauum(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnClauum(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZlauum(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr devInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsytrs_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CUdeviceptr ipiv,
                CudaDataType dataTypeB,
                CUdeviceptr b,
                long ldb,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsytrs(
                CusolverDnHandle handle,
                FillMode uplo,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CUdeviceptr ipiv,
                CudaDataType dataTypeB,
                CUdeviceptr b,
                long ldb,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsytri_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr ipiv,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsytri_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr ipiv,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCsytri_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr ipiv,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZsytri_bufferSize(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr ipiv,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsytri(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr ipiv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsytri(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr ipiv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCsytri(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr ipiv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZsytri(
                CusolverDnHandle handle,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr ipiv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsyevdx_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsyevdx_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCheevdx_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZheevdx_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsyevdx(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsyevdx(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCheevdx(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZheevdx(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsygvdx_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsygvdx_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnChegvdx_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZhegvdx_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                ref int lwork);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsygvdx(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsygvdx(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnChegvdx(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZhegvdx(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCreateSyevjInfo(
                ref SyevjInfo info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDestroySyevjInfo(
                SyevjInfo info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsyevjSetTolerance(
                SyevjInfo info,
                double tolerance);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsyevjSetMaxSweeps(
                SyevjInfo info,
                int maxSweeps);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsyevjSetSortEig(
                SyevjInfo info,
                int sortEig);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsyevjGetResidual(
                CusolverDnHandle handle,
                SyevjInfo info,
                ref double residual);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsyevjGetSweeps(
                CusolverDnHandle handle,
                SyevjInfo info,
                ref int executedSweeps);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsyevjBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsyevjBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCheevjBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZheevjBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsyevjBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsyevjBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCheevjBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZheevjBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsyevj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsyevj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCheevj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZheevj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsyevj(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsyevj(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCheevj(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZheevj(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsygvj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsygvj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnChegvj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZhegvj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                ref int lwork,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSsygvj(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDsygvj(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnChegvj(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZhegvj(
                CusolverDnHandle handle,
                CusolverEigType itype,
                CusolverEigMode jobz,
                FillMode uplo,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr b,
                int ldb,
                CUdeviceptr w,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCreateGesvdjInfo(
                ref GesvdjInfo info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDestroyGesvdjInfo(
                GesvdjInfo info);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvdjSetTolerance(
                GesvdjInfo info,
                double tolerance);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvdjSetMaxSweeps(
                GesvdjInfo info,
                int maxSweeps);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvdjSetSortEig(
                GesvdjInfo info,
                int sortSvd);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvdjGetResidual(
                CusolverDnHandle handle,
                GesvdjInfo info,
                ref double residual);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvdjGetSweeps(
                CusolverDnHandle handle,
                GesvdjInfo info,
                ref int executedSweeps);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgesvdjBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgesvdjBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgesvdjBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgesvdjBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgesvdjBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgesvdjBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgesvdjBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgesvdjBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgesvdj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int econ,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgesvdj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int econ,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgesvdj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int econ,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgesvdj_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int econ,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgesvdj(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int econ,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgesvdj(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int econ,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgesvdj(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int econ,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgesvdj(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int econ,
                int m,
                int n,
                CUdeviceptr a,
                int lda,
                CUdeviceptr s,
                CUdeviceptr u,
                int ldu,
                CUdeviceptr v,
                int ldv,
                CUdeviceptr work,
                int lwork,
                CUdeviceptr info,
                GesvdjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgesvdaStridedBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int rank,
                int m,
                int n,
                CUdeviceptr dA,
                int lda,
                long strideA,
                CUdeviceptr dS,
                long strideS,
                CUdeviceptr dU,
                int ldu,
                long strideU,
                CUdeviceptr dV,
                int ldv,
                long strideV,
                ref int lwork,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgesvdaStridedBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int rank,
                int m,
                int n,
                CUdeviceptr dA,
                int lda,
                long strideA,
                CUdeviceptr dS,
                long strideS,
                CUdeviceptr dU,
                int ldu,
                long strideU,
                CUdeviceptr dV,
                int ldv,
                long strideV,
                ref int lwork,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgesvdaStridedBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int rank,
                int m,
                int n,
                CUdeviceptr dA,
                int lda,
                long strideA,
                CUdeviceptr dS,
                long strideS,
                CUdeviceptr dU,
                int ldu,
                long strideU,
                CUdeviceptr dV,
                int ldv,
                long strideV,
                ref int lwork,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgesvdaStridedBatched_bufferSize(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int rank,
                int m,
                int n,
                CUdeviceptr dA,
                int lda,
                long strideA,
                CUdeviceptr dS,
                long strideS,
                CUdeviceptr dU,
                int ldu,
                long strideU,
                CUdeviceptr dV,
                int ldv,
                long strideV,
                ref int lwork,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSgesvdaStridedBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int rank,
                int m,
                int n,
                CUdeviceptr dA,
                int lda,
                long strideA,
                CUdeviceptr dS,
                long strideS,
                CUdeviceptr dU,
                int ldu,
                long strideU,
                CUdeviceptr dV,
                int ldv,
                long strideV,
                CUdeviceptr dWork,
                int lwork,
                CUdeviceptr dInfo,
                double[] hRNrmF,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDgesvdaStridedBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int rank,
                int m,
                int n,
                CUdeviceptr dA,
                int lda,
                long strideA,
                CUdeviceptr dS,
                long strideS,
                CUdeviceptr dU,
                int ldu,
                long strideU,
                CUdeviceptr dV,
                int ldv,
                long strideV,
                CUdeviceptr dWork,
                int lwork,
                CUdeviceptr dInfo,
                double[] hRNrmF,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCgesvdaStridedBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int rank,
                int m,
                int n,
                CUdeviceptr dA,
                int lda,
                long strideA,
                CUdeviceptr dS,
                long strideS,
                CUdeviceptr dU,
                int ldu,
                long strideU,
                CUdeviceptr dV,
                int ldv,
                long strideV,
                CUdeviceptr dWork,
                int lwork,
                CUdeviceptr dInfo,
                double[] hRNrmF,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnZgesvdaStridedBatched(
                CusolverDnHandle handle,
                CusolverEigMode jobz,
                int rank,
                int m,
                int n,
                CUdeviceptr dA,
                int lda,
                long strideA,
                CUdeviceptr dS,
                long strideS,
                CUdeviceptr dU,
                int ldu,
                long strideU,
                CUdeviceptr dV,
                int ldv,
                long strideV,
                CUdeviceptr dWork,
                int lwork,
                CUdeviceptr dInfo,
                double[] hRNrmF,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnCreateParams(
                ref CusolverDnParams parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnDestroyParams(
                CusolverDnParams parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnSetAdvOptions(
                CusolverDnParams parameters,
                CusolverDnFunction function,
                CusolverAlgMode algo);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnPotrf_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);

            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnPotrf(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType computeType,
                CUdeviceptr pBuffer,
                SizeT workspaceInBytes,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnPotrs(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                FillMode uplo,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeB,
                CUdeviceptr b,
                long ldb,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnGeqrf_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeTau,
                CUdeviceptr tau,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);

            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnGeqrf(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeTau,
                CUdeviceptr tau,
                CudaDataType computeType,
                CUdeviceptr pBuffer,
                SizeT workspaceInBytes,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnGetrf_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);

            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnGetrf(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CUdeviceptr ipiv,
                CudaDataType computeType,
                CUdeviceptr pBuffer,
                SizeT workspaceInBytes,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnGetrs(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                Operation trans,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CUdeviceptr ipiv,
                CudaDataType dataTypeB,
                CUdeviceptr b,
                long ldb,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnSyevd_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeW,
                CUdeviceptr w,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);

            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnSyevd(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeW,
                CUdeviceptr w,
                CudaDataType computeType,
                CUdeviceptr pBuffer,
                SizeT workspaceInBytes,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnSyevdx_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                IntPtr vl,
                IntPtr vu,
                long il,
                long iu,
                ref long hMeig,
                CudaDataType dataTypeW,
                CUdeviceptr w,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CusolverStatus cusolverDnSyevdx(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                IntPtr vl,
                IntPtr vu,
                long il,
                long iu,
                ref long meig64,
                CudaDataType dataTypeW,
                CUdeviceptr w,
                CudaDataType computeType,
                CUdeviceptr pBuffer,
                SizeT workspaceInBytes,
                CUdeviceptr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXpotrf_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXpotrf(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType computeType,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXpotrs(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                FillMode uplo,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeB,
                CUdeviceptr b,
                long ldb,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgeqrf_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeTau,
                CUdeviceptr tau,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgeqrf(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeTau,
                CUdeviceptr tau,
                CudaDataType computeType,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgetrf_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgetrf(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CUdeviceptr ipiv,
                CudaDataType computeType,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgetrs(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                Operation trans,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CUdeviceptr ipiv,
                CudaDataType dataTypeB,
                CUdeviceptr b,
                long ldb,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsyevd_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeW,
                CUdeviceptr w,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsyevd(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeW,
                CUdeviceptr w,
                CudaDataType computeType,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsyevdx_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                IntPtr vl,
                IntPtr vu,
                long il,
                long iu,
                ref long hMeig,
                CudaDataType dataTypeW,
                CUdeviceptr w,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXsyevdx(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                CusolverEigRange range,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                IntPtr vl,
                IntPtr vu,
                long il,
                long iu,
                ref long meig64,
                CudaDataType dataTypeW,
                CUdeviceptr w,
                CudaDataType computeType,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvd_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                sbyte jobu,
                sbyte jobvt,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeS,
                CUdeviceptr s,
                CudaDataType dataTypeU,
                CUdeviceptr u,
                long ldu,
                CudaDataType dataTypeVt,
                CUdeviceptr vt,
                long ldvt,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvd(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                sbyte jobu,
                sbyte jobvt,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeS,
                CUdeviceptr s,
                CudaDataType dataTypeU,
                CUdeviceptr u,
                long ldu,
                CudaDataType dataTypeVt,
                CUdeviceptr vt,
                long ldvt,
                CudaDataType computeType,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr info);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvdp_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                int econ,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeS,
                CUdeviceptr s,
                CudaDataType dataTypeU,
                CUdeviceptr u,
                long ldu,
                CudaDataType dataTypeV,
                CUdeviceptr v,
                long ldv,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvdp(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                CusolverEigMode jobz,
                int econ,
                long m,
                long n,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeS,
                CUdeviceptr s,
                CudaDataType dataTypeU,
                CUdeviceptr u,
                long ldu,
                CudaDataType dataTypeV,
                CUdeviceptr v,
                long ldv,
                CudaDataType computeType,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr dInfo,
                ref double hErrSigma);


            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvdr_bufferSize(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                sbyte jobu,
                sbyte jobv,
                long m,
                long n,
                long k,
                long p,
                long niters,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeSrand,
                CUdeviceptr srand,
                CudaDataType dataTypeUrand,
                CUdeviceptr urand,
                long ldUrand,
                CudaDataType dataTypeVrand,
                CUdeviceptr vrand,
                long ldVrand,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverDnXgesvdr(
                CusolverDnHandle handle,
                CusolverDnParams parameters,
                sbyte jobu,
                sbyte jobv,
                long m,
                long n,
                long k,
                long p,
                long niters,
                CudaDataType dataTypeA,
                CUdeviceptr a,
                long lda,
                CudaDataType dataTypeSrand,
                CUdeviceptr srand,
                CudaDataType dataTypeUrand,
                CUdeviceptr urand,
                long ldUrand,
                CudaDataType dataTypeVrand,
                CUdeviceptr vrand,
                long ldVrand,
                CudaDataType computeType,
                CUdeviceptr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CUdeviceptr dInfo
            );
        }
        public static class Sparse
        {
            static Sparse()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpCreate(ref CusolverSpHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDestroy(CusolverSpHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpSetStream(CusolverSpHandle handle, CUstream streamId);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpGetStream(CusolverSpHandle handle, ref CUstream streamId);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpXcsrissymHost(CusolverSpHandle handle, int m, int nnzA, CusparseMatDescr descrA, int[] csrRowPtrA, int[] csrEndPtrA, int[] csrColIndA, ref int issym);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpScsrlsvluHost(CusolverSpHandle handle, int n, int nnzA, CusparseMatDescr descrA, float[] csrValA, int[] csrRowPtrA, int[] csrColIndA, float[] b, float tol, int reorder, float[] x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDcsrlsvluHost(CusolverSpHandle handle, int n, int nnzA, CusparseMatDescr descrA, double[] csrValA, int[] csrRowPtrA, int[] csrColIndA, double[] b, double tol, int reorder, double[] x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpScsrlsvqr(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrValA, CUdeviceptr csrRowPtrA, CUdeviceptr csrColIndA, CUdeviceptr b, float tol, int reorder, CUdeviceptr x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDcsrlsvqr(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrValA, CUdeviceptr csrRowPtrA, CUdeviceptr csrColIndA, CUdeviceptr b, double tol, int reorder, CUdeviceptr x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpCcsrlsvqr(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrValA, CUdeviceptr csrRowPtrA, CUdeviceptr csrColIndA, CUdeviceptr b, float tol, int reorder, CUdeviceptr x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpZcsrlsvqr(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrValA, CUdeviceptr csrRowPtrA, CUdeviceptr csrColIndA, CUdeviceptr b, double tol, int reorder, CUdeviceptr x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpScsrlsvqrHost(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, float[] csrValA, int[] csrRowPtrA, int[] csrColIndA, float[] b, float tol, int reorder, float[] x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDcsrlsvqrHost(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, double[] csrValA, int[] csrRowPtrA, int[] csrColIndA, double[] b, double tol, int reorder, double[] x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpScsrlsvcholHost(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, float[] csrVal, int[] csrRowPtr, int[] csrColInd, float[] b, float tol, int reorder, float[] x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDcsrlsvcholHost(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, double[] csrVal, int[] csrRowPtr, int[] csrColInd, double[] b, double tol, int reorder, double[] x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpScsrlsvchol(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrVal, CUdeviceptr csrRowPtr, CUdeviceptr csrColInd, CUdeviceptr b, float tol, int reorder, CUdeviceptr x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDcsrlsvchol(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrVal, CUdeviceptr csrRowPtr, CUdeviceptr csrColInd, CUdeviceptr b, double tol, int reorder, CUdeviceptr x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpCcsrlsvchol(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrVal, CUdeviceptr csrRowPtr, CUdeviceptr csrColInd, CUdeviceptr b, float tol, int reorder, CUdeviceptr x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpZcsrlsvchol(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrVal, CUdeviceptr csrRowPtr, CUdeviceptr csrColInd, CUdeviceptr b, double tol, int reorder, CUdeviceptr x, ref int singularity);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpScsrlsqvqrHost(CusolverSpHandle handle, int m, int n, int nnz, CusparseMatDescr descrA, float[] csrValA, int[] csrRowPtrA, int[] csrColIndA, float[] b, float tol, ref int rankA, float[] x, int[] p, ref float minNorm);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDcsrlsqvqrHost(CusolverSpHandle handle, int m, int n, int nnz, CusparseMatDescr descrA, double[] csrValA, int[] csrRowPtrA, int[] csrColIndA, double[] b, double tol, ref int rankA, double[] x, int[] p, ref double minNorm);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpScsreigvsiHost(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, float[] csrValA, int[] csrRowPtrA, int[] csrColIndA, float mu0, float[] x0, int maxite, float tol, ref float mu, float[] x);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDcsreigvsiHost(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, double[] csrValA, int[] csrRowPtrA, int[] csrColIndA, double mu0, double[] x0, int maxite, double tol, ref double mu, double[] x);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpScsreigvsi(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrValA, CUdeviceptr csrRowPtrA, CUdeviceptr csrColIndA, float mu0, CUdeviceptr x0, int maxite, float tol, ref float mu, CUdeviceptr x);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDcsreigvsi(CusolverSpHandle handle, int m, int nnz, CusparseMatDescr descrA, CUdeviceptr csrValA, CUdeviceptr csrRowPtrA, CUdeviceptr csrColIndA, double mu0, CUdeviceptr x0, int maxite, double tol, ref double mu, CUdeviceptr x);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpXcsrsymrcmHost(CusolverSpHandle handle, int n, int nnzA, CusparseMatDescr descrA, int[] csrRowPtrA, int[] csrColIndA, int[] p);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpXcsrsymmdqHost(
                CusolverSpHandle handle,
                int n,
                int nnzA,
                CusparseMatDescr descrA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                int[] p);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpXcsrsymamdHost(
                CusolverSpHandle handle,
                int n,
                int nnzA,
                CusparseMatDescr descrA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                int[] p);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpXcsrmetisndHost(
                CusolverSpHandle handle,
                int n,
                int nnzA,
                CusparseMatDescr descrA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                long[] options,
                int[] p);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpScsrzfdHost(
                CusolverSpHandle handle,
                int n,
                int nnz,
                CusparseMatDescr descrA,
                float[] csrValA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                int[] p,
                ref int numnz);

            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpDcsrzfdHost(
                CusolverSpHandle handle,
                int n,
                int nnz,
                CusparseMatDescr descrA,
                double[] csrValA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                int[] p,
                ref int numnz);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpXcsrperm_bufferSizeHost(CusolverSpHandle handle, int m, int n, int nnzA, CusparseMatDescr descrA, int[] csrRowPtrA, int[] csrColIndA, int[] p, int[] q, ref SizeT bufferSizeInBytes);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverSpXcsrpermHost(CusolverSpHandle handle, int m, int n, int nnzA, CusparseMatDescr descrA, int[] csrRowPtrA, int[] csrColIndA, int[] p, int[] q, int[] map, byte[] pBuffer);
        }
        public static class Refactorization
        {
            static Refactorization()
            {
                DriverApiNativeMethods.Init();
            }
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfCreate(ref CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfDestroy(CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfGetMatrixFormat(CusolverRfHandle handle, ref MatrixFormat format, ref UnitDiagonal diag);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfSetMatrixFormat(CusolverRfHandle handle, MatrixFormat format, UnitDiagonal diag);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfSetNumericProperties(CusolverRfHandle handle, double zero, double boost);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfGetNumericProperties(CusolverRfHandle handle, ref double zero, ref double boost);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfGetNumericBoostReport(CusolverRfHandle handle, ref NumericBoostReport report);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfSetAlgs(CusolverRfHandle handle, Factorization factAlg, TriangularSolve solveAlg);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfGetAlgs(CusolverRfHandle handle, ref Factorization factAlg, ref TriangularSolve solveAlg);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfGetResetValuesFastMode(CusolverRfHandle handle, ref ResetValuesFastMode fastMode);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfSetResetValuesFastMode(CusolverRfHandle handle, ResetValuesFastMode fastMode);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfSetupHost(int n, int nnzA, int[] hCsrRowPtrA, int[] hCsrColIndA, double[] hCsrValA, int nnzL, int[] hCsrRowPtrL, int[] hCsrColIndL, double[] hCsrValL, int nnzU, int[] hCsrRowPtrU, int[] hCsrColIndU, double[] hCsrValU, int[] hP, int[] hQ, CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfSetupDevice(int n, int nnzA, CUdeviceptr csrRowPtrA, CUdeviceptr csrColIndA, CUdeviceptr csrValA, int nnzL,
                CUdeviceptr csrRowPtrL, CUdeviceptr csrColIndL, CUdeviceptr csrValL, int nnzU, CUdeviceptr csrRowPtrU, CUdeviceptr csrColIndU, CUdeviceptr csrValU,
                CUdeviceptr p, CUdeviceptr q, CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfResetValues(int n, int nnzA, CUdeviceptr csrRowPtrA, CUdeviceptr csrColIndA, CUdeviceptr csrValA,
                CUdeviceptr p, CUdeviceptr q, CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfAnalyze(CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfRefactor(CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfAccessBundledFactorsDevice(CusolverRfHandle handle, ref int nnzM, ref CUdeviceptr mp, ref CUdeviceptr mi, ref CUdeviceptr mx);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfExtractBundledFactorsHost(CusolverRfHandle handle, ref int hNnzM, ref IntPtr hMp, ref IntPtr hMi, ref IntPtr hMx);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfExtractSplitFactorsHost(CusolverRfHandle handle, ref int hNnzL, ref IntPtr hCsrRowPtrL, ref IntPtr hCsrColIndL, ref IntPtr hCsrValL, ref int hNnzU, ref IntPtr hCsrRowPtrU, ref IntPtr hCsrColIndU, ref IntPtr hCsrValU);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfSolve(CusolverRfHandle handle, CUdeviceptr p, CUdeviceptr q, int nrhs, double[] temp, int ldt, double[] xf, int ldxf);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfBatchSetupHost(int batchSize, int n, int nnzA, int[] hCsrRowPtrA, int[] hCsrColIndA, IntPtr[] hCsrValAArray, int nnzL,
                int[] hCsrRowPtrL, int[] hCsrColIndL, double[] hCsrValL, int nnzU, int[] hCsrRowPtrU, int[] hCsrColIndU, double[] hCsrValU, int[] hP, int[] hQ, CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfBatchResetValues(int batchSize, int n, int nnzA, CUdeviceptr csrRowPtrA, CUdeviceptr csrColIndA, CUdeviceptr[] csrValAArray,
                CUdeviceptr p, CUdeviceptr q, CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfBatchAnalyze(CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfBatchRefactor(CusolverRfHandle handle);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfBatchSolve(CusolverRfHandle handle, CUdeviceptr p, CUdeviceptr q, int nrhs, double[] temp,
                int ldt, IntPtr[] xfArray, int ldxf);
            [DllImport(CuSolveApiDllName)]
            public static extern CusolverStatus cusolverRfBatchZeroPivot(CusolverRfHandle handle, int[] position);
        }
    }
    
}