## 1.  `cublasSgeam`

```
cublasStatus_t cublasSgeam(cublasHandle_t handle,
                           cublasOperation_t transa,
                           cublasOperation_t transb,
                           int m,
                           int n,
                           const float *alpha,
                           const float *A,
                           int lda,
                           const float *B,
                           int ldb,
                           float *C,
                           int ldc)
```

**Description** – Performs the matrix‑matrix addition/transposition  

\[
C = \alpha \, \text{op}(A) + \beta \, \text{op}(B)
\]

where `op(X)` is `X`, `Xᵀ` or `Xᴴ` according to `transa`/`transb`.  
Special cases: setting `α = β = 0` zeroes `C`; setting `α = 1, β = 0` copies `A` to `C`; setting `α = 0, β = 1` copies `B` to `C`.

| Parameter | Memory | In/Out | Meaning |
|---|---|---|---|
| `handle` | device | in | cuBLAS library context |
| `transa` | device | in | operation on `A` (`CUBLAS_OP_N`, `CUBLAS_OP_T`, `CUBLAS_OP_C`) |
| `transb` | device | in | operation on `B` |
| `m` | host | in | rows of `op(A)` and `C` |
| `n` | host | in | cols of `op(B)` and `C` |
| `alpha` | host | in | scalar multiplier for `op(A)` |
| `A` | device | in | matrix `A` (column‑major) |
| `lda` | host | in | leading dimension of `A` |
| `B` | device | in | matrix `B` (column‑major) |
| `ldb` | host | in | leading dimension of `B` |
| `beta` | host | in | scalar multiplier for `op(B)` |
| `C` | device | in/out | matrix `C` (column‑major) |
| `ldc` | host | in | leading dimension of `C` |

**Return values**

| Error value | Meaning |
|---|---|
| `CUBLAS_STATUS_SUCCESS` | operation completed |
| `CUBLAS_STATUS_NOT_INITIALIZED` | cuBLAS not initialized |
| `CUBLAS_STATUS_INVALID_VALUE` | illegal dimensions or leading‑dimension |
| `CUBLAS_STATUS_EXECUTION_FAILED` | kernel launch failed |

---  

## 2.  `cublasDgeam`

Same prototype as `cublasSgeam` with `double` types.

---  

## 3.  `cublasCgeam`

Same prototype as `cublasSgeam` with `cuComplex` types.

---  

## 4.  `cublasZgeam`

Same prototype as `cublasSgeam` with `cuDoubleComplex` types.

---  

## 5.  `cublasSdgmm`

```
cublasStatus_t cublasSdgmm(cublasHandle_t handle,
                           cublasSideMode_t mode,
                           int m,
                           int n,
                           const float *A,
                           int lda,
                           const float *x,
                           int incx,
                           float *C,
                           int ldc)
```

**Description** – Performs a matrix‑matrix multiplication with a diagonal matrix  

\[
C = A \, \text{diag}(x) \quad (\text{or } C = \text{diag}(x) \, A)
\]

depending on `mode` (`CUBLAS_SIDE_LEFT` or `CUBLAS_SIDE_RIGHT`).  
`x` is a vector that supplies the diagonal elements.

| Parameter | Memory | In/Out | Meaning |
|---|---|---|---|
| `mode` | host | in | `CUBLAS_SIDE_LEFT` or `CUBLAS_SIDE_RIGHT` |
| `m` | host | in | rows of `C` |
| `n` | host | in | cols of `C` |
| `A` | device | in | left‑hand matrix |
| `lda` | host | in | leading dimension of `A` |
| `x` | device | in | diagonal vector |
| `incx` | host | in | stride of `x` |
| `C` | device | out | result matrix |
| `ldc` | host | in | leading dimension of `C` |

**Return values** – Same as `cublasSgeam`.

---  

## 6.  `cublasDdgmm`

Same prototype as `cublasSdgmm` with `double` types.

---  

## 7.  `cublasCdgmm`

