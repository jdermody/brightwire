using System;
using System.Collections.Generic;

namespace BrightML.Learning
{
    public class Trainer<TM, TD> : ITrainer<TM, TD>
        where TM : IModel
        where TD : ITrainingData
    {
        class Context : ITrainingContext
        {
            readonly Action<ITrainer<TM, TD>, ITrainingContext> _iterate;
            private readonly Func<ITrainer<TM, TD>, ITrainingContext, float> _cost;
            private readonly ITrainer<TM, TD> _trainer;

            public float LearningRate { get; }
            public float Lambda { get; }
            public uint Iteration { get; private set; } = 0;

            public Context(
                ITrainer<TM, TD> trainer,
                Func<ITrainer<TM, TD>, ITrainingContext, float> cost,
                Action<ITrainer<TM, TD>, ITrainingContext> iterate,
                float learningRate,
                float lambda
            )
            {
                LearningRate = learningRate;
                Lambda = lambda;
                _cost = cost;
                _trainer = trainer;
                _iterate = iterate;
            }

            public void Iterate()
            {
                ++Iteration;
                _iterate(_trainer, this);
            }

            public float CalculateError() => _cost(_trainer, this);
        }

        private readonly Action<ITrainer<TM, TD>, ITrainingContext> _iterate;
        private readonly Func<ITrainer<TM, TD>, ITrainingContext, float> _cost;
        private readonly Func<ITrainer<TM, TD>, IReadOnlyList<(float Output, float Target)>> _evaluate;
        private readonly Func<TD, TM> _getData;
        public TM Model => _getData(Data);
        public TD Data { get; }

        public Trainer(
            TD data,
            Func<ITrainer<TM, TD>, ITrainingContext, float> cost,
            Action<ITrainer<TM, TD>, ITrainingContext> iterate,
            Func<ITrainer<TM, TD>, IReadOnlyList<(float Output, float Target)>> evaluate,
            Func<TD, TM> getData
        )
        {
            _cost = cost;
            _iterate = iterate;
            _evaluate = evaluate;
            _getData = getData;
            Data = data;
        }

        public void Dispose()
        {
            Data.Dispose();
        }

        public ITrainingContext CreateContext(float learningRate, float lambda = 0)
        {
            return new Context(this, _cost, _iterate, learningRate, lambda);
        }

        public IReadOnlyList<(float Output, float Target)> Evaluate() => _evaluate(this);
    }
}
