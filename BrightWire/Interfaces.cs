using System;
using System.Collections.Generic;
using System.Text;

namespace BrightML
{
    public interface IModel : IDisposable
    {

    }

    public interface ITrainingData : IDisposable
    {
    }

    public interface ITrainer<out TM, out TD> : IDisposable
        where TM : IModel
        where TD: ITrainingData
    {
        TM Model { get; }
        TD Data { get; }

        ITrainingContext CreateContext(float learningRate, float lambda = 0f);
        IReadOnlyList<(float Output, float Target)> Evaluate();
    }

    public interface ITrainingContext
    {
        void Iterate();
        float CalculateError();

        float LearningRate { get; }
        float Lambda { get; }
        uint Iteration { get; }
    }

    public interface INode
    {
        /// <summary>
        /// Unique id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Friendly name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// List of outgoing wires
        /// </summary>
        //IReadOnlyList<Wire> Output { get; }
    }
}
