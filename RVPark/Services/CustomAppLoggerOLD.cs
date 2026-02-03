namespace RVPark.Services
{
    //public class CustomAppLogger
    using Microsoft.Extensions.Logging;
    using RVPark.Data;
    using System;

    public class EFAppLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ApplicationDbContext _dbContext;

        public EFAppLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public EFAppLogger(string categoryName, ApplicationDbContext dbcontext)
        {
            _categoryName = categoryName;
            _dbContext = dbcontext;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true; // filter if needed

        public void  Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            // Your custom logic: write to file, DB, SMS, etc.
            var log = new AppLog
            {
                LogLevel = logLevel.ToString(),
                Category = _categoryName,
                Message = message,
                Exception = exception?.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            if (_dbContext?.AppLogs != null)
            {
                _dbContext.AppLogs.Add(log);
                _dbContext.SaveChanges();
            }

            //Console.WriteLine($"[{DateTime.Now}] {_categoryName} [{logLevel}] {message}");
        }
    }

    public class EFAppLoggerProvider : ILoggerProvider
    {
        private readonly string _categoryName;
        private readonly ApplicationDbContext _dbContext;
        public EFAppLoggerProvider(string categoryName, ApplicationDbContext dbcontext)
        {
            _categoryName = categoryName;
            _dbContext = dbcontext;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new EFAppLogger(categoryName);
        }
        public ILogger CreateLogger(string categoryName, ApplicationDbContext applicationDbContext)
        {
            return new EFAppLogger(categoryName, applicationDbContext);
        }
        public void Dispose() { }
    }

}
