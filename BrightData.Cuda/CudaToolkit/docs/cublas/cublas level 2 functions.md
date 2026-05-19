## cublasSgbmv

```
cublasStatus_t cublasSgbmv(cublasHandle_t handle,
                           cublasOperation_t trans,
                           int m, int n,
                           int kl, int ku,
                           const float *alpha,
                           const float *A, int lda,
                           const float *x, int incx,
                           const float *beta,
                           float *y, int incy);
```

Performs the banded matrix‑vector operation  

\[
y = \alpha \; \text{op}(A)\,x + \beta\,y
\]

where `op(A)` is `A`, `Aᵀ` or `Aᴴ` according to `trans`.  
`A` is stored in banded format with `kl` sub‑diagonals and `ku` super‑diagonals.

**Parameter table**

| Parameter | Memory | In/Out | Meaning |
|---|---|---|---|
| `handle` | device | in | cuBLAS handle |
| `trans` | device | in | Matrix operation (`CUBLAS_OP_N`, `CUBLAS_OP_T`, `CUBLAS_OP_C`) |
| `m` | host | in | number of rows of `op(A)` and `y` |
| `n` | host | in | number of columns of `x` and `y` |
| `kl` | host | in | number of sub‑diagonals of `A` |
| `ku` | host | in | number of super‑diagonals of `A` |
| `alpha` | host or device | in | scalar multiplier of `op(A)x` |
| `A` | device | in | banded matrix `A` (column‑major) |
| `lda` | host | in | leading dimension of `A` (≥ `kl+ku+1`) |
| `x` | device | in | input vector |
| `incx` | host | in | stride between consecutive elements of `x` |
| `beta` | host or device | in | scalar multiplier of `y` |
| `y` | device | in/out | output vector |
| `incy` | host | in | stride between consecutive elements of `y` |

**Return values**

| Error value | Meaning |
|---|---|
| `CUBLAS_STATUS_SUCCESS` | success |
| `CUBLAS_STATUS_NOT_INITIALIZED` | cuBLAS not initialized |
| `CUBLAS_STATUS_INVALID_VALUE` | illegal argument (e.g., negative dimensions) |
| `CUBLAS_STATUS_EXECUTION_FAILED` | kernel launch failed |

---  

## cublasDgbmv

```
cublasStatus_t cublasDgbmv(cublasHandle_t handle,
                           cublasOperation_t trans,
                           int m, int n,
                           int kl, int ku,
                           const double *alpha,
                           const double *A, int lda,
                           const double *x, int incx,
                           const double *beta,
                           double *y, int incy);
```

Same operation as `cublasSgbmv` but with double‑precision data.

---  

## cublasCgbmv

```
cublasStatus_t cublasCgbmv(cublasHandle_t handle,
                           cublasOperation_t trans,
                           int m, int n,
                           int kl, int ku,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *x, int incx,
                           const cuComplex *beta,
                           cuComplex *y, int incy);
```

Complex‑single precision version of `cublasSgbmv`.

---  

## cublasZgbmv

```
cublasStatus_t cublasZgbmv(cublasHandle_t handle,
                           cublasOperation_t trans,
                           int m, int n,
                           int kl, int ku,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *x, int incx,
                           const cuDoubleComplex *beta,
                           cuDoubleComplex *y, int incy);
```

Complex‑double precision version of `cublasSgbmv`.

---  

## cublasSgemv

```
cublasStatus_t cublasSgemv(cublasHandle_t handle,
                           cublasOperation_t trans,
                           int m, int n,
                           const float *alpha,
                           const float *A, int lda,
                           const float *x, int incx,
                           const float *beta,
                           float *y, int incy);
```

Performs the matrix‑vector operation  

\[
y = \alpha \; \text{op}(A)\,x + \beta\,y
\]

where `A` is an `m×n` matrix stored in column‑major order.

---  

## cublasDgemv

```
cublasStatus_t cublasDgemv(cublasHandle_t handle,
                           cublasOperation_t trans,
                           int m, int n,
                           const double *alpha,
                           const double *A, int lda,
                           const double *x, int incx,
                           const double *beta,
                           double *y, int incy);
```

