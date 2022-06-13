using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData2;
using MKLNET;

namespace BrightData.MKL
{
    public class MklComputationUnit : ComputationUnit
    {
        public MklComputationUnit(BrightDataContext2 context) : base(context)
        {
        }

        public override IVector CreateVector(ITensorSegment2 data) => new MklVector(data, this);

        public override float DotProduct(ITensorSegment2 tensor, ITensorSegment2 tensor2)
        {
            return Blas.dot(tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray());
        }

        public override ITensorSegment2 Add(ITensorSegment2 tensor, ITensorSegment2 tensor2)
        {
            var size = GetSize(tensor, tensor2);
            var result = CreateSegment(size);
            Vml.Add((int)size, tensor.GetLocalOrNewArray(), tensor2.GetLocalOrNewArray(), result.GetArrayForLocalUseOnly()!);
            return result;
        }
    }
}
