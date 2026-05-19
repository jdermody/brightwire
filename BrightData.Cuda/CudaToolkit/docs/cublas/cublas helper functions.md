## cublasCreate  

```c
cublasStatus_t cublasCreate(cublasHandle_t* handle);
```  

Creates a cuBLAS library context (handle) and allocates the internal resources required for all subsequent cuBLAS calls.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | host | output | Pointer to a variable that will receive the handle of the newly created cuBLAS context. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Creation succeeded. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | cuBLAS was not initialized (CUDA runtime failed). |
| `CUBLAS_STATUS_ALLOC_FAILED` | Failed to allocate internal resources. |
| `CUBLAS_STATUS_INVALID_VALUE` | `handle` is `NULL`. |

---  

## cublasDestroy  

```c
cublasStatus_t cublasDestroy(cublasHandle_t handle);
```  

Releases all resources allocated by `cublasCreate` for the given handle.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | Handle to be destroyed. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Shutdown succeeded. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library was not initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | `handle` is `NULL`. |

---  

## cublasGetVersion  

```c
cublasStatus_t cublasGetVersion(cublasHandle_t handle, int* version);
```  

Returns the version number of the cuBLAS library.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `version`| host | output | Pointer to an `int` that receives the version. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Operation completed successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `version` is `NULL`. |

---  

## cublasGetProperty  

```c
cublasStatus_t cublasGetProperty(libraryPropertyTypetype type, int* value);
```  

Queries a library property (e.g., major version, minor version, patch level).  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `type` | host | input | Enumerated property identifier (see `libraryPropertyType_t`). |
| `value`| host | output | Pointer to an `int` that receives the property value. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Property retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | Invalid `type` or `value` is `NULL`. |

---  

## cublasGetStatusName  

```c
const char* cublasGetStatusName(cublasStatus_t status);
```  

Returns a NULL‑terminated string representing a given status code.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `status` | host | input | Status code to be translated. |

### Return Values  

| Parameter | Meaning |
|-----------|---------|
| `NULL` terminated string | Human‑readable description of `status`. |

---  

## cublasGetStatusString  

```c
const char* cublasGetStatusString(cublasStatus_t status);
```  

Returns a descriptive message for a given status code.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `status` | host | input | Status code to be described. |

### Return Values  

| Parameter | Meaning |
|-----------|---------|
| `NULL` terminated string | Description of `status`. |

---  

## cublasSetStream  

```c
cublasStatus_t cublasSetStream(cublasHandle_t handle, cudaStream_t streamId);
```  

Sets the stream that will be used by all subsequent cuBLAS calls made with `handle`.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `streamId`| device | input | CUDA stream identifier to use. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Stream set successfully. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | `streamId` is `NULL`. |

---  

## cublasSetWorkspace  

```c
cublasStatus_t cublasSetWorkspace(cublasHandle_t handle, void* workspace,
                                  size_t workspaceSizeInBytes);
```  

Assigns a user‑owned device buffer to be used as the cuBLAS workspace for the given handle.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `workspace` | device | input | Pointer to the workspace buffer (must be 256‑byte aligned). |
| `workspaceSizeInBytes` | host | input | Size of the workspace buffer in bytes. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Workspace set successfully. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | Buffer not 256‑byte aligned or size too small. |

---  

## cublasGetStream  

```c
cublasStatus_t cublasGetStream(cublasHandle_t handle, cudaStream_t* streamId);
```  

Queries the currently set stream for the given handle.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `streamId`| host | output | Pointer to a `cudaStream_t` that receives the stream identifier. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Stream retrieved successfully. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | `streamId` is `NULL`. |

---  

## cublasGetPointerMode  

```c
cublasStatus_t cublasGetPointerMode(cublasHandle_t handle,
                                    cublasPointerMode_t* mode);
```  

Retrieves the current pointer‑mode used by the cuBLAS library.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mode` | host | output | Pointer to a `cublasPointerMode_t` that receives the mode. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Mode retrieved successfully. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mode` is `NULL`. |

---  

## cublasSetPointerMode  

```c
cublasStatus_t cublasSetPointerMode(cublasHandle_t handle,
                                    cublasPointerMode_t mode);
```  

