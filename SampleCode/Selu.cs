using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Node;
using BrightWire.LinearAlgebra.Helper;
using BrightWire.TrainingData.WellKnown;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    partial class Program
    {
        class SeluActivation : NodeBase
        {
            const float ALPHA = 1.6732632423543772848170429916717f;
            const float SCALE = 1.0507009873554804934193349852946f;

            class Backpropagation : SingleBackpropagationBase<SeluActivation>
            {
                public Backpropagation(SeluActivation source) : base(source) { }

                protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
                {
                    var matrix = errorSignal.GetMatrix().AsIndexable();
                    var delta = context.LinearAlgebraProvider.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => {
                        var x = matrix[i, j];
                        if(x >= 0)
                            return SCALE;
                        else
                            return SCALE * ALPHA * BoundMath.Exp(x);
                    });
                    return errorSignal.ReplaceWith(delta);
                }
            }

            public SeluActivation(string name = null) : base(name) {}

            public override void ExecuteForward(IContext context)
            {
                var matrix = context.Data.GetMatrix().AsIndexable();
                var output = context.LinearAlgebraProvider.CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => {
                    var x = matrix[i, j];
                    if (x >= 0)
                        return SCALE * x;
                    else
                        return SCALE * (ALPHA * BoundMath.Exp(x) - ALPHA);
                });
                _AddNextGraphAction(context, context.Data.ReplaceWith(output), () => new Backpropagation(this));
            }
        }

        public static void TrainWithSelu(string dataFilesPath)
        {
            using (var lap = BrightWireGpuProvider.CreateLinearAlgebra()) {
                var graph = new GraphFactory(lap);

                // parse the iris CSV into a data table and normalise
                var dataTable = new StreamReader(new MemoryStream(File.ReadAllBytes(dataFilesPath))).ParseCSV(',').Normalise(NormalisationType.Standard);

                // split the data table into training and test tables
                var split = dataTable.Split(0);
                var trainingData = graph.CreateDataSource(split.Training);
                var testData = graph.CreateDataSource(split.Test);

                // use a one hot encoding error metric, rmsprop gradient descent and xavier weight initialisation
                var errorMetric = graph.ErrorMetric.OneHotEncoding;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.GaussianWeightInitialisation(true, 0.1f, GaussianVarianceCalibration.SquareRoot2N, GaussianVarianceCount.FanInFanOut))
                ;

                // create the training engine and schedule a training rate change
                const float TRAINING_RATE = 0.01f;
                var engine = graph.CreateTrainingEngine(trainingData, TRAINING_RATE, 128);

                // create the network
                graph.Connect(engine)
                    .AddFeedForward(32)
                    .Add(new SeluActivation())
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagation(errorMetric)
                ;

                // train the network
                engine.Train(1000, testData, errorMetric, null, 50);
            }
        }
    }
}
