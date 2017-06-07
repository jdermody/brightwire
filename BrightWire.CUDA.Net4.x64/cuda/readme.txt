To compile the cuda kernels you will need to the CUDA toolkit open a Visual Studio command prompt to this directory and execute the following:

nvcc brightwire.cu -use_fast_math -cubin -m 64 -arch compute_35 -code sm_35 -o brightwire.cubin
nvcc brightwire.cu -use_fast_math -ptx -m 64 -arch compute_35 -code sm_35 -o brightwire.ptx

The -arch and -code flags can be modified as appropriate, as described in:

http://docs.nvidia.com/cuda/cuda-compiler-driver-nvcc/index.html