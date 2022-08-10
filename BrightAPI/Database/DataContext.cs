using Microsoft.EntityFrameworkCore;

namespace BrightAPI.Database
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<DataTable> DataTables { get; set; }
    }
}
