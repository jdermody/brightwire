﻿using System;
using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData;
using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Elman or Jordan style recurrent neural network
    /// https://en.wikipedia.org/wiki/Recurrent_neural_network#Elman_networks_and_Jordan_networks
    /// </summary>
    internal class ElmanJordan : NodeBase, IHaveMemoryNode
    {
        MemoryFeeder _memory;
        NodeBase _input, _activation, _activation2, _output;
        OneToMany _start;
        uint _inputSize;
        bool _isElman;

#pragma warning disable 8618
        public ElmanJordan(GraphFactory graph, bool isElman, uint inputSize, float[] memory, NodeBase activation, NodeBase activation2, string? name = null)
#pragma warning restore 8618
            : base(name)
        {
            Create(graph, isElman, inputSize, memory, activation, activation2, null);
        }

        void Create(GraphFactory graph, bool isElman, uint inputSize, float[] memory, NodeBase activation, NodeBase activation2, string? memoryName)
        {
            _isElman = isElman;
            _inputSize = inputSize;
            _activation = activation;
            _activation2 = activation2;
            var hiddenLayerSize = (uint)memory.Length;
            _memory = new MemoryFeeder(graph.Context, memory, Name ?? Id, null, memoryName);
            _input = new FlowThrough();

            var inputChannel = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var memoryChannel = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uh");

            var h = graph.Add(inputChannel, memoryChannel)
                .AddBackwardAction(new ConstrainSignal())
                .Add(activation)
            ;
            if (isElman)
                h = h.AddForwardAction(_memory.SetMemoryAction);

            h = h.AddFeedForward(hiddenLayerSize, "Wy")
                .AddBackwardAction(new ConstrainSignal())
                .Add(activation2)
            ;
            if (!isElman)
                h.AddForwardAction(_memory.SetMemoryAction);

            _output = h.LastNode!;
            _start = new OneToMany(SubNodes, Name != null ? $"{Name}_start" : null);
        }

        public override List<WireToNode> Output => _output.Output;
        public NodeBase Memory => _memory;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source) => _start.ForwardSingleStep(signal, channel, context, source);

        protected override (string Description, byte[] Data) GetInfo()
        {
            return (_isElman ? "ERN" : "JRN", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_isElman);
            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.WriteTo(writer);
            Serialise(_activation, writer);
            Serialise(_activation2, writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = ["Wh", "Wy", "Uh"];

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var isElman = reader.ReadBoolean();
            var inputSize = (uint)reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memoryData = factory.Context.LoadReadOnlyVectorAndThenGetArrayFrom(reader);
            var activation = Hydrate(factory, reader);
            var activation2 = Hydrate(factory, reader);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_memory == null)
                Create(factory, isElman, inputSize, memoryData, activation, activation2, memoryId);
            else
                _memory.Data = memoryData;

            foreach(var item in SerializedNodes)
                ReadSubNode(item, factory, reader);
        }

        public override IEnumerable<NodeBase> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memory;
            }
        }
    }
}
