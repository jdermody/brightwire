using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    class AddGate : GateBase
    {
        //class Backpropagation : BackpropagationBase
        //{
        //    public override IMatrix Backward(IMatrix errorSignal, IContext context, bool calculateOutput)
        //    {
        //        return errorSignal;
        //    }
        //}

        public AddGate(string name = null) : base(name) { }

        protected override void _Activate(IContext primary, IContext secondary)
        {
            var input1 = primary.Data.GetAsMatrix();
            var input2 = secondary.Data.GetAsMatrix();

            var output = input1.Add(input2);

            // default is to send the error signal
            primary.Add(new GraphAction(this, new MatrixGraphData(output)), null);// () => new Backpropagation());
        }
    }
}