Double‑precision version of `cublasSgemv`.

---  

## cublasCgemv

```
cublasStatus_t cublasCgemv(cublasHandle_t handle,
                           cublasOperation_t trans,
                           int m, int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *x, int incx,
                           const cuComplex *beta,
                           cuComplex *y, int incy);
```

Complex‑single precision version of `cublasSgemv`.

---  

## cublasZgemv

```
cublasStatus_t cublasZgemv(cublasHandle_t handle,
                           cublasOperation_t trans,
                           int m, int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *x, int incx,
                           const cuDoubleComplex *beta,
                           cuDoubleComplex *y, int incy);
```

Complex‑double precision version of `cublasSgemv`.

---  

## cublasSger

```
cublasStatus_t cublasSger(cublasHandle_t handle,
                          int m, int n,
                          const float *alpha,
                          const float *x, int incx,
                          const float *y, int incy,
                          float *A, int lda);
```

Performs the rank‑1 update  

\[
A \leftarrow \alpha \; x\,y^{\!T} + A
\]

where `A` is `m×n` column‑major, `x` is `m`‑vector, `y` is `n`‑vector.

---  

## cublasDger

```
cublasStatus_t cublasDger(cublasHandle_t handle,
                          int m, int n,
                          const double *alpha,
                          const double *x, int incx,
                          const double *y, int incy,
                          double *A, int lda);
```

Double‑precision version of `cublasSger`.

---  

## cublasCgeru

```
cublasStatus_t cublasCgeru(cublasHandle_t handle,
                           int m, int n,
                           const cuComplex *alpha,
                           const cuComplex *x, int incx,
                           const cuComplex *y, int incy,
                           cuComplex *A, int lda);
```

Complex‑single precision **unconjugated** rank‑1 update.

---  

## cublasCgerc

```
cublasStatus_t cublasCgerc(cublasHandle_t handle,
                           int m, int n,
                           const cuComplex *alpha,
                           const cuComplex *x, int incx,
                           const cuComplex *y, int incy,
                           cuComplex *A, int lda);
```

Complex‑single precision **conjugated** rank‑1 update.

---  

## cublasZgeru

```
cublasStatus_t cublasZgeru(cublasHandle_t handle,
                           int m, int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *x, int incx,
                           const cuDoubleComplex *y, int incy,
                           cuDoubleComplex *A, int lda);
```

Complex‑double precision **unconjugated** rank‑1 update.

---  

## cublasZgerc

```
cublasStatus_t cublasZgerc(cublasHandle_t handle,
                           int m, int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *x, int incx,
                           const cuDoubleComplex *y, int incy,
                           cuDoubleComplex *A, int lda);
```

Complex‑double precision **conjugated** rank‑1 update.

---  

## cublasSsbmv

```
cublasStatus_t cublasSsbmv(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           int n,
                           int k,
                           const float *alpha,
                           const float *AP, int aparam,
                           const float *x, int incx,
                           const float *beta,
                           float *y, int incy);
```

Performs symmetric banded matrix‑vector multiplication  

\[
y = \alpha \; A\,x + \beta\,y
\]

where `A` is `n×n` symmetric and stored in banded format (`k` sub‑/super‑diagonals).

---  

## cublasDsbmv

```
cublasStatus_t cublasDsbmv(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           int n,
                           int k,
                           const double *alpha,
                           const double *AP, int aparam,
                           const double *x, int incx,
                           const double *beta,
                           double *y, int incy);
```

Double‑precision version of `cublasSsbmv`.

---  

## cublasSspmv

```
cublasStatus_t cublasSspmv(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           int n,
                           const float *AP,
                           const float *x, int incx,
                           const float *beta,
                           float *y, int incy);
```

Symmetric packed matrix‑vector multiplication (same operation as `cublasSsbmv`
but `A` is stored in packed format).

---  

## cublasDspmv

```
cublasStatus_t cublasDspmv(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           int n,
                           const double *AP,
                           const double *x, int incx,
                           const double *beta,
                           double *y, int incy);
```

