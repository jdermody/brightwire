**Chapter 3 – Using the cuBLASLt API**

---  

## 3.1 General Description  

The cuBLASLt library is a new lightweight library dedicated to GEneral Matrix‑to‑matrix Multiply (GEMM)operations with a newflexible API. Thisnew library adds flexibility in matrix datalayouts, input types, compute types, and also in choosing the algorithmic implementations and heuristics through parameter programmability.  

Once a set of options for the intended GEMM operation are identified by the user, these options can be used repeatedly for different inputs. This is analogous to how cuFFT and FFTW first create a plan and reuse for same size and type FFTs with different input data.  

Note:The cuBLASLt library does not guarantee the support of all possible sizes and configurations, however, since CUDA 12.2 update 2, the problem size limitations on *m*, *n*, and batch size have been largely resolved. The main focus of the library is to provide the most performant kernels, which might have some implied limitations. Some non‑standard configurations may require a user to handle them manually, typically by decomposing the problem into smaller parts (seeProblem Size Limitations).  

### 3.1.1Problem Size Limitations  

There are inherent problem size limitations that are a result of limitations in CUDA grid dimensions. For example, many kernels do not support batch sizes greater than 65535 due to a limitation on the *z* dimension of a grid. There are similar restriction on the *m* and *n* values for a given problem. In cases where a problem cannot be run by a single kernel, cuBLASLt will attempt to decompose the problem into multiple sub‑problems and solve it by running the kernel on each sub‑problem. There are some restrictions on cuBLASLt internal problem decomposition which are summarized below:  

* Amax computations are not supported. This means that `CUBLASLT_MATMUL_DESC_AMAX_D_POINTER` and `CUBLASLT_MATMUL_DESC_EPILOGUE_AUX_AMAX_POINTER` must be left unset (see `cublasLtMatmulDescAttributes_t`).  
* All matrix layouts must have `CUBLASLT_MATRIX_LAYOUT_ORDER` set to `CUBLASLT_ORDER_COL` (see `cublasLtOrder_t`).  
* cuBLASLt will not partition along the *n* dimension when `CUBLASLT_MATMUL_DESC_EPILOGUE` is set to `CUBLASLT_EPILOGUE_DRELU_BGRAD` or `CUBLASLT_EPILOGUE_DGELU_BGRAD` (see `cublasLtEpilogue_t`).  

To overcome these limitations, a user may want to partition the problem themself, launch kernels for each sub‑problem, and compute any necessary reductions to combine the results.  

### 3.1.2Heuristics Cache  

cuBLASLt uses heuristics to pick the most suitable matmul kernel for execution based on the problem sizes, GPU configuration, and other parameters. This requires performing some computations on the host CPU, which could take tens of microseconds. To overcome this overhead, it is recommended to query the heuristics once using `cublasLtMatmulAlgoGetHeuristic()` and then reuse the result for subsequent computations using `cublasLtMatmul()`.  

For the cases where querying heuristics once and then reusing them is not feasible, cuBLASLt implements a heuristics cache that maps matmul problems to kernels previously selected by heuristics. The heuristics cache uses an LRU‑like eviction policy and is thread‑safe.  

The user can control the heuristics cache capacity with the `CUBLASLT_HEURISTICS_CACHE_CAPACITY` environment variable or with the `cublasLtHeuristicsCacheSetCapacity()` function which has higher precedence. The capacity is measured in number of entries and might be rounded up to the nearest multiple of some factor for performance reasons. Each entry takes about 360 bytes but is subject to change. The default capacity is 8192 entries.  

Note:Setting capacity to zero disables the cache completely. This can be useful for workloads that do not have a steady state and for which cache operations may have higher overhead than regular heuristics computations. See also: `cublasLtHeuristicsCacheGetCapacity()`, `cublasLtHeuristicsCacheSetCapacity()`.  

### 3.1.3cuBLASLt Logging  

cuBLASLt logging mechanism can be enabled by setting the following environment variables before launching the target application:  

* `CUBLASLT_LOG_LEVEL=<level>` – where `<level>` is one of the following levels:  
  * `0` – Off – logging is disabled (default)  
  * `1` – Error – only errors will be logged  
  * `2` – Trace – API calls that launch CUDA kernels will log their parameters and important information  
  * `3` – Hints – hints that can potentially improve the application’s performance  
  * `4` – Info – provides general information about the library execution, may contain details about heuristic status  

* `CUBLASLT_LOG_MASK=<mask>` – where `<mask>` is a combination of the following flags:  
  * `0` – Off  
  * `1` – Error  
  * `2` – Trace  
  * `4` – Hints  
  * `8` – Info  
  * `16` – API Trace  

For example, use `CUBLASLT_LOG_MASK=5` to enable Error and Hints messages.  

* `CUBLASLT_LOG_FILE=<file_name>` – where `<file_name>` is a path to a logging file. The file name may contain `%i`, which will be replaced with the process ID. For example `file_name_%i.log`.  
  * If `CUBLASLT_LOG_FILE` is not set, the log messages are printed to stdout.  
  * Another option is to use the experimental cuBLASLt logging API. See: `cublasLtLoggerSetCallback()`, `cublasLtLoggerSetFile()`, `cublasLtLoggerOpenFile()`, `cublasLtLoggerSetLevel()`, `cublasLtLoggerSetMask()`, `cublasLtLoggerForceDisable()`.  

---  

## 3.2 cuBLASLt Code Examples  

Please visit <https://github.com/NVIDIA/CUDALibrarySamples/tree/main/cuBLASLt> for updated code examples.  

---  

## 3.3 cuBLASLt Datatypes Reference  

### 3.3.1 `cublasLtClusterShape_t`  

`cublasLtClusterShape_t` is an enumerated type used to configure thread block cluster dimensions. Thread block clusters add an optional hierarchical level and are made up of thread blocks. Similar to thread blocks, these can be one, two, or three‑dimensional. See also *Thread Block Clusters*.  

| Value | Description |
|-------|-------------|
| `CUBLASLT_CLUSTER_SHAPE_AUTO` | Cluster shape is automatically selected. |
| `CUBLASLT_CLUSTER_SHAPE_1x1x1` | Cluster shape is 1 × 1 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_1x2x1` | Cluster shape is 1 × 2 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_1x4x1` | Cluster shape is 1 × 4 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_2x1x1` | Cluster shape is 2 × 1 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_2x2x1` | Cluster shape is 2 × 2 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_2x4x1` | Cluster shape is 2 × 4 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_4x1x1` | Cluster shape is 4 × 1 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_4x2x1` | Cluster shape is 4 × 2 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_4x4x1` | Cluster shape is 4 × 4 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_1x8x1` | Cluster shape is 1 × 8 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_8x1x1` | Cluster shape is 8 × 1 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_2x8x1` | Cluster shape is 2 × 8 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_8x2x1` | Cluster shape is 8 × 2 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_1x16x1` | Cluster shape is 1 × 16 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_16x1x1` | Cluster shape is 16 × 1 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_1x3x1` | Cluster shape is 1 × 3 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_1x5x1` | Cluster shape is 1 × 5 × 1. |
| `CUBLASLT_CLUSTER_SHAPE_1x6x1` | Cluster shape is 1 × 6 × 1. |
| *(continues)* | *(continues)* |

---  

### 3.3.2 `cublasLtEpilogue_t`  

The `cublasLtEpilogue_t` is an enum type to set the post‑processing options for the epilogue.  

