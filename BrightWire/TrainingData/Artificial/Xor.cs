﻿using BrightData;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Simple XOR training data
    /// </summary>
    public static class Xor
    {
        /// <summary>
        /// Generates a data table containing XOR training data
        /// </summary>
        /// <returns></returns>
        public static IRowOrientedDataTable Get(IBrightDataContext context)
        {
            var builder = context.BuildTable();
            builder.AddColumn(ColumnType.Float, "X");
            builder.AddColumn(ColumnType.Float, "Y");
            builder.AddColumn(ColumnType.Float, "XOR").SetTarget(true);

            builder.AddRow(0.0f, 0.0f, 0.0f);
            builder.AddRow(1.0f, 0.0f, 1.0f);
            builder.AddRow(0.0f, 1.0f, 1.0f);
            builder.AddRow(1.0f, 1.0f, 0.0f);

            return builder.BuildRowOriented();
        }
    }
}