Double‑precision version of `cublasSspmv`.

---  

## cublasSspr

```
cublasStatus_t cublasSspr(cublasHandle_t handle,
                          cublasFillMode_t uplo,
                          int n,
                          const float *alpha,
                          const float *x, int incx,
                          float *AP);
```

Performs symmetric rank‑1 update  

\[
A \leftarrow \alpha \; x\,x^{\!T} + A
\]

where `A` is stored in packed format.

---  

## cublasDspr

```
cublasStatus_t cublasDspr(cublasHandle_t handle,
                          cublasFillMode_t uplo,
                          int n,
                          const double *alpha,
                          const double *x, int incx,
                          double *AP);
```

Double‑precision version of `cublasSspr`.

---  

## cublasSspr2

```
cublasStatus_t cublasSspr2(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           int n,
                           const float *alpha,
                           const float *x, int incx,
                           const float *y, int incy,
                           float *AP);
```

Symmetric rank‑2 update  

\[
A \leftarrow \alpha\;(x\,y^{\!T}+y\,x^{\!T}) + A
\]

---  

## cublasDspr2

```
cublasStatus_t cublasDspr2(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           int n,
                           const double *alpha,
                           const double *x, int incx,
                           const double *y, int incy,
                           double *AP);
```

Double‑precision version of `cublasSspr2`.

---  

## cublasSymv

```
cublasStatus_t cublasSymv(cublasHandle_t handle,
                          cublasFillMode_t uplo,
                          int n,
                          const float *alpha,
                          const float *A, int lda,
                          const float *x, int incx,
                          const float *beta,
                          float *y, int incy);
```

Performs symmetric matrix‑vector multiplication  

\[
y = \alpha \; A\,x + \beta\,y
\]

(`A` is `n×n`, stored in column‑major; `uplo` selects lower/upper part).

---  

## cublasDsymv

```
cublasStatus_t cublasDsymv(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           int n,
                           const double *alpha,
                           const double *A, int lda,
                           const double *x, int incx,
                           const double *beta,
                           double *y, int incy);
```

Double‑precision version of `cublasSymv`.

---  

## cublasCsymv

```
cublasStatus_t cublasCsymv(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *x, int incx,
                           const cuComplex *beta,
                           cuComplex *y, int incy);
```

Complex‑single precision version of `cublasSymv`.

---  

## cublasZsymv

```
cublasStatus_t cublasZsymv(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *x, int incx,
                           const cuDoubleComplex *beta,
                           cuDoubleComplex *y, int incy);
```

Complex‑double precision version of `cublasSymv`.

---  

## cublasSyr

```
cublasStatus_t cublasSyr(cublasHandle_t handle,
                         cublasFillMode_t uplo,
                         cublasOperation_t trans,
                         int n,
                         const float *alpha,
                         const float *A, int lda,
                         const float *x, int incx,
                         float *beta,
                         float *C, int ldc);
```

Performs symmetric rank‑1 update  

\[
C = \alpha \; A\,A^{\!T} + \beta \; C
\]

where `A` is `n×k` and `C` is `n×n`.

---  

## cublasDsyr

```
cublasStatus_t cublasDsyr(cublasHandle_t handle,
                          cublasFillMode_t uplo,
                          cublasOperation_t trans,
                          int n,
                          const double *alpha,
                          const double *A, int lda,
                          const double *x, int incx,
                          double *beta,
                          double *C, int ldc);
```

Double‑precision version of `cublasSyr`.

---  

## cublasCsyr

```
cublasStatus_t cublasCsyr(cublasHandle_t handle,
                          cublasFillMode_t uplo,
                          cublasOperation_t trans,
                          int n,
                          const cuComplex *alpha,
                          const cuComplex *A, int lda,
                          const cuComplex *x, int incx,
                          cuComplex *beta,
                          cuComplex *C, int ldc);
```

Complex‑single precision version of `cublasSyr`.

---  

## cublasZsyr

