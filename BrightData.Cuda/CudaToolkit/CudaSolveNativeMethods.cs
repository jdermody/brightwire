using System;
using System.Runtime.InteropServices;
using BrightData.Cuda.CudaToolkit.Types;

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
            public static extern CuSolverStatus cusolverDnCreate(ref CuSolverDnHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDestroy(CuSolverDnHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSetStream(CuSolverDnHandle handle, CuStream streamId);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnGetStream(CuSolverDnHandle handle, ref CuStream sreamId);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSpotrf_bufferSize(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDpotrf_bufferSize(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCpotrf_bufferSize(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZpotrf_bufferSize(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSpotrf(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, CuDevicePtr workspace, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDpotrf(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, CuDevicePtr workspace, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCpotrf(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, CuDevicePtr workspace, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZpotrf(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, CuDevicePtr workspace, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSpotrs(CuSolverDnHandle handle, FillMode uplo, int n, int nrhs, CuDevicePtr a, int lda, CuDevicePtr b, int ldb, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDpotrs(CuSolverDnHandle handle, FillMode uplo, int n, int nrhs, CuDevicePtr a, int lda, CuDevicePtr b, int ldb, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCpotrs(CuSolverDnHandle handle, FillMode uplo, int n, int nrhs, CuDevicePtr a, int lda, CuDevicePtr b, int ldb, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZpotrs(CuSolverDnHandle handle, FillMode uplo, int n, int nrhs, CuDevicePtr a, int lda, CuDevicePtr b, int ldb, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgetrf_bufferSize(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgetrf_bufferSize(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgetrf_bufferSize(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgetrf_bufferSize(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgetrf(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr workspace, CuDevicePtr devIpiv, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgetrf(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr workspace, CuDevicePtr devIpiv, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgetrf(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr workspace, CuDevicePtr devIpiv, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgetrf(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr workspace, CuDevicePtr devIpiv, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSlaswp(CuSolverDnHandle handle, int n, CuDevicePtr a, int lda, int k1, int k2, CuDevicePtr devIpiv, int incx);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDlaswp(CuSolverDnHandle handle, int n, CuDevicePtr a, int lda, int k1, int k2, CuDevicePtr devIpiv, int incx);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnClaswp(CuSolverDnHandle handle, int n, CuDevicePtr a, int lda, int k1, int k2, CuDevicePtr devIpiv, int incx);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZlaswp(CuSolverDnHandle handle, int n, CuDevicePtr a, int lda, int k1, int k2, CuDevicePtr devIpiv, int incx);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgetrs(CuSolverDnHandle handle, Operation trans, int n, int nrhs, CuDevicePtr a, int lda, CuDevicePtr devIpiv, CuDevicePtr b, int ldb, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgetrs(CuSolverDnHandle handle, Operation trans, int n, int nrhs, CuDevicePtr a, int lda, CuDevicePtr devIpiv, CuDevicePtr b, int ldb, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgetrs(CuSolverDnHandle handle, Operation trans, int n, int nrhs, CuDevicePtr a, int lda, CuDevicePtr devIpiv, CuDevicePtr b, int ldb, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgetrs(CuSolverDnHandle handle, Operation trans, int n, int nrhs, CuDevicePtr a, int lda, CuDevicePtr devIpiv, CuDevicePtr b, int ldb, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgeqrf(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr workspace, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgeqrf(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr workspace, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgeqrf(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr workspace, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgeqrf(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr workspace, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSormqr(CuSolverDnHandle handle, SideMode side, Operation trans, int m, int n, int k, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr c, int ldc, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDormqr(CuSolverDnHandle handle, SideMode side, Operation trans, int m, int n, int k, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr c, int ldc, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCunmqr(CuSolverDnHandle handle, SideMode side, Operation trans, int m, int n, int k, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr c, int ldc, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZunmqr(CuSolverDnHandle handle, SideMode side, Operation trans, int m, int n, int k, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr c, int ldc, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgeqrf_bufferSize(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgeqrf_bufferSize(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgeqrf_bufferSize(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgeqrf_bufferSize(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSormqr_bufferSize(CuSolverDnHandle handle, SideMode side, Operation trans,
                int m, int n, int k, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr c, int ldc, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDormqr_bufferSize(CuSolverDnHandle handle, SideMode side, Operation trans,
                int m, int n, int k, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr c, int ldc, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCunmqr_bufferSize(CuSolverDnHandle handle, SideMode side, Operation trans,
                int m, int n, int k, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr c, int ldc, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZunmqr_bufferSize(CuSolverDnHandle handle, SideMode side, Operation trans,
                int m, int n, int k, CuDevicePtr a, int lda, CuDevicePtr tau, CuDevicePtr c, int ldc, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSorgqr_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDorgqr_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCungqr_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZungqr_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSorgqr(
                CuSolverDnHandle handle,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDorgqr(
                CuSolverDnHandle handle,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCungqr(
                CuSolverDnHandle handle,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZungqr(
                CuSolverDnHandle handle,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgebrd(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr d, CuDevicePtr e, CuDevicePtr tauq, CuDevicePtr taup, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgebrd(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr d, CuDevicePtr e, CuDevicePtr tauq, CuDevicePtr taup, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgebrd(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr d, CuDevicePtr e, CuDevicePtr tauq, CuDevicePtr taup, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgebrd(CuSolverDnHandle handle, int m, int n, CuDevicePtr a, int lda, CuDevicePtr d, CuDevicePtr e, CuDevicePtr tauq, CuDevicePtr taup, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsytrd_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr d,
                CuDevicePtr e,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsytrd_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr d,
                CuDevicePtr e,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsytrd(CuSolverDnHandle handle, char uplo, int n, CuDevicePtr a, int lda, CuDevicePtr d, CuDevicePtr e, CuDevicePtr tau, CuDevicePtr work, int lwork, CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsytrd(CuSolverDnHandle handle, char uplo, int n, CuDevicePtr a, int lda, CuDevicePtr d, CuDevicePtr e, CuDevicePtr tau, CuDevicePtr work, int lwork, CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnChetrd_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr d,
                CuDevicePtr e,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZhetrd_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr d,
                CuDevicePtr e,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnChetrd(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr d,
                CuDevicePtr e,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZhetrd(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr d,
                CuDevicePtr e,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgebrd_bufferSize(CuSolverDnHandle handle, int m, int n, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgebrd_bufferSize(CuSolverDnHandle handle, int m, int n, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgebrd_bufferSize(CuSolverDnHandle handle, int m, int n, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgebrd_bufferSize(CuSolverDnHandle handle, int m, int n, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgesvd_bufferSize(CuSolverDnHandle handle, int m, int n, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgesvd_bufferSize(CuSolverDnHandle handle, int m, int n, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgesvd_bufferSize(CuSolverDnHandle handle, int m, int n, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgesvd_bufferSize(CuSolverDnHandle handle, int m, int n, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgesvd(CuSolverDnHandle handle, [MarshalAs(UnmanagedType.I1)] char jobu, [MarshalAs(UnmanagedType.I1)] char jobvt, int m, int n, CuDevicePtr a, int lda, CuDevicePtr s, CuDevicePtr u, int ldu, CuDevicePtr vt, int ldvt, CuDevicePtr work, int lwork, CuDevicePtr rwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgesvd(CuSolverDnHandle handle, [MarshalAs(UnmanagedType.I1)] char jobu, [MarshalAs(UnmanagedType.I1)] char jobvt, int m, int n, CuDevicePtr a, int lda, CuDevicePtr s, CuDevicePtr u, int ldu, CuDevicePtr vt, int ldvt, CuDevicePtr work, int lwork, CuDevicePtr rwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgesvd(CuSolverDnHandle handle, [MarshalAs(UnmanagedType.I1)] char jobu, [MarshalAs(UnmanagedType.I1)] char jobvt, int m, int n, CuDevicePtr a, int lda, CuDevicePtr s, CuDevicePtr u, int ldu, CuDevicePtr vt, int ldvt, CuDevicePtr work, int lwork, CuDevicePtr rwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgesvd(CuSolverDnHandle handle, [MarshalAs(UnmanagedType.I1)] char jobu, [MarshalAs(UnmanagedType.I1)] char jobvt, int m, int n, CuDevicePtr a, int lda, CuDevicePtr s, CuDevicePtr u, int ldu, CuDevicePtr vt, int ldvt, CuDevicePtr work, int lwork, CuDevicePtr rwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsytrf(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, CuDevicePtr ipiv, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsytrf(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, CuDevicePtr ipiv, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCsytrf(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, CuDevicePtr ipiv, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZsytrf(CuSolverDnHandle handle, FillMode uplo, int n, CuDevicePtr a, int lda, CuDevicePtr ipiv, CuDevicePtr work, int lwork, CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsytrf_bufferSize(CuSolverDnHandle handle, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsytrf_bufferSize(CuSolverDnHandle handle, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCsytrf_bufferSize(CuSolverDnHandle handle, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZsytrf_bufferSize(CuSolverDnHandle handle, int n, CuDevicePtr a, int lda, ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSorgbr_bufferSize(
                CuSolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDorgbr_bufferSize(
                CuSolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCungbr_bufferSize(
                CuSolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZungbr_bufferSize(
                CuSolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSorgbr(
                CuSolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDorgbr(
                CuSolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCungbr(
                CuSolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZungbr(
                CuSolverDnHandle handle,
                SideMode side,
                int m,
                int n,
                int k,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSorgtr_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDorgtr_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCungtr_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZungtr_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSorgtr(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDorgtr(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCungtr(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZungtr(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSormtr_bufferSize(
                CuSolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr c,
                int ldc,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDormtr_bufferSize(
                CuSolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr c,
                int ldc,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCunmtr_bufferSize(
                CuSolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr c,
                int ldc,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZunmtr_bufferSize(
                CuSolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr c,
                int ldc,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSormtr(
                CuSolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr c,
                int ldc,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDormtr(
                CuSolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr c,
                int ldc,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCunmtr(
                CuSolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr c,
                int ldc,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZunmtr(
                CuSolverDnHandle handle,
                SideMode side,
                FillMode uplo,
                Operation trans,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr tau,
                CuDevicePtr c,
                int ldc,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsyevd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsyevd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCheevd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZheevd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsyevd(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsyevd(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCheevd(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZheevd(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsygvd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsygvd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnChegvd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZhegvd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsygvd(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsygvd(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnChegvd(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZhegvd(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsCreate(ref CuSolverDnIrsParams paramsPtr);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsDestroy(CuSolverDnIrsParams parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsSetRefinementSolver(
                CuSolverDnIrsParams parameters, CuSolverIrsRefinement refinementSolver);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsSetSolverMainPrecision(
                CuSolverDnIrsParams parameters, CuSolverPrecType solverMainPrecision);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsSetSolverLowestPrecision(
                CuSolverDnIrsParams parameters, CuSolverPrecType solverLowestPrecision);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsSetSolverPrecisions(
                CuSolverDnIrsParams parameters, CuSolverPrecType solverMainPrecision, CuSolverPrecType solverLowestPrecision);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsSetTol(CuSolverDnIrsParams parameters, double val);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsSetTolInner(CuSolverDnIrsParams parameters, double val);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsSetMaxIters(CuSolverDnIrsParams parameters, int maxiters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsSetMaxItersInner(CuSolverDnIrsParams parameters, int maxitersInner);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsGetMaxIters(CuSolverDnIrsParams parameters, ref int maxiters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsEnableFallback(CuSolverDnIrsParams parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSParamsDisableFallback(CuSolverDnIrsParams parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSInfosDestroy(
                CuSolverDnIrsInfos infos);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSInfosCreate(
                ref CuSolverDnIrsInfos infosPtr);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSInfosGetNiters(
                CuSolverDnIrsInfos infos,
                ref int niters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSInfosGetOuterNiters(
                CuSolverDnIrsInfos infos,
                ref int outerNiters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSInfosGetMaxIters(
                CuSolverDnIrsInfos infos,
                ref int maxiters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSInfosRequestResidual(
                CuSolverDnIrsInfos infos);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSInfosGetResidualHistory(
                CuSolverDnIrsInfos infos,
                ref IntPtr residualHistory);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZZgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZCgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZKgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZEgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZYgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCCgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCEgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCKgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCYgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDDgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDSgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDHgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDBgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDXgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSSgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSHgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSBgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSXgesv(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZZgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZCgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZKgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZEgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZYgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCCgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCKgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCEgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCYgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDDgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDSgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDHgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDBgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDXgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSSgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSHgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSBgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSXgesv_bufferSize(
                CuSolverDnHandle handle,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dipiv,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZZgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZCgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZKgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZEgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZYgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCCgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCKgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCEgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCYgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDDgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDSgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDHgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDBgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDXgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSSgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSHgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSBgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSXgels(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int iter,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZZgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZCgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZKgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZEgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZYgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCCgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCKgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCEgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCYgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDDgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDSgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDHgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDBgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDXgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSSgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSHgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSBgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSXgels_bufferSize(
                CuSolverDnHandle handle,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, ref SizeT lworkBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSXgesv(
                CuSolverDnHandle handle,
                CuSolverDnIrsParams gesvIrsParameters,
                CuSolverDnIrsInfos gesvIrsInfos,
                int n, int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int niters,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSXgesv_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnIrsParams parameters,
                int n, int nrhs,
                ref SizeT lworkBytes);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSXgels(
                CuSolverDnHandle handle,
                CuSolverDnIrsParams gelsIrsParams,
                CuSolverDnIrsInfos gelsIrsInfos,
                int m,
                int n,
                int nrhs,
                CuDevicePtr dA, int ldda,
                CuDevicePtr dB, int lddb,
                CuDevicePtr dX, int lddx,
                CuDevicePtr dWorkspace, SizeT lworkBytes,
                ref int niters,
                CuDevicePtr dInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnIRSXgels_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnIrsParams parameters,
                int m,
                int n,
                int nrhs,
                ref SizeT lworkBytes);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSpotrfBatched(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr aarray,
                int lda,
                CuDevicePtr infoArray,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDpotrfBatched(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr aarray,
                int lda,
                CuDevicePtr infoArray,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCpotrfBatched(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr aarray,
                int lda,
                CuDevicePtr infoArray,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZpotrfBatched(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr aarray,
                int lda,
                CuDevicePtr infoArray,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSpotrsBatched(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                int nrhs,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr dInfo,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDpotrsBatched(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                int nrhs,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr dInfo,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCpotrsBatched(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                int nrhs,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr dInfo,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZpotrsBatched(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                int nrhs,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr dInfo,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSpotri_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDpotri_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCpotri_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZpotri_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSpotri(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDpotri(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCpotri(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZpotri(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXtrtri_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                DiagType diag,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXtrtri(
                CuSolverDnHandle handle,
                FillMode uplo,
                DiagType diag,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr devInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSlauum_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDlauum_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnClauum_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZlauum_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSlauum(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDlauum(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnClauum(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr devInfo);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZlauum(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr devInfo);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsytrs_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CuDevicePtr ipiv,
                CudaDataType dataTypeB,
                CuDevicePtr b,
                long ldb,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsytrs(
                CuSolverDnHandle handle,
                FillMode uplo,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CuDevicePtr ipiv,
                CudaDataType dataTypeB,
                CuDevicePtr b,
                long ldb,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsytri_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr ipiv,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsytri_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr ipiv,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCsytri_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr ipiv,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZsytri_bufferSize(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr ipiv,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsytri(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr ipiv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsytri(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr ipiv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCsytri(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr ipiv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZsytri(
                CuSolverDnHandle handle,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr ipiv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsyevdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsyevdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCheevdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZheevdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsyevdx(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsyevdx(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCheevdx(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZheevdx(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsygvdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsygvdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnChegvdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                ref int lwork);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZhegvdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                ref int lwork);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsygvdx(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsygvdx(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnChegvdx(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                float vl,
                float vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZhegvdx(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                double vl,
                double vu,
                int il,
                int iu,
                ref int meig,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCreateSyevjInfo(
                ref SyevjInfo info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDestroySyevjInfo(
                SyevjInfo info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsyevjSetTolerance(
                SyevjInfo info,
                double tolerance);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsyevjSetMaxSweeps(
                SyevjInfo info,
                int maxSweeps);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsyevjSetSortEig(
                SyevjInfo info,
                int sortEig);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsyevjGetResidual(
                CuSolverDnHandle handle,
                SyevjInfo info,
                ref double residual);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsyevjGetSweeps(
                CuSolverDnHandle handle,
                SyevjInfo info,
                ref int executedSweeps);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsyevjBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsyevjBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCheevjBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZheevjBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsyevjBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsyevjBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCheevjBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZheevjBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsyevj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsyevj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCheevj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZheevj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsyevj(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsyevj(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCheevj(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZheevj(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsygvj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsygvj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnChegvj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZhegvj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                ref int lwork,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSsygvj(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDsygvj(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnChegvj(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZhegvj(
                CuSolverDnHandle handle,
                CuSolverEigType itype,
                CuSolverEigMode jobz,
                FillMode uplo,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr b,
                int ldb,
                CuDevicePtr w,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                SyevjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCreateGesvdjInfo(
                ref GesvdjInfo info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDestroyGesvdjInfo(
                GesvdjInfo info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvdjSetTolerance(
                GesvdjInfo info,
                double tolerance);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvdjSetMaxSweeps(
                GesvdjInfo info,
                int maxSweeps);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvdjSetSortEig(
                GesvdjInfo info,
                int sortSvd);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvdjGetResidual(
                CuSolverDnHandle handle,
                GesvdjInfo info,
                ref double residual);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvdjGetSweeps(
                CuSolverDnHandle handle,
                GesvdjInfo info,
                ref int executedSweeps);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgesvdjBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgesvdjBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgesvdjBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgesvdjBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgesvdjBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgesvdjBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgesvdjBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgesvdjBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                GesvdjInfo parameters,
                int batchSize);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgesvdj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int econ,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgesvdj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int econ,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgesvdj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int econ,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgesvdj_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int econ,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                ref int lwork,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgesvdj(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int econ,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgesvdj(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int econ,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgesvdj(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int econ,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                GesvdjInfo parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgesvdj(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int econ,
                int m,
                int n,
                CuDevicePtr a,
                int lda,
                CuDevicePtr s,
                CuDevicePtr u,
                int ldu,
                CuDevicePtr v,
                int ldv,
                CuDevicePtr work,
                int lwork,
                CuDevicePtr info,
                GesvdjInfo parameters);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgesvdaStridedBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int rank,
                int m,
                int n,
                CuDevicePtr dA,
                int lda,
                long strideA,
                CuDevicePtr dS,
                long strideS,
                CuDevicePtr dU,
                int ldu,
                long strideU,
                CuDevicePtr dV,
                int ldv,
                long strideV,
                ref int lwork,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgesvdaStridedBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int rank,
                int m,
                int n,
                CuDevicePtr dA,
                int lda,
                long strideA,
                CuDevicePtr dS,
                long strideS,
                CuDevicePtr dU,
                int ldu,
                long strideU,
                CuDevicePtr dV,
                int ldv,
                long strideV,
                ref int lwork,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgesvdaStridedBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int rank,
                int m,
                int n,
                CuDevicePtr dA,
                int lda,
                long strideA,
                CuDevicePtr dS,
                long strideS,
                CuDevicePtr dU,
                int ldu,
                long strideU,
                CuDevicePtr dV,
                int ldv,
                long strideV,
                ref int lwork,
                int batchSize
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgesvdaStridedBatched_bufferSize(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int rank,
                int m,
                int n,
                CuDevicePtr dA,
                int lda,
                long strideA,
                CuDevicePtr dS,
                long strideS,
                CuDevicePtr dU,
                int ldu,
                long strideU,
                CuDevicePtr dV,
                int ldv,
                long strideV,
                ref int lwork,
                int batchSize
            );


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSgesvdaStridedBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int rank,
                int m,
                int n,
                CuDevicePtr dA,
                int lda,
                long strideA,
                CuDevicePtr dS,
                long strideS,
                CuDevicePtr dU,
                int ldu,
                long strideU,
                CuDevicePtr dV,
                int ldv,
                long strideV,
                CuDevicePtr dWork,
                int lwork,
                CuDevicePtr dInfo,
                double[] hRNrmF,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDgesvdaStridedBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int rank,
                int m,
                int n,
                CuDevicePtr dA,
                int lda,
                long strideA,
                CuDevicePtr dS,
                long strideS,
                CuDevicePtr dU,
                int ldu,
                long strideU,
                CuDevicePtr dV,
                int ldv,
                long strideV,
                CuDevicePtr dWork,
                int lwork,
                CuDevicePtr dInfo,
                double[] hRNrmF,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCgesvdaStridedBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int rank,
                int m,
                int n,
                CuDevicePtr dA,
                int lda,
                long strideA,
                CuDevicePtr dS,
                long strideS,
                CuDevicePtr dU,
                int ldu,
                long strideU,
                CuDevicePtr dV,
                int ldv,
                long strideV,
                CuDevicePtr dWork,
                int lwork,
                CuDevicePtr dInfo,
                double[] hRNrmF,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnZgesvdaStridedBatched(
                CuSolverDnHandle handle,
                CuSolverEigMode jobz,
                int rank,
                int m,
                int n,
                CuDevicePtr dA,
                int lda,
                long strideA,
                CuDevicePtr dS,
                long strideS,
                CuDevicePtr dU,
                int ldu,
                long strideU,
                CuDevicePtr dV,
                int ldv,
                long strideV,
                CuDevicePtr dWork,
                int lwork,
                CuDevicePtr dInfo,
                double[] hRNrmF,
                int batchSize);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnCreateParams(
                ref CuSolverDnParams parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnDestroyParams(
                CuSolverDnParams parameters);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnSetAdvOptions(
                CuSolverDnParams parameters,
                CuSolverDnFunction function,
                CuSolverAlgMode algo);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnPotrf_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);

            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnPotrf(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType computeType,
                CuDevicePtr pBuffer,
                SizeT workspaceInBytes,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnPotrs(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                FillMode uplo,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeB,
                CuDevicePtr b,
                long ldb,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnGeqrf_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeTau,
                CuDevicePtr tau,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);

            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnGeqrf(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeTau,
                CuDevicePtr tau,
                CudaDataType computeType,
                CuDevicePtr pBuffer,
                SizeT workspaceInBytes,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnGetrf_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);

            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnGetrf(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CuDevicePtr ipiv,
                CudaDataType computeType,
                CuDevicePtr pBuffer,
                SizeT workspaceInBytes,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnGetrs(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                Operation trans,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CuDevicePtr ipiv,
                CudaDataType dataTypeB,
                CuDevicePtr b,
                long ldb,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnSyevd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeW,
                CuDevicePtr w,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);

            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnSyevd(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeW,
                CuDevicePtr w,
                CudaDataType computeType,
                CuDevicePtr pBuffer,
                SizeT workspaceInBytes,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnSyevdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                IntPtr vl,
                IntPtr vu,
                long il,
                long iu,
                ref long hMeig,
                CudaDataType dataTypeW,
                CuDevicePtr w,
                CudaDataType computeType,
                ref SizeT workspaceInBytes);


            [DllImport(CuSolveApiDllName)]
            [Obsolete("Deprecated in Cuda version 11.1")]
            public static extern CuSolverStatus cusolverDnSyevdx(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                IntPtr vl,
                IntPtr vu,
                long il,
                long iu,
                ref long meig64,
                CudaDataType dataTypeW,
                CuDevicePtr w,
                CudaDataType computeType,
                CuDevicePtr pBuffer,
                SizeT workspaceInBytes,
                CuDevicePtr info);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXpotrf_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXpotrf(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType computeType,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXpotrs(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                FillMode uplo,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeB,
                CuDevicePtr b,
                long ldb,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgeqrf_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeTau,
                CuDevicePtr tau,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgeqrf(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeTau,
                CuDevicePtr tau,
                CudaDataType computeType,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgetrf_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgetrf(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CuDevicePtr ipiv,
                CudaDataType computeType,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgetrs(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                Operation trans,
                long n,
                long nrhs,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CuDevicePtr ipiv,
                CudaDataType dataTypeB,
                CuDevicePtr b,
                long ldb,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsyevd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeW,
                CuDevicePtr w,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsyevd(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeW,
                CuDevicePtr w,
                CudaDataType computeType,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsyevdx_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                IntPtr vl,
                IntPtr vu,
                long il,
                long iu,
                ref long hMeig,
                CudaDataType dataTypeW,
                CuDevicePtr w,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXsyevdx(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                CuSolverEigRange range,
                FillMode uplo,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                IntPtr vl,
                IntPtr vu,
                long il,
                long iu,
                ref long meig64,
                CudaDataType dataTypeW,
                CuDevicePtr w,
                CudaDataType computeType,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvd_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                sbyte jobu,
                sbyte jobvt,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeS,
                CuDevicePtr s,
                CudaDataType dataTypeU,
                CuDevicePtr u,
                long ldu,
                CudaDataType dataTypeVt,
                CuDevicePtr vt,
                long ldvt,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvd(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                sbyte jobu,
                sbyte jobvt,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeS,
                CuDevicePtr s,
                CudaDataType dataTypeU,
                CuDevicePtr u,
                long ldu,
                CudaDataType dataTypeVt,
                CuDevicePtr vt,
                long ldvt,
                CudaDataType computeType,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr info);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvdp_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                int econ,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeS,
                CuDevicePtr s,
                CudaDataType dataTypeU,
                CuDevicePtr u,
                long ldu,
                CudaDataType dataTypeV,
                CuDevicePtr v,
                long ldv,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvdp(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                CuSolverEigMode jobz,
                int econ,
                long m,
                long n,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeS,
                CuDevicePtr s,
                CudaDataType dataTypeU,
                CuDevicePtr u,
                long ldu,
                CudaDataType dataTypeV,
                CuDevicePtr v,
                long ldv,
                CudaDataType computeType,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr dInfo,
                ref double hErrSigma);


            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvdr_bufferSize(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                sbyte jobu,
                sbyte jobv,
                long m,
                long n,
                long k,
                long p,
                long niters,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeSrand,
                CuDevicePtr srand,
                CudaDataType dataTypeUrand,
                CuDevicePtr urand,
                long ldUrand,
                CudaDataType dataTypeVrand,
                CuDevicePtr vrand,
                long ldVrand,
                CudaDataType computeType,
                ref SizeT workspaceInBytesOnDevice,
                ref SizeT workspaceInBytesOnHost
            );

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverDnXgesvdr(
                CuSolverDnHandle handle,
                CuSolverDnParams parameters,
                sbyte jobu,
                sbyte jobv,
                long m,
                long n,
                long k,
                long p,
                long niters,
                CudaDataType dataTypeA,
                CuDevicePtr a,
                long lda,
                CudaDataType dataTypeSrand,
                CuDevicePtr srand,
                CudaDataType dataTypeUrand,
                CuDevicePtr urand,
                long ldUrand,
                CudaDataType dataTypeVrand,
                CuDevicePtr vrand,
                long ldVrand,
                CudaDataType computeType,
                CuDevicePtr bufferOnDevice,
                SizeT workspaceInBytesOnDevice,
                byte[] bufferOnHost,
                SizeT workspaceInBytesOnHost,
                CuDevicePtr dInfo
            );
        }

        public static class Sparse
        {
            static Sparse()
            {
                DriverApiNativeMethods.Init();
            }

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpCreate(ref CuSolverSpHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDestroy(CuSolverSpHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpSetStream(CuSolverSpHandle handle, CuStream streamId);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpGetStream(CuSolverSpHandle handle, ref CuStream streamId);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpXcsrissymHost(CuSolverSpHandle handle, int m, int nnzA, CuSparseMatDescriptor descrA, int[] csrRowPtrA, int[] csrEndPtrA, int[] csrColIndA, ref int issym);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpScsrlsvluHost(CuSolverSpHandle handle, int n, int nnzA, CuSparseMatDescriptor descrA, float[] csrValA, int[] csrRowPtrA, int[] csrColIndA, float[] b, float tol, int reorder, float[] x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDcsrlsvluHost(CuSolverSpHandle handle, int n, int nnzA, CuSparseMatDescriptor descrA, double[] csrValA, int[] csrRowPtrA, int[] csrColIndA, double[] b, double tol, int reorder, double[] x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpScsrlsvqr(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrValA, CuDevicePtr csrRowPtrA, CuDevicePtr csrColIndA, CuDevicePtr b, float tol, int reorder, CuDevicePtr x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDcsrlsvqr(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrValA, CuDevicePtr csrRowPtrA, CuDevicePtr csrColIndA, CuDevicePtr b, double tol, int reorder, CuDevicePtr x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpCcsrlsvqr(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrValA, CuDevicePtr csrRowPtrA, CuDevicePtr csrColIndA, CuDevicePtr b, float tol, int reorder, CuDevicePtr x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpZcsrlsvqr(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrValA, CuDevicePtr csrRowPtrA, CuDevicePtr csrColIndA, CuDevicePtr b, double tol, int reorder, CuDevicePtr x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpScsrlsvqrHost(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, float[] csrValA, int[] csrRowPtrA, int[] csrColIndA, float[] b, float tol, int reorder, float[] x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDcsrlsvqrHost(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, double[] csrValA, int[] csrRowPtrA, int[] csrColIndA, double[] b, double tol, int reorder, double[] x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpScsrlsvcholHost(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, float[] csrVal, int[] csrRowPtr, int[] csrColInd, float[] b, float tol, int reorder, float[] x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDcsrlsvcholHost(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, double[] csrVal, int[] csrRowPtr, int[] csrColInd, double[] b, double tol, int reorder, double[] x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpScsrlsvchol(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrVal, CuDevicePtr csrRowPtr, CuDevicePtr csrColInd, CuDevicePtr b, float tol, int reorder, CuDevicePtr x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDcsrlsvchol(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrVal, CuDevicePtr csrRowPtr, CuDevicePtr csrColInd, CuDevicePtr b, double tol, int reorder, CuDevicePtr x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpCcsrlsvchol(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrVal, CuDevicePtr csrRowPtr, CuDevicePtr csrColInd, CuDevicePtr b, float tol, int reorder, CuDevicePtr x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpZcsrlsvchol(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrVal, CuDevicePtr csrRowPtr, CuDevicePtr csrColInd, CuDevicePtr b, double tol, int reorder, CuDevicePtr x, ref int singularity);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpScsrlsqvqrHost(CuSolverSpHandle handle, int m, int n, int nnz, CuSparseMatDescriptor descrA, float[] csrValA, int[] csrRowPtrA, int[] csrColIndA, float[] b, float tol, ref int rankA, float[] x, int[] p, ref float minNorm);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDcsrlsqvqrHost(CuSolverSpHandle handle, int m, int n, int nnz, CuSparseMatDescriptor descrA, double[] csrValA, int[] csrRowPtrA, int[] csrColIndA, double[] b, double tol, ref int rankA, double[] x, int[] p, ref double minNorm);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpScsreigvsiHost(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, float[] csrValA, int[] csrRowPtrA, int[] csrColIndA, float mu0, float[] x0, int maxite, float tol, ref float mu, float[] x);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDcsreigvsiHost(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, double[] csrValA, int[] csrRowPtrA, int[] csrColIndA, double mu0, double[] x0, int maxite, double tol, ref double mu, double[] x);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpScsreigvsi(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrValA, CuDevicePtr csrRowPtrA, CuDevicePtr csrColIndA, float mu0, CuDevicePtr x0, int maxite, float tol, ref float mu, CuDevicePtr x);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDcsreigvsi(CuSolverSpHandle handle, int m, int nnz, CuSparseMatDescriptor descrA, CuDevicePtr csrValA, CuDevicePtr csrRowPtrA, CuDevicePtr csrColIndA, double mu0, CuDevicePtr x0, int maxite, double tol, ref double mu, CuDevicePtr x);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpXcsrsymrcmHost(CuSolverSpHandle handle, int n, int nnzA, CuSparseMatDescriptor descrA, int[] csrRowPtrA, int[] csrColIndA, int[] p);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpXcsrsymmdqHost(
                CuSolverSpHandle handle,
                int n,
                int nnzA,
                CuSparseMatDescriptor descrA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                int[] p);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpXcsrsymamdHost(
                CuSolverSpHandle handle,
                int n,
                int nnzA,
                CuSparseMatDescriptor descrA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                int[] p);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpXcsrmetisndHost(
                CuSolverSpHandle handle,
                int n,
                int nnzA,
                CuSparseMatDescriptor descrA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                long[] options,
                int[] p);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpScsrzfdHost(
                CuSolverSpHandle handle,
                int n,
                int nnz,
                CuSparseMatDescriptor descrA,
                float[] csrValA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                int[] p,
                ref int numnz);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpDcsrzfdHost(
                CuSolverSpHandle handle,
                int n,
                int nnz,
                CuSparseMatDescriptor descrA,
                double[] csrValA,
                int[] csrRowPtrA,
                int[] csrColIndA,
                int[] p,
                ref int numnz);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpXcsrperm_bufferSizeHost(CuSolverSpHandle handle, int m, int n, int nnzA, CuSparseMatDescriptor descrA, int[] csrRowPtrA, int[] csrColIndA, int[] p, int[] q, ref SizeT bufferSizeInBytes);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverSpXcsrpermHost(CuSolverSpHandle handle, int m, int n, int nnzA, CuSparseMatDescriptor descrA, int[] csrRowPtrA, int[] csrColIndA, int[] p, int[] q, int[] map, byte[] pBuffer);
        }

        public static class Refactorization
        {
            static Refactorization()
            {
                DriverApiNativeMethods.Init();
            }

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfCreate(ref CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfDestroy(CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfGetMatrixFormat(CuSolverRfHandle handle, ref MatrixFormat format, ref UnitDiagonal diag);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfSetMatrixFormat(CuSolverRfHandle handle, MatrixFormat format, UnitDiagonal diag);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfSetNumericProperties(CuSolverRfHandle handle, double zero, double boost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfGetNumericProperties(CuSolverRfHandle handle, ref double zero, ref double boost);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfGetNumericBoostReport(CuSolverRfHandle handle, ref NumericBoostReport report);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfSetAlgs(CuSolverRfHandle handle, Factorization factAlg, TriangularSolve solveAlg);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfGetAlgs(CuSolverRfHandle handle, ref Factorization factAlg, ref TriangularSolve solveAlg);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfGetResetValuesFastMode(CuSolverRfHandle handle, ref ResetValuesFastMode fastMode);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfSetResetValuesFastMode(CuSolverRfHandle handle, ResetValuesFastMode fastMode);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfSetupHost(int n, int nnzA, int[] hCsrRowPtrA, int[] hCsrColIndA, double[] hCsrValA, int nnzL, int[] hCsrRowPtrL, int[] hCsrColIndL, double[] hCsrValL, int nnzU, int[] hCsrRowPtrU, int[] hCsrColIndU, double[] hCsrValU, int[] hP, int[] hQ, CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfSetupDevice(int n, int nnzA, CuDevicePtr csrRowPtrA, CuDevicePtr csrColIndA, CuDevicePtr csrValA, int nnzL,
                CuDevicePtr csrRowPtrL, CuDevicePtr csrColIndL, CuDevicePtr csrValL, int nnzU, CuDevicePtr csrRowPtrU, CuDevicePtr csrColIndU, CuDevicePtr csrValU,
                CuDevicePtr p, CuDevicePtr q, CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfResetValues(int n, int nnzA, CuDevicePtr csrRowPtrA, CuDevicePtr csrColIndA, CuDevicePtr csrValA,
                CuDevicePtr p, CuDevicePtr q, CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfAnalyze(CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfRefactor(CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfAccessBundledFactorsDevice(CuSolverRfHandle handle, ref int nnzM, ref CuDevicePtr mp, ref CuDevicePtr mi, ref CuDevicePtr mx);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfExtractBundledFactorsHost(CuSolverRfHandle handle, ref int hNnzM, ref IntPtr hMp, ref IntPtr hMi, ref IntPtr hMx);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfExtractSplitFactorsHost(CuSolverRfHandle handle, ref int hNnzL, ref IntPtr hCsrRowPtrL, ref IntPtr hCsrColIndL, ref IntPtr hCsrValL, ref int hNnzU, ref IntPtr hCsrRowPtrU, ref IntPtr hCsrColIndU, ref IntPtr hCsrValU);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfSolve(CuSolverRfHandle handle, CuDevicePtr p, CuDevicePtr q, int nrhs, double[] temp, int ldt, double[] xf, int ldxf);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfBatchSetupHost(int batchSize, int n, int nnzA, int[] hCsrRowPtrA, int[] hCsrColIndA, IntPtr[] hCsrValAArray, int nnzL,
                int[] hCsrRowPtrL, int[] hCsrColIndL, double[] hCsrValL, int nnzU, int[] hCsrRowPtrU, int[] hCsrColIndU, double[] hCsrValU, int[] hP, int[] hQ, CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfBatchResetValues(int batchSize, int n, int nnzA, CuDevicePtr csrRowPtrA, CuDevicePtr csrColIndA, CuDevicePtr[] csrValAArray,
                CuDevicePtr p, CuDevicePtr q, CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfBatchAnalyze(CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfBatchRefactor(CuSolverRfHandle handle);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfBatchSolve(CuSolverRfHandle handle, CuDevicePtr p, CuDevicePtr q, int nrhs, double[] temp,
                int ldt, IntPtr[] xfArray, int ldxf);

            [DllImport(CuSolveApiDllName)]
            public static extern CuSolverStatus cusolverRfBatchZeroPivot(CuSolverRfHandle handle, int[] position);
        }
    }
}