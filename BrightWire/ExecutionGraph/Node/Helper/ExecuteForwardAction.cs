﻿using System;
using System.Text;
using BrightData.Helper;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes an action when executing forward
    /// </summary>
    internal class ExecuteForwardAction(IAction action, string? name = null) : NodeBase(name), IHaveAction
    {
        public IAction Action { get; set; } = action;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            var output = Action.Execute(signal, context, source!);
            return (this, output, null);
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return (TypeLoader.GetTypeName(Action), Encoding.UTF8.GetBytes(Action.Serialise()));
        }

        protected override void Initialise(GraphFactory factory, string? description, byte[]? data)
        {
            if (description == null)
                throw new ArgumentException("Description cannot be null");

            Action = GenericActivator.CreateUninitialized<IAction>(TypeLoader.LoadType(description));
            Action.Initialise(data != null ? Encoding.UTF8.GetString(data) : "");
        }
    }
}
