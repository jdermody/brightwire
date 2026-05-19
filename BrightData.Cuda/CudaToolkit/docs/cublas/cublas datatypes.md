## cublasHandle_t

*Pointer to an opaque structure that holds the cuBLAS library context.*  
This type is used to create, configure, and destroy a cuBLAS context. It must be passed to every cuBLAS function call.

| Value | Meaning |
|---|---|
| *(none)* | No distinct values – it is an opaque pointer type. |

---

## cublasStatus_t

*Enumeration used for returning the status of cuBLAS API calls.*

| Value | Meaning |
|---|---|
| `CUBLAS_STATUS_SUCCESS` | Operation completed successfully. |
| `CUBLAS_STATUS_NOT_INITIALIZED` | cuBLAS was not initialized. |
| `CUBLAS_STATUS_ALLOC_FAILED` | Resource allocation failed (usually `cudaMalloc` failure). |
| `CUBLAS_STATUS_INVALID_VALUE` | An unsupported value or parameter was passed. |
| `CUBLAS_STATUS_ARCH_MISMATCH` | Function requires a feature absent from the device architecture. |
| `CUBLAS_STATUS_MAPPING_ERROR` | Access to GPU memory space failed. |
| `CUBLAS_STATUS_EXECUTION_FAILED` | GPU program failed to execute. |
| `CUBLAS_STATUS_INTERNAL_ERROR` | An internal cuBLAS operation failed. |
| `CUBLAS_STATUS_NOT_SUPPORTED` | Requested functionality is not supported. |
| `CUBLAS_STATUS_LICENSE_ERROR` | License error detected. |

---

## cublasOperation_t

*Specifies which operation to perform on dense matrices (transpose, conjugate‑transpose, or none).*

| Value | Meaning |
|---|---|
| `CUBLAS_OP_N` | Non‑transpose operation. |
| `CUBLAS_OP_T` | Transpose operation. |
| `CUBLAS_OP_C` | Conjugate transpose operation. |

---

## cublasFillMode_t

*Specifies which part of a symmetric/hermitian matrix is stored.*

| Value | Meaning |
|---|---|
| `CUBLAS_FILL_MODE_LOWER` | Lower triangular part is stored. |
| `CUBLAS_FILL_MODE_UPPER` | Upper triangular part is stored. |
| `CUBLAS_FILL_MODE_FULL` | Full matrix is stored. |

---

## cublasDiagType_t

*Indicates whether the main diagonal of a matrix is unit (should not be referenced).*

| Value | Meaning |
|---|---|
| `CUBLAS_DIAG_NON_UNIT` | Diagonal elements are non‑unit. |
| `CUBLAS_DIAG_UNIT` | Diagonal elements are unit. |

---

## cublasSideMode_t

*Specifies whether a matrix operand is on the left or right side of the operation.*

| Value | Meaning |
|---|---|
| `CUBLAS_SIDE_LEFT` | Matrix is on the left side. |
| `CUBLAS_SIDE_RIGHT` | Matrix is on the right side. |

---

## cublasPointerMode_t

*Specifies how scalar parameters are passed to cuBLAS functions.*

| Value | Meaning |
|---|---|
| `CUBLAS_POINTER_MODE_HOST` | Scalars are passed by reference on the host. |
| `CUBLAS_POINTER_MODE_DEVICE` | Scalars are passed by reference on the device. |

---

## cublasAtomicsMode_t

*Controls whether atomic operations may be used by certain cuBLAS routines.*

| Value | Meaning |
|---|---|
| `CUBLAS_ATOMICS_NOT_ALLOWED` | Atomics are not allowed. |
| `CUBLAS_ATOMICS_ALLOWED` | Atomics are allowed. |

---

## cublasGemmAlgo_t

*Enumerates GEMM algorithm selection options.*

