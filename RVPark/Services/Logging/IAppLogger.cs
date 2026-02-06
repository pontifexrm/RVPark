namespace RVPark.Services.Logging
{
    public interface IAppLogger
    {
        Task LogAsync(string level, string source, string message, Exception? ex = null);
    }
}
