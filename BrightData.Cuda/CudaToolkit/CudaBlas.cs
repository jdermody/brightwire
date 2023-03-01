using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Cuda.CudaToolkit
{
    /// <summary>
    /// Wrapper for CUBLAS
    /// </summary>
    public class CudaBlas : IDisposable
    {
        bool disposed;
        CudaBlasHandle _blasHandle;
        CublasStatus _status;

        /// <summary>
        /// Creates a new cudaBlas handler
        /// </summary>
        public CudaBlas()
        {
            _blasHandle = new CudaBlasHandle();
            _status = CudaBlasNativeMethods.cublasCreate_v2(ref _blasHandle);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasCreate_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// Creates a new cudaBlas handler
        /// </summary>
        public CudaBlas(CUstream stream)
            : this()
        {
            Stream = stream;
        }
        /// <summary>
        /// Creates a new cudaBlas handler
        /// </summary>
        public CudaBlas(PointerMode pointermode)
            : this()
        {
            PointerMode = pointermode;
        }
        /// <summary>
        /// Creates a new cudaBlas handler
        /// </summary>
        public CudaBlas(AtomicsMode atomicsmode)
            : this()
        {
            AtomicsMode = atomicsmode;
        }
        /// <summary>
        /// Creates a new cudaBlas handler
        /// </summary>
        public CudaBlas(CUstream stream, PointerMode pointermode)
            : this()
        {
            Stream = stream;
            PointerMode = pointermode;
        }
        /// <summary>
        /// Creates a new cudaBlas handler
        /// </summary>
        public CudaBlas(CUstream stream, AtomicsMode atomicsmode)
            : this()
        {
            Stream = stream;
            AtomicsMode = atomicsmode;
        }
        /// <summary>
        /// Creates a new cudaBlas handler
        /// </summary>
        public CudaBlas(PointerMode pointermode, AtomicsMode atomicsmode)
            : this()
        {
            PointerMode = pointermode;
            AtomicsMode = atomicsmode;
        }
        /// <summary>
        /// Creates a new cudaBlas handler
        /// </summary>
        public CudaBlas(CUstream stream, PointerMode pointermode, AtomicsMode atomicsmode)
            : this()
        {
            Stream = stream;
            PointerMode = pointermode;
            AtomicsMode = atomicsmode;
        }

        /// <summary>
        /// For dispose
        /// </summary>
        ~CudaBlas()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// For IDisposable
        /// </summary>
        /// <param name="fDisposing"></param>
        protected virtual void Dispose(bool fDisposing)
        {
            if (fDisposing && !disposed)
            {
                _status = CudaBlasNativeMethods.cublasDestroy_v2(_blasHandle);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDestroy_v2", _status));
                disposed = true;
            }
            if (!fDisposing && !disposed)
                Debug.WriteLine(String.Format("CudaBlas not-disposed warning: {0}", this.GetType()));
        }

        /// <summary>
        /// Returns the wrapped cublas handle
        /// </summary>
        public CudaBlasHandle CublasHandle
        {
            get { return _blasHandle; }
        }

        /// <summary>
        /// 
        /// </summary>
        public CUstream Stream
        {
            get
            {
                CUstream stream = new CUstream();
                _status = CudaBlasNativeMethods.cublasGetStream_v2(_blasHandle, ref stream);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetStream_v2", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
                return stream;
            }
            set
            {
                _status = CudaBlasNativeMethods.cublasSetStream_v2(_blasHandle, value);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetStream_v2", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public PointerMode PointerMode
        {
            get
            {
                PointerMode pm = new PointerMode();
                _status = CudaBlasNativeMethods.cublasGetPointerMode_v2(_blasHandle, ref pm);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetPointerMode_v2", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
                return pm;
            }
            set
            {
                _status = CudaBlasNativeMethods.cublasSetPointerMode_v2(_blasHandle, value);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetPointerMode_v2", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public AtomicsMode AtomicsMode
        {
            get
            {
                AtomicsMode am = new AtomicsMode();
                _status = CudaBlasNativeMethods.cublasGetAtomicsMode(_blasHandle, ref am);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetAtomicsMode", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
                return am;
            }
            set
            {
                _status = CudaBlasNativeMethods.cublasSetAtomicsMode(_blasHandle, value);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetAtomicsMode", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Math MathMode
        {
            get
            {
                Math mm = new Math();
                _status = CudaBlasNativeMethods.cublasGetMathMode(_blasHandle, ref mm);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetMathMode", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
                return mm;
            }
            set
            {
                _status = CudaBlasNativeMethods.cublasSetMathMode(_blasHandle, value);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetMathMode", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int SmCountTarget
        {
            get
            {
                int sm = 0;
                _status = CudaBlasNativeMethods.cublasGetSmCountTarget(_blasHandle, ref sm);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetSmCountTarget", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
                return sm;
            }
            set
            {
                _status = CudaBlasNativeMethods.cublasSetSmCountTarget(_blasHandle, value);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetSmCountTarget", _status));
                if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Version GetVersion()
        {
            int version = 0;
            _status = CudaBlasNativeMethods.cublasGetVersion_v2(_blasHandle, ref version);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetVersion_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return new Version(version / 1000, version % 1000);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetWorkspace(CudaDeviceVariable<byte> workspace)
        {
            _status = CudaBlasNativeMethods.cublasSetWorkspace_v2(_blasHandle, workspace.DevicePointer, workspace.SizeInBytes);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetWorkspace_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function copies the vector x into the vector y.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="yType"></param>
        /// <param name="incy"></param>
        public void Copy(int n, CUdeviceptr x, cudaDataType xType, int incx, CUdeviceptr y, cudaDataType yType, int incy)
        {
            _status = CudaBlasNativeMethods.cublasCopyEx(_blasHandle, n, x, xType, incx, y, yType, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasCopyEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function copies the vector x into the vector y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Copy(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasScopy_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasScopy_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function copies the vector x into the vector y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Copy(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDcopy_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDcopy_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function interchanges the elements of vector x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Swap(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSswap_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSswap_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function interchanges the elements of vector x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Swap(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDswap_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDswap_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function interchanges the elements of vector x and y.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="yType"></param>
        /// <param name="incy"></param>
        public void Swap(int n, CUdeviceptr x, cudaDataType xType, int incx, CUdeviceptr y, cudaDataType yType, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSwapEx(_blasHandle, n, x, xType, incx, y, yType, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSwapEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Norm2(CudaDeviceVariable<float> x, int incx, ref float result)
        {
            _status = CudaBlasNativeMethods.cublasSnrm2_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSnrm2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public float Norm2(CudaDeviceVariable<float> x, int incx)
        {
            float result = 0;
            _status = CudaBlasNativeMethods.cublasSnrm2_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSnrm2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }

        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Norm2(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> result)
        {
            _status = CudaBlasNativeMethods.cublasSnrm2_v2(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSnrm2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Norm2(CudaDeviceVariable<double> x, int incx, ref double result)
        {
            _status = CudaBlasNativeMethods.cublasDnrm2_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDnrm2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public double Norm2(CudaDeviceVariable<double> x, int incx)
        {
            double result = 0;
            _status = CudaBlasNativeMethods.cublasDnrm2_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDnrm2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }

        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Norm2(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> result)
        {
            _status = CudaBlasNativeMethods.cublasDnrm2_v2(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDnrm2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="result"></param>
        public void Dot(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, ref float result)
        {
            _status = CudaBlasNativeMethods.cublasSdot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSdot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public float Dot(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy)
        {
            float result = 0;
            _status = CudaBlasNativeMethods.cublasSdot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSdot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }

        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="result"></param>
        public void Dot(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, CudaDeviceVariable<float> result)
        {
            _status = CudaBlasNativeMethods.cublasSdot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSdot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="result"></param>
        public void Dot(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, ref double result)
        {
            _status = CudaBlasNativeMethods.cublasDdot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDdot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public double Dot(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy)
        {
            double result = 0;
            _status = CudaBlasNativeMethods.cublasDdot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDdot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }

        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="result"></param>
        public void Dot(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, CudaDeviceVariable<double> result)
        {
            _status = CudaBlasNativeMethods.cublasDdot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDdot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function scales the vector x by the scalar and overwrites it with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public void Scale(float alpha, CudaDeviceVariable<float> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasSscal_v2(_blasHandle, x.Size, ref alpha, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSscal_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function scales the vector x by the scalar and overwrites it with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public void Scale(CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasSscal_v2(_blasHandle, x.Size, alpha.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSscal_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function scales the vector x by the scalar and overwrites it with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public void Scale(double alpha, CudaDeviceVariable<double> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasDscal_v2(_blasHandle, x.Size, ref alpha, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDscal_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function scales the vector x by the scalar and overwrites it with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public void Scale(CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasDscal_v2(_blasHandle, x.Size, alpha.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDscal_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function multiplies the vector x by the scalar and adds it to the vector y overwriting
        /// the latest vector with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Axpy(float alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSaxpy_v2(_blasHandle, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSaxpy_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function multiplies the vector x by the scalar and adds it to the vector y overwriting
        /// the latest vector with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Axpy(CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSaxpy_v2(_blasHandle, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSaxpy_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function multiplies the vector x by the scalar and adds it to the vector y overwriting
        /// the latest vector with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Axpy(double alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDaxpy_v2(_blasHandle, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDaxpy_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function multiplies the vector x by the scalar and adds it to the vector y overwriting
        /// the latest vector with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Axpy(CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDaxpy_v2(_blasHandle, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDaxpy_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(CudaDeviceVariable<float> x, int incx, ref int result)
        {
            _status = CudaBlasNativeMethods.cublasIsamin_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamin_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public int Min(CudaDeviceVariable<float> x, int incx)
        {
            int result = 0;
            _status = CudaBlasNativeMethods.cublasIsamin_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamin_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<int> result)
        {
            _status = CudaBlasNativeMethods.cublasIsamin_v2(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamin_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(CudaDeviceVariable<double> x, int incx, ref int result)
        {
            _status = CudaBlasNativeMethods.cublasIdamin_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamin_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public int Min(CudaDeviceVariable<double> x, int incx)
        {
            int result = 0;
            _status = CudaBlasNativeMethods.cublasIdamin_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamin_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<int> result)
        {
            _status = CudaBlasNativeMethods.cublasIdamin_v2(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamin_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(int n, CUdeviceptr x, cudaDataType xType, int incx, CudaDeviceVariable<int> result)
        {
            _status = CudaBlasNativeMethods.cublasIaminEx(_blasHandle, n, x, xType, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIaminEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(CudaDeviceVariable<float> x, int incx, ref int result)
        {
            _status = CudaBlasNativeMethods.cublasIsamax_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamax_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public int Max(CudaDeviceVariable<float> x, int incx)
        {
            int result = 0;
            _status = CudaBlasNativeMethods.cublasIsamax_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamax_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<int> result)
        {
            _status = CudaBlasNativeMethods.cublasIsamax_v2(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamax_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(CudaDeviceVariable<double> x, int incx, ref int result)
        {
            _status = CudaBlasNativeMethods.cublasIdamax_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamax_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public int Max(CudaDeviceVariable<double> x, int incx)
        {
            int result = 0;
            _status = CudaBlasNativeMethods.cublasIdamax_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamax_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<int> result)
        {
            _status = CudaBlasNativeMethods.cublasIdamax_v2(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamax_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(int n, CUdeviceptr x, cudaDataType xType, int incx, CudaDeviceVariable<int> result)
        {
            _status = CudaBlasNativeMethods.cublasIamaxEx(_blasHandle, n, x, xType, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIamaxEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void AbsoluteSum(CudaDeviceVariable<float> x, int incx, ref float result)
        {
            _status = CudaBlasNativeMethods.cublasSasum_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSasum_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public float AbsoluteSum(CudaDeviceVariable<float> x, int incx)
        {
            float result = 0;
            _status = CudaBlasNativeMethods.cublasSasum_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSasum_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void AbsoluteSum(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> result)
        {
            _status = CudaBlasNativeMethods.cublasSasum_v2(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSasum_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        /// <param name="resultType"></param>
        /// <param name="executiontype"></param>
        public void AbsoluteSum(int n, CUdeviceptr x, cudaDataType xType, int incx, CUdeviceptr result, cudaDataType resultType, cudaDataType executiontype)
        {
            _status = CudaBlasNativeMethods.cublasAsumEx(_blasHandle, n, x, xType, incx, result, resultType, executiontype);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasAsumEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void AbsoluteSum(CudaDeviceVariable<double> x, int incx, ref double result)
        {
            _status = CudaBlasNativeMethods.cublasDasum_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDasum_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public double AbsoluteSum(CudaDeviceVariable<double> x, int incx)
        {
            double result = 0;
            _status = CudaBlasNativeMethods.cublasDasum_v2(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDasum_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void AbsoluteSum(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> result)
        {
            _status = CudaBlasNativeMethods.cublasIdamax_v2(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamax_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rot(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, float c, float s)
        {
            _status = CudaBlasNativeMethods.cublasSrot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref c, ref s);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rot(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, CudaDeviceVariable<float> c, CudaDeviceVariable<float> s)
        {
            _status = CudaBlasNativeMethods.cublasSrot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, c.DevicePointer, s.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rot(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, double c, double s)
        {
            _status = CudaBlasNativeMethods.cublasDrot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref c, ref s);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rot(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, CudaDeviceVariable<double> c, CudaDeviceVariable<double> s)
        {
            _status = CudaBlasNativeMethods.cublasDrot_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, c.DevicePointer, s.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrot_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        
        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="yType"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        /// <param name="csType"></param>
        /// <param name="executiontype"></param>
        public void Rot(int n, CUdeviceptr x, cudaDataType xType, int incx, CUdeviceptr y, cudaDataType yType, int incy, CUdeviceptr c, CUdeviceptr s, cudaDataType csType, cudaDataType executiontype)
        {
            _status = CudaBlasNativeMethods.cublasRotEx(_blasHandle, n, x, xType, incx, y, yType, incy, c, s, csType, executiontype);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasRotEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function constructs the Givens rotation matrix G = |c s; -s c| that zeros out the second entry of a 2x1 vector (a; b)T
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rotg(CudaDeviceVariable<float> a, CudaDeviceVariable<float> b, CudaDeviceVariable<float> c, CudaDeviceVariable<float> s)
        {
            _status = CudaBlasNativeMethods.cublasSrotg_v2(_blasHandle, a.DevicePointer, b.DevicePointer, c.DevicePointer, s.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrotg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function constructs the Givens rotation matrix G = |c s; -s c| that zeros out the second entry of a 2x1 vector (a; b)T
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rotg(ref float a, ref float b, ref float c, ref float s)
        {
            _status = CudaBlasNativeMethods.cublasSrotg_v2(_blasHandle, ref a, ref b, ref c, ref s);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrotg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function constructs the Givens rotation matrix G = |c s; -s c| that zeros out the second entry of a 2x1 vector (a; b)T
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rotg(CudaDeviceVariable<double> a, CudaDeviceVariable<double> b, CudaDeviceVariable<double> c, CudaDeviceVariable<double> s)
        {
            _status = CudaBlasNativeMethods.cublasDrotg_v2(_blasHandle, a.DevicePointer, b.DevicePointer, c.DevicePointer, s.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrotg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function constructs the Givens rotation matrix G = |c s; -s c| that zeros out the second entry of a 2x1 vector (a; b)T
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rotg(ref double a, ref double b, ref double c, ref double s)
        {
            _status = CudaBlasNativeMethods.cublasDrotg_v2(_blasHandle, ref a, ref b, ref c, ref s);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrotg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, float[] param)
        {
            _status = CudaBlasNativeMethods.cublasSrotm_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, param);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrotg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, CudaDeviceVariable<float> param)
        {
            _status = CudaBlasNativeMethods.cublasSrotm_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, param.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrotg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, double[] param)
        {
            _status = CudaBlasNativeMethods.cublasDrotm_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, param);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrotg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, CudaDeviceVariable<double> param)
        {
            _status = CudaBlasNativeMethods.cublasDrotm_v2(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, param.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrotg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        /// <param name="executiontype"></param>
        /// <param name="n"></param>
        /// <param name="paramType"></param>
        /// <param name="xType"></param>
        /// <param name="yType"></param>
        public void Rotm(CUdeviceptr x, int n, cudaDataType xType, int incx, CUdeviceptr y, cudaDataType yType, int incy, CUdeviceptr param, cudaDataType paramType, cudaDataType executiontype)
        {
            _status = CudaBlasNativeMethods.cublasRotmEx(_blasHandle, n, x, xType, incx, y, yType, incy, param, paramType, executiontype);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasRotmEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function constructs the modified Givens transformation H = |h11 h12; h21 h22| that zeros out the second entry of a 2x1 vector 
        /// [sqrt(d1)*x1; sqrt(d2)*y1].<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="param"></param>
        public void Rotm(ref float d1, ref float d2, ref float x1, float y1, float[] param)
        {
            _status = CudaBlasNativeMethods.cublasSrotmg_v2(_blasHandle, ref d1, ref d2, ref x1, ref y1, param);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrotmg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function constructs the modified Givens transformation H = |h11 h12; h21 h22| that zeros out the second entry of a 2x1 vector 
        /// [sqrt(d1)*x1; sqrt(d2)*y1].<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<float> d1, CudaDeviceVariable<float> d2, CudaDeviceVariable<float> x1, CudaDeviceVariable<float> y1, CudaDeviceVariable<float> param)
        {
            _status = CudaBlasNativeMethods.cublasSrotmg_v2(_blasHandle, d1.DevicePointer, d2.DevicePointer, x1.DevicePointer, y1.DevicePointer, param.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrotmg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function constructs the modified Givens transformation H = |h11 h12; h21 h22| that zeros out the second entry of a 2x1 vector 
        /// [sqrt(d1)*x1; sqrt(d2)*y1].<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="param"></param>
        public void Rotm(ref double d1, ref double d2, ref double x1, double y1, double[] param)
        {
            _status = CudaBlasNativeMethods.cublasDrotmg_v2(_blasHandle, ref d1, ref d2, ref x1, ref y1, param);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrotmg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function constructs the modified Givens transformation H = |h11 h12; h21 h22| that zeros out the second entry of a 2x1 vector 
        /// [sqrt(d1)*x1; sqrt(d2)*y1].<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="param"></param>
        /// <param name="d1Type"></param>
        /// <param name="d2Type"></param>
        /// <param name="x1Type"></param>
        /// <param name="y1Type"></param>
        /// <param name="paramType"></param>
        /// <param name="executiontype"></param>
        public void Rotm(CUdeviceptr d1, cudaDataType d1Type, CUdeviceptr d2, cudaDataType d2Type, CUdeviceptr x1, cudaDataType x1Type, CUdeviceptr y1, cudaDataType y1Type, CUdeviceptr param, cudaDataType paramType, cudaDataType executiontype)
        {
            _status = CudaBlasNativeMethods.cublasRotmgEx(_blasHandle, d1, d1Type, d2, d2Type, x1, x1Type, y1, y1Type, param, paramType, executiontype);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasRotmgEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function constructs the modified Givens transformation H = |h11 h12; h21 h22| that zeros out the second entry of a 2x1 vector 
        /// [sqrt(d1)*x1; sqrt(d2)*y1].<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<double> d1, CudaDeviceVariable<double> d2, CudaDeviceVariable<double> x1, CudaDeviceVariable<double> y1, CudaDeviceVariable<double> param)
        {
            _status = CudaBlasNativeMethods.cublasDrotmg_v2(_blasHandle, d1.DevicePointer, d2.DevicePointer, x1.DevicePointer, y1.DevicePointer, param.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrotmg_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the triangular matrix-vector multiplication x= Op(A) x where A is a triangular matrix stored in 
        /// lower or upper mode with or without the main diagonal, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Trmv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasStrmv_v2(_blasHandle, uplo, trans, diag, x.Size, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular matrix-vector multiplication x= Op(A) x where A is a triangular matrix stored in 
        /// lower or upper mode with or without the main diagonal, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Trmv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasDtrmv_v2(_blasHandle, uplo, trans, diag, x.Size, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the triangular banded matrix-vector multiplication x= Op(A) x where A is a triangular banded matrix, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tbmv(FillMode uplo, Operation trans, DiagType diag, int k, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasStbmv_v2(_blasHandle, uplo, trans, diag, x.Size, k, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular banded matrix-vector multiplication x= Op(A) x where A is a triangular banded matrix, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tbmv(FillMode uplo, Operation trans, DiagType diag, int k, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasDtbmv_v2(_blasHandle, uplo, trans, diag, x.Size, k, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the triangular packed matrix-vector multiplication x= Op(A) x where A is a triangular matrix stored in packed format, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tpmv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<float> AP, CudaDeviceVariable<float> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasStpmv_v2(_blasHandle, uplo, trans, diag, x.Size, AP.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStpmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular packed matrix-vector multiplication x= Op(A) x where A is a triangular matrix stored in packed format, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tpmv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<double> AP, CudaDeviceVariable<double> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasDtpmv_v2(_blasHandle, uplo, trans, diag, x.Size, AP.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtpmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves the triangular linear system with a single right-hand-side Op(A)x = b where A is a triangular matrix stored in lower or 
        /// upper mode with or without the main diagonal, and x and b are vectors. The solution x overwrites the right-hand-sides b on exit. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Trsv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasStrsv_v2(_blasHandle, uplo, trans, diag, x.Size, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the triangular linear system with a single right-hand-side Op(A)x = b where A is a triangular matrix stored in lower or 
        /// upper mode with or without the main diagonal, and x and b are vectors. The solution x overwrites the right-hand-sides b on exit. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Trsv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasDtrsv_v2(_blasHandle, uplo, trans, diag, x.Size, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves the packed triangular linear system with a single right-hand-side Op(A) x = b where A is a triangular matrix stored in packed format, and x and b are vectors. 
        /// The solution x overwrites the right-hand-sides b on exit. n is given by x.Size. No test for singularity or near-singularity is included in this function.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tpsv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<float> AP, CudaDeviceVariable<float> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasStpsv_v2(_blasHandle, uplo, trans, diag, x.Size, AP.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStpsv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the packed triangular linear system with a single right-hand-side Op(A) x = b where A is a triangular matrix stored in packed format, and x and b are vectors. 
        /// The solution x overwrites the right-hand-sides b on exit. n is given by x.Size. No test for singularity or near-singularity is included in this function.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tpsv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<double> AP, CudaDeviceVariable<double> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasDtpsv_v2(_blasHandle, uplo, trans, diag, x.Size, AP.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtpsv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves the triangular banded linear system with a single right-hand-side Op(A) x = b where A is a triangular banded matrix, and x and b is a vector. 
        /// The solution x overwrites the right-hand-sides b on exit. n is given by x.Size. No test for singularity or near-singularity is included in this function.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tbsv(FillMode uplo, Operation trans, DiagType diag, int k, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasStbsv_v2(_blasHandle, uplo, trans, diag, x.Size, k, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStbsv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the triangular banded linear system with a single right-hand-side Op(A) x = b where A is a triangular banded matrix, and x and b is a vector. 
        /// The solution x overwrites the right-hand-sides b on exit. n is given by x.Size. No test for singularity or near-singularity is included in this function.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tbsv(FillMode uplo, Operation trans, DiagType diag, int k, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx)
        {
            _status = CudaBlasNativeMethods.cublasDtbsv_v2(_blasHandle, uplo, trans, diag, x.Size, k, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtbsv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gemv(Operation trans, int m, int n, float alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx, float beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSgemv_v2(_blasHandle, trans, m, n, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gemv(Operation trans, int m, int n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSgemv_v2(_blasHandle, trans, m, n, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gemv(Operation trans, int m, int n, double alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx, double beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDgemv_v2(_blasHandle, trans, m, n, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gemv(Operation trans, int m, int n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDgemv_v2(_blasHandle, trans, m, n, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="kl">number of subdiagonals of matrix A.</param>
        /// <param name="ku">number of superdiagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gbmv(Operation trans, int m, int n, int kl, int ku, float alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx, float beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSgbmv_v2(_blasHandle, trans, m, n, kl, ku, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="kl">number of subdiagonals of matrix A.</param>
        /// <param name="ku">number of superdiagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gbmv(Operation trans, int m, int n, int kl, int ku, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSgbmv_v2(_blasHandle, trans, m, n, kl, ku, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="kl">number of subdiagonals of matrix A.</param>
        /// <param name="ku">number of superdiagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gbmv(Operation trans, int m, int n, int kl, int ku, double alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx, double beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDgbmv_v2(_blasHandle, trans, m, n, kl, ku, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="kl">number of subdiagonals of matrix A.</param>
        /// <param name="ku">number of superdiagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gbmv(Operation trans, int m, int n, int kl, int ku, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDgbmv_v2(_blasHandle, trans, m, n, kl, ku, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in lower or upper mode, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Symv(FillMode uplo, float alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx, float beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSsymv_v2(_blasHandle, uplo, x.Size, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in lower or upper mode, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Symv(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSsymv_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in lower or upper mode, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Symv(FillMode uplo, double alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx, double beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDsymv_v2(_blasHandle, uplo, x.Size, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsymv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-vector multiplication y = alpha *A * x + beta * y where A is a n*n symmetric matrix stored in lower or upper mode, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Symv(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDsymv_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric banded matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix with k subdiagonals and superdiagonals, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Sbmv(FillMode uplo, int k, float alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx, float beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSsbmv_v2(_blasHandle, uplo, x.Size, k, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric banded matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix with k subdiagonals and superdiagonals, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Sbmv(FillMode uplo, int k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSsbmv_v2(_blasHandle, uplo, x.Size, k, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric banded matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix with k subdiagonals and superdiagonals, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Sbmv(FillMode uplo, int k, double alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx, double beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDsbmv_v2(_blasHandle, uplo, x.Size, k, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric banded matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix with k subdiagonals and superdiagonals, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Sbmv(FillMode uplo, int k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDsbmv_v2(_blasHandle, uplo, x.Size, k, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsbmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric packed matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in packed format, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Spmv(FillMode uplo, float alpha, CudaDeviceVariable<float> AP, CudaDeviceVariable<float> x, int incx, float beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSspmv_v2(_blasHandle, uplo, x.Size, ref alpha, AP.DevicePointer, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric packed matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in packed format, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Spmv(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> AP, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasSspmv_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, AP.DevicePointer, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric packed matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in packed format, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Spmv(FillMode uplo, double alpha, CudaDeviceVariable<double> AP, CudaDeviceVariable<double> x, int incx, double beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDspmv_v2(_blasHandle, uplo, x.Size, ref alpha, AP.DevicePointer, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric packed matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in packed format, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Spmv(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> AP, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, int incy)
        {
            _status = CudaBlasNativeMethods.cublasDspmv_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, AP.DevicePointer, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspmv_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the rank-1 update A = alpha * x * y^T + A where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha is a scalar. m = x.Size, n = y.Size.
        /// </summary>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Ger(float alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, CudaDeviceVariable<float> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasSger_v2(_blasHandle, x.Size, y.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSger_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the rank-1 update A = alpha * x * y^T + A where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha is a scalar. m = x.Size, n = y.Size.
        /// </summary>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Ger(CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, CudaDeviceVariable<float> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasSger_v2(_blasHandle, x.Size, y.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSger_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the rank-1 update A = alpha * x * y^T + A where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha is a scalar. m = x.Size, n = y.Size.
        /// </summary>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Ger(double alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, CudaDeviceVariable<double> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasDger_v2(_blasHandle, x.Size, y.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDger_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the rank-1 update A = alpha * x * y^T + A where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha is a scalar. m = x.Size, n = y.Size.
        /// </summary>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Ger(CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, CudaDeviceVariable<double> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasDger_v2(_blasHandle, x.Size, y.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDger_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr(FillMode uplo, float alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasSsyr_v2(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasSsyr_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr(FillMode uplo, double alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasDsyr_v2(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasDsyr_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr(FillMode uplo, float alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasSspr_v2(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspr_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasSspr_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspr_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr(FillMode uplo, double alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> AP)
        {
            _status = CudaBlasNativeMethods.cublasDspr_v2(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspr_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> AP)
        {
            _status = CudaBlasNativeMethods.cublasDspr_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspr_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-2 update A = alpha * (x * y^T + y * y^T) + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr2(FillMode uplo, float alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, CudaDeviceVariable<float> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasSsyr2_v2(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-2 update A = alpha * (x * y^T + y * y^T) + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr2(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, CudaDeviceVariable<float> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasSsyr2_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-2 update A = alpha * (x * y^T + y * y^T) + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr2(FillMode uplo, double alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, CudaDeviceVariable<double> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasDsyr2_v2(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-2 update A = alpha * (x * y^T + y * y^T) + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr2(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, CudaDeviceVariable<double> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasDsyr2_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the packed symmetric rank-2 update A = alpha * (x * y^T + y * x^T) + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr2(FillMode uplo, float alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasSspr2_v2(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspr2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the packed symmetric rank-2 update A = alpha * (x * y^T + y * x^T) + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr2(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, int incx, CudaDeviceVariable<float> y, int incy, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasSspr2_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspr2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the packed symmetric rank-2 update A = alpha * (x * y^T + y * x^T) + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr2(FillMode uplo, double alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, CudaDeviceVariable<double> AP)
        {
            _status = CudaBlasNativeMethods.cublasDspr2_v2(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspr2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the packed symmetric rank-2 update A = alpha * (x * y^T + y * x^T) + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr2(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, int incx, CudaDeviceVariable<double> y, int incy, CudaDeviceVariable<double> AP)
        {
            _status = CudaBlasNativeMethods.cublasDspr2_v2(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspr2_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, int m, int n, int k, float alpha, CudaDeviceVariable<float> A, int lda,
            CudaDeviceVariable<float> B, int ldb, float beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgemm_v2(_blasHandle, transa, transb, m, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda,
            CudaDeviceVariable<float> B, int ldb, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgemm_v2(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, int m, int n, int k, double alpha, CudaDeviceVariable<double> A, int lda,
            CudaDeviceVariable<double> B, int ldb, double beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDgemm_v2(_blasHandle, transa, transb, m, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda,
            CudaDeviceVariable<double> B, int ldb, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDgemm_v2(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, int m, int n, int k, half alpha, CudaDeviceVariable<half> A, int lda,
            CudaDeviceVariable<half> B, int ldb, half beta, CudaDeviceVariable<half> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasHgemm(_blasHandle, transa, transb, m, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasHgemm", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<half> alpha, CudaDeviceVariable<half> A, int lda,
            CudaDeviceVariable<half> B, int ldb, CudaDeviceVariable<half> beta, CudaDeviceVariable<half> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasHgemm(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasHgemm", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="Atype">enumerant specifying the datatype of matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="Btype">enumerant specifying the datatype of matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        /// <param name="Ctype">enumerant specifying the datatype of matrix C.</param>
        public void GemmEx(Operation transa, Operation transb, int m, int n, int k, float alpha, CUdeviceptr A, DataType Atype, int lda,
            CUdeviceptr B, DataType Btype, int ldb, float beta, CUdeviceptr C, DataType Ctype, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgemmEx(_blasHandle, transa, transb, m, n, k, ref alpha, A, Atype, lda, B, Btype, ldb, ref beta, C, Ctype, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="Atype">enumerant specifying the datatype of matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="Btype">enumerant specifying the datatype of matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        /// <param name="Ctype">enumerant specifying the datatype of matrix C.</param>
        public void GemmEx(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<float> alpha, CUdeviceptr A, DataType Atype, int lda,
            CUdeviceptr B, DataType Btype, int ldb, CudaDeviceVariable<float> beta, CUdeviceptr C, DataType Ctype, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgemmEx(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A, Atype, lda, B, Btype, ldb, beta.DevicePointer, C, Ctype, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * Op(A)*Op(A)^T + beta * C where
        /// alpha and beta are scalars, and A, B and C are matrices stored in lower or upper mode, and A is a matrix with dimensions op(A) n*k.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrk(FillMode uplo, Operation trans, int n, int k, float alpha, CudaDeviceVariable<float> A, int lda,
            float beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyrk_v2(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyrk_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * Op(A)*Op(A)^T + beta * C where
        /// alpha and beta are scalars, and A, B and C are matrices stored in lower or upper mode, and A is a matrix with dimensions op(A) n*k.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrk(FillMode uplo, Operation trans, int n, int k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda,
            CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyrk_v2(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyrk_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * Op(A)*Op(A)^T + beta * C where
        /// alpha and beta are scalars, and A, B and C are matrices stored in lower or upper mode, and A is a matrix with dimensions op(A) n*k.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrk(FillMode uplo, Operation trans, int n, int k, double alpha, CudaDeviceVariable<double> A, int lda,
            double beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyrk_v2(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyrk_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * Op(A)*Op(A)^T + beta * C where
        /// alpha and beta are scalars, and A, B and C are matrices stored in lower or upper mode, and A is a matrix with dimensions op(A) n*k.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrk(FillMode uplo, Operation trans, int n, int k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda,
            CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyrk_v2(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyrk_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * (Op(A)*Op(B)^T + Op(B)*Op(A)^T) + beta * C where
        /// alpha and beta are scalars, and C is a symmetrux matrix stored in lower or upper mode, and A and B are matrices with dimensions Op(A) n*k
        /// and Op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * k.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syr2k(FillMode uplo, Operation trans, int n, int k, float alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> B, int ldb,
            float beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyr2k_v2(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr2k_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * (Op(A)*Op(B)^T + Op(B)*Op(A)^T) + beta * C where
        /// alpha and beta are scalars, and C is a symmetrux matrix stored in lower or upper mode, and A and B are matrices with dimensions Op(A) n*k
        /// and Op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * k.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syr2k(FillMode uplo, Operation trans, int n, int k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> B, int ldb,
            CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyr2k_v2(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr2k_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * (Op(A)*Op(B)^T + Op(B)*Op(A)^T) + beta * C where
        /// alpha and beta are scalars, and C is a symmetrux matrix stored in lower or upper mode, and A and B are matrices with dimensions Op(A) n*k
        /// and Op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * k.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syr2k(FillMode uplo, Operation trans, int n, int k, double alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> B, int ldb,
            double beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyr2k_v2(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr2k_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * (Op(A)*Op(B)^T + Op(B)*Op(A)^T) + beta * C where
        /// alpha and beta are scalars, and C is a symmetrux matrix stored in lower or upper mode, and A and B are matrices with dimensions Op(A) n*k
        /// and Op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * k.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syr2k(FillMode uplo, Operation trans, int n, int k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> B, int ldb,
            CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyr2k_v2(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr2k_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs a variation of the symmetric rank- update C = alpha * (Op(A)*Op(B))^T + beta * C where alpha 
        /// and beta are scalars, C is a symmetric matrix stored in lower or upper mode, and A
        /// and B are matrices with dimensions op(A) n*k and op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix C lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or transpose.</param>
        /// <param name="n">number of rows of matrix op(A), op(B) and C.</param>
        /// <param name="k">number of columns of matrix op(A) and op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimension lda x k with lda>=max(1,n) if transa == CUBLAS_OP_N and lda x n with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb x k with ldb>=max(1,n) if transa == CUBLAS_OP_N and ldb x n with ldb>=max(1,k) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0, then C does not have to be a valid input.</param>
        /// <param name="C">array of dimensions ldc x n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrkx(FillMode uplo, Operation trans, int n, int k, ref float alpha, CudaDeviceVariable<float> A, int lda,
                                                    CudaDeviceVariable<float> B, int ldb, ref float beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyrkx(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyrkx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs a variation of the symmetric rank- update C = alpha * (Op(A)*Op(B))^T + beta * C where alpha 
        /// and beta are scalars, C is a symmetric matrix stored in lower or upper mode, and A
        /// and B are matrices with dimensions op(A) n*k and op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix C lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or transpose.</param>
        /// <param name="n">number of rows of matrix op(A), op(B) and C.</param>
        /// <param name="k">number of columns of matrix op(A) and op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimension lda x k with lda>=max(1,n) if transa == CUBLAS_OP_N and lda x n with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb x k with ldb>=max(1,n) if transa == CUBLAS_OP_N and ldb x n with ldb>=max(1,k) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0, then C does not have to be a valid input.</param>
        /// <param name="C">array of dimensions ldc x n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrkx(FillMode uplo, Operation trans, int n, int k, ref double alpha, CudaDeviceVariable<double> A, int lda,
                                                    CudaDeviceVariable<double> B, int ldb, ref double beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyrkx(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyrkx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }



        /// <summary>
        /// This function performs a variation of the symmetric rank- update C = alpha * (Op(A)*Op(B))^T + beta * C where alpha 
        /// and beta are scalars, C is a symmetric matrix stored in lower or upper mode, and A
        /// and B are matrices with dimensions op(A) n*k and op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix C lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or transpose.</param>
        /// <param name="n">number of rows of matrix op(A), op(B) and C.</param>
        /// <param name="k">number of columns of matrix op(A) and op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimension lda x k with lda>=max(1,n) if transa == CUBLAS_OP_N and lda x n with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb x k with ldb>=max(1,n) if transa == CUBLAS_OP_N and ldb x n with ldb>=max(1,k) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0, then C does not have to be a valid input.</param>
        /// <param name="C">array of dimensions ldc x n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrkx(FillMode uplo, Operation trans, int n, int k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda,
                                                    CudaDeviceVariable<float> B, int ldb, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyrkx(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyrkx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs a variation of the symmetric rank- update C = alpha * (Op(A)*Op(B))^T + beta * C where alpha 
        /// and beta are scalars, C is a symmetric matrix stored in lower or upper mode, and A
        /// and B are matrices with dimensions op(A) n*k and op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix C lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or transpose.</param>
        /// <param name="n">number of rows of matrix op(A), op(B) and C.</param>
        /// <param name="k">number of columns of matrix op(A) and op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimension lda x k with lda>=max(1,n) if transa == CUBLAS_OP_N and lda x n with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb x k with ldb>=max(1,n) if transa == CUBLAS_OP_N and ldb x n with ldb>=max(1,k) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0, then C does not have to be a valid input.</param>
        /// <param name="C">array of dimensions ldc x n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrkx(FillMode uplo, Operation trans, int n, int k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda,
                                                    CudaDeviceVariable<double> B, int ldb, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyrkx(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyrkx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric matrix-matrix multiplication C = alpha*A*B + beta*C if side==SideMode.Left or C = alpha*B*A + beta*C if side==SideMode.Right 
        /// where A is a symmetric matrix stored in lower or upper mode, B and C are m*n matrices, and alpha and beta are scalars.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of B.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="m">number of rows of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Symm(SideMode side, FillMode uplo, int m, int n, float alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> B, int ldb,
            float beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsymm_v2(_blasHandle, side, uplo, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-matrix multiplication C = alpha*A*B + beta*C if side==SideMode.Left or C = alpha*B*A + beta*C if side==SideMode.Right 
        /// where A is a symmetric matrix stored in lower or upper mode, B and C are m*n matrices, and alpha and beta are scalars.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of B.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="m">number of rows of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Symm(SideMode side, FillMode uplo, int m, int n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> B, int ldb,
            CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsymm_v2(_blasHandle, side, uplo, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric matrix-matrix multiplication C = alpha*A*B + beta*C if side==SideMode.Left or C = alpha*B*A + beta*C if side==SideMode.Right 
        /// where A is a symmetric matrix stored in lower or upper mode, B and C are m*n matrices, and alpha and beta are scalars.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of B.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="m">number of rows of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Symm(SideMode side, FillMode uplo, int m, int n, double alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> B, int ldb,
            double beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsymm_v2(_blasHandle, side, uplo, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsymm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-matrix multiplication C = alpha*A*B + beta*C if side==SideMode.Left or C = alpha*B*A + beta*C if side==SideMode.Right 
        /// where A is a symmetric matrix stored in lower or upper mode, B and C are m*n matrices, and alpha and beta are scalars.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of B.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="m">number of rows of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Symm(SideMode side, FillMode uplo, int m, int n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> B, int ldb,
            CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsymm_v2(_blasHandle, side, uplo, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsymm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves the triangular linear system with multiple right-hand-sides Op(A)X = alpha*B side==SideMode.Left or XOp(A) = alpha*B if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the maindiagonal, X and B are m*n matrices, and alpha is a scalar.<para/>
        /// The solution X overwrites the right-hand-sides B on exit.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, float alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> B, int ldb)
        {
            _status = CudaBlasNativeMethods.cublasStrsm_v2(_blasHandle, side, uplo, trans, diag, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the triangular linear system with multiple right-hand-sides Op(A)X = alpha*B side==SideMode.Left or XOp(A) = alpha*B if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the maindiagonal, X and B are m*n matrices, and alpha is a scalar.<para/>
        /// The solution X overwrites the right-hand-sides B on exit.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> B, int ldb)
        {
            _status = CudaBlasNativeMethods.cublasStrsm_v2(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves the triangular linear system with multiple right-hand-sides Op(A)X = alpha*B side==SideMode.Left or XOp(A) = alpha*B if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the maindiagonal, X and B are m*n matrices, and alpha is a scalar.<para/>
        /// The solution X overwrites the right-hand-sides B on exit.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, double alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> B, int ldb)
        {
            _status = CudaBlasNativeMethods.cublasDtrsm_v2(_blasHandle, side, uplo, trans, diag, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the triangular linear system with multiple right-hand-sides Op(A)X = alpha*B side==SideMode.Left or XOp(A) = alpha*B if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the maindiagonal, X and B are m*n matrices, and alpha is a scalar.<para/>
        /// The solution X overwrites the right-hand-sides B on exit.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<double> B, int ldb)
        {
            _status = CudaBlasNativeMethods.cublasDtrsm_v2(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the triangular matrix-matrix multiplication C = alpha*Op(A) * B if side==SideMode.Left or C = alpha*B * Op(A) if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the main diagonal, B and C are m*n matrices, and alpha is a scalar.<para/>
        /// Notice that in order to achieve better parallelism CUBLAS differs from the BLAS API only for this routine. The BLAS API assumes an in-place implementation (with results
        /// written back to B), while the CUBLAS API assumes an out-of-place implementation (with results written into C). The application can obtain the in-place functionality of BLAS in
        /// the CUBLAS API by passing the address of the matrix B in place of the matrix C. No other overlapping in the input parameters is supported.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, float alpha, CudaDeviceVariable<float> A, int lda,
            CudaDeviceVariable<float> B, int ldb, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasStrmm_v2(_blasHandle, side, uplo, trans, diag, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrmm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular matrix-matrix multiplication C = alpha*Op(A) * B if side==SideMode.Left or C = alpha*B * Op(A) if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the main diagonal, B and C are m*n matrices, and alpha is a scalar.<para/>
        /// Notice that in order to achieve better parallelism CUBLAS differs from the BLAS API only for this routine. The BLAS API assumes an in-place implementation (with results
        /// written back to B), while the CUBLAS API assumes an out-of-place implementation (with results written into C). The application can obtain the in-place functionality of BLAS in
        /// the CUBLAS API by passing the address of the matrix B in place of the matrix C. No other overlapping in the input parameters is supported.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda,
            CudaDeviceVariable<float> B, int ldb, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasStrmm_v2(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrmm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the triangular matrix-matrix multiplication C = alpha*Op(A) * B if side==SideMode.Left or C = alpha*B * Op(A) if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the main diagonal, B and C are m*n matrices, and alpha is a scalar.<para/>
        /// Notice that in order to achieve better parallelism CUBLAS differs from the BLAS API only for this routine. The BLAS API assumes an in-place implementation (with results
        /// written back to B), while the CUBLAS API assumes an out-of-place implementation (with results written into C). The application can obtain the in-place functionality of BLAS in
        /// the CUBLAS API by passing the address of the matrix B in place of the matrix C. No other overlapping in the input parameters is supported.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, double alpha, CudaDeviceVariable<double> A, int lda,
            CudaDeviceVariable<double> B, int ldb, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDtrmm_v2(_blasHandle, side, uplo, trans, diag, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrmm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular matrix-matrix multiplication C = alpha*Op(A) * B if side==SideMode.Left or C = alpha*B * Op(A) if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the main diagonal, B and C are m*n matrices, and alpha is a scalar.<para/>
        /// Notice that in order to achieve better parallelism CUBLAS differs from the BLAS API only for this routine. The BLAS API assumes an in-place implementation (with results
        /// written back to B), while the CUBLAS API assumes an out-of-place implementation (with results written into C). The application can obtain the in-place functionality of BLAS in
        /// the CUBLAS API by passing the address of the matrix B in place of the matrix C. No other overlapping in the input parameters is supported.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda,
            CudaDeviceVariable<double> B, int ldb, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDtrmm_v2(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrmm_v2", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix addition/transposition C = alpha * Op(A) + beta * Op(B) where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*n, op(B) m*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Geam(Operation transa, Operation transb, int m, int n, float alpha, CudaDeviceVariable<float> A, int lda,
            CudaDeviceVariable<float> B, int ldb, float beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgeam(_blasHandle, transa, transb, m, n, ref alpha, A.DevicePointer, lda, ref beta,
                B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgeam", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix addition/transposition C = alpha * Op(A) + beta * Op(B) where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*n, op(B) m*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Geam(Operation transa, Operation transb, int m, int n, double alpha, CudaDeviceVariable<double> A, int lda,
            CudaDeviceVariable<double> B, int ldb, double beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDgeam(_blasHandle, transa, transb, m, n, ref alpha, A.DevicePointer, lda, ref beta,
                B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgeam", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix addition/transposition C = alpha * Op(A) + beta * Op(B) where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*n, op(B) m*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Geam(Operation transa, Operation transb, int m, int n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, int lda,
            CudaDeviceVariable<float> B, int ldb, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgeam(_blasHandle, transa, transb, m, n, alpha.DevicePointer, A.DevicePointer, lda, beta.DevicePointer,
                B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgeam", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix addition/transposition C = alpha * Op(A) + beta * Op(B) where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*n, op(B) m*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Geam(Operation transa, Operation transb, int m, int n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, int lda,
            CudaDeviceVariable<double> B, int ldb, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDgeam(_blasHandle, transa, transb, m, n, alpha.DevicePointer, A.DevicePointer, lda, beta.DevicePointer,
                B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgeam", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="A"></param>
        /// <param name="lda"></param>
        /// <param name="Ainv"></param>
        /// <param name="lda_inv"></param>
        /// <param name="INFO"></param>
        /// <param name="batchSize"></param>
        public void MatinvBatchedS(int n, CudaDeviceVariable<CUdeviceptr> A, int lda,
                                                           CudaDeviceVariable<CUdeviceptr> Ainv, int lda_inv, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasSmatinvBatched(_blasHandle, n, A.DevicePointer, lda, Ainv.DevicePointer, lda_inv, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSmatinvBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="A"></param>
        /// <param name="lda"></param>
        /// <param name="Ainv"></param>
        /// <param name="lda_inv"></param>
        /// <param name="INFO"></param>
        /// <param name="batchSize"></param>
        public void MatinvBatchedD(int n, CudaDeviceVariable<CUdeviceptr> A, int lda,
                                                           CudaDeviceVariable<CUdeviceptr> Ainv, int lda_inv, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasDmatinvBatched(_blasHandle, n, A.DevicePointer, lda, Ainv.DevicePointer, lda_inv, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDmatinvBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="A"></param>
        /// <param name="lda"></param>
        /// <param name="Ainv"></param>
        /// <param name="lda_inv"></param>
        /// <param name="INFO"></param>
        /// <param name="batchSize"></param>
        public void MatinvBatchedC(int n, CudaDeviceVariable<CUdeviceptr> A, int lda,
                                                           CudaDeviceVariable<CUdeviceptr> Ainv, int lda_inv, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasCmatinvBatched(_blasHandle, n, A.DevicePointer, lda, Ainv.DevicePointer, lda_inv, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasCmatinvBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="A"></param>
        /// <param name="lda"></param>
        /// <param name="Ainv"></param>
        /// <param name="lda_inv"></param>
        /// <param name="INFO"></param>
        /// <param name="batchSize"></param>
        public void MatinvBatchedZ(int n, CudaDeviceVariable<CUdeviceptr> A, int lda,
                                                           CudaDeviceVariable<CUdeviceptr> Ainv, int lda_inv, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasZmatinvBatched(_blasHandle, n, A.DevicePointer, lda, Ainv.DevicePointer, lda_inv, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasZmatinvBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication C = A x diag(X) if mode == CUBLAS_SIDE_RIGHT, or 
        /// C = diag(X) x A if mode == CUBLAS_SIDE_LEFT.<para/>
        /// where A and C are matrices stored in column-major format with dimensions m*n. X is a
        /// vector of size n if mode == CUBLAS_SIDE_RIGHT and of size m if mode ==
        /// CUBLAS_SIDE_LEFT. X is gathered from one-dimensional array x with stride incx. The
        /// absolute value of incx is the stride and the sign of incx is direction of the stride. If incx
        /// is positive, then we forward x from the first element. Otherwise, we backward x from the
        /// last element.
        /// </summary>
        /// <param name="mode">left multiply if mode == CUBLAS_SIDE_LEFT 
        /// or right multiply if mode == CUBLAS_SIDE_RIGHT</param>
        /// <param name="m">number of rows of matrix A and C.</param>
        /// <param name="n">number of columns of matrix A and C.</param>
        /// <param name="A">array of dimensions lda x n with lda >= max(1,m)</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store the matrix A.</param>
        /// <param name="X">one-dimensional array of size |incx|*m 
        /// if mode == CUBLAS_SIDE_LEFT and |incx|*n
        /// if mode == CUBLAS_SIDE_RIGHT</param>
        /// <param name="incx">stride of one-dimensional array x.</param>
        /// <param name="C">array of dimensions ldc*n with ldc >= max(1,m).</param>
        /// <param name="ldc">leading dimension of a two-dimensional array used to store the matrix C.</param>
        public void Dgmm(SideMode mode, int m, int n, CudaDeviceVariable<float> A, int lda,
            CudaDeviceVariable<float> X, int incx, CudaDeviceVariable<float> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasSdgmm(_blasHandle, mode, m, n, A.DevicePointer, lda,
                X.DevicePointer, incx, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSdgmm", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = A x diag(X) if mode == CUBLAS_SIDE_RIGHT, or 
        /// C = diag(X) x A if mode == CUBLAS_SIDE_LEFT.<para/>
        /// where A and C are matrices stored in column-major format with dimensions m*n. X is a
        /// vector of size n if mode == CUBLAS_SIDE_RIGHT and of size m if mode ==
        /// CUBLAS_SIDE_LEFT. X is gathered from one-dimensional array x with stride incx. The
        /// absolute value of incx is the stride and the sign of incx is direction of the stride. If incx
        /// is positive, then we forward x from the first element. Otherwise, we backward x from the
        /// last element.
        /// </summary>
        /// <param name="mode">left multiply if mode == CUBLAS_SIDE_LEFT 
        /// or right multiply if mode == CUBLAS_SIDE_RIGHT</param>
        /// <param name="m">number of rows of matrix A and C.</param>
        /// <param name="n">number of columns of matrix A and C.</param>
        /// <param name="A">array of dimensions lda x n with lda >= max(1,m)</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store the matrix A.</param>
        /// <param name="X">one-dimensional array of size |incx|*m 
        /// if mode == CUBLAS_SIDE_LEFT and |incx|*n
        /// if mode == CUBLAS_SIDE_RIGHT</param>
        /// <param name="incx">stride of one-dimensional array x.</param>
        /// <param name="C">array of dimensions ldc*n with ldc >= max(1,m).</param>
        /// <param name="ldc">leading dimension of a two-dimensional array used to store the matrix C.</param>
        public void Dgmm(SideMode mode, int m, int n, CudaDeviceVariable<double> A, int lda,
            CudaDeviceVariable<double> X, int incx, CudaDeviceVariable<double> C, int ldc)
        {
            _status = CudaBlasNativeMethods.cublasDdgmm(_blasHandle, mode, m, n, A.DevicePointer, lda,
                X.DevicePointer, incx, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDdgmm", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<half> alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> Barray, int ldb,
                                   CudaDeviceVariable<half> beta, CudaDeviceVariable<CUdeviceptr> Carray, int ldc, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasHgemmBatched(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, beta.DevicePointer, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasHgemmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<float> alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> Barray, int ldb,
                                   CudaDeviceVariable<float> beta, CudaDeviceVariable<CUdeviceptr> Carray, int ldc, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasSgemmBatched(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, beta.DevicePointer, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        /// <param name="algo"></param>
        /// <param name="Atype"></param>
        /// <param name="Btype"></param>
        /// <param name="computeType"></param>
        /// <param name="Ctype"></param>
        public void GemmBatched(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<float> alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, cudaDataType Atype, int lda, CudaDeviceVariable<CUdeviceptr> Barray, cudaDataType Btype, int ldb,
                                   CudaDeviceVariable<float> beta, CudaDeviceVariable<CUdeviceptr> Carray, cudaDataType Ctype, int ldc, int batchCount, ComputeType computeType, GemmAlgo algo)
        {
            _status = CudaBlasNativeMethods.cublasGemmBatchedEx(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, Aarray.DevicePointer, Atype, lda, Barray.DevicePointer, Btype, ldb, beta.DevicePointer, Carray.DevicePointer, Ctype, ldc, batchCount, computeType, algo);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGemmBatchedEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }



        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to <![CDATA[<Atype>]]> matrix, A, corresponds to the first instance of the batch of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="strideA">value of type long long int that gives the address offset between A[i] and A[i+1]. </param>
        /// <param name="strideB">value of type long long int that gives the address offset between B[i] and B[i+1]. </param>
        /// <param name="strideC">value of type long long int that gives the address offset between C[i] and C[i+1]. </param>
        /// <param name="B">pointer to <![CDATA[<Btype>]]> matrix, A, corresponds to the first instance of the batch of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to <![CDATA[<Ctype>]]> matrix, A, corresponds to the first instance of the batch. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        /// <param name="algo"></param>
        /// <param name="Atype"></param>
        /// <param name="Btype"></param>
        /// <param name="computeType"></param>
        /// <param name="Ctype"></param>
        public void GemmStridedBatched(Operation transa, Operation transb, int m, int n, int k, CUdeviceptr alpha,
                                   CUdeviceptr A, cudaDataType Atype, int lda, long strideA, CUdeviceptr B, cudaDataType Btype, int ldb, long strideB,
                                   CUdeviceptr beta, CUdeviceptr C, cudaDataType Ctype, int ldc, long strideC, int batchCount, ComputeType computeType, GemmAlgo algo)
        {
            _status = CudaBlasNativeMethods.cublasGemmStridedBatchedEx(_blasHandle, transa, transb, m, n, k, alpha, A,
                Atype, lda, strideA, B, Btype, ldb, strideB, beta, C, Ctype, ldc, strideC, batchCount, computeType, algo);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGemmStridedBatchedEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<double> alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> Barray, int ldb, CudaDeviceVariable<double> beta,
                                   CudaDeviceVariable<CUdeviceptr> Carray, int ldc, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDgemmBatched(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, beta.DevicePointer, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function performs the matrix-matrix multiplication of a batch of matrices. The batch is considered to be "uniform", 
        /// i.e. all instances have the same dimensions (m, n, k), leading dimensions (lda, ldb, ldc) and transpositions (transa, transb) 
        /// for their respective A, B and C matrices. Input matrices A, B and output matrix C for each instance of the batch are located 
        /// at fixed address offsets from their locations in the previous instance. Pointers to A, B and C matrices for the first 
        /// instance are passed to the function by the user along with the address offsets - strideA, strideB and strideC that determine 
        /// the locations of input and output matrices in future instances. 
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose. </param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose. </param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i]. </param>
        /// <param name="n">number of columns of op(B[i]) and C[i]. </param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to the A matrix corresponding to the first instance of the batch, with dimensions lda x k with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise. </param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i]. </param>
        /// <param name="strideA">Value of type long long int that gives the address offset between A[i] and A[i+1]</param>
        /// <param name="B">pointer to the B matrix corresponding to the first instance of the batch, with dimensions ldb x n with ldb>=max(1,k) if transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise. </param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i]. </param>
        /// <param name="strideB">Value of type long long int that gives the address offset between B[i] and B[i+1]</param>
        /// <param name="beta"> scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to the C matrix corresponding to the first instance of the batch, with dimensions ldc x n with ldc>=max(1,m). </param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i]. </param>
        /// <param name="strideC">Value of type long long int that gives the address offset between C[i] and C[i+1]</param>
        /// <param name="batchCount">number of GEMMs to perform in the batch.</param>
        public void GemmStridedBatched(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<float> alpha,
            CudaDeviceVariable<float> A, int lda, long strideA, CudaDeviceVariable<float> B, int ldb, long strideB,
            CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc, long strideC, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasSgemmStridedBatched(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, strideA,
                B.DevicePointer, ldb, strideB, beta.DevicePointer, C.DevicePointer, ldc, strideC, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmStridedBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication of a batch of matrices. The batch is considered to be "uniform", 
        /// i.e. all instances have the same dimensions (m, n, k), leading dimensions (lda, ldb, ldc) and transpositions (transa, transb) 
        /// for their respective A, B and C matrices. Input matrices A, B and output matrix C for each instance of the batch are located 
        /// at fixed address offsets from their locations in the previous instance. Pointers to A, B and C matrices for the first 
        /// instance are passed to the function by the user along with the address offsets - strideA, strideB and strideC that determine 
        /// the locations of input and output matrices in future instances. 
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose. </param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose. </param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i]. </param>
        /// <param name="n">number of columns of op(B[i]) and C[i]. </param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to the A matrix corresponding to the first instance of the batch, with dimensions lda x k with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise. </param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i]. </param>
        /// <param name="strideA">Value of type long long int that gives the address offset between A[i] and A[i+1]</param>
        /// <param name="B">pointer to the B matrix corresponding to the first instance of the batch, with dimensions ldb x n with ldb>=max(1,k) if transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise. </param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i]. </param>
        /// <param name="strideB">Value of type long long int that gives the address offset between B[i] and B[i+1]</param>
        /// <param name="beta"> scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to the C matrix corresponding to the first instance of the batch, with dimensions ldc x n with ldc>=max(1,m). </param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i]. </param>
        /// <param name="strideC">Value of type long long int that gives the address offset between C[i] and C[i+1]</param>
        /// <param name="batchCount">number of GEMMs to perform in the batch.</param>
        public void GemmStridedBatched(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<double> alpha,
            CudaDeviceVariable<double> A, int lda, long strideA, CudaDeviceVariable<double> B, int ldb, long strideB,
            CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc, long strideC, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDgemmStridedBatched(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, strideA,
                B.DevicePointer, ldb, strideB, beta.DevicePointer, C.DevicePointer, ldc, strideC, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemmStridedBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication of a batch of matrices. The batch is considered to be "uniform", 
        /// i.e. all instances have the same dimensions (m, n, k), leading dimensions (lda, ldb, ldc) and transpositions (transa, transb) 
        /// for their respective A, B and C matrices. Input matrices A, B and output matrix C for each instance of the batch are located 
        /// at fixed address offsets from their locations in the previous instance. Pointers to A, B and C matrices for the first 
        /// instance are passed to the function by the user along with the address offsets - strideA, strideB and strideC that determine 
        /// the locations of input and output matrices in future instances. 
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose. </param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose. </param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i]. </param>
        /// <param name="n">number of columns of op(B[i]) and C[i]. </param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to the A matrix corresponding to the first instance of the batch, with dimensions lda x k with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise. </param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i]. </param>
        /// <param name="strideA">Value of type long long int that gives the address offset between A[i] and A[i+1]</param>
        /// <param name="B">pointer to the B matrix corresponding to the first instance of the batch, with dimensions ldb x n with ldb>=max(1,k) if transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise. </param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i]. </param>
        /// <param name="strideB">Value of type long long int that gives the address offset between B[i] and B[i+1]</param>
        /// <param name="beta"> scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to the C matrix corresponding to the first instance of the batch, with dimensions ldc x n with ldc>=max(1,m). </param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i]. </param>
        /// <param name="strideC">Value of type long long int that gives the address offset between C[i] and C[i+1]</param>
        /// <param name="batchCount">number of GEMMs to perform in the batch.</param>
        public void GemmStridedBatched(Operation transa, Operation transb, int m, int n, int k, CudaDeviceVariable<half> alpha,
            CudaDeviceVariable<half> A, int lda, long strideA, CudaDeviceVariable<half> B, int ldb, long strideB,
            CudaDeviceVariable<half> beta, CudaDeviceVariable<half> C, int ldc, long strideC, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasHgemmStridedBatched(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, strideA,
                B.DevicePointer, ldb, strideB, beta.DevicePointer, C.DevicePointer, ldc, strideC, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmHtridedBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, int m, int n, int k, half alpha, CudaDeviceVariable<CUdeviceptr> Aarray,
            int lda, CudaDeviceVariable<CUdeviceptr> Barray, int ldb, half beta, CudaDeviceVariable<CUdeviceptr> Carray, int ldc, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasHgemmBatched(_blasHandle, transa, transb, m, n, k, ref alpha, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, ref beta, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasHgemmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        /// <param name="algo"></param>
        /// <param name="Atype"></param>
        /// <param name="Btype"></param>
        /// <param name="computeType"></param>
        /// <param name="Ctype"></param>
        public void GemmBatched(Operation transa, Operation transb, int m, int n, int k, IntPtr alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, cudaDataType Atype, int lda, CudaDeviceVariable<CUdeviceptr> Barray, cudaDataType Btype, int ldb,
                                   IntPtr beta, CudaDeviceVariable<CUdeviceptr> Carray, cudaDataType Ctype, int ldc, int batchCount, cudaDataType computeType, GemmAlgo algo)
        {
            _status = CudaBlasNativeMethods.cublasGemmBatchedEx(_blasHandle, transa, transb, m, n, k, alpha, Aarray.DevicePointer, Atype, lda, Barray.DevicePointer, Btype, ldb, beta, Carray.DevicePointer, Ctype, ldc, batchCount, computeType, algo);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGemmBatchedEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to <![CDATA[<Atype>]]> matrix, A, corresponds to the first instance of the batch of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="strideA">value of type long long int that gives the address offset between A[i] and A[i+1]. </param>
        /// <param name="strideB">value of type long long int that gives the address offset between B[i] and B[i+1]. </param>
        /// <param name="strideC">value of type long long int that gives the address offset between C[i] and C[i+1]. </param>
        /// <param name="B">pointer to <![CDATA[<Btype>]]> matrix, A, corresponds to the first instance of the batch of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to <![CDATA[<Ctype>]]> matrix, A, corresponds to the first instance of the batch. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        /// <param name="algo"></param>
        /// <param name="Atype"></param>
        /// <param name="Btype"></param>
        /// <param name="computeType"></param>
        /// <param name="Ctype"></param>
        public void GemmStridedBatched(Operation transa, Operation transb, int m, int n, int k, IntPtr alpha,
                                   CUdeviceptr A, cudaDataType Atype, int lda, long strideA, CUdeviceptr B, cudaDataType Btype, int ldb, long strideB,
                                   IntPtr beta, CUdeviceptr C, cudaDataType Ctype, int ldc, long strideC, int batchCount, cudaDataType computeType, GemmAlgo algo)
        {
            _status = CudaBlasNativeMethods.cublasGemmStridedBatchedEx(_blasHandle, transa, transb, m, n, k, alpha, A,
                Atype, lda, strideA, B, Btype, ldb, strideB, beta, C, Ctype, ldc, strideC, batchCount, computeType, algo);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGemmBatchedEx", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, int m, int n, int k, float alpha, CudaDeviceVariable<CUdeviceptr> Aarray,
            int lda, CudaDeviceVariable<CUdeviceptr> Barray, int ldb, float beta, CudaDeviceVariable<CUdeviceptr> Carray, int ldc, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasSgemmBatched(_blasHandle, transa, transb, m, n, k, ref alpha, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, ref beta, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, int m, int n, int k, double alpha,
            CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> Barray, int ldb, double beta,
            CudaDeviceVariable<CUdeviceptr> Carray, int ldc, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDgemmBatched(_blasHandle, transa, transb, m, n, k, ref alpha, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, ref beta, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function performs the matrix-matrix multiplication of a batch of matrices. The batch is considered to be "uniform", 
        /// i.e. all instances have the same dimensions (m, n, k), leading dimensions (lda, ldb, ldc) and transpositions (transa, transb) 
        /// for their respective A, B and C matrices. Input matrices A, B and output matrix C for each instance of the batch are located 
        /// at fixed address offsets from their locations in the previous instance. Pointers to A, B and C matrices for the first 
        /// instance are passed to the function by the user along with the address offsets - strideA, strideB and strideC that determine 
        /// the locations of input and output matrices in future instances. 
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose. </param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose. </param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i]. </param>
        /// <param name="n">number of columns of op(B[i]) and C[i]. </param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to the A matrix corresponding to the first instance of the batch, with dimensions lda x k with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise. </param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i]. </param>
        /// <param name="strideA">Value of type long long int that gives the address offset between A[i] and A[i+1]</param>
        /// <param name="B">pointer to the B matrix corresponding to the first instance of the batch, with dimensions ldb x n with ldb>=max(1,k) if transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise. </param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i]. </param>
        /// <param name="strideB">Value of type long long int that gives the address offset between B[i] and B[i+1]</param>
        /// <param name="beta"> scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to the C matrix corresponding to the first instance of the batch, with dimensions ldc x n with ldc>=max(1,m). </param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i]. </param>
        /// <param name="strideC">Value of type long long int that gives the address offset between C[i] and C[i+1]</param>
        /// <param name="batchCount">number of GEMMs to perform in the batch.</param>
        public void GemmStridedBatched(Operation transa, Operation transb, int m, int n, int k, float alpha,
            CudaDeviceVariable<float> A, int lda, long strideA, CudaDeviceVariable<float> B, int ldb, long strideB,
            float beta, CudaDeviceVariable<float> C, int ldc, long strideC, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasSgemmStridedBatched(_blasHandle, transa, transb, m, n, k, ref alpha, A.DevicePointer, lda, strideA,
                B.DevicePointer, ldb, strideB, ref beta, C.DevicePointer, ldc, strideC, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmStridedBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication of a batch of matrices. The batch is considered to be "uniform", 
        /// i.e. all instances have the same dimensions (m, n, k), leading dimensions (lda, ldb, ldc) and transpositions (transa, transb) 
        /// for their respective A, B and C matrices. Input matrices A, B and output matrix C for each instance of the batch are located 
        /// at fixed address offsets from their locations in the previous instance. Pointers to A, B and C matrices for the first 
        /// instance are passed to the function by the user along with the address offsets - strideA, strideB and strideC that determine 
        /// the locations of input and output matrices in future instances. 
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose. </param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose. </param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i]. </param>
        /// <param name="n">number of columns of op(B[i]) and C[i]. </param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to the A matrix corresponding to the first instance of the batch, with dimensions lda x k with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise. </param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i]. </param>
        /// <param name="strideA">Value of type long long int that gives the address offset between A[i] and A[i+1]</param>
        /// <param name="B">pointer to the B matrix corresponding to the first instance of the batch, with dimensions ldb x n with ldb>=max(1,k) if transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise. </param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i]. </param>
        /// <param name="strideB">Value of type long long int that gives the address offset between B[i] and B[i+1]</param>
        /// <param name="beta"> scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to the C matrix corresponding to the first instance of the batch, with dimensions ldc x n with ldc>=max(1,m). </param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i]. </param>
        /// <param name="strideC">Value of type long long int that gives the address offset between C[i] and C[i+1]</param>
        /// <param name="batchCount">number of GEMMs to perform in the batch.</param>
        public void GemmStridedBatched(Operation transa, Operation transb, int m, int n, int k, double alpha,
            CudaDeviceVariable<double> A, int lda, long strideA, CudaDeviceVariable<double> B, int ldb, long strideB,
            double beta, CudaDeviceVariable<double> C, int ldc, long strideC, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDgemmStridedBatched(_blasHandle, transa, transb, m, n, k, ref alpha, A.DevicePointer, lda, strideA,
                B.DevicePointer, ldb, strideB, ref beta, C.DevicePointer, ldc, strideC, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemmStridedBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication of a batch of matrices. The batch is considered to be "uniform", 
        /// i.e. all instances have the same dimensions (m, n, k), leading dimensions (lda, ldb, ldc) and transpositions (transa, transb) 
        /// for their respective A, B and C matrices. Input matrices A, B and output matrix C for each instance of the batch are located 
        /// at fixed address offsets from their locations in the previous instance. Pointers to A, B and C matrices for the first 
        /// instance are passed to the function by the user along with the address offsets - strideA, strideB and strideC that determine 
        /// the locations of input and output matrices in future instances. 
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose. </param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose. </param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i]. </param>
        /// <param name="n">number of columns of op(B[i]) and C[i]. </param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to the A matrix corresponding to the first instance of the batch, with dimensions lda x k with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise. </param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i]. </param>
        /// <param name="strideA">Value of type long long int that gives the address offset between A[i] and A[i+1]</param>
        /// <param name="B">pointer to the B matrix corresponding to the first instance of the batch, with dimensions ldb x n with ldb>=max(1,k) if transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise. </param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i]. </param>
        /// <param name="strideB">Value of type long long int that gives the address offset between B[i] and B[i+1]</param>
        /// <param name="beta"> scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to the C matrix corresponding to the first instance of the batch, with dimensions ldc x n with ldc>=max(1,m). </param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i]. </param>
        /// <param name="strideC">Value of type long long int that gives the address offset between C[i] and C[i+1]</param>
        /// <param name="batchCount">number of GEMMs to perform in the batch.</param>
        public void GemmStridedBatched(Operation transa, Operation transb, int m, int n, int k, half alpha,
            CudaDeviceVariable<half> A, int lda, long strideA, CudaDeviceVariable<half> B, int ldb, long strideB,
            half beta, CudaDeviceVariable<half> C, int ldc, long strideC, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasHgemmStridedBatched(_blasHandle, transa, transb, m, n, k, ref alpha, A.DevicePointer, lda, strideA,
                B.DevicePointer, ldb, strideB, ref beta, C.DevicePointer, ldc, strideC, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmHtridedBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the LU factorization of an array of n x n matrices.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimension n to 32.
        /// </summary>
        /// <param name="n">number of rows and columns of A[i].</param>
        /// <param name="A">array of device pointers with each array/device pointer of dim. n x n with lda>=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="P">array of size n x batchSize that contains the permutation vector 
        /// of each factorization of A[i] stored in a linear fashion.</param>
        /// <param name="INFO">If info=0, the execution is successful.<para/>
        /// If info = -i, the i-th parameter had an illegal value.<para/>
        /// If info = i, aii is 0. The factorization has been completed, but U is exactly singular.</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        public void GetrfBatchedS(int n, CudaDeviceVariable<CUdeviceptr> A, int lda, CudaDeviceVariable<int> P,
            CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasSgetrfBatched(_blasHandle, n, A.DevicePointer, lda, P.DevicePointer, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgetrfBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the LU factorization of an array of n x n matrices.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimension n to 32.
        /// </summary>
        /// <param name="n">number of rows and columns of A[i].</param>
        /// <param name="A">array of device pointers with each array/device pointer of dim. n x n with lda>=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="P">array of size n x batchSize that contains the permutation vector 
        /// of each factorization of A[i] stored in a linear fashion.</param>
        /// <param name="INFO">If info=0, the execution is successful.<para/>
        /// If info = -i, the i-th parameter had an illegal value.<para/>
        /// If info = i, aii is 0. The factorization has been completed, but U is exactly singular.</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        public void GetrfBatchedD(int n, CudaDeviceVariable<CUdeviceptr> A, int lda, CudaDeviceVariable<int> P,
            CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasDgetrfBatched(_blasHandle, n, A.DevicePointer, lda, P.DevicePointer, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgetrfBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the LU factorization of an array of n x n matrices.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimension n to 32.
        /// </summary>
        /// <param name="n">number of rows and columns of A[i].</param>
        /// <param name="A">array of device pointers with each array/device pointer of dim. n x n with lda>=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="P">array of size n x batchSize that contains the permutation vector 
        /// of each factorization of A[i] stored in a linear fashion.</param>
        /// <param name="INFO">If info=0, the execution is successful.<para/>
        /// If info = -i, the i-th parameter had an illegal value.<para/>
        /// If info = i, aii is 0. The factorization has been completed, but U is exactly singular.</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        public void GetrfBatchedC(int n, CudaDeviceVariable<CUdeviceptr> A, int lda,
            CudaDeviceVariable<int> P, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasCgetrfBatched(_blasHandle, n, A.DevicePointer, lda, P.DevicePointer, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasCgetrfBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the LU factorization of an array of n x n matrices.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimension n to 32.
        /// </summary>
        /// <param name="n">number of rows and columns of A[i].</param>
        /// <param name="A">array of device pointers with each array/device pointer of dim. n x n with lda>=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="P">array of size n x batchSize that contains the permutation vector 
        /// of each factorization of A[i] stored in a linear fashion.</param>
        /// <param name="INFO">If info=0, the execution is successful.<para/>
        /// If info = -i, the i-th parameter had an illegal value.<para/>
        /// If info = i, aii is 0. The factorization has been completed, but U is exactly singular.</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        public void GetrfBatchedZ(int n, CudaDeviceVariable<CUdeviceptr> A, int lda,
            CudaDeviceVariable<int> P, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasZgetrfBatched(_blasHandle, n, A.DevicePointer, lda, P.DevicePointer, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasZgetrfBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// Aarray and Carray are arrays of pointers to matrices stored in column-major format
        /// with dimensions n*n and leading dimension lda and ldc respectively.
        /// This function performs the inversion of matrices A[i] for i = 0, ..., batchSize-1.<para/>
        /// Prior to calling GetriBatched, the matrix A[i] must be factorized first using
        /// the routine GetrfBatched. After the call of GetrfBatched, the matrix
        /// pointing by Aarray[i] will contain the LU factors of the matrix A[i] and the vector
        /// pointing by (PivotArray+i) will contain the pivoting sequence.<para/>
        /// Following the LU factorization, GetriBatched uses forward and backward
        /// triangular solvers to complete inversion of matrices A[i] for i = 0, ..., batchSize-1. The
        /// inversion is out-of-place, so memory space of Carray[i] cannot overlap memory space of
        /// Array[i].
        /// </summary>
        /// <param name="n">number of rows and columns of Aarray[i].</param>
        /// <param name="Aarray">array of pointers to array, with each array of dimension n*n with lda>=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i].</param>
        /// <param name="P">array of size n*batchSize that contains the pivoting sequence of each factorization of Aarray[i] stored in a linear fashion.</param>
        /// <param name="Carray">array of pointers to array, with each array of dimension n*n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix Carray[i].</param>
        /// <param name="INFO">array of size batchSize that info(=infoArray[i]) contains the information of inversion of A[i].<para/>
        /// If info=0, the execution is successful.<para/>
        /// If info = k, U(k,k) is 0. The U is exactly singular and the inversion failed.</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        public void GetriBatchedS(int n, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<int> P,
            CudaDeviceVariable<CUdeviceptr> Carray, int ldc, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasSgetriBatched(_blasHandle, n, Aarray.DevicePointer, lda, P.DevicePointer, Carray.DevicePointer, ldc, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgetriBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// Aarray and Carray are arrays of pointers to matrices stored in column-major format
        /// with dimensions n*n and leading dimension lda and ldc respectively.
        /// This function performs the inversion of matrices A[i] for i = 0, ..., batchSize-1.<para/>
        /// Prior to calling GetriBatched, the matrix A[i] must be factorized first using
        /// the routine GetrfBatched. After the call of GetrfBatched, the matrix
        /// pointing by Aarray[i] will contain the LU factors of the matrix A[i] and the vector
        /// pointing by (PivotArray+i) will contain the pivoting sequence.<para/>
        /// Following the LU factorization, GetriBatched uses forward and backward
        /// triangular solvers to complete inversion of matrices A[i] for i = 0, ..., batchSize-1. The
        /// inversion is out-of-place, so memory space of Carray[i] cannot overlap memory space of
        /// Array[i].
        /// </summary>
        /// <param name="n">number of rows and columns of Aarray[i].</param>
        /// <param name="Aarray">array of pointers to array, with each array of dimension n*n with lda>=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i].</param>
        /// <param name="P">array of size n*batchSize that contains the pivoting sequence of each factorization of Aarray[i] stored in a linear fashion.</param>
        /// <param name="Carray">array of pointers to array, with each array of dimension n*n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix Carray[i].</param>
        /// <param name="INFO">array of size batchSize that info(=infoArray[i]) contains the information of inversion of A[i].<para/>
        /// If info=0, the execution is successful.<para/>
        /// If info = k, U(k,k) is 0. The U is exactly singular and the inversion failed.</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        public void GetriBatchedD(int n, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<int> P,
            CudaDeviceVariable<CUdeviceptr> Carray, int ldc, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasDgetriBatched(_blasHandle, n, Aarray.DevicePointer, lda, P.DevicePointer, Carray.DevicePointer, ldc, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgetriBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// Aarray and Carray are arrays of pointers to matrices stored in column-major format
        /// with dimensions n*n and leading dimension lda and ldc respectively.
        /// This function performs the inversion of matrices A[i] for i = 0, ..., batchSize-1.<para/>
        /// Prior to calling GetriBatched, the matrix A[i] must be factorized first using
        /// the routine GetrfBatched. After the call of GetrfBatched, the matrix
        /// pointing by Aarray[i] will contain the LU factors of the matrix A[i] and the vector
        /// pointing by (PivotArray+i) will contain the pivoting sequence.<para/>
        /// Following the LU factorization, GetriBatched uses forward and backward
        /// triangular solvers to complete inversion of matrices A[i] for i = 0, ..., batchSize-1. The
        /// inversion is out-of-place, so memory space of Carray[i] cannot overlap memory space of
        /// Array[i].
        /// </summary>
        /// <param name="n">number of rows and columns of Aarray[i].</param>
        /// <param name="Aarray">array of pointers to array, with each array of dimension n*n with lda>=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i].</param>
        /// <param name="P">array of size n*batchSize that contains the pivoting sequence of each factorization of Aarray[i] stored in a linear fashion.</param>
        /// <param name="Carray">array of pointers to array, with each array of dimension n*n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix Carray[i].</param>
        /// <param name="INFO">array of size batchSize that info(=infoArray[i]) contains the information of inversion of A[i].<para/>
        /// If info=0, the execution is successful.<para/>
        /// If info = k, U(k,k) is 0. The U is exactly singular and the inversion failed.</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        public void GetriBatchedC(int n, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<int> P,
            CudaDeviceVariable<CUdeviceptr> Carray, int ldc, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasCgetriBatched(_blasHandle, n, Aarray.DevicePointer, lda, P.DevicePointer, Carray.DevicePointer, ldc, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasCgetriBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// Aarray and Carray are arrays of pointers to matrices stored in column-major format
        /// with dimensions n*n and leading dimension lda and ldc respectively.
        /// This function performs the inversion of matrices A[i] for i = 0, ..., batchSize-1.<para/>
        /// Prior to calling GetriBatched, the matrix A[i] must be factorized first using
        /// the routine GetrfBatched. After the call of GetrfBatched, the matrix
        /// pointing by Aarray[i] will contain the LU factors of the matrix A[i] and the vector
        /// pointing by (PivotArray+i) will contain the pivoting sequence.<para/>
        /// Following the LU factorization, GetriBatched uses forward and backward
        /// triangular solvers to complete inversion of matrices A[i] for i = 0, ..., batchSize-1. The
        /// inversion is out-of-place, so memory space of Carray[i] cannot overlap memory space of
        /// Array[i].
        /// </summary>
        /// <param name="n">number of rows and columns of Aarray[i].</param>
        /// <param name="Aarray">array of pointers to array, with each array of dimension n*n with lda>=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i].</param>
        /// <param name="P">array of size n*batchSize that contains the pivoting sequence of each factorization of Aarray[i] stored in a linear fashion.</param>
        /// <param name="Carray">array of pointers to array, with each array of dimension n*n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix Carray[i].</param>
        /// <param name="INFO">array of size batchSize that info(=infoArray[i]) contains the information of inversion of A[i].<para/>
        /// If info=0, the execution is successful.<para/>
        /// If info = k, U(k,k) is 0. The U is exactly singular and the inversion failed.</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        public void GetriBatchedZ(int n, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<int> P,
            CudaDeviceVariable<CUdeviceptr> Carray, int ldc, CudaDeviceVariable<int> INFO, int batchSize)
        {
            _status = CudaBlasNativeMethods.cublasZgetriBatched(_blasHandle, n, Aarray.DevicePointer, lda, P.DevicePointer, Carray.DevicePointer, ldc, INFO.DevicePointer, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasZgetriBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves an array of triangular linear systems with multiple right-hand-sides.<para/>
        /// The solution overwrites the right-hand-sides on exit.<para/>
        /// No test for singularity or near-singularity is included in this function.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimensions m and n to 32.
        /// </summary>
        /// <param name="side">indicates if matrix A[i] is on the left or right of X[i].</param>
        /// <param name="uplo">indicates if matrix A[i] lower or upper part is stored, the
        /// other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix
        /// A[i] are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B[i], with matrix A[i] sized accordingly.</param>
        /// <param name="n">number of columns of matrix B[i], with matrix A[i] is sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication, if alpha==0 then A[i] is not 
        /// referenced and B[i] does not have to be a valid input.</param>
        /// <param name="A">array of device pointers with each array/device pointerarray 
        /// of dim. lda x m with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x n with
        /// lda>=max(1,n) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A[i].</param>
        /// <param name="B">array of device pointers with each array/device pointerarrayof dim.
        /// ldb x n with ldb>=max(1,m)</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B[i].</param>
        /// <param name="batchCount"></param>
        public void TrsmBatched(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, CudaDeviceVariable<float> alpha,
            CudaDeviceVariable<CUdeviceptr> A, int lda, CudaDeviceVariable<CUdeviceptr> B, int ldb, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasStrsmBatched(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer,
                A.DevicePointer, lda, B.DevicePointer, ldb, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves an array of triangular linear systems with multiple right-hand-sides.<para/>
        /// The solution overwrites the right-hand-sides on exit.<para/>
        /// No test for singularity or near-singularity is included in this function.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimensions m and n to 32.
        /// </summary>
        /// <param name="side">indicates if matrix A[i] is on the left or right of X[i].</param>
        /// <param name="uplo">indicates if matrix A[i] lower or upper part is stored, the
        /// other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix
        /// A[i] are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B[i], with matrix A[i] sized accordingly.</param>
        /// <param name="n">number of columns of matrix B[i], with matrix A[i] is sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication, if alpha==0 then A[i] is not 
        /// referenced and B[i] does not have to be a valid input.</param>
        /// <param name="A">array of device pointers with each array/device pointerarray 
        /// of dim. lda x m with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x n with
        /// lda>=max(1,n) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A[i].</param>
        /// <param name="B">array of device pointers with each array/device pointerarrayof dim.
        /// ldb x n with ldb>=max(1,m)</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B[i].</param>
        /// <param name="batchCount"></param>
        public void TrsmBatched(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, CudaDeviceVariable<double> alpha,
            CudaDeviceVariable<CUdeviceptr> A, int lda, CudaDeviceVariable<CUdeviceptr> B, int ldb, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDtrsmBatched(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer,
                A.DevicePointer, lda, B.DevicePointer, ldb, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves an array of triangular linear systems with multiple right-hand-sides.<para/>
        /// The solution overwrites the right-hand-sides on exit.<para/>
        /// No test for singularity or near-singularity is included in this function.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimensions m and n to 32.
        /// </summary>
        /// <param name="side">indicates if matrix A[i] is on the left or right of X[i].</param>
        /// <param name="uplo">indicates if matrix A[i] lower or upper part is stored, the
        /// other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix
        /// A[i] are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B[i], with matrix A[i] sized accordingly.</param>
        /// <param name="n">number of columns of matrix B[i], with matrix A[i] is sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication, if alpha==0 then A[i] is not 
        /// referenced and B[i] does not have to be a valid input.</param>
        /// <param name="A">array of device pointers with each array/device pointerarray 
        /// of dim. lda x m with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x n with
        /// lda>=max(1,n) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A[i].</param>
        /// <param name="B">array of device pointers with each array/device pointerarrayof dim.
        /// ldb x n with ldb>=max(1,m)</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B[i].</param>
        /// <param name="batchCount"></param>
        public void TrsmBatched(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, ref float alpha,
            CudaDeviceVariable<CUdeviceptr> A, int lda, CudaDeviceVariable<CUdeviceptr> B, int ldb, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasStrsmBatched(_blasHandle, side, uplo, trans, diag, m, n, ref alpha,
                A.DevicePointer, lda, B.DevicePointer, ldb, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves an array of triangular linear systems with multiple right-hand-sides.<para/>
        /// The solution overwrites the right-hand-sides on exit.<para/>
        /// No test for singularity or near-singularity is included in this function.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimensions m and n to 32.
        /// </summary>
        /// <param name="side">indicates if matrix A[i] is on the left or right of X[i].</param>
        /// <param name="uplo">indicates if matrix A[i] lower or upper part is stored, the
        /// other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix
        /// A[i] are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B[i], with matrix A[i] sized accordingly.</param>
        /// <param name="n">number of columns of matrix B[i], with matrix A[i] is sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication, if alpha==0 then A[i] is not 
        /// referenced and B[i] does not have to be a valid input.</param>
        /// <param name="A">array of device pointers with each array/device pointerarray 
        /// of dim. lda x m with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x n with
        /// lda>=max(1,n) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A[i].</param>
        /// <param name="B">array of device pointers with each array/device pointerarrayof dim.
        /// ldb x n with ldb>=max(1,m)</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B[i].</param>
        /// <param name="batchCount"></param>
        public void TrsmBatched(SideMode side, FillMode uplo, Operation trans, DiagType diag, int m, int n, ref double alpha,
            CudaDeviceVariable<CUdeviceptr> A, int lda, CudaDeviceVariable<CUdeviceptr> B, int ldb, int batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDtrsmBatched(_blasHandle, side, uplo, trans, diag, m, n, ref alpha,
                A.DevicePointer, lda, B.DevicePointer, ldb, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsmBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the conversion from the triangular packed format to the
        /// triangular format.<para/>
        /// If uplo == CUBLAS_FILL_MODE_LOWER then the elements of AP are copied into the
        /// lower triangular part of the triangular matrix A and the upper part of A is left untouched.<para/>
        /// If uplo == CUBLAS_FILL_MODE_UPPER then the elements of AP are copied into the
        /// upper triangular part of the triangular matrix A and the lower part of A is left untouched.
        /// </summary>
        /// <param name="uplo">indicates if matrix AP contains lower or upper part of matrix A.</param>
        /// <param name="n">number of rows and columns of matrix A.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        /// <param name="A">array of dimensions lda x n , with lda&gt;=max(1,n). The
        /// opposite side of A is left untouched.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Stpttr(FillMode uplo, int n, CudaDeviceVariable<float> AP, CudaDeviceVariable<float> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasStpttr(_blasHandle, uplo, n, AP.DevicePointer, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStpttr", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the conversion from the triangular packed format to the
        /// triangular format.<para/>
        /// If uplo == CUBLAS_FILL_MODE_LOWER then the elements of AP are copied into the
        /// lower triangular part of the triangular matrix A and the upper part of A is left untouched.<para/>
        /// If uplo == CUBLAS_FILL_MODE_UPPER then the elements of AP are copied into the
        /// upper triangular part of the triangular matrix A and the lower part of A is left untouched.
        /// </summary>
        /// <param name="uplo">indicates if matrix AP contains lower or upper part of matrix A.</param>
        /// <param name="n">number of rows and columns of matrix A.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        /// <param name="A">array of dimensions lda x n , with lda&gt;=max(1,n). The
        /// opposite side of A is left untouched.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Dtpttr(FillMode uplo, int n, CudaDeviceVariable<double> AP, CudaDeviceVariable<double> A, int lda)
        {
            _status = CudaBlasNativeMethods.cublasDtpttr(_blasHandle, uplo, n, AP.DevicePointer, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtpttr", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the conversion from the triangular format to the triangular
        /// packed format.<para/>
        /// If uplo == CUBLAS_FILL_MODE_LOWER then the lower triangular part of the triangular
        /// matrix A is copied into the array AP. <para/>If uplo == CUBLAS_FILL_MODE_UPPER then then
        /// the upper triangular part of the triangular matrix A is copied into the array AP
        /// </summary>
        /// <param name="uplo">indicates which matrix A lower or upper part is referenced</param>
        /// <param name="n">number of rows and columns of matrix A.</param>
        /// <param name="A">array of dimensions lda x n , with lda&gt;=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Strttp(FillMode uplo, int n, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasStrttp(_blasHandle, uplo, n, A.DevicePointer, lda, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrttp", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the conversion from the triangular format to the triangular
        /// packed format.<para/>
        /// If uplo == CUBLAS_FILL_MODE_LOWER then the lower triangular part of the triangular
        /// matrix A is copied into the array AP. <para/>If uplo == CUBLAS_FILL_MODE_UPPER then then
        /// the upper triangular part of the triangular matrix A is copied into the array AP
        /// </summary>
        /// <param name="uplo">indicates which matrix A lower or upper part is referenced</param>
        /// <param name="n">number of rows and columns of matrix A.</param>
        /// <param name="A">array of dimensions lda x n , with lda&gt;=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Dtrttp(FillMode uplo, int n, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasDtrttp(_blasHandle, uplo, n, A.DevicePointer, lda, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrttp", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the QR factorization of each Aarray[i] for i =
        /// 0, ...,batchSize-1 using Householder reflections. Each matrix Q[i] is represented
        /// as a product of elementary reflectors and is stored in the lower part of each Aarray[i].
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.<para/>
        /// cublas<![CDATA[<t>]]>geqrfBatched supports arbitrary dimension.<para/>
        /// cublas<![CDATA[<t>]]>geqrfBatched only supports compute capability 2.0 or above.
        /// </summary>
        /// <param name="m">number of rows Aarray[i].</param>
        /// <param name="n">number of columns of Aarray[i].</param>
        /// <param name="Aarray">array of pointers to device array, with each array of dim. m x n with lda&gt;=max(1,m). The array size determines the number of batches.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i].</param>
        /// <param name="TauArray">array of pointers to device vector, with each vector of dim. max(1,min(m,n)).</param>
        /// <returns>0, if the parameters passed to the function are valid, &lt;0, if the parameter in postion -value is invalid</returns>
        public int GeqrfBatchedS(int m, int n, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> TauArray)
        {
            int info = 0;
            _status = CudaBlasNativeMethods.cublasSgeqrfBatched(_blasHandle, m, n, Aarray.DevicePointer, lda, TauArray.DevicePointer, ref info, Aarray.Size);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgeqrfBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function performs the QR factorization of each Aarray[i] for i =
        /// 0, ...,batchSize-1 using Householder reflections. Each matrix Q[i] is represented
        /// as a product of elementary reflectors and is stored in the lower part of each Aarray[i].
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.<para/>
        /// cublas<![CDATA[<t>]]>geqrfBatched supports arbitrary dimension.<para/>
        /// cublas<![CDATA[<t>]]>geqrfBatched only supports compute capability 2.0 or above.
        /// </summary>
        /// <param name="m">number of rows Aarray[i].</param>
        /// <param name="n">number of columns of Aarray[i].</param>
        /// <param name="Aarray">array of pointers to device array, with each array of dim. m x n with lda&gt;=max(1,m). The array size determines the number of batches.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i].</param>
        /// <param name="TauArray">array of pointers to device vector, with each vector of dim. max(1,min(m,n)).</param>
        /// <returns>0, if the parameters passed to the function are valid, &lt;0, if the parameter in postion -value is invalid</returns>
        public int GeqrfBatchedD(int m, int n, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> TauArray)
        {
            int info = 0;
            _status = CudaBlasNativeMethods.cublasDgeqrfBatched(_blasHandle, m, n, Aarray.DevicePointer, lda, TauArray.DevicePointer, ref info, Aarray.Size);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgeqrfBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function performs the QR factorization of each Aarray[i] for i =
        /// 0, ...,batchSize-1 using Householder reflections. Each matrix Q[i] is represented
        /// as a product of elementary reflectors and is stored in the lower part of each Aarray[i].
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.<para/>
        /// cublas<![CDATA[<t>]]>geqrfBatched supports arbitrary dimension.<para/>
        /// cublas<![CDATA[<t>]]>geqrfBatched only supports compute capability 2.0 or above.
        /// </summary>
        /// <param name="m">number of rows Aarray[i].</param>
        /// <param name="n">number of columns of Aarray[i].</param>
        /// <param name="Aarray">array of pointers to device array, with each array of dim. m x n with lda&gt;=max(1,m). The array size determines the number of batches.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i].</param>
        /// <param name="TauArray">array of pointers to device vector, with each vector of dim. max(1,min(m,n)).</param>
        /// <returns>0, if the parameters passed to the function are valid, &lt;0, if the parameter in postion -value is invalid</returns>
        public int GeqrfBatchedC(int m, int n, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> TauArray)
        {
            int info = 0;
            _status = CudaBlasNativeMethods.cublasCgeqrfBatched(_blasHandle, m, n, Aarray.DevicePointer, lda, TauArray.DevicePointer, ref info, Aarray.Size);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasCgeqrfBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function performs the QR factorization of each Aarray[i] for i =
        /// 0, ...,batchSize-1 using Householder reflections. Each matrix Q[i] is represented
        /// as a product of elementary reflectors and is stored in the lower part of each Aarray[i].
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.<para/>
        /// cublas<![CDATA[<t>]]>geqrfBatched supports arbitrary dimension.<para/>
        /// cublas<![CDATA[<t>]]>geqrfBatched only supports compute capability 2.0 or above.
        /// </summary>
        /// <param name="m">number of rows Aarray[i].</param>
        /// <param name="n">number of columns of Aarray[i].</param>
        /// <param name="Aarray">array of pointers to device array, with each array of dim. m x n with lda&gt;=max(1,m). The array size determines the number of batches.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i].</param>
        /// <param name="TauArray">array of pointers to device vector, with each vector of dim. max(1,min(m,n)).</param>
        /// <returns>0, if the parameters passed to the function are valid, &lt;0, if the parameter in postion -value is invalid</returns>
        public int GeqrfBatchedZ(int m, int n, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> TauArray)
        {
            int info = 0;
            _status = CudaBlasNativeMethods.cublasZgeqrfBatched(_blasHandle, m, n, Aarray.DevicePointer, lda, TauArray.DevicePointer, ref info, Aarray.Size);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasZgeqrfBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function find the least squares solution of a batch of overdetermined systems.
        /// On exit, each Aarray[i] is overwritten with their QR factorization and each Carray[i] is overwritten with the least square solution
        /// GelsBatched supports only the non-transpose operation and only solves overdetermined
        /// systems (m >= n).<para/>
        /// GelsBatched only supports compute capability 2.0 or above.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.
        /// </summary>
        /// <param name="trans">operation op(Aarray[i]) that is non- or (conj.) transpose. Only non-transpose operation is currently supported.</param>
        /// <param name="m">number of rows Aarray[i].</param>
        /// <param name="n">number of columns of each Aarray[i] and rows of each Carray[i].</param>
        /// <param name="nrhs">number of columns of each Carray[i].</param>
        /// <param name="Aarray">array of pointers to device array, with each array of dim. m x n with lda&gt;=max(1,m). The array size determines the number of batches.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i]</param>
        /// <param name="Carray">array of pointers to device array, with each array of dim. m x n with ldc&gt;=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix Carray[i].</param>
        /// <param name="devInfoArray">null or optional array of integers of dimension batchsize.</param>
        /// <returns>0, if the parameters passed to the function are valid, &lt;0, if the parameter in postion -value is invalid</returns>
        public int GelsBatchedS(Operation trans, int m, int n, int nrhs, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> Carray, int ldc, CudaDeviceVariable<int> devInfoArray)
        {
            int info = 0;
            CUdeviceptr _devInfoArray = devInfoArray == null ? new CUdeviceptr(0) : devInfoArray.DevicePointer;
            _status = CudaBlasNativeMethods.cublasSgelsBatched(_blasHandle, trans, m, n, nrhs, Aarray.DevicePointer, lda, Carray.DevicePointer, ldc, ref info, _devInfoArray, Aarray.Size);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgelsBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function find the least squares solution of a batch of overdetermined systems.
        /// On exit, each Aarray[i] is overwritten with their QR factorization and each Carray[i] is overwritten with the least square solution
        /// GelsBatched supports only the non-transpose operation and only solves overdetermined
        /// systems (m >= n).<para/>
        /// GelsBatched only supports compute capability 2.0 or above.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.
        /// </summary>
        /// <param name="trans">operation op(Aarray[i]) that is non- or (conj.) transpose. Only non-transpose operation is currently supported.</param>
        /// <param name="m">number of rows Aarray[i].</param>
        /// <param name="n">number of columns of each Aarray[i] and rows of each Carray[i].</param>
        /// <param name="nrhs">number of columns of each Carray[i].</param>
        /// <param name="Aarray">array of pointers to device array, with each array of dim. m x n with lda&gt;=max(1,m). The array size determines the number of batches.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i]</param>
        /// <param name="Carray">array of pointers to device array, with each array of dim. m x n with ldc&gt;=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix Carray[i].</param>
        /// <param name="devInfoArray">null or optional array of integers of dimension batchsize.</param>
        /// <returns>0, if the parameters passed to the function are valid, &lt;0, if the parameter in postion -value is invalid</returns>
        public int GelsBatchedD(Operation trans, int m, int n, int nrhs, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> Carray, int ldc, CudaDeviceVariable<int> devInfoArray)
        {
            int info = 0;
            CUdeviceptr _devInfoArray = devInfoArray == null ? new CUdeviceptr(0) : devInfoArray.DevicePointer;
            _status = CudaBlasNativeMethods.cublasDgelsBatched(_blasHandle, trans, m, n, nrhs, Aarray.DevicePointer, lda, Carray.DevicePointer, ldc, ref info, _devInfoArray, Aarray.Size);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgelsBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function find the least squares solution of a batch of overdetermined systems.
        /// On exit, each Aarray[i] is overwritten with their QR factorization and each Carray[i] is overwritten with the least square solution
        /// GelsBatched supports only the non-transpose operation and only solves overdetermined
        /// systems (m >= n).<para/>
        /// GelsBatched only supports compute capability 2.0 or above.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.
        /// </summary>
        /// <param name="trans">operation op(Aarray[i]) that is non- or (conj.) transpose. Only non-transpose operation is currently supported.</param>
        /// <param name="m">number of rows Aarray[i].</param>
        /// <param name="n">number of columns of each Aarray[i] and rows of each Carray[i].</param>
        /// <param name="nrhs">number of columns of each Carray[i].</param>
        /// <param name="Aarray">array of pointers to device array, with each array of dim. m x n with lda&gt;=max(1,m). The array size determines the number of batches.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i]</param>
        /// <param name="Carray">array of pointers to device array, with each array of dim. m x n with ldc&gt;=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix Carray[i].</param>
        /// <param name="devInfoArray">null or optional array of integers of dimension batchsize.</param>
        /// <returns>0, if the parameters passed to the function are valid, &lt;0, if the parameter in postion -value is invalid</returns>
        public int GelsBatchedC(Operation trans, int m, int n, int nrhs, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> Carray, int ldc, CudaDeviceVariable<int> devInfoArray)
        {
            int info = 0;
            CUdeviceptr _devInfoArray = devInfoArray == null ? new CUdeviceptr(0) : devInfoArray.DevicePointer;
            _status = CudaBlasNativeMethods.cublasCgelsBatched(_blasHandle, trans, m, n, nrhs, Aarray.DevicePointer, lda, Carray.DevicePointer, ldc, ref info, _devInfoArray, Aarray.Size);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasCgelsBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function find the least squares solution of a batch of overdetermined systems.
        /// On exit, each Aarray[i] is overwritten with their QR factorization and each Carray[i] is overwritten with the least square solution
        /// GelsBatched supports only the non-transpose operation and only solves overdetermined
        /// systems (m >= n).<para/>
        /// GelsBatched only supports compute capability 2.0 or above.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.
        /// </summary>
        /// <param name="trans">operation op(Aarray[i]) that is non- or (conj.) transpose. Only non-transpose operation is currently supported.</param>
        /// <param name="m">number of rows Aarray[i].</param>
        /// <param name="n">number of columns of each Aarray[i] and rows of each Carray[i].</param>
        /// <param name="nrhs">number of columns of each Carray[i].</param>
        /// <param name="Aarray">array of pointers to device array, with each array of dim. m x n with lda&gt;=max(1,m). The array size determines the number of batches.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix Aarray[i]</param>
        /// <param name="Carray">array of pointers to device array, with each array of dim. m x n with ldc&gt;=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix Carray[i].</param>
        /// <param name="devInfoArray">null or optional array of integers of dimension batchsize.</param>
        /// <returns>0, if the parameters passed to the function are valid, &lt;0, if the parameter in postion -value is invalid</returns>
        public int GelsBatchedZ(Operation trans, int m, int n, int nrhs, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<CUdeviceptr> Carray, int ldc, CudaDeviceVariable<int> devInfoArray)
        {
            int info = 0;
            CUdeviceptr _devInfoArray = devInfoArray == null ? new CUdeviceptr(0) : devInfoArray.DevicePointer;
            _status = CudaBlasNativeMethods.cublasZgelsBatched(_blasHandle, trans, m, n, nrhs, Aarray.DevicePointer, lda, Carray.DevicePointer, ldc, ref info, _devInfoArray, Aarray.Size);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasZgelsBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function solves an array of systems of linear equations of the form:<para/>
        /// op(A[i]) X[i] = a B[i]<para/>
        /// where A[i] is a matrix which has been LU factorized with pivoting, X[i] and B[i] are
        /// n x nrhs matrices.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of rows and columns of Aarray[i].</param>
        /// <param name="nrhs">number of columns of Barray[i].</param>
        /// <param name="Aarray">array of pointers to array, with each array of dim. n 
        /// x n with lda&gt;=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store
        /// each matrix Aarray[i].</param>
        /// <param name="devIpiv">array of size n x batchSize that contains the pivoting
        /// sequence of each factorization of Aarray[i] stored in a
        /// linear fashion. If devIpiv is nil, pivoting for all Aarray[i]
        /// is ignored.</param>
        /// <param name="Barray">array of pointers to array, with each array of dim. n
        /// x nrhs with ldb&gt;=max(1,n).</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store
        /// each solution matrix Barray[i].</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        /// <returns>If info=0, the execution is successful. If info = -j, the j-th parameter had an illegal value.</returns>
        public int GetrsBatchedS(Operation trans, int n, int nrhs, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<int> devIpiv,
            CudaDeviceVariable<CUdeviceptr> Barray, int ldb, int batchSize)
        {
            int info = 0;
            _status = CudaBlasNativeMethods.cublasSgetrsBatched(_blasHandle, trans, n, nrhs, Aarray.DevicePointer, lda, devIpiv.DevicePointer, Barray.DevicePointer, ldb, ref info, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgetrsBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function solves an array of systems of linear equations of the form:<para/>
        /// op(A[i]) X[i] = a B[i]<para/>
        /// where A[i] is a matrix which has been LU factorized with pivoting, X[i] and B[i] are
        /// n x nrhs matrices.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of rows and columns of Aarray[i].</param>
        /// <param name="nrhs">number of columns of Barray[i].</param>
        /// <param name="Aarray">array of pointers to array, with each array of dim. n 
        /// x n with lda&gt;=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store
        /// each matrix Aarray[i].</param>
        /// <param name="devIpiv">array of size n x batchSize that contains the pivoting
        /// sequence of each factorization of Aarray[i] stored in a
        /// linear fashion. If devIpiv is nil, pivoting for all Aarray[i]
        /// is ignored.</param>
        /// <param name="Barray">array of pointers to array, with each array of dim. n
        /// x nrhs with ldb&gt;=max(1,n).</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store
        /// each solution matrix Barray[i].</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        /// <returns>If info=0, the execution is successful. If info = -j, the j-th parameter had an illegal value.</returns>
        public int GetrsBatchedD(Operation trans, int n, int nrhs, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<int> devIpiv,
            CudaDeviceVariable<CUdeviceptr> Barray, int ldb, int batchSize)
        {
            int info = 0;
            _status = CudaBlasNativeMethods.cublasDgetrsBatched(_blasHandle, trans, n, nrhs, Aarray.DevicePointer, lda, devIpiv.DevicePointer, Barray.DevicePointer, ldb, ref info, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgetrsBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function solves an array of systems of linear equations of the form:<para/>
        /// op(A[i]) X[i] = a B[i]<para/>
        /// where A[i] is a matrix which has been LU factorized with pivoting, X[i] and B[i] are
        /// n x nrhs matrices.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of rows and columns of Aarray[i].</param>
        /// <param name="nrhs">number of columns of Barray[i].</param>
        /// <param name="Aarray">array of pointers to array, with each array of dim. n 
        /// x n with lda&gt;=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store
        /// each matrix Aarray[i].</param>
        /// <param name="devIpiv">array of size n x batchSize that contains the pivoting
        /// sequence of each factorization of Aarray[i] stored in a
        /// linear fashion. If devIpiv is nil, pivoting for all Aarray[i]
        /// is ignored.</param>
        /// <param name="Barray">array of pointers to array, with each array of dim. n
        /// x nrhs with ldb&gt;=max(1,n).</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store
        /// each solution matrix Barray[i].</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        /// <returns>If info=0, the execution is successful. If info = -j, the j-th parameter had an illegal value.</returns>
        public int GetrsBatchedC(Operation trans, int n, int nrhs, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<int> devIpiv,
            CudaDeviceVariable<CUdeviceptr> Barray, int ldb, int batchSize)
        {
            int info = 0;
            _status = CudaBlasNativeMethods.cublasCgetrsBatched(_blasHandle, trans, n, nrhs, Aarray.DevicePointer, lda, devIpiv.DevicePointer, Barray.DevicePointer, ldb, ref info, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasCgetrsBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// This function solves an array of systems of linear equations of the form:<para/>
        /// op(A[i]) X[i] = a B[i]<para/>
        /// where A[i] is a matrix which has been LU factorized with pivoting, X[i] and B[i] are
        /// n x nrhs matrices.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of rows and columns of Aarray[i].</param>
        /// <param name="nrhs">number of columns of Barray[i].</param>
        /// <param name="Aarray">array of pointers to array, with each array of dim. n 
        /// x n with lda&gt;=max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store
        /// each matrix Aarray[i].</param>
        /// <param name="devIpiv">array of size n x batchSize that contains the pivoting
        /// sequence of each factorization of Aarray[i] stored in a
        /// linear fashion. If devIpiv is nil, pivoting for all Aarray[i]
        /// is ignored.</param>
        /// <param name="Barray">array of pointers to array, with each array of dim. n
        /// x nrhs with ldb&gt;=max(1,n).</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store
        /// each solution matrix Barray[i].</param>
        /// <param name="batchSize">number of pointers contained in A</param>
        /// <returns>If info=0, the execution is successful. If info = -j, the j-th parameter had an illegal value.</returns>
        public int GetrsBatchedZ(Operation trans, int n, int nrhs, CudaDeviceVariable<CUdeviceptr> Aarray, int lda, CudaDeviceVariable<int> devIpiv,
            CudaDeviceVariable<CUdeviceptr> Barray, int ldb, int batchSize)
        {
            int info = 0;
            _status = CudaBlasNativeMethods.cublasZgetrsBatched(_blasHandle, trans, n, nrhs, Aarray.DevicePointer, lda, devIpiv.DevicePointer, Barray.DevicePointer, ldb, ref info, batchSize);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasZgetrsBatched", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return info;
        }

        /// <summary>
        /// copies elements from a vector <c>hostSourceVector</c> in CPU memory space to a vector <c>devDestVector</c> 
        /// in GPU memory space. Storage spacing between consecutive elements
        /// is <c>incrHostSource</c> for the source vector <c>hostSourceVector</c> and <c>incrDevDest</c> for the destination vector
        /// <c>devDestVector</c>. Column major format for two-dimensional matrices
        /// is assumed throughout CUBLAS. Therefore, if the increment for a vector 
        /// is equal to 1, this access a column vector while using an increment 
        /// equal to the leading dimension of the respective matrix accesses a 
        /// row vector.
        /// </summary>
        /// <typeparam name="T">Vector datatype </typeparam>
        /// <param name="hostSourceVector">Source vector in host memory</param>
        /// <param name="incrHostSource"></param>
        /// <param name="devDestVector">Destination vector in device memory</param>
        /// <param name="incrDevDest"></param>
        public static void SetVector<T>(T[] hostSourceVector, int incrHostSource, CudaDeviceVariable<T> devDestVector, int incrDevDest) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostSourceVector, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasSetVector(hostSourceVector.Length, devDestVector.TypeSize, handle.AddrOfPinnedObject(), incrHostSource, devDestVector.DevicePointer, incrDevDest);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetVector", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies elements from a vector <c>devSourceVector</c> in GPU memory space to a vector <c>hostDestVector</c> 
        /// in CPU memory space. Storage spacing between consecutive elements
        /// is <c>incrHostDest</c> for the source vector <c>devSourceVector</c> and <c>incrDevSource</c> for the destination vector
        /// <c>hostDestVector</c>. Column major format for two-dimensional matrices
        /// is assumed throughout CUBLAS. Therefore, if the increment for a vector 
        /// is equal to 1, this access a column vector while using an increment 
        /// equal to the leading dimension of the respective matrix accesses a 
        /// row vector.
        /// </summary>
        /// <typeparam name="T">Vector datatype</typeparam>
        /// <param name="devSourceVector">Source vector in device memory</param>
        /// <param name="incrDevSource"></param>
        /// <param name="hostDestVector">Destination vector in host memory</param>
        /// <param name="incrHostDest"></param>
        public static void GetVector<T>(CudaDeviceVariable<T> devSourceVector, int incrDevSource, T[] hostDestVector, int incrHostDest) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostDestVector, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasGetVector(hostDestVector.Length, devSourceVector.TypeSize, devSourceVector.DevicePointer, incrDevSource, handle.AddrOfPinnedObject(), incrHostDest);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetVector", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies a tile of <c>rows</c> x <c>cols</c> elements from a matrix <c>hostSource</c> in CPU memory
        /// space to a matrix <c>devDest</c> in GPU memory space. Both matrices are assumed to be stored in column 
        /// major format, with the leading dimension (i.e. number of rows) of 
        /// source matrix <c>hostSource</c> provided in <c>ldHostSource</c>, and the leading dimension of matrix <c>devDest</c>
        /// provided in <c>ldDevDest</c>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="hostSource"></param>
        /// <param name="ldHostSource"></param>
        /// <param name="devDest"></param>
        /// <param name="ldDevDest"></param>
        public static void SetMatrix<T>(int rows, int cols, T[] hostSource, int ldHostSource, CudaDeviceVariable<T> devDest, int ldDevDest) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostSource, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasSetMatrix(rows, cols, devDest.TypeSize, handle.AddrOfPinnedObject(), ldHostSource, devDest.DevicePointer, ldDevDest);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetMatrix", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies a tile of <c>rows</c> x <c>cols</c> elements from a matrix <c>devSource</c> in GPU memory
        /// space to a matrix <c>hostDest</c> in CPU memory space. Both matrices are assumed to be stored in column 
        /// major format, with the leading dimension (i.e. number of rows) of 
        /// source matrix <c>devSource</c> provided in <c>devSource</c>, and the leading dimension of matrix <c>hostDest</c>
        /// provided in <c>ldHostDest</c>. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="devSource"></param>
        /// <param name="ldDevSource"></param>
        /// <param name="hostDest"></param>
        /// <param name="ldHostDest"></param>
        public static void GetMatrix<T>(int rows, int cols, CudaDeviceVariable<T> devSource, int ldDevSource, T[] hostDest, int ldHostDest) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostDest, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasGetMatrix(rows, cols, devSource.TypeSize, devSource.DevicePointer, ldDevSource, handle.AddrOfPinnedObject(), ldHostDest);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetMatrix", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies elements from a vector <c>hostSourceVector</c> in CPU memory space to a vector <c>devDestVector</c> 
        /// in GPU memory space. Storage spacing between consecutive elements
        /// is <c>incrHostSource</c> for the source vector <c>hostSourceVector</c> and <c>incrDevDest</c> for the destination vector
        /// <c>devDestVector</c>. Column major format for two-dimensional matrices
        /// is assumed throughout CUBLAS. Therefore, if the increment for a vector 
        /// is equal to 1, this access a column vector while using an increment 
        /// equal to the leading dimension of the respective matrix accesses a 
        /// row vector.
        /// </summary>
        /// <typeparam name="T">Vector datatype </typeparam>
        /// <param name="hostSourceVector">Source vector in host memory</param>
        /// <param name="incrHostSource"></param>
        /// <param name="devDestVector">Destination vector in device memory</param>
        /// <param name="incrDevDest"></param>
        /// <param name="stream"></param>
        public static void SetVectorAsync<T>(T[] hostSourceVector, int incrHostSource, CudaDeviceVariable<T> devDestVector, int incrDevDest, CUstream stream) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostSourceVector, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasSetVectorAsync(hostSourceVector.Length, devDestVector.TypeSize, handle.AddrOfPinnedObject(), incrHostSource, devDestVector.DevicePointer, incrDevDest, stream);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetVectorAsync", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies elements from a vector <c>devSourceVector</c> in GPU memory space to a vector <c>hostDestVector</c> 
        /// in CPU memory space. Storage spacing between consecutive elements
        /// is <c>incrHostDest</c> for the source vector <c>devSourceVector</c> and <c>incrDevSource</c> for the destination vector
        /// <c>hostDestVector</c>. Column major format for two-dimensional matrices
        /// is assumed throughout CUBLAS. Therefore, if the increment for a vector 
        /// is equal to 1, this access a column vector while using an increment 
        /// equal to the leading dimension of the respective matrix accesses a 
        /// row vector.
        /// </summary>
        /// <typeparam name="T">Vector datatype</typeparam>
        /// <param name="devSourceVector">Source vector in device memory</param>
        /// <param name="incrDevSource"></param>
        /// <param name="hostDestVector">Destination vector in host memory</param>
        /// <param name="incrHostDest"></param>
        /// <param name="stream"></param>
        public static void GetVectorAsync<T>(CudaDeviceVariable<T> devSourceVector, int incrDevSource, T[] hostDestVector, int incrHostDest, CUstream stream) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostDestVector, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasGetVectorAsync(hostDestVector.Length, devSourceVector.TypeSize, devSourceVector.DevicePointer, incrDevSource, handle.AddrOfPinnedObject(), incrHostDest, stream);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetVectorAsync", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies a tile of <c>rows</c> x <c>cols</c> elements from a matrix <c>hostSource</c> in CPU memory
        /// space to a matrix <c>devDest</c> in GPU memory space. Both matrices are assumed to be stored in column 
        /// major format, with the leading dimension (i.e. number of rows) of 
        /// source matrix <c>hostSource</c> provided in <c>ldHostSource</c>, and the leading dimension of matrix <c>devDest</c>
        /// provided in <c>ldDevDest</c>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="hostSource"></param>
        /// <param name="ldHostSource"></param>
        /// <param name="devDest"></param>
        /// <param name="ldDevDest"></param>
        /// <param name="stream"></param>
        public static void SetMatrixAsync<T>(int rows, int cols, T[] hostSource, int ldHostSource, CudaDeviceVariable<T> devDest, int ldDevDest, CUstream stream) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostSource, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasSetMatrixAsync(rows, cols, devDest.TypeSize, handle.AddrOfPinnedObject(), ldHostSource, devDest.DevicePointer, ldDevDest, stream);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetMatrixAsync", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies a tile of <c>rows</c> x <c>cols</c> elements from a matrix <c>devSource</c> in GPU memory
        /// space to a matrix <c>hostDest</c> in CPU memory space. Both matrices are assumed to be stored in column 
        /// major format, with the leading dimension (i.e. number of rows) of 
        /// source matrix <c>devSource</c> provided in <c>devSource</c>, and the leading dimension of matrix <c>hostDest</c>
        /// provided in <c>ldHostDest</c>. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="devSource"></param>
        /// <param name="ldDevSource"></param>
        /// <param name="hostDest"></param>
        /// <param name="ldHostDest"></param>
        /// <param name="stream"></param>
        public static void GetMatrixAsync<T>(int rows, int cols, CudaDeviceVariable<T> devSource, int ldDevSource, T[] hostDest, int ldHostDest, CUstream stream) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostDest, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasGetMatrixAsync(rows, cols, devSource.TypeSize, devSource.DevicePointer, ldDevSource, handle.AddrOfPinnedObject(), ldHostDest, stream);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetMatrixAsync", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// This function copies the vector x into the vector y.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="yType"></param>
        /// <param name="incy"></param>
        public void Copy(long n, CUdeviceptr x, cudaDataType xType, long incx, CUdeviceptr y, cudaDataType yType, long incy)
        {
            _status = CudaBlasNativeMethods.cublasCopyEx_64(_blasHandle, n, x, xType, incx, y, yType, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasCopyEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function copies the vector x into the vector y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Copy(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasScopy_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasScopy_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function copies the vector x into the vector y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Copy(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDcopy_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDcopy_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function interchanges the elements of vector x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Swap(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSswap_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSswap_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function interchanges the elements of vector x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Swap(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDswap_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDswap_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function interchanges the elements of vector x and y.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="yType"></param>
        /// <param name="incy"></param>
        public void Swap(long n, CUdeviceptr x, cudaDataType xType, long incx, CUdeviceptr y, cudaDataType yType, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSwapEx_64(_blasHandle, n, x, xType, incx, y, yType, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSwapEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Norm2(CudaDeviceVariable<float> x, long incx, ref float result)
        {
            _status = CudaBlasNativeMethods.cublasSnrm2_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSnrm2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public float Norm2(CudaDeviceVariable<float> x, long incx)
        {
            float result = 0;
            _status = CudaBlasNativeMethods.cublasSnrm2_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSnrm2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }

        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Norm2(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> result)
        {
            _status = CudaBlasNativeMethods.cublasSnrm2_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSnrm2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Norm2(CudaDeviceVariable<double> x, long incx, ref double result)
        {
            _status = CudaBlasNativeMethods.cublasDnrm2_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDnrm2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public double Norm2(CudaDeviceVariable<double> x, long incx)
        {
            double result = 0;
            _status = CudaBlasNativeMethods.cublasDnrm2_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDnrm2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }

        /// <summary>
        /// This function computes the Euclidean norm of the vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Norm2(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> result)
        {
            _status = CudaBlasNativeMethods.cublasDnrm2_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDnrm2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="result"></param>
        public void Dot(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, ref float result)
        {
            _status = CudaBlasNativeMethods.cublasSdot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSdot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public float Dot(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy)
        {
            float result = 0;
            _status = CudaBlasNativeMethods.cublasSdot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSdot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }

        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="result"></param>
        public void Dot(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, CudaDeviceVariable<float> result)
        {
            _status = CudaBlasNativeMethods.cublasSdot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSdot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="result"></param>
        public void Dot(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, ref double result)
        {
            _status = CudaBlasNativeMethods.cublasDdot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDdot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public double Dot(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy)
        {
            double result = 0;
            _status = CudaBlasNativeMethods.cublasDdot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDdot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }

        /// <summary>
        /// This function computes the dot product of vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="result"></param>
        public void Dot(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, CudaDeviceVariable<double> result)
        {
            _status = CudaBlasNativeMethods.cublasDdot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDdot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function scales the vector x by the scalar and overwrites it with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public void Scale(float alpha, CudaDeviceVariable<float> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasSscal_v2_64(_blasHandle, x.Size, ref alpha, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSscal_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function scales the vector x by the scalar and overwrites it with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public void Scale(CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasSscal_v2_64(_blasHandle, x.Size, alpha.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSscal_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function scales the vector x by the scalar and overwrites it with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public void Scale(double alpha, CudaDeviceVariable<double> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasDscal_v2_64(_blasHandle, x.Size, ref alpha, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDscal_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function scales the vector x by the scalar and overwrites it with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public void Scale(CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasDscal_v2_64(_blasHandle, x.Size, alpha.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDscal_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function multiplies the vector x by the scalar and adds it to the vector y overwriting
        /// the latest vector with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Axpy(float alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSaxpy_v2_64(_blasHandle, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSaxpy_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function multiplies the vector x by the scalar and adds it to the vector y overwriting
        /// the latest vector with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Axpy(CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSaxpy_v2_64(_blasHandle, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSaxpy_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function multiplies the vector x by the scalar and adds it to the vector y overwriting
        /// the latest vector with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Axpy(double alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDaxpy_v2_64(_blasHandle, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDaxpy_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function multiplies the vector x by the scalar and adds it to the vector y overwriting
        /// the latest vector with the result.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        public void Axpy(CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDaxpy_v2_64(_blasHandle, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDaxpy_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(CudaDeviceVariable<float> x, long incx, ref long result)
        {
            _status = CudaBlasNativeMethods.cublasIsamin_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamin_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public long Min(CudaDeviceVariable<float> x, long incx)
        {
            long result = 0;
            _status = CudaBlasNativeMethods.cublasIsamin_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamin_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<long> result)
        {
            _status = CudaBlasNativeMethods.cublasIsamin_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamin_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(CudaDeviceVariable<double> x, long incx, ref long result)
        {
            _status = CudaBlasNativeMethods.cublasIdamin_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamin_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public long Min(CudaDeviceVariable<double> x, long incx)
        {
            long result = 0;
            _status = CudaBlasNativeMethods.cublasIdamin_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamin_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<long> result)
        {
            _status = CudaBlasNativeMethods.cublasIdamin_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamin_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the minimum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Min(long n, CUdeviceptr x, cudaDataType xType, long incx, CudaDeviceVariable<long> result)
        {
            _status = CudaBlasNativeMethods.cublasIaminEx_64(_blasHandle, n, x, xType, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIaminEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(CudaDeviceVariable<float> x, long incx, ref long result)
        {
            _status = CudaBlasNativeMethods.cublasIsamax_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamax_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public long Max(CudaDeviceVariable<float> x, long incx)
        {
            long result = 0;
            _status = CudaBlasNativeMethods.cublasIsamax_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamax_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<long> result)
        {
            _status = CudaBlasNativeMethods.cublasIsamax_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIsamax_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(CudaDeviceVariable<double> x, long incx, ref long result)
        {
            _status = CudaBlasNativeMethods.cublasIdamax_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamax_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public long Max(CudaDeviceVariable<double> x, long incx)
        {
            long result = 0;
            _status = CudaBlasNativeMethods.cublasIdamax_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamax_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<long> result)
        {
            _status = CudaBlasNativeMethods.cublasIdamax_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamax_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function finds the (smallest) index of the element of the maximum magnitude.<para/>
        /// First index starts at 1 (Fortran notation)
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void Max(long n, CUdeviceptr x, cudaDataType xType, long incx, CudaDeviceVariable<long> result)
        {
            _status = CudaBlasNativeMethods.cublasIamaxEx_64(_blasHandle, n, x, xType, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIamaxEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void AbsoluteSum(CudaDeviceVariable<float> x, long incx, ref float result)
        {
            _status = CudaBlasNativeMethods.cublasSasum_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSasum_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public float AbsoluteSum(CudaDeviceVariable<float> x, long incx)
        {
            float result = 0;
            _status = CudaBlasNativeMethods.cublasSasum_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSasum_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void AbsoluteSum(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> result)
        {
            _status = CudaBlasNativeMethods.cublasSasum_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSasum_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        /// <param name="resultType"></param>
        /// <param name="executiontype"></param>
        public void AbsoluteSum(long n, CUdeviceptr x, cudaDataType xType, long incx, CUdeviceptr result, cudaDataType resultType, cudaDataType executiontype)
        {
            _status = CudaBlasNativeMethods.cublasAsumEx_64(_blasHandle, n, x, xType, incx, result, resultType, executiontype);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasAsumEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void AbsoluteSum(CudaDeviceVariable<double> x, long incx, ref double result)
        {
            _status = CudaBlasNativeMethods.cublasDasum_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDasum_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        public double AbsoluteSum(CudaDeviceVariable<double> x, long incx)
        {
            double result = 0;
            _status = CudaBlasNativeMethods.cublasDasum_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, ref result);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDasum_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
            return result;
        }
        /// <summary>
        /// This function computes the sum of the absolute values of the elements of vector x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="result"></param>
        public void AbsoluteSum(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> result)
        {
            _status = CudaBlasNativeMethods.cublasIdamax_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, result.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasIdamax_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rot(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, float c, float s)
        {
            _status = CudaBlasNativeMethods.cublasSrot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref c, ref s);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rot(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, CudaDeviceVariable<float> c, CudaDeviceVariable<float> s)
        {
            _status = CudaBlasNativeMethods.cublasSrot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, c.DevicePointer, s.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rot(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, double c, double s)
        {
            _status = CudaBlasNativeMethods.cublasDrot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, ref c, ref s);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        public void Rot(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, CudaDeviceVariable<double> c, CudaDeviceVariable<double> s)
        {
            _status = CudaBlasNativeMethods.cublasDrot_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, c.DevicePointer, s.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrot_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies Givens rotation matrix G = |c s; -s c| to vectors x and y.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <param name="xType"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="yType"></param>
        /// <param name="incy"></param>
        /// <param name="c">Cosine component</param>
        /// <param name="s">Sine component</param>
        /// <param name="csType"></param>
        /// <param name="executiontype"></param>
        public void Rot(long n, CUdeviceptr x, cudaDataType xType, long incx, CUdeviceptr y, cudaDataType yType, long incy, CUdeviceptr c, CUdeviceptr s, cudaDataType csType, cudaDataType executiontype)
        {
            _status = CudaBlasNativeMethods.cublasRotEx_64(_blasHandle, n, x, xType, incx, y, yType, incy, c, s, csType, executiontype);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasRotEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, float[] param)
        {
            _status = CudaBlasNativeMethods.cublasSrotm_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, param);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrotg_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, CudaDeviceVariable<float> param)
        {
            _status = CudaBlasNativeMethods.cublasSrotm_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, param.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSrotg_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, double[] param)
        {
            _status = CudaBlasNativeMethods.cublasDrotm_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, param);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrotg_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        public void Rotm(CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, CudaDeviceVariable<double> param)
        {
            _status = CudaBlasNativeMethods.cublasDrotm_v2_64(_blasHandle, x.Size, x.DevicePointer, incx, y.DevicePointer, incy, param.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDrotg_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function applies the modified Givens transformation H = |h11 h12; h21 h22| to vectors x and y.<para/>
        /// The elements h11, h21, h12 and h22 of 2x2 matrix H are stored in param[1], param[2], param[3] and param[4], respectively. <para/>
        /// The flag = param[0] defines the following predefined values for the matrix H entries:<para/>
        /// flag=-1.0: H = |h11 h12; h21 h22|<para/>
        /// flag= 0.0: H = |1.0 h12; h21 1.0|<para/> 
        /// flag= 1.0: H = |h11 1.0; -1.0 h22|<para/>
        /// flag=-2.0: H = |1.0 0.0; 0.0 1.0|<para/>
        /// Notice that the values -1.0, 0.0 and 1.0 implied by the flag are not stored in param.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="incx"></param>
        /// <param name="y"></param>
        /// <param name="incy"></param>
        /// <param name="param"></param>
        /// <param name="executiontype"></param>
        /// <param name="n"></param>
        /// <param name="paramType"></param>
        /// <param name="xType"></param>
        /// <param name="yType"></param>
        public void Rotm(CUdeviceptr x, long n, cudaDataType xType, long incx, CUdeviceptr y, cudaDataType yType, long incy, CUdeviceptr param, cudaDataType paramType, cudaDataType executiontype)
        {
            _status = CudaBlasNativeMethods.cublasRotmEx_64(_blasHandle, n, x, xType, incx, y, yType, incy, param, paramType, executiontype);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasRotmEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the triangular matrix-vector multiplication x= Op(A) x where A is a triangular matrix stored in 
        /// lower or upper mode with or without the main diagonal, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Trmv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasStrmv_v2_64(_blasHandle, uplo, trans, diag, x.Size, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular matrix-vector multiplication x= Op(A) x where A is a triangular matrix stored in 
        /// lower or upper mode with or without the main diagonal, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Trmv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasDtrmv_v2_64(_blasHandle, uplo, trans, diag, x.Size, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the triangular banded matrix-vector multiplication x= Op(A) x where A is a triangular banded matrix, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tbmv(FillMode uplo, Operation trans, DiagType diag, long k, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasStbmv_v2_64(_blasHandle, uplo, trans, diag, x.Size, k, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular banded matrix-vector multiplication x= Op(A) x where A is a triangular banded matrix, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tbmv(FillMode uplo, Operation trans, DiagType diag, long k, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasDtbmv_v2_64(_blasHandle, uplo, trans, diag, x.Size, k, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the triangular packed matrix-vector multiplication x= Op(A) x where A is a triangular matrix stored in packed format, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tpmv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<float> AP, CudaDeviceVariable<float> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasStpmv_v2_64(_blasHandle, uplo, trans, diag, x.Size, AP.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStpmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular packed matrix-vector multiplication x= Op(A) x where A is a triangular matrix stored in packed format, and x is a vector. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tpmv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<double> AP, CudaDeviceVariable<double> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasDtpmv_v2_64(_blasHandle, uplo, trans, diag, x.Size, AP.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtpmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves the triangular linear system with a single right-hand-side Op(A)x = b where A is a triangular matrix stored in lower or 
        /// upper mode with or without the main diagonal, and x and b are vectors. The solution x overwrites the right-hand-sides b on exit. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Trsv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasStrsv_v2_64(_blasHandle, uplo, trans, diag, x.Size, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the triangular linear system with a single right-hand-side Op(A)x = b where A is a triangular matrix stored in lower or 
        /// upper mode with or without the main diagonal, and x and b are vectors. The solution x overwrites the right-hand-sides b on exit. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Trsv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasDtrsv_v2_64(_blasHandle, uplo, trans, diag, x.Size, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves the packed triangular linear system with a single right-hand-side Op(A) x = b where A is a triangular matrix stored in packed format, and x and b are vectors. 
        /// The solution x overwrites the right-hand-sides b on exit. n is given by x.Size. No test for singularity or near-singularity is included in this function.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tpsv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<float> AP, CudaDeviceVariable<float> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasStpsv_v2_64(_blasHandle, uplo, trans, diag, x.Size, AP.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStpsv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the packed triangular linear system with a single right-hand-side Op(A) x = b where A is a triangular matrix stored in packed format, and x and b are vectors. 
        /// The solution x overwrites the right-hand-sides b on exit. n is given by x.Size. No test for singularity or near-singularity is included in this function.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tpsv(FillMode uplo, Operation trans, DiagType diag, CudaDeviceVariable<double> AP, CudaDeviceVariable<double> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasDtpsv_v2_64(_blasHandle, uplo, trans, diag, x.Size, AP.DevicePointer, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtpsv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves the triangular banded linear system with a single right-hand-side Op(A) x = b where A is a triangular banded matrix, and x and b is a vector. 
        /// The solution x overwrites the right-hand-sides b on exit. n is given by x.Size. No test for singularity or near-singularity is included in this function.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tbsv(FillMode uplo, Operation trans, DiagType diag, long k, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasStbsv_v2_64(_blasHandle, uplo, trans, diag, x.Size, k, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStbsv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the triangular banded linear system with a single right-hand-side Op(A) x = b where A is a triangular banded matrix, and x and b is a vector. 
        /// The solution x overwrites the right-hand-sides b on exit. n is given by x.Size. No test for singularity or near-singularity is included in this function.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        public void Tbsv(FillMode uplo, Operation trans, DiagType diag, long k, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx)
        {
            _status = CudaBlasNativeMethods.cublasDtbsv_v2_64(_blasHandle, uplo, trans, diag, x.Size, k, A.DevicePointer, lda, x.DevicePointer, incx);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtbsv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gemv(Operation trans, long m, long n, float alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx, float beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSgemv_v2_64(_blasHandle, trans, m, n, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gemv(Operation trans, long m, long n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSgemv_v2_64(_blasHandle, trans, m, n, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gemv(Operation trans, long m, long n, double alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx, double beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDgemv_v2_64(_blasHandle, trans, m, n, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gemv(Operation trans, long m, long n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDgemv_v2_64(_blasHandle, trans, m, n, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="kl">number of subdiagonals of matrix A.</param>
        /// <param name="ku">number of superdiagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gbmv(Operation trans, long m, long n, long kl, long ku, float alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx, float beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSgbmv_v2_64(_blasHandle, trans, m, n, kl, ku, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="kl">number of subdiagonals of matrix A.</param>
        /// <param name="ku">number of superdiagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gbmv(Operation trans, long m, long n, long kl, long ku, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSgbmv_v2_64(_blasHandle, trans, m, n, kl, ku, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="kl">number of subdiagonals of matrix A.</param>
        /// <param name="ku">number of superdiagonals of matrix A.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gbmv(Operation trans, long m, long n, long kl, long ku, double alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx, double beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDgbmv_v2_64(_blasHandle, trans, m, n, kl, ku, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-vector multiplication y = alpha * Op(A) * x + beta * y where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha and beta are scalars.
        /// </summary>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix A.</param>
        /// <param name="n">number of columns of matrix A.</param>
        /// <param name="kl">number of subdiagonals of matrix A.</param>
        /// <param name="ku">number of superdiagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Gbmv(Operation trans, long m, long n, long kl, long ku, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDgbmv_v2_64(_blasHandle, trans, m, n, kl, ku, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in lower or upper mode, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Symv(FillMode uplo, float alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx, float beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSsymv_v2_64(_blasHandle, uplo, x.Size, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in lower or upper mode, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Symv(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSsymv_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in lower or upper mode, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Symv(FillMode uplo, double alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx, double beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDsymv_v2_64(_blasHandle, uplo, x.Size, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsymv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-vector multiplication y = alpha *A * x + beta * y where A is a n*n symmetric matrix stored in lower or upper mode, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Symv(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDsymv_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric banded matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix with k subdiagonals and superdiagonals, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Sbmv(FillMode uplo, long k, float alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx, float beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSsbmv_v2_64(_blasHandle, uplo, x.Size, k, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric banded matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix with k subdiagonals and superdiagonals, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Sbmv(FillMode uplo, long k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSsbmv_v2_64(_blasHandle, uplo, x.Size, k, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric banded matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix with k subdiagonals and superdiagonals, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Sbmv(FillMode uplo, long k, double alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx, double beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDsbmv_v2_64(_blasHandle, uplo, x.Size, k, ref alpha, A.DevicePointer, lda, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric banded matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix with k subdiagonals and superdiagonals, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="k">number of sub- and super-diagonals of matrix A.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Sbmv(FillMode uplo, long k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDsbmv_v2_64(_blasHandle, uplo, x.Size, k, alpha.DevicePointer, A.DevicePointer, lda, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsbmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric packed matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in packed format, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Spmv(FillMode uplo, float alpha, CudaDeviceVariable<float> AP, CudaDeviceVariable<float> x, long incx, float beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSspmv_v2_64(_blasHandle, uplo, x.Size, ref alpha, AP.DevicePointer, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric packed matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in packed format, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Spmv(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> AP, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasSspmv_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, AP.DevicePointer, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric packed matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in packed format, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Spmv(FillMode uplo, double alpha, CudaDeviceVariable<double> AP, CudaDeviceVariable<double> x, long incx, double beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDspmv_v2_64(_blasHandle, uplo, x.Size, ref alpha, AP.DevicePointer, x.DevicePointer, incx, ref beta, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric packed matrix-vector multiplication y = alpha * A * x + beta * y where A is a n*n symmetric matrix stored in packed format, 
        /// x and y are vectors, and alpha and beta are scalars. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="AP">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0 then y does not have to be a valid input.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        public void Spmv(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> AP, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y, long incy)
        {
            _status = CudaBlasNativeMethods.cublasDspmv_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, AP.DevicePointer, x.DevicePointer, incx, beta.DevicePointer, y.DevicePointer, incy);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspmv_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the rank-1 update A = alpha * x * y^T + A where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha is a scalar. m = x.Size, n = y.Size.
        /// </summary>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Ger(float alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, CudaDeviceVariable<float> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasSger_v2_64(_blasHandle, x.Size, y.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSger_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the rank-1 update A = alpha * x * y^T + A where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha is a scalar. m = x.Size, n = y.Size.
        /// </summary>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Ger(CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, CudaDeviceVariable<float> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasSger_v2_64(_blasHandle, x.Size, y.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSger_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the rank-1 update A = alpha * x * y^T + A where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha is a scalar. m = x.Size, n = y.Size.
        /// </summary>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Ger(double alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, CudaDeviceVariable<double> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasDger_v2_64(_blasHandle, x.Size, y.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDger_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the rank-1 update A = alpha * x * y^T + A where A is a m*n matrix stored in column-major format, 
        /// x and y are vectors, and alpha is a scalar. m = x.Size, n = y.Size.
        /// </summary>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Ger(CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, CudaDeviceVariable<double> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasDger_v2_64(_blasHandle, x.Size, y.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDger_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr(FillMode uplo, float alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasSsyr_v2_64(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasSsyr_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr(FillMode uplo, double alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasDsyr_v2_64(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasDsyr_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr(FillMode uplo, float alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasSspr_v2_64(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspr_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasSspr_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspr_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr(FillMode uplo, double alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> AP)
        {
            _status = CudaBlasNativeMethods.cublasDspr_v2_64(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspr_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-1 update A = alpha * x * x^T + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> AP)
        {
            _status = CudaBlasNativeMethods.cublasDspr_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspr_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-2 update A = alpha * (x * y^T + y * y^T) + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr2(FillMode uplo, float alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, CudaDeviceVariable<float> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasSsyr2_v2_64(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-2 update A = alpha * (x * y^T + y * y^T) + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr2(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, CudaDeviceVariable<float> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasSsyr2_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-2 update A = alpha * (x * y^T + y * y^T) + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr2(FillMode uplo, double alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, CudaDeviceVariable<double> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasDsyr2_v2_64(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-2 update A = alpha * (x * y^T + y * y^T) + A where A is a n*n symmetric Matrix stored in column-major format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of y.</param>
        /// <param name="A">array of dimensions lda * n, with lda >= max(1,n).</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        public void Syr2(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, CudaDeviceVariable<double> A, long lda)
        {
            _status = CudaBlasNativeMethods.cublasDsyr2_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, A.DevicePointer, lda);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the packed symmetric rank-2 update A = alpha * (x * y^T + y * x^T) + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr2(FillMode uplo, float alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasSspr2_v2_64(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspr2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the packed symmetric rank-2 update A = alpha * (x * y^T + y * x^T) + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr2(FillMode uplo, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> x, long incx, CudaDeviceVariable<float> y, long incy, CudaDeviceVariable<float> AP)
        {
            _status = CudaBlasNativeMethods.cublasSspr2_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSspr2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the packed symmetric rank-2 update A = alpha * (x * y^T + y * x^T) + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr2(FillMode uplo, double alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, CudaDeviceVariable<double> AP)
        {
            _status = CudaBlasNativeMethods.cublasDspr2_v2_64(_blasHandle, uplo, x.Size, ref alpha, x.DevicePointer, incx, y.DevicePointer, incy, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspr2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the packed symmetric rank-2 update A = alpha * (x * y^T + y * x^T) + A where A is a n*n symmetric Matrix stored in packed format,
        /// x is a vector, and alpha is a scalar. n is given by x.Size = y.Size.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="x">vector with n elements.</param>
        /// <param name="incx">stride between consecutive elements of x.</param>
        /// <param name="y">vector with n elements.</param>
        /// <param name="incy">stride between consecutive elements of x.</param>
        /// <param name="AP">array with A stored in packed format.</param>
        public void Spr2(FillMode uplo, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> x, long incx, CudaDeviceVariable<double> y, long incy, CudaDeviceVariable<double> AP)
        {
            _status = CudaBlasNativeMethods.cublasDspr2_v2_64(_blasHandle, uplo, x.Size, alpha.DevicePointer, x.DevicePointer, incx, y.DevicePointer, incy, AP.DevicePointer);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDspr2_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, long m, long n, long k, float alpha, CudaDeviceVariable<float> A, long lda,
            CudaDeviceVariable<float> B, long ldb, float beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgemm_v2_64(_blasHandle, transa, transb, m, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda,
            CudaDeviceVariable<float> B, long ldb, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgemm_v2_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, long m, long n, long k, double alpha, CudaDeviceVariable<double> A, long lda,
            CudaDeviceVariable<double> B, long ldb, double beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDgemm_v2_64(_blasHandle, transa, transb, m, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda,
            CudaDeviceVariable<double> B, long ldb, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDgemm_v2_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, long m, long n, long k, half alpha, CudaDeviceVariable<half> A, long lda,
            CudaDeviceVariable<half> B, long ldb, half beta, CudaDeviceVariable<half> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasHgemm_64(_blasHandle, transa, transb, m, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasHgemm_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Gemm(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<half> alpha, CudaDeviceVariable<half> A, long lda,
            CudaDeviceVariable<half> B, long ldb, CudaDeviceVariable<half> beta, CudaDeviceVariable<half> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasHgemm_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasHgemm_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="Atype">enumerant specifying the datatype of matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="Btype">enumerant specifying the datatype of matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        /// <param name="Ctype">enumerant specifying the datatype of matrix C.</param>
        public void GemmEx(Operation transa, Operation transb, long m, long n, long k, float alpha, CUdeviceptr A, DataType Atype, long lda,
            CUdeviceptr B, DataType Btype, long ldb, float beta, CUdeviceptr C, DataType Ctype, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgemmEx_64(_blasHandle, transa, transb, m, n, k, ref alpha, A, Atype, lda, B, Btype, ldb, ref beta, C, Ctype, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = alpha * Op(A) * Op(B) + beta * C where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*k, op(B) k*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="Atype">enumerant specifying the datatype of matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="Btype">enumerant specifying the datatype of matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        /// <param name="Ctype">enumerant specifying the datatype of matrix C.</param>
        public void GemmEx(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<float> alpha, CUdeviceptr A, DataType Atype, long lda,
            CUdeviceptr B, DataType Btype, long ldb, CudaDeviceVariable<float> beta, CUdeviceptr C, DataType Ctype, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgemmEx_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A, Atype, lda, B, Btype, ldb, beta.DevicePointer, C, Ctype, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * Op(A)*Op(A)^T + beta * C where
        /// alpha and beta are scalars, and A, B and C are matrices stored in lower or upper mode, and A is a matrix with dimensions op(A) n*k.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrk(FillMode uplo, Operation trans, long n, long k, float alpha, CudaDeviceVariable<float> A, long lda,
            float beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyrk_v2_64(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyrk_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * Op(A)*Op(A)^T + beta * C where
        /// alpha and beta are scalars, and A, B and C are matrices stored in lower or upper mode, and A is a matrix with dimensions op(A) n*k.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrk(FillMode uplo, Operation trans, long n, long k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda,
            CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyrk_v2_64(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyrk_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * Op(A)*Op(A)^T + beta * C where
        /// alpha and beta are scalars, and A, B and C are matrices stored in lower or upper mode, and A is a matrix with dimensions op(A) n*k.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrk(FillMode uplo, Operation trans, long n, long k, double alpha, CudaDeviceVariable<double> A, long lda,
            double beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyrk_v2_64(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyrk_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * Op(A)*Op(A)^T + beta * C where
        /// alpha and beta are scalars, and A, B and C are matrices stored in lower or upper mode, and A is a matrix with dimensions op(A) n*k.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrk(FillMode uplo, Operation trans, long n, long k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda,
            CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyrk_v2_64(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyrk_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * (Op(A)*Op(B)^T + Op(B)*Op(A)^T) + beta * C where
        /// alpha and beta are scalars, and C is a symmetrux matrix stored in lower or upper mode, and A and B are matrices with dimensions Op(A) n*k
        /// and Op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * k.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syr2k(FillMode uplo, Operation trans, long n, long k, float alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> B, long ldb,
            float beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyr2k_v2_64(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr2k_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * (Op(A)*Op(B)^T + Op(B)*Op(A)^T) + beta * C where
        /// alpha and beta are scalars, and C is a symmetrux matrix stored in lower or upper mode, and A and B are matrices with dimensions Op(A) n*k
        /// and Op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * k.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syr2k(FillMode uplo, Operation trans, long n, long k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> B, long ldb,
            CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyr2k_v2_64(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyr2k_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * (Op(A)*Op(B)^T + Op(B)*Op(A)^T) + beta * C where
        /// alpha and beta are scalars, and C is a symmetrux matrix stored in lower or upper mode, and A and B are matrices with dimensions Op(A) n*k
        /// and Op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * k.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syr2k(FillMode uplo, Operation trans, long n, long k, double alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> B, long ldb,
            double beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyr2k_v2_64(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr2k_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric rank-k update C = alpha * (Op(A)*Op(B)^T + Op(B)*Op(A)^T) + beta * C where
        /// alpha and beta are scalars, and C is a symmetrux matrix stored in lower or upper mode, and A and B are matrices with dimensions Op(A) n*k
        /// and Op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="k">number of columns of op(A) and rows of op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * k.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syr2k(FillMode uplo, Operation trans, long n, long k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> B, long ldb,
            CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyr2k_v2_64(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyr2k_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs a variation of the symmetric rank- update C = alpha * (Op(A)*Op(B))^T + beta * C where alpha 
        /// and beta are scalars, C is a symmetric matrix stored in lower or upper mode, and A
        /// and B are matrices with dimensions op(A) n*k and op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix C lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or transpose.</param>
        /// <param name="n">number of rows of matrix op(A), op(B) and C.</param>
        /// <param name="k">number of columns of matrix op(A) and op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimension lda x k with lda>=max(1,n) if transa == CUBLAS_OP_N and lda x n with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb x k with ldb>=max(1,n) if transa == CUBLAS_OP_N and ldb x n with ldb>=max(1,k) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0, then C does not have to be a valid input.</param>
        /// <param name="C">array of dimensions ldc x n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrkx(FillMode uplo, Operation trans, long n, long k, ref float alpha, CudaDeviceVariable<float> A, long lda,
                                                    CudaDeviceVariable<float> B, long ldb, ref float beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyrkx_64(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyrkx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs a variation of the symmetric rank- update C = alpha * (Op(A)*Op(B))^T + beta * C where alpha 
        /// and beta are scalars, C is a symmetric matrix stored in lower or upper mode, and A
        /// and B are matrices with dimensions op(A) n*k and op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix C lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or transpose.</param>
        /// <param name="n">number of rows of matrix op(A), op(B) and C.</param>
        /// <param name="k">number of columns of matrix op(A) and op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimension lda x k with lda>=max(1,n) if transa == CUBLAS_OP_N and lda x n with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb x k with ldb>=max(1,n) if transa == CUBLAS_OP_N and ldb x n with ldb>=max(1,k) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0, then C does not have to be a valid input.</param>
        /// <param name="C">array of dimensions ldc x n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrkx(FillMode uplo, Operation trans, long n, long k, ref double alpha, CudaDeviceVariable<double> A, long lda,
                                                    CudaDeviceVariable<double> B, long ldb, ref double beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyrkx_64(_blasHandle, uplo, trans, n, k, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyrkx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs a variation of the symmetric rank- update C = alpha * (Op(A)*Op(B))^T + beta * C where alpha 
        /// and beta are scalars, C is a symmetric matrix stored in lower or upper mode, and A
        /// and B are matrices with dimensions op(A) n*k and op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix C lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or transpose.</param>
        /// <param name="n">number of rows of matrix op(A), op(B) and C.</param>
        /// <param name="k">number of columns of matrix op(A) and op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimension lda x k with lda>=max(1,n) if transa == CUBLAS_OP_N and lda x n with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb x k with ldb>=max(1,n) if transa == CUBLAS_OP_N and ldb x n with ldb>=max(1,k) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0, then C does not have to be a valid input.</param>
        /// <param name="C">array of dimensions ldc x n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrkx(FillMode uplo, Operation trans, long n, long k, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda,
                                                    CudaDeviceVariable<float> B, long ldb, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsyrkx_64(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsyrkx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs a variation of the symmetric rank- update C = alpha * (Op(A)*Op(B))^T + beta * C where alpha 
        /// and beta are scalars, C is a symmetric matrix stored in lower or upper mode, and A
        /// and B are matrices with dimensions op(A) n*k and op(B) n*k, respectively.
        /// </summary>
        /// <param name="uplo">indicates if matrix C lower or upper part, is stored, the other symmetric part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or transpose.</param>
        /// <param name="n">number of rows of matrix op(A), op(B) and C.</param>
        /// <param name="k">number of columns of matrix op(A) and op(B).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimension lda x k with lda>=max(1,n) if transa == CUBLAS_OP_N and lda x n with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb x k with ldb>=max(1,n) if transa == CUBLAS_OP_N and ldb x n with ldb>=max(1,k) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication, if beta==0, then C does not have to be a valid input.</param>
        /// <param name="C">array of dimensions ldc x n with ldc>=max(1,n).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Syrkx(FillMode uplo, Operation trans, long n, long k, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda,
                                                    CudaDeviceVariable<double> B, long ldb, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsyrkx_64(_blasHandle, uplo, trans, n, k, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsyrkx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric matrix-matrix multiplication C = alpha*A*B + beta*C if side==SideMode.Left or C = alpha*B*A + beta*C if side==SideMode.Right 
        /// where A is a symmetric matrix stored in lower or upper mode, B and C are m*n matrices, and alpha and beta are scalars.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of B.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="m">number of rows of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Symm(SideMode side, FillMode uplo, long m, long n, float alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> B, long ldb,
            float beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsymm_v2_64(_blasHandle, side, uplo, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-matrix multiplication C = alpha*A*B + beta*C if side==SideMode.Left or C = alpha*B*A + beta*C if side==SideMode.Right 
        /// where A is a symmetric matrix stored in lower or upper mode, B and C are m*n matrices, and alpha and beta are scalars.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of B.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="m">number of rows of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Symm(SideMode side, FillMode uplo, long m, long n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> B, long ldb,
            CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSsymm_v2_64(_blasHandle, side, uplo, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSsymm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the symmetric matrix-matrix multiplication C = alpha*A*B + beta*C if side==SideMode.Left or C = alpha*B*A + beta*C if side==SideMode.Right 
        /// where A is a symmetric matrix stored in lower or upper mode, B and C are m*n matrices, and alpha and beta are scalars.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of B.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="m">number of rows of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Symm(SideMode side, FillMode uplo, long m, long n, double alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> B, long ldb,
            double beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsymm_v2_64(_blasHandle, side, uplo, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsymm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the symmetric matrix-matrix multiplication C = alpha*A*B + beta*C if side==SideMode.Left or C = alpha*B*A + beta*C if side==SideMode.Right 
        /// where A is a symmetric matrix stored in lower or upper mode, B and C are m*n matrices, and alpha and beta are scalars.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of B.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="m">number of rows of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix C and B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Symm(SideMode side, FillMode uplo, long m, long n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> B, long ldb,
            CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDsymm_v2_64(_blasHandle, side, uplo, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDsymm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function solves the triangular linear system with multiple right-hand-sides Op(A)X = alpha*B side==SideMode.Left or XOp(A) = alpha*B if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the maindiagonal, X and B are m*n matrices, and alpha is a scalar.<para/>
        /// The solution X overwrites the right-hand-sides B on exit.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, float alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> B, long ldb)
        {
            _status = CudaBlasNativeMethods.cublasStrsm_v2_64(_blasHandle, side, uplo, trans, diag, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the triangular linear system with multiple right-hand-sides Op(A)X = alpha*B side==SideMode.Left or XOp(A) = alpha*B if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the maindiagonal, X and B are m*n matrices, and alpha is a scalar.<para/>
        /// The solution X overwrites the right-hand-sides B on exit.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda, CudaDeviceVariable<float> B, long ldb)
        {
            _status = CudaBlasNativeMethods.cublasStrsm_v2_64(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves the triangular linear system with multiple right-hand-sides Op(A)X = alpha*B side==SideMode.Left or XOp(A) = alpha*B if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the maindiagonal, X and B are m*n matrices, and alpha is a scalar.<para/>
        /// The solution X overwrites the right-hand-sides B on exit.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, double alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> B, long ldb)
        {
            _status = CudaBlasNativeMethods.cublasDtrsm_v2_64(_blasHandle, side, uplo, trans, diag, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves the triangular linear system with multiple right-hand-sides Op(A)X = alpha*B side==SideMode.Left or XOp(A) = alpha*B if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the maindiagonal, X and B are m*n matrices, and alpha is a scalar.<para/>
        /// The solution X overwrites the right-hand-sides B on exit.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda, CudaDeviceVariable<double> B, long ldb)
        {
            _status = CudaBlasNativeMethods.cublasDtrsm_v2_64(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function performs the triangular matrix-matrix multiplication C = alpha*Op(A) * B if side==SideMode.Left or C = alpha*B * Op(A) if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the main diagonal, B and C are m*n matrices, and alpha is a scalar.<para/>
        /// Notice that in order to achieve better parallelism CUBLAS differs from the BLAS API only for this routine. The BLAS API assumes an in-place implementation (with results
        /// written back to B), while the CUBLAS API assumes an out-of-place implementation (with results written into C). The application can obtain the in-place functionality of BLAS in
        /// the CUBLAS API by passing the address of the matrix B in place of the matrix C. No other overlapping in the input parameters is supported.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, float alpha, CudaDeviceVariable<float> A, long lda,
            CudaDeviceVariable<float> B, long ldb, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasStrmm_v2_64(_blasHandle, side, uplo, trans, diag, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrmm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular matrix-matrix multiplication C = alpha*Op(A) * B if side==SideMode.Left or C = alpha*B * Op(A) if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the main diagonal, B and C are m*n matrices, and alpha is a scalar.<para/>
        /// Notice that in order to achieve better parallelism CUBLAS differs from the BLAS API only for this routine. The BLAS API assumes an in-place implementation (with results
        /// written back to B), while the CUBLAS API assumes an out-of-place implementation (with results written into C). The application can obtain the in-place functionality of BLAS in
        /// the CUBLAS API by passing the address of the matrix B in place of the matrix C. No other overlapping in the input parameters is supported.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda,
            CudaDeviceVariable<float> B, long ldb, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasStrmm_v2_64(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrmm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the triangular matrix-matrix multiplication C = alpha*Op(A) * B if side==SideMode.Left or C = alpha*B * Op(A) if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the main diagonal, B and C are m*n matrices, and alpha is a scalar.<para/>
        /// Notice that in order to achieve better parallelism CUBLAS differs from the BLAS API only for this routine. The BLAS API assumes an in-place implementation (with results
        /// written back to B), while the CUBLAS API assumes an out-of-place implementation (with results written into C). The application can obtain the in-place functionality of BLAS in
        /// the CUBLAS API by passing the address of the matrix B in place of the matrix C. No other overlapping in the input parameters is supported.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, double alpha, CudaDeviceVariable<double> A, long lda,
            CudaDeviceVariable<double> B, long ldb, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDtrmm_v2_64(_blasHandle, side, uplo, trans, diag, m, n, ref alpha, A.DevicePointer, lda, B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrmm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the triangular matrix-matrix multiplication C = alpha*Op(A) * B if side==SideMode.Left or C = alpha*B * Op(A) if side==SideMode.Right 
        /// where A is a triangular matrix stored in lower or upper mode with or without the main diagonal, B and C are m*n matrices, and alpha is a scalar.<para/>
        /// Notice that in order to achieve better parallelism CUBLAS differs from the BLAS API only for this routine. The BLAS API assumes an in-place implementation (with results
        /// written back to B), while the CUBLAS API assumes an out-of-place implementation (with results written into C). The application can obtain the in-place functionality of BLAS in
        /// the CUBLAS API by passing the address of the matrix B in place of the matrix C. No other overlapping in the input parameters is supported.
        /// </summary>
        /// <param name="side">indicates if matrix A is on the left or right of X.</param>
        /// <param name="uplo">indicates if matrix A lower or upper part is stored, the other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix A are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B, with matrix A sized accordingly.</param>
        /// <param name="n">number of columns of matrix B, with matrix A sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication. If alpha==0 then A is not referenced and B does not have to be a valid input.</param>
        /// <param name="A">array of dimensions lda * m.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="C">array of dimensions ldc * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Trsm(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda,
            CudaDeviceVariable<double> B, long ldb, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDtrmm_v2_64(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer, A.DevicePointer, lda, B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrmm_v2_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix addition/transposition C = alpha * Op(A) + beta * Op(B) where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*n, op(B) m*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Geam(Operation transa, Operation transb, long m, long n, float alpha, CudaDeviceVariable<float> A, long lda,
            CudaDeviceVariable<float> B, long ldb, float beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgeam_64(_blasHandle, transa, transb, m, n, ref alpha, A.DevicePointer, lda, ref beta,
                B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgeam_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix addition/transposition C = alpha * Op(A) + beta * Op(B) where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*n, op(B) m*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Geam(Operation transa, Operation transb, long m, long n, double alpha, CudaDeviceVariable<double> A, long lda,
            CudaDeviceVariable<double> B, long ldb, double beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDgeam_64(_blasHandle, transa, transb, m, n, ref alpha, A.DevicePointer, lda, ref beta,
                B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgeam_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix addition/transposition C = alpha * Op(A) + beta * Op(B) where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*n, op(B) m*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Geam(Operation transa, Operation transb, long m, long n, CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> A, long lda,
            CudaDeviceVariable<float> B, long ldb, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSgeam_64(_blasHandle, transa, transb, m, n, alpha.DevicePointer, A.DevicePointer, lda, beta.DevicePointer,
                B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgeam_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix addition/transposition C = alpha * Op(A) + beta * Op(B) where 
        /// alpha and beta are scalars, and A, B and C are matrices stored in column-major format with dimensions 
        /// op(A) m*n, op(B) m*n and C m*n, respectively.
        /// </summary>
        /// <param name="transa">operation op(A) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A) and C.</param>
        /// <param name="n">number of columns of matrix op(B) and C.</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">array of dimensions lda * k.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A.</param>
        /// <param name="B">array of dimensions ldb * n.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B.</param>
        /// <param name="beta">scalar used for multiplication.</param>
        /// <param name="C">array of dimensions ldb * n.</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store matrix C.</param>
        public void Geam(Operation transa, Operation transb, long m, long n, CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> A, long lda,
            CudaDeviceVariable<double> B, long ldb, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDgeam_64(_blasHandle, transa, transb, m, n, alpha.DevicePointer, A.DevicePointer, lda, beta.DevicePointer,
                B.DevicePointer, ldb, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgeam_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication C = A x diag(X) if mode == CUBLAS_SIDE_RIGHT, or 
        /// C = diag(X) x A if mode == CUBLAS_SIDE_LEFT.<para/>
        /// where A and C are matrices stored in column-major format with dimensions m*n. X is a
        /// vector of size n if mode == CUBLAS_SIDE_RIGHT and of size m if mode ==
        /// CUBLAS_SIDE_LEFT. X is gathered from one-dimensional array x with stride incx. The
        /// absolute value of incx is the stride and the sign of incx is direction of the stride. If incx
        /// is positive, then we forward x from the first element. Otherwise, we backward x from the
        /// last element.
        /// </summary>
        /// <param name="mode">left multiply if mode == CUBLAS_SIDE_LEFT 
        /// or right multiply if mode == CUBLAS_SIDE_RIGHT</param>
        /// <param name="m">number of rows of matrix A and C.</param>
        /// <param name="n">number of columns of matrix A and C.</param>
        /// <param name="A">array of dimensions lda x n with lda >= max(1,m)</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store the matrix A.</param>
        /// <param name="X">one-dimensional array of size |incx|*m 
        /// if mode == CUBLAS_SIDE_LEFT and |incx|*n
        /// if mode == CUBLAS_SIDE_RIGHT</param>
        /// <param name="incx">stride of one-dimensional array x.</param>
        /// <param name="C">array of dimensions ldc*n with ldc >= max(1,m).</param>
        /// <param name="ldc">leading dimension of a two-dimensional array used to store the matrix C.</param>
        public void Dgmm(SideMode mode, long m, long n, CudaDeviceVariable<float> A, long lda,
            CudaDeviceVariable<float> X, long incx, CudaDeviceVariable<float> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasSdgmm_64(_blasHandle, mode, m, n, A.DevicePointer, lda,
                X.DevicePointer, incx, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSdgmm_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function performs the matrix-matrix multiplication C = A x diag(X) if mode == CUBLAS_SIDE_RIGHT, or 
        /// C = diag(X) x A if mode == CUBLAS_SIDE_LEFT.<para/>
        /// where A and C are matrices stored in column-major format with dimensions m*n. X is a
        /// vector of size n if mode == CUBLAS_SIDE_RIGHT and of size m if mode ==
        /// CUBLAS_SIDE_LEFT. X is gathered from one-dimensional array x with stride incx. The
        /// absolute value of incx is the stride and the sign of incx is direction of the stride. If incx
        /// is positive, then we forward x from the first element. Otherwise, we backward x from the
        /// last element.
        /// </summary>
        /// <param name="mode">left multiply if mode == CUBLAS_SIDE_LEFT 
        /// or right multiply if mode == CUBLAS_SIDE_RIGHT</param>
        /// <param name="m">number of rows of matrix A and C.</param>
        /// <param name="n">number of columns of matrix A and C.</param>
        /// <param name="A">array of dimensions lda x n with lda >= max(1,m)</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store the matrix A.</param>
        /// <param name="X">one-dimensional array of size |incx|*m 
        /// if mode == CUBLAS_SIDE_LEFT and |incx|*n
        /// if mode == CUBLAS_SIDE_RIGHT</param>
        /// <param name="incx">stride of one-dimensional array x.</param>
        /// <param name="C">array of dimensions ldc*n with ldc >= max(1,m).</param>
        /// <param name="ldc">leading dimension of a two-dimensional array used to store the matrix C.</param>
        public void Dgmm(SideMode mode, long m, long n, CudaDeviceVariable<double> A, long lda,
            CudaDeviceVariable<double> X, long incx, CudaDeviceVariable<double> C, long ldc)
        {
            _status = CudaBlasNativeMethods.cublasDdgmm_64(_blasHandle, mode, m, n, A.DevicePointer, lda,
                X.DevicePointer, incx, C.DevicePointer, ldc);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDdgmm_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<half> alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, long lda, CudaDeviceVariable<CUdeviceptr> Barray, long ldb,
                                   CudaDeviceVariable<half> beta, CudaDeviceVariable<CUdeviceptr> Carray, long ldc, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasHgemmBatched_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, beta.DevicePointer, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasHgemmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<float> alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, long lda, CudaDeviceVariable<CUdeviceptr> Barray, long ldb,
                                   CudaDeviceVariable<float> beta, CudaDeviceVariable<CUdeviceptr> Carray, long ldc, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasSgemmBatched_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, beta.DevicePointer, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        /// <param name="algo"></param>
        /// <param name="Atype"></param>
        /// <param name="Btype"></param>
        /// <param name="computeType"></param>
        /// <param name="Ctype"></param>
        public void GemmBatched(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<float> alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, cudaDataType Atype, long lda, CudaDeviceVariable<CUdeviceptr> Barray, cudaDataType Btype, long ldb,
                                   CudaDeviceVariable<float> beta, CudaDeviceVariable<CUdeviceptr> Carray, cudaDataType Ctype, long ldc, long batchCount, ComputeType computeType, GemmAlgo algo)
        {
            _status = CudaBlasNativeMethods.cublasGemmBatchedEx_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, Aarray.DevicePointer, Atype, lda, Barray.DevicePointer, Btype, ldb, beta.DevicePointer, Carray.DevicePointer, Ctype, ldc, batchCount, computeType, algo);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGemmBatchedEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }



        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to <![CDATA[<Atype>]]> matrix, A, corresponds to the first instance of the batch of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="strideA">value of type long long long that gives the address offset between A[i] and A[i+1]. </param>
        /// <param name="strideB">value of type long long long that gives the address offset between B[i] and B[i+1]. </param>
        /// <param name="strideC">value of type long long long that gives the address offset between C[i] and C[i+1]. </param>
        /// <param name="B">pointer to <![CDATA[<Btype>]]> matrix, A, corresponds to the first instance of the batch of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to <![CDATA[<Ctype>]]> matrix, A, corresponds to the first instance of the batch. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        /// <param name="algo"></param>
        /// <param name="Atype"></param>
        /// <param name="Btype"></param>
        /// <param name="computeType"></param>
        /// <param name="Ctype"></param>
        public void GemmStridedBatched(Operation transa, Operation transb, long m, long n, long k, CUdeviceptr alpha,
                                   CUdeviceptr A, cudaDataType Atype, long lda, long strideA, CUdeviceptr B, cudaDataType Btype, long ldb, long strideB,
                                   CUdeviceptr beta, CUdeviceptr C, cudaDataType Ctype, long ldc, long strideC, long batchCount, ComputeType computeType, GemmAlgo algo)
        {
            _status = CudaBlasNativeMethods.cublasGemmStridedBatchedEx_64(_blasHandle, transa, transb, m, n, k, alpha, A,
                Atype, lda, strideA, B, Btype, ldb, strideB, beta, C, Ctype, ldc, strideC, batchCount, computeType, algo);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGemmStridedBatchedEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<double> alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, long lda, CudaDeviceVariable<CUdeviceptr> Barray, long ldb, CudaDeviceVariable<double> beta,
                                   CudaDeviceVariable<CUdeviceptr> Carray, long ldc, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDgemmBatched_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, beta.DevicePointer, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }



        /// <summary>
        /// This function performs the matrix-matrix multiplication of a batch of matrices. The batch is considered to be "uniform", 
        /// i.e. all instances have the same dimensions (m, n, k), leading dimensions (lda, ldb, ldc) and transpositions (transa, transb) 
        /// for their respective A, B and C matrices. Input matrices A, B and output matrix C for each instance of the batch are located 
        /// at fixed address offsets from their locations in the previous instance. Pointers to A, B and C matrices for the first 
        /// instance are passed to the function by the user along with the address offsets - strideA, strideB and strideC that determine 
        /// the locations of input and output matrices in future instances. 
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose. </param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose. </param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i]. </param>
        /// <param name="n">number of columns of op(B[i]) and C[i]. </param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to the A matrix corresponding to the first instance of the batch, with dimensions lda x k with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise. </param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i]. </param>
        /// <param name="strideA">Value of type long long long that gives the address offset between A[i] and A[i+1]</param>
        /// <param name="B">pointer to the B matrix corresponding to the first instance of the batch, with dimensions ldb x n with ldb>=max(1,k) if transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise. </param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i]. </param>
        /// <param name="strideB">Value of type long long long that gives the address offset between B[i] and B[i+1]</param>
        /// <param name="beta"> scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to the C matrix corresponding to the first instance of the batch, with dimensions ldc x n with ldc>=max(1,m). </param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i]. </param>
        /// <param name="strideC">Value of type long long long that gives the address offset between C[i] and C[i+1]</param>
        /// <param name="batchCount">number of GEMMs to perform in the batch.</param>
        public void GemmStridedBatched(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<float> alpha,
            CudaDeviceVariable<float> A, long lda, long strideA, CudaDeviceVariable<float> B, long ldb, long strideB,
            CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, long ldc, long strideC, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasSgemmStridedBatched_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, strideA,
                B.DevicePointer, ldb, strideB, beta.DevicePointer, C.DevicePointer, ldc, strideC, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmStridedBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplication of a batch of matrices. The batch is considered to be "uniform", 
        /// i.e. all instances have the same dimensions (m, n, k), leading dimensions (lda, ldb, ldc) and transpositions (transa, transb) 
        /// for their respective A, B and C matrices. Input matrices A, B and output matrix C for each instance of the batch are located 
        /// at fixed address offsets from their locations in the previous instance. Pointers to A, B and C matrices for the first 
        /// instance are passed to the function by the user along with the address offsets - strideA, strideB and strideC that determine 
        /// the locations of input and output matrices in future instances. 
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose. </param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose. </param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i]. </param>
        /// <param name="n">number of columns of op(B[i]) and C[i]. </param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to the A matrix corresponding to the first instance of the batch, with dimensions lda x k with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise. </param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i]. </param>
        /// <param name="strideA">Value of type long long long that gives the address offset between A[i] and A[i+1]</param>
        /// <param name="B">pointer to the B matrix corresponding to the first instance of the batch, with dimensions ldb x n with ldb>=max(1,k) if transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise. </param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i]. </param>
        /// <param name="strideB">Value of type long long long that gives the address offset between B[i] and B[i+1]</param>
        /// <param name="beta"> scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to the C matrix corresponding to the first instance of the batch, with dimensions ldc x n with ldc>=max(1,m). </param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i]. </param>
        /// <param name="strideC">Value of type long long long that gives the address offset between C[i] and C[i+1]</param>
        /// <param name="batchCount">number of GEMMs to perform in the batch.</param>
        public void GemmStridedBatched(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<double> alpha,
            CudaDeviceVariable<double> A, long lda, long strideA, CudaDeviceVariable<double> B, long ldb, long strideB,
            CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, long ldc, long strideC, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDgemmStridedBatched_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, strideA,
                B.DevicePointer, ldb, strideB, beta.DevicePointer, C.DevicePointer, ldc, strideC, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemmStridedBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function performs the matrix-matrix multiplication of a batch of matrices. The batch is considered to be "uniform", 
        /// i.e. all instances have the same dimensions (m, n, k), leading dimensions (lda, ldb, ldc) and transpositions (transa, transb) 
        /// for their respective A, B and C matrices. Input matrices A, B and output matrix C for each instance of the batch are located 
        /// at fixed address offsets from their locations in the previous instance. Pointers to A, B and C matrices for the first 
        /// instance are passed to the function by the user along with the address offsets - strideA, strideB and strideC that determine 
        /// the locations of input and output matrices in future instances. 
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose. </param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose. </param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i]. </param>
        /// <param name="n">number of columns of op(B[i]) and C[i]. </param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to the A matrix corresponding to the first instance of the batch, with dimensions lda x k with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise. </param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i]. </param>
        /// <param name="strideA">Value of type long long long that gives the address offset between A[i] and A[i+1]</param>
        /// <param name="B">pointer to the B matrix corresponding to the first instance of the batch, with dimensions ldb x n with ldb>=max(1,k) if transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise. </param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i]. </param>
        /// <param name="strideB">Value of type long long long that gives the address offset between B[i] and B[i+1]</param>
        /// <param name="beta"> scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to the C matrix corresponding to the first instance of the batch, with dimensions ldc x n with ldc>=max(1,m). </param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i]. </param>
        /// <param name="strideC">Value of type long long long that gives the address offset between C[i] and C[i+1]</param>
        /// <param name="batchCount">number of GEMMs to perform in the batch.</param>
        public void GemmStridedBatched(Operation transa, Operation transb, long m, long n, long k, CudaDeviceVariable<half> alpha,
            CudaDeviceVariable<half> A, long lda, long strideA, CudaDeviceVariable<half> B, long ldb, long strideB,
            CudaDeviceVariable<half> beta, CudaDeviceVariable<half> C, long ldc, long strideC, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasHgemmStridedBatched_64(_blasHandle, transa, transb, m, n, k, alpha.DevicePointer, A.DevicePointer, lda, strideA,
                B.DevicePointer, ldb, strideB, beta.DevicePointer, C.DevicePointer, ldc, strideC, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmHtridedBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, long m, long n, long k, half alpha, CudaDeviceVariable<CUdeviceptr> Aarray,
            long lda, CudaDeviceVariable<CUdeviceptr> Barray, long ldb, half beta, CudaDeviceVariable<CUdeviceptr> Carray, long ldc, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasHgemmBatched_64(_blasHandle, transa, transb, m, n, k, ref alpha, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, ref beta, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasHgemmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        /// <param name="algo"></param>
        /// <param name="Atype"></param>
        /// <param name="Btype"></param>
        /// <param name="computeType"></param>
        /// <param name="Ctype"></param>
        public void GemmBatched(Operation transa, Operation transb, long m, long n, long k, IntPtr alpha,
                                   CudaDeviceVariable<CUdeviceptr> Aarray, cudaDataType Atype, long lda, CudaDeviceVariable<CUdeviceptr> Barray, cudaDataType Btype, long ldb,
                                   IntPtr beta, CudaDeviceVariable<CUdeviceptr> Carray, cudaDataType Ctype, long ldc, long batchCount, cudaDataType computeType, GemmAlgo algo)
        {
            _status = CudaBlasNativeMethods.cublasGemmBatchedEx_64(_blasHandle, transa, transb, m, n, k, alpha, Aarray.DevicePointer, Atype, lda, Barray.DevicePointer, Btype, ldb, beta, Carray.DevicePointer, Ctype, ldc, batchCount, computeType, algo);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGemmBatchedEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="A">pointer to <![CDATA[<Atype>]]> matrix, A, corresponds to the first instance of the batch of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="strideA">value of type long long long that gives the address offset between A[i] and A[i+1]. </param>
        /// <param name="strideB">value of type long long long that gives the address offset between B[i] and B[i+1]. </param>
        /// <param name="strideC">value of type long long long that gives the address offset between C[i] and C[i+1]. </param>
        /// <param name="B">pointer to <![CDATA[<Btype>]]> matrix, A, corresponds to the first instance of the batch of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="C">pointer to <![CDATA[<Ctype>]]> matrix, A, corresponds to the first instance of the batch. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        /// <param name="algo"></param>
        /// <param name="Atype"></param>
        /// <param name="Btype"></param>
        /// <param name="computeType"></param>
        /// <param name="Ctype"></param>
        public void GemmStridedBatched(Operation transa, Operation transb, long m, long n, long k, IntPtr alpha,
                                   CUdeviceptr A, cudaDataType Atype, long lda, long strideA, CUdeviceptr B, cudaDataType Btype, long ldb, long strideB,
                                   IntPtr beta, CUdeviceptr C, cudaDataType Ctype, long ldc, long strideC, long batchCount, cudaDataType computeType, GemmAlgo algo)
        {
            _status = CudaBlasNativeMethods.cublasGemmStridedBatchedEx_64(_blasHandle, transa, transb, m, n, k, alpha, A,
                Atype, lda, strideA, B, Btype, ldb, strideB, beta, C, Ctype, ldc, strideC, batchCount, computeType, algo);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGemmBatchedEx_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, long m, long n, long k, float alpha, CudaDeviceVariable<CUdeviceptr> Aarray,
            long lda, CudaDeviceVariable<CUdeviceptr> Barray, long ldb, float beta, CudaDeviceVariable<CUdeviceptr> Carray, long ldc, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasSgemmBatched_64(_blasHandle, transa, transb, m, n, k, ref alpha, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, ref beta, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSgemmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }


        /// <summary>
        /// This function performs the matrix-matrix multiplications of an array of matrices.
        /// where and are scalars, and , and are arrays of pointers to matrices stored
        /// in column-major format with dimensions op(A[i])m x k, op(B[i])k x n and op(C[i])m x n, 
        /// respectively.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. For small sizes, typically smaller than 100x100,
        /// this function improves significantly performance compared to making calls to its
        /// corresponding cublas<![CDATA[<type>]]>gemm routine. However, on GPU architectures that support
        /// concurrent kernels, it might be advantageous to make multiple calls to cublas<![CDATA[<type>]]>gemm
        /// into different streams as the matrix sizes increase.
        /// </summary>
        /// <param name="transa">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="transb">operation op(B[i]) that is non- or (conj.) transpose.</param>
        /// <param name="m">number of rows of matrix op(A[i]) and C[i].</param>
        /// <param name="n">number of columns of op(B[i]) and C[i].</param>
        /// <param name="k">number of columns of op(A[i]) and rows of op(B[i]).</param>
        /// <param name="alpha">scalar used for multiplication.</param>
        /// <param name="Aarray">array of device pointers, with each array/device pointer of dim. lda x k with lda>=max(1,m) if
        /// transa==CUBLAS_OP_N and lda x m with lda>=max(1,k) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store each matrix A[i].</param>
        /// <param name="Barray">array of device pointers, with each array of dim. ldb x n with ldb>=max(1,k) if
        /// transa==CUBLAS_OP_N and ldb x k with ldb>=max(1,n) max(1,) otherwise.</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store each matrix B[i].</param>
        /// <param name="beta">scalar used for multiplication. If beta == 0, C does not have to be a valid input.</param>
        /// <param name="Carray">array of device pointers. It has dimensions ldc x n with ldc>=max(1,m).</param>
        /// <param name="ldc">leading dimension of two-dimensional array used to store each matrix C[i].</param>
        /// <param name="batchCount">number of pointers contained in A, B and C.</param>
        public void GemmBatched(Operation transa, Operation transb, long m, long n, long k, double alpha,
            CudaDeviceVariable<CUdeviceptr> Aarray, long lda, CudaDeviceVariable<CUdeviceptr> Barray, long ldb, double beta,
            CudaDeviceVariable<CUdeviceptr> Carray, long ldc, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDgemmBatched_64(_blasHandle, transa, transb, m, n, k, ref alpha, Aarray.DevicePointer, lda, Barray.DevicePointer, ldb, ref beta, Carray.DevicePointer, ldc, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDgemmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }
        /// <summary>
        /// This function solves an array of triangular linear systems with multiple right-hand-sides.<para/>
        /// The solution overwrites the right-hand-sides on exit.<para/>
        /// No test for singularity or near-singularity is included in this function.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimensions m and n to 32.
        /// </summary>
        /// <param name="side">indicates if matrix A[i] is on the left or right of X[i].</param>
        /// <param name="uplo">indicates if matrix A[i] lower or upper part is stored, the
        /// other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix
        /// A[i] are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B[i], with matrix A[i] sized accordingly.</param>
        /// <param name="n">number of columns of matrix B[i], with matrix A[i] is sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication, if alpha==0 then A[i] is not 
        /// referenced and B[i] does not have to be a valid input.</param>
        /// <param name="A">array of device pointers with each array/device pointerarray 
        /// of dim. lda x m with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x n with
        /// lda>=max(1,n) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A[i].</param>
        /// <param name="B">array of device pointers with each array/device pointerarrayof dim.
        /// ldb x n with ldb>=max(1,m)</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B[i].</param>
        /// <param name="batchCount"></param>
        public void TrsmBatched(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, CudaDeviceVariable<float> alpha,
            CudaDeviceVariable<CUdeviceptr> A, long lda, CudaDeviceVariable<CUdeviceptr> B, long ldb, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasStrsmBatched_64(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer,
                A.DevicePointer, lda, B.DevicePointer, ldb, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves an array of triangular linear systems with multiple right-hand-sides.<para/>
        /// The solution overwrites the right-hand-sides on exit.<para/>
        /// No test for singularity or near-singularity is included in this function.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimensions m and n to 32.
        /// </summary>
        /// <param name="side">indicates if matrix A[i] is on the left or right of X[i].</param>
        /// <param name="uplo">indicates if matrix A[i] lower or upper part is stored, the
        /// other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix
        /// A[i] are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B[i], with matrix A[i] sized accordingly.</param>
        /// <param name="n">number of columns of matrix B[i], with matrix A[i] is sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication, if alpha==0 then A[i] is not 
        /// referenced and B[i] does not have to be a valid input.</param>
        /// <param name="A">array of device pointers with each array/device pointerarray 
        /// of dim. lda x m with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x n with
        /// lda>=max(1,n) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A[i].</param>
        /// <param name="B">array of device pointers with each array/device pointerarrayof dim.
        /// ldb x n with ldb>=max(1,m)</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B[i].</param>
        /// <param name="batchCount"></param>
        public void TrsmBatched(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, CudaDeviceVariable<double> alpha,
            CudaDeviceVariable<CUdeviceptr> A, long lda, CudaDeviceVariable<CUdeviceptr> B, long ldb, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDtrsmBatched_64(_blasHandle, side, uplo, trans, diag, m, n, alpha.DevicePointer,
                A.DevicePointer, lda, B.DevicePointer, ldb, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves an array of triangular linear systems with multiple right-hand-sides.<para/>
        /// The solution overwrites the right-hand-sides on exit.<para/>
        /// No test for singularity or near-singularity is included in this function.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimensions m and n to 32.
        /// </summary>
        /// <param name="side">indicates if matrix A[i] is on the left or right of X[i].</param>
        /// <param name="uplo">indicates if matrix A[i] lower or upper part is stored, the
        /// other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix
        /// A[i] are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B[i], with matrix A[i] sized accordingly.</param>
        /// <param name="n">number of columns of matrix B[i], with matrix A[i] is sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication, if alpha==0 then A[i] is not 
        /// referenced and B[i] does not have to be a valid input.</param>
        /// <param name="A">array of device pointers with each array/device pointerarray 
        /// of dim. lda x m with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x n with
        /// lda>=max(1,n) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A[i].</param>
        /// <param name="B">array of device pointers with each array/device pointerarrayof dim.
        /// ldb x n with ldb>=max(1,m)</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B[i].</param>
        /// <param name="batchCount"></param>
        public void TrsmBatched(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, ref float alpha,
            CudaDeviceVariable<CUdeviceptr> A, long lda, CudaDeviceVariable<CUdeviceptr> B, long ldb, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasStrsmBatched_64(_blasHandle, side, uplo, trans, diag, m, n, ref alpha,
                A.DevicePointer, lda, B.DevicePointer, ldb, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasStrsmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// This function solves an array of triangular linear systems with multiple right-hand-sides.<para/>
        /// The solution overwrites the right-hand-sides on exit.<para/>
        /// No test for singularity or near-singularity is included in this function.<para/>
        /// This function is intended to be used for matrices of small sizes where the launch
        /// overhead is a significant factor. The current implementation limits the dimensions m and n to 32.
        /// </summary>
        /// <param name="side">indicates if matrix A[i] is on the left or right of X[i].</param>
        /// <param name="uplo">indicates if matrix A[i] lower or upper part is stored, the
        /// other part is not referenced and is inferred from the stored elements.</param>
        /// <param name="trans">operation op(A[i]) that is non- or (conj.) transpose.</param>
        /// <param name="diag">indicates if the elements on the main diagonal of matrix
        /// A[i] are unity and should not be accessed.</param>
        /// <param name="m">number of rows of matrix B[i], with matrix A[i] sized accordingly.</param>
        /// <param name="n">number of columns of matrix B[i], with matrix A[i] is sized accordingly.</param>
        /// <param name="alpha">scalar used for multiplication, if alpha==0 then A[i] is not 
        /// referenced and B[i] does not have to be a valid input.</param>
        /// <param name="A">array of device pointers with each array/device pointerarray 
        /// of dim. lda x m with lda>=max(1,m) if transa==CUBLAS_OP_N and lda x n with
        /// lda>=max(1,n) otherwise.</param>
        /// <param name="lda">leading dimension of two-dimensional array used to store matrix A[i].</param>
        /// <param name="B">array of device pointers with each array/device pointerarrayof dim.
        /// ldb x n with ldb>=max(1,m)</param>
        /// <param name="ldb">leading dimension of two-dimensional array used to store matrix B[i].</param>
        /// <param name="batchCount"></param>
        public void TrsmBatched(SideMode side, FillMode uplo, Operation trans, DiagType diag, long m, long n, ref double alpha,
            CudaDeviceVariable<CUdeviceptr> A, long lda, CudaDeviceVariable<CUdeviceptr> B, long ldb, long batchCount)
        {
            _status = CudaBlasNativeMethods.cublasDtrsmBatched_64(_blasHandle, side, uplo, trans, diag, m, n, ref alpha,
                A.DevicePointer, lda, B.DevicePointer, ldb, batchCount);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasDtrsmBatched_64", _status));
            if (_status != CublasStatus.Success) throw new CudaBlasException(_status);
        }

        /// <summary>
        /// copies elements from a vector <c>hostSourceVector</c> in CPU memory space to a vector <c>devDestVector</c> 
        /// in GPU memory space. Storage spacing between consecutive elements
        /// is <c>incrHostSource</c> for the source vector <c>hostSourceVector</c> and <c>incrDevDest</c> for the destination vector
        /// <c>devDestVector</c>. Column major format for two-dimensional matrices
        /// is assumed throughout CUBLAS. Therefore, if the increment for a vector 
        /// is equal to 1, this access a column vector while using an increment 
        /// equal to the leading dimension of the respective matrix accesses a 
        /// row vector.
        /// </summary>
        /// <typeparam name="T">Vector datatype </typeparam>
        /// <param name="hostSourceVector">Source vector in host memory</param>
        /// <param name="incrHostSource"></param>
        /// <param name="devDestVector">Destination vector in device memory</param>
        /// <param name="incrDevDest"></param>
        public static void SetVector<T>(T[] hostSourceVector, long incrHostSource, CudaDeviceVariable<T> devDestVector, long incrDevDest) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostSourceVector, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasSetVector_64(hostSourceVector.Length, devDestVector.TypeSize, handle.AddrOfPinnedObject(), incrHostSource, devDestVector.DevicePointer, incrDevDest);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetVector_64", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies elements from a vector <c>devSourceVector</c> in GPU memory space to a vector <c>hostDestVector</c> 
        /// in CPU memory space. Storage spacing between consecutive elements
        /// is <c>incrHostDest</c> for the source vector <c>devSourceVector</c> and <c>incrDevSource</c> for the destination vector
        /// <c>hostDestVector</c>. Column major format for two-dimensional matrices
        /// is assumed throughout CUBLAS. Therefore, if the increment for a vector 
        /// is equal to 1, this access a column vector while using an increment 
        /// equal to the leading dimension of the respective matrix accesses a 
        /// row vector.
        /// </summary>
        /// <typeparam name="T">Vector datatype</typeparam>
        /// <param name="devSourceVector">Source vector in device memory</param>
        /// <param name="incrDevSource"></param>
        /// <param name="hostDestVector">Destination vector in host memory</param>
        /// <param name="incrHostDest"></param>
        public static void GetVector<T>(CudaDeviceVariable<T> devSourceVector, long incrDevSource, T[] hostDestVector, long incrHostDest) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostDestVector, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasGetVector_64(hostDestVector.Length, devSourceVector.TypeSize, devSourceVector.DevicePointer, incrDevSource, handle.AddrOfPinnedObject(), incrHostDest);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetVector_64", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies a tile of <c>rows</c> x <c>cols</c> elements from a matrix <c>hostSource</c> in CPU memory
        /// space to a matrix <c>devDest</c> in GPU memory space. Both matrices are assumed to be stored in column 
        /// major format, with the leading dimension (i.e. number of rows) of 
        /// source matrix <c>hostSource</c> provided in <c>ldHostSource</c>, and the leading dimension of matrix <c>devDest</c>
        /// provided in <c>ldDevDest</c>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="hostSource"></param>
        /// <param name="ldHostSource"></param>
        /// <param name="devDest"></param>
        /// <param name="ldDevDest"></param>
        public static void SetMatrix<T>(long rows, long cols, T[] hostSource, long ldHostSource, CudaDeviceVariable<T> devDest, long ldDevDest) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostSource, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasSetMatrix_64(rows, cols, devDest.TypeSize, handle.AddrOfPinnedObject(), ldHostSource, devDest.DevicePointer, ldDevDest);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetMatrix_64", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies a tile of <c>rows</c> x <c>cols</c> elements from a matrix <c>devSource</c> in GPU memory
        /// space to a matrix <c>hostDest</c> in CPU memory space. Both matrices are assumed to be stored in column 
        /// major format, with the leading dimension (i.e. number of rows) of 
        /// source matrix <c>devSource</c> provided in <c>devSource</c>, and the leading dimension of matrix <c>hostDest</c>
        /// provided in <c>ldHostDest</c>. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="devSource"></param>
        /// <param name="ldDevSource"></param>
        /// <param name="hostDest"></param>
        /// <param name="ldHostDest"></param>
        public static void GetMatrix<T>(long rows, long cols, CudaDeviceVariable<T> devSource, long ldDevSource, T[] hostDest, long ldHostDest) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostDest, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasGetMatrix_64(rows, cols, devSource.TypeSize, devSource.DevicePointer, ldDevSource, handle.AddrOfPinnedObject(), ldHostDest);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetMatrix_64", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies elements from a vector <c>hostSourceVector</c> in CPU memory space to a vector <c>devDestVector</c> 
        /// in GPU memory space. Storage spacing between consecutive elements
        /// is <c>incrHostSource</c> for the source vector <c>hostSourceVector</c> and <c>incrDevDest</c> for the destination vector
        /// <c>devDestVector</c>. Column major format for two-dimensional matrices
        /// is assumed throughout CUBLAS. Therefore, if the increment for a vector 
        /// is equal to 1, this access a column vector while using an increment 
        /// equal to the leading dimension of the respective matrix accesses a 
        /// row vector.
        /// </summary>
        /// <typeparam name="T">Vector datatype </typeparam>
        /// <param name="hostSourceVector">Source vector in host memory</param>
        /// <param name="incrHostSource"></param>
        /// <param name="devDestVector">Destination vector in device memory</param>
        /// <param name="incrDevDest"></param>
        /// <param name="stream"></param>
        public static void SetVectorAsync<T>(T[] hostSourceVector, long incrHostSource, CudaDeviceVariable<T> devDestVector, long incrDevDest, CUstream stream) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostSourceVector, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasSetVectorAsync_64(hostSourceVector.Length, devDestVector.TypeSize, handle.AddrOfPinnedObject(), incrHostSource, devDestVector.DevicePointer, incrDevDest, stream);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetVectorAsync_64", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies elements from a vector <c>devSourceVector</c> in GPU memory space to a vector <c>hostDestVector</c> 
        /// in CPU memory space. Storage spacing between consecutive elements
        /// is <c>incrHostDest</c> for the source vector <c>devSourceVector</c> and <c>incrDevSource</c> for the destination vector
        /// <c>hostDestVector</c>. Column major format for two-dimensional matrices
        /// is assumed throughout CUBLAS. Therefore, if the increment for a vector 
        /// is equal to 1, this access a column vector while using an increment 
        /// equal to the leading dimension of the respective matrix accesses a 
        /// row vector.
        /// </summary>
        /// <typeparam name="T">Vector datatype</typeparam>
        /// <param name="devSourceVector">Source vector in device memory</param>
        /// <param name="incrDevSource"></param>
        /// <param name="hostDestVector">Destination vector in host memory</param>
        /// <param name="incrHostDest"></param>
        /// <param name="stream"></param>
        public static void GetVectorAsync<T>(CudaDeviceVariable<T> devSourceVector, long incrDevSource, T[] hostDestVector, long incrHostDest, CUstream stream) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostDestVector, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasGetVectorAsync_64(hostDestVector.Length, devSourceVector.TypeSize, devSourceVector.DevicePointer, incrDevSource, handle.AddrOfPinnedObject(), incrHostDest, stream);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetVectorAsync_64", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies a tile of <c>rows</c> x <c>cols</c> elements from a matrix <c>hostSource</c> in CPU memory
        /// space to a matrix <c>devDest</c> in GPU memory space. Both matrices are assumed to be stored in column 
        /// major format, with the leading dimension (i.e. number of rows) of 
        /// source matrix <c>hostSource</c> provided in <c>ldHostSource</c>, and the leading dimension of matrix <c>devDest</c>
        /// provided in <c>ldDevDest</c>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="hostSource"></param>
        /// <param name="ldHostSource"></param>
        /// <param name="devDest"></param>
        /// <param name="ldDevDest"></param>
        /// <param name="stream"></param>
        public static void SetMatrixAsync<T>(long rows, long cols, T[] hostSource, long ldHostSource, CudaDeviceVariable<T> devDest, long ldDevDest, CUstream stream) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostSource, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasSetMatrixAsync_64(rows, cols, devDest.TypeSize, handle.AddrOfPinnedObject(), ldHostSource, devDest.DevicePointer, ldDevDest, stream);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasSetMatrixAsync_64", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }

        /// <summary>
        /// copies a tile of <c>rows</c> x <c>cols</c> elements from a matrix <c>devSource</c> in GPU memory
        /// space to a matrix <c>hostDest</c> in CPU memory space. Both matrices are assumed to be stored in column 
        /// major format, with the leading dimension (i.e. number of rows) of 
        /// source matrix <c>devSource</c> provided in <c>devSource</c>, and the leading dimension of matrix <c>hostDest</c>
        /// provided in <c>ldHostDest</c>. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="devSource"></param>
        /// <param name="ldDevSource"></param>
        /// <param name="hostDest"></param>
        /// <param name="ldHostDest"></param>
        /// <param name="stream"></param>
        public static void GetMatrixAsync<T>(long rows, long cols, CudaDeviceVariable<T> devSource, long ldDevSource, T[] hostDest, long ldHostDest, CUstream stream) where T : struct
        {
            CublasStatus status;
            GCHandle handle = GCHandle.Alloc(hostDest, GCHandleType.Pinned);
            status = CudaBlasNativeMethods.cublasGetMatrixAsync_64(rows, cols, devSource.TypeSize, devSource.DevicePointer, ldDevSource, handle.AddrOfPinnedObject(), ldHostDest, stream);
            handle.Free();
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cublasGetMatrixAsync_64", status));
            if (status != CublasStatus.Success) throw new CudaBlasException(status);
        }
    }
}
