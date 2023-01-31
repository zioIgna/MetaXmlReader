using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Security.Cryptography;

namespace LettoreXml
{
    internal class CachingSystem
    {
        private System.Runtime.Caching.MemoryCache hashesCache;
        private System.Runtime.Caching.MemoryCache connectionsCache;
        CacheItemPolicy cacheItemPolicy;
        Dictionary<string, HashSet<string>> localCache;

        /**
         * la hashesCache serve per la verifica delle eventuali modifiche ai files
         * la connectionsCache serve a generare la rete di riferimenti tra files dovuta ai tag di Import
         * la connectionsCache in Memory Cache è la copia della cache in locale
         */
        public CachingSystem()
        {
            hashesCache = new System.Runtime.Caching.MemoryCache("HaschesCache");
            connectionsCache = new System.Runtime.Caching.MemoryCache("ConnectionsCache");
            cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.MaxValue };
            localCache = new Dictionary<string, HashSet<string>>();
        }

        public bool currFileChanged(string fileName)
        {
            byte[] hash = calculateMD5(fileName);
            return !filesMatch((byte[])hashesCache.Get(fileName), hash);
        }

        //ad ogni file la connectionsCache associa i files che lo referenziano
        public HashSet<string> getLinkedFilesList(string fileName)
        {
            HashSet<string> linkedFiles = null;
            if (connectionsCache.GetCacheItem(fileName) != null)
            {
                linkedFiles = (HashSet<string>)connectionsCache.GetCacheItem(fileName).Value;
            }
            return linkedFiles;
        }

        public void upsertHashToCache(string fileName)
        {
            byte[] hash = calculateMD5(fileName);
            
            var cacheItem = new CacheItem(fileName, hash);
            hashesCache.Set(cacheItem, cacheItemPolicy);
        }

        internal void upsertLinkToLocalCache(string fileName, string newFileName)
        {
            HashSet<string> links;
            if (!localCache.ContainsKey(fileName))
            {
                links = new HashSet<string>();
                links.Add(newFileName);
                localCache.Add(fileName,links);
            }
            else
            {
                bool success = localCache.TryGetValue(fileName, out links);
                links.Add(newFileName);
            }
        }

        private void loadLinksToCache()
        {
            foreach (var item in localCache)
            {
                var cacheItem = new CacheItem(item.Key, item.Value);
                connectionsCache.Add(cacheItem, cacheItemPolicy);
            }
        }

        //la rete delle connessioni tra files viene eliminata e ricreata ogni volta che si
        //ricaricano i files per poter individuare eventuali eliminazioni di rinvii tra files
        internal void regenConnectionsCache()
        {
            connectionsCache.Dispose();
            connectionsCache = new System.Runtime.Caching.MemoryCache("ConnectionsCache");
            loadLinksToCache();
            localCache.Clear();
        }

        private byte[] calculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return hash;
                }
            }
        }

        private bool filesMatch(byte[] b1, byte[] b2, int position = 0)
        {
            return b1.Length == b2.Length && (position == b1.Length || (b1[position] == b2[position] && filesMatch(b1, b2, ++position)));
        }

    }
}
