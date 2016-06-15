# Bright Wire

*Bright Wire* is a machine learning library for .NET with GPU support (via CUDA).

## Getting Started

*Bright Wire* runs "out of the box" for CPU based computation.  For GPU based learning, you will need to install
[NVIDIA CUDA Toolkit 7.5](https://developer.nvidia.com/cuda-toolkit) (and own a supported device!).

To enable faster CPU based computation, *Bright Wire* supports the Intel Math Kernel Library (MKL) 
via the [Numerics.Net Wrapper](http://numerics.mathdotnet.com/MKL.html).

## Features

### Connectionist aka "Deep Learning"
* Feed forward neural networks
* Recurrent, LSTM and bidirectional neural networks
* Minibatch training
* L2, Dropout and DropConnect regularisation
* RELU, LeakyRelu, Sigmoid and Tanh activation functions
* Gaussian, Xavier and Identity weight initialisation
* Cross entropy, Quadratic and RMSE cost functions
* Momentum, NesterovMomentum, Adagrad, RMSprop and Adam

## Sample Code
```
// create a template that describes each layer of the network
// in this case no regularisation, and using a sigmoid activation function 
var layerTemplate = new LayerDescriptor(0f) {
    Activation = ActivationType.Sigmoid
};

// create a (CPU based) linear algebra provider
using (var lap = new NumericsProvider()) {
	// Create some training data that the network will learn.  The XOR pattern looks like:
	// 0 0 => 0
	// 1 0 => 1
	// 0 1 => 1
	// 1 1 => 0
    var testDataProvider = new DenseTrainingDataProvider(_lap, XorData.Get());

	// create a batch trainer.  This network has a hidden layer of size 4,
	// and input and outputs of 2 and 1 respectively.
    using (var trainer = _lap.NN.CreateBatchTrainer(layerTemplate, 2, 4, 1)) {
		// create a training context that will hold the training rate and batch size
        var trainingContext = _lap.NN.CreateTrainingContext(0.03f, 2);

		// train the network!
        trainer.Train(testDataProvider, 1000, trainingContext);

		// execute the trained data 
        var results = trainer.Execute(testDataProvider);
        for (var i = 0; i < results.Count; i++) {
            var result = results[i];
            var predictedResult = Convert.ToSingle(Math.Round(result.Item1[0]));
            var expectedResult = result.Item2[0];
            FloatingPointHelper.AssertEqual(predictedResult, expectedResult);
        }
    }
}
```

## Dependencies
* [ManagedCuda](https://github.com/kunzmi/managedCuda)
* [MathNet.Numerics](https://github.com/mathnet/mathnet-numerics)
* [Protobuf-net](https://github.com/mgravell/protobuf-net)