Same prototype as `cublasSdgmm` with `cuComplex` types.

---  

## 8.  `cublasZdgmm`

Same prototype as `cublasSdgmm` with `cuDoubleComplex` types.

---  

## 9.  `cublasSgetrfBatched`

```
cublasStatus_t cublasSgetrfBatched(cublasHandle_t handle,
                                   int n,
                                   float *const Aarray[],
                                   int lda,
                                   int *PivotArray,
                                   int *InfoArray,
                                   int batchSize)
```

**Description** – Batched LU factorisation of an array of `n×n` matrices.  
For each `i` it computes the factorisation  

\[
P_i A_i = L_i U_i
\]

and stores the pivots in `PivotArray[i]`, the info flag in `InfoArray[i]`.

| Parameter | Memory | In/Out | Meaning |
|---|---|---|---|
| `n` | host | in | dimension of each square matrix |
| `Aarray` | device | in | array of pointers to each matrix |
| `lda` | host | in | leading dimension of each matrix |
| `PivotArray` | device | out | concatenated pivot arrays |
| `InfoArray` | device | out | status of each factorisation |
| `batchSize` | host | in | number of matrices |

**Return values** – `CUBLAS_STATUS_SUCCESS`, `CUBLAS_STATUS_INVALID_VALUE`, `CUBLAS_STATUS_EXECUTION_FAILED`.

---  

## 10.  `cublasDgetrfBatched`

Same prototype as `cublasSgetrfBatched` with `double` types.

---  

## 11.  `cublasCgetrfBatched`

Same prototype as `cublasSgetrfBatched` with `cuComplex` types.

---  

## 12.  `cublasZgetrfBatched`

Same prototype as `cublasSgetrfBatched` with `cuDoubleComplex` types.

---  

## 13.  `cublasSgetrsBatched`

```
cublasStatus_t cublasSgetrsBatched(cublasHandle_t handle,
                                   cublasOperation_t trans,
                                   int n,
                                   int nrhs,
                                   const float *const Aarray[],
                                   int lda,
                                   const int *devIpiv,
                                   float *const Barray[],
                                   int ldb,
                                   int *info,
                                   int batchSize)
```

**Description** – Batched solve of linear systems  
\( \text{op}(A_i) X_i = B_i \) using the LU factors computed by `getrfBatched`.

| Parameter | Meaning |
|---|---|
| `trans` | operation (`N`, `T`, `C`) |
| `n` | order of matrices |
| `nrhs` | number of right‑hand sides |
| `Aarray` | pointers to each `A_i` |
| `lda` | leading dimension of each `A_i` |
| `devIpiv` | device array of pivot indices |
| `Barray` | pointers to each `B_i` |
| `ldb` | leading dimension of each `B_i` |
| `info` | output status array |
| `batchSize` | number of systems |

---  

## 14.  `cublasDgetrsBatched`

Same prototype as `cublasSgetrsBatched` with `double` types.

---  

## 15.  `cublasCgetrsBatched`

Same prototype as `cublasSgetrsBatched` with `cuComplex` types.

---  

## 16.  `cublasZgetrsBatched`

Same prototype as `cublasSgetrsBatched` with `cuDoubleComplex` types.

---  

## 17.  `cublasSgetriBatched`

```
cublasStatus_t cublasSgetriBatched(cublasHandle_t handle,
                                   int n,
                                   const float *const Aarray[],
                                   int lda,
                                   int *PivotArray,
                                   float *const Carray[],
                                   int ldc,
                                   int *infoArray,
                                   int batchSize)
```

**Description** – Batched inversion of matrices that were previously factorised with `getrfBatched`.  
Each output matrix `C_i = A_i^{-1}`.

| Parameter | Meaning |
|---|---|
| `PivotArray` | pivot array from `getrfBatched` |
| `Carray` | output pointers to each inverse matrix |
| `ldc` | leading dimension of each `C_i` |
| `infoArray` | status of each inversion |

---  

## 18.  `cublasDgetriBatched`

