
  _          _       _     _              _          
 | |__  _ __(_) __ _| |__ | |_  __      _(_)_ __ ___ 
 | '_ \| '__| |/ _` | '_ \| __| \ \ /\ / / | '__/ _ \
 | |_) | |  | | (_| | | | | |_   \ V  V /| | | |  __/
 |_.__/|_|  |_|\__, |_| |_|\__|   \_/\_/ |_|_|  \___|
               |___/                                 


Bright Wire - http://www.jackdermody.net/brightwire
Copyright (c) Jack Dermody - Open Source MIT License


Bright Wire is an extensible machine learning library for .NET with GPU support (via CUDA).

Bright Wire uses a directed graph based system of nodes and "wires" to create learning algorithms composed of modular components.

Data flows into the graph from data tables and flows down wires. Nodes alter and reroute the data to each of their connected node(s).

Nodes adjust their state (and future behaviour) based on the backpropagation of error signals that flow backwards up the graph.

Nodes can perform actions (such as copying memory into named slots), transform their input signal, combine multiple input signals, and propagate signals to
any number of connected nodes.

While this might all sound intimidatingly complex, in practice the overall modular design leads to a simpler machine learning experience 
as Bright Wire comes with a large number of prebuilt nodes that can be snapped together like LEGO bricks.

It is also easy to extend Bright Wire with additional nodes to tweak existing behaviour or implement new machine learning algorithms entirely.