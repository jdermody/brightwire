## cublasSgemm  

```c
cublasStatus_t cublasSgemm(cublasHandle_t handle,
                           cublasOperation_t transa,
                           cublasOperation_t transb,
                           int m,
                           int n,
                           int k,
                           const float *alpha,
                           const float *A, int lda,
                           const float *B, int ldb,
                           const float *beta,
                           float *C, int ldc)
```

**Description** – Performs single‑precision general matrix‑matrix multiplication  
\(C = \alpha \, \text{op}(A)\,\text{op}(B) + \beta\,C\) where `op(A)` and `op(B)` are
`A`, `Aᵀ`, or `Aᴴ` as specified by `transa`/`transb`.

| Parameter | Meaning |
|---|---|
| **handle** | CUBLAS handle (input) |
| **transa** | Operation on matrix *A* (`CUBLAS_OP_N`, `CUBLAS_OP_T`, `CUBLAS_OP_C`) |
| **transb** | Operation on matrix *B* (`CUBLAS_OP_N`, `CUBLAS_OP_T`, `CUBLAS_OP_C`) |
| **m** | Number of rows of `op(A)` and `C` (input) |
| **n** | Number of columns of `op(B)` and `C` (input) |
| **k** | Number of columns of `op(A)` and rows of `op(B)` (input) |
| **alpha** | Scalar multiplier for `op(A)·op(B)` (device or host pointer) |
| **A** | Matrix `A` stored column‑major, dimension `lda × k` (or `lda × m` if transposed) |
| **lda** | Leading dimension of `A` |
| **B** | Matrix `B` stored column‑major, dimension `ldb × n` (or `ldb × k` if transposed) |
| **ldb** | Leading dimension of `B` |
| **beta** | Scalar multiplier for `C` (device or host pointer) |
| **C** | Matrix `C` stored column‑major, dimension `ldc × n` |
| **ldc** | Leading dimension of `C` |

### Return Values  

| Error Value | Meaning |
|---|---|
| `CUBLAS_STATUS_SUCCESS` | Operation completed successfully |
| `CUBLAS_STATUS_NOT_INITIALIZED` | Library not initialized |
| `CUBLAS_STATUS_INVALID_VALUE` | Invalid argument (e.g., negative size) |
| `CUBLAS_STATUS_EXECUTION_FAILED` | Kernel launch failed |
| `CUBLAS_STATUS_ARCH_MISMATCH` | Architecture does not support the requested precision |
| `CUBLAS_STATUS_NOT_SUPPORTED` | Combination of types/algo not supported |

---  

## cublasDgemm  

```c
cublasStatus_t cublasDgemm(cublasHandle_t handle,
                           cublasOperation_t transa,
                           cublasOperation_t transb,
                           int m,
                           int n,
                           int k,
                           const double *alpha,
                           const double *A, int lda,
                           const double *B, int ldb,
                           const double *beta,
                           double *C, int ldc)
```

*Same semantics as `cublasSgemm`, but with double‑precision arrays.*

---  

## cublasCgemm  

```c
cublasStatus_t cublasCgemm(cublasHandle_t handle,
                           cublasOperation_t transa,
                           cublasOperation_t transb,
                           int m,
                           int n,
                           int k,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *B, int ldb,
                           const cuComplex *beta,
                           cuComplex *C, int ldc)
```

*Complex‑single precision version of `cublasSgemm`.*

---  

## cublasZgemm  

```c
cublasStatus_t cublasZgemm(cublasHandle_t handle,
                           cublasOperation_t transa,
                           cublasOperation_t transb,
                           int m,
                           int n,
                           int k,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *B, int ldb,
                           const cuDoubleComplex *beta,
                           cuDoubleComplex *C, int ldc)
```

*Complex‑double precision version of `cublasDgemm`.*

---  

## cublasHgemm  

```c
cublasStatus_t cublasHgemm(cublasHandle_t handle,
                           cublasOperation_t transa,
                           cublasOperation_t transb,
                           int m,
                           int n,
                           int k,
                           const __half *alpha,
                           const __half *A, int lda,
                           const __half *B, int ldb,
                           const __half *beta,
                           __half *C, int ldc)
```

*Half‑precision version of `cublasSgemm`. Requires GPU architecture that supports FP16
matrix‑multiply (Volta+).*

---  

## cublasSgemmBatched  

```c
cublasStatus_t cublasSgemmBatched(cublasHandle_t handle,
                                cublasOperation_t transa,
                                cublasOperation_t transb,
                                int m,
                                int n,
                                int k,
                                const float *const alpha,
                                const float *const *Aarray,
                                int lda,
                                const float *const Barray[],
                                int ldb,
                                const float *beta,
                                float *const Carray[],
                                int ldc,
                                int batchCount)
```

