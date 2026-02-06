namespace RVPark.Services.Environment
{
    public interface IEnvironmentInfoService
    {
        string EnvironmentName { get; }
        string ServerName { get; }
        string DatabaseName { get; }
        string ApplicationVersion { get; }
        bool IsLiveDatabaseEnvironment { get; }
        bool ShouldDisplayEnvInfo { get; }
        string FeatureMenuType { get; }
    }
}
