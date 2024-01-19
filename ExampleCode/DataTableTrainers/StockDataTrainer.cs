using BrightData;
using BrightWire;

namespace ExampleCode.DataTableTrainers
{
    internal class StockDataTrainer(IDataTable table) : DataTableTrainer(table)
    {
        public SequentialWindowStockDataTrainer GetSequentialWindow(uint windowSize = 14)
        {
            return new(
                Table.Value.CreateSequentialWindow(windowSize, 0, 1, 2, 3, 4).Result
            );
        }
    }
}
