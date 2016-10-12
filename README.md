# Bright Wire

Bright Wire is a machine learning library for .NET with GPU support (via CUDA).

## Getting Started

Bright Wire runs "out of the box" for CPU based computation on .Net 4.6 and above.  For GPU based computation, you will need to install
[NVIDIA CUDA Toolkit 7.5](https://developer.nvidia.com/cuda-toolkit) (and have a supported NVIDIA GPU!).

To enable higher performance CPU based computation, Bright Wire also supports the Intel Math Kernel Library (MKL) 
via the [Numerics.Net Wrapper](http://numerics.mathdotnet.com/MKL.html).

## Nuget Installation

To install the standard version (no CUDA support, any CPU) use:

```
Install-Package BrightWire.Net4
```

To install the CUDA version (x64 only) use:

```
Install-Package BrightWire.CUDA.Net4.x64
```
The CUDA version is a superset of the standard version.

## Features

### Connectionist aka "Deep Learning"
* Feed Forward, Recurrent and Bidirectional Neural Networks
* Minibatch Training
* L2, Dropout and DropConnect Regularisation
* RELU, LeakyRelu, Sigmoid and Tanh Activation Functions
* Gaussian, Xavier and Identity Weight Initialisation
* Cross Entropy, Quadratic and RMSE Cost Functions
* Momentum, NesterovMomentum, Adagrad, RMSprop and Adam Gradient Descent Optimisations

### Bayesian
* Markov Models
* Naive Bayes
* Multinomial Bayes
* Multivariate Bernoulli

### Unsupervised
* K Means
* Non Negative Matrix Factorisation
* Random Projection

### Linear
* Regression
* Logistic Regression

### Tree Based
* Decision Trees
* Random Forest

### Other Features
* In Memory and File Based Data Processing
* Random Projections

## Sample Code
```
// create a template that describes each layer of the network
// in this case no regularisation, and using a sigmoid activation function 
var layerTemplate = new LayerDescriptor(0f) {
    Activation = ActivationType.Sigmoid
};

// Create some training data that the network will learn.  The XOR pattern looks like:
// 0 0 => 0
// 1 0 => 1
// 0 1 => 1
// 1 1 => 0
using(var lap = LinearAlgebraProvider.CreateCPU()) {
	var testDataProvider = lap.NN.CreateTrainingDataProvider(XorData.Get());

	// create a batch trainer (hidden layer of size 4).
	using (var trainer = lap.NN.CreateBatchTrainer(layerTemplate, testDataProvider.InputSize, 4, testDataProvider.OutputSize)) {
		// create a training context that will hold the training rate and batch size
		var trainingContext = lap.NN.CreateTrainingContext(0.03f, 2, ErrorMetricType.OneHot);

		// train the network!
		trainer.Train(testDataProvider, 1000, trainingContext);

		// execute the network to get the predictions
		var trainingResults = trainer.Execute(testDataProvider);
		for (var i = 0; i < trainingResults.Count; i++) {
			var result = trainingResults[i];
			var predictedResult = Convert.ToSingle(Math.Round(result.Output[0]));
			var expectedResult = result.ExpectedOutput[0];
			FloatingPointHelper.AssertEqual(predictedResult, expectedResult);
		}

		// serialise the network parameters and data
		var networkData = trainer.NetworkInfo;

		// create a new network to execute the learned network
		var network = lap.NN.CreateFeedForward(networkData);
		var results = XorData.Get().Select(d => Tuple.Create(network.Execute(d.Item1), d.Item2)).ToList();
		for (var i = 0; i < results.Count; i++) {
			var result = results[i].Item1.AsIndexable();
			var predictedResult = Convert.ToSingle(Math.Round(result[0]));
			var expectedResult = results[i].Item2[0];
			FloatingPointHelper.AssertEqual(predictedResult, expectedResult);
		}
	}
}
```

## Dependencies
* [ManagedCuda](https://github.com/kunzmi/managedCuda) (optional)
* [MathNet.Numerics](https://github.com/mathnet/mathnet-numerics)
* [Protobuf-net](https://github.com/mgravell/protobuf-net)
