namespace BrightAPI.Database
{
    public partial class DatabaseManager
    {
        readonly DataContext _dataContext;

        public DatabaseManager(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        
    }
}
