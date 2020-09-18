using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.FloatTensors
{
    class SimpleLinearAlgebraProvider : ILinearAlgebraProvider
    {
        public SimpleLinearAlgebraProvider(IBrightDataContext context, bool isStochastic)
        {
            Context = context;
            IsStochastic = isStochastic;
        }

        public void Dispose()
        {
        }

        public string Name { get; } = "Simple";
        public IBrightDataContext Context { get; }
        public IFloatVector CreateVector(uint length, bool setToZero = false)
        {
            var segment = Context.CreateSegment<float>(length);
            if(setToZero)
                segment.Initialize(0f);
            return new FloatVector(new Vector<float>(segment));
        }

        public IFloatVector CreateVector(uint length, Func<uint, float> init)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix CreateMatrix(uint rows, uint columns, bool setToZero = false)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix CreateMatrix(uint rows, uint columns, Func<uint, uint, float> init)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix CreateMatrixFromRows(params IFloatVector[] vectorRows)
        {
            throw new NotImplementedException();
        }

        public IFloatMatrix CreateMatrixFromColumns(params IFloatVector[] vectorColumns)
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor Create3DTensor(uint rows, uint columns, uint depth, bool setToZero = false)
        {
            throw new NotImplementedException();
        }

        public I3DFloatTensor Create3DTensor(params IFloatMatrix[] matrices)
        {
            throw new NotImplementedException();
        }

        public I4DFloatTensor Create4DTensor(uint rows, uint columns, uint depth, uint count, bool setToZero = false)
        {
            throw new NotImplementedException();
        }

        public I4DFloatTensor Create4DTensor(params I3DFloatTensor[] tensors)
        {
            throw new NotImplementedException();
        }

        public I4DFloatTensor Create4DTensor(params Tensor3D<float>[] tensors)
        {
            throw new NotImplementedException();
        }

        public void PushLayer()
        {
            throw new NotImplementedException();
        }

        public void PopLayer()
        {
            throw new NotImplementedException();
        }

        public bool IsStochastic { get; }
        public bool IsGpu { get; } = false;


        public IFloatMatrix CalculateDistances(IFloatVector[] vectors, IReadOnlyList<IFloatVector> comparison, DistanceMetric distanceMetric)
        {
            throw new NotImplementedException();
        }

        public IFloatVector CreateVector(ITensorSegment<float> data)
        {
            throw new NotImplementedException();
        }
    }
}
