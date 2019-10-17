using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Binner.Common.Models;
using Microsoft.Data.SqlClient;
using TypeSupport;
using TypeSupport.Extensions;

namespace Binner.Common.StorageProviders
{
    /// <summary>
    /// A storage provider for Sql Server
    /// </summary>
    public class SqlServerStorageProvider : IStorageProvider
    {
        public const string ProviderName = "SqlServer";

        private readonly SqlServerStorageConfiguration _config;
        private bool _isDisposed;

        public SqlServerStorageProvider(IDictionary<string, string> config)
        {
            _config = new SqlServerStorageConfiguration(config);
            Task.Run(async () => await GenerateDatabaseIfNotExistsAsync<IBinnerDb>()).GetAwaiter().GetResult();
        }

        public async Task<Part> AddPartAsync(Part part)
        {
            var query =
$@"INSERT INTO Parts (Quantity, LowStockThreshold, PartNumber, DigiKeyPartNumber, Description, PartTypeId, ProjectId, Keywords, DatasheetUrl, Project, Location, BinNumber, BinNumber2, UserId) 
output INSERTED.PartId 
VALUES(@Quantity, @LowStockThreshold, @PartNumber, @DigiKeyPartNumber, @Description, @PartTypeId, @ProjectId, @Keywords, @DatasheetUrl, @Project, @Location, @BinNumber, @BinNumber2, @UserId);
";
            return await InsertAsync<Part, long>(query, part, (x, key) => { x.PartId = key; });
        }

        public async Task<Project> AddProjectAsync(Project project)
        {
            var query =
            $@"INSERT INTO Projects (Name, Description, DateCreatedUtc, UserId) 
output INSERTED.ProjectId 
VALUES(@Name, @Description, @DateCreatedUtc, @UserId);
";
            return await InsertAsync<Project, long>(query, project, (x, key) => { x.ProjectId = key; });
        }

        public async Task<bool> DeletePartAsync(Part part)
        {
            var query = $"DELETE FROM Projects WHERE PartId = @PartId AND UserId = @UserId;";
            return await ExecuteAsync<Part>(query, part) > 0;
        }

        public async Task<bool> DeleteProjectAsync(Project project)
        {
            var query = $"DELETE FROM Projects WHERE ProjectId = @ProjectId AND UserId = @UserId;";
            return await ExecuteAsync<Project>(query, project) > 0;
        }

        public async Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords)
        {
            var query = $"SELECT * FROM Parts WHERE UserId = @UserId AND PartNumber LIKE @Keywords OR DigiKeyPartNumber LIKE @Keywords OR Description LIKE @Keywords OR Keywords LIKE @Keywords OR Location LIKE @Keywords OR BinNumber LIKE @Keywords OR BinNumber2 LIKE @Keywords;";
            var result = await SqlQueryAsync<Part>(query, new { Keywords = keywords });
            return result.Select(x => new SearchResult<Part>(x, 100)).ToList();
        }

        public async Task<OAuthCredential> GetOAuthCredentialAsync(string providerName)
        {
            var query = $"SELECT * FROM OAuthCredentials WHERE UserId = @UserId AND Provider = @ProviderName;";
            var result = await SqlQueryAsync<OAuthCredential>(query, new { ProviderName = providerName });
            return result.FirstOrDefault();
        }

        public async Task<PartType> GetOrCreatePartTypeAsync(PartType partType)
        {
            var query = $"SELECT PartTypeId FROM PartTypes WHERE Name = @Name AND UserId = @UserId;";
            var result = await SqlQueryAsync<PartType>(query, partType);
            if (result.Any())
            {
                return result.FirstOrDefault();
            }
            else
            {
                query =
$@"INSERT INTO PartTypes (ParentPartTypeId, Name, UserId) 
VALUES (@ParentPartTypeId, Name, UserId);";
                partType = await InsertAsync<PartType, long>(query, partType, (x, key) => { x.PartTypeId = key; });
            }
            return partType;
        }

        public async Task<Part> GetPartAsync(long partId)
        {
            var query = $"SELECT * FROM Parts WHERE PartId = @PartId;";
            var result = await SqlQueryAsync<Part>(query, new { PartId = partId });
            return result.FirstOrDefault();
        }

        public async Task<Part> GetPartAsync(string partNumber)
        {
            var query = $"SELECT * FROM Parts WHERE PartNumber = @PartNumber;";
            var result = await SqlQueryAsync<Part>(query, new { PartNumber = partNumber });
            return result.FirstOrDefault();
        }

        public async Task<ICollection<Part>> GetPartsAsync()
        {
            var query = $"SELECT * FROM Parts;";
            var result = await SqlQueryAsync<Part>(query);
            return result.ToList();
        }

        public async Task<Project> GetProjectAsync(long projectId)
        {
            var query = $"SELECT * FROM Projects WHERE ProjectId = @ProjectId;";
            var result = await SqlQueryAsync<Project>(query, new { ProjectId = projectId });
            return result.FirstOrDefault();
        }

        public async Task<Project> GetProjectAsync(string projectName)
        {
            var query = $"SELECT * FROM Projects WHERE Name = @Name;";
            var result = await SqlQueryAsync<Project>(query, new { Name = projectName });
            return result.FirstOrDefault();
        }

        public async Task<ICollection<Project>> GetProjectsAsync()
        {
            var query = $"SELECT * FROM Projects;";
            var result = await SqlQueryAsync<Project>(query);
            return result.ToList();
        }

        public async Task RemoveOAuthCredentialAsync(string providerName)
        {
            var query = $"DELETE FROM OAuthCredentials WHERE Provider = @Provider;";
            await ExecuteAsync<object>(query, new { Provider = providerName });
        }

