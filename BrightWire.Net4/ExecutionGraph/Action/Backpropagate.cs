using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    internal class Backpropagate : IAction
    {
        IErrorMetric _errorMetric;

        public Backpropagate(IErrorMetric errorMetric)
        {
            _errorMetric = errorMetric;
        }

        public void Initialise(string data)
        {
            _errorMetric = (IErrorMetric)Activator.CreateInstance(Type.GetType(data));
        }

        public string Serialise()
        {
            return _errorMetric.GetType().FullName;
        }

        public IGraphData Execute(IGraphData input, IContext context)
        {
            var output = input.GetMatrix();
            if (context.IsTraining) {
                var gradient = _errorMetric.CalculateGradient(output, context.BatchSequence.Target);
                //context.LearningContext?.Log("backprogation-error", gradient);
                context.Backpropagate(gradient.ToGraphData());
            }
            return input;
        }
    }
}
