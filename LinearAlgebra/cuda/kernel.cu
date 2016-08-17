//Includes for IntelliSense 
#define _SIZE_T_DEFINED
#ifndef __CUDACC__
#define __CUDACC__
#endif
#ifndef __cplusplus
#define __cplusplus
#endif

#define BLOCKSIZE 32
#define BLOCKSIZE2 BLOCKSIZE*BLOCKSIZE

#include <cuda.h>
#include <device_launch_parameters.h>
#include <texture_fetch_functions.h>
#include "float.h"
#include <builtin_types.h>
#include <vector_functions.h>

extern "C"
{
	/*const float TOO_SMALL = -1.0E20f;
	const float TOO_BIG = 1.0E20f;

	__global__ float _Constrain(float d)
	{
		if (isnan(d))
			return 0;
		else if (isinf(d))
			return TOO_BIG;
		else if (d < TOO_SMALL)
			return TOO_SMALL;
		else if (d > TOO_BIG)
			return TOO_BIG;
		return d;
	}*/

	__global__ void PointwiseMultiply(float* a, float* b, int size)
	{
		int tidX = threadIdx.x;
		int index = blockDim.x * blockIdx.x + tidX;

		// read blockA into shared memory
		__shared__ float blockA[BLOCKSIZE2];
		if (index < size)
			blockA[tidX] = a[index];
		__syncthreads();

		if (index < size)
            b[index] *= blockA[tidX];
	}

	__global__ void PointwiseDivide(float* a, float* b, int size)
	{
		int tidX = threadIdx.x;
		int index = blockDim.x * blockIdx.x + tidX;

		// read blockA into shared memory
		__shared__ float blockA[BLOCKSIZE2];
		if (index < size)
			blockA[tidX] = a[index];
		__syncthreads();

		if (index < size)
            b[index] = blockA[tidX] / b[index];
	}

	__global__ void Sqrt(float* a, float* b, int size, float valueAdjustment)
	{
		int tidX = threadIdx.x;
		int index = blockDim.x * blockIdx.x + tidX;

		// read blockA into shared memory
		__shared__ float blockA[BLOCKSIZE2];
		if (index < size)
			blockA[tidX] = a[index];
		__syncthreads();

		if (index < size) {
            b[index] = sqrt(blockA[tidX] + valueAdjustment);
		}
	}

	__global__ void AddInPlace(float* a, float* b, int size, float coefficient1, float coefficient2)
	{
		int tidX = threadIdx.x;
		int index = blockDim.x * blockIdx.x + tidX;

		// read blockB into shared memory
		__shared__ float blockB[BLOCKSIZE2];
		if (index < size)
			blockB[tidX] = b[index];
		__syncthreads();

		if (index < size)
            a[index] = (a[index] * coefficient1) + (blockB[tidX] * coefficient2);
	}

	__global__ void SubtractInPlace(float* a, float* b, int size, float coefficient1, float coefficient2)
	{
		int tidX = threadIdx.x;
		int index = blockDim.x * blockIdx.x + tidX;

		// read blockB into shared memory
		__shared__ float blockB[BLOCKSIZE2];
		if (index < size)
			blockB[tidX] = b[index];
		__syncthreads();

		if (index < size)
            a[index] = (a[index] * coefficient1) - (blockB[tidX] * coefficient2);
	}

	__global__ void Transpose(float* a, float* b, int rows, int columns)
	{
		int i = BLOCKSIZE * blockIdx.x + threadIdx.x;
		int j = BLOCKSIZE * blockIdx.y + threadIdx.y;

		// read the data into shared memory
		__shared__ float block[BLOCKSIZE][BLOCKSIZE+1];
		if (i < rows && j < columns)
            block[threadIdx.y][threadIdx.x] = a[j * rows + i];
		__syncthreads();

		// write output
		i = blockIdx.y * BLOCKSIZE + threadIdx.x;
        j = blockIdx.x * BLOCKSIZE + threadIdx.y;
		if (i < columns && j < rows)
			b[j * columns + i] = block[threadIdx.x][threadIdx.y];
	}

	__global__ void InitData(float* a, int size, float value)
	{
		int index = blockDim.x * blockIdx.x + threadIdx.x;
		if (index < size)
            a[index] = value;
	}

	__global__ void AddToEachRow(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns)
            a[j * rows + i] += b[j];
	}

	__global__ void AddToEachColumn(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns)
			a[j * rows + i] += b[i];
	}

	__global__ void TanH(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns) {
			int index = j * rows + i;
			b[index] = tanh(a[index]);
		}
	}

	__global__ void TanHDerivative(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns) {
			int index = j * rows + i;
			b[index] = 1.0f - pow(tanh(a[index]), 2);
		}
	}

	__global__ void Sigmoid(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns) {
			int index = j * rows + i;
            b[index] = 1.0f / (1.0f + exp(-1.0f * a[index]));
		}
	}

	__global__ void SigmoidDerivative(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns) {
			int index = j * rows + i;
            float sigmoid = 1.0f / (1.0f + exp(-1.0f * a[index]));
            b[index] = sigmoid * (1.0f - sigmoid);
        }
	}

	__global__ void RELU(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns) {
			int index = j * rows + i;
			float val = a[index];
			b[index] = (val <= 0) ? 0 : val;
		}
	}

	__global__ void RELUDerivative(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns) {
			int index = j * rows + i;
			float val = a[index];
			b[index] = (val <= 0) ? 0 : 1;
		}
	}

	__global__ void LeakyRELU(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns) {
			int index = j * rows + i;
			float val = a[index];
			b[index] = (val <= 0) ? 0.01f*val : val;
		}
	}

	__global__ void LeakyRELUDerivative(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns) {
			int index = j * rows + i;
            float val = a[index];
            b[index] = (val <= 0) ? 0.01f : 1;
        }
	}

	__global__ void SumRows(float* a, float* b, int rows, int columns)
	{
		// TODO: synchronised read?

		int row = blockIdx.x * blockDim.x + threadIdx.x;
		if (row < rows) {
            float temp = 0;
            for (int i = 0; i < columns; i++)
                temp += a[i * rows + row];
            b[row] = temp;
        }
	}

	__global__ void SumColumns(float* a, float* b, int rows, int columns)
	{
		// TODO: synchronised read?

		int column = blockIdx.x * blockDim.x + threadIdx.x;
		if (column < columns) {
            float temp = 0;
            for (int i = 0; i < rows; i++)
                temp += a[column * rows + i];
            b[column] = temp;
        }
	}

	__global__ void MemClear(float* data, int count, int srcOffset, int srcIncrement)
	{
		int index = blockIdx.x * blockDim.x + threadIdx.x;
        if (index < count) {
            int arrayIndex = srcOffset + (index * srcIncrement);
            data[arrayIndex] = 0.0f;
        }
	}

	__global__ void MemCopy(float* a, float* b, int count, int srcOffset, int srcIncrement)
	{
		int index = blockIdx.x * blockDim.x + threadIdx.x;
        if (index < count) {
            int arrayIndex = srcOffset + (index * srcIncrement);
            b[index] = a[arrayIndex];
        }
	}

	__global__ void SparseLoad(float* data, int* offset, int* length, int* destinationIndex, float* src, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;
		if (i < rows && j < columns) {
			int len = length[j];
			if(i < len) {
				int off = offset[j];
				int index = j * rows + destinationIndex[off + i];
				data[index] = src[off + i];
			}
		}
	}

	__global__ void FindMinAndMax(float* data, int count, float* minBlock, float* maxBlock)
	{
		int tidX = threadIdx.x;
		int blockX = blockIdx.x;
		int index = blockDim.x * blockX + tidX;

		// read block into shared memory
		__shared__ float block[BLOCKSIZE2];
		block[tidX] = (index < count) ? data[index] : 0;
		__syncthreads();

		// aggregate per block
		if (tidX == 0) {
			float min = FLT_MAX, max = FLT_MIN;
			int maxIndex = BLOCKSIZE2;
			if (count - index < BLOCKSIZE2)
				maxIndex = count - index;
			for (int i = 0; i < maxIndex; i++) {
				float val = block[i];
				if (val > max)
					max = val;
				if (val < min)
					min = val;
			}
			minBlock[blockX] = min;
			maxBlock[blockX] = max;
		}
	}

	__global__ void FindSum(float* data, int count, float* sum)
	{
		int tidX = threadIdx.x;
		int blockX = blockIdx.x;
		int index = blockDim.x * blockX + tidX;

		// read block into shared memory
		__shared__ float block[BLOCKSIZE2];
		if (index < count)
			block[tidX] = data[index];
		__syncthreads();

		// aggregate per block
		if (tidX == 0) {
			float total = 0;
			int maxIndex = BLOCKSIZE2;
			if (count - blockX * BLOCKSIZE2 < BLOCKSIZE2)
				maxIndex = count - blockX * BLOCKSIZE2;
			for (int i = 0; i < maxIndex; i++) {
				total += block[i];
			}
			sum[blockX] = total;
		}
	}

	__global__ void FindStdDev(float* data, int count, float mean, float* stdDev)
	{
		int tidX = threadIdx.x;
		int blockX = blockIdx.x;
		int index = blockDim.x * blockX + tidX;

		// read block into shared memory
		__shared__ float block[BLOCKSIZE2];
		if (index < count)
			block[tidX] = data[index];
		__syncthreads();

		// aggregate per block
		if (tidX == 0) {
			float total = 0;
			int maxIndex = BLOCKSIZE2;
			if (count - blockX * BLOCKSIZE2 < BLOCKSIZE2)
				maxIndex = count - blockX * BLOCKSIZE2;
			for (int i = 0; i < maxIndex; i++) {
				total += pow(block[i] - mean, 2);
			}
			stdDev[blockX] = total;
		}
	}

	__global__ void Constrain(float* data, int count, float min, float max)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;

		if (i < count) {
			float val = data[i];
			if (val < min)
				data[i] = min;
			if (val > max)
				data[i] = max;
		}
	}

	__global__ void Pow(float* a, float* b, int count, float power)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;

		if (i < count) {
			float val = a[i];
			b[i] = pow(val, power);
		}
	}

	__global__ void Diagonal(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;

		if (i < rows && i < columns) {
			b[i] = a[i * rows + i];
		}
	}

	__global__ void L1Regularisation(float* a, int count, float coefficient)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;

		if (i < count) {
			float val = a[i];
			a[i] = val - ((val > 0 ? 1 : val < 0 ? -1 : 0) * coefficient);
		}
	}

	__global__ void PointwiseDivideRows(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;

		if (i < rows && j < columns) {
			int index = j * rows + i;
			float val = a[index];
			a[index] = val / b[i];
		}
	}

	__global__ void PointwiseDivideColumns(float* a, float* b, int rows, int columns)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;

		if (i < rows && j < columns) {
			int index = j * rows + i;
			float val = a[index];
			a[index] = val / b[j];
		}
	}

	__global__ void SplitRows(float* a, float* b, float* c, int rows, int columns, int position)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;

		if (i < rows && j < columns) {
			int index = j * rows + i;
			float val = a[index];
			if(j >= position) {
				int diff = j - position;
				c[diff * rows + i] = val;
			}else
				b[index] = val;
		}
	}

	__global__ void SplitColumns(float* a, float* b, float* c, int rows, int columns, int position)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;

		if (i < rows && j < columns) {
			float val = a[j * rows + i];
			if(i >= position) {
				int diff = i - position;
				c[j * (rows-position) + diff] = val;
			}else
				b[j * position + i] = val;
		}
	}

	__global__ void ConcatColumns(float* a, float* b, float* c, int rows, int columns, int topRowCount, int bottomRowCount)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;

		if (i < rows && j < columns) {
			float val;
			if(i >= topRowCount)
				val = b[j * bottomRowCount + i - topRowCount];
			else
				val = a[j * topRowCount + i];
			c[j * rows + i] = val;
		}
	}

	__global__ void ConcatRows(float* a, float* b, float* c, int rows, int columns, int leftColumnCount)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;
		int j = blockDim.y * blockIdx.y + threadIdx.y;

		if (i < rows && j < columns) {
			float val;
			if(j >= leftColumnCount)
				val = b[(j-leftColumnCount) * rows + i];
			else
				val = a[j * rows + i];
			c[j * rows + i] = val;
		}
	}

	__global__ void EuclideanDistance(float* a, float* b, float* c, int count)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;

		if (i < count) {
			c[i] = pow(a[i] - b[i], 2);
		}
	}

	__global__ void ManhattanDistance(float* a, float* b, float* c, int count)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;

		if (i < count) {
			c[i] = abs(a[i] - b[i]);
		}
	}

	__global__ void Abs(float* a, float* b, int count)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;

		if (i < count) {
			b[i] = abs(a[i]);
		}
	}

	__global__ void Normalise(float* a, int count, float min, float range)
	{
		int i = blockDim.x * blockIdx.x + threadIdx.x;

		if (i < count) {
			a[i] = (a[i] - min) / range;
		}
	}
}