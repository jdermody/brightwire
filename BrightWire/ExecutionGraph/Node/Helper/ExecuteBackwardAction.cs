﻿using System;
using System.Text;
using BrightData.Helper;
using BrightWire.Helper;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    /// <summary>
    /// Executes an action when back-propagating
    /// </summary>
    internal class ExecuteBackwardAction(IAction action, string? name = null) : NodeBase(name), IHaveAction
    {
        class Backpropagation(ExecuteBackwardAction source) : SingleBackpropagationBase<ExecuteBackwardAction>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                return _source.Action.Execute(errorSignal, context, _source);
            }
        }

        public IAction Action { get; set; } = action;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            return (this, signal, () => new Backpropagation(this));
        }

        protected override (string Description, byte[] Data) GetInfo()
        {
            return (TypeLoader.GetTypeName(Action), Encoding.UTF8.GetBytes(Action.Serialise()));
        }

        protected override void Initialise(GraphFactory factory, string? description, byte[]? data)
        {
            if (description == null)
                throw new Exception("Description cannot be null");

            Action = GenericActivator.CreateUninitialized<IAction>(TypeLoader.LoadType(description));
            Action.Initialise(data != null ? Encoding.UTF8.GetString(data) : "");
        }
    }
}