Sets the pointer‑mode used by the cuBLAS library.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mode` | host | input | Desired pointer mode (`CUBLAS_POINTER_MODE_HOST` or `CUBLAS_POINTER_MODE_DEVICE`). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Mode set successfully. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mode` is not a valid enum value. |

---  

## cublasSetVector  

```c
cublasStatus_t cublasSetVector(int n, int elemSize, const void* x,
                               int incx, void* y, int incy);
```  

Copies a vector `x` from host memory to a vector `y` in GPU memory (or vice‑versa).  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `n` | host | input | Number of elements to copy. |
| `elemSize` | host | input | Size of each element (bytes). |
| `x` | host | input | Source vector pointer. |
| `incx` | host | input | Stride between consecutive elements of `x`. |
| `y` | device | output | Destination vector pointer. |
| `incy` | host | input | Stride between consecutive elements of `y`. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Copy succeeded. |
| `CUBLAS_STATUS_INVALID_VALUE` | Any of `n`, `elemSize`, `incx`, `incy` not positive. |
| `CUBLAS_STATUS_MAPPING_ERROR` | Failed to access GPU memory. |

---  

## cublasGetVector  

```c
cublasStatus_t cublasGetVector(int n, int elemSize, const void* x,
                               int incx, void* y, int incy);
```  

Copies a vector `x` from GPU memory to host memory (or vice‑versa).  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `n` | host | input | Number of elements. |
| `elemSize` | host | input | Size of each element (bytes). |
| `x` | device | input | Source vector pointer (GPU). |
| `incx` | host | input | Stride of source vector. |
| `y` | host | output | Destination vector pointer (host). |
| `incy` | host | input | Stride of destination vector. |

### Return Values  

Same as `cublasSetVector`.  

---  

## cublasSetMatrix  

```c
cublasStatus_t cublasSetMatrix(int rows, int cols, int elemSize,
                               const void* A, int lda,
                               void* B, int ldb);
```  

Copies a matrix `A` from host memory to matrix `B` in GPU memory (or vice‑versa).  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `rows` | host | input | Number of rows of the matrix. |
| `cols` | host | input | Number of columns. |
| `elemSize` | host | input | Size of each element (bytes). |
| `A` | host | input | Source matrix pointer. |
| `lda` | host | input | Leading dimension of source matrix. |
| `B` | device | output | Destination matrix pointer. |
| `ldb` | host | input | Leading dimension of destination matrix. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Copy succeeded. |
| `CUBLAS_STATUS_INVALID_VALUE` | Negative dimensions or non‑positive `lda`, `ldb`. |
| `CUBLAS_STATUS_MAPPING_ERROR` | GPU memory mapping failed. |

---  

## cublasGetMatrix  

```c
cublasStatus_t cublasGetMatrix(int rows, int cols, int elemSize,
                               const void* A, int lda,
                               void* B, int ldb);
```  

Copies a matrix `A` from GPU memory to host memory (or vice‑versa).  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `rows` | host | input | Number of rows. |
| `cols` | host | input | Number of columns. |
| `elemSize` | host | input | Size of each element. |
| `A` | device | input | Source matrix pointer (GPU). |
| `lda` | host | input | Leading dimension of source matrix. |
| `B` | host | output | Destination matrix pointer (host). |
| `ldb` | host | input | Leading dimension of destination matrix. |

### Return Values  

Same as `cublasSetMatrix`.  

---  

## cublasSetVectorAsync  

```c
cublasStatus_t cublasSetVectorAsync(int n, int elemSize,
                                      const void* hostPtr, int incx,
                                      void* devicePtr, int incy,
                                      cudaStream_t stream);
```  

Asynchronously copies a vector from host to device (or reverse) using a supplied CUDA stream.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `n` | host | input | Number of elements. |
| `elemSize` | host | input | Size of each element. |
| `hostPtr` | host | input | Source vector pointer (host). |
| `incx` | host | input | Stride of source vector. |
| `devicePtr` | device | output | Destination vector pointer (device). |
| `incy` | host | input | Stride of destination vector. |
| `stream` | device | input | CUDA stream for asynchronous transfer. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Async copy succeeded. |
| `CUBLAS_STATUS_INVALID_VALUE` | Any stride/size not positive. |
| `CUBLAS_STATUS_MAPPING_ERROR` | GPU memory mapping failed. |