```
cublasStatus_t cublasZsyr(cublasHandle_t handle,
                          cublasFillMode_t uplo,
                          cublasOperation_t trans,
                          int n,
                          const cuDoubleComplex *alpha,
                          const cuDoubleComplex *A, int lda,
                          const cuDoubleComplex *x, int incx,
                          cuDoubleComplex *beta,
                          cuDoubleComplex *C, int ldc);
```

Complex‑double precision version of `cublasSyr`.

---  

## cublasSyr2

```
cublasStatus_t cublasSyr2(cublasHandle_t handle,
                          cublasFillMode_t uplo,
                          cublasOperation_t trans,
                          int n,
                          const float *alpha,
                          const float *A, int lda,
                          const float *B, int ldb,
                          float *beta,
                          float *C, int ldc);
```

Performs symmetric rank‑2 update  

\[
C = \alpha\;(A\,B^{\!T}+B\,A^{\!T}) + \beta\;C
\]

---  

## cublasDsyr2

```
cublasStatus_t cublasDsyr2(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           const double *alpha,
                           const double *A, int lda,
                           const double *B, int ldb,
                           double *beta,
                           double *C, int ldc);
```

Double‑precision version of `cublasSyr2`.

---  

## cublasCsyr2

```
cublasStatus_t cublasCsyr2(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *B, int ldb,
                           cuComplex *beta,
                           cuComplex *C, int ldc);
```

Complex‑single precision version of `cublasSyr2`.

---  

## cublasZsyr2

```
cublasStatus_t cublasZsyr2(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *B, int ldb,
                           cuDoubleComplex *beta,
                           cuDoubleComplex *C, int ldc);
```

Complex‑double precision version of `cublasSyr2`.

---  

## cublasSyrk

```
cublasStatus_t cublasSyrk(cublasHandle_t handle,
                          cublasFillMode_t uplo,
                          cublasOperation_t trans,
                          int n, int k,
                          const float *alpha,
                          const float *A, int lda,
                          const float *beta,
                          float *C, int ldc);
```

Performs symmetric rank‑k update  

\[
C = \alpha\;A\,A^{\!T} + \beta\;C
\]

where `A` is `n×k`.

---  

## cublasDsyrk

```
cublasStatus_t cublasDsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n, int k,
                           const double *alpha,
                           const double *A, int lda,
                           const double *beta,
                           double *C, int ldc);
```

Double‑precision version of `cublasSyrk`.

---  

## cublasCsyrk

```
cublasStatus_t cublasCsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n, int k,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *beta,
                           cuComplex *C, int ldc);
```

Complex‑single precision version of `cublasSyrk`.

---  

## cublasZsyrk

```
cublasStatus_t cublasZsyrk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n, int k,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *beta,
                           cuDoubleComplex *C, int ldc);
```

Complex‑double precision version of `cublasSyrk`.

---  

## cublasSyr2k

```
cublasStatus_t cublasSyr2k(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n, int k,
                           const float *alpha,
                           const float *A, int lda,
                           const float *B, int ldb,
                           const float *beta,
                           float *C, int ldc);
```

Performs symmetric rank‑2k update  

\[
C = \alpha\;(A\,B^{\!T}+B\,A^{\!T}) + \beta\;C
\]

---  

## cublasDsyr2k

```
cublasStatus_t cublasDsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n, int k,
                            const double *alpha,
                            const double *A, int lda,
                            const double *B, int ldb,
                            const double *beta,
                            double *C, int ldc);
```

Double‑precision version of `cublasSyr2k`.

---  

## cublasCsyr2k

```
cublasStatus_t cublasCsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n, int k,
                            const cuComplex *alpha,
                            const cuComplex *A, int lda,
                            const cuComplex *B, int ldb,
                            const cuComplex *beta,
                            cuComplex *C, int ldc);
```

Complex‑single precision version of `cublasSyr2k`.

---  

## cublasZsyr2k

```
cublasStatus_t cublasZsyr2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n, int k,
                            const cuDoubleComplex *alpha,
                            const cuDoubleComplex *A, int lda,
                            const cuDoubleComplex *B, int ldb,
                            const cuDoubleComplex *beta,
                            cuDoubleComplex *C, int ldc);
```

Complex‑double precision version of `cublasSyr2k`.

