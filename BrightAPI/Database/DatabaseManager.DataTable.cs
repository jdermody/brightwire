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
            await _dataContext.DataTables.AddAsync(ret);
            await _dataContext.SaveChangesAsync();
            return ret;
        }

        public Task<DataTable[]> GetAllDataTables() => _dataContext.DataTables
            .OrderByDescending(x => x.DateCreated)
            .ToArrayAsync();

        public Task<DataTable?> GetDataTable(string id) => _dataContext.DataTables
            .Where(x => x.PublicId == id)
            .SingleOrDefaultAsync();
    }
}
