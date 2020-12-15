using System;
using System.Collections.Generic;
using System.Text;
using BrightTable;
using BrightWire;

namespace ExampleCode.DataTableTrainers
{
    class LinearTrainer : DataTableTrainer
    {
        public LinearTrainer(IRowOrientedDataTable table) : base(table)
        {
        }

        public void TrainLinearRegression()
        {
            var lap = Table.Context.LinearAlgebraProvider;
            {
                var trainer = Table.CreateLinearRegressionTrainer(lap);
                var theta = trainer.GradientDescent(20, 0.03f, 0.1f, cost =>
                {
                    Console.WriteLine(cost);
                    return true;
                });
                Console.WriteLine(theta.Theta);
            }
        }
    }
}
