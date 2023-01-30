using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;

namespace LettoreXml
{
    internal class CachingSystem
    {
        private System.Runtime.Caching.MemoryCache hashesCache;
        private System.Runtime.Caching.MemoryCache connectionsCache;
        CacheItemPolicy cacheItemPolicy;
        //private byte[] hash;
        public CachingSystem()
        {
            hashesCache = new System.Runtime.Caching.MemoryCache("HaschesCache");
            connectionsCache = new System.Runtime.Caching.MemoryCache("ConnectionsCache");
            cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.MaxValue };
        }

        public bool shouldReadFile(string fileName)
        {
            byte[]  hash = calculateMD5(fileName);
            return !hashesCache.Contains(fileName) || !filesTheSame((byte[])hashesCache.Get(fileName), hash) ;
        }

        public bool currFileIsNewOrChanged(string fileName)
        {
            byte[]  hash = calculateMD5(fileName);
            Console.WriteLine("Upserted item, Key: {0} hash: {1}", fileName.Split("\\").Last(), hashToString(hash));
            return !hashesCache.Contains(fileName) || !filesTheSame((byte[])hashesCache.Get(fileName), hash);
        }

        public bool currFileIsNew(string fileName)
        {
            byte[] hash = calculateMD5(fileName);
            Console.WriteLine("Inserting new item for file: {0}", fileName);
            return !hashesCache.Contains(fileName);
        }

        public bool currFileChanged(string fileName)
        {
            byte[] hash = calculateMD5(fileName);
            Console.WriteLine("Upserting item, Key: {0} hash: {1}", fileName.Split("\\").Last(), hashToString(hash));
            return !filesTheSame((byte[])hashesCache.Get(fileName), hash);
        }

        public HashSet<string> getLinkedFilesList(string fileName)
        {
            HashSet<string> linkedFiles = null;
            if (connectionsCache.GetCacheItem(fileName) != null)
            {
                linkedFiles = (HashSet<string>)connectionsCache.GetCacheItem(fileName).Value;
            }
            //TODO remove from here
            //Console.WriteLine("Elenco dei files collegati a {0}", fileName.Split('\\').Last());
            //if (linkedFiles != null)
            //{
            //    //Console.WriteLine(String.Join(", ", linkedFiles));
            //    foreach (var linkedFile in linkedFiles)
            //    {
            //        Console.Write(linkedFile.Split("\\").Last() + ", ");
            //    }
            //}
            // to here
            return linkedFiles;
        }

        public void upsertHashToCache(string fileName)
        {
            //bool outcome = false;
            byte[] hash = calculateMD5(fileName);
            
            var cacheItem = new CacheItem(fileName, hash);
            hashesCache.Set(cacheItem, cacheItemPolicy);
            //TODO remove writeline
            Console.WriteLine("Upserted item, Key: {0} hash: {1}", fileName.Split('\\').Last(), hashToString(hash));
        }

        private string hashToString(byte[] hash)
        {
            return BitConverter.ToString(hash);
        }

        internal void upsertLinkToCache(string fileName, string newFileName)
        {
            if (!connectionsCache.Contains(fileName))
            {
                var cacheItem = new CacheItem(fileName, new HashSet<string>() { newFileName });
                connectionsCache.Add(cacheItem, cacheItemPolicy);
            }
            else
            {
                HashSet<string> linksSet = (HashSet<string>)connectionsCache.GetCacheItem(fileName).Value;
                linksSet.Add(newFileName);
                //TODO: remove console writeline
                Console.WriteLine(String.Join(",",linksSet));
                connectionsCache.Set(new CacheItem(fileName, linksSet), cacheItemPolicy);
            }
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

        private bool filesTheSame(byte[] b1, byte[] b2, int position = 0)
        {
            return b1.Length == b2.Length && (position == b1.Length || (b1[position] == b2[position] && filesTheSame(b1, b2, ++position)));
        }

    }
}
