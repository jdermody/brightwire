//Includes for IntelliSense 
#define _SIZE_T_DEFINED
#ifndef __CUDACC__
#define __CUDACC__
#endif
#ifndef __cplusplus
#define __cplusplus
#endif

#include <cuda.h>
#include <device_launch_parameters.h>
#include "float.h"
#include <builtin_types.h>
#include <vector_functions.h>

#define BLOCKSIZE 32
#define N BLOCKSIZE*BLOCKSIZE
#define NEG_INF __int_as_float(0xff800000)
#define POS_INF __int_as_float(0x7f800000)

typedef unsigned int uint;

extern "C"
{
    __global__ void IsFinite(const float* __restrict a, float* __restrict b, uint size, uint ai)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index] = isfinite(a[index * ai]) ? 0 : 1;
        }
	}

    __global__ void Scale(float* __restrict a, uint size, float scale, uint ai)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            a[index * ai] *= scale;
        }
	}

	__global__ void PointwiseMultiply(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index * bi] *= a[index * ai];
        }
	}

	__global__ void PointwiseDivide(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index * bi] = a[index * ai] / b[index * bi];
        }
	}

	__global__ void Sqrt(const float* __restrict a, float* __restrict b, uint size, float valueAdjustment, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index * bi] = sqrt(a[index * ai] + valueAdjustment);
        }
	}

	__global__ void AddInPlace(float* __restrict a, const float* __restrict b, uint size, float coefficient1, float coefficient2, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            a[index * ai] = (a[index * ai] * coefficient1) + (b[index * bi] * coefficient2);
        }
	}

	__global__ void SubtractInPlace(float* __restrict a, const float* __restrict b, uint size, float coefficient1, float coefficient2, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            a[index * ai] = (a[index * ai] * coefficient1) - (b[index * bi] * coefficient2);
        }
	}

	__global__ void AddToEachRow(float* __restrict a, const float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                a[j * rows + i] += b[j];
            }
        }
	}

	__global__ void AddToEachColumn(float* __restrict a, const float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                a[j * rows + i] += b[i];
            }
        }
	}

    __global__ void MultiplyByEachRow(float* __restrict a, const float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                a[j * rows + i] *= b[j];
            }
        }
	}

	__global__ void MultiplyByEachColumn(float* __restrict a, const float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                a[j * rows + i] *= b[i];
            }
        }
	}

	__global__ void TanH(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index * bi] = tanh(a[index * ai]);
        }
	}

	__global__ void TanHDerivative(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float ta = tanh(a[index * ai]);
            b[index * bi] = 1.0f - ta * ta;
        }
	}

	__global__ void Sigmoid(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index * bi] = 1.0f / (1.0f + exp(-1.0f * a[index * ai]));
        }
	}

	__global__ void SigmoidDerivative(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float sigmoid = 1.0f / (1.0f + exp(-1.0f * a[index * ai]));
			b[index * bi] = sigmoid * (1.0f - sigmoid);
        }
	}

	__global__ void RELU(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float val = a[index * ai];
			b[index * bi] = (val <= 0) ? 0 : val;
        }
	}

	__global__ void RELUDerivative(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float val = a[index * ai];
			b[index * bi] = (val <= 0) ? 0 : 1;
        }
	}

	__global__ void LeakyRELU(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float val = a[index * ai];
			b[index * bi] = (val <= 0) ? 0.01f*val : val;
        }
	}

	__global__ void LeakyRELUDerivative(const float* __restrict a, float* __restrict b, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float val = a[index * ai];
			b[index * bi] = (val <= 0) ? 0.01f : 1;
        }
	}

	__global__ void Reverse(const float* __restrict a, float* __restrict b, uint size)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[size - index - 1] = a[index];
        }
	}

	__global__ void SumRows(const float* __restrict a, float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                atomicAdd(b + i, a[j * rows + i]);
            }
        }
	}

	__global__ void SumColumns(const float* __restrict a, float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                atomicAdd(b + j, a[j * rows + i]);
            }
        }
	}

	__global__ void MemSet(float* a, float val, uint count, uint offset, uint ai)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
			a[offset + (index * ai)] = val;
        }
	}

    __global__ void MemCpy(float* __restrict a, float* __restrict b, uint count, uint offsetA, uint offsetB, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
			a[offsetA + (index * ai)] = b[offsetB + (index * bi)];
        }
	}

	__global__ void FindMinAndMax(const float* __restrict a, uint count, float* __restrict minBlock, float* __restrict maxBlock, uint ai)
	{
		uint tidX = threadIdx.x;
		uint blockX = blockIdx.x;
		uint index = blockDim.x * blockX + tidX;

		// read block into shared memory
		__shared__ float block[N];
		block[tidX] = (index < count) ? a[index * ai] : 0;
		__syncthreads();

		// aggregate per block
		if (tidX == 0) {
			float min = FLT_MAX, max = FLT_MIN;
			uint maxIndex = N;
			if (count - index < N)
				maxIndex = count - index;
			for (uint i = 0; i < maxIndex; i++) {
				float val = block[i];
				if (i == 0 || val > max)
					max = val;
				if (i == 0 || val < min)
					min = val;
			}
			minBlock[blockX] = min;
			maxBlock[blockX] = max;
		}
	}

	__global__ void FindStdDev(const float* __restrict a, uint count, float mean, float* __restrict stdDev, uint ai)
	{
		uint tidX = threadIdx.x;
		uint blockX = blockIdx.x;
		uint index = blockDim.x * blockX + tidX;

		// read block into shared memory
		__shared__ float block[N];
		if (index < count)
			block[tidX] = a[index * ai];
		__syncthreads();

		// aggregate per block
		if (tidX == 0) {
			float total = 0;
			uint maxIndex = N;
			if (count - blockX * N < N)
				maxIndex = count - blockX * N;
			for (uint i = 0; i < maxIndex; i++) {
                float val = block[i] - mean;
				total += val * val;
			}
			stdDev[blockX] = total;
		}
	}

	__global__ void Constrain(float* __restrict a, uint count, float min, float max, uint ai)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            uint ind = index * ai;
            float val = a[ind];
			if (val < min || val == NEG_INF)
				a[ind] = min;
			if (val > max || val == POS_INF)
				a[ind] = max;
            if(isnan(val))
                a[ind] = 0;
        }
	}

    __global__ void RoundInPlace(float* __restrict a, uint count, float lower, float upper, float mid, uint ai)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            uint ind = index * ai;
            float val = a[ind * ai];
			if (val >= mid)
				a[ind * ai] = upper;
			else
				a[ind * ai] = lower;
        }
	}

	__global__ void Pow(const float* __restrict a, float* __restrict b, uint count, float power, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
			b[index * bi] = pow(a[index * ai], power);
        }
	}

	__global__ void Diagonal(const float* __restrict a, float* __restrict b, uint rows, uint columns, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < rows && index < columns; index += blockDim.x * gridDim.x) {
            b[index * bi] = a[index * ai * rows + index];
        }
	}

	__global__ void L1Regularisation(float* __restrict a, uint count, float coefficient, uint ai)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            float val = a[index * ai];
            float absVal = abs(val);
            if(absVal < coefficient)
                a[index * ai] = 0;
            else {
                float reduced = absVal - coefficient;
                a[index * ai] = (val > 0) ? reduced : -reduced;
            }
        }
	}

	__global__ void PointwiseDivideRows(float* __restrict a, const float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                uint index = j * rows + i;
			    float val = a[index];
			    a[index] = val / b[i];
            }
        }
	}

	__global__ void PointwiseDivideColumns(float* __restrict a, const float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                uint index = j * rows + i;
			    float val = a[index];
			    a[index] = val / b[j];
            }
        }
	}

	__global__ void SplitRows(const float* __restrict a, float* __restrict b, float* __restrict c, uint rows, uint columns, uint position)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                uint index = j * rows + i;
			    float val = a[index];
			    if(j >= position) {
				    uint diff = j - position;
				    c[diff * rows + i] = val;
			    }else
				    b[index] = val;
            }
        }
	}

	__global__ void SplitColumns(const float* __restrict a, float* __restrict b, float* __restrict c, uint rows, uint columns, uint position)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val = a[j * rows + i];
			    if(i >= position) {
				    uint diff = i - position;
				    c[j * (rows-position) + diff] = val;
			    }else
				    b[j * position + i] = val;
            }
        }
	}

	__global__ void ConcatColumns(const float* __restrict a, const float* __restrict b, float* __restrict c, uint rows, uint columns, uint topRowCount, uint bottomRowCount)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val;
			    if(i >= topRowCount)
				    val = b[j * bottomRowCount + i - topRowCount];
			    else
				    val = a[j * topRowCount + i];
			    c[j * rows + i] = val;
            }
        }
	}

	__global__ void ConcatRows(const float* __restrict a, const float* __restrict b, float* __restrict c, uint rows, uint columns, uint leftColumnCount)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val;
			    if(j >= leftColumnCount)
				    val = b[(j-leftColumnCount) * rows + i];
			    else
				    val = a[j * rows + i];
			    c[j * rows + i] = val;
            }
        }
	}

	__global__ void EuclideanDistance(const float* __restrict a, const float* __restrict b, float* __restrict c, uint count, uint ai, uint bi, uint ci)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            float val = a[index * ai] - b[index * bi];
            c[index * ci] = val * val;
        }
	}

	/*__global__ void MultiEuclideanDistance(const float* __restrict a, const float* __restrict* b, float* __restrict c, uint size, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val1 = a[i];
			    float val2 = b[j][i];
                float val3 = val1 - val2;
			    c[j * size + i] = val3 * val3;
            }
        }
	}*/

	__global__ void ManhattanDistance(const float* __restrict a, const float* __restrict b, float* __restrict c, uint count, uint ai, uint bi, uint ci)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            c[index * ci] = abs(a[index * ai] - b[index * bi]);
        }
	}

	/*__global__ void MultiManhattanDistance(const float* __restrict a, const float* __restrict* b, float* __restrict c, uint size, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val1 = a[i];
			    float val2 = b[j][i];
			    c[j * size + i] = abs(val1 - val2);
            }
        }
	}*/

	__global__ void CosineDistance(const float* __restrict a, const float* __restrict b, float* __restrict aa, float* __restrict ab, float* __restrict bb, uint count, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
			float left = a[index * ai];
			float right = b[index * bi];
			atomicAdd(aa, left * left);
			atomicAdd(ab, left * right);
			atomicAdd(bb, right * right);
        }
	}

	__global__ void Abs(const float* __restrict a, float* __restrict b, uint count, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            b[index * bi] = abs(a[index * ai]);
        }
	}

	__global__ void Log(const float* __restrict a, float* __restrict b, uint count, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            b[index * bi] = log(a[index * ai]);
        }
	}

    __global__ void Exp(const float* __restrict a, float* __restrict b, uint count, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            b[index * bi] = exp(a[index * ai]);
        }
	}

	__global__ void Normalise(float* __restrict a, uint count, float min, float range, uint ai)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            a[index * ai] = (a[index * ai] - min) / range;
        }
	}

	__global__ void SoftmaxVector(const float* __restrict a, float* __restrict b, uint count, float max, uint ai)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            b[index] = exp(a[index * ai] - max);
        }
	}

	__global__ void VectorAddInPlace(float* __restrict a, uint size, float scalar, uint ai)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            a[index * ai] += scalar;
        }
	}

	__global__ void VectorCopyRandom(const float* __restrict a, float* __restrict b, uint* __restrict c, uint size, uint ai, uint bi)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
			b[index * bi] += a[c[index] * ai];
        }
	}

	__global__ void CopyToMatrixRows(const float* __restrict* a, float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val = a[i][j];
			    b[j * rows + i] = val;
            }
        }
	}

    __global__ void CopyToMatrixColumns(const float* __restrict* a, float* __restrict b, uint rows, uint columns)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val = a[j][i];
                //printf("i:%i(%i) j:%i(%i)\n", i, rows, j, columns);
			    b[j * rows + i] = val;
            }
        }
	}

	__global__ void TensorAddPadding(
        uint size, 
        const float* __restrict a, 
        float* __restrict b, 
        uint rows, 
        uint columns, 
        uint depth, 
        uint count, 
        uint outputRows, 
        uint outputColumns, 
        uint padding
    ) {
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            uint i = index % outputRows;
            uint index2 = index / outputRows;

            uint j = index2 % outputColumns;
            uint index3 = index2 / outputColumns;

            uint k = index3 % depth;
            uint z = index3 / depth;

            float val = 0;
            if(i >= padding && i < (outputRows - padding) && j >= padding && j < (outputColumns - padding)) {
                const float* inputPtr = a + (rows * columns * depth * z) + (rows * columns * k);
                uint aIndex = (j-padding) * rows + (i-padding);
                val = inputPtr[aIndex];

                /*printf("index:%i i:%i(%i) j:%i(%i) k:%i(%i) z:%i(%i) ai:%i val:%f\n", index,
                    i, outputRows,
                    j, outputColumns, 
                    k, depth, 
                    z, count,
                    aIndex, val
                );*/
            }

            float* outputPtr = b + (outputRows * outputColumns * depth * z) + (outputRows * outputColumns * k);
            outputPtr[j * outputRows + i] = val;
        }
	}

	__global__ void TensorRemovePadding(
        uint size, 
        const float* __restrict a, 
        float* __restrict b, 
        uint rows, 
        uint columns, 
        uint depth, 
        uint count, 
        uint outputRows, 
        uint outputColumns, 
        uint padding
    ) {
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            uint i = index % rows;
            uint index2 = index / rows;

            uint j = index2 % columns;
            if(i >= padding && i < (rows-padding) && j >= padding && j < (columns-padding)) {
                uint index3 = index2 / columns;

                uint k = index3 % depth;
                uint z = index3 / depth;

                const float* inputPtr = a + (rows * columns * depth * z) + (rows * columns * k);
                uint aIndex = j * rows + i;
                float val = inputPtr[aIndex];

                float* outputPtr = b + (outputRows * outputColumns * depth * z) + (outputRows * outputColumns * k);
                uint bIndex = (j-padding) * outputRows + (i-padding);
                outputPtr[bIndex] = val;

                /*printf("index:%i i:%i(%i) j:%i(%i) k:%i(%i) z:%i(%i) ai:%i bi:%i val:%f\n", index,
                    i, outputRows,
                    j, outputColumns, 
                    k, depth, 
                    z, count,
                    aIndex, bIndex,
                    val
                );*/
            }
        }
	}

    __global__ void TensorIm2Col(
        uint size, 
        const float* __restrict a, 
        float* __restrict b, 
        const float* __restrict cx, 
        const float* __restrict cy, 
        uint rows,
        uint columns,
        uint depth,
        uint count,
        uint outputRows,
        uint outputColumns,
        uint convolutionCount, 
        uint filterWidth, 
        uint filterHeight,
        uint xStride,
		uint yStride
    ) {
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            uint x = index % filterWidth;
            uint index2 = index / filterWidth;

            uint y = index2 % filterHeight;
            uint index3 = index2 / filterHeight;

            uint k = index3 % depth;
            uint index4 = index3 / depth;

            uint ci = index4 % convolutionCount;
            uint i = index4 / convolutionCount;

            uint offsetX = cx[ci];
            uint offsetY = cy[ci];

            /*printf("index:%i, i:%i(%i), ci:%i(%i), k:%i(%i), x:%i(%i), y:%i(%i), cx:%i=%i, cy:%i=%i\n", index,
                i, count,
                ci, convolutionCount,
                k, depth,
                x, filterWidth,
                y, filterHeight,
                offsetX, (uint)cx[ci],
                offsetY, (uint)cy[ci]
            );*/

            uint filterOffset = k * filterWidth * filterHeight;
            uint filterIndex = filterOffset + (x * filterHeight + y);

            float* outputPtr = b + (outputRows * outputColumns * i);
            const float* inputPtr = a + (rows * columns * depth * i) + (rows * columns * k);
            outputPtr[filterIndex * outputRows + ci] = inputPtr[(offsetX + x) * rows + (offsetY + y)];
        }
    }

    __global__ void TensorReverseIm2Col(
        uint size, 
        const float* __restrict a, 
        const float* __restrict filters, 
        float* __restrict b, 
        const float* __restrict cx, 
        const float* __restrict cy, 
        uint rows, 
        uint columns, 
        uint depth, 
        uint count,
        uint convolutionCount,  
        uint filterWidth, 
        uint filterHeight, 
        uint xStride,
		uint yStride,
        uint outputRows,
        uint outputColumns,
        uint outputDepth
    ) {
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            uint z = index % outputDepth;
            uint index2 = index / outputDepth;

            uint x = index2 % filterWidth;
            uint index3 = index2 / filterWidth;

            uint y = index3 % filterHeight;
            uint index4 = index3 / filterHeight;

            uint ci = index4 % convolutionCount;
            uint index5 = index4 / convolutionCount;

            uint k = index5 % depth;
            uint i = index5 / depth;

            uint offsetX = cx[ci];
            uint offsetY = cy[ci];

            /*printf("index:%i di:%i(%i) ci:%i(%i) k:%i(%i) x:%i(%i) y:%i(%i) z:%i(%i) cx:%i cy:%i\n", index,
                i, count,
                ci, convolutionCount, 
                k, depth, 
                x, filterWidth, 
                y, filterHeight, 
                z, outputDepth, 
                offsetX, offsetY
            );*/

            const float* slice = a + (i * rows * columns * depth) + (k * rows * columns);
            const float* filter = filters + (k * outputDepth * filterWidth * filterHeight) + (z * filterWidth * filterHeight);
            float* output = b + (i * outputRows * outputColumns * outputDepth) + (z * outputRows * outputColumns);

            uint errorX = offsetX / xStride;
            uint errorY = offsetY / yStride;
            if(errorX < columns && errorY < rows) {
                float error = slice[errorX * rows + errorY];

                uint filterIndex = (filterWidth-x-1) * filterHeight + (filterHeight-y-1);
                uint outputIndex = (offsetX+x) * outputRows + (offsetY+y);
                float val = filter[filterIndex] * error;

                atomicAdd(output + outputIndex, val);
            }
        }
    }

	__global__ void SoftmaxDerivative(const float* __restrict a, float* __restrict b, uint size, uint ai)
	{
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < size; j += blockDim.y * gridDim.y) {
                uint index = j * size + i;
			    if(i == j)
				    b[index] = a[i * ai] * (1 - a[i * ai]);
			    else
				    b[index] = -a[i * ai] * a[j * ai];
            }
        }
	}

	__global__ void RotateInPlace(float* __restrict a, uint size, uint blockCount, uint blockSize)
	{
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            uint blockIndex = index / blockSize;
			uint blockOffset = index % blockSize;
            uint index1 = blockIndex * blockSize + blockSize - blockOffset - 1;
			uint index2 = blockIndex * blockSize + blockOffset; 
			float temp = a[index1];
			a[index1] = a[index2];
			a[index2] = temp;
        }
	}

	__global__ void TensorMaxPool(
        uint size, 
        const float* __restrict a, 
        float* __restrict b, 
        float* __restrict indexOffset,
        const float* __restrict cx, 
        const float* __restrict cy,
        uint convolutionCount,
        uint rows, 
        uint columns, 
        uint depth, 
        uint count, 
        uint outputRows, 
        uint outputColumns, 
        uint filterWidth, 
        uint filterHeight, 
        uint xStride,
		uint yStride,
        uint saveIndices
    ) {
		for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            uint ci = index % convolutionCount;
            uint index2 = index / convolutionCount;

            uint k = index2 % depth;
            uint z = index2 / depth;

            uint aX = cx[ci];
			uint aY = cy[ci];
            uint bX = aX / xStride;
            uint bY = aY / yStride;

            /*printf("index:%i k:%i(%i) z:%i(%i) ax:%i ay:%i bx:%i by:%i\n", index,
                k, depth, 
                z, count,
                aX, aY,
                bX, bY
            );*/

            uint targetOffset = (z * outputRows * outputColumns * depth) + (k * outputRows * outputColumns);
            const float* source = a + (z * rows * columns * depth) + (k * rows * columns);
            float* target = b + targetOffset;

            float maxVal = 0;
	        int bestOffset = -1;
	        uint offset = 0;
	                
	        for (uint x = 0; x < filterWidth; x++) {
		        for (uint y = 0; y < filterHeight; y++) {
			        float val = source[(aX + x) * rows + (aY + y)];
                    bool isGreater = (bestOffset < 0 || val > maxVal);
			        if (isGreater) {
				        bestOffset = offset;
				        maxVal = val;
			        }
                    //printf("index:%i, x:%i, y:%i val:%f max:%f offset:%i is-greater:%i\n", index, x, y, val, maxVal, bestOffset, isGreater ? 1 : 0);
					++offset;
		        }
	        }

            //printf("\tindex:%i i:%i j:%i val:%f\n", index, i, j, maxVal);
            if(saveIndices) {
                float* indices = indexOffset + targetOffset;
                indices[bX * outputRows + bY] = bestOffset;
            }
            target[bX * outputRows + bY] = maxVal;
        }
	}

	__global__ void TensorReverseMaxPool(
        uint size, 
        const float* __restrict a,
        const float* __restrict indices,
        float* __restrict b, 
        uint rows,
        uint columns,
        uint depth,
        uint count,
        uint outputRows,
        uint outputColumns,
        uint filterWidth,
        uint filterHeight,
        uint xStride,
		uint yStride
    ) {
        for (uint index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            uint i = index % rows;
            uint index2 = index / rows;

            uint j = index2 % columns;
            uint index3 = index2 / columns;

            uint k = index3 % depth;
            uint z = index3 / depth;

            uint sourceOffset = (z * rows * columns * depth) + (k * rows * columns);
            const float* source = a + sourceOffset;
            const float* indexPtr = indices + sourceOffset;
            float* target = b + (z * outputRows * outputColumns * depth) + (k * outputRows * outputColumns);
            uint sourceIndex = j * rows + i;
            float val = source[sourceIndex];
            int offset = indexPtr[sourceIndex];

            if(offset < 0)
                offset = 0;

            uint targetX = j * xStride + (offset / filterHeight);
            uint targetY = i * yStride + (offset % filterHeight);

            /*printf("index:%i s:%i i:%i(%i) j:%i(%i) k:%i(%i) z:%i(%i) val:%f offset:%i tx:%i ty:%i\n", 
                index, xStride, yStride
                i, outputRows,
                j, outputColumns, 
                k, depth, 
                z, count,
                val, offset,
                targetX, targetY
            );*/

            target[targetX * outputRows + targetY] = val;
        }
	}

    __global__ void CalculateMultiDistances(
        const float** __restrict a,
        const float** __restrict b,
        float* __restrict c,
        uint rows,
        uint columns,
        uint size,
        uint distanceMetric
    ) {
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                for (uint k = blockDim.z * blockIdx.z + threadIdx.z; k < rows; k += blockDim.z * gridDim.z) {
                    float aVal = a[j][i];
                    float bVal = b[k][i];
                    float output = 0;

                    if(distanceMetric == 0) { // euclidean
                        float diff = aVal - bVal;
                        output = diff * diff;
                    }else if(distanceMetric == 2) { // manhattan
                        output = abs(aVal - bVal);
                    }
                    float* outputPtr = c + (j * rows + k);
                    atomicAdd(outputPtr, output);
                }
            }
        }
	}

    __global__ void CalculateDistances(
        const float* __restrict a,
        const float** __restrict b,
        float* __restrict c,
        uint numVectors,
        uint size,
        uint distanceMetric
    ) {
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < numVectors; j += blockDim.y * gridDim.y) {
                float aVal = a[i];
                float bVal = b[j][i];
                float output = 0;

                if(distanceMetric == 0) { // euclidean
                    float diff = aVal - bVal;
                    output = diff * diff;
                }else if(distanceMetric == 2) { // manhattan
                    output = abs(aVal - bVal);
                }
                atomicAdd(c + j, output);
            }
        }
	}

	__global__ void CosineMultiDistance(
		const float** __restrict a, 
		const float** __restrict b, 
		float* __restrict aa, 
		float* __restrict ab, 
		float* __restrict bb, 
		uint rows,
        uint columns,
        uint size
	) {
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                for (uint k = blockDim.z * blockIdx.z + threadIdx.z; k < rows; k += blockDim.z * gridDim.z) {
					float aVal = a[j][i];
					float bVal = b[k][i];
					uint offset = j * rows + k;
					atomicAdd(aa + offset, aVal * aVal);
					atomicAdd(ab + offset, aVal * bVal);
					atomicAdd(bb + offset, bVal * bVal);
				}
            }
        }
	}

    __global__ void CosineDistances(
		const float* __restrict a, 
		const float** __restrict b, 
		float* __restrict aa, 
		float* __restrict ab, 
		float* __restrict bb, 
		uint numVectors,
        uint size
	) {
        for (uint i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (uint j = blockDim.y * blockIdx.y + threadIdx.y; j < numVectors; j += blockDim.y * gridDim.y) {
				float aVal = a[i];
				float bVal = b[j][i];
				atomicAdd(aa + j, aVal * aVal);
				atomicAdd(ab + j, aVal * bVal);
				atomicAdd(bb + j, bVal * bVal);
            }
        }
	}

    __global__ void SumValues(const float* __restrict a, uint count, float* __restrict sum, uint ai)
	{
		uint tidX = threadIdx.x;
		uint blockX = blockIdx.x;
		uint index = blockDim.x * blockX + tidX;

		// read block into shared memory
		__shared__ float block[N];
		if (index < count)
			block[tidX] = a[index * ai];
		__syncthreads();

		// aggregate per block
		if (tidX == 0) {
			float total = 0;
			uint maxIndex = N;
			if (count - blockX * N < N)
				maxIndex = count - blockX * N;
			for (uint i = 0; i < maxIndex; i++) {
				total += block[i];
			}
			sum[blockX] = total;
		}
	}
}