| Value | Description |
|-------|-------------|
| `CUBLASLT_EPILOGUE_DEFAULT = 1` | No special post‑processing, just scale and quantize the results if necessary. |
| `CUBLASLT_EPILOGUE_RELU = 2` | Apply ReLU point‑wise transform to the results (`x := max(x, 0)`). |
| `CUBLASLT_EPILOGUE_RELU_AUX =` | *(implementation‑specific extension)* |
| `CUBLASLT_EPILOGUE_RELU | 128` | Apply ReLU point‑wise transform to the results (`x := max(x, 0)`). This epilogue mode produces an extra output, see `CUBLASLT_MATMUL_DESC_EPILOGUE_AUX_POINTER` of `cublasLtMatmulDescAttributes_t`. |
| `CUBLASLT_EPILOGUE_BIAS = 4` | Apply (broadcast) bias from the bias vector. Bias vector length must match matrix *D* rows, and it must be packed (such as stride between vector elements is 1). Bias vector is broadcast to all columns and added before applying the final post‑processing. |
| `CUBLASLT_EPILOGUE_RELU_BIAS =` | *(implementation‑specific extension)* |
| `CUBLASLT_EPILOGUE_RELU_AUX_BIAS =` | *(implementation‑specific extension)* |
| `CUBLASLT_EPILOGUE_DRELU = 8 | 128` | Apply ReLu gradient to matmul output. Store ReLu gradient in the output matrix. This epilogue mode requires an extra input, see `CUBLASLT_MATMUL_DESC_EPILOGUE_AUX_POINTER` of `cublasLtMatmulDescAttributes_t`. |
| `CUBLASLT_EPILOGUE_DRELU_BGRAD =` | *(implementation‑specific extension)* |
| `CUBLASLT_EPILOGUE_GELU = 32` | Apply GELU point‑wise transform to the results (`x := GELU(x)`). |
| `CUBLASLT_EPILOGUE_GELU_AUX =` | *(implementation‑specific extension)* |
| `CUBLASLT_EPILOGUE_GELU | 128` | Apply GELU point‑wise transform to the results (`x := GELU(x)`). This epilogue mode outputs GELU input as a separate matrix (useful for training). See `CUBLASLT_MATMUL_DESC_EPILOGUE_AUX_POINTER` of `cublasLtMatmulDescAttributes_t`. |
| `CUBLASLT_EPILOGUE_GELU_BIAS =` | *(implementation‑specific extension)* |
| `CUBLASLT_EPILOGUE_GELU_AUX_BIAS =` | *(implementation‑specific extension)* |
| `CUBLASLT_EPILOGUE_DGELU = 64 | 128` | Apply GELU gradient to matmul output. Store GELU gradient in the output matrix. This epilogue mode requires an extra input, see `CUBLASLT_MATMUL_DESC_EPILOGUE_AUX_POINTER` of `cublasLtMatmulDescAttributes_t`. |
| `CUBLASLT_EPILOGUE_DGELU_BGRAD =` | *(implementation‑specific extension)* |
| `CUBLASLT_EPILOGUE_BGRADA = 256` | Apply Bias gradient to the input matrix *A*. The bias size corresponds to the number of rows of the matrix *D*. The reduction happens over the GEMM’s “k” dimension. Store Bias gradient in the bias buffer, see `CUBLASLT_MATMUL_DESC_BIAS_POINTER` of `cublasLtMatmulDescAttributes_t`. |
| `CUBLASLT_EPILOGUE_BGRADB = 512` | Apply Bias gradient to the input matrix *B*. The bias size corresponds to the number of columns of the matrix *D*. The reduction happens over the GEMM’s “k” dimension. Store Bias gradient in the bias buffer, see `CUBLASLT_MATMUL_DESC_BIAS_POINTER` of `cublasLtMatmulDescAttributes_t`. |

*Note:* Only `CUBLASLT_EPILOGUE_DEFAULT` is supported when any matrix is set to `CUBLASLT_BATCH_MODE_POINTER_ARRAY`.  

---  

### 3.3.3 `cublasLtHandle_t`  

The `cublasLtHandle_t` type is a pointer type to an opaque structure holding the cuBLASLt library context. Use `cublasLtCreate()` to initialize the cuBLASLt library context and return a handle to an opaque structure holding the cuBLASLt library context, and use `cublasLtDestroy()` to destroy a previously created cuBLASLt library context descriptor and release the resources.  

Note:cuBLAS handle (`cublasHandle_t`) encapsulates a cuBLASLt handle. Any valid `cublasHandle_t` can be used in place of `cublasLtHandle_t` with a simple cast. However, unlike a cuBLAS handle, a cuBLASLt handle is not tied to any particular CUDA context with the exception of CUDA contexts tied to graphics contexts (starting from CUDA 12.8). If a cuBLASLt handle is created when the current CUDA context is tied to a graphics context, cuBLASLt detects the corresponding shared‑memory limitations and records it in the handle.  

---  

### 3.3.4 `cublasLtLoggerCallback_t`  

`cublasLtLoggerCallback_t` is a callback function pointer type. A callback function can be set using `cublasLtLoggerSetCallback()`.  

**Parameters**  

| Parameter | Memory | Input / Output | Description |
|-----------|--------|----------------|-------------|
| `logLevel` | Output | SeecuBLASLt Logging. |
| `functionName` | Output | The name of the API that logged this message. |
| `message` | Output | The log message. |

---  

## 3.4 cuBLASLt API Reference  

### 3.4.1 `cublasLtCreate()`  

```c
cublasStatus_t
cublasLtCreate(cublasLtHandle_t*lighthandle);
```

This function initializes the cuBLASLt library and creates a handle to an opaque structure holding the cuBLASLt library context. It allocates light hardware resources on the host and device and must be called prior to making any other cuBLASLt library calls.  

The cuBLASLt library context is tied to the current CUDA device. To use the library on multiple devices, one cuBLASLt handle must be created for each device. Furthermore, the device must be set as the current before invoking cuBLASLt functions with a handle tied to that device.  

**Parameters**  

| Parameter | Memory | Input / Output | Description |
|-----------|--------|----------------|-------------|
| `lighthandle` | Output | Pointer to the allocated cuBLASLt handle for the created cuBLASLt context. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | The allocation completed successfully. |
| `CUBLAS_STATUS_ALLOC_FAILED` | Resource allocation failed inside the cuBLASLt library. This is usually caused by a `cudaMalloc()` failure. To correct: prior to the function call, deallocate previously allocated memory as much as possible. |
| `CUBLAS_STATUS_INVALID_VALUE` | `lighthandle` is NULL. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | The cuBLASLt library was not initialized. This usually happens: * when `cublasLtCreate()` is not called first * an error in the CUDA Runtime API called by the cuBLASLt routine, or an error in the hardware setup. |

---  

### 3.4.2 `cublasLtDestroy()`  

```c
cublasStatus_t
cublasLtDestroy(cublasLtHandle_t lighthandle);
```

This function releases hardware resources used by the cuBLASLt library. This function is usually the last call with a particular handle to the cuBLASLt library. Because `cublasLtCreate()` allocates some internal resources and the release of those resources by calling `cublasLtDestroy()` will implicitly call `cudaDeviceSynchronize()`, it is recommended to minimize the number of times these functions are called.  

**Parameters**  

| Parameter | Memory | Input / Output | Description |
|-----------|--------|----------------|-------------|
| `lighthandle` | Input | Pointer to the cuBLASLt handle to be destroyed. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | The cuBLASLt context was successfully destroyed. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | The cuBLASLt library was not initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | `lighthandle` is NULL. |

---  

### 3.4.3 `cublasLtDisableCpuInstructionsSetMask()`  

```c
unsigned cublasLtDisableCpuInstructionsSetMask(unsigned mask);
```

Instructs cuBLASLt library to not use CPU instructionsspecified by the flags in the mask. The function takes precedence over the `CUBLASLT_DISABLE_CPU_INSTRUCTIONS_MASK` environment variable.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `mask` – the flags combined with bitwise OR (`|`) operator that specify which CPU instructions should not be used. |

**Supported flags**  

| Flag | Description |
|------|-------------|
| `0x1` | x86‑64 AVX512 ISA. |

**Returns** – the previous value of the mask.  

---  

### 3.4.4 `cublasLtGetCudartVersion()`  

```c
size_t cublasLtGetCudartVersion(void);
```

This function returns the version number of the CUDA Runtime library.  

**Parameters** – None.  

**Returns** – `size_t` – The version number of the CUDA Runtime library.  

---  

### 3.4.5 `cublasLtGetProperty()`  

```c
cublasStatus_t cublasLtGetProperty(libraryPropertyTypetype, int *value);
```

This function returns the value of the requested property by writing it to the memory location pointed to by the `value` parameter.  

**Parameters**  

| Parameter | Memory | Input / Output | Description |
|-----------|--------|----------------|-------------|
| `type` | Input | Of the type `libraryPropertyType`, whose value is requested from the property. See `libraryPropertyType_t`. |
| `value` | Output | Pointer to the host memory location where the requested information should be written. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | The requested `libraryPropertyType` information is successfully written at the provided address. |
| `CUBLAS_STATUS_INVALID_VALUE` | * If invalid value of the `type` input argument, or * if `value` is NULL. |

---  

### 3.4.6 `cublasLtGetStatusName()`  

```c
const char * cublasLtGetStatusName(cublasStatus_t status);
```

Returns the string representation of a given status.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `status` | The status. |

**Returns** – `const char *` – The NULL‑terminated string.  

---  

### 3.4.7 `cublasLtGetStatusString()`  

