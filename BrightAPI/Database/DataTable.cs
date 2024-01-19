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
        [Required, MaxLength(32)] public string PublicId { get; set; } = Guid.NewGuid().ToString("n");
        [Required, MaxLength(256)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(2048)] public string LocalPath { get; set; } = string.Empty;
        [Required] public DateTime DateCreated { get; set; }
        [Required] public uint RowCount { get; set; }
    }
}
