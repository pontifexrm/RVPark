using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;

namespace RVPark.Services
{
    public class SiteStatsService
    {
        //private readonly SourceDbContext _source;
        //private readonly LocalDbContext _local;
        private readonly ApplicationDbContext dbcontext;

        //public SiteStatsService(SourceDbContext source, LocalDbContext local)
        //{
        //    _source = source;
        //    _local = local;
        //}
        public SiteStatsService(ApplicationDbContext context)
        {
            dbcontext = context;
        }

        public async Task<TransferResult> TransferAsync()
        {
            var result = new TransferResult();

            // Transfer AppLogs
            var applog = await _source.AppLogs.AsNoTracking().ToListAsync<AppLog>();

            if (applog.Any())
            {
                // Load local app logs once for duplicate checks
                var localAppLogCache = await _local.AppLogs.AsNoTracking().ToListAsync();

                foreach (var rec in applog)
                {
                    if (IsDuplicateRecord(rec, localAppLogCache))
                        continue;

                    ResetPrimaryKeyIfStoreGenerated(rec, _local);

                    _local.AppLogs.Add(rec);
                    result.AppLogsAdded++;
                }

                await _local.SaveChangesAsync();

                // no tracked changes on _source because of AsNoTracking()
                await _source.SaveChangesAsync();
            }
            // Transfer Bkg_Users BUT DO NOT DELETE THEM FROM THE SOURCE just compare and add any new ones
            var userRecords = await _source.Bkg_Users.AsNoTracking().ToListAsync<Bkg_User>();
            if (userRecords.Any())
            {
                foreach (var user in userRecords)
                {
                    // Unique by combination of AppUserId + UserName
                    var exists = await _local.Bkg_Users.AnyAsync(u =>
                        u.AppUserId == user.AppUserId &&
                        u.UserName == user.UserName);

                    if (!exists)
                    {
                        user.UserId = 0;
                        _local.Bkg_Users.Add(user);
                        result.UsersAdded++;
                    }
                }
                await _local.SaveChangesAsync();

            }
            // Transfer LoginLogs
            var loginRecords = await _source.LoginLogs.AsNoTracking().ToListAsync<LoginLog>();
            if (loginRecords.Any())
            {
                // Load local login logs once for duplicate checks
                var localLoginCache = await _local.LoginLogs.AsNoTracking().ToListAsync();

                foreach (var rec in loginRecords)
                {
                    if (IsDuplicateLogin(rec, localLoginCache))
                        continue;

                    ResetPrimaryKeyIfStoreGenerated(rec, _local);

                    _local.LoginLogs.Add(rec);
                    result.LoginLogsAdded++;
                }

                await _local.SaveChangesAsync();
                await _source.SaveChangesAsync();
            }
            // Transfer VisitLogs
            var visitRecords = await _source.VisitLogs.AsNoTracking().ToListAsync<VisitLog>();
            if (visitRecords.Any())
            {
                // Load local visit logs once for duplicate checks
                var localVisitCache = await _local.VisitLogs.AsNoTracking().ToListAsync();

                foreach (var rec in visitRecords)
                {
                    if (IsDuplicateRecord(rec, localVisitCache))
                        continue;

                    ResetPrimaryKeyIfStoreGenerated(rec, _local);

                    _local.VisitLogs.Add(rec);
                    result.VisitLogsAdded++;
                }

                await _local.SaveChangesAsync();
                await _source.SaveChangesAsync();

            }

            return result;
        }

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
            if (Nullable.GetUnderlyingType(t) != null)
                return IsSimpleType(Nullable.GetUnderlyingType(t));
            return false;
        }
    }
}
