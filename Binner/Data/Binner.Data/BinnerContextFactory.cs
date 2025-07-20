using Binner.Model;
using Binner.Model.Configuration;
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
                throw new ArgumentNullException(nameof(connectionStringName));
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, AppConstants.AppSettings))
                .Build();
            var connectionString = configuration.GetConnectionString(connectionStringName);
            return connectionString;
        }
    }
}