*Performs a batch of `batchCount` independent `cublasSgemm` operations.  The
matrices are taken from arrays of pointers (`Aarray`, `Barray`, `Carray`).*

---  

## cublasDgemmBatched  

```c
cublasStatus_t cublasDgemmBatched(cublasHandle_t handle,
                                cublasOperation_t transa,
                                cublasOperation_t transb,
                                int m,
                                int n,
                                int k,
                                const double *const alpha,
                                const double *const Aarray[],
                                int lda,
                                const double *const Barray[],
                                int ldb,
                                const double *beta,
                                double *const Carray[],
                                int ldc,
                                int batchCount)
```

*Double‑precision batched `cublasDgemm`.*

---  

## cublasCgemmBatched  

```c
cublasStatus_t cublasCgemmBatched(cublasHandle_t handle,
                                cublasOperation_t transa,
                                cublasOperation_t transb,
                                int m,
                                int n,
                                int k,
                                const cuComplex *const alpha,
                                const cuComplex *const Aarray[],
                                int lda,
                                const cuComplex *const Barray[],
                                int ldb,
                                const cuComplex *beta,
                                cuComplex *const Carray[],
                                int ldc,
                                int batchCount)
```

*Complex‑single precision batched `cublasCgemm`.*

---  

## cublasZgemmBatched  

```c
cublasStatus_t cublasZgemmBatched(cublasHandle_t handle,
                                cublasOperation_t transa,
                                cublasOperation_t transb,
                                int m,
                                int n,
                                int k,
                                const cuDoubleComplex *const alpha,
                                const cuDoubleComplex *const Aarray[],
                                int lda,
                                const cuDoubleComplex *const Barray[],
                                int ldb,
                                const cuDoubleComplex *beta,
                                cuDoubleComplex *const Carray[],
                                int ldc,
                                int batchCount)
```

*Complex‑double precision batched `cublasZgemm`.*

---  

## cublasHgemmBatched  

```c
cublasStatus_t cublasHgemmBatched(cublasHandle_t handle,
                                cublasOperation_t transa,
                                cublasOperation_t transb,
                                int m,
                                int n,
                                int k,
                                const __half *const alpha,
                                const __half *const Aarray[],
                                int lda,
                                const __half *const Barray[],
                                int ldb,
                                const __half *beta,
                                __half *const Carray[],
                                int ldc,
                                int batchCount)
```

*Half‑precision batched `cublasHgemm`.*

---  

## cublasSgemmStridedBatched  

```c
cublasStatus_t cublasSgemmStridedBatched(cublasHandle_t handle,
                                       cublasOperation_t transa,
                                       cublasOperation_t transb,
                                       int m,
                                       int n,
                                       int k,
                                       const float *alpha,
                                       const float *A, int lda,
                                       long long int strideA,
                                       const float *B, int ldb,
                                       long long int strideB,
                                       const float *beta,
                                       float *C, int ldc,
                                       long long int strideC,
                                       int batchCount)
```

*Strided‑batched variant – matrices are stored with a fixed element offset
(`strideA`, `strideB`, `strideC`) between successive batches.*

---  

## cublasDgemmStridedBatched  

```c
cublasStatus_t cublasDgemmStridedBatched(cublasHandle_t handle,
                                       cublasOperation_t transa,
                                       cublasOperation_t transb,
                                       int m,
                                       int n,
                                       int k,
                                       const double *alpha,
                                       const double *A, int lda,
                                       long long int strideA,
                                       const double *B, int ldb,
                                       long long int strideB,
                                       const double *beta,
                                       double *C, int ldc,
                                       long long int strideC,
                                       int batchCount)
```

*Double‑precision strided‑batched `cublasDgemm`.*

---  

## cublasCgemmStridedBatched  

```c
cublasStatus_t cublasCgemmStridedBatched(cublasHandle_t handle,
                                       cublasOperation_t transa,
                                       cublasOperation_t transb,
                                       int m,
                                       int n,
                                       int k,
                                       const cuComplex *alpha,
                                       const cuComplex *A, int lda,
                                       long long int strideA,
                                       const cuComplex *B, int ldb,
                                       long long int strideB,
                                       const cuComplex *beta,
                                       cuComplex *C, int ldc,
                                       long long int strideC,
                                       int batchCount)
```

*Complex‑single precision strided‑batched `cublasCgemm`.*

---  

## cublasZgemmStridedBatched  

