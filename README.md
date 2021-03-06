<img src="http://www.jackdermody.net/image/bright-wire.png" alt="Bright Wire" style="max-height:144" />

Bright Wire is an extensible machine learning library for .NET with GPU support (via CUDA).

## Getting Started

Bright Wire is a .net core 3.1 class library.

The previous .net 4.6 version can be found here: https://github.com/jdermody/brightwire-v2

Bright Wire runs "out of the box" for CPU based computation. For GPU based computation, you will need to install
[NVIDIA CUDA Toolkit 11](https://developer.nvidia.com/cuda-downloads) 
(and have a [Kepler or better NVIDIA GPU](https://en.wikipedia.org/wiki/CUDA#GPUs_supported)).

To enable higher performance CPU based computation, Bright Wire also supports the Intel Math Kernel Library (MKL) 
via the [Numerics.Net Wrapper](http://numerics.mathdotnet.com/MKL.html).

## Tutorials

* [Getting Started](http://www.jackdermody.net/brightwire/article/Introduction_to_Bright_Wire)
* [Classification Overview](http://www.jackdermody.net/brightwire/article/Classification_Overview_with_Bright_Wire)
* [Building a Language Model](http://www.jackdermody.net/brightwire/article/Generating_Text_with_Markov_Chains)
* [Recognising Handwritten Digits (MNIST)](http://www.jackdermody.net/brightwire/article/Recognising_Handwritten_Digits_(MNIST))
* [Sentiment Analysis](http://www.jackdermody.net/brightwire/article/Sentiment_Analysis)
* [Text Clustering](http://www.jackdermody.net/brightwire/article/Text_Clustering_Four_Ways)
* [Simple Recurrent Neural Networks](http://www.jackdermody.net/brightwire/article/Teaching_a_Recurrent_Neural_Net_Binary_Addition)
* [GRU Recurrent Neural Networks](http://www.jackdermody.net/brightwire/article/GRU_Recurrent_Neural_Networks)
* [Sequence to Sequence Neural Networks with LSTM](http://www.jackdermody.net/brightwire/article/Sequence_to_Sequence_with_LSTM)
* [Convolutional Neural Networks](http://www.jackdermody.net/brightwire/article/Convolutional_Neural_Networks)

## Nuget Installation

To install the cpu version (no CUDA support) use:

```
Install-Package BrightWire
Install-Package BrightWire.Numerics
```

To add CUDA support use:

```
Install-Package BrightWire
Install-Package BrightWire.Cuda
```

## Features

### Connectionist aka "Deep Learning"
* Feed Forward, Convolutional, Bidirectional and Sequence to Sequence (seq2seq) network architectures
* LSTM, GRU, Simple, Elman and Jordan recurrent neural networks
* L2, Dropout and DropConnect regularisation
* Relu, LeakyRelu, Sigmoid, Tanh and SoftMax activation functions
* Gaussian, Xavier and Identity weight initialisation
* Cross Entropy, Quadratic and Binary cost functions
* Momentum, NesterovMomentum, Adagrad, RMSprop and Adam gradient descent optimisations

### Bayesian
* Naive Bayes
* Multinomial Bayes
* Multivariate Bernoulli
* Markov Models

### Unsupervised
* K Means clustering
* Hierachical clustering
* Non Negative Matrix Factorisation
* Random Projection

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
* K Nearest Neighbour classification
* In-memory and file based data processing

## Dependencies
* [ManagedCuda](https://github.com/kunzmi/managedCuda) (for CUDA version of BrightWire)
* [MathNet.Numerics](https://github.com/mathnet/mathnet-numerics)