| Value | Meaning |
|---|---|
| `CUBLAS_GEMM_DEFAULT` | Let cuBLAS heuristics select the best algorithm. |
| `CUBLAS_GEMM_ALGO0` – `CUBLAS_GEMM_ALGO23` | Explicitly choose algorithm 0 through 23. |
| `CUBLAS_GEMM_DEFAULT_TENSOR_OP` | Deprecated tensor‑operation mode (kept for compatibility). |
| `CUBLAS_GEMM_AUTOTUNE` | Enable automatic tuning; results are cached. |

---

## cublasMath_t

*Selects the compute‑precision mode for cuBLAS routines.*

| Value | Meaning |
|---|---|
| `CUBLAS_DEFAULT_MATH` | Default highest‑performance mode (uses tensor cores when possible). |
| `CUBLAS_PEDANTIC_MATH` | Use precise arithmetic; may be slower. |
| `CUBLAS_TENSOR_OP_MATH` | Enable tensor‑core operations (deprecated). |
| `CUBLAS_FP32_EMULATED_BF16X9_MATH` | Enable BF16×9 emulation for single precision. |
| `CUBLAS_FP64_EMULATED_FIXEDPOINT_MATH` | Enable fixed‑point emulation for double precision. |
| `CUBLAS_MATH_DISALLOW_REDUCED_PRECISION_REDUCTION` | Disallow reduced‑precision reduction in mixed‑precision GEMM. |
| `CUBLAS_TENSOR_OP_MATH` | Enable Tensor‑Op math (deprecated). |

---

## cublasComputeType_t

*Selects the compute precision used for GEMM operations.*

| Value | Meaning |
|---|---|
| `CUBLAS_COMPUTE_16F` | 16‑bit half precision (default high‑performance). |
| `CUBLAS_COMPUTE_16F_PEDANTIC` | 16‑bit half precision with pedantic checks. |
| `CUBLAS_COMPUTE_32F` | 32‑bit single precision (default). |
| `CUBLAS_COMPUTE_32F_PEDANTIC` | 32‑bit single precision with pedantic checks. |
| `CUBLAS_COMPUTE_32F_FAST_16F` | Use Tensor Cores with fast 16‑bit compute for 32‑bit inputs/outputs. |
| `CUBLAS_COMPUTE_32F_FAST_16BF` | Use Tensor Cores with fast bfloat16 compute for 32‑bit inputs/outputs. |
| `CUBLAS_COMPUTE_32F_FAST_TF32` | Use Tensor Cores with TF32 compute for 32‑bit inputs/outputs. |
| `CUBLAS_COMPUTE_32F_EMULATED_16BFX9` | Use BF16×9 emulation for 32‑bit arithmetic. |
| `CUBLAS_COMPUTE_64F` | 64‑bit double precision (default). |
| `CUBLAS_COMPUTE_64F_PEDANTIC` | 64‑bit double precision with pedantic checks. |
| `CUBLAS_COMPUTE_64F_EMULATED_FIXEDPOINT` | Fixed‑point emulation for double precision. |
| `CUBLAS_COMPUTE_32I` | 32‑bit integer compute mode. |
| `CUBLAS_COMPUTE_32I_PEDANTIC` | 32‑bit integer compute with pedantic checks. |

---

## cublasEmulationStrategy_t

*Selects how floating‑point emulation algorithms are applied.*

| Value | Meaning |
|---|---|
| `CUBLAS_EMULATION_STRATEGY_DEFAULT` | Default strategy (equivalent to `PERFORMANT` unless overridden). |
| `CUBLAS_EMULATION_STRATEGY_PERFORMANT` | Use emulation whenever it yields a performance benefit. |
| `CUBLAS_EMULATION_STRATEGY_EAGER` | Use emulation whenever it is possible. |

---

## cudaEmulationSpecialValuesSupport_t

*Specifies which special floating‑point values (Inf, NaN) emulation should support.*

