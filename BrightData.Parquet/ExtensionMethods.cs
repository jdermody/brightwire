using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Table;

namespace BrightData.Parquet
{
    public static class ExtensionMethods
    {
        public static async Task<IDataTable> LoadParquet(this BrightDataContext _, Stream stream) => await ParquetDataTable.Create(stream);
    }
}
