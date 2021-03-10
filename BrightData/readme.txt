  _          _       _     _         _       _        
 | |__  _ __(_) __ _| |__ | |_    __| | __ _| |_ __ _ 
 | '_ \| '__| |/ _` | '_ \| __|  / _` |/ _` | __/ _` |
 | |_) | |  | | (_| | | | | |_  | (_| | (_| | || (_| |
 |_.__/|_|  |_|\__, |_| |_|\__|  \__,_|\__,_|\__\__,_|
               |___/                                  


Bright Wire - http://www.jackdermody.net/brightwire
Copyright (c) 2016-2021 Jack Dermody - Open Source MIT License


Bright Data is a data table, data processing and data analysis library for .NET.

Data tables are aligned for either row or column based access and can be stored either in memory or on disk.

Row based data tables can be used as the input to Bright Wire's machine learning graph.

Certian data table transformations/analysis are more efficient with column based data tables.

Data tables can contain tensors (including vectors and matrices), binary data, strings, sparse vectors and numbers.

The current tensor implementation has not been fully implemented but can be made complete by adding tensor computation via either:

BrightData.Numerics (CPU - including optional MKL support)
BrightData.Cuda (GPU - via CUDA)
