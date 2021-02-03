using System;
using System.Collections.Generic;
using System.Linq;
using Rsk.AspNetCore.Scim.Caching;

namespace Caching.Caching
{
    public class CacheEntry : IScimCacheEntry
    {
        public CacheEntry(string key)
        {
            Key = key;
        }

        public object Value { get; set; }
        public object Key { get; }
    }

    public class CustomCache : IScimCache
    {
        private List<CacheEntry> entries;

        public IScimCacheEntry CreateEntry(string key)
        {
            if (entries.Any(e => e.Key as string == key)) throw new Exception($"Key already exists: {key}");

            var entry = new CacheEntry(key);
            entries.Add(entry);
            return entry;
        }

        public void Remove(string key)
        {
            var foundEntries = entries.Where(e => e.Key as string == key);

            entries = entries.Except(foundEntries).ToList();
        }

        public bool TryGetValue(string key, out object value)
        {
            var foundEntry = entries.FirstOrDefault(e => e.Key as string == key);

            value = foundEntry;

            return foundEntry != null;
        }
    }
}