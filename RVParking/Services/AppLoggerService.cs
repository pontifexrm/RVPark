using RVParking.Data;

namespace RVParking.Services
{
    public interface IAppLogger
    {
        Task LogAsync(string level, string source, string message, Exception ex = null);
    }

    public class AppLogger : IAppLogger
    {
        private readonly ApplicationDbContext _db;

        public AppLogger(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string level, string source, string message, Exception ex = null)
        {
            var log = new AppLog
            {
                CreatedAt = DateTime.UtcNow,
                LogLevel = level,
                Category = source,
                Message = message,
                Exception = ex?.ToString()
            };

            _db.AppLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