```c
const char * cublasLtGetStatusString(cublasStatus_t status);
```

Returns the description string for a given status.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `status` | The status. |

**Returns** – `const char *` – The NULL‑terminated string.  

---  

### 3.4.8 `cublasLtHeuristicsCacheGetCapacity()`  

```c
cublasStatus_t cublasLtHeuristicsCacheGetCapacity(size_t *capacity);
```

Returns the Heuristics Cache capacity.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `capacity` | The pointer to the returned capacity value. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | The capacity was successfully written. |
| `CUBLAS_STATUS_INVALID_VALUE` | The capacity was successfully set. |

---  

### 3.4.9 `cublasLtHeuristicsCacheSetCapacity()`  

```c
cublasStatus_t cublasLtHeuristicsCacheSetCapacity(size_t capacity);
```

Sets the Heuristics Cache capacity. Set the capacity to 0 to disable the heuristics cache. This function takes precedence over `CUBLASLT_HEURISTICS_CACHE_CAPACITY` environment variable.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `capacity` | The desirable heuristics cache capacity. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | The capacity was successfully set. |

---  

### 3.4.10 `cublasLtGetVersion()`  

```c
size_t cublasLtGetVersion(void);
```

This function returns the version number of cuBLASLt library.  

**Parameters** – None.  

**Returns** – `size_t` – The version number of cuBLASLt library.  

---  

### 3.4.11 `cublasLtLoggerSetCallback()`  

```c
cublasStatus_t cublasLtLoggerSetCallback(cublasLtLoggerCallback_t callback);
```

Experimental: This function sets the logging callback function.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `callback` | Input – Pointer to a callback function. See `cublasLtLoggerCallback_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If the callback function was successfully set. |
| `CUBLAS_STATUS_INVALID_VALUE` | If the callback pointer is NULL. |

---  

### 3.4.12 `cublasLtLoggerSetFile()`  

```c
cublasStatus_t cublasLtLoggerSetFile(FILE *file);
```

Experimental: This function sets the logging output file. Note: on a registered usage of this function call, the provided file handle must not be closed unless the function is called again to switch to a different file handle.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `file` | Input – Pointer to an open file. File should have write permission. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If logging file was successfully set. |
| `CUBLAS_STATUS_INVALID_VALUE` | If the mode value is different from `CUBLASXT_PINNING_DISABLED` and `CUBLASXT_PINNING_ENABLED`. |

---  

### 3.4.13 `cublasLtLoggerOpenFile()`  

```c
cublasStatus_t cublasLtLoggerOpenFile(const char *logFile);
```

Experimental: This function opens and sets the logging output file.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `logFile` | Input – Path of the logging output file. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If the logging file was successfully opened. |
| `CUBLAS_STATUS_INVALID_VALUE` | If the mode value is different from `CUBLASXT_PINNING_DISABLED` and `CUBLASXT_PINNING_ENABLED`. |

---  

### 3.4.14 `cublasLtLoggerSetLevel()`  

```c
cublasStatus_t cublasLtLoggerSetLevel(int level);
```

Experimental: This function sets the value of the logging level.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `level` | Input – Value of the logging level. See `cuBLASLt Logging`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | If the value was not a valid logging level. |
| `CUBLAS_STATUS_SUCCESS` | If the logging level was successfully set. |

---  

### 3.4.15 `cublasLtLoggerSetMask()`  

```c
cublasStatus_t cublasLtLoggerSetMask(int mask);
```

Experimental: This function sets the value of the logging mask.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `mask` | Input – Value of the logging mask. See `cuBLASLt Logging`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If the logging mask was successfully set. |
| `CUBLAS_STATUS_INVALID_VALUE` | If the value was not a valid logging mask. |

---  

### 3.4.16 `cublasLtLoggerForceDisable()`  

```c
cublasStatus_t cublasLtLoggerForceDisable(void);
```

Experimental: This function disables logging for the entire run.  

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If logging was successfully disabled. |
| `CUBLAS_STATUS_INVALID_VALUE` | If the value was not a valid logging mask. |

---  

### 3.4.17 `cublasLtMatmul()`  

```c
cublasStatus_t cublasLtMatmul(
    cublasLtHandle_t      ltHandle,
    cublasLtMatmulDesc_t  computeDesc,
    const void           *alpha,
    const void           *A,
    cublasLtMatrixLayout_t Adesc,
    const void           *B,
    cublasLtMatrixLayout_t Bdesc,
    const void           *beta,
    const void           *C,
    cublasLtMatrixLayout_t Cdesc,
    void                 *D,
    cublasLtMatrixLayout_t Ddesc,
    const cublasLtMatmulAlgo_t *algo,
    void                 *workspace,
    size_t               workspaceSizeInBytes,
    cudaStream_t         stream);
```

This function computes the matrix multiplication of matrices *A* and *B* to produce the output matrix *D*, according to the following operation:  

```
D = alpha*(A*B) + beta*(C)
```

where *A*, *B*, and *C* are input matrices, and *alpha* and *beta* are input scalars.  

Note: This function supports both in‑place matrix multiplication (`C == D` and `Cdesc == Ddesc`) and out‑of‑place matrix multiplication (`C != D`, both matrices must have the same data type, number of rows, number of columns, batch size, and memory order). In the out‑of‑place case, the leading dimension of *C* can be different from the leading dimension of *D*. Specifically the leading dimension of *C* can be 0 to achieve row or column broadcast. If `Cdesc` is omitted, this function assumes it to be equal to `Ddesc`.  

The `workspace` pointer must be aligned to at least a multiple of 256 bytes. The recommendations on `workspaceSizeInBytes` are the same as mentioned in the `cublasSetWorkspace()` section.  

**Datatypes Supported**  

`cublasLtMatmul()` supports the following `computeType`, `scaleType`, `Atype/Btype`, and `Ctype`. Footnotes can be found at the end of this section.  

---  

#### Table 6: Table 1. When *A*, *B*, *C*, and *D* are Regular Column‑ or Row‑major Matrices  

| computeType | scaleType | `Atype/Btype` | `Ctype` | BiasType |
|-------------|-----------|---------------|---------|----------|
| `CUBLAS_COMPUTE_16` | `CUBLAS_COMPUTE_16F_PEDANTIC` | `CUDA_R_16F` | `CUDA_R_16F` | — |
| `CUBLAS_COMPUTE_32I` or `CUBLAS_COMPUTE_32I_PEDANTIC` | — | `CUDA_R_32I` | `CUDA_R_8I` | — |
| `CUBLAS_COMPUTE_32F` | `CUBLAS_COMPUTE_32F_PEDANTIC` | `CUDA_R_32F` | `CUDA_R_16BF` | — |
| `CUBLAS_COMPUTE_32F_FAST_16F` | — | `CUDA_R_16F` | `CUDA_R_16F` | — |
| `CUBLAS_COMPUTE_32F_FAST_16B` | — | `CUDA_R_16BF` | `CUDA_R_16F` | — |
| `CUBLAS_COMPUTE_32F_FAST_TF32` or `CUBLAS_COMPUTE_32F_EMULATED_16BFX9` | — | `CUDA_R_32F` | `CUDA_R_16F` | — |
| `CUBLAS_COMPUTE_64F` | `CUBLAS_COMPUTE_64F_PEDANTIC` | `CUDA_R_64F` | `CUDA_R_64F` | — |
| `CUBLAS_COMPUTE_64F_EMULATED_FIXEDPOINT` | — | `CUDA_R_64F` | `CUDA_R_64F` | — |

---  

#### Table 7: Table 2. When *A*, *B*, *C*, and *D* Use Layouts for FP8  

| `Atype` | `Btype` | `Ctype` | `Dtype` | Bias Type |
|---------|---------|---------|---------|-----------|
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` | — |
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_16F` | `CUDA_R_16F` | — |
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_16F` | `CUDA_R_16F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| *(continues)* | *(continues)* | *(continues)* | *(continues)* | *(continues)* |

---  

#### Table 8: Table 3. When *A*, *B*, *C*, and *D* Use Layouts for FP4  

| `Atype` | `Btype` | `Ctype` | `Dtype` | Bias Type |
|---------|---------|---------|---------|-----------|
| `CUDA_R_4F_E2M1` | `CUDA_R_4F_E2M1` | `CUDA_R_16BF` | `CUDA_R_16BF` | — |
| `CUDA_R_4F_E2M1` | `CUDA_R_4F_E2M1` | `CUDA_R_16F` | `CUDA_R_16F` | — |
| `CUDA_R_4F_E2M1` | `CUDA_R_4F_E2M1` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| *(continues)* | *(continues)* | *(continues)* | *(continues)* | *(continues)* |

---  

#### Table 9: Table 4. When *A*, *B*, *C*, and *D* Use Layouts for FP8 (re‑listed)  

| `Atype` | `Btype` | `Ctype` | `Dtype` | Bias Type |
|---------|---------|---------|---------|-----------|
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` | — |
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_16F` | `CUDA_R_16F` | — |
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_16F` | `CUDA_R_16F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| *(continues)* | *(continues)* | *(continues)* | *(continues)* | *(continues)* |

---  

#### Table 10: Table 5. When *A*, *B*, *C*, and *D* are Planar‑Complex Matrices  

| `computeType` | `scaleType` | `Atype` | `Btype` | `Ctype` |
|---------------|-------------|---------|---------|---------|
| `CUBLAS_COMPUTE_32F` | `CUDA_R_32F` | `CUDA_C_32F` | `CUDA_C_32F` | `CUDA_C_16F` |
| `CUBLAS_COMPUTE_32F` | `CUDA_R_32F` | `CUDA_C_32F` | `CUDA_C_32F` | `CUDA_C_16BF` |
| *(continues)* | *(continues)* | *(continues)* | *(continues)* | *(continues)* |

---  

#### Table 11: Table 6. When *A*, *B*, *C*, and *D* are Regular Column‑major Matrices  

| `Atype` | `Btype` | `Ctype` | `Dtype` |
|---------|---------|---------|---------|
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` |
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_16F` | `CUDA_R_16F` |
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_32F` | `CUDA_R_32F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_16BFCUDA_R_16BF` | `CUDA_R_16F` | `CUDA_R_16F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_16BFCUDA_R_16BF` | `CUDA_R_16F` | `CUDA_R_16F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_16BFCUDA_R_16BF` | `CUDA_R_32F` | `CUDA_R_32F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_32F` | `CUDA_R_32F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` |
| *(continues)* | *(continues)* | *(continues)* | *(continues)* |

