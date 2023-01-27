using System;
using System.Collections.Generic;
using System.Linq;

namespace Binner.Common.IO
{
    /// <summary>
    /// Keeps track of temporary relational keys for importing data
    /// </summary>
    public class TemporaryKeyTracker
    {
        private Dictionary<string, List<KeyMapping>> _keyTrackerDb = new Dictionary<string, List<KeyMapping>>();

        /// <summary>
        /// Add a mapping from one id to another for a given table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="originalId"></param>
        /// <param name="remappedId"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddKeyMapping(string table, string name, long originalId, long remappedId)
        {
            if (!_keyTrackerDb.ContainsKey(table))
                _keyTrackerDb.Add(table, new List<KeyMapping>());

            var keymapping = new KeyMapping(name, originalId, remappedId);
            if (!_keyTrackerDb[table].Contains(keymapping))
                _keyTrackerDb[table].Add(keymapping);
            else
                throw new ArgumentException($"The mapping has already been added!");
        }

        /// <summary>
        /// Get the new mapped id from the original id
        /// </summary>
        /// <param name="table"></param>
        /// <param name="name"></param>
        /// <param name="originalId"></param>
        /// <returns></returns>
        public long GetMappedId(string table, string name, long originalId)
        {
            if (_keyTrackerDb.ContainsKey(table) && _keyTrackerDb[table].Any(x => x.Name.Equals(name) && x.OriginalId == originalId))
                return _keyTrackerDb[table].Where(x => x.Name.Equals(name) && x.OriginalId == originalId).Select(x => x.RemappedId).First();
            return originalId;
        }

        public List<KeyMapping>? GetKeyMappings(string table)
        {
            if (_keyTrackerDb.ContainsKey(table))
                return _keyTrackerDb[table];
            return null;
        }

        public class KeyMapping
        {
            public string Name { get; set; }
            public long OriginalId { get; set; }
            public long RemappedId { get; set; }

            public KeyMapping(string name, long originalId, long remappedId)
            {
                Name = name;
                OriginalId = originalId;
                RemappedId = remappedId;
            }

            public override int GetHashCode() => (OriginalId, RemappedId).GetHashCode();

            public override bool Equals(object? obj)
                => obj is KeyMapping other && (other.Name, other.OriginalId, other.RemappedId).Equals((Name, OriginalId, RemappedId));
        }
    }


}
