using AnySerializer;
using Binner.Common.Extensions;
using Binner.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TypeSupport.Extensions;

namespace Binner.Common.StorageProviders
{
    public class BinnerFileStorageProvider : IStorageProvider
    {
        public const string ProviderName = "Binner";

        private SemaphoreSlim _dataLock = new SemaphoreSlim(1, 1);
        private bool _isDisposed = false;
        private readonly BinnerFileStorageConfiguration _config;
        private readonly RequestContextAccessor _requestContext;
        private IBinnerDb _db;
        private PrimaryKeyTracker _primaryKeyTracker;
        private SerializerOptions _serializationOptions = SerializerOptions.EmbedTypes;
        private SerializerProvider _serializer = new SerializerProvider();
        private bool _isDirty;
        private ManualResetEvent _quitEvent = new ManualResetEvent(false);
        private Thread _ioThread;

        public BinnerFileStorageProvider(IDictionary<string, string> config, RequestContextAccessor requestContext)
        {
            _config = new BinnerFileStorageConfiguration(config);
            _requestContext = requestContext;
            Task.Run(async () =>
            {
                await LoadDatabaseAsync();
            }).GetAwaiter().GetResult();
            StartIOThread();
        }

        /// <summary>
        /// Get an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<OAuthCredential> GetOAuthCredentialAsync(string providerName)
        {
            await _dataLock.WaitAsync();
            try
            {
                return _db.OAuthCredentials.Where(x => x.Provider == providerName && x.UserId == GetUserContext(y => y.UserId)).FirstOrDefault();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// Save an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<OAuthCredential> SaveOAuthCredentialAsync(OAuthCredential credential)
        {
            await _dataLock.WaitAsync();
            try
            {
                credential.UserId = GetUserContext(y => y.UserId);
                var existingCredential = _db.OAuthCredentials.Where(x => x.Provider == credential.Provider && x.UserId == credential.UserId).FirstOrDefault();
                if (existingCredential != null)
                {
                    existingCredential.AccessToken = credential.AccessToken;
                    existingCredential.RefreshToken = credential.RefreshToken;
                }
                else
                    _db.OAuthCredentials.Add(credential);
                _isDirty = true;
            }
            finally
            {
                _dataLock.Release();
            }
            return credential;
        }

        /// <summary>
        /// Remove an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task RemoveOAuthCredentialAsync(string providerName)
        {
            await _dataLock.WaitAsync();
            try
            {
                var existingCredential = _db.OAuthCredentials.Where(x => x.Provider == providerName && x.UserId == GetUserContext(y => y.UserId)).FirstOrDefault();
                if (existingCredential != null)
                {
                    _db.OAuthCredentials.Remove(existingCredential);
                    _isDirty = true;
                }
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// Add a new part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public async Task<Part> AddPartAsync(Part part)
        {
            await _dataLock.WaitAsync();
            try
            {
                part.UserId = GetUserContext(y => y.UserId);
                part.PartId = _primaryKeyTracker.GetNextKey<Part>();
                _db.Parts.Add(part);
                _isDirty = true;
            }
            finally
            {
                _dataLock.Release();
            }
            return part;
        }

        /// <summary>
        /// Update an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public async Task<Part> UpdatePartAsync(Part part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            await _dataLock.WaitAsync();
            try
            {
                part.UserId = GetUserContext(y => y.UserId);
                var existingPart = await GetPartAsync(part.PartId);
                // todo: automate this assignment or use a mapper
                existingPart.BinNumber = part.BinNumber;
                existingPart.BinNumber2 = part.BinNumber2;
                existingPart.DatasheetUrl = part.DatasheetUrl;
                existingPart.Description = part.Description;
                existingPart.DigiKeyPartNumber = part.DigiKeyPartNumber;
                existingPart.Keywords = part.Keywords;
                existingPart.Location = part.Location;
                existingPart.LowStockThreshold = part.LowStockThreshold;
                existingPart.MouserPartNumber = part.MouserPartNumber;
                existingPart.PartNumber = part.PartNumber;
                existingPart.PartTypeId = part.PartTypeId;
                existingPart.ProjectId = part.ProjectId;
                existingPart.Quantity = part.Quantity;
                existingPart.UserId = part.UserId;
                _isDirty = true;
            }
            finally
            {
                _dataLock.Release();
            }
            return part;
        }

        /// <summary>
        /// Get an existing part type or create a new one
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        public async Task<PartType> GetOrCreatePartTypeAsync(PartType partType)
        {
            await _dataLock.WaitAsync();
            try
            {
                partType.UserId = GetUserContext(y => y.UserId);
                var existingPartType = _db.PartTypes
                    .Where(x => x.Name.Equals(partType.Name, StringComparison.InvariantCultureIgnoreCase) && x.UserId == partType.UserId)
                    .FirstOrDefault();
                if (existingPartType == null)
                {
                    existingPartType = new PartType
                    {
                        Name = partType.Name,
                        ParentPartTypeId = null,
                        PartTypeId = _primaryKeyTracker.GetNextKey<PartType>(),
                        UserId = partType.UserId,
                    };
                    _db.PartTypes.Add(existingPartType);
                    _isDirty = true;
                }
                return existingPartType;
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// Delete an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public async Task<bool> DeletePartAsync(Part part)
        {
            await _dataLock.WaitAsync();
            try
            {
                part.UserId = GetUserContext(y => y.UserId);
                var itemRemoved = _db.Parts.Remove(part);
                if (itemRemoved)
                {
                    var nextPartId = _db.Parts.OrderByDescending(x => x.PartId)
                        .Select(x => x.PartId)
                        .FirstOrDefault() + 1;
                    _primaryKeyTracker.SetNextKey<Part>(nextPartId);
                    _isDirty = true;
                }
                return itemRemoved;
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// Find a part by any keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords)
        {
            if (string.IsNullOrEmpty(keywords)) throw new ArgumentNullException(nameof(keywords));

            await _dataLock.WaitAsync();
            try
            {
                var comparisonType = StringComparison.InvariantCultureIgnoreCase;
                var words = keywords.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var matches = new List<SearchResult<Part>>();

                // exact match first
                matches.AddRange(GetExactMatches(words, comparisonType).ToList());
                matches.AddRange(GetPartialMatches(words, comparisonType).ToList());

                return matches
                    .OrderBy(x => x.Rank)
                    .DistinctBy(x => x.Result)
                    .ToList();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// Get a part by its internal id
        /// </summary>
        /// <param name="partId"></param>
        /// <returns></returns>
        public async Task<Part> GetPartAsync(long partId)
        {
            if (partId <= 0) throw new ArgumentException(nameof(partId));
            await _dataLock.WaitAsync();
            try
            {
                return _db.Parts.Where(x => x.PartId == partId && x.UserId == GetUserContext(y => y.UserId)).FirstOrDefault();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// Get a part by part number
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<Part> GetPartAsync(string partNumber)
        {
            if (string.IsNullOrEmpty(partNumber)) throw new ArgumentNullException(nameof(partNumber));
            await _dataLock.WaitAsync();
            try
            {
                return _db.Parts.Where(x => x.PartNumber.Equals(partNumber, StringComparison.InvariantCultureIgnoreCase) && x.UserId == GetUserContext(y => y.UserId)).FirstOrDefault();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public async Task<ICollection<Part>> GetPartsAsync()
        {
            await _dataLock.WaitAsync();
            try
            {
                return _db.Parts.Where(x => x.UserId == GetUserContext(y => y.UserId)).ToList();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public async Task<Project> GetProjectAsync(long projectId)
        {
            if (projectId <= 0) throw new ArgumentException(nameof(projectId));
            await _dataLock.WaitAsync();
            try
            {
                return _db.Projects.Where(x => x.ProjectId.Equals(projectId) && x.UserId == GetUserContext(y => y.UserId)).FirstOrDefault();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public async Task<Project> GetProjectAsync(string projectName)
        {
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentNullException(nameof(projectName));
            await _dataLock.WaitAsync();
            try
            {
                return _db.Projects.Where(x => x.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase) && x.UserId == GetUserContext(y => y.UserId)).FirstOrDefault();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public async Task<ICollection<Project>> GetProjectsAsync()
        {
            await _dataLock.WaitAsync();
            try
            {
                return _db.Projects.Where(x => x.UserId == GetUserContext(y => y.UserId)).ToList();
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public async Task<Project> AddProjectAsync(Project project)
        {
            await _dataLock.WaitAsync();
            try
            {
                project.UserId = GetUserContext(y => y.UserId);
                project.ProjectId = _primaryKeyTracker.GetNextKey<Project>();
                _db.Projects.Add(project);
                _isDirty = true;
            }
            finally
            {
                _dataLock.Release();
            }
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            await _dataLock.WaitAsync();
            try
            {
                project.UserId = GetUserContext(y => y.UserId);
                var existingPart = await GetProjectAsync(project.ProjectId);
                existingPart.Name = project.Name;
                existingPart.Description = project.Description;
                existingPart.Location = project.Location;
                existingPart.UserId = project.UserId;
                _isDirty = true;
            }
            finally
            {
                _dataLock.Release();
            }
            return project;
        }

        public async Task<bool> DeleteProjectAsync(Project project)
        {
            await _dataLock.WaitAsync();
            try
            {
                project.UserId = GetUserContext(y => y.UserId);
                var itemRemoved = _db.Projects.Remove(project);
                if (itemRemoved)
                {
                    var nextProjectId = _db.Projects.OrderByDescending(x => x.ProjectId)
                        .Select(x => x.ProjectId)
                        .FirstOrDefault() + 1;
                    _primaryKeyTracker.SetNextKey<Project>(nextProjectId);
                    _isDirty = true;
                }
                return itemRemoved;
            }
            finally
            {
                _dataLock.Release();
            }
        }

        private Nullable<T> GetUserContext<T>(Func<UserContext, T> userSelector)
            where T : struct
        {
            var userContext = _requestContext.GetUserContext();
            if (userContext != null)
                return userSelector.Invoke(userContext);
            return new Nullable<T>(default(T));
        }

        private T GetUserContext<T>(Func<UserContext, T> userSelector, T defaultValue)
            where T : class
        {
            var userContext = _requestContext.GetUserContext();
            if (userContext != null)
                return userSelector.Invoke(userContext);
            return default(T) ?? defaultValue;
        }

        private ICollection<SearchResult<Part>> GetExactMatches(ICollection<string> words, StringComparison comparisonType)
        {
            var matches = new List<SearchResult<Part>>();

            // by part number
            var partNumberMatches = _db.Parts.Where(x => words.Any(y => x.PartNumber.Equals(y, comparisonType)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 10))
                .ToList();
            matches.AddRange(partNumberMatches);

            // by keyword
            var keywordMatches = _db.Parts
                .Where(x => words.Any(y => x.Keywords.Contains(y, StringComparer.OrdinalIgnoreCase)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 20))
                .ToList();
            matches.AddRange(keywordMatches);

            // by description
            var descriptionMatches = _db.Parts.Where(x => words.Any(y => x.Description.Equals(y, comparisonType)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 30))
                .ToList();
            matches.AddRange(descriptionMatches);

            // by digikey id
            var digikeyMatches = _db.Parts.Where(x => words.Any(y => x.DigiKeyPartNumber.Equals(y, comparisonType)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 40))
                .ToList();
            matches.AddRange(digikeyMatches);

            // by bin number
            var binNumberMatches = _db.Parts.Where(x => words.Any(y => x.BinNumber.Equals(y, comparisonType) || x.BinNumber2.Equals(y, comparisonType)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 50))
                .ToList();
            matches.AddRange(binNumberMatches);

            return matches;
        }

        private ICollection<SearchResult<Part>> GetPartialMatches(ICollection<string> words, StringComparison comparisonType)
        {
            var matches = new List<SearchResult<Part>>();

            // by part number
            var partNumberMatches = _db.Parts.Where(x => words.Any(y => x.PartNumber.Contains(y, comparisonType)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 100))
                .ToList();
            matches.AddRange(partNumberMatches);

            // by keyword
            var keywordMatches = _db.Parts
                .Where(x => words.Any(y => x.Keywords.Contains(y, StringComparer.OrdinalIgnoreCase)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 200))
                .ToList();
            matches.AddRange(keywordMatches);

            // by description
            var descriptionMatches = _db.Parts.Where(x => words.Any(y => x.Description.Contains(y, comparisonType)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 300))
                .ToList();
            matches.AddRange(descriptionMatches);

            // by digikey id
            var digikeyMatches = _db.Parts.Where(x => words.Any(y => x.DigiKeyPartNumber.Contains(y, comparisonType)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 400))
                .ToList();
            matches.AddRange(digikeyMatches);

            // by bin number
            var binNumberMatches = _db.Parts.Where(x => words.Any(y => x.BinNumber.Contains(y, comparisonType) || x.BinNumber2.Contains(y, comparisonType)) && x.UserId == GetUserContext(y => y.UserId))
                .Select(x => new SearchResult<Part>(x, 500))
                .ToList();
            matches.AddRange(binNumberMatches);

            return matches;
        }

        /// <summary>
        /// Load the database from disk
        /// </summary>
        /// <param name="requireLock">True if a lock needs to be acquired before saving</param>
        /// <returns></returns>
        private async Task LoadDatabaseAsync(bool requireLock = true)
        {
            if (File.Exists(_config.Filename))
            {
                if (requireLock)
                    await _dataLock.WaitAsync();
                try
                {
                    using (var stream = File.OpenRead(_config.Filename))
                    {
                        var dbVersion = ReadDbVersion(stream);

                        // copy the rest of the bytes
                        var bytes = new byte[stream.Length - stream.Position];
                        stream.Read(bytes, 0, bytes.Length);

                        _db = LoadDatabaseByVersion(dbVersion, bytes);
                        bytes = null;

                        // could make this non-fatal
                        if (!ValidateChecksum(_db))
                            throw new Exception("The database did not pass validation! It is either corrupt or has been modified outside of the application.");
                        _primaryKeyTracker = new PrimaryKeyTracker(new Dictionary<string, long>
                        {
                            // there is no guaranteed order so we must sort first
                            { typeof(Part).Name, _db.Parts.OrderByDescending(x => x.PartId).Select(x => x.PartId).FirstOrDefault() },
                            { typeof(PartType).Name, _db.PartTypes.OrderByDescending(x => x.PartTypeId).Select(x => x.PartTypeId).FirstOrDefault() },
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to load database!", ex);
                }
                finally
                {
                    if (requireLock)
                        _dataLock.Release();
                }
            }
            else
            {
                if (requireLock)
                    await _dataLock.WaitAsync();
                try
                {
                    _db = NewDatabase();
                }
                finally
                {
                    if (requireLock)
                        _dataLock.Release();
                }
            }
        }

        private IBinnerDb LoadDatabaseByVersion(BinnerDbVersion version, byte[] bytes)
        {
            IBinnerDb db;
            // Support database loading by version number
#if (NET462 || NET471)
            switch (version.Version)
            {
                case BinnerDbV1.VersionNumber:
                    // Version 1
                    db = _serializer.Deserialize<BinnerDbV1>(bytes, _serializationOptions);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported database version: {version}");
            }
#else
            db = version.Version switch
            {
                // Version 1
                BinnerDbV1.VersionNumber => _serializer.Deserialize<BinnerDbV1>(bytes, _serializationOptions),
                _ => throw new InvalidOperationException($"Unsupported database version: {version}"),
            };
#endif
            return db;
        }

        /// <summary>
        /// Save the database to disk
        /// </summary>
        /// <param name="requireLock">True if a lock needs to be acquired before saving</param>
        /// <returns></returns>
        private async Task SaveDatabaseAsync(bool requireLock = true)
        {
            if (requireLock)
                await _dataLock.WaitAsync();
            try
            {
                var db = _db as BinnerDbV1;
                db.FirstPartId = db.Parts
                    .OrderBy(x => x.PartId)
                    .Select(x => x.PartId)
                    .FirstOrDefault();
                db.LastPartId = db.Parts
                    .OrderByDescending(x => x.PartId)
                    .Select(x => x.PartId)
                    .FirstOrDefault();
                db.Count = db.Parts.Count;
                db.Checksum = BuildChecksum(db);
                using (var stream = new MemoryStream())
                {
                    WriteDbVersion(stream, new BinnerDbVersion(BinnerDbV1.VersionNumber, BinnerDbV1.VersionCreated));
                    var serializedBytes = _serializer.Serialize(db, _serializationOptions);
                    stream.Write(serializedBytes, 0, serializedBytes.Length);
                    Directory.CreateDirectory(Path.GetDirectoryName(_config.Filename));
                    File.WriteAllBytes(_config.Filename, stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error trying to save the database!", ex);
            }
            finally
            {
                if (requireLock)
                    _dataLock.Release();
            }
        }

        private BinnerDbVersion ReadDbVersion(Stream stream)
        {
            BinnerDbVersion version = null;
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var versionByte = reader.ReadByte();
                var versionCreated = reader.ReadInt64();
                version = new BinnerDbVersion(versionByte, DateTime.FromBinary(versionCreated));
            }
            return version;
        }

        private void WriteDbVersion(Stream stream, BinnerDbVersion version)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(version.Version);
                writer.Write(version.Created.ToBinary());
            }
        }

        private IBinnerDb NewDatabase()
        {
            _primaryKeyTracker = new PrimaryKeyTracker(new Dictionary<string, long>
            {
                { typeof(Part).Name, 1 },
                { typeof(PartType).Name, 1 },
            });

            var defaultPartTypesTs = typeof(SystemDefaults.DefaultPartTypes).GetExtendedType();
            var initialPartTypes = defaultPartTypesTs.EnumValues.Select(x => new PartType
            {
                PartTypeId = (int)x.Key,
                Name = x.Value
            }).ToList();

            var created = DateTime.UtcNow;
            return new BinnerDbV1
            {
                Count = 0,
                FirstPartId = 0,
                LastPartId = 0,
                DateCreatedUtc = created,
                DateModifiedUtc = created,
                PartTypes = initialPartTypes,
                Parts = new List<Part>(),
                OAuthCredentials = new List<OAuthCredential>()
            };
        }

        private string BuildChecksum(IBinnerDb db)
        {
            string hash;
            byte[] bytes = _serializer.Serialize(db, "Checksum");
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                hash = Convert.ToBase64String(sha1.ComputeHash(bytes));
            }
            return hash;
        }

        private bool ValidateChecksum(IBinnerDb db)
        {
            var calculatedChecksum = BuildChecksum(db);
            if (db.Checksum.Equals(calculatedChecksum))
                return true;
            return false;
        }

        private void StartIOThread()
        {
            _ioThread = new Thread(new ThreadStart(SaveDataThread));
            _ioThread.Start();
        }

        private void SaveDataThread()
        {
            while (!_quitEvent.WaitOne(500))
            {
                if (_isDirty)
                {
                    _dataLock.Wait();
                    try
                    {
                        Task.Run(async () => await SaveDatabaseAsync(false))
                            .GetAwaiter()
                            .GetResult();
                    }
                    finally
                    {
                        _isDirty = false;
                        _dataLock.Release();
                    }
                }
            }
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
                // obtain the lock before removing it
                _dataLock.Wait();
                try
                {
                    _quitEvent.Set();
                    if (!_ioThread.Join(5000))
                        _ioThread.Abort();

                    if (_isDirty)
                    {
                        Task.Run(async () =>
                        {
                            await SaveDatabaseAsync(false);
                        }).GetAwaiter().GetResult();
                    }

                    _ioThread = null;
                    _quitEvent.Dispose();
                    _quitEvent = null;
                    _db = null;
                }
                finally
                {
                    _dataLock.Release();
                    _dataLock.Dispose();
                    _dataLock = null;
                }
            }
            _isDisposed = true;
        }
    }
}
