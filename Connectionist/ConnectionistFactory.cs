using BrightWire.Connectionist.Activation;
using BrightWire.Connectionist.Execution;
using BrightWire.Connectionist.Execution.Layer;
using BrightWire.Connectionist.Helper;
using BrightWire.Connectionist.Training;
using BrightWire.Connectionist.Training.Batch;
using BrightWire.Connectionist.Training.Helper;
using BrightWire.Connectionist.Training.Layer;
using BrightWire.Connectionist.Training.Manager;
using BrightWire.Connectionist.Training.WeightInitialisation;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist
{
    internal class ConnectionistFactory : INeuralNetworkFactory
    {
        readonly bool _stochastic;
        readonly ILinearAlgebraProvider _lap;
        readonly TrainerFactory _trainer;
        readonly UpdaterFactory _weightUpdater;
        readonly Dictionary<ActivationType, IActivationFunction> _activation = new Dictionary<ActivationType, IActivationFunction>();
        readonly Dictionary<WeightInitialisationType, IWeightInitialisation> _weightInitialisation = new Dictionary<WeightInitialisationType, IWeightInitialisation>();

        public ConnectionistFactory(ILinearAlgebraProvider lap, bool stochastic = true)
        {
            _lap = lap;
            _stochastic = stochastic;
            _trainer = new TrainerFactory(lap);
            _weightUpdater = new UpdaterFactory(lap);

            _activation.Add(ActivationType.Relu, new Relu());
            _activation.Add(ActivationType.LeakyRelu, new LeakyRelu());
            _activation.Add(ActivationType.Sigmoid, new Sigmoid());
            _activation.Add(ActivationType.Tanh, new Tanh());
            //_activation.Add(ActivationType.Softmax, new Softmax());
            _activation.Add(ActivationType.None, null);

            _weightInitialisation.Add(WeightInitialisationType.Gaussian, new Gaussian(stochastic));
            _weightInitialisation.Add(WeightInitialisationType.Xavier, new Xavier(stochastic));
            _weightInitialisation.Add(WeightInitialisationType.Identity, new Identity());
            _weightInitialisation.Add(WeightInitialisationType.Identity1, new Identity(0.1f));
            _weightInitialisation.Add(WeightInitialisationType.Identity01, new Identity(0.01f));
            _weightInitialisation.Add(WeightInitialisationType.Identity001, new Identity(0.001f));
        }

        public ILinearAlgebraProvider LinearAlgebraProvider { get { return _lap; } }

        public ITrainingDataProvider CreateTrainingDataProvider(IDataTable table, int classColumnIndex)
        {
            return new DataTableTrainingDataProvider(_lap, table, classColumnIndex);
        }

        public ITrainingDataProvider CreateTrainingDataProvider(IReadOnlyList<Tuple<float[], float[]>> data)
        {
            return new DenseTrainingDataProvider(_lap, data);
        }

        public ISequentialTrainingDataProvider CreateSequentialTrainingDataProvider(IReadOnlyList<Tuple<float[], float[]>[]> data)
        {
            return new DenseSequentialTrainingDataProvider(_lap, data);
        }

        public IActivationFunction GetActivation(ActivationType activation)
        {
            return _activation[activation];
        }

        public IWeightInitialisation GetWeightInitialisation(WeightInitialisationType type)
        {
            return _weightInitialisation[type];
        }

        public INeuralNetworkLayerTrainer CreateTrainer(int inputSize, int outputSize, LayerDescriptor descriptor)
        {
            var layerUpdater = CreateUpdater(inputSize, outputSize, descriptor);
            return _CreateLayerUpdater(layerUpdater, descriptor);
        }

        public INeuralNetworkLayerUpdater CreateUpdater(int inputSize, int outputSize, LayerDescriptor descriptor)
        {
            var layer = CreateLayer(inputSize, outputSize, descriptor);
            return CreateUpdater(layer, descriptor);
        }

        public INeuralNetworkLayer CreateLayer(int inputSize, int outputSize, LayerDescriptor descriptor)
        {
            return new Standard(_lap, inputSize, outputSize, descriptor, _activation[descriptor.Activation], _weightInitialisation[descriptor.WeightInitialisation]);
        }

        public INeuralNetworkTrainer CreateBatchTrainer(IReadOnlyList<INeuralNetworkLayerTrainer> layer, bool calculateTrainingError = true)
        {
            return new BatchTrainer(layer, _stochastic, calculateTrainingError);
        }

        public INeuralNetworkTrainer CreateBatchTrainer(LayerDescriptor descriptor, params int[] layerSizes)
        {
            return CreateBatchTrainer(
                Enumerable.Range(0, layerSizes.Length - 1)
                    .Select(i => CreateTrainer(layerSizes[i], layerSizes[i + 1], descriptor))
                    .ToList()
            );
        }

        public INeuralNetworkBidirectionalLayer CreateBidirectionalLayer(INeuralNetworkRecurrentLayer forward, INeuralNetworkRecurrentLayer backward = null)
        {
            return new Training.Layer.Recurrent.Bidirectional(forward, backward);
        }

        public INeuralNetworkRecurrentLayer CreateSimpleRecurrentLayer(int inputSize, int outputSize, LayerDescriptor descriptor)
        {
            return new Training.Layer.Recurrent.SimpleRecurrent(inputSize, outputSize, this, descriptor);
        }

        public INeuralNetworkRecurrentLayer CreateLstmRecurrentLayer(int inputSize, int outputSize, LayerDescriptor descriptor)
        {
            return new Training.Layer.Recurrent.Lstm(inputSize, outputSize, this, descriptor);
        }

        public INeuralNetworkRecurrentLayer CreateFeedForwardRecurrentLayer(int inputSize, int outputSize, LayerDescriptor descriptor)
        {
            return new Training.Layer.Recurrent.FeedForward(CreateTrainer(inputSize, outputSize, descriptor));
        }

        public INeuralNetworkRecurrentBatchTrainer CreateRecurrentBatchTrainer(IReadOnlyList<INeuralNetworkRecurrentLayer> layer, bool calculateTrainingError = true)
        {
            return new RecurrentBatchTrainer(_lap, layer, _stochastic, calculateTrainingError);
        }

        public INeuralNetworkBidirectionalBatchTrainer CreateBidirectionalBatchTrainer(IReadOnlyList<INeuralNetworkBidirectionalLayer> layer, bool calculateTrainingError = true, int padding = 0)
        {
            return new BidirectionalBatchTrainer(_lap, layer, calculateTrainingError, padding, _stochastic);
        }

        INeuralNetworkLayerUpdater _CreatePrimaryUpdater(INeuralNetworkLayer layer, RegularisationType type, float lambda)
        {
            switch (type) {
                case RegularisationType.L1:
                    return _weightUpdater.L1(layer, lambda);

                case RegularisationType.L2:
                    return _weightUpdater.L2(layer, lambda);

                //case RegularisationType.MaxNorm:
                //    return _weightUpdater.MaxNorm(layer, lambda);

                default:
                    return _weightUpdater.Simple(layer);
            }
        }

        public INeuralNetworkLayerUpdater CreateUpdater(INeuralNetworkLayer layer, LayerDescriptor descriptor)
        {
            var primary = _CreatePrimaryUpdater(layer, descriptor.Regularisation, descriptor.Lambda);

            switch (descriptor.WeightUpdate) {
                case WeightUpdateType.Adagrad:
                    return _weightUpdater.Adagrad(primary);

                case WeightUpdateType.Momentum:
                    return _weightUpdater.Momentum(primary, descriptor.Momentum);

                case WeightUpdateType.NesterovMomentum:
                    return _weightUpdater.NesterovMomentum(primary, descriptor.Momentum);

                case WeightUpdateType.RMSprop:
                    return _weightUpdater.RMSprop(primary, descriptor.DecayRate);

                case WeightUpdateType.Adam:
                    return _weightUpdater.Adam(primary, descriptor.DecayRate, descriptor.DecayRate2);

                default:
                    return primary;
            }
        }

        INeuralNetworkLayerTrainer _CreateLayerUpdater(INeuralNetworkLayerUpdater layerUpdater, LayerDescriptor init)
        {
            switch (init.LayerTrainer) {
                case LayerTrainerType.DropConnect:
                    return _trainer.DropConnect(layerUpdater, init.Dropout);

                case LayerTrainerType.Dropout:
                    return _trainer.Dropout(layerUpdater, init.Dropout);

                default:
                    return _trainer.Standard(layerUpdater);
            }
        }

        public IStandardExecution CreateFeedForward(FeedForwardNetwork network)
        {
            var layer = new List<StandardFeedForward>();

            foreach(var item in network.Layer)
                layer.Add(_ReadFeedForward(item));

            return new FeedForwardExecution(_lap, layer);
        }

        StandardFeedForward _ReadFeedForward(NetworkLayer layer)
        {
            var descriptor = LayerDescriptor.CreateFrom(layer);

            var bias = _lap.Create(layer.OutputSize, 0f);
            bias.Data = layer.Bias;

            var weight = _lap.Create(layer.InputSize, layer.OutputSize, 0f);
            weight.Data = layer.Weight;

            return new StandardFeedForward(weight, bias, _activation[descriptor.Activation]);
        }

        public ITrainingContext CreateTrainingContext(float learningRate, int batchSize, IErrorMetric errorMetric)
        {
            return new TrainingContext(learningRate, batchSize, errorMetric);
        }

        public ITrainingContext CreateTrainingContext(float learningRate, int batchSize, ErrorMetricType errorMetric)
        {
            return new TrainingContext(learningRate, batchSize, errorMetric.Create());
        }

        RecurrentLayerComponent _ReadComponent(RecurrentLayer network, ActivationType activation)
        {
            var c1 = _ReadFeedForward(network.Layer[0]);
            var c2 = _ReadFeedForward(network.Layer[1]);
            return new RecurrentLayerComponent(c1, c2, _activation[activation]);
        }

        IRecurrentLayerExecution _GetRecurrentExecution(RecurrentLayer network)
        {
            var type = network.Type;
            if (type == RecurrentLayerType.SimpleRecurrent) {
                return new SimpleRecurrent(_ReadComponent(network, network.Activation));
            }
            else if (type == RecurrentLayerType.Lstm) {
                var c = _ReadComponent(network, network.Activation);
                var i = _ReadComponent(network, ActivationType.Sigmoid);
                var f = _ReadComponent(network, ActivationType.Sigmoid);
                var o = _ReadComponent(network, ActivationType.Sigmoid);
                return new Lstm(c, i, f, o, _activation[network.Activation]);
            }
            else if (type == RecurrentLayerType.FeedForward)
                return new RecurrentFeedForward(_ReadFeedForward(network.Layer[0]));
            else
                throw new Exception("Unknown recurrent type: " + type.ToString());
        }

        public IRecurrentExecution CreateRecurrent(RecurrentNetwork network)
        {
            var layer = network.Layer.Select(l => _GetRecurrentExecution(l)).ToList();
            return new RecurrentExecution(_lap, layer, _lap.Create(network.Memory.Data));
        }

        public IBidirectionalRecurrentExecution CreateBidirectional(BidirectionalNetwork network)
        {
            var layerCount = network.Layer.Length;
            var layer = new List<Tuple<IRecurrentLayerExecution, IRecurrentLayerExecution>>();
            for (var i = 0; i < layerCount; i++) {
                var l = network.Layer[i];
                IRecurrentLayerExecution forward = null, backward = null;
                if (l.Forward != null)
                    forward = _GetRecurrentExecution(l.Forward);
                if (l.Backward != null)
                    backward = _GetRecurrentExecution(l.Backward);
                layer.Add(Tuple.Create(forward, backward));
            }
            return new BidirectionalExecution(_lap, layer, _lap.Create(network.ForwardMemory.Data), _lap.Create(network.BackwardMemory.Data), network.Padding);
        }

        public IFeedForwardTrainingManager CreateFeedForwardManager(
            INeuralNetworkTrainer trainer,
            string dataFile,
            ITrainingDataProvider testData,
            int? autoAdjustOnNoChangeCount = null
        )
        {
            return new FeedForwardManager(trainer, dataFile, testData, autoAdjustOnNoChangeCount);
        }

        public IRecurrentTrainingManager CreateRecurrentManager(
            INeuralNetworkRecurrentBatchTrainer trainer,
            string dataFile,
            ISequentialTrainingDataProvider testData,
            int memorySize,
            int? autoAdjustOnNoChangeCount = null
        )
        {
            return new RecurrentManager(trainer, dataFile, testData, memorySize, autoAdjustOnNoChangeCount);
        }

        public IBidirectionalRecurrentTrainingManager CreateBidirectionalManager(
            INeuralNetworkBidirectionalBatchTrainer trainer,
            string dataFile,
            ISequentialTrainingDataProvider testData,
            int memorySize,
            int? autoAdjustOnNoChangeCount = null
        )
        {
            return new BidirectionalManager(_lap, trainer, dataFile, testData, memorySize, autoAdjustOnNoChangeCount);
        }
    }
}
