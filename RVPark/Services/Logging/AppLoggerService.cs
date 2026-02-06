using RVPark.Data;

namespace RVPark.Services.Logging
{

    //### 🗂 Field-by-field explanation

    //- **`AppLogId`**
    //  - Primary key, unique identifier for each log entry.
    //  - Usually an auto-incrementing integer or GUID.
    //  - Example: `12345`

    //- **`LogLevel`**
    //  - Severity of the log message.
    //  - Common values: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`.
    //  - Example: `Error` (when a database connection fails).

    //- **`Category`**
    //  - Logical grouping of the log source.
    //  - Often the class name, subsystem, or feature area.
    //  - Example: `BookingController` or `System.Security.Authentication`.

    //- **`Message`**
    //  - Human-readable description of the event.
    //  - Should explain what happened in plain text.
    //  - Example: `"Guest booking request failed due to invalid date range."`

    //- **`Exception`**
    //  - Detailed error information if an exception was thrown.
    //  - Typically includes stack trace, exception type, and message.
    //  - Example:  
    //    ```
    //    System.NullReferenceException: Object reference not set to an instance of an object
    //       at RVPark.Services.BookingService.CreateBooking(...)
    //    ```

    //-**`CreatedAt`**
    //  -Timestamp when the log entry was created.
    //  -Stored in UTC or local time depending on your design(for NZ, you’ll want consistent handling with DST).
    //  - Example: `2025 - 12 - 07 14:05:23`

    //---

    //### ✅ How this helps in practice
    //-**Filtering * *: You can query by `LogLevel` to quickly find errors or warnings.
    //-**Diagnostics * *: `Category` + `Message` help pinpoint which part of the app misbehaved.
    //-**Root cause analysis**: `Exception` gives the stack trace for debugging.
    //- **Auditing * *: `CreatedAt` lets you correlate logs with user actions or system events.

    //-- -

    //Since you’re running this in production for your RV park, you’ll want to:
    //-Ensure * *indexing * *on `CreatedAt` and `LogLevel` for fast queries.
    //-Consider * *log rotation / archiving * *so the table doesn’t grow indefinitely.
    //- Normalize * *time zones * *(store UTC, convert to NZDT in the UI).

    //Would you like me to sketch out a** sample log entry row** showing realistic values for your RV park app ? That way you can visualize how the data flows into this table.    /// <summary>

    //public interface IAppLogger
    //{
    //    Task LogAsync(string level, string source, string message, Exception ex = null);
    //}

    public class AppLogger : IAppLogger
    {
        private readonly ApplicationDbContext _db;

        public AppLogger(ApplicationDbContext db)
        {
            _db = db;
        }
        /// <summary>
        /// Asynchronously writes a log entry to the application log store with the specified level, source, message,
        /// and optional exception details.
        /// </summary>
        /// <remarks>This method adds a new log entry to the application's persistent log store and saves
        /// the changes asynchronously. The log entry includes the current UTC timestamp. This method is thread-safe if
        /// the underlying database context is used in a thread-safe manner.</remarks>
        /// <param name="LogLevel">The severity level of the log entry. 
        ///                  Common values include "Information", "Warning", and "Error". Cannot be null or empty.</param>
        /// <param name="Category">The category or source of the log entry, 
        ///                  typically indicating the component or class where the log originated. Cannot be null or empty.</param>
        /// <param name="Message">The message describing the event or error to log. Cannot be null or empty.</param>
        /// <param name="Exception">An optional exception associated with the log entry. 
        ///                  If provided, exception details are included in the log; otherwise, this parameter can be null.</param>
        /// <returns>A task that represents the asynchronous logging operation.</returns>
        public async Task LogAsync(string level, string source, string message, Exception? ex = null)
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