| Value | Meaning |
|---|---|
| `CUDA_EMULATION_SPECIAL_VALUES_SUPPORT_DEFAULT` | Support signed infinities and NaN (default). |
| `CUDA_EMULATION_SPECIAL_VALUES_SUPPORT_NONE` | Do not require support for special values. |
| `CUDA_EMULATION_SPECIAL_VALUES_SUPPORT_INFINITY` | Require support for signed infinities. |
| `CUDA_EMULATION_SPECIAL_VALUES_SUPPORT_NAN` | Require support for NaN. |

---

## cudaEmulationMantissaControl_t

*Controls how mantissa bits are chosen for fixed‑point emulation.*

| Value | Meaning |
|---|---|
| `CUDA_EMULATION_MANTISSA_CONTROL_DYNAMIC` | Compute mantissa bits dynamically at runtime. |
| `CUDA_EMULATION_MANTISSA_CONTROL_FIXED` | Use a fixed mantissa‑bit count. |

---

## libraryPropertyType_t

*Enumerates properties that can be queried with `cublasGetProperty`.*

| Value | Meaning |
|---|---|
| `MAJOR_VERSION` | Major version number. |
| `MINOR_VERSION` | Minor version number. |
| `PATCH_LEVEL` | Patch level number. |

---

## cudaDataType_t (CUDA Datatypes)

*Enumerates data‑type precisions used by CUDA libraries.*

| Value | Meaning |
|---|---|
| `CUDA_R_16F` | 16‑bit real half‑precision floating point. |
| `CUDA_C_16F` | 32‑bit complex type consisting of two `CUDA_R_16F` values. |
| `CUDA_R_16BF` | 16‑bit real bfloat16 floating point. |
| `CUDA_C_16BF` | 32‑bit complex type consisting of two `CUDA_R_16BF` values. |
| `CUDA_R_32F` | 32‑bit real single‑precision floating point. |
| `CUDA_C_32F` | 64‑bit complex type consisting of two `CUDA_R_32F` values. |
| `CUDA_R_64F` | 64‑bit real double‑precision floating point. |
| `CUDA_C_64F` | 128‑bit complex type consisting of two `CUDA_R_64F` values. |
| `CUDA_R_8I` | 8‑bit real signed integer. |
| `CUDA_C_8I` | 16‑bit complex type of two `CUDA_R_8I` values. |
| `CUDA_R_8U` | 8‑bit real unsigned integer. |
| `CUDA_C_8U` | 16‑bit complex type of two `CUDA_R_8U` values. |
| `CUDA_R_32I` | 32‑bit real signed integer. |
| `CUDA_C_32I` | 64‑bit complex type of two `CUDA_R_32I` values. |
| `CUDA_R_8F_E4M3` | 8‑bit real floating point with 4‑bit exponent and 3‑bit mantissa (E4M3). |
| `CUDA_R_8F_E5M2` | 8‑bit real floating point with 5‑bit exponent and 2‑bit mantissa (E5M2). |
| `CUDA_R_4F_E2M1` | 4‑bit real floating point with 2‑bit exponent and 1‑bit mantissa (E2M1). |

---

## cudaEmulationMantissaControl_t

*Controls mantissa‑bit handling for floating‑point emulation.*

| Value | Meaning |
|---|---|
| `CUDA_EMULATION_MANTISSA_CONTROL_DYNAMIC` | Mantissa bits are computed dynamically at runtime. |
| `CUDA_EMULATION_MANTISSA_CONTROL_FIXED` | Mantissa bits are fixed at a user‑specified value. |

---

## cudaEmulationSpecialValuesSupport_t

*Bitmask specifying which special floating‑point values must be supported by emulation.*

| Value | Meaning |
|---|---|
| `CUDA_EMULATION_SPECIAL_VALUES_SUPPORT_DEFAULT` | Support signed infinities and NaN (default). |
| `CUDA_EMULATION_SPECIAL_VALUES_SUPPORT_NONE` | No special‑value support required. |
| `CUDA_EMULATION_SPECIAL_VALUES_SUPPORT_INFINITY` | Must support signed infinities. |
| `CUDA_EMULATION_SPECIAL_VALUES_SUPPORT_NAN` | Must support NaN. |

---