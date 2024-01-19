using Microsoft.EntityFrameworkCore;

namespace BrightAPI.Database
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<DataTable> DataTables { get; set; } = null!;
    }
}