---  

#### Table 12: Table 7. When *A*, *B*, *C*, and *D* Use Layouts for FP4  

| `Atype` | `Btype` | `Ctype` | `Dtype` | Bias Type |
|---------|---------|---------|---------|-----------|
| `CUDA_R_4F_E2M1` | `CUDA_R_4F_E2M1` | `CUDA_R_16BF` | `CUDA_R_16BF` | — |
| `CUDA_R_4F_E2M1` | `CUDA_R_4F_E2M1` | `CUDA_R_16F` | `CUDA_R_16F` | — |
| `CUDA_R_4F_E2M1` | `CUDA_R_4F_E2M1` | `CUDA_R_32F` | `CUDA_R_32F` | — |
| *(continues)* | *(continues)* | *(continues)* | *(continues)* | *(continues)* |

---  

#### Table 13: Table 8. When *A*, *B*, *C*, and *D* are Planar‑Complex Matrices  

| `computeType` | `scaleType` | `Atype` | `Btype` | `Ctype` |
|---------------|-------------|---------|---------|---------|
| `CUBLAS_COMPUTE_32F` | `CUDA_R_32F` | `CUDA_C_32F` | `CUDA_C_32F` | `CUDA_C_16F` |
| `CUBLAS_COMPUTE_32F` | `CUDA_R_32F` | `CUDA_C_32F` | `CUDA_C_32F` | `CUDA_C_16BF` |
| *(continues)* | *(continues)* | *(continues)* | *(continues)* | *(continues)* |

---  

#### Table 14: Table 9. When *A*, *B*, *C*, and *D* are Regular Column‑major Matrices  

| `Atype` | `Btype` | `Ctype` | `Dtype` |
|---------|---------|---------|---------|
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` |
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_16F` | `CUDA_R_16F` |
| `CUDA_R_8F_E4M3` | `CUDA_R_8F_E4M3` | `CUDA_R_32F` | `CUDA_R_32F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_16BFCUDA_R_16BF` | `CUDA_R_16F` | `CUDA_R_16F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_16BFCUDA_R_16BF` | `CUDA_R_16F` | `CUDA_R_16F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_16BFCUDA_R_16BF` | `CUDA_R_32F` | `CUDA_R_32F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_16BF` | `CUDA_R_16BF` |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E4M3` | `CUDA_R_32F` | `CUDA_R_32F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` |
| `CUDA_R_8F_E5M2` | `CUDA_R_8F_E5M2` | `CUDA_R_32F` | `CUDA_R_32F` |
| *(continues)* | *(continues)* | *(continues)* | *(continues)* |

---  

#### Table 15: Table 10. When *A*, *B*, *C*, and *D* are Planar‑Complex Matrices  

| `computeType` | `scaleType` | `Atype` | `Btype` | `Ctype` |
|---------------|-------------|---------|---------|---------|
| `CUBLAS_COMPUTE_32F` | `CUDA_R_32F` | `CUDA_C_32F` | `CUDA_C_32F` | `CUDA_C_16F` |
| `CUBLAS_COMPUTE_32F` | `CUDA_R_32F` | `CUDA_C_32F` | `CUDA_C_32F` | `CUDA_C_16BF` |
| *(continues)* | *(continues)* | *(continues)* | *(continues)* | *(continues)* |

---  

### 3.4.18 `cublasLtMatmulAlgoCapGetAttribute()`  

```c
cublasStatus_t cublasLtMatmulAlgoCapGetAttribute(
    const cublasLtMatmulAlgo_t *algo,
    cublasLtMatmulAlgoCapAttributes_t attr,
    void *buf,
    size_t sizeInBytes,
    size_t *sizeWritten);
```

This function returns the value of the queried capability attribute for an initialized `cublasLtMatmulAlgo_t` descriptor structure. The capability attribute value is retrieved from the enumerated type `cublasLtMatmulAlgoCapAttributes_t`.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `algo` | Pointer to the previously created opaque structure holding the matrix multiply algorithm descriptor. See `cublasLtMatmulAlgo_t`. |
| `attr` | The capability attribute whose value will be retrieved by this function. See `cublasLtMatmulAlgoCapAttributes_t`. |
| `buf` | The attribute value returned by this function. |
| `sizeInBytes` | Size of `buf` buffer (in bytes) for verification. |
| `sizeWritten` | Valid only when the return value is `CUBLAS_STATUS_SUCCESS`. If `sizeInBytes` is non‑zero: then `sizeWritten` is the number of bytes actually written; if `sizeInBytes` is 0: then `sizeWritten` is the number of bytes needed to write full contents. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `sizeInBytes` is 0 and `sizeWritten` is NULL, or * if `sizeInBytes` is non‑zero and `buf` is NULL, or * if `sizeInBytes` doesn’t match size of internal storage for the selected attribute |
| `CUBLAS_STATUS_SUCCESS` | If attribute’s value was successfully written to user memory. |

---  

### 3.4.19 `cublasLtMatmulAlgoCheck()`  

```c
cublasStatus_t cublasLtMatmulAlgoCheck(
    cublasLtHandle_t      ltHandle,
    cublasLtMatmulDesc_t  operationDesc,
    cublasLtMatrixLayout_t Adesc,
    cublasLtMatrixLayout_t Bdesc,
    cublasLtMatrixLayout_t Cdesc,
    cublasLtMatrixLayout_t Ddesc,
    const cublasLtMatmulAlgo_t *algo,
    cublasLtMatmulHeuristicResult_t *result);
```

This function performs the correctness check on the matrix multiply algorithm descriptor for the matrix multiply operation `cublasLtMatmul()` function with the given input matrices *A*, *B* and *C*, and the output matrix *D*. It checks whether the descriptor is supported on the current device, and returns the result containing the required workspace and the calculated wave count.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `ltHandle` | Pointer to the allocated cuBLASLt handle for the cuBLASLt context. See `cublasLtHandle_t`. |
| `operationDesc` | Handle to a previously created matrix multiply descriptor of type `cublasLtMatmulDesc_t`. |
| `Adesc`, `Bdesc`, `Cdesc`, `Ddesc` | Handles to the previously created matrix layout descriptors of the type `cublasLtMatrixLayout_t`. |
| `algo` | Descriptor which specifies which matrix multiply algorithm should be used. See `cublasLtMatmulAlgo_t`. May point to `result->algo`. |
| `result` | Pointer to the structure holding the results returned by this function. The results comprise of the required workspace and the calculated wave count. The `algo` field is never updated. See `cublasLtMatmulHeuristicResult_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | If matrix layout descriptors or the operation descriptor do not match the `algo` descriptor. |
| `CUBLAS_STATUS_NOT_SUPPORTED` | If the `algo` configuration or data type combination is not currently supported on the given device. |
| `CUBLAS_STATUS_ARCH_MISMATCH` | If the `algo` configuration cannot be run using the selected device. |
| `CUBLAS_STATUS_SUCCESS` | If the check was successful. |