---  

## cublasGetVectorAsync  

```c
cublasStatus_t cublasGetVectorAsync(int n, int elemSize,
                                      const void* devicePtr, int incx,
                                      void* hostPtr, int incy,
                                      cudaStream_t stream);
```  

Asynchronously copies a vector from device to host (or reverse) using a supplied CUDA stream.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `n` | host | input | Number of elements. |
| `elemSize` | host | input | Size of each element. |
| `devicePtr` | device | input | Source vector pointer (device). |
| `incx` | host | input | Stride of source vector. |
| `hostPtr` | host | output | Destination vector pointer (host). |
| `incy` | host | input | Stride of destination vector. |
| `stream` | device | input | CUDA stream for async transfer. |

### Return Values  

Same as `cublasSetVectorAsync`.  

---  

## cublasSetMatrixAsync  

```c
cublasStatus_t cublasSetMatrixAsync(int rows, int cols, int elemSize,
                                      const void* hostPtr, int lda,
                                      void* devicePtr, int ldb,
                                      cudaStream_t stream);
```  

Asynchronously copies a matrix from host to device (or reverse) using a supplied CUDA stream.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `rows` | host | input | Number of rows. |
| `cols` | host | input | Number of columns. |
| `elemSize` | host | input | Size of each element. |
| `hostPtr` | host | input | Source matrix pointer (host). |
| `lda` | host | input | Leading dimension of source matrix. |
| `devicePtr` | device | output | Destination matrix pointer (device). |
| `ldb` | host | input | Leading dimension of destination matrix. |
| `stream` | device | input | CUDA stream for async transfer. |

### Return Values  

Same as `cublasSetVectorAsync`.  

---  

## cublasGetMatrixAsync  

```c
cublasStatus_t cublasGetMatrixAsync(int rows, int cols, int elemSize,
                                      const void* devicePtr, int lda,
                                      void* hostPtr, int ldb,
                                      cudaStream_t stream);
```  

Asynchronously copies a matrix from device to host (or reverse) using a supplied CUDA stream.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `rows` | host | input | Number of rows. |
| `cols` | host | input | Number of columns. |
| `elemSize` | host | input | Size of each element. |
| `devicePtr` | device | input | Source matrix pointer (device). |
| `lda` | host | input | Leading dimension of source matrix. |
| `hostPtr` | host | output | Destination matrix pointer (host). |
| `ldb` | host | input | Leading dimension of destination matrix. |
| `stream` | device | input | CUDA stream for async transfer. |

### Return Values  

Same as `cublasSetVectorAsync`.  

---  

## cublasSetAtomicsMode  

```c
cublasStatus_t cublasSetAtomicsMode(cublasHandle_t handle,
                                    cublasAtomicsMode_t mode);
```  

Enables or disables the use of atomic operations in cuBLAS kernels that have an alternate atomic implementation (e.g., `symv`, `hemv`).  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mode` | host | input | Desired atomic mode (`CUBLAS_ATOMICS_ALLOWED` or `CUBLAS_ATOMICS_NOT_ALLOWED`). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Mode set successfully. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mode` not one of the allowed enum values. |

---  

## cublasGetAtomicsMode  

```c
cublasStatus_t cublasGetAtomicsMode(cublasHandle_t handle,
                                    cublasAtomicsMode_t* mode);
```  

Queries the currently configured atomic mode for the given cuBLAS handle.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mode` | host | output | Pointer to a `cublasAtomicsMode_t` that receives the mode. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Mode queried successfully. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mode` is `NULL`. |

---  

## cublasSetMathMode  

```c
cublasStatus_t cublasSetMathMode(cublasHandle_t handle,
                                 cublasMath_t mode);
```  

Selects a math mode that controls precision, tensor‑core usage, and other numerical options.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mode` | host | input | Desired math mode (`CUBLAS_DEFAULT_MATH`, `CUBLAS_PEDANTIC_MATH`, `CUBLAS_TENSOR_OP_MATH`, etc.). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Math mode set successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mode` not a valid enum value. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasGetMathMode  

