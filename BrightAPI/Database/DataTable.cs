using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BrightAPI.Database
{
    [Table(nameof(DataTable))]
    [Index(nameof(PublicId), IsUnique = true)]
    public class DataTable
    {
        [Key, Required] public int DataTableId { get; set; }
        [Required] public string PublicId { get; set; } = Guid.NewGuid().ToString("n");
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string LocalPath { get; set; } = string.Empty;
        [Required] public DateTime DateCreated { get; set; }
        [Required] public uint RowCount { get; set; }
    }
}
