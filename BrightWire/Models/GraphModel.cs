using System.IO;
using BrightData;
using BrightWire.Helper;

namespace BrightWire.Models
{
    /// <summary>
    /// A serialised graph
    /// </summary>
    public class GraphModel : ISerializable
    {
        /// <summary>
        /// Segment contract version number
        /// </summary>
        public string Version { get; set; } = "3.0";

        /// <summary>
        /// The name of the graph
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The primary execution graph
        /// </summary>
        public ExecutionGraphModel Graph { get; set; } = new ExecutionGraphModel();

        /// <summary>
        /// Optional data source associated with the model
        /// </summary>
        public DataSourceModel? DataSource { get; set; }

        /// <inheritdoc />
        public void WriteTo(BinaryWriter writer) => ModelSerialisation.WriteTo(this, writer);

        /// <inheritdoc />
        public void Initialize(IBrightDataContext context, BinaryReader reader) => ModelSerialisation.ReadFrom(context, reader, this);
    }
}
