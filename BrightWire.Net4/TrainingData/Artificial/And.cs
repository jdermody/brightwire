using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Simple AND training data
    /// </summary>
    public static class And
    {
        /// <summary>
        /// Generates a data table containing AND training data
        /// </summary>
        /// <returns></returns>
        public static IDataTable Get()
        {
            var builder = new DataTableBuilder();
            builder.AddColumn(ColumnType.Float, "X");
            builder.AddColumn(ColumnType.Float, "Y");
            builder.AddColumn(ColumnType.Float, "AND", true);

            builder.Add(0.0f, 0.0f, 0.0f);
            builder.Add(1.0f, 0.0f, 0.0f);
            builder.Add(0.0f, 1.0f, 0.0f);
            builder.Add(1.0f, 1.0f, 1.0f);

            return builder.Build();
        }
    }
}