Same prototype as `cublasSgetriBatched` with `double` types.

---  

## 19.  `cublasCgetriBatched`

Same prototype as `cublasSgetriBatched` with `cuComplex` types.

---  

## 20.  `cublasZgetriBatched`

Same prototype as `cublasSgetriBatched` with `cuDoubleComplex` types.

---  

## 21.  `cublasSmatinvBatched`

```
cublasStatus_t cublasSmatinvBatched(cublasHandle_t handle,
                                    int n,
                                    const float *const A[],
                                    int lda,
                                    float *const Ainv[],
                                    int lda_inv,
                                    int *info,
                                    int batchSize)
```

**Description** – Shortcut that performs `getrfBatched` followed by `getriBatched`.  
Only works for `n ≤ 32`.

| Parameter | Meaning |
|---|---|
| `A` | array of pointers to input matrices |
| `Ainv` | array of pointers to output inverse matrices |
| `info` | status array |

---  

## 22.  `cublasDmatinvBatched`

Same prototype as `cublasSmatinvBatched` with `double` types.

---  

## 23.  `cublasCmatinvBatched`

Same prototype as `cublasSmatinvBatched` with `cuComplex` types.

---  

## 24.  `cublasZmatinvBatched`

Same prototype as `cublasSmatinvBatched` with `cuDoubleComplex` types.

---  

## 25.  `cublasSgeqrfBatched`

```
cublasStatus_t cublasSgeqrfBatched(cublasHandle_t handle,
                                   int m,
                                   int n,
                                   float *const Aarray[],
                                   int lda,
                                   float *const TauArray[],
                                   int *info,
                                   int batchSize)
```

**Description** – Batched QR factorisation using Householder reflections.  
Each matrix `A_i` (size `m×n`) is reduced to upper‑triangular `R_i` and the Householder vectors are stored in `TauArray`.

| Parameter | Meaning |
|---|---|
| `TauArray` | array of pointers to each scalar `tau` vector |
| `info` | status array |

---  

## 26.  `cublasDgeqrfBatched`

Same prototype as `cublasSgeqrfBatched` with `double` types.

---  

## 27.  `cublasCgeqrfBatched`

Same prototype as `cublasSgeqrfBatched` with `cuComplex` types.

---  

## 28.  `cublasZgeqrfBatched`

Same prototype as `cublasSgeqrfBatched` with `cuDoubleComplex` types.

---  

## 29.  `cublasSgelsBatched`

```
cublasStatus_t cublasSgelsBatched(cublasHandle_t handle,
                                  cublasOperation_t trans,
                                  int m,
                                  int n,
                                  int nrhs,
                                  float *const Aarray[],
                                  int lda,
                                  float *const Carray[],
                                  int ldc,
                                  int *info,
                                  int *devInfoArray,
                                  int batchSize)
```

**Description** – Batched least‑squares solver for over‑determined systems.  
Solves  

\[
\min_x \| C_i - A_i x \|_2
\]

for each batch element.  Only the **non‑transpose** case (`trans = CUBLAS_OP_N`) is supported.

| Parameter | Meaning |
|---|---|
| `info` | per‑batch success flag |
| `devInfoArray` | device‑side version of `info` |

---  

## 30.  `cublasDgelsBatched`

Same prototype as `cublasSgelsBatched` with `double` types.

---  

## 31.  `cublasCgelsBatched`

Same prototype as `cublasSgelsBatched` with `cuComplex` types.

---  

## 32.  `cublasZgelsBatched`

Same prototype as `cublasSgelsBatched` with `cuDoubleComplex` types.

---  

## 33.  `cublasStpttr`

```
cublasStatus_t cublasStpttr(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            int n,
                            const float *AP,
                            float *A,
                            int lda)
```

**Description** – Converts a triangular matrix stored in packed format to a full triangular matrix.

| Parameter | Meaning |
|---|---|
| `uplo` | whether the stored part is lower or upper |
| `AP` | packed input (`n(n+1)/2` elements) |
| `A` | full matrix output |
| `lda` | leading dimension of `A` |

