using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;

namespace RVPark.Services
{
    /// <summary>
    /// Result of a transfer operation between databases.
    /// </summary>
    public class TransferResult
    {
        public int AppLogsAdded { get; set; }
        public int UsersAdded { get; set; }
        public int LoginLogsAdded { get; set; }
        public int VisitLogsAdded { get; set; }
    }

    public class SiteStatsService
    {
        private readonly ApplicationDbContext _dbContext;

        public SiteStatsService(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        // NOTE: TransferAsync is currently disabled as it requires separate source and local database contexts.
        // To enable, uncomment and provide proper SourceDbContext and LocalDbContext implementations.
        /*
        public async Task<TransferResult> TransferAsync()
        {
            // This method requires _source and _local DbContext instances
            // which are not currently configured.
            throw new NotImplementedException("TransferAsync requires separate source and local database contexts.");
        }
        */

        private static void ResetPrimaryKeyIfStoreGenerated<TEntity>(TEntity entity, DbContext targetContext)
            where TEntity : class
        {
            if (entity is null || targetContext is null) return;

            var entityType = targetContext.Model.FindEntityType(typeof(TEntity));
            var pkProperty = entityType?.FindPrimaryKey()?.Properties?.FirstOrDefault();
            if (pkProperty == null) return;

            if (pkProperty.ValueGenerated == Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd)
            {
                var propInfo = typeof(TEntity).GetProperty(pkProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propInfo == null || !propInfo.CanWrite) return;

                object defaultValue = propInfo.PropertyType.IsValueType
                    ? Activator.CreateInstance(propInfo.PropertyType)
                    : null;

                propInfo.SetValue(entity, defaultValue);
            }
        }

        private static bool IsDuplicateLogin(LoginLog sourceRec, List<LoginLog> localCache)
        {
            if (sourceRec == null) return true;

            var sourceType = sourceRec.GetType();
            var appUserProp = sourceType.GetProperty("AppUserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                              ?? sourceType.GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var timeProp = sourceType.GetProperty("Timestamp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? sourceType.GetProperty("LoginTime", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? sourceType.GetProperty("Created", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? sourceType.GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (appUserProp != null && timeProp != null)
            {
                var aVal = appUserProp.GetValue(sourceRec);
                var tVal = timeProp.GetValue(sourceRec);

                return localCache.Any(local =>
                {
                    var laVal = appUserProp.GetValue(local);
                    var ltVal = timeProp.GetValue(local);
                    return Equals(laVal, aVal) && Equals(ltVal, tVal);
                });
            }

            // Fallback: compare scalar properties except primary key
            var scalarProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType)).ToArray();

            if (!scalarProps.Any()) return true;

            return localCache.Any(local =>
            {
                foreach (var p in scalarProps)
                {
                    if (IsPrimaryKeyLike(p)) continue;

                    var sv = p.GetValue(sourceRec);
                    var lv = p.GetValue(local);
                    if (!Equals(sv, lv)) return false;
                }
                return true;
            });
        }

        private static bool IsDuplicateRecord<T>(T sourceRec, List<T> localCache)
            where T : class
        {
            if (sourceRec == null) return true;

            var sourceType = sourceRec.GetType();
            var timeProp = sourceType.GetProperty("Timestamp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? sourceType.GetProperty("Created", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? sourceType.GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var keyProp = sourceType.GetProperty("AppUserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                          ?? sourceType.GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                          ?? sourceType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (keyProp != null && timeProp != null)
            {
                var aVal = keyProp.GetValue(sourceRec);
                var tVal = timeProp.GetValue(sourceRec);

                return localCache.Any(local =>
                {
                    var laVal = keyProp.GetValue(local);
                    var ltVal = timeProp.GetValue(local);
                    return Equals(laVal, aVal) && Equals(ltVal, tVal);
                });
            }

            // Fallback to scalar comparison (excluding PK-like properties)
            var scalarProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType)).ToArray();

            if (!scalarProps.Any()) return true;

            return localCache.Any(local =>
            {
                foreach (var p in scalarProps)
                {
                    if (IsPrimaryKeyLike(p)) continue;

                    var sv = p.GetValue(sourceRec);
                    var lv = p.GetValue(local);
                    if (!Equals(sv, lv)) return false;
                }
                return true;
            });
        }

        private static bool IsPrimaryKeyLike(PropertyInfo p)
        {
            var name = p.Name.ToLowerInvariant();
            return name == "id" || name.EndsWith("id");
        }

        private static bool IsSimpleType(Type t)
        {
            if (t.IsPrimitive) return true;
            if (t == typeof(string)) return true;
            if (t == typeof(decimal)) return true;
            if (t == typeof(DateTime)) return true;
            if (t == typeof(DateTimeOffset)) return true;
            if (t == typeof(Guid)) return true;
            if (t == typeof(TimeSpan)) return true;
            var underlyingType = Nullable.GetUnderlyingType(t);
            if (underlyingType != null)
                return IsSimpleType(underlyingType);
            return false;
        }
    }
}
