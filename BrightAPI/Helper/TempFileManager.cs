namespace BrightAPI.Helper
{
    public class TempFileManager(string? basePath = null) : IDisposable
    {
        readonly List<string> _tempPaths = [];

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var tempPath in _tempPaths.Where(File.Exists))
                File.Delete(tempPath);
        }

        public string BasePath { get; } = basePath ?? Path.GetTempPath();

        public (string Path, Guid UniqueId) GetNewTempPath()
        {
            var id = Guid.NewGuid();
            var path = Path.Combine(BasePath, id.ToString("n"));
            _tempPaths.Add(path);
            return (path, id);
        }
    }
}