---  

### 3.4.20 `cublasLtMatmulAlgoConfigGetAttribute()`  

```c
cublasStatus_t cublasLtMatmulAlgoConfigGetAttribute(
    const cublasLtMatmulAlgo_t *algo,
    cublasLtMatmulAlgoConfigAttributes_t attr,
    void *buf,
    size_t sizeInBytes,
    size_t *sizeWritten);
```

This function returns the value of the queried configuration attribute for an initialized `cublasLtMatmulAlgo_t` descriptor structure. The configuration attribute value is retrieved from the enumerated type `cublasLtMatmulAlgoConfigAttributes_t`.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `algo` | Pointer to the previously created opaque structure holding the matrix multiply algorithm descriptor. See `cublasLtMatmulAlgo_t`. |
| `attr` | The configuration attribute whose value will be retrieved by this function. See `cublasLtMatmulAlgoConfigAttributes_t`. |
| `buf` | The attribute value returned by this function. |
| `sizeInBytes` | Size of `buf` buffer (in bytes) for verification. |
| `sizeWritten` | Valid only when the return value is `CUBLAS_STATUS_SUCCESS`. If `sizeInBytes` is non‑zero: then `sizeWritten` is the number of bytes actually written; if `sizeInBytes` is 0: then `sizeWritten` is the number of bytes needed to write full contents. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `sizeInBytes` is 0 and `sizeWritten` is NULL, or * if `sizeInBytes` is non‑zero and `buf` is NULL, or * if `sizeInBytes` doesn’t match size of internal storage for the selected attribute |
| `CUBLAS_STATUS_SUCCESS` | If attribute’s value was successfully written to user memory. |

---  

### 3.4.21 `cublasLtMatmulAlgoConfigSetAttribute()`  

```c
cublasStatus_t cublasLtMatmulAlgoConfigSetAttribute(
    cublasLtMatmulAlgo_t *algo,
    cublasLtMatmulAlgoConfigAttributes_t attr,
    const void *buf,
    size_t sizeInBytes);
```

This function sets the value of the specified configuration attribute for an initialized `cublasLtMatmulAlgo_t` descriptor. The configuration attribute is an enumerant of the type `cublasLtMatmulAlgoConfigAttributes_t`.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `algo` | Pointer to the previously created opaque structure holding the matrix multiply algorithm descriptor. See `cublasLtMatmulAlgo_t`. |
| `attr` | The configuration attribute whose value will be set by this function. See `cublasLtMatmulAlgoConfigAttributes_t`. |
| `buf` | The value to which the configuration attribute should be set. |
| `sizeInBytes` | Size of `buf` buffer (in bytes) for verification. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `buf` is NULL or `sizeInBytes` doesn’t match the size of the internal storage for the selected attribute. |
| `CUBLAS_STATUS_SUCCESS` | If the attribute was set successfully. |

---  

### 3.4.22 `cublasLtMatmulAlgoGetHeuristic()`  

```c
cublasStatus_t cublasLtMatmulAlgoGetHeuristic(
    cublasLtHandle_t               ltHandle,
    cublasLtMatmulDesc_t           operationDesc,
    cublasLtMatrixLayout_t         Adesc,
    cublasLtMatrixLayout_t         Bdesc,
    cublasLtMatrixLayout_t         Cdesc,
    cublasLtMatrixLayout_t         Ddesc,
    cublasLtMatmulPreference_t     preference,
    int                            requestedAlgoCount,
    cublasLtMatmulHeuristicResult_t heuristicResultsArray[],
    int *                           returnAlgoCount);
```

This function retrieves the possible algorithms for the matrix multiply operation `cublasLtMatmul()` function with the given input matrices *A*, *B* and *C*, and the output matrix *D*. The output is placed in `heuristicResultsArray[]` in the order of increasing estimated compute time.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `ltHandle` | Pointer to the allocated cuBLASLt handle for the cuBLASLt context. See `cublasLtHandle_t`. |
| `operationDesc` | Handle to a previously created matrix multiply descriptor of type `cublasLtMatmulDesc_t`. |
| `Adesc`, `Bdesc`, `Cdesc`, `Ddesc` | Handles to the previously created matrix layout descriptors of the type `cublasLtMatrixLayout_t`. |
| `preference` | Pointer to the structure holding the heuristic search preferences descriptor. See `cublasLtMatmulPreference_t`. |
| `requestedAlgoCount` | Size of the `heuristicResultsArray` (in elements). This is the requested maximum number of algorithms to return. |
| `heuristicResultsArray[]` | Output – Array containing the algorithm heuristics and associated runtime characteristics, returned by this function, in the order of increasing estimated compute time. |
| `returnAlgoCount` | Output – Number of algorithms actually returned by this function. This is the number of `heuristicResultsArray` elements written. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | If `requestedAlgoCount` is less or equal to zero. |
| `CUBLAS_STATUS_NOT_SUPPORTED` | If no heuristic function is available for current configuration. |
| `CUBLAS_STATUS_SUCCESS` | If query was successful. Inspect `heuristicResultsArray[0 to (returnAlgoCount‑1)]` .state for the status of the results. |

*Note:* This function may load some kernels using CUDA Driver API which may fail when there is no available GPU memory. Do not allocate the entire VRAM before running `cublasLtMatmulAlgoGetHeuristic()`.  

---  

### 3.4.23 `cublasLtMatmulAlgoGetIds()`  

```c
cublasStatus_t cublasLtMatmulAlgoGetIds(
    cublasLtHandle_t               ltHandle,
    cublasComputeType_t            computeType,
    cudaDataType_t                 scaleType,
    cudaDataType_t                 Atype,
    cudaDataType_t                 Btype,
    cudaDataType_t                 Ctype,
    cudaDataType_t                 Dtype,
    int                            requestedAlgoCount,
    int                            algoIdsArray[],
    int *                           returnAlgoCount);
```

This function retrieves the IDs of all the matrix multiply algorithms that are valid, and can potentially be run by the `cublasLtMatmul()` function, for given types of the input matrices *A*, *B* and *C*, and of the output matrix *D*.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `ltHandle` | Pointer to the allocated cuBLASLt handle for the cuBLASLt context. See `cublasLtHandle_t`. |
| `computeType`, `scaleType`, `Atype`, `Btype`, `Ctype`, `Dtype` | Input – Data types of the computation type, scaling factors and of the operand matrices. See `cudaDataType_t`. |
| `requestedAlgoCount` | Input – Number of algorithms requested. Must be > 0. |
| `algoIdsArray[]` | Output – Array containing the algorithm IDs returned by this function. |
| `returnAlgoCount` | Output – Number of algorithms actually returned by this function. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | If `requestedAlgoCount` is less or equal to zero. |
| `CUBLAS_STATUS_SUCCESS` | If query was successful. Inspect `returnAlgoCount` to get actual number of IDs available. |

---  

### 3.4.24 `cublasLtMatmulAlgoInit()`  

```c
cublasStatus_t cublasLtMatmulAlgoInit(
    cublasLtHandle_t               ltHandle,
    cublasComputeType_t            computeType,
    cudaDataType_t                 scaleType,
    cudaDataType_t                 Atype,
    cudaDataType_t                 Btype,
    cudaDataType_t                 Ctype,
    cudaDataType_t                 Dtype,
    int                            algoId,
    cublasLtMatmulAlgo_t          *algo);
```