```c
cublasStatus_t cublasZgemmStridedBatched(cublasHandle_t handle,
                                       cublasOperation_t transa,
                                       cublasOperation_t transb,
                                       int m,
                                       int n,
                                       int k,
                                       const cuDoubleComplex *alpha,
                                       const cuDoubleComplex *A, int lda,
                                       long long int strideA,
                                       const cuDoubleComplex *B, int ldb,
                                       long long int strideB,
                                       const cuDoubleComplex *beta,
                                       cuDoubleComplex *C, int ldc,
                                       long long int strideC,
                                       int batchCount)
```

*Complex‑double precision strided‑batched `cublasZgemm`.*

---  

## cublasSgemmGroupedBatched  

```c
cublasStatus_t cublasSgemmGroupedBatched(cublasHandle_t handle,
                                       const cublasOperation_t transa_array[],
                                       const cublasOperation_t transb_array[],
                                       const int m_array[],
                                       const int n_array[],
                                       const int k_array[],
                                       const float *alpha_array[],
                                       const float *const *Aarray[],
                                       const int lda_array[],
                                       const float *const *Barray[],
                                       const int ldb_array[],
                                       const float *beta_array[],
                                       float *const Carray[],
                                       const int ldc_array[],
                                       int group_count,
                                       const int group_size[])
```

*Performs batched GEMM on groups of matrices where each group may have a
different size, leading dimension, or scaling factor.*

---  

## cublasDgemmGroupedBatched  

```c
cublasStatus_t cublasDgemmGroupedBatched(cublasHandle_t handle,
                                       const cublasOperation_t transa_array[],
                                       const cublasOperation_t transb_array[],
                                       const int m_array[],
                                       const int n_array[],
                                       const int k_array[],
                                       const double *alpha_array[],
                                       const double *const *Aarray[],
                                       const int lda_array[],
                                       const double *const *Barray[],
                                       const int ldb_array[],
                                       const double *beta_array[],
                                       double *const Carray[],
                                       const int ldc_array[],
                                       int group_count,
                                       const int group_size[])
```

*Double‑precision version of `cublasSgemmGroupedBatched`.*

---  

## cublasCgemmGroupedBatched  

```c
cublasStatus_t cublasCgemmGroupedBatched(cublasHandle_t handle,
                                       const cublasOperation_t transa_array[],
                                       const cublasOperation_t transb_array[],
                                       const int m_array[],
                                       const int n_array[],
                                       const int k_array[],
                                       const cuComplex *alpha_array[],
                                       const cuComplex *const *Aarray[],
                                       const int lda_array[],
                                       const cuComplex *const *Barray[],
                                       const int ldb_array[],
                                       const cuComplex *beta_array[],
                                       cuComplex *const Carray[],
                                       const int ldc_array[],
                                       int group_count,
                                       const int group_size[])
```

*Complex‑single precision grouped‑batched GEMM.*

---  

## cublasZgemmGroupedBatched  

```c
cublasStatus_t cublasZgemmGroupedBatched(cublasHandle_t handle,
                                       const cublasOperation_t transa_array[],
                                       const cublasOperation_t transb_array[],
                                       const int m_array[],
                                       const int n_array[],
                                       const int k_array[],
                                       const cuDoubleComplex *alpha_array[],
                                       const cuDoubleComplex *const *Aarray[],
                                       const int lda_array[],
                                       const cuDoubleComplex *const *Barray[],
                                       const int ldb_array[],
                                       const cuDoubleComplex *beta_array[],
                                       cuDoubleComplex *const Carray[],
                                       const int ldc_array[],
                                       int group_count,
                                       const int group_size[])
```

*Complex‑double precision grouped‑batched GEMM.*

---  

## cublasSsymm  

```c
cublasStatus_t cublasSsymm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           int m,
                           int n,
                           const float *alpha,
                           const float *A, int lda,
                           const float *B, int ldb,
                           const float *beta,
                           float *C, int ldc)
```

*Performs symmetric matrix‑matrix multiplication `C = α·A·B + β·C` (or `C = α·B·A + β·C`
depending on `side`).  `A` is symmetric.*

---  

## cublasDsymm  

```c
cublasStatus_t cublasDsymm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           int m,
                           int n,
                           const double *alpha,
                           const double *A, int lda,
                           const double *B, int ldb,
                           const double *beta,
                           double *C, int ldc)
```

*Double‑precision symmetric `cublasDsymm`.*

---  

## cublasCsymm  

```c
cublasStatus_t cublasCsymm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           int m,
                           int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *B, int ldb,
                           const cuComplex *beta,
                           cuComplex *C, int ldc)
```

*Complex‑single precision symmetric `cublasCsymm`.*

---  

## cublasZsymm  

```c
cublasStatus_t cublasZsymm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           int m,
                           int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *B, int ldb,
                           const cuDoubleComplex *beta,
                           cuDoubleComplex *C, int ldc)
```

*Complex‑double precision symmetric `cublasZsymm`.*

---  

