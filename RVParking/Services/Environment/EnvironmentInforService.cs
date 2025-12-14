using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using RVParking.Data;


namespace RVParking.Services.Environment
{
    public class EnvironmentInfoService : IEnvironmentInfoService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private string _cachedMenuType;

        public EnvironmentInfoService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public EnvironmentInfoService(IWebHostEnvironment env, IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _env = env;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public string EnvironmentName => _env?.EnvironmentName ?? "Unknown";

        public string ServerName => ParseServerAndDatabase(_configuration["ConnectionStrings:DefaultConnection"]).Server ?? "n/a";

        public string DatabaseName => ParseServerAndDatabase(_configuration["ConnectionStrings:DefaultConnection"]).Database ?? "n/a";

        public string FeatureMenuType
        {
            get
            {
                // get the MenuType from the database AppParameters table where ParmKey = "MenuType" // default to "" if not found
                if (_cachedMenuType == null)
                {
                    _cachedMenuType = _dbContext.AppParameters
                                        .Where(p => p.ParamKey == "MenuType")
                                        .Select(p => p.ParamValue)
                                        .FirstOrDefault() ?? "";
                }
                return _cachedMenuType;
            }
        }
        public bool IsLiveDatabaseEnvironment
        {
            get
            {
                var prodCcn = _configuration["ConnectionStrings:ProductionSrvDB"] ?? "";
                if (string.IsNullOrEmpty(prodCcn))
                    return false;

                var (prodServer, prodDb) = ParseServerAndDatabase(prodCcn);
                var (currServer, currDb) = ParseServerAndDatabase(_configuration["ConnectionStrings:DefaultConnection"] ?? "");
                return string.Equals(prodServer, currServer, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(prodDb, currDb, StringComparison.OrdinalIgnoreCase);
            }
        }

        // show env info when NOT live production DB
        public bool ShouldDisplayEnvInfo => !IsLiveDatabaseEnvironment;

        private static (string Server, string Database) ParseServerAndDatabase(string ccn)
        {
            if (string.IsNullOrEmpty(ccn))
                return (string.Empty, string.Empty);

            var server = "";
            var database = "";
            var parts = ccn.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var p = part.Trim();
                if (p.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) ||
                    p.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                {
                    var idx = p.IndexOf('='); if (idx >= 0) server = p[(idx + 1)..].Trim();
                }
                else if (p.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
                            p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                {
                    var idx = p.IndexOf('='); if (idx >= 0) database = p[(idx + 1)..].Trim();
                }
            }

            if (!string.IsNullOrEmpty(server))
            {
                if (server.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
                    server = server.Substring("tcp:".Length);

                var sepIdx = server.IndexOfAny(new[] { ',', ':' });
                if (sepIdx > 0)
                    server = server.Substring(0, sepIdx);
            }

            return (server, database);
        }

        public string ApplicationVersion
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                //var fileVersionAttr = assembly.GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>();
                //var infoVersionAttr = assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>();
                var productAttr = assembly.GetCustomAttribute<System.Reflection.AssemblyProductAttribute>();
                var productName = productAttr?.Product ?? "n/a";

                return $"Name: {productName.ToString() ?? "n/a"}"
                + $" Ver: {version?.ToString() ?? "0.0.0.0"} ";
                    //+
                    //   $"(File: {fileVersionAttr?.Version ?? "n/a"}, " +
                    //   $"Product: {infoVersionAttr?.InformationalVersion ?? "n/a"})";
            }
        }
    }
}
