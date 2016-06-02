using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Connectionist.Training.Layer.Recurrent
{
    public class RecurrentLayerBase
    {
        protected internal INeuralNetworkLayerUpdater CreateLayer(int inputSize, int outputSize, INeuralNetworkFactory factory, INeuralNetworkLayerDescriptor template)
        {
            var descriptor = template.Clone();
            descriptor.Activation = ActivationType.None;
            var layer = factory.CreateLayer(inputSize, outputSize, descriptor);
            return factory.CreateUpdater(layer, template);
        }

        protected internal IMatrix Combine(IMatrix input, IMatrix memory, INeuralNetworkLayer inputLayer, INeuralNetworkLayer memoryLayer, Func<IMatrix, IMatrix> activation)
        {
            using (var inputOutput = inputLayer.Execute(input))
            using (var memoryOutput = memoryLayer.Execute(memory)) {
                inputOutput.AddInPlace(memoryOutput);
                return activation(inputOutput);
            }
        }
    }
}