## cublasSsyrk  

```c
cublasStatus_t cublasSsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const float *alpha,
                           const float *A, int lda,
                           const float *beta,
                           float *C, int ldc)
```

*Single‑precision symmetric rank‑k update `C = α·A·Aᵀ + β·C` (or with `trans`).*

---  

## cublasDsyrk  

```c
cublasStatus_t cublasDsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const double *alpha,
                           const double *A, int lda,
                           const double *beta,
                           double *C, int ldc)
```

*Double‑precision version of `cublasSsyrk`.*

---  

## cublasCsyrk  

```c
cublasStatus_t cublasCsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *beta,
                           cuComplex *C, int ldc)
```

*Complex‑single precision symmetric rank‑k update.*

---  

## cublasZsyrk  

```c
cublasStatus_t cublasZsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *beta,
                           cuDoubleComplex *C, int ldc)
```

*Complex‑double precision symmetric rank‑k update.*

---  

## cublasSsyr2k  

```c
cublasStatus_t cublasSsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const float *alpha,
                            const float *A, int lda,
                            const float *B, int ldb,
                            const float *beta,
                            float *C, int ldc)
```

*Single‑precision symmetric rank‑2k update `C = α·A·Bᵀ + β·Cᵀ + α·B·Aᵀ + β·C`.*

---  

## cublasDsyr2k  

```c
cublasStatus_t cublasDsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const double *alpha,
                            const double *A, int lda,
                            const double *B, int ldb,
                            const double *beta,
                            double *C, int ldc)
```

*Double‑precision version of `cublasSsyr2k`.*

---  

## cublasCsyr2k  

```c
cublasStatus_t cublasCsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuComplex *alpha,
                            const cuComplex *A, int lda,
                            const cuComplex *B, int ldb,
                            const cuComplex *beta,
                            cuComplex *C, int ldc)
```

*Complex‑single precision symmetric rank‑2k update.*

---  

## cublasZsyr2k  

```c
cublasStatus_t cublasZsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuDoubleComplex *alpha,
                            const cuDoubleComplex *A, int lda,
                            const cuDoubleComplex *B, int ldb,
                            const cuDoubleComplex *beta,
                            cuDoubleComplex *C, int ldc)
```

*Complex‑double precision symmetric rank‑2k update.*

---  

## cublasSsyrkx  

```c
cublasStatus_t cublasSsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const float *alpha,
                            const float *A, int lda,
                            const float *B, int ldb,
                            const float *beta,
                            float *C, int ldc)
```

*Single‑precision symmetric rank‑k update with an extra matrix `B` (variation of
`syrk`).*

---  

## cublasDsyrkx  

```c
cublasStatus_t cublasDsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const double *alpha,
                            const double *A, int lda,
                            const double *B, int ldb,
                            const double *beta,
                            double *C, int ldc)
```

*Double‑precision version of `cublasSsyrkx`.*

---  

## cublasCsyrkx  

```c
cublasStatus_t cublasCsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuComplex *alpha,
                            const cuComplex *A, int lda,
                            const cuComplex *B, int ldb,
                            const cuComplex *beta,
                            cuComplex *C, int ldc)
```

*Complex‑single precision version of `cublasSsyrkx`.*

---  

## cublasZsyrkx  

```c
cublasStatus_t cublasZsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuDoubleComplex *alpha,
                            const cuDoubleComplex *A, int lda,
                            const cuDoubleComplex *B, int ldb,
                            const cuDoubleComplex *beta,
                            cuDoubleComplex *C, int ldc)
```

*Complex‑double precision version of `cublasSsyrkx`.*

---  

## cublasStrmm  

```c
cublasStatus_t cublasStrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const float *alpha,
                           const float *A, int lda,
                           float *B, int ldb)
```

*Single‑precision triangular matrix‑matrix multiplication `B = α·op(A)·B`
(or `B = α·B·op(A)`).*

---  

## cublasDtrmm  

```c
cublasStatus_t cublasDtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const double *alpha,
                           const double *A, int lda,
                           double *B, int ldb)
```

*Double‑precision version of `cublasStrmm`.*

---  

## cublasCtrmm  

```c
cublasStatus_t cublasCtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           cuComplex *B, int ldb)
```

*Complex‑single precision triangular matrix‑matrix multiplication.*

---  

## cublasZtrmm  

```c
cublasStatus_t cublasZtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           cuDoubleComplex *B, int ldb)
```

*Complex‑double precision version of `cublasStrmm`.*

---  

## cublasHtrmm  

```c
cublasStatus_t cublasHtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const __half *alpha,
                           const __half *A, int lda,
                           __half *B, int ldb)
```

*Half‑precision triangular matrix‑matrix multiplication (requires FP16 tensor
cores).*

