using Microsoft.EntityFrameworkCore;

namespace BrightAPI.Database
{
    public partial class DatabaseManager
    {
        public async Task<DataTable> CreateDataTable(string name, Guid id, string path, uint rowCount)
        {
            var ret = new DataTable {
                Name = name,
                LocalPath = path,
                PublicId = id.ToString("n"),
                DateCreated = DateTime.UtcNow,
                RowCount = rowCount
            };
            await dataContext.DataTables.AddAsync(ret);
            await dataContext.SaveChangesAsync();
            return ret;
        }

        public Task<DataTable[]> GetAllDataTables() => dataContext.DataTables
            .OrderByDescending(x => x.DateCreated)
            .ToArrayAsync();

        public Task<DataTable?> GetDataTable(string id) => dataContext.DataTables
            .Where(x => x.PublicId == id)
            .SingleOrDefaultAsync();

        public async Task DeleteDataTable(DataTable dataTable)
        {
            dataContext.DataTables.Remove(dataTable);
            await dataContext.SaveChangesAsync();
        }
    }
}
