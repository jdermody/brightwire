using BrightData;
using BrightData.DataTable2;

namespace ExampleCode.DataTableTrainers
{
    internal class IrisTrainer : DataTableTrainer
    {
        public string[] Labels { get; }

        public IrisTrainer(BrightDataTable table, string[] labels) : base(table)
        {
            Labels = labels;
        }

        public void TrainSigmoidNeuralNetwork(uint hiddenLayerSize, uint numIterations, float trainingRate, uint batchSize)
        {
            base.TrainSigmoidNeuralNetwork(hiddenLayerSize, numIterations, trainingRate, batchSize, 50);
        }
    }
}