---  

## cublasStrsm  

```c
cublasStatus_t cublasStrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const float *alpha,
                           const float *A, int lda,
                           float *B, int ldb)
```

*Single‑precision triangular solve `op(A)·X = α·B` (or `X·op(A) = α·B`).*

---  

## cublasDtrsm  

```c
cublasStatus_t cublasDtrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const double *alpha,
                           const double *A, int lda,
                           double *B, int ldb)
```

*Double‑precision version of `cublasStrsm`.*

---  

## cublasCtrsm  

```c
cublasStatus_t cublasCtrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           cuComplex *B, int ldb)
```

*Complex‑single precision triangular solve.*

---  

## cublasZtrsm  

```c
cublasStatus_t cublasZtrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           cuDoubleComplex *B, int ldb)
```

*Complex‑double precision triangular solve.*

---  

## cublasHtrsm  

```c
cublasStatus_t cublasHtrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const __half *alpha,
                           const __half *A, int lda,
                           __half *B, int ldb)
```

*Half‑precision triangular solve (requires FP16 tensor cores).*

---  

## cublasSgemm3m  

```c
cublasStatus_t cublasSgemm3m(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const float *alpha,
                             const float *A, int lda,
                             const float *B, int ldb,
                             const float *beta,
                             float *C, int ldc)
```

*Single‑precision GEMM that uses the “Gauss complexity‑reduction” algorithm
(optimized for complex‑valued matrices).*

---  

## cublasDgemm3m  

```c
cublasStatus_t cublasDgemm3m(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const double *alpha,
                             const double *A, int lda,
                             const double *B, int ldb,
                             const double *beta,
                             double *C, int ldc)
```

*Double‑precision version of `cublasSgemm3m`.*

---  

## cublasCgemm3m  

```c
cublasStatus_t cublasCgemm3m(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const cuComplex *alpha,
                             const cuComplex *A, int lda,
                             const cuComplex *B, int ldb,
                             const cuComplex *beta,
                             cuComplex *C, int ldc)
```

*Complex‑single precision version of `cublasSgemm3m`.*

---  

## cublasZgemm3m  

```c
cublasStatus_t cublasZgemm3m(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const cuDoubleComplex *alpha,
                             const cuDoubleComplex *A, int lda,
                             const cuDoubleComplex *B, int ldb,
                             const cuDoubleComplex *beta,
                             cuDoubleComplex *C, int ldc)
```

*Complex‑double precision version of `cublasSgemm3m`.*

---  

## cublasSgemmEx  

```c
cublasStatus_t cublasSgemmEx(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const float *alpha,
                             const void *A, cudaDataType_t Atype,
                             int lda,
                             const void *B, cudaDataType_t Btype,
                             int ldb,
                             const float *beta,
                             void *C, cudaDataType_t Ctype,
                             int ldc)
```

*Generalized GEMM that allows each matrix to have an independent data type
(e.g., `CUDA_R_16F`, `CUDA_R_32F`, `CUDA_R_8I`, …).*

---  

## cublasCgemmEx  

```c
cublasStatus_t cublasCgemmEx(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const cuComplex *alpha,
                             const void *A, cudaDataType_t Atype,
                             int lda,
                             const void *B, cudaDataType_t Btype,
                             int ldb,
                             const cuComplex *beta,
                             void *C, cudaDataType_t Ctype,
                             int ldc)
```

*Complex‑single precision version of `cublasSgemmEx`.*

---  

## cublasZgemmEx  

```c
cublasStatus_t cublasZgemmEx(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const cuDoubleComplex *alpha,
                             const void *A, cudaDataType_t Atype,
                             int lda,
                             const void *B, cudaDataType_t Btype,
                             int ldb,
                             const cuDoubleComplex *beta,
                             void *C, cudaDataType_t Ctype,
                             int ldc)
```

*Complex‑double precision version of `cublasSgemmEx`.*

---  

## cublasHgemmEx  

```c
cublasStatus_t cublasHgemmEx(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const __half *alpha,
                             const void *A, cudaDataType_t Atype,
                             int lda,
                             const void *B, cudaDataType_t Btype,
                             int ldb,
                             const __half *beta,
                             void *C, cudaDataType_t Ctype,
                             int ldc)
```

*Half‑precision version of `cublasSgemmEx`.*

---  

## cublasGemmEx  

```c
cublasStatus_t cublasGemmEx(cublasHandle_t handle,
                            cublasOperation_t transa,
                            cublasOperation_t transb,
                            int m,
                            int n,
                            int k,
                            const void *alpha,
                            const void *A, cudaDataType_t Atype,
                            int lda,
                            const void *B, cudaDataType_t Btype,
                            int ldb,
                            const void *beta,
                            void *C, cudaDataType_t Ctype,
                            int ldc,
                            cublasComputeType_t computeType,
                            cublasGemmAlgo_t algo)
```

