![image](https://user-images.githubusercontent.com/1952388/177148366-bb4f2d2f-92af-4f60-a0de-ce5e3b08f135.png)

*Bright Wire* is an extensible machine learning library for .NET with optional MKL and GPU support (via CUDA).

## Getting Started

*Bright Wire* is a .net 8 class library.

The previous .net 4.6 version can be found here: https://github.com/jdermody/brightwire-v2

*Bright Wire* runs "out of the box" with its own vectorised linear algebra library.

If you have a NVIDIA GPU then you can also use GPU based computation. You will need to install
[NVIDIA CUDA Toolkit 12](https://developer.nvidia.com/cuda-downloads) 
(and have a [Kepler or better NVIDIA GPU](https://en.wikipedia.org/wiki/CUDA#GPUs_supported)).

To enable higher performance CPU based computation on Intel hardware, *Bright Wire* also supports the Intel Math Kernel Library (MKL).

## Tutorials

* [Getting Started](https://github.com/jdermody/brightwire/wiki/0.-Getting-Started)
* [Introduction](https://github.com/jdermody/brightwire/wiki/01.-Introduction)
* [Classification Overview](https://github.com/jdermody/brightwire/wiki/02.-Classification-Overview)
* [Building a Simple Language Model](https://github.com/jdermody/brightwire/wiki/03.-Generating-Text-with-Markov-Chains)
* [Recognising Handwritten Digits (MNIST)](https://github.com/jdermody/brightwire/wiki/04.-Recognising-Handwritten-Digits-(MNIST))
* [Sentiment Analysis](https://github.com/jdermody/brightwire/wiki/05.-Sentiment-Analysis)
* [Text Clustering](https://github.com/jdermody/brightwire/wiki/06.-Text-Clustering-Four-Ways)
* [Simple Recurrent Neural Networks](https://github.com/jdermody/brightwire/wiki/07.-Teaching-a-Recurrent-Neural-Net-Binary-Addition)
* [GRU Recurrent Neural Networks](https://github.com/jdermody/brightwire/wiki/08.-GRU-Recurrent-Neural-Networks)
* [Sequence to Sequence Neural Networks with LSTM](https://github.com/jdermody/brightwire/wiki/09.-Sequence-to-Sequence-with-LSTM)
* [Convolutional Neural Networks](https://github.com/jdermody/brightwire/wiki/10.-Convolutional-Neural-Networks)

## Nuget Installation

To install the cpu version (no CUDA support) use:

```
Install-Package BrightWire
```
### MKL
To add MKL support use:

```
Install-Package BrightWire
Install-Package BrightData.MKL
```
then install the MKL.NET nuget installation for your OS, for example `Install-Package MKL.NET.win-x64`

### CUDA
To add CUDA support use:

```
Install-Package BrightWire
Install-Package BrightData.Cuda
```

## Features

### Neural Networks
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
* Hierarchical clustering
* Non Negative Matrix Factorisation
* Random Projection

### Tree Based
* Decision Trees
* Random Forest

### Ensemble Methods
* Stacking

### Other
* K Nearest Neighbour classification
* In-memory and file based data processing