```c
cublasStatus_t cublasGetMathMode(cublasHandle_t handle,
                                 cublasMath_t* mode);
```  

Retrieves the currently active math mode for the given cuBLAS handle.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mode` | host | output | Pointer to a `cublasMath_t` that receives the mode. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Mode retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mode` is `NULL`. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasSetSmCountTarget  

```c
cublasStatus_t cublasSetSmCountTarget(cublasHandle_t handle,
                                      int smCountTarget);
```  

Overrides the number of multiprocessors (SMs) that cuBLAS may use for kernel execution.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `smCountTarget` | host | input | Desired number of SMs (0 = use default). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Target set successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | Value outside allowed range. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasGetSmCountTarget  

```c
cublasStatus_t cublasGetSmCountTarget(cublasHandle_t handle,
                                      int* smCountTarget);
```  

Queries the previously programmed SM count target.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `smCountTarget` | host | output | Pointer to an `int` that receives the target value. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Target retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `smCountTarget` is `NULL`. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasSetEmulationStrategy  

```c
cublasStatus_t cublasSetEmulationStrategy(cublasHandle_t handle,
                                          cublasEmulationStrategy_t emulationStrategy);
```  

Selects how floating‑point emulation algorithms are used.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `emulationStrategy` | host | input | Desired strategy (`CUBLAS_EMULATION_STRATEGY_DEFAULT`, `CUBLAS_EMULATION_STRATEGY_PERFORMANT`, `CUBLAS_EMULATION_STRATEGY_EAGER`). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Strategy set successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `emulationStrategy` not a valid enum value. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasGetEmulationStrategy  

```c
cublasStatus_t cublasGetEmulationStrategy(cublasHandle_t handle,
                                          cublasEmulationStrategy_t* emulationStrategy);
```  

Queries the currently configured emulation strategy.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `emulationStrategy` | host | output | Pointer to a `cublasEmulationStrategy_t` that receives the strategy. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Strategy retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `emulationStrategy` is `NULL`. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasGetEmulationSpecialValuesSupport  

```c
cublasStatus_t cublasGetEmulationSpecialValuesSupport(cublasHandle_t handle,
                                                      cudaEmulationSpecialValuesSupport* mask);
```  

Retrieves the special‑value support mask for floating‑point emulation.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mask` | device | output | Pointer to a `cudaEmulationSpecialValuesSupport_t` that receives the mask. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Mask retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mask` is `NULL`. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasSetEmulationSpecialValuesSupport  

```c
cublasStatus_t cublasSetEmulationSpecialValuesSupport(cublasHandle_t handle,
                                                      cudaEmulationSpecialValuesSupport mask);
```  

Sets the special‑value support mask for floating‑point emulation.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mask` | host | input | Desired mask (`CUDA_EMULATION_SPECIAL_VALUES_SUPPORT_*`). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Mask set successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mask` outside allowed range. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasGetFixedPointEmulationMantissaControl  

```c
cublasStatus_t cublasGetFixedPointEmulationMantissaControl(cublasHandle_t handle,
                                                         cudaEmulationMantissaControl* mantissaControl);
```  

Queries the current mantissa‑control setting for fixed‑point emulation.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mantissaControl` | device | output | Pointer to a `cudaEmulationMantissaControl_t`. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Control retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mantissaControl` is `NULL`. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasSetFixedPointEmulationMantissaControl  

```c
cublasStatus_t cublasSetFixedPointEmulationMantissaControl(cublasHandle_t handle,
                                                          cudaEmulationMantissaControl mantissaControl);
```  

Sets the mantissa‑control setting for fixed‑point emulation.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mantissaControl` | host | input | Desired control (`CUDA_EMULATION_MANTISSA_CONTROL_DYNAMIC` or `CUDA_EMULATION_MANTISSA_CONTROL_FIXED`). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Control set successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mantissaControl` outside allowed range. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasGetFixedPointEmulationMaxMantissaBitCount  

```c
cublasStatus_t cublasGetFixedPointEmulationMaxMantissaBitCount(cublasHandle_t handle,
                                                             int* maxMantissaBitCount);
```  