*Full‑featured GEMM that lets the caller specify the compute type, scale type,
algorithmic heuristic, and individual data types for `A`, `B`, and `C`.*

---  

## cublasGemmBatchedEx  

```c
cublasStatus_t cublasGemmBatchedEx(cublasHandle_t handle,
                                 cublasOperation_t transa,
                                 cublasOperation_t transb,
                                 int m,
                                 int n,
                                 int k,
                                 const void *alpha,
                                 const void *const *Aarray,
                                 cudaDataType_t Atype,
                                 int lda,
                                 const void *const *Barray,
                                 cudaDataType_t Btype,
                                 int ldb,
                                 const void *beta,
                                 void *const *Carray,
                                 cudaDataType_t Ctype,
                                 int ldc,
                                 int batchCount,
                                 cublasComputeType_t computeType,
                                 cublasGemmAlgo_t algo)
```

*Batched variant of `cublasGemmEx`; each matrix in the batch may have its own
pointer and leading dimension.*

---  

## cublasGemmStridedBatchedEx  

```c
cublasStatus_t cublasGemmStridedBatchedEx(cublasHandle_t handle,
                                        cublasOperation_t transa,
                                        cublasOperation_t transb,
                                        int m,
                                        int n,
                                        int k,
                                        const void *alpha,
                                        const void *A, cudaDataType_t Atype,
                                        int lda,
                                        long long int strideA,
                                        const void *B, cudaDataType_t Btype,
                                        int ldb,
                                        long long int strideB,
                                        const void *beta,
                                        void *C, cudaDataType_t Ctype,
                                        int ldc,
                                        long long int strideC,
                                        int batchCount,
                                        cublasComputeType_t computeType,
                                        cublasGemmAlgo_t algo)
```

*Strided‑batched version of `cublasGemmEx`; matrices are placed at fixed
offsets (`strideA`, `strideB`, `strideC`) between successive batches.*

---  

## cublasGemmGroupedBatchedEx  

```c
cublasStatus_t cublasGemmGroupedBatchedEx(cublasHandle_t handle,
                                        const cublasOperation_t transa_array[],
                                        const cublasOperation_t transb_array[],
                                        const int m_array[],
                                        const int n_array[],
                                        const int k_array[],
                                        const void *alpha_array[],
                                        const void *const *Aarray[],
                                        cudaDataType_t Atype,
                                        const int lda_array[],
                                        const void *const *Barray[],
                                        cudaDataType_t Btype,
                                        const int ldb_array[],
                                        const void *beta_array[],
                                        void *const *Carray,
                                        cudaDataType_t Ctype,
                                        const int ldc_array[],
                                        int group_count,
                                        const int group_size[],
                                        cublasComputeType_t computeType)
```

*Grouped‑batched GEMM where each group may have different dimensions,
leading dimensions, or scaling factors.*

---  

## cublasSsyrk  

```c
cublasStatus_t cublasSsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const float *alpha,
                           const float *A, int lda,
                           const float *beta,
                           float *C, int ldc)
```

*Single‑precision symmetric rank‑k update `C = α·A·Aᵀ + β·C` (or with `trans`).*

---  

## cublasDsyrk  

```c
cublasStatus_t cublasDsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const double *alpha,
                           const double *A, int lda,
                           const double *beta,
                           double *C, int ldc)
```

*Double‑precision version of `cublasSsyrk`.*

---  

## cublasCsyrk  

```c
cublasStatus_t cublasCsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *beta,
                           cuComplex *C, int ldc)
```

*Complex‑single precision symmetric rank‑k update.*

---  

## cublasZsyrk  

```c
cublasStatus_t cublasZsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *beta,
                           cuDoubleComplex *C, int ldc)
```

*Complex‑double precision symmetric rank‑k update.*

---  

## cublasSsyr2k  

```c
cublasStatus_t cublasSsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const float *alpha,
                            const float *A, int lda,
                            const float *B, int ldb,
                            const float *beta,
                            float *C, int ldc)
```

*Single‑precision symmetric rank‑2k update `C = α·A·Bᵀ + β·Cᵀ + α·B·Aᵀ + β·C`.*

---  

## cublasDsyr2k  

```c
cublasStatus_t cublasDsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const double *alpha,
                            const double *A, int lda,
                            const double *B, int ldb,
                            const double *beta,
                            double *C, int ldc)
```

*Double‑precision version of `cublasSsyr2k`.*

---  

## cublasCsyr2k  