This function initializes the matrix multiply algorithm structure for the `cublasLtMatmul()` function, for a specified matrix multiply algorithm and input matrices *A*, *B* and *C*, and the output matrix *D*.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `ltHandle` | Pointer to the allocated cuBLASLt handle for the cuBLASLt context. See `cublasLtHandle_t`. |
| `computeType` | Input – Computation type. See `CUBLASLT_MATMUL_DESC_COMPUTE_TYPE` of `cublasLtMatmulDescAttributes_t`. |
| `scaleType` | Input – Scale type. See `CUBLASLT_MATMUL_DESC_SCALE_TYPE` of `cublasLtMatmulDescAttributes_t`. Usually same as `computeType`. |
| `Atype`, `Btype`, `Ctype`, `Dtype` | Input – Data type precision for the input and output matrices. See `cudaDataType_t`. |
| `algoId` | Input – Specifies the algorithm being initialized. Should be a valid `algoId` returned by the `cublasLtMatmulAlgoGetIds()` function. |
| `algo` | Output – Pointer to the opaque structure to be initialized. See `cublasLtMatmulAlgo_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | If `algo` is NULL or `algoId` is outside the recognized range. |
| `CUBLAS_STATUS_NOT_SUPPORTED` | If `algoId` is not supported for given combination of data types. |
| `CUBLAS_STATUS_SUCCESS` | If the structure was successfully initialized. |

---  

### 3.4.25 `cublasLtMatmulDescCreate()`  

```c
cublasStatus_t cublasLtMatmulDescCreate(cublasLtMatmulDesc_t *matmulDesc,
                                      cublasComputeType_t computeType,
                                      cudaDataType_t scaleType);
```

This function creates a matrix multiply descriptor by allocating the memory needed to hold its opaque structure.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matmulDesc` | Output – Pointer to the structure holding the matrix multiply descriptor created by this function. See `cublasLtMatmulDesc_t`. |
| `computeType` | Input – Computation type. See `CUBLASLT_MATMUL_DESC_COMPUTE_TYPE` of `cublasLtMatmulDescAttributes_t`. |
| `scaleType` | Input – Scale type. See `cudaDataType_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.26 `cublasLtMatmulDescInit()`  

```c
cublasStatus_t cublasLtMatmulDescInit(cublasLtMatmulDesc_t matmulDesc,
                                      cublasComputeType_t computeType,
                                      cudaDataType_t scaleType);
```

This function initializes a matrix multiply descriptor in a previously allocated one.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matmulDesc` | Output – Pointer to the structure holding the matrix multiply descriptor initialized by this function. See `cublasLtMatmulDesc_t`. |
| `computeType` | Input – Computation type. See `CUBLASLT_MATMUL_DESC_COMPUTE_TYPE` of `cublasLtMatmulDescAttributes_t`. |
| `scaleType` | Input – Scale type. See `cudaDataType_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.27 `cublasLtMatmulDescDestroy()`  

```c
cublasStatus_t cublasLtMatmulDescDestroy(cublasLtMatmulDesc_t matmulDesc);
```

This function destroys a previously created matrix multiply descriptor object.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matmulDesc` | Input – Pointer to the structure holding the matrix multiply descriptor that should be destroyed by this function. See `cublasLtMatmulDesc_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If operation was successful. |

---  

### 3.4.28 `cublasLtMatmulDescGetAttribute()`  

```c
cublasStatus_t cublasLtMatmulDescGetAttribute(
    cublasLtMatmulDesc_t               matmulDesc,
    cublasLtMatmulDescAttributes_t     attr,
    void                              *buf,
    size_t                              sizeInBytes,
    size_t                             *sizeWritten);
```

This function returns the value of the queried attribute belonging to a previously created matrix multiply descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matmulDesc` | Input – Pointer to the previously created structure holding the matrix multiply descriptor queried by this function. See `cublasLtMatmulDesc_t`. |
| `attr` | Input – The attribute being queried for. See `cublasLtMatmulDescAttributes_t`. |
| `buf` | Output – Memory address containing the attribute value retrieved by this function. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |
| `sizeWritten` | Output – Valid only when the return value is `CUBLAS_STATUS_SUCCESS`. If `sizeInBytes` is non‑zero: then `sizeWritten` is the number of bytes actually written; if `sizeInBytes` is 0: then `sizeWritten` is the number of bytes needed to write full contents. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `sizeInBytes` is 0 and `sizeWritten` is NULL, or * if `sizeInBytes` is non‑zero and `buf` is NULL, or * if `sizeInBytes` doesn’t match size of internal storage for the selected attribute |
| `CUBLAS_STATUS_SUCCESS` | If attribute’s value was successfully written to user memory. |

---  

### 3.4.29 `cublasLtMatmulDescSetAttribute()`  

```c
cublasStatus_t cublasLtMatmulDescSetAttribute(
    cublasLtMatmulDesc_t               matmulDesc,
    cublasLtMatmulDescAttributes_t     attr,
    const void                         *buf,
    size_t                              sizeInBytes);
```

This function sets the value of the specified attribute belonging to a previously created matrix multiply descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matmulDesc` | Input – Pointer to the previously created structure holding the matrix multiply descriptor queried by this function. See `cublasLtMatmulDesc_t`. |
| `attr` | Input – The attribute that will be set by this function. See `cublasLtMatmulDescAttributes_t`. |
| `buf` | Input – The value to which the configuration attribute should be set. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `buf` is NULL or `sizeInBytes` doesn’t match the size of the internal storage for the selected attribute. |
| `CUBLAS_STATUS_SUCCESS` | If the attribute was set successfully. |

---  

### 3.4.30 `cublasLtMatmulPreferenceCreate()`  

```c
cublasStatus_t cublasLtMatmulPreferenceCreate(cublasLtMatmulPreference_t *pref);
```

This function creates a matrix multiply heuristic search preferences descriptor by allocating the memory needed to hold its opaque structure.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `pref` | Output – Pointer to the structure holding the matrix multiply preferences descriptor created by this function. See `cublasLtMatmulPreference_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.31 `cublasLtMatmulPreferenceInit()`  

```c
cublasStatus_t cublasLtMatmulPreferenceInit(cublasLtMatmulPreference_t pref);
```

This function initializes a matrix multiply heuristic search preferences descriptor in a previously allocated one.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `pref` | Output – Pointer to the structure holding the matrix multiply preferences descriptor created by this function. See `cublasLtMatmulPreference_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.32 `cublasLtMatmulPreferenceDestroy()`  

```c
cublasStatus_t cublasLtMatmulPreferenceDestroy(cublasLtMatmulPreference_t pref);
```

This function destroys a previously created matrix multiply preferences descriptor object.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `pref` | Input – Pointer to the structure holding the matrix multiply preferences descriptor that should be destroyed by this function. See `cublasLtMatmulPreference_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If the operation was successful. |

---  

### 3.4.33 `cublasLtMatmulPreferenceGetAttribute()`  

```c
cublasStatus_t cublasLtMatmulPreferenceGetAttribute(
    cublasLtMatmulPreference_t           pref,
    cublasLtMatmulPreferenceAttributes_t attr,
    void                                 *buf,
    size_t                               sizeInBytes,
    size_t                               *sizeWritten);
```

This function returns the value of the queried attribute belonging to a previously created matrix multiply heuristic search preferences descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `pref` | Input – Pointer to the previously created structure holding the matrix multiply preferences descriptor queried by this function. See `cublasLtMatmulPreference_t`. |
| `attr` | Input – The attribute that will be queried by this function. See `cublasLtMatmulPreferenceAttributes_t`. |
| `buf` | Output – Memory address containing the attribute value retrieved by this function. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |
| `sizeWritten` | Output – Valid only when the return value is `CUBLAS_STATUS_SUCCESS`. If `sizeInBytes` is non‑zero: then `sizeWritten` is the number of bytes actually written; if `sizeInBytes` is 0: then `sizeWritten` is the number of bytes needed to write full contents. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `sizeInBytes` is 0 and `sizeWritten` is NULL, or * if `sizeInBytes` is non‑zero and `buf` is NULL, or * if `sizeInBytes` doesn’t match size of internal storage for the selected attribute |
| `CUBLAS_STATUS_SUCCESS` | If attribute’s value was successfully written to user memory. |

---  

### 3.4.34 `cublasLtMatmulPreferenceSetAttribute()`  

```c
cublasStatus_t cublasLtMatmulPreferenceSetAttribute(
    cublasLtMatmulPreference_t           pref,
    cublasLtMatmulPreferenceAttributes_t attr,
    const void                          *buf,
    size_t                               sizeInBytes);
```

This function sets the value of the specified attribute belonging to a previously created matrix multiply preferences descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `pref` | Input – Pointer to the previously created structure holding the matrix multiply preferences descriptor queried by this function. See `cublasLtMatmulPreference_t`. |
| `attr` | Input – The attribute that will be set by this function. See `cublasLtMatmulPreferenceAttributes_t`. |
| `buf` | Input – The value to which the configuration attribute should be set. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `buf` is NULL or `sizeInBytes` doesn’t match the size of the internal storage for the selected attribute. |
| `CUBLAS_STATUS_SUCCESS` | If the attribute was set successfully. |

---  

### 3.4.35 `cublasLtMatrixLayoutCreate()`  