        public async Task<OAuthCredential> SaveOAuthCredentialAsync(OAuthCredential credential)
        {
            var query = @"SELECT Provider FROM OAuthCredentials WHERE Provider = @Provider;";
            var result = await SqlQueryAsync<OAuthCredential>(query, credential);
            if (result.Any())
            {
                query = $@"UPDATE OAuthCredentials SET AccessToken = @AccessToken, RefreshToken = @RefreshToken, DateCreatedUtc = @DateCreatedUtc, DateExpiresUtc = @DateExpiresUtc WHERE Provider = @Provider;";
                await ExecuteAsync<object>(query, credential);
            }
            else
            {
                query =
$@"INSERT INTO OAuthCredentials (Provider, AccessToken, RefreshToken, DateCreatedUtc, DateExpiresUtc) 
VALUES (Provider, AccessToken, RefreshToken, DateCreatedUtc, DateExpiresUtc);";
                await InsertAsync<OAuthCredential, string>(query, credential, (x, key) => { });
            }
            return credential;
        }

        public async Task<Part> UpdatePartAsync(Part part)
        {
            var query = $"SELECT PartId FROM Parts WHERE PartId = @PartId AND UserId = @UserId;";
            var result = await SqlQueryAsync<Part>(query, part);
            if (result.Any())
            {
                query = $"UPDATE Parts SET Quantity = @Quantity, LowStockThreshold = @LowStockThreshold, PartNumber = @PartNumber, DigiKeyPartNumber = @DigiKeyPartNumber, Description = @Description, PartTypeId = @PartTypeId, ProjectId = @ProjectId, Keywords = @Keywords, DatasheetUrl = @DatasheetUrl, Project = @Project, Location = @Location, BinNumber = @BinNumber, BinNumber2 = @BinNumber2 WHERE PartId = @PartId AND UserId = @UserId;";
                await ExecuteAsync<Part>(query, part);
            }
            else
            {
                throw new ArgumentException($"Record not found for {nameof(Part)} = {part.PartId}");
            }
            return part;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            var query = $"SELECT ProjectId FROM Projects WHERE ProjectId = @ProjectId AND UserId = @UserId;";
            var result = await SqlQueryAsync<Project>(query, project);
            if (result.Any())
            {
                query = $"UPDATE Projects SET Name = @Name, Description = @Description, DateCreatedUtc = @DateCreatedUtc WHERE ProjectId = @ProjectId AND UserId = @UserId;";
                await ExecuteAsync<Project>(query, project);
            }
            else
            {
                throw new ArgumentException($"Record not found for {nameof(Project)} = {project.ProjectId}");
            }
            return project;
        }

        private async Task<T> InsertAsync<T, TKey>(string query, T parameters, Action<T, TKey> keySetter)
        {
            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                connection.Open();
                using (var sqlCmd = new SqlCommand(query))
                {
                    sqlCmd.Parameters.AddRange(CreateParameters<T>(parameters));
                    sqlCmd.CommandType = CommandType.Text;
                    var result = (TKey)await sqlCmd.ExecuteScalarAsync();
                    keySetter.Invoke(parameters, result);
                }
                connection.Close();
            }
            return parameters;
        }

        private async Task<ICollection<T>> SqlQueryAsync<T>(string query, object parameters = null)
        {
            var results = new List<T>();
            var type = typeof(T).GetExtendedType();
            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                connection.Open();
                using (var sqlCmd = new SqlCommand(query))
                {
                    if (parameters != null)
                        sqlCmd.Parameters.AddRange(CreateParameters(parameters));
                    sqlCmd.CommandType = CommandType.Text;
                    var reader = await sqlCmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        var newObj = Activator.CreateInstance<T>();
                        foreach (var prop in type.Properties)
                        {
                            newObj.SetPropertyValue(prop.PropertyInfo, reader[prop.Name]);
                        }
                    }
                }
                connection.Close();
            }
            return results;
        }

        private async Task<int> ExecuteAsync<T>(string query, T record)
        {
            var modified = 0;
            using (var connection = new SqlConnection(_config.ConnectionString))
            {
                connection.Open();
                using (var sqlCmd = new SqlCommand(query))
                {
                    sqlCmd.Parameters.AddRange(CreateParameters<T>(record));
                    sqlCmd.CommandType = CommandType.Text;
                    modified = await sqlCmd.ExecuteNonQueryAsync();
                }
                connection.Close();
            }
            return modified;
        }

        private SqlParameter[] CreateParameters<T>(T record)
        {
            var parameters = new List<SqlParameter>();
            var typeSupport = typeof(T).GetExtendedType();
            foreach (var prop in typeSupport.Properties)
            {
                parameters.Add(new SqlParameter(prop.Name, record.GetPropertyValue(prop)));
            }
            return parameters.ToArray();
        }

        private async Task<bool> GenerateDatabaseIfNotExistsAsync<T>()
        {
            var schemaGenerator = new SqlServerSchemaGenerator<T>("Binner");
            var modified = 0;
            var query = schemaGenerator.CreateDatabaseIfNotExists();

            try
            {
                using (var connection = new SqlConnection(_config.ConnectionString))
                {
                    connection.Open();
                    using (var sqlCmd = new SqlCommand(query, connection))
                    {
                        modified = await sqlCmd.ExecuteNonQueryAsync();
                    }
                    connection.Close();
                }
                query = schemaGenerator.CreateTableSchemaIfNotExists();
                using (var connection = new SqlConnection(_config.ConnectionString))
                {
                    connection.Open();
                    using (var sqlCmd = new SqlCommand(query, connection))
                    {
                        modified = await sqlCmd.ExecuteNonQueryAsync();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return modified > 0;
        }



        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_isDisposed)
                return;
            if (isDisposing)
            {

            }
            _isDisposed = true;
        }
    }
}
