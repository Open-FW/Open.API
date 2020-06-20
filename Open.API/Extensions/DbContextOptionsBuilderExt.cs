using System;

using Microsoft.EntityFrameworkCore;

namespace Open.API.Extensions
{
    public static class DbContextOptionsBuilderExt
    {
        public static DbContextOptionsBuilder UseProvider(
                this DbContextOptionsBuilder builder,
                string provider,
                string connectionString,
                string migrationAssembly
        )
        {
            builder = provider switch
            {
                "MSSQL" => builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationAssembly)),
                "PostgreSQL" => builder.UseNpgsql(connectionString, options => options.MigrationsAssembly(migrationAssembly)),
                "MySQL" => builder.UseMySql(connectionString, options => options.MigrationsAssembly(migrationAssembly)),
                "SQLite" => builder.UseSqlite(connectionString, options => options.MigrationsAssembly(migrationAssembly)),
                _ => throw new ArgumentException("Unknown DB provider -- valid values are PostgreSQL, MSSQL, MySQL, SQLite"),
            };
            return builder;
        }
    }
}