---  

## cublasStrmm

```
cublasStatus_t cublasStrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m, int n,
                           const float *alpha,
                           const float *A, int lda,
                           float *B, int ldb);
```

Performs triangular matrix‑matrix multiplication  

\[
B = \alpha \; \text{op}(A)\;B
\]

where `A` is triangular, `B` is `m×n`, `op(A)` is `A`, `Aᵀ` or `Aᴴ` according to `trans`.

---  

## cublasDtrmm

```
cublasStatus_t cublasDtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m, int n,
                           const double *alpha,
                           const double *A, int lda,
                           double *B, int ldb);
```

Double‑precision version of `cublasStrmm`.

---  

## cublasCtrmm

```
cublasStatus_t cublasCtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m, int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           cuComplex *B, int ldb);
```

Complex‑single precision version of `cublasStrmm`.

---  

## cublasZtrmm

```
cublasStatus_t cublasZtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m, int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           cuComplex *B, int ldb);
```

Complex‑double precision version of `cublasStrmm`.

---  

## cublasStrsv

```
cublasStatus_t cublasStrsv(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int n,
                           const float *A, int lda,
                           float *B, int ldb);
```

Solves a triangular linear system with multiple right‑hand sides  

\[
\text{op}(A)\,B = C
\]

---  

## cublasDtrsv

```
cublasStatus_t cublasDtrsv(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int n,
                           const double *A, int lda,
                           double *B, int ldb);
```

Double‑precision version of `cublasStrsv`.

---  

## cublasCtrsv

```
cublasStatus_t cublasCtrsv(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int n,
                           const cuComplex *A, int lda,
                           cuComplex *B, int ldb);
```

Complex‑single precision version of `cublasStrsv`.

---  

## cublasZtrsv

```
cublasStatus_t cublasZtrsv(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int n,
                           const cuDoubleComplex *A, int lda,
                           cuDoubleComplex *B, int ldb);
```

Complex‑double precision version of `cublasStrsv`.

---  

## cublasChemm

```
cublasStatus_t cublasChemm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           int m, int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *B, int ldb,
                           const cuComplex *beta,
                           cuComplex *C, int ldc);
```

Performs Hermitian matrix‑matrix multiplication  

\[
C = \alpha \; A\,B + \beta \; C
\]

where `A` is Hermitian.

---  

## cublasZhemm

```
cublasStatus_t cublasZhemm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           int m, int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           const cuDoubleComplex *B, int ldb,
                           const cuDoubleComplex *beta,
                           cuComplex *C, int ldc);
```

Double‑precision Hermitian matrix‑matrix multiplication version.

---  

## cublasCherk

```
cublasStatus_t cublasCherk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           const float *alpha,
                           const cuComplex *A, int lda,
                           const float *beta,
                           cuComplex *C, int ldc);
```

Complex‑single precision Hermitian rank‑k update.

---  

## cublasZherk

```
cublasStatus_t cublasZherk(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           const double *alpha,
                           const cuDoubleComplex *A, int lda,
                           const double *beta,
                           cuComplex *C, int ldc);
```

Complex‑double precision Hermitian rank‑k update.

---  

## cublasCher2k

```
cublasStatus_t cublasCher2k(cublasHandle_t handle,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           const cuComplex *B, int ldb,
                           const float *beta,
                           cuComplex *C, int ldc);
```

Complex‑single precision Hermitian rank‑2k update.

---  

## cublasZher2k

```
cublasStatus_t cublasZher2k(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            const cuDoubleComplex *alpha,
                            const cuDoubleComplex *A, int lda,
                            const cuDoubleComplex *B, int ldb,
                            const double *beta,
                            cuComplex *C, int ldc);
```

Complex‑double precision Hermitian rank‑2k update.

---  

## cublasCherkx

```
cublasStatus_t cublasCherkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            const cuComplex *alpha,
                            const cuComplex *A, int lda,
                            const cuComplex *B, int ldb,
                            const float *beta,
                            cuComplex *C, int ldc);
```

Complex‑single precision variant with extra scaling options (see documentation).

---  

## cublasZherkx

