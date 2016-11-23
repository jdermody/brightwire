<img src="http://www.jackdermody.net/Content/image/bright-wire.png" alt="Bright Wire" style="max-height:144" />

Bright Wire is a machine learning library for .NET with GPU support (via CUDA).

## Getting Started

Bright Wire runs "out of the box" for CPU based computation on .Net 4.6 and above.  For GPU based computation, you will need to install
[NVIDIA CUDA Toolkit 7.5](https://developer.nvidia.com/cuda-toolkit) 
(and have a [Kepler or better NVIDIA GPU](https://en.wikipedia.org/wiki/CUDA#GPUs_supported)).

To enable higher performance CPU based computation, Bright Wire also supports the Intel Math Kernel Library (MKL) 
via the [Numerics.Net Wrapper](http://numerics.mathdotnet.com/MKL.html).

## Geting Started Tutorials

* [Iris Classification](http://www.jackdermody.net/brightwire/article/Introduction_to_Bright_Wire)
* [Building a Language Model](http://jackdermody.net/brightwire/article/Generating_Text_with_Markov_Chains)
* [Recognising Handwritten Digits (MNIST)](http://jackdermody.net/brightwire/article/Recognising_Handwritten_Digits_(MNIST))
* [Sentiment Analysis](http://jackdermody.net/brightwire/article/Sentiment_Analysis)
* [Text Clustering](http://www.jackdermody.net/brightwire/article/Text_Clustering_Four_Ways)
* [Recurrent Neural Networks](http://www.jackdermody.net/brightwire/article/Teaching_a_Recurrent_Neural_Net_Binary_Addition)

## Nuget Installation

To install the standard version (no CUDA support, any CPU) use:

```
Install-Package BrightWire.Net4
```

To add CUDA support (x64 only) use:

```
Install-Package BrightWire.CUDA.Net4.x64
```

Note: When using the CUDA version, make sure that the /LinearAlgebra/cuda/kernel.ptx file is copied to the output directory (Properties/Copy To Output Directory).

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
* Hierachical Clustering
* Non Negative Matrix Factorisation

### Linear
* Regression
* Logistic Regression
* Multinomial Logistic Regression

### Tree Based
* Decision Trees
* Random Forest

### Ensemble Methods
* Stacking

### Other
* Random Projections
* K Nearest Neighbour Classification
* In-memory and file based data processing

## Dependencies
* [ManagedCuda](https://github.com/kunzmi/managedCuda) (optional)
* [MathNet.Numerics](https://github.com/mathnet/mathnet-numerics)
* [Protobuf-net](https://github.com/mgravell/protobuf-net)
