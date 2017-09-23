<img src="http://www.jackdermody.net/Content/image/bright-wire.png" alt="Bright Wire" style="max-height:144" />

Bright Wire is an extensible machine learning library for .NET with GPU support (via CUDA).

## Getting Started

Bright Wire runs "out of the box" for CPU based computation on .Net 4.6 and above.  For GPU based computation, you will need to install
[NVIDIA CUDA Toolkit 8](https://developer.nvidia.com/cuda-toolkit) 
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

To install the .net4 version (no CUDA support, any CPU) use:

```
Install-Package BrightWire.Net4
```

To add CUDA support (.net4, x64 only) use:

```
Install-Package BrightWire.CUDA.Net4.x64
```

Note: When using the CUDA version, make sure that the `/cuda/brightwire.ptx` file is copied to the output directory (Properties/Copy To Output Directory).

To install the .net standard version for use with Xamarin or UWP use:

```
Install-Package BrightWire
```

### Recompiling the PTX

It's highly likely that your GPU supports different CUDA capabilities than the precompiled `brightwire.ptx` in this repository. You can find what is your capability level [here](https://developer.nvidia.com/cuda-gpus). It's a number, ex. 3.0, 3.5, that you use for specifying `compute_XX` and `sm_XX` parameters.

If you get an `ErrorNoBinaryForGPU` exception, that means you have to recompile. The instructions are [here](https://github.com/jdermody/brightwire/blob/master/LinearAlgebra/cuda/readme.txt).

Example command for NVIDIA GeForce GTX770M (CUDA 3.0)

```
nvcc kernel.cu -use_fast_math -ptx -m 64 -arch compute_30 -code sm_30 -o kernel.ptx
```

## Linux Support

### Without CUDA

You can use Bright Wire on Mono (tested with 4.6.2/Fedora 25) out of the box, no additional setting up is needed.

### With CUDA

Bright Wire can also work with CUDA on Mono. When you build your solution, you will need to extract `ConfigForLinux.zip` archive from [here](https://github.com/kunzmi/managedCuda/releases) to your output path.
That way, CUDA won't look for `nvcuda` on Linux, but for libcuda shared object. You can even run on your Optimus enabled laptop (tested with GTX770M with Bumblebee) with `optirun mono [binary_name]`.

Another issue you may have is that `protobuf` library complains that it is already referencing `NETStandard` library. NuGet version is a bit older on Mono, so please try with the latest NuGet binary from their website. That way, all the libraries get pulled correctly.

## Features

### Connectionist aka "Deep Learning"
* Feed Forward, Convolutional and Bidirectional network architectures
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
* [ManagedCuda](https://github.com/kunzmi/managedCuda) (only required for CUDA version of BrightWire)
* [MathNet.Numerics](https://github.com/mathnet/mathnet-numerics)
* [Protobuf-net](https://github.com/mgravell/protobuf-net)
