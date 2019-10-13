using AnySerializer;
using Binner.Common.Extensions;
using Binner.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.StorageProviders
{
    public class BinnerFileStorageProvider : IStorageProvider
    {
        private const byte DbVersion = 1;
        private SemaphoreSlim _dataLock = new SemaphoreSlim(1, 1);
        private bool _isDisposed = false;
        private readonly BinnerFileStorageConfiguration _config;
        private BinnerDb _db;
        private long _nextPartId;
        private SerializerOptions _serializationOptions = SerializerOptions.EmbedTypes;
        private SerializerProvider _serializer = new SerializerProvider();
        private bool _isDirty;
        private ManualResetEvent _quitEvent = new ManualResetEvent(false);
        private Thread _ioThread;

        public BinnerFileStorageProvider(BinnerFileStorageConfiguration config)
        {
            _config = config;
            Task.Run(async () =>
            {
                await LoadDatabaseAsync();
            }).GetAwaiter().GetResult();
            StartIOThread();
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
                part.PartId = _nextPartId;
                _nextPartId++;
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
        /// Delete an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public async Task<bool> DeletePartAsync(Part part)
        {
            await _dataLock.WaitAsync();
            try
            {
                var itemRemoved = _db.Parts.Remove(part);
                if (itemRemoved)
                {
                    _nextPartId = _db.Parts.OrderByDescending(x => x.PartId)
                        .Select(x => x.PartId)
                        .FirstOrDefault() + 1;
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

        private ICollection<SearchResult<Part>> GetExactMatches(ICollection<string> words, StringComparison comparisonType)
        {
            var matches = new List<SearchResult<Part>>();

            // by part number
            var partNumberMatches = _db.Parts.Where(x => words.Any(y => x.PartNumber.Equals(y, comparisonType)))
                .Select(x => new SearchResult<Part>(x, 10))
                .ToList();
            matches.AddRange(partNumberMatches);

            // by keyword
            var keywordMatches = _db.Parts
                .Where(x => words.Any(y => x.Keywords.Contains(y, StringComparer.OrdinalIgnoreCase)))
                .Select(x => new SearchResult<Part>(x, 20))
                .ToList();
            matches.AddRange(keywordMatches);

            // by description
            var descriptionMatches = _db.Parts.Where(x => words.Any(y => x.Description.Equals(y, comparisonType)))
                .Select(x => new SearchResult<Part>(x, 30))
                .ToList();
            matches.AddRange(descriptionMatches);

            // by digikey id
            var digikeyMatches = _db.Parts.Where(x => words.Any(y => x.DigiKeyPartNumber.Equals(y, comparisonType)))
                .Select(x => new SearchResult<Part>(x, 40))
                .ToList();
            matches.AddRange(digikeyMatches);

            // by bin number
            var binNumberMatches = _db.Parts.Where(x => words.Any(y => x.BinNumber.Equals(y, comparisonType) || x.BinNumber2.Equals(y, comparisonType)))
                .Select(x => new SearchResult<Part>(x, 50))
                .ToList();
            matches.AddRange(binNumberMatches);

            return matches;
        }

        private ICollection<SearchResult<Part>> GetPartialMatches(ICollection<string> words, StringComparison comparisonType)
        {
            var matches = new List<SearchResult<Part>>();

            // by part number
            var partNumberMatches = _db.Parts.Where(x => words.Any(y => x.PartNumber.Contains(y, comparisonType)))
                .Select(x => new SearchResult<Part>(x, 100))
                .ToList();
            matches.AddRange(partNumberMatches);

            // by keyword
            var keywordMatches = _db.Parts
                .Where(x => words.Any(y => x.Keywords.Contains(y, StringComparer.OrdinalIgnoreCase)))
                .Select(x => new SearchResult<Part>(x, 200))
                .ToList();
            matches.AddRange(keywordMatches);

            // by description
            var descriptionMatches = _db.Parts.Where(x => words.Any(y => x.Description.Contains(y, comparisonType)))
                .Select(x => new SearchResult<Part>(x, 300))
                .ToList();
            matches.AddRange(descriptionMatches);

            // by digikey id
            var digikeyMatches = _db.Parts.Where(x => words.Any(y => x.DigiKeyPartNumber.Contains(y, comparisonType)))
                .Select(x => new SearchResult<Part>(x, 400))
                .ToList();
            matches.AddRange(digikeyMatches);

            // by bin number
            var binNumberMatches = _db.Parts.Where(x => words.Any(y => x.BinNumber.Contains(y, comparisonType) || x.BinNumber2.Contains(y, comparisonType)))
                .Select(x => new SearchResult<Part>(x, 500))
                .ToList();
            matches.AddRange(binNumberMatches);

            return matches;
        }

        /// <summary>
        /// Get a part by its internal id
        /// </summary>
        /// <param name="partId"></param>
        /// <returns></returns>
        public async Task<Part> GetPartAsync(long partId)
        {
            await _dataLock.WaitAsync();
            try
            {
                return _db.Parts.Where(x => x.PartId == partId).FirstOrDefault();
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
                return _db.Parts.Where(x => x.PartNumber.Equals(partNumber, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }
            finally
            {
                _dataLock.Release();
            }
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
                        _db = _serializer.Deserialize<BinnerDb>(stream, _serializationOptions);
                        // could make this non-fatal
                        if (!ValidateChecksum(_db))
                            throw new Exception("The database did not pass validation! It is either corrupt or has been modified outside of the application.");
                        _nextPartId = _db.LastPartId + 1;
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

        /// <summary>
        /// Save the database to disk
        /// </summary>
        /// <param name="requireLock">True if a lock needs to be acquired before saving</param>
        /// <returns></returns>
        private async Task SaveDatabaseAsync(bool requireLock = true)
        {
            byte[] bytes;
            if (requireLock)
                await _dataLock.WaitAsync();
            try
            {
                _db.FirstPartId = _db.Parts
                    .OrderBy(x => x.PartId)
                    .Select(x => x.PartId)
                    .FirstOrDefault();
                _db.LastPartId = _db.Parts
                    .OrderByDescending(x => x.PartId)
                    .Select(x => x.PartId)
                    .FirstOrDefault();
                _db.Count = _db.Parts.Count;
                _db.Checksum = BuildChecksum(_db);
                bytes = _serializer.Serialize(_db, _serializationOptions);
                Directory.CreateDirectory(Path.GetDirectoryName(_config.Filename));
                File.WriteAllBytes(_config.Filename, bytes);
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

        private BinnerDb NewDatabase()
        {
            _nextPartId = 1;
            return new BinnerDb
            {
                Count = 0,
                DateCreatedUtc = DateTime.UtcNow,
                DateModifiedUtc = DateTime.UtcNow,
                FirstPartId = 0,
                LastPartId = 0,
                Parts = new List<Part>(),
                Version = DbVersion
            };
        }

        private string BuildChecksum(BinnerDb db)
        {
            string hash;
            byte[] bytes = _serializer.Serialize(db, "Checksum");
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                hash = Convert.ToBase64String(sha1.ComputeHash(bytes));
            }
            return hash;
        }

        private bool ValidateChecksum(BinnerDb db)
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
