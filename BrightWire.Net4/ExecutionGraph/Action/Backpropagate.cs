using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Action
{
    public class Backpropagate : IAction
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

        public void Execute(IMatrix input, IContext context)
        {
            context.SetOutput(input);

            if (context.IsTraining) {
                var gradient = _errorMetric.CalculateGradient(input, context.BatchSequence.Target);
                context.LearningContext?.Log("backprogation-error", gradient);
                context.Backpropagate(gradient);
            }
        }
    }
}
