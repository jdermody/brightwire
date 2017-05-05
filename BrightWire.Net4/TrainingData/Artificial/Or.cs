using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Simple OR training data
    /// </summary>
    public static class Or
    {
        /// <summary>
        /// Generates a data table containing OR training data
        /// </summary>
        /// <returns></returns>
        public static IDataTable Get()
        {
            var builder = new DataTableBuilder();
            builder.AddColumn(ColumnType.Float, "X");
            builder.AddColumn(ColumnType.Float, "Y");
            builder.AddColumn(ColumnType.Float, "OR", true);

            builder.Add(0.0f, 0.0f, 0.0f);
            builder.Add(1.0f, 0.0f, 1.0f);
            builder.Add(0.0f, 1.0f, 1.0f);
            builder.Add(1.0f, 1.0f, 1.0f);

            return builder.Build();
        }
    }
}
