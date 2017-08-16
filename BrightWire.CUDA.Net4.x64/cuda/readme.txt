
  _          _       _     _              _          
 | |__  _ __(_) __ _| |__ | |_  __      _(_)_ __ ___ 
 | '_ \| '__| |/ _` | '_ \| __| \ \ /\ / / | '__/ _ \
 | |_) | |  | | (_| | | | | |_   \ V  V /| | | |  __/
 |_.__/|_|  |_|\__, |_| |_|\__|   \_/\_/ |_|_|  \___|
               |___/                                 


Bright Wire - http://www.jackdermody.net/brightwire
Copyright (c) Jack Dermody - Open Source MIT License

To compile the cuda kernels you will need to the CUDA toolkit open a Visual Studio command prompt to this directory and execute the following:

nvcc brightwire.cu -use_fast_math -cubin -m 64 -arch compute_35 -code sm_35 -o brightwire.cubin
nvcc brightwire.cu -use_fast_math -ptx -m 64 -arch compute_35 -code sm_35 -o brightwire.ptx

The -arch and -code flags can be modified as appropriate, as described in:

http://docs.nvidia.com/cuda/cuda-compiler-driver-nvcc/index.html