---  

## 34.  `cublasDtpttr`

Same prototype as `cublasStpttr` with `double` types.

---  

## 35.  `cublasCtpttr`

Same prototype as `cublasStpttr` with `cuComplex` types.

---  

## 36.  `cublasZtpttr`

Same prototype as `cublasStpttr` with `cuDoubleComplex` types.

---  

## 37.  `cublasStrttp`

```
cublasStatus_t cublasStrttp(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            int n,
                            const float *A,
                            int lda,
                            float *AP)
```

**Description** – Converts a triangular matrix in full format to packed format.

| Parameter | Meaning |
|---|---|
| `uplo` | whether the stored part is lower or upper |
| `A` | full matrix input |
| `AP` | packed output |

---  

## 38.  `cublasDtrttp`

Same prototype as `cublasStrttp` with `double` types.

---  

## 39.  `cublasCtrttp`

Same prototype as `cublasStrttp` with `cuComplex` types.

---  

## 40.  `cublasZtrttp`

Same prototype as `cublasStrttp` with `cuDoubleComplex` types.

---  

## 41.  `cublasSgemmEx`

```
cublasStatus_t cublasSgemmEx(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const float *alpha,
                             const void *A,
                             cudaDataType Atype,
                             int lda,
                             const void *B,
                             cudaDataType Btype,
                             int ldb,
                             const float *beta,
                             void *C,
                             cudaDataType Ctype,
                             int ldc)
```

**Description** – Extended GEMM where each matrix may have a different data type.  
The computation is performed in the type given by `computeType` (see below).

| Parameter | Meaning |
|---|---|
| `Atype`, `Btype`, `Ctype` | data‑type enums (`CUDA_R_16F`, `CUDA_R_32F`, …) |
| `alpha`, `beta` | scalars of the *compute* type |
| `computeType` | precision used for accumulation (implicitly `float` for `SgemmEx`) |

---  

## 42.  `cublasGemmEx`

Same prototype as `cublasSgemmEx` with the appropriate type (`double`, `cuComplex`, …).  
Additional parameters `cublasComputeType_t computeType` and `cublasGemmAlgo_t algo` allow selection of accumulation precision and algorithm.

---  

## 43.  `cublasGemmBatchedEx`

```
cublasStatus_t cublasGemmBatchedEx(cublasHandle_t handle,
                                   cublasOperation_t transa,
                                   cublasOperation_t transb,
                                   int m,
                                   int n,
                                   int k,
                                   const void *alpha,
                                   const void *const Aarray[],
                                   cudaDataType Atype,
                                   int lda,
                                   const void *const Barray[],
                                   cudaDataType Btype,
                                   int ldb,
                                   const void *beta,
                                   void *const Carray[],
                                   cudaDataType Ctype,
                                   int ldc,
                                   int batchCount,
                                   cublasComputeType_t computeType,
                                   cublasGemmAlgo_t algo)
```

**Description** – Batched version of `GemmEx`.  Allows each matrix in the batch to have its own pointer and stride.

---  

## 44.  `cublasGemmStridedBatchedEx`

```
cublasStatus_t cublasGemmStridedBatchedEx(cublasHandle_t handle,
                                          cublasOperation_t transa,
                                          cublasOperation_t transb,
                                          int m,
                                          int n,
                                          int k,
                                          const void *alpha,
                                          const void *A,
                                          cudaDataType Atype,
                                          int lda,
                                          long long int strideA,
                                          const void *B,
                                          cudaDataType Btype,
                                          int ldb,
                                          long long int strideB,
                                          const void *beta,
                                          void *C,
                                          cudaDataType Ctype,
                                          int ldc,
                                          long long int strideC,
                                          int batchCount,
                                          cublasComputeType_t computeType,
                                          cublasGemmAlgo_t algo)
```

**Description** – Same as `GemmEx` but supports strided batches via `strideA`, `strideB`, `strideC`.

