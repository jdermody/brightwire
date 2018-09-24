
  _          _       _     _              _          
 | |__  _ __(_) __ _| |__ | |_  __      _(_)_ __ ___ 
 | '_ \| '__| |/ _` | '_ \| __| \ \ /\ / / | '__/ _ \
 | |_) | |  | | (_| | | | | |_   \ V  V /| | | |  __/
 |_.__/|_|  |_|\__, |_| |_|\__|   \_/\_/ |_|_|  \___|
               |___/                                 


Bright Wire - http://www.jackdermody.net/brightwire
Copyright (c) 2016-2018 Jack Dermody - Open Source MIT License

To compile the cuda kernels you will need to have installed the CUDA toolkit for a supported
version of visual studio. https://developer.nvidia.com/cuda-toolkit

The latest version of the cuda toolkit may not work with the latest updates to visual studio, so
using the previous version of visual studio might be a better option.

IMPORTANT: You will need to have installed a c++ compiler as part of your Visual Studio installation.

Open a Visual Studio command prompt to this directory (with the brightwire.cu file) and execute the following:


nvcc brightwire.cu -use_fast_math -ptx -m 64 -arch compute_35 -code sm_35 -o brightwire.ptx


NOTE: The -arch and -code flags can be modified as appropriate for your GPU, as described in:

http://docs.nvidia.com/cuda/cuda-compiler-driver-nvcc/index.html