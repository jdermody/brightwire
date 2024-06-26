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
    /// Simple recurrent neural network
    /// </summary>
    internal class SimpleRecurrent : NodeBase, IHaveMemoryNode
    {
        MemoryFeeder _memory;
        NodeBase _input, _activation, _output;
        OneToMany _start;
        uint _inputSize;

#pragma warning disable 8618
        public SimpleRecurrent(GraphFactory graph, uint inputSize, float[] memory, NodeBase activation, string? name = null)
#pragma warning restore 8618
            : base(name)
        {
            Create(graph, inputSize, memory, activation, null);
        }

        void Create(GraphFactory graph, uint inputSize, float[] memory, NodeBase activation, string? memoryId)
        {
            _inputSize = inputSize;
            _activation = activation;
            var hiddenLayerSize = (uint)memory.Length;
            _memory = new MemoryFeeder(graph.Context, memory, Name ?? Id, null, memoryId);
            _input = new FlowThrough(Name != null ? $"{Name}_start" : null);

            var inputChannel = graph.Connect(inputSize, _input)
                .AddFeedForward(hiddenLayerSize, "Wh");
            var memoryChannel = graph.Connect(hiddenLayerSize, _memory)
                .AddFeedForward(hiddenLayerSize, "Uh");

            _output = graph.Add(inputChannel, memoryChannel)
                .AddBackwardAction(new ConstrainSignal())
                .Add(activation)
                .AddForwardAction(_memory.SetMemoryAction, Name != null ? $"{Name}_last" : null)
                .LastNode!
			;
            _start = new OneToMany(SubNodes, Name != null ? $"{Name}_start" : null);
        }

        public override List<WireToNode> Output => _output.Output;
        public NodeBase Memory => _memory;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source) => _start.ForwardSingleStep(signal, channel, context, source);

        protected override (string Description, byte[] Data) GetInfo()
        {
            return ("SRN", WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.WriteTo(writer);
            Serialise(_activation, writer);

            foreach(var item in SerializedNodes)
                WriteSubNode(item, writer);
        }

        static readonly string[] SerializedNodes = ["Wh", "Uh"];

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = (uint)reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memoryData = factory.Context.LoadReadOnlyVectorAndThenGetArrayFrom(reader);
            var activation = Hydrate(factory, reader);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_memory == null)
                Create(factory, inputSize, memoryData, activation, memoryId);
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
