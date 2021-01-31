using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.LinearAlgebra.FloatTensor;

namespace BrightData.LinearAlgebra
{
    internal class SimpleLinearAlgebraProvider : ILinearAlgebraProvider
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
                segment.InitializeTo(0f);
            return new FloatVector(new Vector<float>(segment));
        }

        public IFloatVector CreateVector(uint length, Func<uint, float> init)
        {
            return new FloatVector(Context.CreateVector(length, init));
        }

        public IFloatMatrix CreateMatrix(uint rows, uint columns, bool setToZero = false)
        {
            var ret = new FloatMatrix(Context.CreateMatrix<float>(rows, columns));
            if(setToZero)
                ret.Data.Initialize(0f);
            return ret;
        }

        public IFloatMatrix CreateMatrix(uint rows, uint columns, Func<uint, uint, float> init)
        {
            return new FloatMatrix(Context.CreateMatrix(rows, columns, init));
        }

        public IFloatMatrix CreateMatrixFromRows(params IFloatVector[] vectorRows)
        {
            return new FloatMatrix(Context.CreateMatrixFromRows(vectorRows.Select(v => v.Data).ToArray()));
        }

        public IFloatMatrix CreateMatrixFromColumns(params IFloatVector[] vectorColumns)
        {
            return new FloatMatrix(Context.CreateMatrixFromColumns(vectorColumns.Select(v => v.Data).ToArray()));
        }

        public I3DFloatTensor Create3DTensor(uint rows, uint columns, uint depth, bool setToZero = false)
        {
            var ret = new Float3DTensor(Context.CreateTensor3D<float>(depth, rows, columns));
            if(setToZero)
                ret.Data.Initialize(0f);
            return ret;
        }

        public I3DFloatTensor Create3DTensor(params IFloatMatrix[] matrices)
        {
            return new Float3DTensor(Context.CreateTensor3D(matrices.Select(m => m.Data).ToArray()));
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
