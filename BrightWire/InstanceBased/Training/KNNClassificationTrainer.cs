using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightWire.Models.InstanceBased;
using CommunityToolkit.HighPerformance;

namespace BrightWire.InstanceBased.Training
{
    /// <summary>
    /// K Nearest Neighbour classification trainer
    /// </summary>
    internal static class KnnClassificationTrainer
    {
        public static async Task<KNearestNeighbours> Train(IDataTable table)
        {
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var featureColumnIndices = table.ColumnIndicesOfFeatures().ToArray();
            var vectoriser = await table.GetVectoriser(true, featureColumnIndices);
            var targetColumn = table.GetColumn(targetColumnIndex).ToStringBuffer();
            var featureColumns = table.GetColumns(featureColumnIndices);

            uint offset = 0;
            var input = new float[table.RowCount][];
            await foreach (var block in vectoriser.Vectorise(featureColumns))
                Copy(block, input, ref offset);

            return new KNearestNeighbours {
                Instance = input,
                Classification = await targetColumn.ToArray(),
                DataColumns = featureColumnIndices,
                TargetColumn = targetColumnIndex
            };

            static void Copy(float[,] vectorised, float[][] input, ref uint offset)
            {
                var span = new Span2D<float>(vectorised);
                for (var i = 0; i < span.Height; i++)
                    input[offset++] = span.GetRowSpan(i).ToArray();
            }
        }
    }
}