```c
cublasStatus_t cublasLtMatrixLayoutCreate(cublasLtMatrixLayout_t *matLayout,
                                        cudaDataType_t type,
                                        uint64_t rows,
                                        uint64_t cols,
                                        int64_t ld);
```

This function creates a matrix layout descriptor by allocating the memory needed to hold its opaque structure.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matLayout` | Output – Pointer to the structure holding the matrix layout descriptor created by this function. See `cublasLtMatrixLayout_t`. |
| `type` | Input – Data type precision for the matrix layout descriptor this function creates. See `cudaDataType_t`. |
| `rows` | Input – Number of rows of the matrix. |
| `cols` | Input – Number of columns of the matrix. |
| `ld` | Input – Leading dimension of the matrix. In column major layout, this is the number of elements to jump to reach the next column. Thus `ld >= rows` (number of rows). |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If the memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.36 `cublasLtMatrixLayoutInit()`  

```c
cublasStatus_t cublasLtMatrixLayoutInit(cublasLtMatrixLayout_t matLayout,
                                      cudaDataType_t type,
                                      uint64_t rows,
                                      uint64_t cols,
                                      int64_t ld);
```

This function initializes a matrix layout descriptor in a previously allocated one.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matLayout` | Output – Pointer to the structure holding the matrix layout descriptor initialized by this function. See `cublasLtMatrixLayout_t`. |
| `type` | Input – Data type precision for the matrix layout descriptor this function initializes. See `cudaDataType_t`. |
| `rows` | Input – Number of rows of the matrix. |
| `cols` | Input – Number of columns of the matrix. |
| `ld` | Input – Leading dimension of the matrix. In column major layout, this is the number of elements to jump to reach the next column. Thus `ld >= rows` (number of rows). |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If the memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.37 `cublasLtGroupedMatrixLayoutCreate()`  

```c
cublasStatus_t cublasLtGroupedMatrixLayoutCreate(cublasLtMatrixLayout_t *matLayout,
                                                cudaDataType_t type,
                                                int groupCount,
                                                const void *rows_array,
                                                const void *cols_array,
                                                const void *ld_array);
```

Experimental: This function creates a grouped matrix layout descriptor by allocating the memory needed to hold its opaque structure.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matLayout` | Output – Pointer to the structure holding the matrix layout descriptor created by this function. See `cublasLtMatrixLayout_t`. |
| `type` | Input – Data type precision for the matrix layout descriptor this function creates. See `cudaDataType_t`. |
| `groupCount` | Input – Number of groups in the grouped matrix layout descriptor. |
| `rows_array` | Input – Input – Pointer to a device array of rows of the grouped matrix layout descriptor. |
| `cols_array` | Input – Input – Pointer to a device array of columns of the grouped matrix layout descriptor. |
| `ld_array` | Input – Input – Pointer to a device array of leading dimensions of the grouped matrix layout descriptor. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If the memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.38 `cublasLtGroupedMatrixLayoutInit()`  

```c
cublasStatus_t cublasLtGroupedMatrixLayoutInit(cublasLtMatrixLayout_t matLayout,
                                              cudaDataType_t type,
                                              int groupCount,
                                              const void *rows_array,
                                              const void *cols_array,
                                              const void *ld_array);
```

Experimental: This function initializes a grouped matrix layout descriptor in a previously allocated one.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matLayout` | Output – Pointer to the structure holding the matrix layout descriptor initialized by this function. See `cublasLtMatrixLayout_t`. |
| `type` | Input – Data type precision for the matrix layout descriptor this function initializes. See `cudaDataType_t`. |
| `groupCount` | Input – Number of groups in the grouped matrix layout descriptor. |
| `rows_array` | Input – Input – Pointer to a device array of rows of the grouped matrix layout descriptor. |
| `cols_array` | Input – Input – Pointer to a device array of columns of the grouped matrix layout descriptor. |
| `ld_array` | Input – Input – Pointer to a device array of leading dimensions of the grouped matrix layout descriptor. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If the memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.39 `cublasLtMatrixLayoutDestroy()`  

```c
cublasStatus_t cublasLtMatrixLayoutDestroy(cublasLtMatrixLayout_t matLayout);
```

This function destroys a previously created matrix layout descriptor object.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matLayout` | Input – Pointer to the structure holding the matrix layout descriptor that should be destroyed by this function. See `cublasLtMatrixLayout_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If the operation was successful. |

---  

### 3.4.40 `cublasLtMatrixLayoutGetAttribute()`  

```c
cublasStatus_t cublasLtMatrixLayoutGetAttribute(
    cublasLtMatrixLayout_t               matLayout,
    cublasLtMatrixLayoutAttribute_t      attr,
    void                                 *buf,
    size_t                               sizeInBytes,
    size_t                               *sizeWritten);
```

This function returns the value of the queried attribute belonging to the specified matrix layout descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matLayout` | Input – Pointer to the previously created structure holding the matrix layout descriptor queried by this function. See `cublasLtMatrixLayout_t`. |
| `attr` | Input – The attribute being queried for. See `cublasLtMatrixLayoutAttribute_t`. |
| `buf` | Output – Memory address containing the attribute value retrieved by this function. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |
| `sizeWritten` | Output – Valid only when the return value is `CUBLAS_STATUS_SUCCESS`. If `sizeInBytes` is non‑zero: then `sizeWritten` is the number of bytes actually written; if `sizeInBytes` is 0: then `sizeWritten` is the number of bytes needed to write full contents. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `sizeInBytes` is 0 and `sizeWritten` is NULL, or * if `sizeInBytes` is non‑zero and `buf` is NULL, or * if `sizeInBytes` doesn’t match size of internal storage for the selected attribute |
| `CUBLAS_STATUS_SUCCESS` | If attribute’s value was successfully written to user memory. |

---  

### 3.4.41 `cublasLtMatrixLayoutSetAttribute()`  

```c
cublasStatus_t cublasLtMatrixLayoutSetAttribute(
    cublasLtMatrixLayout_t               matLayout,
    cublasLtMatrixLayoutAttribute_t      attr,
    const void                          *buf,
    size_t                               sizeInBytes);
```

This function sets the value of the specified attribute belonging to a previously created matrix layout descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `matLayout` | Input – Pointer to the previously created structure holding the matrix layout descriptor queried by this function. See `cublasLtMatrixLayout_t`. |
| `attr` | Input – The attribute that will be set by this function. See `cublasLtMatrixLayoutAttribute_t`. |
| `buf` | Input – The value to which the configuration attribute should be set. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `buf` is NULL or `sizeInBytes` doesn’t match the size of the internal storage for the selected attribute. |
| `CUBLAS_STATUS_SUCCESS` | If the attribute was set successfully. |

---  

### 3.4.42 `cublasLtMatrixTransform()`  

```c
cublasStatus_t cublasLtMatrixTransform(
    cublasLtHandle_t               ltHandle,
    cublasLtMatrixTransformDesc_t    transformDesc,
    const void                      *alpha,
    const void                      *A,
    cublasLtMatrixLayout_t           Adesc,
    const void                      *beta,
    const void                      *B,
    cublasLtMatrixLayout_t           Bdesc,
    void                           *C,
    cublasLtMatrixLayout_t           Cdesc,
    cudaStream_t                     stream);
```

This function computes the matrix transformation operation on the input matrices *A* and *B* to produce the output matrix *C*, according to the following operation:  

```
C = alpha*transformation(A) + beta*transformation(B)
```

where *A*, *B* are input matrices, and *alpha* and *beta* are input scalars. The transformation operation is defined by the `transformDesc` pointer. This function can be used to change the memory order of data or to scale and shift the values.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `ltHandle` | Input – Pointer to the allocated cuBLASLt handle for the cuBLASLt context. See `cublasLtHandle_t`. |
| `transformDesc` | Input – Pointer to the opaque descriptor holding the matrix transformation operation. See `cublasLtMatrixTransformDesc_t`. |
| `alpha`, `beta` | Input – Pointers to the scalars used in the multiplication. |
| `A`, `B` | Input – Pointers to the GPU memory associated with the corresponding descriptors `Adesc` and `Bdesc`. |
| `Adesc`, `Bdesc`, `Cdesc` | Input – Handles to the previously created descriptors of the type `cublasLtMatrixLayout_t`. |
| `C` | Output – Pointer to the GPU memory associated with the `Cdesc` descriptor. |
| `stream` | Input – The CUDA stream where all the GPU work will be submitted. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_NOT_INITIALIZED` | If `ltHandle` has not been initialized. |
| `CUBLAS_STATUS_INVALID_VALUE` | If the parameters are in conflict or in an impossible configuration. For example, when `A` is not NULL, but `Adesc` is NULL. |
| `CUBLAS_STATUS_NOT_SUPPORTED` | If the current implementation on the selected device does not support the configured operation. |
| `CUBLAS_STATUS_ARCH_MISMATCH` | If the configured operation cannot be run using the selected device. |
| `CUBLAS_STATUS_EXECUTION_FAILED` | If CUDA reported an execution error from the device. |
| `CUBLAS_STATUS_SUCCESS` | If the operation completed successfully. |