```c
cublasStatus_t cublasCsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuComplex *alpha,
                            const cuComplex *A, int lda,
                            const cuComplex *B, int ldb,
                            const cuComplex *beta,
                            cuComplex *C, int ldc)
```

*Complex‑single precision symmetric rank‑2k update.*

---  

## cublasZsyr2k  

```c
cublasStatus_t cublasZsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuDoubleComplex *alpha,
                            const cuDoubleComplex *A, int lda,
                            const cuDoubleComplex *B, int ldb,
                            const cuDoubleComplex *beta,
                            cuDoubleComplex *C, int ldc)
```

*Complex‑double precision symmetric rank‑2k update.*

---  

## cublasSsyrkx  

```c
cublasStatus_t cublasSsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const float *alpha,
                            const float *A, int lda,
                            const float *B, int ldb,
                            const float *beta,
                            float *C, int ldc)
```

*Single‑precision symmetric rank‑k update with an extra matrix `B` (variation of
`syrk`).*

---  

## cublasDsyrkx  

```c
cublasStatus_t cublasDsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const double *alpha,
                            const double *A, int lda,
                            const double *B, int ldb,
                            const double *beta,
                            double *C, int ldc)
```

*Double‑precision version of `cublasSsyrkx`.*

---  

## cublasCsyrkx  

```c
cublasStatus_t cublasCsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuComplex *alpha,
                            const cuComplex *A, int lda,
                            const cuComplex *B, int ldb,
                            const cuComplex *beta,
                            cuComplex *C, int ldc)
```

*Complex‑single precision version of `cublasSsyrkx`.*

---  

## cublasZsyrkx  

```c
cublasStatus_t cublasZsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuDoubleComplex *alpha,
                            const cuDoubleComplex *A, int lda,
                            const cuDoubleComplex *B, int ldb,
                            const cuDoubleComplex *beta,
                            cuDoubleComplex *C, int ldc)
```

*Complex‑double precision version of `cublasSsyrkx`.*

---  

## cublasSgemm3m  

```c
cublasStatus_t cublasSgemm3m(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const float *alpha,
                             const float *A, int lda,
                             const float *B, int ldb,
                             const float *beta,
                             float *C, int ldc)
```

*Single‑precision GEMM that uses the Gauss complexity‑reduction algorithm
(optimized for complex matrices).*

---  

## cublasDgemm3m  

```c
cublasStatus_t cublasDgemm3m(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const double *alpha,
                             const double *A, int lda,
                             const double *B, int ldb,
                             const double *beta,
                             double *C, int ldc)
```

*Double‑precision version of `cublasSgemm3m`.*

---  

## cublasCgemm3m  

```c
cublasStatus_t cublasCgemm3m(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const cuComplex *alpha,
                             const cuComplex *A, int lda,
                             const cuComplex *B, int ldb,
                             const cuComplex *beta,
                             cuComplex *C, int ldc)
```

*Complex‑single precision version of `cublasSgemm3m`.*

---  

## cublasZgemm3m  

```c
cublasStatus_t cublasZgemm3m(cublasHandle_t handle,
                             cublasOperation_t transa,
                             cublasOperation_t transb,
                             int m,
                             int n,
                             int k,
                             const cuDoubleComplex *alpha,
                             const cuDoubleComplex *A, int lda,
                             const cuDoubleComplex *B, int ldb,
                             const cuDoubleComplex *beta,
                             cuDoubleComplex *C, int ldc)
```

*Complex‑double precision version of `cublasSgemm3m`.*

---  

## cublasStrmm  

```c
cublasStatus_t cublasStrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const float *alpha,
                           const float *A, int lda,
                           float *B, int ldb)
```

*Single‑precision triangular matrix‑matrix multiplication `B = α·op(A)·B`
(or `B = α·B·op(A)`).*

---  

## cublasDtrmm  

```c
cublasStatus_t cublasDtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const double *alpha,
                           const double *A, int lda,
                           double *B, int ldb)
```

*Double‑precision version of `cublasStrmm`.*

---  

## cublasCtrmm  

```c
cublasStatus_t cublasCtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           cuComplex *B, int ldb)
```

*Complex‑single precision triangular matrix‑matrix multiplication.*

---  

## cublasZtrmm  

```c
cublasStatus_t cublasZtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           cuDoubleComplex *B, int ldb)
```

*Complex‑double precision version of `cublasStrmm`.*

---  

## cublasHtrmm  

```c
cublasStatus_t cublasHtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const __half *alpha,
                           const __half *A, int lda,
                           __half *B, int ldb)
```

*Half‑precision triangular matrix‑matrix multiplication (requires FP16 tensor
cores).*

---  

## cublasStrsm  

```c
cublasStatus_t cublasStrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const float *alpha,
                           const float *A, int lda,
                           float *B, int ldb)
```

