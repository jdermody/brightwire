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
#include <texture_fetch_functions.h>
#include "float.h"
#include <builtin_types.h>
#include <vector_functions.h>

#define BLOCKSIZE 16
#define BLOCKSIZE2 BLOCKSIZE*BLOCKSIZE

extern "C"
{
    __global__ void IsFinite(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index] = isfinite(a[index]) ? 0 : 1;
        }
	}

	__global__ void PointwiseMultiply(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index] *= a[index];
        }
	}

	__global__ void PointwiseDivide(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index] = a[index] / b[index];
        }
	}

	__global__ void Sqrt(float* a, float* b, int size, float valueAdjustment)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index] = sqrt(a[index] + valueAdjustment);
        }
	}

	__global__ void AddInPlace(float* a, float* b, int size, float coefficient1, float coefficient2)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            a[index] = (a[index] * coefficient1) + (b[index] * coefficient2);
        }
	}

	__global__ void SubtractInPlace(float* a, float* b, int size, float coefficient1, float coefficient2)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            a[index] = (a[index] * coefficient1) - (b[index] * coefficient2);
        }
	}

	__global__ void AddToEachRow(float* a, float* b, int rows, int columns)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                a[j * rows + i] += b[j];
            }
        }
	}

	__global__ void AddToEachColumn(float* a, float* b, int rows, int columns)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                a[j * rows + i] += b[i];
            }
        }
	}

	__global__ void TanH(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index] = tanh(a[index]);
        }
	}

	__global__ void TanHDerivative(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index] = 1.0f - pow(tanh(a[index]), 2);
        }
	}

	__global__ void Sigmoid(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[index] = 1.0f / (1.0f + exp(-1.0f * a[index]));
        }
	}

	__global__ void SigmoidDerivative(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float sigmoid = 1.0f / (1.0f + exp(-1.0f * a[index]));
			b[index] = sigmoid * (1.0f - sigmoid);
        }
	}

	__global__ void RELU(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float val = a[index];
			b[index] = (val <= 0) ? 0 : val;
        }
	}

	__global__ void RELUDerivative(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float val = a[index];
			b[index] = (val <= 0) ? 0 : 1;
        }
	}

	__global__ void LeakyRELU(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float val = a[index];
			b[index] = (val <= 0) ? 0.01f*val : val;
        }
	}

	__global__ void LeakyRELUDerivative(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            float val = a[index];
			b[index] = (val <= 0) ? 0.01f : 1;
        }
	}

	__global__ void Reverse(float* a, float* b, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            b[size - index - 1] = a[index];
        }
	}

	__global__ void SumRows(float* a, float* b, int rows, int columns)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < rows; index += blockDim.x * gridDim.x) {
            float temp = 0;
			for (int i = 0; i < columns; i++)
				temp += a[i * rows + index];
			b[index] = temp;
        }
	}

	__global__ void SumColumns(float* a, float* b, int rows, int columns)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < columns; index += blockDim.x * gridDim.x) {
            float temp = 0;
			for (int i = 0; i < rows; i++)
				temp += a[index * rows + i];
			b[index] = temp;
        }
	}

	__global__ void MemClear(float* data, int count, int srcOffset, int srcIncrement)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            int arrayIndex = srcOffset + (index * srcIncrement);
			data[arrayIndex] = 0.0f;
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
				if (i == 0 || val > max)
					max = val;
				if (i == 0 || val < min)
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
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            float val = data[index];
			if (val < min)
				data[index] = min;
			if (val > max)
				data[index] = max;
        }
	}

	__global__ void Pow(float* a, float* b, int count, float power)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            float val = a[index];
			b[index] = pow(val, power);
        }
	}

	__global__ void Diagonal(float* a, float* b, int rows, int columns)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < rows && index < columns; index += blockDim.x * gridDim.x) {
            b[index] = a[index * rows + index];
        }
	}

	__global__ void L1Regularisation(float* a, int count, float coefficient)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            float val = a[index];
			a[index] = val - ((val > 0 ? 1 : val < 0 ? -1 : 0) * coefficient);
        }
	}

	__global__ void PointwiseDivideRows(float* a, float* b, int rows, int columns)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                int index = j * rows + i;
			    float val = a[index];
			    a[index] = val / b[i];
            }
        }
	}

	__global__ void PointwiseDivideColumns(float* a, float* b, int rows, int columns)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                int index = j * rows + i;
			    float val = a[index];
			    a[index] = val / b[j];
            }
        }
	}

	__global__ void SplitRows(float* a, float* b, float* c, int rows, int columns, int position)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                int index = j * rows + i;
			    float val = a[index];
			    if(j >= position) {
				    int diff = j - position;
				    c[diff * rows + i] = val;
			    }else
				    b[index] = val;
            }
        }
	}

	__global__ void SplitColumns(float* a, float* b, float* c, int rows, int columns, int position)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val = a[j * rows + i];
			    if(i >= position) {
				    int diff = i - position;
				    c[j * (rows-position) + diff] = val;
			    }else
				    b[j * position + i] = val;
            }
        }
	}

	__global__ void ConcatColumns(float* a, float* b, float* c, int rows, int columns, int topRowCount, int bottomRowCount)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val;
			    if(i >= topRowCount)
				    val = b[j * bottomRowCount + i - topRowCount];
			    else
				    val = a[j * topRowCount + i];
			    c[j * rows + i] = val;
            }
        }
	}

	__global__ void ConcatRows(float* a, float* b, float* c, int rows, int columns, int leftColumnCount)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val;
			    if(j >= leftColumnCount)
				    val = b[(j-leftColumnCount) * rows + i];
			    else
				    val = a[j * rows + i];
			    c[j * rows + i] = val;
            }
        }
	}

	__global__ void EuclideanDistance(float* a, float* b, float* c, int count)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            c[index] = pow(a[index] - b[index], 2);
        }
	}

	__global__ void MultiEuclideanDistance(float* a, float** b, float* c, int size, int columns)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val1 = a[i];
			    float val2 = b[j][i];
			    c[j * size + i] = pow(val1 - val2, 2);
            }
        }
	}

	__global__ void ManhattanDistance(float* a, float* b, float* c, int count)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            c[index] = abs(a[index] - b[index]);
        }
	}

	__global__ void MultiManhattanDistance(float* a, float** b, float* c, int size, int columns)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val1 = a[i];
			    float val2 = b[j][i];
			    c[j * size + i] = abs(val1 - val2);
            }
        }
	}

	__global__ void Abs(float* a, float* b, int count)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            b[index] = abs(a[index]);
        }
	}

	__global__ void Log(float* a, float* b, int count)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            b[index] = log(a[index]);
        }
	}

	__global__ void Normalise(float* a, int count, float min, float range)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            a[index] = (a[index] - min) / range;
        }
	}

	__global__ void SoftmaxVector(float* a, float* b, int count, float max)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < count; index += blockDim.x * gridDim.x) {
            b[index] = exp(a[index] - max);
        }
	}

	__global__ void VectorAdd(float* a, int size, float scalar)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            a[index] += scalar;
        }
	}

	__global__ void VectorCopyRandom(float* a, float* b, int* c, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
			b[index] += a[c[index]];
        }
	}

	__global__ void CopyToMatrixRows(float** a, float* b, int rows, int columns)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val = a[i][j];
			    b[j * rows + i] = val;
            }
        }
	}

    __global__ void CopyToMatrixColumns(float** a, float* b, int rows, int columns)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < rows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < columns; j += blockDim.y * gridDim.y) {
                float val = a[j][i];
                //printf("i:%i(%i) j:%i(%i)\n", i, rows, j, columns);
			    b[j * rows + i] = val;
            }
        }
	}

	/*__global__ void VectorSplit(float* a, float** b, int inputSize, int blockSize)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < inputSize; index += blockDim.x * gridDim.x) {
            b[index / blockSize][index % blockSize] = a[index];
        }
	}

	__global__ void TensorConvertToVector(float** a, float* b, int matrixSize, int size)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            int offset = index / matrixSize;
			int index2 = index % matrixSize;
			b[index] = a[offset][index2];
        }
	}

	__global__ void TensorConvertToMatrix(float** a, float* b, int aRows, int aColumns, int bRows, int bColumns)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < bRows; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < bColumns; j += blockDim.y * gridDim.y) {
                int x = i % aRows;
			    int y = i / aRows;
			    b[j * bRows + i] = a[j][y * aRows + x];
            }
        }
	}*/

	__global__ void TensorAddPadding(
        int size, 
        float* a, 
        float* b, 
        int rows, 
        int columns, 
        int depth, 
        int count, 
        int outputRows, 
        int outputColumns, 
        int padding
    ) {
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            int i = index % outputRows;
            int index2 = index / outputRows;

            int j = index2 % outputColumns;
            int index3 = index2 / outputColumns;

            int k = index3 % depth;
            int z = index3 / depth;

            float val = 0;
            if(i >= padding && i < (outputRows - padding) && j >= padding && j < (outputColumns - padding)) {
                float* inputPtr = a + (rows * columns * depth * z) + (rows * columns * k);
                int aIndex = (j-padding) * rows + (i-padding);
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
        int size, 
        float* a, 
        float* b, 
        int rows, 
        int columns, 
        int depth, 
        int count, 
        int outputRows, 
        int outputColumns, 
        int padding
    ) {
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            int i = index % rows;
            int index2 = index / rows;

            int j = index2 % columns;
            if(i >= padding && i < (rows-padding) && j >= padding && j < (columns-padding)) {
                int index3 = index2 / columns;

                int k = index3 % depth;
                int z = index3 / depth;

                float* inputPtr = a + (rows * columns * depth * z) + (rows * columns * k);
                int aIndex = j * rows + i;
                float val = inputPtr[aIndex];

                float* outputPtr = b + (outputRows * outputColumns * depth * z) + (outputRows * outputColumns * k);
                int bIndex = (j-padding) * outputRows + (i-padding);
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
        int size, 
        float* a, 
        float* b, 
        float* cx, 
        float* cy, 
        int rows,
        int columns,
        int depth,
        int count,
        int outputRows,
        int outputColumns,
        int convolutionCount, 
        int filterWidth, 
        int filterHeight,
        int stride
    ) {
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            int x = index % filterWidth;
            int index2 = index / filterWidth;

            int y = index2 % filterHeight;
            int index3 = index2 / filterHeight;

            int k = index3 % depth;
            int index4 = index3 / depth;

            int ci = index4 % convolutionCount;
            int i = index4 / convolutionCount;
            
			//int extent = (rows - filterWidth) / stride + 1;
            //int offsetY = ci / extent * stride;
            //int offsetX = ci % extent * stride;

            int offsetX = cx[ci];
            int offsetY = cy[ci];

            /*printf("index:%i, i:%i(%i), ci:%i(%i), k:%i(%i), x:%i(%i), y:%i(%i), cx:%i=%i, cy:%i=%i\n", index,
                i, count,
                ci, convolutionCount,
                k, depth,
                x, filterWidth,
                y, filterHeight,
                offsetX, (int)cx[ci],
                offsetY, (int)cy[ci]
            );*/

            int filterOffset = k * filterWidth * filterHeight;
            int filterIndex = filterOffset + (x * filterHeight + y);

            float* outputPtr = b + (outputRows * outputColumns * i);
            float* inputPtr = a + (rows * columns * depth * i) + (rows * columns * k);
            outputPtr[filterIndex * outputRows + ci] = inputPtr[(offsetX + x) * rows + (offsetY + y)];
        }
    }

    __global__ void TensorReverseIm2Col(
        int size, 
        float* a, 
        float* filters, 
        float* b, 
        float* cx, 
        float* cy, 
        int rows, 
        int columns, 
        int depth, 
        int count,
        int convolutionCount,  
        int filterWidth, 
        int filterHeight, 
        int stride, 
        int outputRows,
        int outputColumns,
        int outputDepth
    ) {
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            int z = index % outputDepth;
            int index2 = index / outputDepth;

            int x = index2 % filterWidth;
            int index3 = index2 / filterWidth;

            int y = index3 % filterHeight;
            int index4 = index3 / filterHeight;

            int ci = index4 % convolutionCount;
            int index5 = index4 / convolutionCount;

            int k = index5 % depth;
            int i = index5 / depth;

            int offsetX = cx[ci];
            int offsetY = cy[ci];

            /*printf("index:%i di:%i(%i) ci:%i(%i) k:%i(%i) x:%i(%i) y:%i(%i) z:%i(%i) cx:%i cy:%i\n", index,
                i, count,
                ci, convolutionCount, 
                k, depth, 
                x, filterWidth, 
                y, filterHeight, 
                z, outputDepth, 
                offsetX, offsetY
            );*/

            float* slice = a + (i * rows * columns * depth) + (k * rows * columns);
            float* filter = filters + (k * outputDepth * filterWidth * filterHeight) + (z * filterWidth * filterHeight);
            float* output = b + (k * outputRows * outputColumns * outputDepth * count) 
                + (i * outputRows * outputColumns * outputDepth) 
                + (z * outputRows * outputColumns)
            ;

            int errorX = offsetX / stride;
            int errorY = offsetY / stride;
            float error = slice[errorX * rows + errorY];

            int filterIndex = x * filterHeight + y;
            int outputIndex = (offsetX+x) * outputRows + (offsetY+y);
            float val = filter[filterIndex] * error;

            output[outputIndex] = val;
        }
    }

	__global__ void SoftmaxDerivative(float* a, float* b, int size)
	{
        for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < size; i += blockDim.x * gridDim.x) {
            for (int j = blockDim.y * blockIdx.y + threadIdx.y; j < size; j += blockDim.y * gridDim.y) {
                int index = j * size + i;
			    if(i == j)
				    b[index] = a[i] * (1 - a[i]);
			    else
				    b[index] = -a[i] * a[j];
            }
        }
	}

	__global__ void RotateInPlace(float* a, int size, int blockCount, int blockSize)
	{
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            int blockIndex = index / blockSize;
			int blockOffset = index % blockSize;
            int index1 = blockIndex * blockSize + blockSize - blockOffset - 1;
			int index2 = blockIndex * blockSize + blockOffset; 
			float temp = a[index1];
			a[index1] = a[index2];
			a[index2] = temp;
        }
	}

	__global__ void TensorMaxPool(
        int size, 
        float* a, 
        float* b, 
        float* indexOffset,
        float* cx, 
        float* cy,
        int convolutionCount,
        int rows, 
        int columns, 
        int depth, 
        int count, 
        int outputRows, 
        int outputColumns, 
        int filterWidth, 
        int filterHeight, 
        int stride,
        int saveIndices
    ) {
		for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            int ci = index % convolutionCount;
            int index2 = index / convolutionCount;

            int k = index2 % depth;
            int z = index2 / depth;

            int aX = cx[ci];
			int aY = cy[ci];
            int bX = aX / stride;
            int bY = aY / stride;

            /*printf("index:%i k:%i(%i) z:%i(%i) ax:%i ay:%i bx:%i by:%i\n", index,
                k, depth, 
                z, count,
                aX, aY,
                bX, bY
            );*/

            int targetOffset = (z * outputRows * outputColumns * depth) + (k * outputRows * outputColumns);
            float* source = a + (z * rows * columns * depth) + (k * rows * columns);
            float* target = b + targetOffset;

            float maxVal = 0;
	        int bestOffset = -1;
	        int offset = 0;
	                
	        for (int x = 0; x < filterWidth; x++) {
		        for (int y = 0; y < filterHeight; y++) {
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
        int size, 
        float* a,
        float* indices,
        float* b, 
        int rows,
        int columns,
        int depth,
        int count,
        int outputRows,
        int outputColumns,
        int filterWidth,
        int filterHeight,
        int stride
    ) {
        for (int index = blockDim.x * blockIdx.x + threadIdx.x; index < size; index += blockDim.x * gridDim.x) {
            int i = index % rows;
            int index2 = index / rows;

            int j = index2 % columns;
            int index3 = index2 / columns;

            int k = index3 % depth;
            int z = index3 / depth;

            int sourceOffset = (z * rows * columns * depth) + (k * rows * columns);
            float* source = a + sourceOffset;
            float* indexPtr = indices + sourceOffset;
            float* target = b + (z * outputRows * outputColumns * depth) + (k * outputRows * outputColumns);
            int sourceIndex = j * rows + i;
            float val = source[sourceIndex];
            int offset = indexPtr[sourceIndex];

            if(offset < 0)
                offset = 0;

            int targetX = j * stride + (offset / filterHeight);
            int targetY = i * stride + (offset % filterHeight);

            /*printf("index:%i s:%i i:%i(%i) j:%i(%i) k:%i(%i) z:%i(%i) val:%f offset:%i tx:%i ty:%i\n", 
                index, stride,
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
}