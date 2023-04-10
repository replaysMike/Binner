using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Binner.Data
{
    public class BinnerContextFactory : IDbContextFactory<BinnerContext>
    {
        public BinnerContext CreateDbContext()
        {
            return CreateDbContext(nameof(BinnerContext));
        }

        public BinnerContext CreateDbContext(string connectionStringName)
        {
            var connectionString = LoadConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception($"Error: Connection string named '{connectionStringName}' is empty.");
            
            var builder = new DbContextOptionsBuilder<BinnerContext>();
            builder.UseSqlServer(connectionString);

            return new BinnerContext(builder.Options);
        }

        internal static string LoadConnectionString(string connectionStringName)
        {
            if (string.IsNullOrEmpty(connectionStringName))
                throw new Exception($"No {nameof(connectionStringName)} was provided.");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString(connectionStringName);
            return connectionString;
        }
    }
}