*Single‑precision triangular solve `op(A)·X = α·B` (or `X·op(A) = α·B`).*

---  

## cublasDtrsm  

```c
cublasStatus_t cublasDtrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const double *alpha,
                           const double *A, int lda,
                           double *B, int ldb)
```

*Double‑precision version of `cublasStrsm`.*

---  

## cublasCtrsm  

```c
cublasStatus_t cublasCtrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           cuComplex *B, int ldb)
```

*Complex‑single precision triangular solve.*

---  

## cublasZtrsm  

```c
cublasStatus_t cublasZtrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           cuDoubleComplex *B, int ldb)
```

*Complex‑double precision version of `cublasStrsm`.*

---  

## cublasHtrsm  

```c
cublasStatus_t cublasHtrsm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m,
                           int n,
                           const __half *alpha,
                           const __half *A, int lda,
                           __half *B, int ldb)
```

*Half‑precision triangular solve (requires FP16 tensor cores).*

---  

## cublasSsyrk  

```c
cublasStatus_t cublasSsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const float *alpha,
                           const float *A, int lda,
                           const float *beta,
                           float *C, int ldc)
```

*Single‑precision symmetric rank‑k update `C = α·A·Aᵀ + β·C` (or with `trans`).*

---  

## cublasDsyrk  

```c
cublasStatus_t cublasDsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const double *alpha,
                           const double *A, int lda,
                           const double *beta,
                           double *C, int ldc)
```

*Double‑precision version of `cublasSsyrk`.*

---  

## cublasCsyrk  

```c
cublasStatus_t cublasCsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *beta,
                           cuComplex *C, int ldc)
```

*Complex‑single precision symmetric rank‑k update.*

---  

## cublasZsyrk  

```c
cublasStatus_t cublasZsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           int k,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *beta,
                           cuDoubleComplex *C, int ldc)
```

*Complex‑double precision symmetric rank‑k update.*

---  

## cublasSsyr2k  

```c
cublasStatus_t cublasSsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const float *alpha,
                            const float *A, int lda,
                            const float *B, int ldb,
                            const float *beta,
                            float *C, int ldc)
```

*Single‑precision symmetric rank‑2k update.*

---  

## cublasDsyr2k  

```c
cublasStatus_t cublasDsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const double *alpha,
                            const double *A, int lda,
                            const double *B, int ldb,
                            const double *beta,
                            double *C, int ldc)
```

*Double‑precision version of `cublasSsyr2k`.*

---  

## cublasCsyr2k  

```c
cublasStatus_t cublasCsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuComplex *alpha,
                            const cuComplex *A, int lda,
                            const cuComplex *B, int ldb,
                            const cuComplex *beta,
                            cuComplex *C, int ldc)
```

*Complex‑single precision symmetric rank‑2k update.*

---  

## cublasZsyr2k  

```c
cublasStatus_t cublasZsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuDoubleComplex *alpha,
                            const cuDoubleComplex *A, int lda,
                            const cuDoubleComplex *B, int ldb,
                            const cuDoubleComplex *beta,
                            cuDoubleComplex *C, int ldc)
```

*Complex‑double precision symmetric rank‑2k update.*

---  

## cublasSsyrkx  

```c
cublasStatus_t cublasSsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const float *alpha,
                            const float *A, int lda,
                            const float *B, int ldb,
                            const float *beta,
                            float *C, int ldc)
```

*Single‑precision symmetric rank‑k update with an extra matrix `B` (variation of
`syrk`).*

---  

## cublasDsyrkx  

```c
cublasStatus_t cublasDsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const double *alpha,
                            const double *A, int lda,
                            const double *B, int ldb,
                            const double *beta,
                            double *C, int ldc)
```

*Double‑precision version of `cublasSsyrkx`.*

---  

## cublasCsyrkx  

```c
cublasStatus_t cublasCsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuComplex *alpha,
                            const cuComplex *A, int lda,
                            const cuComplex *B, int ldb,
                            const cuComplex *beta,
                            cuComplex *C, int ldc)
```

*Complex‑single precision version of `cublasSsyrkx`.*

---  

## cublasZsyrkx  

```c
cublasStatus_t cublasZsyrkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            int k,
                            const cuDoubleComplex *alpha,
                            const cuDoubleComplex *A, int lda,
                            const cuDoubleComplex *B, int ldb,
                            const cuDoubleComplex *beta,
                            cuDoubleComplex *C, int ldc)
```

*Complex‑double precision version of `cublasSsyrkx`.*

---  

## cublasSsyr2k  

*(already listed under “cublasSsyr2k” above – duplicate entry removed)*

---  

## cublasSsyrk  

*(already listed under “cublasSsyrk” above – duplicate entry removed)*