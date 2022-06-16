
  _          _       _     _              _          
 | |__  _ __(_) __ _| |__ | |_  __      _(_)_ __ ___ 
 | '_ \| '__| |/ _` | '_ \| __| \ \ /\ / / | '__/ _ \
 | |_) | |  | | (_| | | | | |_   \ V  V /| | | |  __/
 |_.__/|_|  |_|\__, |_| |_|\__|   \_/\_/ |_|_|  \___|
               |___/                                 


Bright Wire - http://www.jackdermody.net/brightwire
Copyright (c) 2016-2022 Jack Dermody - Open Source MIT License


Bright Wire is an extensible machine learning library for .NET with GPU support (via CUDA).

Bright Wire uses a directed graph based system of nodes and "wires" to create learning algorithms composed of modular components.

Data flows into the graph from data tables and flows through wires into nodes.

Nodes can adjust their parameters based on the forward and backward (backpropagation) signals that flow through the graph.

Nodes can also perform actions (such as copying memory into named slots), transform the input signal, combine multiple input signals, and propagate signals to
any number of connected nodes.

It possible to combine existing predefined node types into novel graph architectures, and to extend Bright Wire with additional node types to implement new machine learning algorithms entirely.