Queries the maximum mantissa‑bit count configured for fixed‑point emulation.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `maxMantissaBitCount` | host | output | Pointer to an `int` that receives the maximum bit count. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Count retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `maxMantissaBitCount` is `NULL`. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasSetFixedPointEmulationMaxMantissaBitCount  

```c
cublasStatus_t cublasSetFixedPointEmulationMaxMantissaBitCount(cublasHandle_t handle,
                                                             int maxMantissaBitCount);
```  

Sets the maximum mantissa‑bit count for fixed‑point emulation.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `maxMantissaBitCount` | host | input | Desired maximum bit count. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Count set successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | Value outside allowed range. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasGetFixedPointEmulationMantissaBitOffset  

```c
cublasStatus_t cublasGetFixedPointEmulationMantissaBitOffset(cublasHandle_t handle,
                                                           int* mantissaBitOffset);
```  

Queries the mantissa‑bit offset for fixed‑point emulation.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mantissaBitOffset` | host | output | Pointer to an `int` that receives the offset. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Offset retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mantissaBitOffset` is `NULL`. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasSetFixedPointEmulationMantissaBitOffset  

```c
cublasStatus_t cublasSetFixedPointEmulationMantissaBitOffset(cublasHandle_t handle,
                                                           int mantissaBitOffset);
```  

Sets the mantissa‑bit offset for fixed‑point emulation.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mantissaBitOffset` | host | input | Desired offset (integer). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Offset set successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mantissaBitOffset` outside allowed range. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasGetFixedPointEmulationMantissaBitCountPointer  

```c
cublasStatus_t cublasGetFixedPointEmulationMantissaBitCountPointer(cublasHandle_t handle,
                                                                 int** mantissaBitCount);
```  

Queries a device pointer that will receive the mantissa‑bit count.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mantissaBitCount` | device | output | Pointer to a device pointer (`int**`). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Pointer retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mantissaBitCount` is `NULL`. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasSetFixedPointEmulationMantissaBitCountPointer  

```c
cublasStatus_t cublasSetFixedPointEmulationMantissaBitCountPointer(cublasHandle_t handle,
                                                                 int* mantissaBitCount);
```  

Sets a device pointer that will hold the mantissa‑bit count.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `handle` | device | input | cuBLAS context handle. |
| `mantissaBitCount` | host | input | Pointer to an `int` that holds the count. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Pointer set successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `mantissaBitCount` is `NULL` or outside allowed range. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized. |

---  

## cublasLoggerConfigure  

```c
cublasStatus_t cublasLoggerConfigure(int logIsOn,
                                     int logToStdOut,
                                     int logToStdErr,
                                     const char* logFileName);
```  

Enables or configures runtime logging for cuBLAS.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `logIsOn` | host | input | Turn logging on (`1`) or off (`0`). |
| `logToStdOut` | host | input | Enable/disable logging to standard output. |
| `logToStdErr` | host | input | Enable/disable logging to standard error. |
| `logFileName` | host | input | File name for file‑based logging (`NULL` disables). |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Configuration succeeded. |
| `CUBLAS_STATUS_INVALID_VALUE` | Any parameter out of allowed range. |

---  

## cublasGetLoggerCallback  

```c
cublasStatus_t cublasGetLoggerCallback(cublasLogCallback* userCallback);
```  

Retrieves the currently installed custom logger callback function.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `userCallback` | host | output | Pointer to a `cublasLogCallback_t` that receives the callback. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Callback retrieved successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `userCallback` is `NULL`. |

---  

## cublasSetLoggerCallback  

```c
cublasStatus_t cublasSetLoggerCallback(cublasLogCallback userCallback);
```  

Installs a custom logger callback function that will be invoked during runtime.  

| Parameter | Memory | In/Out | Meaning |
|-----------|--------|--------|---------|
| `userCallback` | host | input | Pointer to the callback function to install. |

### Return Values  

| Error Value | Meaning |
|-------------|---------|
| `CUBLAS_STATUS_SUCCESS` | Callback installed successfully. |
| `CUBLAS_STATUS_INVALID_VALUE` | `userCallback` is `NULL`. |