using System.IO;
using BrightData;
using BrightWire.Helper;

namespace BrightWire.Models
{
    /// <summary>
    /// Serialises an adaptive data source - that is, a data source that takes the output from a preliminary output graph and sends it to the primary graph
    /// </summary>
    public class DataSourceModel : ISerializable
    {
        /// <summary>
        /// Segment contract version number
        /// </summary>
        public string Version { get; set; } = "3.0";

        /// <summary>
        /// The name of the data source
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The size of each input training data item
        /// </summary>
        public uint InputSize { get; set; }

        /// <summary>
        /// The size of each training item output (classification label)
        /// </summary>
        public uint OutputSize { get; set; }

        /// <summary>
        /// The preliminary graph
        /// </summary>
        public ExecutionGraphModel Graph { get; set; } = new ExecutionGraphModel();

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