```
cublasStatus_t cublasZherkx(cublasHandle_t handle,
                            cublasFillMode_t uplo,
                            cublasOperation_t trans,
                            int n,
                            const cuDoubleComplex *alpha,
                            const cuDoubleComplex *A, int lda,
                            const cuDoubleComplex *B, int ldb,
                            const double *beta,
                            cuComplex *C, int ldc);
```

Complex‑double precision variant of `cublasCherkx`.

---  

## cublasTrmm

```
cublasStatus_t cublasTrmm(cublasHandle_t handle,
                          cublasSideMode_t side,
                          cublasFillMode_t uplo,
                          cublasOperation_t trans,
                          cublasDiagType_t diag,
                          int m, int n,
                          const float *alpha,
                          const float *A, int lda,
                          float *B, int ldb);
```

Triangular matrix‑matrix multiplication (same as `cublasStrmm` but the
operation is `B = alpha * op(A) * B`).

---  

## cublasDtrmm

```
cublasStatus_t cublasDtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m, int n,
                           const double *alpha,
                           const double *A, int lda,
                           double *B, int ldb);
```

Double‑precision version of `cublasTrmm`.

---  

## cublasCtrmm

```
cublasStatus_t cublasCtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m, int n,
                           const cuComplex *alpha,
                           const cuComplex *A, int lda,
                           cuComplex *B, int ldb);
```

Complex‑single precision version of `cublasTrmm`.

---  

## cublasZtrmm

```
cublasStatus_t cublasZtrmm(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int m, int n,
                           const cuDoubleComplex *alpha,
                           const cuDoubleComplex *A, int lda,
                           cuComplex *B, int ldb);
```

Complex‑double precision version of `cublasTrmm`.

---  

## cublasTrsv

```
cublasStatus_t cublasTrsv(cublasHandle_t handle,
                          cublasSideMode_t side,
                          cublasFillMode_t uplo,
                          cublasOperation_t trans,
                          cublasDiagType_t diag,
                          int n,
                          const float *A, int lda,
                          float *B, int ldb);
```

Solves a triangular linear system with a single right‑hand side (real).

---  

## cublasDtrsv

```
cublasStatus_t cublasDtrsv(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int n,
                           const double *A, int lda,
                           double *B, int ldb);
```

Double‑precision version of `cublasTrsv`.

---  

## cublasCtrsv

```
cublasStatus_t cublasCtrsv(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int n,
                           const cuComplex *A, int lda,
                           cuComplex *B, int ldb);
```

Complex‑single precision version of `cublasTrsv`.

---  

## cublasZtrsv

```
cublasStatus_t cublasZtrsv(cublasHandle_t handle,
                           cublasSideMode_t side,
                           cublasFillMode_t uplo,
                           cublasOperation_t trans,
                           cublasDiagType_t diag,
                           int n,
                           const cuDoubleComplex *A, int lda,
                           cuDoubleComplex *B, int ldb);
```

Complex‑double precision version of `cublasTrsv`.

---  

## cublasStrmm, cublasDtrmm, cublasCtrmm, cublasZtrmm, cublasStrsv,
cublasDtrsv, cublasCtrsv, cublasZtrsv

These eight routines are listed again here for completeness; their
prototypes and descriptions are identical to the ones already given
above (see the individual entries).

---  

### **All error values common to every routine**

| Error value | Meaning |
|---|---|
| `CUBLAS_STATUS_SUCCESS` | The operation completed successfully |
| `CUBLAS_STATUS_NOT_INITIALIZED` | The cuBLAS library was not initialized |
| `CUBLAS_STATUS_INVALID_VALUE` | One or more arguments were illegal (e.g., null pointer, negative size) |
| `CUBLAS_STATUS_EXECUTION_FAILED` | Kernel launch failed on the GPU |
| `CUBLAS_STATUS_ALLOC_FAILED` | Internal memory allocation failed |
| `CUBLAS_STATUS_ARCH_MISMATCH` | Requested feature requires an unsupported architecture |
| `CUBLAS_STATUS_NOT_SUPPORTED` | The requested feature is not supported in the current build |