using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Learning
{
    public class Trainer<TM, TD> : ITrainer<TM, TD>
        where TM: IModel
        where TD: ITrainingData
    {
        class Context : ITrainingContext
        {
            readonly Func<ITrainer<TM, TD>, ITrainingContext, float> _iterate;
            private readonly ITrainer<TM, TD> _trainer;

            public float LearningRate { get; }
            public float Lambda { get; }
            public uint Iteration { get; private set; } = 0;

            public Context(ITrainer<TM, TD> trainer, Func<ITrainer<TM, TD>, ITrainingContext, float> iterate, float learningRate, float lambda)
            {
                LearningRate = learningRate;
                Lambda = lambda;
                _trainer = trainer;
                _iterate = iterate;
            }

            public float Iterate()
            {
                ++Iteration;
                return _iterate(_trainer, this);
            }
        }

        private readonly Func<ITrainer<TM, TD>, ITrainingContext, float> _iterate;
        private readonly Func<ITrainer<TM, TD>, IReadOnlyList<(float Output, float Target)>> _evaluate;
        public TM Model { get; }
        public TD Data { get; }

        public Trainer(TM model, TD data, Func<ITrainer<TM, TD>, ITrainingContext, float> iterate, Func<ITrainer<TM, TD>, IReadOnlyList<(float Output, float Target)>> evaluate)
        {
            _iterate = iterate;
            _evaluate = evaluate;
            Model = model;
            Data = data;
        }

        public void Dispose()
        {
            Model.Dispose();
            Data.Dispose();
        }

        public ITrainingContext CreateContext(float learningRate, float lambda = 0)
        {
            return new Context(this, _iterate, learningRate, lambda);
        }

        public IReadOnlyList<(float Output, float Target)> Evaluate() => _evaluate(this);
    }
}