---  

## 45.  `cublasGemmGroupedBatchedEx`

```
cublasStatus_t cublasGemmGroupedBatchedEx(cublasHandle_t handle,
                                          const cublasOperation_t transa_array[],
                                          const cublasOperation_t transb_array[],
                                          const int m_array[],
                                          const int n_array[],
                                          const int k_array[],
                                          const void *alpha_array[],
                                          const void *const Aarray[],
                                          cudaDataType Atype,
                                          const int lda_array[],
                                          const void *const Barray[],
                                          cudaDataType Btype,
                                          const int ldb_array[],
                                          const void *beta_array[],
                                          void *const Carray[],
                                          cudaDataType Ctype,
                                          const int ldc_array[],
                                          int group_count,
                                          const int group_size[],
                                          cublasComputeType_t computeType)
```

**Description** – GEMM over *groups* of matrices where each group may have a different size/shape.

---  

## 46.  `cublasCsyrkEx`

```
cublasStatus_t cublasCsyrkEx(cublasHandle_t handle,
                             cublasFillMode_t uplo,
                             cublasOperation_t trans,
                             int n,
                             int k,
                             const cuComplex *alpha,
                             const void *A,
                             cudaDataType Atype,
                             int lda,
                             const cuComplex *beta,
                             cuComplex *C,
                             cudaDataType Ctype,
                             int ldc)
```

**Description** – Symmetric rank‑k update with complex matrices, supporting mixed‑precision input/output.

---  

## 47.  `cublasCsyrk3mEx`

Same prototype as `cublasCsyrkEx` with additional `beta` handling for the 3‑m algorithm; details omitted for brevity.

---  

## 48.  `cublasCherkEx`

Complex Hermitian rank‑k update (mixed precision).

---  

## 49.  `cublasCherk3mEx`

Complex Hermitian rank‑2k update (mixed precision).

---  

## 50.  `cublasNrm2Ex`

```
cublasStatus_t cublasNrm2Ex(cublasHandle_t handle,
                            int n,
                            const void *x,
                            cudaDataType xType,
                            int incx,
                            void *result,
                            cudaDataType resultType,
                            cudaDataType executionType)
```

**Description** – Computes the Euclidean norm of a vector with independent input, result and execution data types.

---  

## 51.  `cublasAxpyEx`

```
cublasStatus_t cublasAxpyEx(cublasHandle_t handle,
                            int n,
                            const void *alpha,
                            cudaDataType alphaType,
                            const void *x,
                            cudaDataType xType,
                            int incx,
                            void *y,
                            cudaDataType yType,
                            intincy,
                            cudaDataType executionType)
```

**Description** – Scales a vector and adds it to another vector, with independent data types.

---  

## 52.  `cublasDotEx`

```
cublasStatus_t cublasDotEx(cublasHandle_t handle,
                           int n,
                           const void *x,
                           cudaDataType xType,
                           int incx,
                           const void *y,
                           cudaDataType yType,
                           intincy,
                           void *result,
                           cudaDataType resultType,
                           cudaDataType executionType)
```

**Description** – Computes the dot product of two vectors with independent types.

---  

## 53.  `cublasRotEx`

```
cublasStatus_t cublasRotEx(cublasHandle_t handle,
                           int n,
                           void *x,
                           cudaDataType xType,
                           int incx,
                           void *y,
                           cudaDataType yType,
                           intincy,
                           const void *c,
                           cudaDataType cType,
                           const void *s,
                           cudaDataType sType,
                           cudaDataType executionType)
```

**Description** – Applies a Givens rotation to two vectors.

---  

## 54.  `cublasScalEx`

```
cublasStatus_t cublasScalEx(cublasHandle_t handle,
                            int n,
                            const void *alpha,
                            cudaDataType alphaType,
                            void *x,
                            cudaDataType xType,
                            int incx,
                            cudaDataType executionType)
```

**Description** – Scales a vector in place.
