using Microsoft.VisualBasic;
using RVPark.Data;
using System;
using System.Diagnostics;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RVPark.Services.Logging
{
    public interface IAppLogger
    {
        Task LogAsync(string level, string source, string message, Exception ex = null);
    }
}