---  

### 3.4.43 `cublasLtMatrixTransformDescCreate()`  

```c
cublasStatus_t cublasLtMatrixTransformDescCreate(
    cublasLtMatrixTransformDesc_t *transformDesc,
    cudaDataType_t                     scaleType);
```

This function creates a matrix transform descriptor by allocating the memory needed to hold its opaque structure.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `transformDesc` | Output – Pointer to the structure holding the matrix transform descriptor created by this function. See `cublasLtMatrixTransformDesc_t`. |
| `scaleType` | Input – Scale type. See `cudaDataType_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.44 `cublasLtMatrixTransformDescInit()`  

```c
cublasStatus_t cublasLtMatrixTransformDescInit(
    cublasLtMatrixTransformDesc_t transformDesc,
    cudaDataType_t                 scaleType);
```

This function initializes a matrix transform descriptor in a previously allocated one.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `transformDesc` | Output – Pointer to the structure holding the matrix transform descriptor initialized by this function. See `cublasLtMatrixTransformDesc_t`. |
| `scaleType` | Input – Scale type. See `cudaDataType_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.45 `cublasLtMatrixTransformDescDestroy()`  

```c
cublasStatus_t cublasLtMatrixTransformDescDestroy(
    cublasLtMatrixTransformDesc_t transformDesc);
```

This function destroys a previously created matrix transform descriptor object.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `transformDesc` | Input – Pointer to the structure holding the matrix transform descriptor that should be destroyed by this function. See `cublasLtMatrixTransformDesc_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If the operation was successful. |

---  

### 3.4.46 `cublasLtMatrixTransformDescGetAttribute()`  

```c
cublasStatus_t cublasLtMatrixTransformDescGetAttribute(
    cublasLtMatrixTransformDesc_t          transformDesc,
    cublasLtMatrixTransformDescAttributes_t attr,
    void                                   *buf,
    size_t                                 sizeInBytes,
    size_t                                 *sizeWritten);
```

This function returns the value of the queried attribute belonging to a previously created matrix transform descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `transformDesc` | Input – Pointer to the previously created structure holding the matrix transform descriptor queried by this function. See `cublasLtMatrixTransformDesc_t`. |
| `attr` | Input – The attribute that will be retrieved by this function. See `cublasLtMatrixTransformDescAttributes_t`. |
| `buf` | Output – Memory address containing the attribute value retrieved by this function. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |
| `sizeWritten` | Output – Valid only when the return value is `CUBLAS_STATUS_SUCCESS`. If `sizeInBytes` is non‑zero: then `sizeWritten` is the number of bytes actually written; if `sizeInBytes` is 0: then `sizeWritten` is the number of bytes needed to write full contents. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `sizeInBytes` is 0 and `sizeWritten` is NULL, or * if `sizeInBytes` is non‑zero and `buf` is NULL, or * if `sizeInBytes` doesn’t match size of internal storage for the selected attribute |
| `CUBLAS_STATUS_SUCCESS` | If attribute’s value was successfully written to user memory. |

---  

### 3.4.47 `cublasLtMatrixTransformDescSetAttribute()`  

```c
cublasStatus_t cublasLtMatrixTransformDescSetAttribute(
    cublasLtMatrixTransformDesc_t          transformDesc,
    cublasLtMatrixTransformDescAttributes_t attr,
    const void                           *buf,
    size_t                                 sizeInBytes);
```

This function sets the value of the specified attribute belonging to a previously created matrix transform descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `transformDesc` | Input – Pointer to the previously created structure holding the matrix transform descriptor queried by this function. See `cublasLtMatrixTransformDesc_t`. |
| `attr` | Input – The attribute that will be set by this function. See `cublasLtMatrixTransformDescAttributes_t`. |
| `buf` | Input – The value to which the configuration attribute should be set. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `buf` is NULL or `sizeInBytes` doesn’t match the size of the internal storage for the selected attribute. |
| `CUBLAS_STATUS_SUCCESS` | If the attribute was set successfully. |

---  

### 3.4.48 `cublasLtEmulationDescInit()`  

```c
cublasStatus_t cublasLtEmulationDescInit(cublasLtEmulationDesc_t emulationDesc);
```

This function initializes a previously allocated emulation descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `emulationDesc` | Input – Pointer to the previously created structure holding the emulation descriptor queried by this function. See `cublasLtEmulationDesc_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If the size of the pre‑allocated space is insufficient. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.49 `cublasLtEmulationDescCreate()`  

```c
cublasStatus_t cublasLtEmulationDescCreate(cublasLtEmulationDesc_t *emulationDesc);
```

This function creates a new emulation descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `emulationDesc` | Input – Pointer to the previously created structure holding the emulation descriptor queried by this function. See `cublasLtEmulationDesc_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_ALLOC_FAILED` | If memory could not be allocated. |
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was created successfully. |

---  

### 3.4.50 `cublasLtEmulationDescDestroy()`  

```c
cublasStatus_t cublasLtEmulationDescDestroy(cublasLtEmulationDesc_t emulationDesc);
```

This function destroys a previously created emulation descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `emulationDesc` | Input – Pointer to the previously created structure holding the emulation descriptor queried by this function. See `cublasLtEmulationDesc_t`. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_SUCCESS` | If the descriptor was destroyed successfully. |

---  

### 3.4.51 `cublasLtEmulationDescSetAttribute()`  

```c
cublasStatus_t cublasLtEmulationDescSetAttribute(
    cublasLtEmulationDesc_t                emulationDesc,
    cublasLtEmulationDescAttributes_t      attr,
    const void                           *buf,
    size_t                                 sizeInBytes);
```

This function sets the value of the specified attribute belonging to a previously created emulation descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `emulationDesc` | Input – Pointer to the previously created structure holding the emulation descriptor queried by this function. See `cublasLtEmulationDesc_t`. |
| `attr` | Input – The attribute that will be set by this function. See `cublasLtEmulationDescAttributes_t`. |
| `buf` | Input – The value to which the configuration attribute should be set. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `buf` is NULL or `sizeInBytes` doesn’t match the size of the internal storage for the selected attribute. |
| `CUBLAS_STATUS_SUCCESS` | If the attribute was set successfully. |

---  

### 3.4.52 `cublasLtEmulationDescGetAttribute()`  

```c
cublasStatus_t cublasLtEmulationDescGetAttribute(
    cublasLtEmulationDesc_t                emulationDesc,
    cublasLtEmulationDescAttributes_t      attr,
    void                                   *buf,
    size_t                                 sizeInBytes,
    size_t                                 *sizeWritten);
```

This function returns the value of the queried attribute belonging to a previously created emulation descriptor.  

**Parameters**  

| Parameter | Description |
|-----------|-------------|
| `emulationDesc` | Input – Pointer to the previously created structure holding the emulation descriptor queried by this function. See `cublasLtEmulationDesc_t`. |
| `attr` | Input – The attribute that will be retrieved by this function. See `cublasLtEmulationDescAttributes_t`. |
| `buf` | Output – Memory address containing the attribute value retrieved by this function. |
| `sizeInBytes` | Input – Size of `buf` buffer (in bytes) for verification. |
| `sizeWritten` | Output – Valid only when the return value is `CUBLAS_STATUS_SUCCESS`. If `sizeInBytes` is non‑zero: then `sizeWritten` is the number of bytes actually written; if `sizeInBytes` is 0: then `sizeWritten` is the number of bytes needed to write full contents. |

**Returns**  

| Return Value | Meaning |
|--------------|---------|
| `CUBLAS_STATUS_INVALID_VALUE` | * If `sizeInBytes` is 0 and `sizeWritten` is NULL, or * if `sizeInBytes` is non‑zero and `buf` is NULL, or * if `sizeInBytes` doesn’t match size of internal storage for the selected attribute |
| `CUBLAS_STATUS_SUCCESS` | If attribute’s value was successfully written to user memory. |

---  