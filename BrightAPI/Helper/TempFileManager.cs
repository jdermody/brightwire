namespace BrightAPI.Helper
{
    public class TempFileManager : IDisposable
    {
        readonly List<string> _tempPaths = new();

        public TempFileManager(string? basePath = null)
        {
            BasePath = basePath ?? Path.GetTempPath();
        }

        public void Dispose()
        {
            foreach (var tempPath in _tempPaths.Where(File.Exists))
                File.Delete(tempPath);
        }

        public string BasePath { get; }

        public (string Path, Guid UniqueId) GetNewTempPath()
        {
            var id = Guid.NewGuid();
            var path = Path.Combine(BasePath, id.ToString("n"));
            _tempPaths.Add(path);
            return (path, id);
        }
    }
}
