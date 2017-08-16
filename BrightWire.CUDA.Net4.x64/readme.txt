
  _          _       _     _              _          
 | |__  _ __(_) __ _| |__ | |_  __      _(_)_ __ ___ 
 | '_ \| '__| |/ _` | '_ \| __| \ \ /\ / / | '__/ _ \
 | |_) | |  | | (_| | | | | |_   \ V  V /| | | |  __/
 |_.__/|_|  |_|\__, |_| |_|\__|   \_/\_/ |_|_|  \___|
               |___/                                 


Bright Wire - http://www.jackdermody.net/brightwire
Copyright (c) Jack Dermody - Open Source MIT License


This assembly provides a CUDA based linear alegbra provider so that Bright Wire can run on the GPU.

The cuda directory contains the CUDA kernel (brightwire.ptx and corresponding brightwire.cu source file) that will be run on the GPU.

DeviceMemory in /Helper maintains a cache of GPU allocated memory blocks that can have been released but not yet deallocated. New block requests can
reuse blocks that are the same size if available. This is a significant performance improvment.